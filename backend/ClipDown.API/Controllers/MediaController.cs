using ClipDown.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClipDown.API.Controllers;

[ApiController]
[Route("api/media")]
public class MediaController(
    ImageConverter imageConverter,
    VideoConverter videoConverter,
    VideoDownloader videoDownloader) : ControllerBase
{
    private const long MaxImageSizeBytes = 50 * 1024 * 1024;
    private const long MaxVideoSizeBytes = 200 * 1024 * 1024;

    public record DownloadRequest(string Url, string Format);

    [HttpPost("convert/image")]
    [RequestSizeLimit(MaxImageSizeBytes)]
    public async Task<IActionResult> ConvertImage(
        IFormFile file, [FromForm] string format, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return ToResponse(await imageConverter.ConvertAsync(stream, file.FileName, format, cancellationToken));
    }

    [HttpPost("convert/video")]
    [RequestSizeLimit(MaxVideoSizeBytes)]
    // O limite de formulário do .NET (128 MB) barraria uploads maiores mesmo com o RequestSizeLimit.
    [RequestFormLimits(MultipartBodyLengthLimit = MaxVideoSizeBytes)]
    public async Task<IActionResult> ConvertVideo(
        IFormFile file, [FromForm] string format, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return ToResponse(await videoConverter.ConvertAsync(stream, file.FileName, format, cancellationToken));
    }

    [HttpPost("download")]
    public async Task<IActionResult> DownloadVideo(
        [FromBody] DownloadRequest request, CancellationToken cancellationToken) =>
        ToResponse(await videoDownloader.DownloadAsync(request.Url, request.Format, cancellationToken));

    /// <summary>Sucesso: devolve o arquivo. Falha: devolve 400 com a mensagem de erro.</summary>
    private IActionResult ToResponse(Result<OutputFile> result) =>
        result.IsSuccess
            ? File(result.Value.Content, result.Value.ContentType, result.Value.FileName)
            : Problem(result.Error, statusCode: StatusCodes.Status400BadRequest);
}
