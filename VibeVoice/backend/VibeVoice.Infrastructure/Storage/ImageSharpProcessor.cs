using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using VibeVoice.Application.Abstractions;

namespace VibeVoice.Infrastructure.Storage;

public sealed class ImageSharpProcessor : IImageProcessor
{
    public const int ThumbnailSize = 480;

    public async Task<ProcessedImage> ProcessAsync(Stream input, CancellationToken ct)
    {
        using var image = await Image.LoadAsync(input, ct);
        var format = image.Metadata.DecodedImageFormat;

        // Bake the camera rotation into the pixels, then drop every metadata block — EXIF can hold GPS location.
        image.Mutate(x => x.AutoOrient());
        StripMetadata(image);

        var (encoder, ext, mime) = format switch
        {
            PngFormat => ((IImageEncoder)new PngEncoder(), "png", "image/png"),
            GifFormat => (new GifEncoder(), "gif", "image/gif"),
            WebpFormat => (new WebpEncoder { Quality = 90 }, "webp", "image/webp"),
            _ => (new JpegEncoder { Quality = 90 }, "jpg", "image/jpeg"),
        };

        var output = new MemoryStream();
        await image.SaveAsync(output, encoder, ct);
        output.Position = 0;

        // First frame only, so animated GIF thumbnails stay small.
        using var thumb = image.Frames.CloneFrame(0);
        thumb.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(ThumbnailSize, ThumbnailSize), Mode = ResizeMode.Max }));
        var thumbStream = new MemoryStream();
        await thumb.SaveAsync(thumbStream, new WebpEncoder { Quality = 80 }, ct);
        thumbStream.Position = 0;

        return new ProcessedImage(output, thumbStream, image.Width, image.Height, ext, mime);
    }

    private static void StripMetadata(Image image)
    {
        image.Metadata.ExifProfile = null;
        image.Metadata.XmpProfile = null;
        image.Metadata.IptcProfile = null;
        foreach (var frame in image.Frames)
        {
            frame.Metadata.ExifProfile = null;
            frame.Metadata.XmpProfile = null;
            frame.Metadata.IptcProfile = null;
        }
    }
}
