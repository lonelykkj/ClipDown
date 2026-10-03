using ClipDown.Application.Common.Models;
using ClipDown.Domain.Enums;

namespace ClipDown.Application.Common.Interfaces;

public interface IImageConverter
{
    /// <summary>Falha (sem lançar exceção) quando a entrada não é uma imagem válida ou suportada.</summary>
    Task<Result<ConvertedFile>> ConvertAsync(Stream input, ImageFormat targetFormat, CancellationToken cancellationToken = default);
}
