namespace ClipDown.API.Services;

/// <summary>Converte vídeos em MP3 ou GIF usando o ffmpeg (precisa estar instalado e no PATH).</summary>
public sealed class VideoConverter
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    public async Task<Result<OutputFile>> ConvertAsync(
        Stream input, string fileName, string format, CancellationToken cancellationToken)
    {
        var extension = format.Trim().ToLowerInvariant();
        string[]? options = extension switch
        {
            // -vn: descarta o vídeo. -q:a 2: qualidade alta (~190 kbps).
            "mp3" => ["-vn", "-codec:a", "libmp3lame", "-q:a", "2"],

            // Gera uma paleta de cores a partir do próprio vídeo (palettegen/paletteuse), o que deixa o GIF
            // muito melhor que a paleta padrão. 15 fps e 480 px de largura mantêm o tamanho razoável.
            "gif" => ["-vf", "fps=15,scale=480:-1:flags=lanczos,split[a][b];[a]palettegen[p];[b][p]paletteuse", "-loop", "0"],

            _ => null
        };
        if (options is null)
            return Result<OutputFile>.Failure("Formato inválido. Use: mp3 ou gif.");

        var workDir = Directory.CreateTempSubdirectory("clipdown-");
        try
        {
            // O ffmpeg trabalha com arquivos, então o upload é salvo numa pasta temporária.
            var inputPath = Path.Combine(workDir.FullName, "input");
            var outputPath = Path.Combine(workDir.FullName, $"output.{extension}");
            await using (var file = File.Create(inputPath))
                await input.CopyToAsync(file, cancellationToken);

            string[] arguments = ["-y", "-i", inputPath, .. options, outputPath];
            var error = await ExternalProcess.RunAsync("ffmpeg", arguments, Timeout, DescribeError, cancellationToken);
            if (error is not null)
                return Result<OutputFile>.Failure(error);

            var content = await File.ReadAllBytesAsync(outputPath, cancellationToken);
            var contentType = extension == "mp3" ? "audio/mpeg" : "image/gif";
            var outputName = $"{Path.GetFileNameWithoutExtension(fileName)}.{extension}";
            return Result<OutputFile>.Success(new OutputFile(content, contentType, outputName));
        }
        finally
        {
            workDir.Delete(recursive: true);
        }
    }

    private static string DescribeError(string stderr) =>
        stderr.Contains("does not contain any stream")
            ? "O vídeo não possui áudio para converter."
            : "O arquivo enviado não é um vídeo válido ou não pôde ser convertido.";
}
