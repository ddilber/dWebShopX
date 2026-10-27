using dWebShop.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace dWebShop.Application.Features.Social.Queries;

public record SocialProfileDto(
    int Id,
    string Key,
    string Name,
    string Handle,
    string Url,
    int SortOrder,
    bool Published);

public record SocialVideoDto(
    int Id,
    string Platform,
    string Caption,
    string Duration,
    string PosterImage,
    string EmbedUrl,
    string FallbackUrl,
    int SortOrder,
    bool Published);

// Public: what the homepage block renders. Published only, in display order.
public record GetPublishedSocialContentQuery(int MaxVideos = 3)
    : IRequest<SocialContentResult>;

public record SocialContentResult(
    List<SocialProfileDto> Profiles,
    List<SocialVideoDto> Videos);

public class GetPublishedSocialContentQueryHandler(IAppDbContext db)
    : IRequestHandler<GetPublishedSocialContentQuery, SocialContentResult>
{
    public async Task<SocialContentResult> Handle(GetPublishedSocialContentQuery request, CancellationToken ct)
    {
        var profiles = await db.SocialProfiles
            .AsNoTracking()
            .Where(x => x.Published)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new SocialProfileDto(
                x.Id, x.Key, x.Name, x.Handle, x.Url, x.SortOrder, x.Published))
            .ToListAsync(ct);

        var videos = await db.SocialVideos
            .AsNoTracking()
            .Where(x => x.Published)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Take(request.MaxVideos)
            .Select(x => new SocialVideoDto(
                x.Id, x.Platform, x.Caption, x.Duration, x.PosterImage,
                x.EmbedUrl, x.FallbackUrl, x.SortOrder, x.Published))
            .ToListAsync(ct);

        return new SocialContentResult(profiles, videos);
    }
}

// Admin: everything, drafts included.
public record GetSocialContentQuery : IRequest<SocialContentResult>;

public class GetSocialContentQueryHandler(IAppDbContext db)
    : IRequestHandler<GetSocialContentQuery, SocialContentResult>
{
    public async Task<SocialContentResult> Handle(GetSocialContentQuery request, CancellationToken ct)
    {
        var profiles = await db.SocialProfiles
            .AsNoTracking()
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new SocialProfileDto(
                x.Id, x.Key, x.Name, x.Handle, x.Url, x.SortOrder, x.Published))
            .ToListAsync(ct);

        var videos = await db.SocialVideos
            .AsNoTracking()
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new SocialVideoDto(
                x.Id, x.Platform, x.Caption, x.Duration, x.PosterImage,
                x.EmbedUrl, x.FallbackUrl, x.SortOrder, x.Published))
            .ToListAsync(ct);

        return new SocialContentResult(profiles, videos);
    }
}

// Admin: one video for the editor.
public record GetSocialVideoByIdQuery(int Id) : IRequest<SocialVideoDto?>;

public class GetSocialVideoByIdQueryHandler(IAppDbContext db)
    : IRequestHandler<GetSocialVideoByIdQuery, SocialVideoDto?>
{
    public async Task<SocialVideoDto?> Handle(GetSocialVideoByIdQuery request, CancellationToken ct) =>
        await db.SocialVideos
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new SocialVideoDto(
                x.Id, x.Platform, x.Caption, x.Duration, x.PosterImage,
                x.EmbedUrl, x.FallbackUrl, x.SortOrder, x.Published))
            .FirstOrDefaultAsync(ct);
}
