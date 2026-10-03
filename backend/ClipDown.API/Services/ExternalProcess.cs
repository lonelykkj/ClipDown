using System.ComponentModel;
using System.Diagnostics;

namespace ClipDown.API.Services;

/// <summary>Executa programas externos (yt-dlp, ffmpeg) sem passar por um shell.</summary>
public static class ExternalProcess
{
    /// <summary>
    /// Roda o programa e espera ele terminar.
    /// Devolve <c>null</c> se deu certo, ou a mensagem de erro para o usuário.
    /// </summary>
    /// <param name="describeError">Transforma a saída de erro do programa numa mensagem amigável.</param>
    public static async Task<string?> RunAsync(
        string program,
        IEnumerable<string> arguments,
        TimeSpan timeout,
        Func<string, string> describeError,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(program)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true
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
            return $"{program} não está instalado no servidor.";
        }

        // As duas saídas precisam ser lidas, senão o processo trava quando o buffer enche.
        var stdout = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        var stderr = process.StandardError.ReadToEndAsync(CancellationToken.None);

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);

            // Cancelado pelo cliente (fechou a página): não há a quem responder.
            cancellationToken.ThrowIfCancellationRequested();
            return "A operação demorou demais e foi cancelada.";
        }

        await Task.WhenAll(stdout, stderr);
        return process.ExitCode == 0 ? null : describeError(stderr.Result);
    }
}
