using ClipDown.Application.Common.Models;

namespace ClipDown.Application.Common.Interfaces;

public interface IImageConversionService
{
    /// <summary>Falha (sem lançar exceção) quando o formato não existe ou a entrada não é uma imagem válida.</summary>
    Task<Result<OutputFile>> ConvertAsync(
        Stream input, string fileName, string format, CancellationToken cancellationToken = default);
}
