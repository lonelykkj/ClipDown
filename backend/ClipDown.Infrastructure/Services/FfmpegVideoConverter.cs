using ClipDown.Application.Common.Interfaces;
using ClipDown.Application.Common.Models;
using ClipDown.Domain.Enums;

namespace ClipDown.Infrastructure.Services;

/// <summary>Converte vídeos chamando o ffmpeg (que precisa estar instalado e no PATH).</summary>
public sealed class FfmpegVideoConverter : IVideoConverter
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    public async Task<Result<ConvertedFile>> ConvertAsync(
        Stream input, VideoConversionTarget target, CancellationToken cancellationToken = default)
    {
        var workDir = Path.Combine(Path.GetTempPath(), "clipdown-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);

        try
        {
            // O ffmpeg lê de um arquivo: o formato é detectado pelo conteúdo, então o nome não importa.
            var inputPath = Path.Combine(workDir, "input");
            await using (var file = File.Create(inputPath))
                await input.CopyToAsync(file, cancellationToken);

            var (extension, contentType) = Describe(target);
            var outputPath = Path.Combine(workDir, $"output.{extension}");

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(Timeout);

            var run = await ExternalProcess.RunAsync("ffmpeg", BuildArguments(target, inputPath, outputPath), timeout.Token);
            if (run is null)
                return Result<ConvertedFile>.Failure("ffmpeg não está instalado no servidor.");
            if (!run.Succeeded || !File.Exists(outputPath))
                return Result<ConvertedFile>.Failure(ToFriendlyError(run.StandardError));

            var content = await File.ReadAllBytesAsync(outputPath, cancellationToken);
            return Result<ConvertedFile>.Success(new ConvertedFile(content, contentType, extension));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<ConvertedFile>.Failure("A conversão demorou demais e foi cancelada.");
        }
        finally
        {
            try { Directory.Delete(workDir, recursive: true); } catch (IOException) { /* limpeza best-effort */ }
        }
    }

    private static string[] BuildArguments(VideoConversionTarget target, string inputPath, string outputPath) =>
        target switch
        {
            // -vn: descarta o vídeo. -q:a 2: qualidade VBR alta (~190 kbps).
            VideoConversionTarget.Mp3 =>
                ["-y", "-i", inputPath, "-vn", "-codec:a", "libmp3lame", "-q:a", "2", outputPath],

            // Gera uma paleta de cores a partir do próprio vídeo (palettegen/paletteuse): GIF com
            // muito mais qualidade que a paleta padrão. 15 fps e 480px de largura mantêm o tamanho razoável.
            VideoConversionTarget.Gif =>
                ["-y", "-i", inputPath,
                 "-vf", "fps=15,scale=480:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse",
                 "-loop", "0", outputPath],

            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };

    private static (string Extension, string ContentType) Describe(VideoConversionTarget target) =>
        target switch
        {
            VideoConversionTarget.Mp3 => ("mp3", "audio/mpeg"),
            VideoConversionTarget.Gif => ("gif", "image/gif"),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
        };

    private static string ToFriendlyError(string stderr) =>
        stderr.Contains("does not contain any stream", StringComparison.OrdinalIgnoreCase)
            ? "O vídeo não possui áudio para converter."
            : "O arquivo enviado não é um vídeo válido ou não pôde ser convertido.";
}
