using ClipDown.Application.Common.Models;

namespace ClipDown.Application.Common.Interfaces;

public interface IVideoConversionService
{
    /// <summary>Falha (sem lançar exceção) quando o formato não existe ou a entrada não é um vídeo válido.</summary>
    Task<Result<OutputFile>> ConvertAsync(
        Stream input, string fileName, string format, CancellationToken cancellationToken = default);
}
