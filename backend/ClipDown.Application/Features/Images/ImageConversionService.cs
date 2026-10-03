using ClipDown.Application.Common;
using ClipDown.Application.Common.Interfaces;
using ClipDown.Application.Common.Models;
using ClipDown.Domain.Enums;

namespace ClipDown.Application.Features.Images;

public sealed class ImageConversionService(IImageConverter imageConverter) : IImageConversionService
{
    public async Task<Result<OutputFile>> ConvertAsync(
        Stream input, string fileName, string format, CancellationToken cancellationToken = default)
    {
        if (!TryParseFormat(format, out var targetFormat))
            return Result<OutputFile>.Failure(
                $"Formato inválido. Use: {string.Join(", ", Enum.GetNames<ImageFormat>())}.");

        var converted = await imageConverter.ConvertAsync(input, targetFormat, cancellationToken);
        if (!converted.IsSuccess)
            return Result<OutputFile>.Failure(converted.Error);

        var outputName = $"{Path.GetFileNameWithoutExtension(fileName)}.{converted.Value.Extension}";
        return Result<OutputFile>.Success(
            new OutputFile(converted.Value.Content, converted.Value.ContentType, outputName));
    }

    private static bool TryParseFormat(string? value, out ImageFormat format)
    {
        // "jpeg" é aceito como apelido de "jpg".
        if (string.Equals(value?.Trim(), "jpeg", StringComparison.OrdinalIgnoreCase))
        {
            format = ImageFormat.Jpg;
            return true;
        }

        return FormatParser.TryParse(value, out format);
    }
}
