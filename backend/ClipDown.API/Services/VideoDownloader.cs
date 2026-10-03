namespace ClipDown.API.Services;

/// <summary>Baixa vídeos por URL usando o yt-dlp (precisa estar instalado e no PATH, junto com o ffmpeg).</summary>
public sealed class VideoDownloader
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(10);

    public async Task<Result<OutputFile>> DownloadAsync(string url, string format, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            return Result<OutputFile>.Failure("Informe um link válido (http ou https).");

        var extension = format.Trim().ToLowerInvariant();
        var sortOrder = extension switch
        {
            // Prefere H.264 + AAC, que tocam em qualquer player (QuickTime, iPhone, editores). Sem isso o
            // yt-dlp costuma escolher VP9, que fica dentro de um .mp4 mas não abre em vários deles.
            "mp4" => "vcodec:h264,res,acodec:m4a",
            "webm" => "ext:webm:webm",
            _ => null
        };
        if (sortOrder is null)
            return Result<OutputFile>.Failure("Formato inválido. Use: mp4 ou webm.");

        var workDir = Directory.CreateTempSubdirectory("clipdown-");
        try
        {
            string[] arguments =
            [
                "--no-playlist",
                "--no-warnings",
                "--restrict-filenames",
                "--max-filesize", "500M",
                "-S", sortOrder,
                "--merge-output-format", extension,
                "-o", Path.Combine(workDir.FullName, "%(title).100B.%(ext)s"),
                // "--" garante que a URL nunca seja interpretada como uma opção do yt-dlp.
                "--", uri.AbsoluteUri
            ];
            var error = await ExternalProcess.RunAsync("yt-dlp", arguments, Timeout, DescribeError, cancellationToken);
            if (error is not null)
                return Result<OutputFile>.Failure(error);

            var file = workDir.GetFiles().FirstOrDefault();
            if (file is null)
                return Result<OutputFile>.Failure("Não foi possível baixar o vídeo.");

            var content = await File.ReadAllBytesAsync(file.FullName, cancellationToken);
            return Result<OutputFile>.Success(new OutputFile(content, $"video/{extension}", file.Name));
        }
        finally
        {
            workDir.Delete(recursive: true);
        }
    }

    /// <summary>Usa a última linha "ERROR: ..." do yt-dlp, que explica o motivo da falha.</summary>
    private static string DescribeError(string stderr)
    {
        var line = stderr.Split('\n').LastOrDefault(l => l.StartsWith("ERROR:"));
        return line is null
            ? "Não foi possível baixar o vídeo."
            : $"Não foi possível baixar o vídeo: {line["ERROR:".Length..].Trim()}";
    }
}
