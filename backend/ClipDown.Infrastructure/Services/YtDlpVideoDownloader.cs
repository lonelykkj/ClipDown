using System.ComponentModel;
using System.Diagnostics;
using ClipDown.Application.Common.Interfaces;
using ClipDown.Application.Common.Models;
using ClipDown.Domain.Enums;

namespace ClipDown.Infrastructure.Services;

/// <summary>Baixa vídeos chamando o yt-dlp (que precisa estar instalado e no PATH, junto com o ffmpeg).</summary>
public sealed class YtDlpVideoDownloader : IVideoDownloader
{
    private const string MaxFileSize = "500M";
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(10);

    public async Task<Result<OutputFile>> DownloadAsync(
        Uri url, VideoFormat format, CancellationToken cancellationToken = default)
    {
        var workDir = Path.Combine(Path.GetTempPath(), "clipdown-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(Timeout);

            var run = await RunYtDlpAsync(BuildArguments(url, format, workDir), timeout.Token);
            if (!run.Success)
                return Result<OutputFile>.Failure(run.Error);

            // Ignora arquivos temporários do yt-dlp (.part, .ytdl) caso sobrem no diretório.
            var file = Directory.GetFiles(workDir)
                .Where(f => !f.EndsWith(".part") && !f.EndsWith(".ytdl"))
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.Length)
                .FirstOrDefault();

            if (file is null)
                return Result<OutputFile>.Failure("Não foi possível baixar o vídeo.");

            var content = await File.ReadAllBytesAsync(file.FullName, cancellationToken);
            return Result<OutputFile>.Success(new OutputFile(content, ContentTypeOf(format), file.Name));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<OutputFile>.Failure("O download demorou demais e foi cancelado.");
        }
        finally
        {
            try { Directory.Delete(workDir, recursive: true); } catch (IOException) { /* limpeza best-effort */ }
        }
    }

    private static List<string> BuildArguments(Uri url, VideoFormat format, string workDir)
    {
        var extension = format == VideoFormat.Mp4 ? "mp4" : "webm";

        return
        [
            "--no-playlist",
            "--no-warnings",
            "--restrict-filenames",
            "--max-filesize", MaxFileSize,
            "-S", SortOrder(format),
            "--merge-output-format", extension,
            "-o", Path.Combine(workDir, "%(title).100B.%(ext)s"),
            // "--" garante que a URL nunca seja interpretada como uma opção do yt-dlp.
            "--", url.AbsoluteUri
        ];
    }

    /// <summary>
    /// Ordem de preferência de formatos do yt-dlp. Para MP4 prefere H.264 + AAC, que tocam em qualquer
    /// player (QuickTime, iPhone, editores); sem isso o yt-dlp costuma escolher VP9, que fica dentro de
    /// um .mp4 mas não abre em vários deles. Se o vídeo só existir em outro codec, ele usa esse mesmo.
    /// </summary>
    private static string SortOrder(VideoFormat format) =>
        format == VideoFormat.Mp4 ? "vcodec:h264,res,acodec:m4a" : "ext:webm:webm";

    private static async Task<(bool Success, string Error)> RunYtDlpAsync(
        IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo("yt-dlp")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            return (false, "yt-dlp não está instalado no servidor.");
        }

        // As duas saídas precisam ser lidas, senão o processo pode travar com o buffer cheio.
        var stdout = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        var stderr = process.StandardError.ReadToEndAsync(CancellationToken.None);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        await Task.WhenAll(stdout, stderr);
        return process.ExitCode == 0 ? (true, "") : (false, ToFriendlyError(stderr.Result));
    }

    /// <summary>Pega a última linha "ERROR: ..." do yt-dlp, sem expor caminhos do servidor.</summary>
    private static string ToFriendlyError(string stderr)
    {
        var line = stderr.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault(l => l.StartsWith("ERROR:"));

        return line is null
            ? "Não foi possível baixar o vídeo."
            : $"Não foi possível baixar o vídeo: {line["ERROR:".Length..].Trim()}";
    }

    private static string ContentTypeOf(VideoFormat format) =>
        format == VideoFormat.Mp4 ? "video/mp4" : "video/webm";
}
