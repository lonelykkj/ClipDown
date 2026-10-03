using ClipDown.Application.Common.Models;
using ClipDown.Domain.Enums;

namespace ClipDown.Application.Common.Interfaces;

public interface IVideoDownloader
{
    /// <summary>Falha (sem lançar exceção) quando o vídeo não pode ser baixado.</summary>
    Task<Result<OutputFile>> DownloadAsync(Uri url, VideoFormat format, CancellationToken cancellationToken = default);
}
