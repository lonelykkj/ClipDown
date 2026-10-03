using ClipDown.Application.Common.Models;

namespace ClipDown.Application.Common.Interfaces;

public interface IVideoDownloadService
{
    /// <summary>Falha (sem lançar exceção) quando a URL ou o formato são inválidos, ou o download não é possível.</summary>
    Task<Result<OutputFile>> DownloadAsync(string url, string format, CancellationToken cancellationToken = default);
}
