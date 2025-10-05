using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MTM_Template_Application.Services.Secrets;

namespace MTM_Template_Application.ViewModels.Configuration;

/// <summary>
/// ViewModel for credential recovery dialog
/// Shown when OS-native credential storage fails or credentials are corrupted
/// </summary>
public partial class CredentialDialogViewModel : ObservableObject
{
    private readonly ISecretsService _secretsService;
    private readonly ILogger<CredentialDialogViewModel> _logger;
    private bool _dialogResult;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private bool _isLoading;

    [ObservableProperty]
    private string _dialogTitle = "Enter Credentials";

    [ObservableProperty]
    private string _dialogMessage = "Your saved credentials could not be retrieved. Please enter them again.";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RetryCommand))]
    [NotifyPropertyChangedFor(nameof(ShowRetryButton))]
    [NotifyPropertyChangedFor(nameof(ShowRetryAttempts))]
    private int _retryAttempts;

    /// <summary>
    /// Maximum retry attempts before manual retry required (NFR-017)
    /// </summary>
    private const int MaxAutoRetryAttempts = 3;

    /// <summary>
    /// Show retry button when retry attempts >= 3
    /// </summary>
    public bool ShowRetryButton => RetryAttempts >= MaxAutoRetryAttempts;

    /// <summary>
    /// Show retry attempts info when retry attempts > 0
    /// </summary>
    public bool ShowRetryAttempts => RetryAttempts > 0;

    /// <summary>
    /// Show error message when not empty
    /// </summary>
    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public CredentialDialogViewModel(
        ISecretsService secretsService,
        ILogger<CredentialDialogViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(secretsService);
        ArgumentNullException.ThrowIfNull(logger);

        _secretsService = secretsService;
        _logger = logger;
    }

    /// <summary>
    /// Submit credentials for re-storage
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            // Validate inputs
            if (!ValidateCredentials())
            {
                return;
            }

            _logger.LogInformation("Attempting to re-store credentials");

            // Re-attempt storing credentials with exponential backoff
            await RetryWithBackoffAsync(async () =>
            {
                await _secretsService.StoreSecretAsync("Visual.Username", Username, cancellationToken);
                await _secretsService.StoreSecretAsync("Visual.Password", Password, cancellationToken);
            }, cancellationToken);

            _logger.LogInformation("Credentials successfully re-stored");

            // Close dialog with success
            _dialogResult = true;
            CloseDialog();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Credential storage cancelled by user");
            ErrorMessage = "Operation was cancelled.";
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Insufficient permissions to store credentials");
            ErrorMessage = "Insufficient permissions. Please check security settings.";
            RetryAttempts++;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store credentials");
            ErrorMessage = "Could not save credentials. Please try again.";
            RetryAttempts++;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Manual retry trigger after max auto-retry attempts (NFR-017)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRetry))]
    private async Task RetryAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Manual retry triggered by user after {RetryAttempts} failed attempts", RetryAttempts);

        // Reset retry counter and attempt submission again
        RetryAttempts = 0;
        ErrorMessage = string.Empty;

        await SubmitAsync(cancellationToken);
    }

    private bool CanRetry() => RetryAttempts >= MaxAutoRetryAttempts && !IsLoading;

    /// <summary>
    /// Cancel dialog without saving
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInformation("Credential dialog cancelled by user");
        _dialogResult = false;
        CloseDialog();
    }

    private bool CanSubmit() =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !IsLoading;

    private bool ValidateCredentials()
    {
        // Username validation
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username is required.";
            return false;
        }

        if (Username.Length < 3)
        {
            ErrorMessage = "Username must be at least 3 characters.";
            return false;
        }

        if (Username.Length > 100)
        {
            ErrorMessage = "Username must not exceed 100 characters.";
            return false;
        }

        // Password validation
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Password is required.";
            return false;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters (Visual ERP requirement).";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Retry with exponential backoff (5s, 15s, 30s) - NFR-017
    /// </summary>
    private async Task RetryWithBackoffAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        int[] backoffDelays = { 5000, 15000, 30000 }; // 5s, 15s, 30s

        for (int attempt = 0; attempt <= MaxAutoRetryAttempts; attempt++)
        {
            try
            {
                await action();
                return; // Success
            }
            catch (Exception ex) when (attempt < MaxAutoRetryAttempts)
            {
                var delay = backoffDelays[Math.Min(attempt, backoffDelays.Length - 1)];
                _logger.LogWarning(ex,
                    "Credential storage attempt {Attempt} failed. Retrying in {Delay}ms",
                    attempt + 1, delay);

                await Task.Delay(delay, cancellationToken);
            }
        }

        // All retries exhausted
        throw new InvalidOperationException(
            $"Failed to store credentials after {MaxAutoRetryAttempts} attempts. Manual retry required.");
    }

    private void CloseDialog()
    {
        // Dialog close logic handled by view
        // View should subscribe to property changes or use commands
    }

    public bool GetDialogResult() => _dialogResult;
}
