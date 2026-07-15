using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;

namespace dWebShop.Web;

// Social crawlers (Facebook, X/Twitter, LinkedIn) do not render WebP link
// previews reliably, and most of the shop/inspiration imagery is stored as
// WebP. This endpoint serves a JPEG rendition of any image under the shared
// uploads root, contained (never cropped) on the 1200x630 canvas that the
// platforms expect. The <meta property="og:image"> tags point here instead
// of at the raw file. SkiaSharp is used (MIT-licensed) for the conversion.
public static class OgImageEndpoint
{
    // Facebook / LinkedIn recommended link-image size (1.91:1).
    private const int TargetWidth = 1200;
    private const int TargetHeight = 630;

    public static void MapOgImage(this WebApplication app)
    {
        app.MapGet("/og/{**path}", (
            string path,
            HttpContext ctx,
            IMemoryCache cache,
            IConfiguration config) =>
        {
            // Content images (inspiration/product/brand) live under the shared
            // uploads root; site-chrome images (the default card image, logo)
            // live in wwwroot. Resolve against both, guarding path traversal.
            var roots = new List<string>();
            var sharedUploads = config["SharedUploadsPath"];
            if (!string.IsNullOrWhiteSpace(sharedUploads))
                roots.Add(Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, sharedUploads)));
            if (!string.IsNullOrWhiteSpace(app.Environment.WebRootPath))
                roots.Add(Path.GetFullPath(app.Environment.WebRootPath));

            string? full = null;
            foreach (var root in roots)
            {
                var candidate = Path.GetFullPath(Path.Combine(root, path));
                var rootPrefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
                if (candidate.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) && File.Exists(candidate))
                {
                    full = candidate;
                    break;
                }
            }
            if (full is null)
                return Results.NotFound();

            // Cache the encoded JPEG keyed by path + last-write, so repeated
            // crawler hits don't re-decode/re-encode the image every time.
            var lastWrite = File.GetLastWriteTimeUtc(full);
            var cacheKey = $"ogimg::{full}::{lastWrite.Ticks}";

            if (!cache.TryGetValue(cacheKey, out byte[]? jpeg))
            {
                jpeg = Render(full);
                if (jpeg is null)
                    return Results.NotFound();

                cache.Set(cacheKey, jpeg, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromHours(12),
                });
            }

            ctx.Response.Headers.CacheControl = "public, max-age=604800"; // 7 days
            return Results.File(jpeg!, "image/jpeg");
        })
        .AllowAnonymous();
    }

    // Decode the source image and draw it, scaled to fit (contain), centered on
    // a white 1200x630 canvas. Returns null for unsupported/corrupt sources.
    private static byte[]? Render(string filePath)
    {
        try
        {
            using var source = SKBitmap.Decode(filePath);
            if (source is null)
                return null;

            var scale = Math.Min(
                (float)TargetWidth / source.Width,
                (float)TargetHeight / source.Height);
            var drawW = source.Width * scale;
            var drawH = source.Height * scale;
            var left = (TargetWidth - drawW) / 2f;
            var top = (TargetHeight - drawH) / 2f;

            var info = new SKImageInfo(TargetWidth, TargetHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);
            using (var paint = new SKPaint { IsAntialias = true })
            {
                var dest = new SKRect(left, top, left + drawW, top + drawH);
                canvas.DrawBitmap(source, dest, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), paint);
            }
            canvas.Flush();

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 82);
            return data.ToArray();
        }
        catch
        {
            return null;
        }
    }
}
