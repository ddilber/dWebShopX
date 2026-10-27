using dWebShop.Application.Common.Interfaces;
using dWebShop.Domain.Entities.Social;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Social.Commands;

public record CreateSocialVideoCommand(
    string Platform,
    string Caption,
    string Duration,
    string PosterImage,
    string EmbedUrl,
    string FallbackUrl,
    bool Published) : IRequest<int>;

public class CreateSocialVideoCommandValidator : AbstractValidator<CreateSocialVideoCommand>
{
    public CreateSocialVideoCommandValidator()
    {
        RuleFor(x => x.Platform).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Caption).MaximumLength(300);
        // A card with neither URL would render as a dead tile.
        RuleFor(x => x.FallbackUrl)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.EmbedUrl))
            .WithMessage("Set an embed URL, a fallback link, or both — otherwise the card goes nowhere.");
    }
}

public class CreateSocialVideoCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateSocialVideoCommand, int>
{
    public async Task<int> Handle(CreateSocialVideoCommand request, CancellationToken ct)
    {
        // New videos go to the end of the strip.
        var next = await db.SocialVideos.AnyAsync(ct)
            ? await db.SocialVideos.MaxAsync(x => x.SortOrder, ct) + 1
            : 0;

        var video = new SocialVideo
        {
            Platform = request.Platform,
            Caption = request.Caption,
            Duration = request.Duration,
            PosterImage = request.PosterImage,
            EmbedUrl = request.EmbedUrl,
            FallbackUrl = request.FallbackUrl,
            Published = request.Published,
            SortOrder = next,
        };

        db.SocialVideos.Add(video);
        await db.SaveChangesAsync(ct);
        return video.Id;
    }
}

public record UpdateSocialVideoCommand(
    int Id,
    string Platform,
    string Caption,
    string Duration,
    string PosterImage,
    string EmbedUrl,
    string FallbackUrl,
    bool Published) : IRequest;

public class UpdateSocialVideoCommandValidator : AbstractValidator<UpdateSocialVideoCommand>
{
    public UpdateSocialVideoCommandValidator()
    {
        RuleFor(x => x.Platform).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Caption).MaximumLength(300);
        RuleFor(x => x.FallbackUrl)
            .NotEmpty()
            .When(x => string.IsNullOrWhiteSpace(x.EmbedUrl))
            .WithMessage("Set an embed URL, a fallback link, or both — otherwise the card goes nowhere.");
    }
}

public class UpdateSocialVideoCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateSocialVideoCommand>
{
    public async Task Handle(UpdateSocialVideoCommand request, CancellationToken ct)
    {
        var video = await db.SocialVideos.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Social video {request.Id} not found.");

        video.Platform = request.Platform;
        video.Caption = request.Caption;
        video.Duration = request.Duration;
        video.PosterImage = request.PosterImage;
        video.EmbedUrl = request.EmbedUrl;
        video.FallbackUrl = request.FallbackUrl;
        video.Published = request.Published;

        await db.SaveChangesAsync(ct);
    }
}

public record DeleteSocialVideoCommand(int Id) : IRequest;

public class DeleteSocialVideoCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteSocialVideoCommand>
{
    public async Task Handle(DeleteSocialVideoCommand request, CancellationToken ct)
    {
        var video = await db.SocialVideos.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Social video {request.Id} not found.");
        db.SocialVideos.Remove(video);
        await db.SaveChangesAsync(ct);
    }
}

public record ToggleSocialVideoPublishedCommand(int Id) : IRequest<bool>;

public class ToggleSocialVideoPublishedCommandHandler(IAppDbContext db)
    : IRequestHandler<ToggleSocialVideoPublishedCommand, bool>
{
    public async Task<bool> Handle(ToggleSocialVideoPublishedCommand request, CancellationToken ct)
    {
        var video = await db.SocialVideos.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"Social video {request.Id} not found.");
        video.Published = !video.Published;
        await db.SaveChangesAsync(ct);
        return video.Published;
    }
}

/// <summary>
/// Moves one video up or down the strip by swapping SortOrder with its
/// neighbour. Rows are renumbered first, so a list that was never ordered (all
/// zeros, or gaps left by deletes) still moves predictably.
/// </summary>
public record MoveSocialVideoCommand(int Id, bool Up) : IRequest;

public class MoveSocialVideoCommandHandler(IAppDbContext db)
    : IRequestHandler<MoveSocialVideoCommand>
{
    public async Task Handle(MoveSocialVideoCommand request, CancellationToken ct)
    {
        var all = await db.SocialVideos
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToListAsync(ct);

        for (var i = 0; i < all.Count; i++) all[i].SortOrder = i;

        var index = all.FindIndex(x => x.Id == request.Id);
        if (index < 0) throw new KeyNotFoundException($"Social video {request.Id} not found.");

        var target = request.Up ? index - 1 : index + 1;
        if (target < 0 || target >= all.Count)
        {
            // Already at the end — the renumbering above is still worth saving.
            await db.SaveChangesAsync(ct);
            return;
        }

        (all[index].SortOrder, all[target].SortOrder) = (all[target].SortOrder, all[index].SortOrder);
        await db.SaveChangesAsync(ct);
    }
}
