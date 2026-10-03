using ClipDown.Application.Common.Models;
using ClipDown.Domain.Enums;

namespace ClipDown.Application.Common.Interfaces;

public interface IVideoConverter
{
    /// <summary>Falha (sem lançar exceção) quando a entrada não é um vídeo válido ou a conversão não é possível.</summary>
    Task<Result<ConvertedFile>> ConvertAsync(Stream input, VideoConversionTarget target, CancellationToken cancellationToken = default);
}
