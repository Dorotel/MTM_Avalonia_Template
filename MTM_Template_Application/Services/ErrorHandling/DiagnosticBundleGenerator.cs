using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Models.ErrorHandling;

namespace MTM_Template_Application.Services.ErrorHandling;

/// <summary>
/// Generates diagnostic bundles for error reports
/// </summary>
public class DiagnosticBundleGenerator
{
    private readonly ILogger<DiagnosticBundleGenerator> _logger;

    public DiagnosticBundleGenerator(ILogger<DiagnosticBundleGenerator> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Generate a diagnostic bundle for an error
    /// </summary>
    public async Task<string> GenerateAsync(Exception exception, ErrorReport errorReport)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(errorReport);

        try
        {
            var diagnosticData = new
            {
                ErrorReport = errorReport,
                Exception = new
                {
                    exception.Message,
                    exception.StackTrace,
                    Type = exception.GetType().FullName,
                    InnerException = exception.InnerException?.Message
                },
                Environment = new
                {
                    OSVersion = RuntimeInformation.OSDescription,
                    Architecture = RuntimeInformation.OSArchitecture.ToString(),
                    FrameworkDescription = RuntimeInformation.FrameworkDescription,
                    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName,
                    CurrentDirectory = Environment.CurrentDirectory,
                    ProcessorCount = Environment.ProcessorCount,
                    SystemPageSize = Environment.SystemPageSize,
                    WorkingSet = Environment.WorkingSet,
                    TickCount = Environment.TickCount64
                },
                Timestamp = DateTimeOffset.UtcNow
            };

            var json = JsonSerializer.Serialize(diagnosticData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Compress the JSON to save space
            var compressed = await CompressAsync(json);
            var base64 = Convert.ToBase64String(compressed);

            _logger.LogDebug(
                "Generated diagnostic bundle: {Size} bytes (compressed from {OriginalSize} bytes)",
                compressed.Length,
                Encoding.UTF8.GetByteCount(json)
            );

            return base64;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate diagnostic bundle");
            return string.Empty;
        }
    }

    /// <summary>
    /// Compress data using GZip
    /// </summary>
    private static async Task<byte[]> CompressAsync(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);

        using var outputStream = new MemoryStream();
        using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
        {
            await gzipStream.WriteAsync(bytes);
        }

        return outputStream.ToArray();
    }

    /// <summary>
    /// Decompress a diagnostic bundle
    /// </summary>
    public static async Task<string> DecompressBundleAsync(string base64Bundle)
    {
        ArgumentNullException.ThrowIfNull(base64Bundle);

        var compressed = Convert.FromBase64String(base64Bundle);

        using var inputStream = new MemoryStream(compressed);
        using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        using var outputStream = new MemoryStream();

        await gzipStream.CopyToAsync(outputStream);
        var decompressed = outputStream.ToArray();

        return Encoding.UTF8.GetString(decompressed);
    }
}
