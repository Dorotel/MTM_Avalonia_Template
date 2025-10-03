using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Diagnostics;

namespace MTM_Template_Application.Services.Diagnostics.Checks;

/// <summary>
/// Verify network connectivity (5s timeout)
/// </summary>
public class NetworkDiagnostic : IDiagnosticCheck
{
    private const int TimeoutSeconds = 5;
    private readonly HttpClient _httpClient;

    public NetworkDiagnostic()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
        };
    }

    public async Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var details = new Dictionary<string, object>();

        try
        {
            // Check 1: Network interface availability
            var hasNetworkInterface = CheckNetworkInterfaces(details);

            if (!hasNetworkInterface)
            {
                stopwatch.Stop();
                return new DiagnosticResult
                {
                    CheckName = nameof(NetworkDiagnostic),
                    Status = DiagnosticStatus.Failed,
                    Message = "No active network interfaces found",
                    Details = details,
                    Timestamp = DateTimeOffset.UtcNow,
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }

            // Check 2: Internet connectivity test
            var hasInternet = await CheckInternetConnectivityAsync(details);

            stopwatch.Stop();

            if (hasInternet)
            {
                return new DiagnosticResult
                {
                    CheckName = nameof(NetworkDiagnostic),
                    Status = DiagnosticStatus.Passed,
                    Message = "Network connectivity verified",
                    Details = details,
                    Timestamp = DateTimeOffset.UtcNow,
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }
            else
            {
                return new DiagnosticResult
                {
                    CheckName = nameof(NetworkDiagnostic),
                    Status = DiagnosticStatus.Warning,
                    Message = "Network interfaces available but internet connectivity not verified",
                    Details = details,
                    Timestamp = DateTimeOffset.UtcNow,
                    DurationMs = stopwatch.ElapsedMilliseconds
                };
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            details["Exception"] = ex.ToString();

            return new DiagnosticResult
            {
                CheckName = nameof(NetworkDiagnostic),
                Status = DiagnosticStatus.Failed,
                Message = $"Network check failed: {ex.Message}",
                Details = details,
                Timestamp = DateTimeOffset.UtcNow,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    private bool CheckNetworkInterfaces(Dictionary<string, object> details)
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var activeInterfaces = 0;
            var interfaceDetails = new List<Dictionary<string, object>>();

            foreach (var ni in interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    activeInterfaces++;
                    interfaceDetails.Add(new Dictionary<string, object>
                    {
                        ["Name"] = ni.Name,
                        ["Type"] = ni.NetworkInterfaceType.ToString(),
                        ["Speed"] = ni.Speed,
                        ["Status"] = ni.OperationalStatus.ToString()
                    });
                }
            }

            details["ActiveInterfaces"] = activeInterfaces;
            details["InterfaceDetails"] = interfaceDetails;

            return activeInterfaces > 0;
        }
        catch (Exception ex)
        {
            details["InterfaceCheckError"] = ex.Message;
            return false;
        }
    }

    private async Task<bool> CheckInternetConnectivityAsync(Dictionary<string, object> details)
    {
        var testUrls = new[]
        {
            "https://www.google.com",
            "https://www.microsoft.com",
            "https://cloudflare.com"
        };

        foreach (var url in testUrls)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    details["InternetConnectivity"] = "OK";
                    details["TestedUrl"] = url;
                    details["ResponseTime"] = response.Headers.Date?.ToString() ?? "N/A";
                    return true;
                }
            }
            catch (TaskCanceledException)
            {
                // Timeout - try next URL
                continue;
            }
            catch (HttpRequestException)
            {
                // Connection failed - try next URL
                continue;
            }
        }

        details["InternetConnectivity"] = "Failed to reach test URLs";
        return false;
    }
}
