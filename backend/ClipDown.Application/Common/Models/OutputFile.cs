namespace ClipDown.Application.Common.Models;

/// <summary>Arquivo pronto para ser devolvido ao usuário como download.</summary>
public sealed record OutputFile(byte[] Content, string ContentType, string FileName);
