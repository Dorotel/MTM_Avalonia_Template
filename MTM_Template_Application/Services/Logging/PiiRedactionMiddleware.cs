using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace MTM_Template_Application.Services.Logging;

/// <summary>
/// Middleware to detect and redact sensitive data (PII) from log messages
/// </summary>
public class PiiRedactionMiddleware
{
    // Regex patterns for detecting sensitive data
    private static readonly Regex SsnPattern = new(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled);
    private static readonly Regex CreditCardPattern = new(@"\b(?:\d{4}[-\s]?){3}\d{4}\b", RegexOptions.Compiled);
    private static readonly Regex PasswordPattern = new(@"(?i)(password|pwd|pass|secret|token|apikey|api_key|authorization)[""']?\s*[:=]\s*[""']?([^\s""']+)", RegexOptions.Compiled);
    private static readonly Regex EmailPattern = new(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled);
    private static readonly Regex PhonePattern = new(@"\b(?:\+?1[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}\b", RegexOptions.Compiled);
    private static readonly Regex IpAddressPattern = new(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
    private static readonly Regex Base64TokenPattern = new(@"\b[A-Za-z0-9+/]{20,}={0,2}\b", RegexOptions.Compiled);

    // Keywords that indicate sensitive data
    private static readonly string[] SensitiveKeywords =
    {
        "password", "pwd", "pass", "secret", "token", "apikey", "api_key",
        "authorization", "bearer", "credential", "auth", "key", "private"
    };

    /// <summary>
    /// Redact sensitive data from a message
    /// </summary>
    /// <param name="message">The message to redact</param>
    /// <returns>The redacted message</returns>
    public virtual string Redact(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return message;
        }

        string redacted = message;

        // Redact SSN
        redacted = SsnPattern.Replace(redacted, "***-**-****");

        // Redact credit card numbers
        redacted = CreditCardPattern.Replace(redacted, match =>
        {
            var digits = Regex.Replace(match.Value, @"[-\s]", "");
            return $"****-****-****-{digits.Substring(Math.Max(0, digits.Length - 4))}";
        });

        // Redact passwords and secrets (key-value pairs)
        redacted = PasswordPattern.Replace(redacted, match =>
        {
            return $"{match.Groups[1].Value}=***REDACTED***";
        });

        // Redact email addresses (partial - keep domain)
        redacted = EmailPattern.Replace(redacted, match =>
        {
            var email = match.Value;
            var atIndex = email.IndexOf('@');
            if (atIndex > 0)
            {
                var domain = email.Substring(atIndex);
                return $"***{domain}";
            }
            return "***@***";
        });

        // Redact phone numbers
        redacted = PhonePattern.Replace(redacted, "***-***-****");

        // Redact long base64 tokens (likely API keys/tokens)
        redacted = Base64TokenPattern.Replace(redacted, match =>
        {
            if (match.Value.Length >= 20)
            {
                return $"{match.Value.Substring(0, 4)}...***REDACTED***";
            }
            return match.Value;
        });

        // Check for sensitive keywords in key-value context
        redacted = RedactSensitiveKeywords(redacted);

        return redacted;
    }

    /// <summary>
    /// Redact values associated with sensitive keywords
    /// </summary>
    private string RedactSensitiveKeywords(string message)
    {
        foreach (var keyword in SensitiveKeywords)
        {
            // Pattern: keyword: value, keyword=value, keyword "value", etc.
            var pattern = $@"(?i){keyword}\s*[:=]\s*[""']?([^\s,""'}}]+)";
            message = Regex.Replace(message, pattern, match =>
            {
                return $"{match.Value.Substring(0, match.Value.IndexOf(match.Groups[1].Value))}***REDACTED***";
            });
        }

        return message;
    }

    /// <summary>
    /// Check if a message contains potential PII
    /// </summary>
    /// <param name="message">The message to check</param>
    /// <returns>True if PII is detected</returns>
    public bool ContainsPii(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        return SsnPattern.IsMatch(message) ||
               CreditCardPattern.IsMatch(message) ||
               PasswordPattern.IsMatch(message) ||
               Base64TokenPattern.IsMatch(message);
    }
}
