using ClipDown.Application.Common;
using ClipDown.Application.Common.Interfaces;
using ClipDown.Application.Common.Models;
using ClipDown.Domain.Enums;

namespace ClipDown.Application.Features.Videos;

public sealed class VideoConversionService(IVideoConverter videoConverter) : IVideoConversionService
{
    public async Task<Result<OutputFile>> ConvertAsync(
        Stream input, string fileName, string format, CancellationToken cancellationToken = default)
    {
        if (!FormatParser.TryParse<VideoConversionTarget>(format, out var target))
            return Result<OutputFile>.Failure(
                $"Formato inválido. Use: {string.Join(", ", Enum.GetNames<VideoConversionTarget>())}.");

        var converted = await videoConverter.ConvertAsync(input, target, cancellationToken);
        if (!converted.IsSuccess)
            return Result<OutputFile>.Failure(converted.Error);

        var outputName = $"{Path.GetFileNameWithoutExtension(fileName)}.{converted.Value.Extension}";
        return Result<OutputFile>.Success(
            new OutputFile(converted.Value.Content, converted.Value.ContentType, outputName));
    }
}
