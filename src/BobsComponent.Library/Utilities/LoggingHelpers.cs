namespace BobsComponent.Library.Utilities;

/// <summary>
/// Utility class for safe logging that prevents sensitive data exposure
/// </summary>
public static class LoggingHelpers
{
    private static readonly HashSet<string> SensitivePatterns = new()
    {
        "password", "token", "secret", "api-key", "apikey", "auth", "bearer",
        "authorization", "credential", "private", "key"
    };

    /// <summary>
    /// Sanitizes a message to prevent logging sensitive information
    /// </summary>
    /// <param name="message">The message to sanitize</param>
    /// <returns>Sanitized message safe for logging</returns>
    public static string SanitizeMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return string.Empty;

        // Check for sensitive patterns
        foreach (var pattern in SensitivePatterns)
        {
            if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return "[REDACTED - Potentially sensitive information]";
            }
        }

        // Limit message length
        if (message.Length > 500)
        {
            return message[..500] + "... [truncated]";
        }

        return message;
    }

    /// <summary>
    /// Sanitizes a dictionary of properties to prevent logging sensitive data
    /// </summary>
    /// <param name="properties">Properties to sanitize</param>
    /// <returns>Sanitized properties safe for logging</returns>
    public static Dictionary<string, object> SanitizeProperties(Dictionary<string, object> properties)
    {
        var sanitized = new Dictionary<string, object>();

        foreach (var kvp in properties)
        {
            // Check if key contains sensitive pattern
            if (SensitivePatterns.Any(p => kvp.Key.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                sanitized[kvp.Key] = "[REDACTED]";
            }
            else if (kvp.Value is string strValue)
            {
                // Sanitize string values
                sanitized[kvp.Key] = SanitizeMessage(strValue);
            }
            else
            {
                sanitized[kvp.Key] = kvp.Value;
            }
        }

        return sanitized;
    }

    /// <summary>
    /// Sanitizes user input for logging (removes newlines, limits length)
    /// </summary>
    /// <param name="userInput">User input to sanitize</param>
    /// <returns>Sanitized input safe for logging</returns>
    public static string SanitizeUserInput(string userInput)
    {
        if (string.IsNullOrEmpty(userInput))
            return string.Empty;

        // Remove newlines to prevent log injection attacks
        var sanitized = userInput.Replace("\n", " ").Replace("\r", " ");

        // Limit length
        if (sanitized.Length > 100)
        {
            sanitized = sanitized[..100] + "...";
        }

        return sanitized;
    }
}
