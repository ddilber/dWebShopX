using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Products;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.ProductSkus.Commands;

// ── SKU ──────────────────────────────────────────────────────────────────────

public record CreateSkuCommand(int ProductId, string SKU, string ExtRef, string Name, string Gtin, decimal Price, decimal? CompareAtPrice, decimal? CostPrice, decimal Tax, int StockQuantity, int LowStockThreshold, string? ImagePath, List<(int OptionId, int OptionValueId)>? OptionValues) : IRequest<int>;

public class CreateSkuCommandValidator : AbstractValidator<CreateSkuCommand>
{
    public CreateSkuCommandValidator()
    {
        RuleFor(x => x.SKU).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class CreateSkuCommandHandler(IAppDbContext db) : IRequestHandler<CreateSkuCommand, int>
{
    public async Task<int> Handle(CreateSkuCommand request, CancellationToken ct)
    {
        var sku = new ProductSku
        {
            ProductId = request.ProductId,
            SKU = request.SKU,
            ExtRef = request.ExtRef,
            Name = request.Name,
            Gtin = request.Gtin,
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            CostPrice = request.CostPrice,
            Tax = request.Tax,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold,
            ImagePath = request.ImagePath,
        };
        db.ProductSkus.Add(sku);
        await db.SaveChangesAsync(ct);

        if (request.OptionValues?.Count > 0)
        {
            foreach (var (optId, valId) in request.OptionValues)
            {
                db.SkuOptionValues.Add(new SkuOptionValue
                {
                    ProductId = request.ProductId,
                    ProductSkuId = sku.Id,
                    ProductOptionsId = optId,
                    ProductOptionValueId = valId,
                });
            }
            await db.SaveChangesAsync(ct);
        }

        return sku.Id;
    }
}

// OptionValues: null leaves the SKU's option values untouched; a list (even empty)
// replaces them.
public record UpdateSkuCommand(int Id, string SKU, string ExtRef, string Name, string Gtin, decimal Price, decimal? CompareAtPrice, decimal? CostPrice, decimal Tax, int StockQuantity, int LowStockThreshold, string? ImagePath, List<(int OptionId, int OptionValueId)>? OptionValues = null) : IRequest;

public class UpdateSkuCommandValidator : AbstractValidator<UpdateSkuCommand>
{
    public UpdateSkuCommandValidator()
    {
        RuleFor(x => x.SKU).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}

public class UpdateSkuCommandHandler(IAppDbContext db) : IRequestHandler<UpdateSkuCommand>
{
    public async Task Handle(UpdateSkuCommand request, CancellationToken ct)
    {
        var sku = await db.ProductSkus.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"ProductSku {request.Id} not found.");
        sku.SKU = request.SKU;
        sku.ExtRef = request.ExtRef;
        sku.Name = request.Name;
        sku.Gtin = request.Gtin;
        sku.Price = request.Price;
        sku.CompareAtPrice = request.CompareAtPrice;
        sku.CostPrice = request.CostPrice;
        sku.Tax = request.Tax;
        sku.StockQuantity = request.StockQuantity;
        sku.LowStockThreshold = request.LowStockThreshold;
        // null keeps the current image; an empty string clears it.
        if (request.ImagePath is not null)
            sku.ImagePath = request.ImagePath.Length == 0 ? null : request.ImagePath;

        if (request.OptionValues is not null)
        {
            var existing = await db.SkuOptionValues.Where(v => v.ProductSkuId == sku.Id).ToListAsync(ct);
            db.SkuOptionValues.RemoveRange(existing);
            foreach (var (optId, valId) in request.OptionValues)
            {
                db.SkuOptionValues.Add(new SkuOptionValue
                {
                    ProductId = sku.ProductId,
                    ProductSkuId = sku.Id,
                    ProductOptionsId = optId,
                    ProductOptionValueId = valId,
                });
            }
        }

        await db.SaveChangesAsync(ct);
    }
}

public record DeleteSkuCommand(int Id) : IRequest;

public class DeleteSkuCommandHandler(IAppDbContext db) : IRequestHandler<DeleteSkuCommand>
{
    public async Task Handle(DeleteSkuCommand request, CancellationToken ct)
    {
        var sku = await db.ProductSkus.FirstOrDefaultAsync(s => s.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"ProductSku {request.Id} not found.");
        db.ProductSkus.Remove(sku);
        await db.SaveChangesAsync(ct);
    }
}

// ── ProductOption ─────────────────────────────────────────────────────────────

public record CreateProductOptionCommand(int ProductId, string Name, bool IsNamePart) : IRequest<int>;

public class CreateProductOptionCommandValidator : AbstractValidator<CreateProductOptionCommand>
{
    public CreateProductOptionCommandValidator(IAppDbContext db)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x).MustAsync(async (c, ct) =>
                !await db.ProductOptions.AnyAsync(o => o.ProductId == c.ProductId && o.Name == c.Name.Trim(), ct))
            .WithMessage("This product already has an option with that name.");
    }
}

public class CreateProductOptionCommandHandler(IAppDbContext db) : IRequestHandler<CreateProductOptionCommand, int>
{
    public async Task<int> Handle(CreateProductOptionCommand request, CancellationToken ct)
    {
        var opt = new ProductOption { ProductId = request.ProductId, Name = request.Name.Trim(), IsNamePart = request.IsNamePart };
        db.ProductOptions.Add(opt);
        await db.SaveChangesAsync(ct);
        return opt.Id;
    }
}

public record RenameProductOptionCommand(int Id, string Name) : IRequest;

public class RenameProductOptionCommandValidator : AbstractValidator<RenameProductOptionCommand>
{
    public RenameProductOptionCommandValidator(IAppDbContext db)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x).MustAsync(async (c, ct) =>
            {
                var productId = await db.ProductOptions.Where(o => o.Id == c.Id).Select(o => o.ProductId).FirstOrDefaultAsync(ct);
                return !await db.ProductOptions.AnyAsync(o => o.ProductId == productId && o.Id != c.Id && o.Name == c.Name.Trim(), ct);
            })
            .WithMessage("This product already has an option with that name.");
    }
}

public class RenameProductOptionCommandHandler(IAppDbContext db) : IRequestHandler<RenameProductOptionCommand>
{
    public async Task Handle(RenameProductOptionCommand request, CancellationToken ct)
    {
        var opt = await db.ProductOptions.FirstOrDefaultAsync(o => o.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"ProductOption {request.Id} not found.");
        opt.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);
    }
}

// Removes the option, its values, and every SKU's selection of it — the
// foreign keys are Restrict, so the dependants have to go first.
public record DeleteProductOptionCommand(int Id) : IRequest;

public class DeleteProductOptionCommandHandler(IAppDbContext db) : IRequestHandler<DeleteProductOptionCommand>
{
    public async Task Handle(DeleteProductOptionCommand request, CancellationToken ct)
    {
        var opt = await db.ProductOptions.FirstOrDefaultAsync(o => o.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"ProductOption {request.Id} not found.");
        db.SkuOptionValues.RemoveRange(await db.SkuOptionValues.Where(s => s.ProductOptionsId == opt.Id).ToListAsync(ct));
        db.ProductOptionValues.RemoveRange(await db.ProductOptionValues.Where(v => v.ProductOptionId == opt.Id).ToListAsync(ct));
        db.ProductOptions.Remove(opt);
        await db.SaveChangesAsync(ct);
    }
}

// ── ProductOptionValue ────────────────────────────────────────────────────────

public record CreateProductOptionValueCommand(int ProductId, int ProductOptionId, string Name) : IRequest<int>;

public class CreateProductOptionValueCommandValidator : AbstractValidator<CreateProductOptionValueCommand>
{
    public CreateProductOptionValueCommandValidator(IAppDbContext db)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x).MustAsync(async (c, ct) =>
                !await db.ProductOptionValues.AnyAsync(v => v.ProductOptionId == c.ProductOptionId && v.Name == c.Name.Trim(), ct))
            .WithMessage("This option already has that value.");
    }
}

public class CreateProductOptionValueCommandHandler(IAppDbContext db) : IRequestHandler<CreateProductOptionValueCommand, int>
{
    public async Task<int> Handle(CreateProductOptionValueCommand request, CancellationToken ct)
    {
        var val = new ProductOptionValue { ProductId = request.ProductId, ProductOptionId = request.ProductOptionId, Name = request.Name.Trim() };
        db.ProductOptionValues.Add(val);
        await db.SaveChangesAsync(ct);
        return val.Id;
    }
}

public record RenameProductOptionValueCommand(int Id, string Name) : IRequest;

public class RenameProductOptionValueCommandValidator : AbstractValidator<RenameProductOptionValueCommand>
{
    public RenameProductOptionValueCommandValidator(IAppDbContext db)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x).MustAsync(async (c, ct) =>
            {
                var optionId = await db.ProductOptionValues.Where(v => v.Id == c.Id).Select(v => v.ProductOptionId).FirstOrDefaultAsync(ct);
                return !await db.ProductOptionValues.AnyAsync(v => v.ProductOptionId == optionId && v.Id != c.Id && v.Name == c.Name.Trim(), ct);
            })
            .WithMessage("This option already has that value.");
    }
}

public class RenameProductOptionValueCommandHandler(IAppDbContext db) : IRequestHandler<RenameProductOptionValueCommand>
{
    public async Task Handle(RenameProductOptionValueCommand request, CancellationToken ct)
    {
        var val = await db.ProductOptionValues.FirstOrDefaultAsync(v => v.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"ProductOptionValue {request.Id} not found.");
        val.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);
    }
}

// Removes the value and clears it from any SKU that had it selected.
public record DeleteProductOptionValueCommand(int Id) : IRequest;

public class DeleteProductOptionValueCommandHandler(IAppDbContext db) : IRequestHandler<DeleteProductOptionValueCommand>
{
    public async Task Handle(DeleteProductOptionValueCommand request, CancellationToken ct)
    {
        var val = await db.ProductOptionValues.FirstOrDefaultAsync(v => v.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"ProductOptionValue {request.Id} not found.");
        db.SkuOptionValues.RemoveRange(await db.SkuOptionValues.Where(s => s.ProductOptionValueId == val.Id).ToListAsync(ct));
        db.ProductOptionValues.Remove(val);
        await db.SaveChangesAsync(ct);
    }
}
