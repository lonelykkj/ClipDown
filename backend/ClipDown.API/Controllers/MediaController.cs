using ClipDown.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClipDown.API.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController(IImageConversionService imageConversionService) : ControllerBase
{
    private const long MaxImageSizeBytes = 50 * 1024 * 1024;

    [HttpPost("convert/image")]
    [RequestSizeLimit(MaxImageSizeBytes)]
    public async Task<IActionResult> ConvertImage(
        IFormFile file, [FromForm] string format, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return Problem("O arquivo enviado está vazio.", statusCode: StatusCodes.Status400BadRequest);

        await using var stream = file.OpenReadStream();
        var result = await imageConversionService.ConvertAsync(stream, file.FileName, format, cancellationToken);

        if (!result.IsSuccess)
            return Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);

        return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }
}
