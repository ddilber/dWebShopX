using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Orders.Commands;

public record UpdateOrderNotesCommand(int OrderId, string? Notes) : IRequest;

public class UpdateOrderNotesCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateOrderNotesCommand>
{
    public async Task Handle(UpdateOrderNotesCommand request, CancellationToken ct)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new InvalidOperationException("Order not found.");

        order.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        await db.SaveChangesAsync(ct);
    }
}
