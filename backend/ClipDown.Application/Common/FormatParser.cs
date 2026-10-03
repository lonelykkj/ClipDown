namespace ClipDown.Application.Common;

public static class FormatParser
{
    /// <summary>
    /// Converte texto no valor do enum comparando com os nomes (sem diferenciar maiúsculas).
    /// Usa isso em vez de Enum.TryParse, que também aceitaria números ("1") e listas ("Png,Jpg").
    /// </summary>
    public static bool TryParse<TEnum>(string? value, out TEnum result) where TEnum : struct, Enum
    {
        foreach (var candidate in Enum.GetValues<TEnum>())
        {
            if (string.Equals(value?.Trim(), candidate.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                result = candidate;
                return true;
            }
        }

        result = default;
        return false;
    }
}
