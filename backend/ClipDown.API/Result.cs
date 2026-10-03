using System.Diagnostics.CodeAnalysis;

namespace ClipDown.API;

/// <summary>Arquivo pronto para ser devolvido ao usuário como download.</summary>
public sealed record OutputFile(byte[] Content, string ContentType, string FileName);

/// <summary>Resultado de uma operação: ou um valor (sucesso) ou uma mensagem de erro (falha).</summary>
public sealed class Result<T>
{
    private Result(T? value, string? error)
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; }
    public string? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(string error) => new(default, error);
}
