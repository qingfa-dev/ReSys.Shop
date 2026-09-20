using System.Text;

namespace Shared.Governance.Conventions;

/// <summary>
/// Provides extension methods for Base64 and Base64 URL-safe encoding and decoding.
/// </summary>
public static class Base64Converter
{
    /// <summary>
    /// Encodes the specified string into a Base64 string using UTF-8 encoding.
    /// </summary>
    /// <param name="input">The plain text string to encode.</param>
    /// <returns>A Base64-encoded representation of the input string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToBase64(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Decodes a Base64-encoded string into its original UTF-8 string representation.
    /// </summary>
    /// <param name="input">The Base64-encoded string.</param>
    /// <returns>The decoded plain text string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when the input is not a valid Base64 string.</exception>
    public static string FromBase64(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Attempts to decode a Base64-encoded string into its original UTF-8 string representation.
    /// </summary>
    /// <param name="input">The Base64-encoded string.</param>
    /// <param name="decoded">
    /// When this method returns, contains the decoded string if successful; otherwise, an empty string.
    /// </param>
    /// <returns>
    /// <c>true</c> if decoding succeeded; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryFromBase64(this string input, out string decoded)
    {
        decoded = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        try
        {
            decoded = input.FromBase64();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Encodes the specified string into a URL-safe Base64 string (Base64Url) using UTF-8 encoding.
    /// </summary>
    /// <param name="input">The plain text string to encode.</param>
    /// <returns>
    /// A URL-safe Base64-encoded string where '+' is replaced with '-', '/' with '_',
    /// and padding '=' characters are removed.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToBase64Url(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    /// <summary>
    /// Decodes a URL-safe Base64 (Base64Url) string into its original UTF-8 string representation.
    /// </summary>
    /// <param name="input">The Base64Url-encoded string.</param>
    /// <returns>The decoded plain text string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when the input is not a valid Base64Url string.</exception>
    public static string FromBase64Url(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var padded = input
            .Replace("-", "+")
            .Replace("_", "/");

        // Restore padding
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        var bytes = Convert.FromBase64String(padded);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Attempts to decode a URL-safe Base64 (Base64Url) string into its original UTF-8 string representation.
    /// </summary>
    /// <param name="input">The Base64Url-encoded string.</param>
    /// <param name="decoded">
    /// When this method returns, contains the decoded string if successful; otherwise, an empty string.
    /// </param>
    /// <returns>
    /// <c>true</c> if decoding succeeded; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryFromBase64Url(this string input, out string decoded)
    {
        decoded = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        try
        {
            decoded = input.FromBase64Url();
            return true;
        }
        catch
        {
            return false;
        }
    }
}