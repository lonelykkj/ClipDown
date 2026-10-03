using ClipDown.Application.Common.Interfaces;
using ClipDown.Application.Common.Models;
using ClipDown.Domain.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ClipDown.Infrastructure.Services;

public sealed class ImageSharpImageConverter : IImageConverter
{
    public async Task<Result<ConvertedFile>> ConvertAsync(
        Stream input, ImageFormat targetFormat, CancellationToken cancellationToken = default)
    {
        Image image;
        try
        {
            image = await Image.LoadAsync(input, cancellationToken);
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException or NotSupportedException)
        {
            return Result<ConvertedFile>.Failure("O arquivo enviado não é uma imagem válida ou suportada.");
        }

        using (image)
        {
            // JPEG não suporta transparência: sem isso, áreas transparentes ficariam pretas.
            if (targetFormat == ImageFormat.Jpg)
                image.Mutate(ctx => ctx.BackgroundColor(Color.White));

            var (encoder, contentType, extension) = Describe(targetFormat);

            using var output = new MemoryStream();
            await image.SaveAsync(output, encoder, cancellationToken);
            return Result<ConvertedFile>.Success(new ConvertedFile(output.ToArray(), contentType, extension));
        }
    }

    private static (IImageEncoder Encoder, string ContentType, string Extension) Describe(ImageFormat format) =>
        format switch
        {
            ImageFormat.Png => (new PngEncoder(), "image/png", "png"),
            ImageFormat.Jpg => (new JpegEncoder(), "image/jpeg", "jpg"),
            ImageFormat.WebP => (new WebpEncoder(), "image/webp", "webp"),
            ImageFormat.Gif => (new GifEncoder(), "image/gif", "gif"),
            ImageFormat.Bmp => (new BmpEncoder(), "image/bmp", "bmp"),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
}
