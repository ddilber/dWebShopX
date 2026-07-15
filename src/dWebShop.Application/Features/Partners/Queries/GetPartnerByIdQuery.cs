using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Partners.Queries;

public record PartnerPricelistDto(int Id, int PricelistId, string PricelistName, bool IsActive, bool IsDefault);

public record PartnerDetailDto(
    int Id,
    string FirstName,
    string LastName,
    string CompanyName,
    string Email,
    string Phone,
    List<PartnerPricelistDto> Pricelists);

public record GetPartnerByIdQuery(int Id) : IRequest<PartnerDetailDto?>;

public class GetPartnerByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetPartnerByIdQuery, PartnerDetailDto?>
{
    public async Task<PartnerDetailDto?> Handle(GetPartnerByIdQuery request, CancellationToken ct)
    {
        var partner = await db.Partners
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new { p.Id, p.FirstName, p.LastName, p.CompanyName, p.Email, p.Phone })
            .FirstOrDefaultAsync(ct);

        if (partner is null) return null;

        var pricelists = await db.ClientPricelists
            .AsNoTracking()
            .Where(cp => cp.PartnerId == request.Id)
            .OrderByDescending(cp => cp.IsDefault)
            .ThenBy(cp => cp.Pricelist!.Name)
            .Select(cp => new PartnerPricelistDto(
                cp.Id,
                cp.PricelistId,
                cp.Pricelist!.Name,
                cp.Pricelist.IsActive,
                cp.IsDefault))
            .ToListAsync(ct);

        return new PartnerDetailDto(
            partner.Id, partner.FirstName, partner.LastName,
            partner.CompanyName, partner.Email, partner.Phone,
            pricelists);
    }
}
