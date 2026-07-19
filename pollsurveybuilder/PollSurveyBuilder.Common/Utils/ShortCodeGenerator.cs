using System;
using System.Security.Cryptography;
using System.Text;

namespace PollSurveyBuilder.Common.Utils;

public static class ShortCodeGenerator
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Generate(int length = 6)
    {
        var result = new StringBuilder(length);
        var bytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        foreach (var b in bytes)
        {
            result.Append(Chars[b % Chars.Length]);
        }

        return result.ToString();
    }
}
