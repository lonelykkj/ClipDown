namespace ClipDown.Application.Common.Models;

public sealed record ConvertedFile(byte[] Content, string ContentType, string Extension);
