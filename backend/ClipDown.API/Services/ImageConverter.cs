using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ClipDown.API.Services;

/// <summary>Converte imagens entre formatos usando o ImageSharp.</summary>
public sealed class ImageConverter
{
    // Formatos de saída aceitos, sem diferenciar maiúsculas. "jpeg" é apelido de "jpg".
    private static readonly Dictionary<string, (IImageEncoder Encoder, string ContentType, string Extension)> Formats =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["png"] = (new PngEncoder(), "image/png", "png"),
            ["jpg"] = (new JpegEncoder(), "image/jpeg", "jpg"),
            ["jpeg"] = (new JpegEncoder(), "image/jpeg", "jpg"),
            ["webp"] = (new WebpEncoder(), "image/webp", "webp"),
            ["gif"] = (new GifEncoder(), "image/gif", "gif"),
            ["bmp"] = (new BmpEncoder(), "image/bmp", "bmp")
        };

    public async Task<Result<OutputFile>> ConvertAsync(
        Stream input, string fileName, string format, CancellationToken cancellationToken)
    {
        if (!Formats.TryGetValue(format.Trim(), out var target))
            return Result<OutputFile>.Failure("Formato inválido. Use: png, jpg, webp, gif ou bmp.");

        try
        {
            // O formato de entrada é detectado pelo conteúdo do arquivo, não pela extensão.
            using var image = await Image.LoadAsync(input, cancellationToken);

            // JPG não suporta transparência: sem isso, áreas transparentes ficariam pretas.
            if (target.Extension == "jpg")
                image.Mutate(ctx => ctx.BackgroundColor(Color.White));

            using var output = new MemoryStream();
            await image.SaveAsync(output, target.Encoder, cancellationToken);

            var outputName = $"{Path.GetFileNameWithoutExtension(fileName)}.{target.Extension}";
            return Result<OutputFile>.Success(new OutputFile(output.ToArray(), target.ContentType, outputName));
        }
        catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException or NotSupportedException)
        {
            return Result<OutputFile>.Failure("O arquivo enviado não é uma imagem válida ou suportada.");
        }
    }
}
