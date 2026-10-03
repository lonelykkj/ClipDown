using System.ComponentModel;
using System.Diagnostics;

namespace ClipDown.Infrastructure.Services;

internal sealed record ProcessOutput(int ExitCode, string StandardError)
{
    public bool Succeeded => ExitCode == 0;
}

/// <summary>Executa programas externos (yt-dlp, ffmpeg) de forma segura, sem passar por um shell.</summary>
internal static class ExternalProcess
{
    /// <returns>A saída do processo, ou <c>null</c> se o programa não está instalado.</returns>
    public static async Task<ProcessOutput?> RunAsync(
        string program, IEnumerable<string> arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(program)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        // Cada argumento é passado separado: nada é interpretado por um shell.
        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            return null;
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
        return new ProcessOutput(process.ExitCode, stderr.Result);
    }
}
