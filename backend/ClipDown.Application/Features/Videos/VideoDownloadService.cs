using ClipDown.Application.Common;
using ClipDown.Application.Common.Interfaces;
using ClipDown.Application.Common.Models;
using ClipDown.Domain.Enums;

namespace ClipDown.Application.Features.Videos;

public sealed class VideoDownloadService(IVideoDownloader videoDownloader) : IVideoDownloadService
{
    public Task<Result<OutputFile>> DownloadAsync(
        string url, string format, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https"))
            return Task.FromResult(Result<OutputFile>.Failure("Informe um link válido (http ou https)."));

        if (!FormatParser.TryParse<VideoFormat>(format, out var videoFormat))
            return Task.FromResult(Result<OutputFile>.Failure(
                $"Formato inválido. Use: {string.Join(", ", Enum.GetNames<VideoFormat>())}."));

        return videoDownloader.DownloadAsync(uri, videoFormat, cancellationToken);
    }
}
