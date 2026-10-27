using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Products;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Products.Queries;

public record ProductListItemDto(int Id, string Name, string SKU, string Slug, ProductStatus Status, int? BrandId, string? BrandName, string? PrimaryImage, int TotalStock = 0, decimal? MinPrice = null);

public record PagedResult<T>(List<T> Items, int TotalCount, int StartIndex, int Count);

public record GetProductsQuery(
    int StartIndex = 0,
    int Count = 20,
    int? BrandId = null,
    int? CategoryId = null,
    ProductStatus? Status = null,
    string? Search = null,
    string? Sort = null,
    int[]? BrandIds = null,
    int[]? CategoryIds = null,
    bool IncludePrice = false,
    ProductStockFilter Stock = ProductStockFilter.Any) : IRequest<PagedResult<ProductListItemDto>>;

// Low: at least one SKU in stock but at/below its low-stock threshold.
// Out: has SKUs and none of them is in stock.
public enum ProductStockFilter { Any, Low, Out }

public class GetProductsQueryHandler(IAppDbContextFactory dbFactory) : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    public async Task<PagedResult<ProductListItemDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var query = db.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Brand)
            .Include(p => p.ProductDetails)
                .ThenInclude(pd => pd!.Images)
            .Include(p => p.ProductSkus)
            .AsQueryable();

        if (request.BrandIds?.Length > 0)
            query = query.Where(p => p.BrandId.HasValue && request.BrandIds.Contains(p.BrandId.Value));
        else if (request.BrandId.HasValue)
            query = query.Where(p => p.BrandId == request.BrandId);

        if (request.CategoryIds?.Length > 0)
            query = query.Where(p => p.Categories!.Any(c => request.CategoryIds.Contains(c.Id)));
        else if (request.CategoryId.HasValue)
            query = query.Where(p => p.Categories!.Any(c => c.Id == request.CategoryId));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) || p.SKU.ToLower().Contains(s) || p.Description.ToLower().Contains(s));
        }

        if(request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status);
        }

        if (request.Stock == ProductStockFilter.Low)
            query = query.Where(p => p.ProductSkus!.Any(s => s.StockQuantity > 0 && s.StockQuantity <= s.LowStockThreshold));
        else if (request.Stock == ProductStockFilter.Out)
            query = query.Where(p => p.ProductSkus!.Any() && p.ProductSkus!.All(s => s.StockQuantity <= 0));

        var total = await query.CountAsync(ct);

        // Sort is "<Property>:asc|desc", as produced by the admin data grid.
        var sortParts = (request.Sort ?? "").Split(':');
        var desc = sortParts.Length > 1 && sortParts[1] == "desc";
        IOrderedQueryable<Product> ordered = sortParts[0] switch
        {
            "BrandName" => desc
                ? query.OrderByDescending(p => p.Brand != null ? p.Brand.Name : null)
                : query.OrderBy(p => p.Brand != null ? p.Brand.Name : null),
            "MinPrice" => desc
                ? query.OrderByDescending(p => p.ProductSkus!.Min(s => (decimal?)s.Price))
                : query.OrderBy(p => p.ProductSkus!.Min(s => (decimal?)s.Price)),
            "Name" when desc => query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name),
        };

        var items = await ordered
            .ThenBy(p => p.Id)
            .Skip(request.StartIndex)
            .Take(request.Count)
            .Select(p => new ProductListItemDto(
                p.Id, p.Name,
                string.Join(", ", p.ProductSkus.OrderBy(s => s.Id).Take(3).Select(s => s.SKU))
                    + (p.ProductSkus.Count() > 3 ? ", …" : ""),
                p.Slug, p.Status,
                p.BrandId, p.Brand != null ? p.Brand.Name : null,
                p.ProductSkus.OrderBy(s => s.Id).FirstOrDefault() != null && p.ProductSkus.OrderBy(s => s.Id).First().ImagePath != null
                    ? p.ProductSkus.OrderBy(s => s.Id).First().ImagePath
                    : p.ProductDetails != null && p.ProductDetails.Images!.Any()
                        ? p.ProductDetails.Images!.First().Path
                        : null,
                p.ProductSkus.Sum(s => (int?)s.StockQuantity) ?? 0,
                request.IncludePrice
                    ? p.ProductSkus.Min(s => (decimal?)s.Price)
                    : null))
            .ToListAsync(ct);

        return new PagedResult<ProductListItemDto>(items, total, request.StartIndex, request.Count);
    }
}
