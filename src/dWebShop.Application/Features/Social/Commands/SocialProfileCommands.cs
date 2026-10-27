using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Social;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Social.Commands;

/// <summary>
/// Saves the whole profile list in one go — the admin screen edits them as a
/// small fixed block rather than one row at a time. Rows missing from the
/// payload are deleted.
/// </summary>
public record SaveSocialProfilesCommand(List<SocialProfileInput> Profiles) : IRequest;

public record SocialProfileInput(
    int Id,
    string Key,
    string Name,
    string Handle,
    string Url,
    bool Published);

public class SaveSocialProfilesCommandValidator : AbstractValidator<SaveSocialProfilesCommand>
{
    public SaveSocialProfilesCommandValidator()
    {
        RuleForEach(x => x.Profiles).ChildRules(p =>
        {
            p.RuleFor(x => x.Key).NotEmpty().MaximumLength(50);
            p.RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            p.RuleFor(x => x.Url).NotEmpty().MaximumLength(500);
        });

        RuleFor(x => x.Profiles)
            .Must(list => list
                .Select(p => p.Key.Trim().ToLowerInvariant())
                .Distinct()
                .Count() == list.Count)
            .WithMessage("Each network key can only appear once.");
    }
}

public class SaveSocialProfilesCommandHandler(IAppDbContext db)
    : IRequestHandler<SaveSocialProfilesCommand>
{
    public async Task Handle(SaveSocialProfilesCommand request, CancellationToken ct)
    {
        var existing = await db.SocialProfiles.ToListAsync(ct);

        var keptIds = request.Profiles.Where(p => p.Id > 0).Select(p => p.Id).ToHashSet();
        foreach (var gone in existing.Where(e => !keptIds.Contains(e.Id)))
            db.SocialProfiles.Remove(gone);

        for (var i = 0; i < request.Profiles.Count; i++)
        {
            var input = request.Profiles[i];
            var row = input.Id > 0 ? existing.FirstOrDefault(e => e.Id == input.Id) : null;

            if (row is null)
            {
                row = new SocialProfile();
                db.SocialProfiles.Add(row);
            }

            row.Key = input.Key.Trim().ToLowerInvariant();
            row.Name = input.Name.Trim();
            row.Handle = input.Handle.Trim();
            row.Url = input.Url.Trim();
            row.Published = input.Published;
            row.SortOrder = i;
        }

        await db.SaveChangesAsync(ct);
    }
}
