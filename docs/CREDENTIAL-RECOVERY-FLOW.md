# Credential Recovery Flow

**Feature**: 002-environment-and-configuration
**Date**: 2025-10-06
**Status**: Design Complete

## Overview

This document describes the user experience and technical implementation for credential recovery when OS-native storage (Windows DPAPI or Android KeyStore) encounters errors or corruption. The flow prioritizes user control, clarity, and security while maintaining constitutional compliance for graceful degradation.

---

## User Scenarios

### Scenario 1: Corrupted Windows Credential Manager Entry

**Context**: User's Visual ERP credentials were corrupted due to Windows profile migration or registry corruption.

**User Experience**:
1. User launches MTM application
2. Application attempts to retrieve Visual ERP credentials via `WindowsSecretsService.RetrieveSecretAsync()`
3. DPAPI returns error or corrupted data
4. **Modal dialog appears** (blocking all other interaction):

```
┌──────────────────────────────────────────────────────────────┐
│  ⚠️  Credential Recovery Required                            │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  We couldn't access your saved Visual ERP credentials.      │
│  This can happen after Windows updates or profile changes.  │
│                                                              │
│  Please re-enter your credentials to continue.              │
│                                                              │
│  Username: [________________]                                │
│  Password: [****************]                                │
│                                                              │
│  ☐ Save these credentials securely                          │
│                                                              │
│  [    Cancel    ]             [    Continue    ]            │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

5. **User choices**:
   - **Continue**: Validates credentials, re-saves to Windows Credential Manager via `StoreSecretAsync()`, application proceeds
   - **Cancel**: Shows confirmation dialog: *"Credentials are required to use Visual ERP features. Exit application?"*
     - **Confirm exit**: Application closes with graceful shutdown (logs reason)
     - **Go back**: Returns to credential prompt

---

### Scenario 2: Android KeyStore Unavailable

**Context**: User's device lacks hardware-backed encryption or KeyStore API initialization failed.

**User Experience**:
1. User launches MTM Android app
2. Application attempts to retrieve credentials via `AndroidSecretsService.RetrieveSecretAsync()`
3. KeyStore throws `KeyStoreException` or initialization failure
4. **Modal dialog appears**:

```
┌──────────────────────────────────────────────────────────────┐
│  🔒  Security Configuration Required                         │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Your device's secure storage is unavailable. This may be   │
│  because:                                                    │
│  • Device lock screen is disabled (Settings → Security)     │
│  • Android KeyStore initialization failed                   │
│                                                              │
│  To use MTM on this device, you need to:                    │
│  1. Enable screen lock (PIN, Pattern, or Biometric)         │
│  2. Restart the app                                         │
│                                                              │
│  [    Exit App    ]        [    Open Settings    ]          │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

5. **User choices**:
   - **Open Settings**: Launches Android device security settings, user returns to app after setup
   - **Exit App**: Application closes with graceful shutdown

---

### Scenario 3: First-Time Credential Entry (Normal Flow)

**Context**: User launches application for the first time (no saved credentials).

**User Experience**:
1. Application checks for saved credentials via `RetrieveSecretAsync()`
2. Returns `null` (no credentials found)
3. **Credential prompt appears** (same UI as recovery dialog, but without warning text):

```
┌──────────────────────────────────────────────────────────────┐
│  🔐  Visual ERP Login                                        │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Enter your Visual ERP credentials to continue.             │
│                                                              │
│  Username: [________________]                                │
│  Password: [****************]                                │
│                                                              │
│  ☑ Save these credentials securely                          │
│                                                              │
│  [    Cancel    ]             [    Continue    ]            │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

4. User enters credentials, clicks **Continue**
5. Application validates credentials (optional: test Visual API connection)
6. If "Save credentials" checked: Calls `StoreSecretAsync()` to persist
7. Application proceeds to main screen

---

## Technical Implementation

### Error Detection

**Triggering Conditions**:
1. `RetrieveSecretAsync()` throws `InvalidOperationException` (storage unavailable)
2. `RetrieveSecretAsync()` returns `null` but application expects credentials (first run)
3. DPAPI/KeyStore returns corrupted data (decryption fails)
4. Permission denied errors (Windows user changed, Android app reinstalled)

**Error Categorization** (from `ErrorCategory` enum):
- **Critical**: Storage completely unavailable (KeyStore missing, DPAPI broken)
- **Warning**: Credentials not found (first run) or corrupted (recoverable)
- **Info**: Credential saved successfully after recovery

### Recovery Dialog Component

**Location**: `MTM_Template_Application/Views/Dialogs/CredentialRecoveryDialog.axaml`

**ViewModel**: `MTM_Template_Application/ViewModels/CredentialRecoveryViewModel.cs`

```csharp
public partial class CredentialRecoveryViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private bool _saveCredentials = true;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _dialogTitle = "Credential Recovery Required";

    [ObservableProperty]
    private string _instructionText = "We couldn't access your saved credentials.";

    private readonly ISecretsService _secretsService;
    private readonly ILogger<CredentialRecoveryViewModel> _logger;

    public CredentialRecoveryViewModel(
        ISecretsService secretsService,
        ILogger<CredentialRecoveryViewModel> logger)
    {
        _secretsService = secretsService;
        _logger = logger;
    }

    [RelayCommand(CanExecute = nameof(CanContinue))]
    private async Task ContinueAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Validate credentials (optional: test Visual API)
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password are required.";
                return;
            }

            // Save credentials if requested
            if (SaveCredentials)
            {
                await _secretsService.StoreSecretAsync("Visual.Username", Username, cancellationToken);
                await _secretsService.StoreSecretAsync("Visual.Password", Password, cancellationToken);
                _logger.LogInformation("Credentials re-saved successfully after recovery");
            }

            // Close dialog with success result
            CloseRequested?.Invoke(this, new DialogClosedEventArgs { Success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save recovered credentials");
            ErrorMessage = "Failed to save credentials. Please try again.";
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        // Confirm exit
        var confirmExit = await ShowConfirmationDialog(
            "Exit Application?",
            "Credentials are required to use Visual ERP features. Exit application?"
        );

        if (confirmExit)
        {
            _logger.LogWarning("User cancelled credential recovery - application will exit");
            CloseRequested?.Invoke(this, new DialogClosedEventArgs { Success = false, ExitApp = true });
        }
    }

    private bool CanContinue() => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

    public event EventHandler<DialogClosedEventArgs>? CloseRequested;
}

public class DialogClosedEventArgs : EventArgs
{
    public bool Success { get; init; }
    public bool ExitApp { get; init; }
}
```

### Recovery Flow Integration

**Location**: `MTM_Template_Application/Services/Secrets/SecretsServiceFactory.cs`

```csharp
public static async Task<string?> RetrieveSecretWithRecoveryAsync(
    ISecretsService secretsService,
    string key,
    Func<Task<(string? username, string? password, bool save)>> promptForCredentials,
    ILogger logger,
    CancellationToken cancellationToken = default)
{
    try
    {
        // Attempt normal retrieval
        var secret = await secretsService.RetrieveSecretAsync(key, cancellationToken);
        if (!string.IsNullOrEmpty(secret))
        {
            return secret;
        }

        // Credential not found - prompt user
        logger.LogWarning("Credential '{Key}' not found - prompting user for recovery", key);
        var (username, password, save) = await promptForCredentials();

        if (username == null || password == null)
        {
            logger.LogWarning("User cancelled credential recovery");
            return null;
        }

        // Save if requested
        if (save)
        {
            await secretsService.StoreSecretAsync(key, password, cancellationToken);
            logger.LogInformation("Credential '{Key}' re-saved after recovery", key);
        }

        return password;
    }
    catch (InvalidOperationException ex)
    {
        // Storage unavailable - critical error
        logger.LogError(ex, "OS-native credential storage unavailable for key '{Key}'", key);
        throw new InvalidOperationException(
            "Secure credential storage is unavailable. Please check device security settings.",
            ex
        );
    }
}
```

---

## UI/UX Guidelines

### Dialog Design Principles

1. **Clarity Over Brevity**: Explain WHY credentials are needed (corruption, migration, etc.)
2. **Action-Oriented**: Clear "Continue" vs "Cancel" buttons (no ambiguous "OK")
3. **Non-Technical Language**: Avoid "DPAPI failed" - use "couldn't access saved credentials"
4. **Progressive Disclosure**: Show technical details only if user clicks "More Info" link
5. **Security Indicators**: Lock icon, "Save securely" checkbox with tooltip explaining encryption

### Visual Design

**Colors** (from Theme V2 semantic tokens):
- **Critical Error**: `{DynamicResource ThemeV2.Status.Error.Background}` with warning icon ⚠️
- **Security Context**: `{DynamicResource ThemeV2.Status.Info.Background}` with lock icon 🔒
- **Success Recovery**: `{DynamicResource ThemeV2.Status.Success.Background}` with checkmark ✅

**Spacing**:
- Dialog width: 500px (desktop), 90% screen width (Android)
- Padding: 24px all sides
- Button spacing: 16px between buttons
- Input field margin: 12px vertical

**Typography**:
- Title: 18pt bold (Segoe UI/Roboto)
- Body text: 14pt regular
- Error message: 12pt italic, error color
- Input labels: 12pt bold

### Accessibility

- **Keyboard Navigation**: Tab order: Username → Password → Save checkbox → Continue → Cancel
- **Screen Readers**: ARIA labels on all inputs, clear button descriptions
- **High Contrast Mode**: Respects OS high contrast settings (Theme V2 compatibility)
- **Focus Indicators**: Visible focus rings on all interactive elements

---

## Security Considerations

### Credential Handling

1. **No Plaintext Storage**: Temporary credentials in memory only, cleared after save
2. **Secure Input**: Password fields use `PasswordBox` control (masked input, no clipboard history)
3. **Validation**: Username/password validated before save attempt (non-empty, length checks)
4. **Logging**: Credentials NEVER logged (use `[Redacted]` in structured logs)

### Recovery Attack Surface

**Threats**:
- **Social Engineering**: User tricked into entering credentials in fake dialog
- **Credential Stuffing**: Attacker tries multiple credential combinations

**Mitigations**:
- **Application Signature Validation**: Dialog only shown by signed MTM executable
- **Rate Limiting**: Max 3 failed recovery attempts → temporary lock (5 minutes)
- **Audit Logging**: All recovery attempts logged with timestamp, outcome, device ID
- **Two-Factor Auth** (Android): Device certificate validation in addition to credentials

---

## Testing Scenarios

### Manual Testing Checklist

- [ ] **Windows**: Corrupt credential in Credential Manager → verify recovery dialog appears
- [ ] **Windows**: Cancel recovery dialog → verify exit confirmation → app closes
- [ ] **Android**: Disable device lock screen → verify "Security Required" dialog
- [ ] **Android**: Enable lock screen → verify credentials save successfully
- [ ] **First Run**: No saved credentials → verify normal login dialog (no warning text)
- [ ] **Theme Switching**: Verify dialog respects light/dark theme
- [ ] **Screen Reader**: Verify all elements announce correctly with NVDA/TalkBack
- [ ] **Keyboard Navigation**: Verify tab order, Enter/Escape keys work

### Automated Tests

**Location**: `tests/integration/CredentialRecoveryTests.cs`

```csharp
[Fact]
public async Task CredentialRecovery_CorruptedStorage_PromptsUserAndResaves()
{
    // Arrange
    var mockSecretsService = Substitute.For<ISecretsService>();
    mockSecretsService.RetrieveSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
        .ThrowsAsync(new InvalidOperationException("Storage corrupted"));

    // Act
    var recovered = await SecretsServiceFactory.RetrieveSecretWithRecoveryAsync(
        mockSecretsService,
        "Visual.Password",
        async () => ("testuser", "testpass", true),
        Substitute.For<ILogger>()
    );

    // Assert
    recovered.Should().Be("testpass");
    await mockSecretsService.Received(1).StoreSecretAsync("Visual.Password", "testpass", Arg.Any<CancellationToken>());
}

[Fact]
public async Task CredentialRecovery_UserCancels_ReturnsNull()
{
    // Arrange
    var mockSecretsService = Substitute.For<ISecretsService>();
    mockSecretsService.RetrieveSecretAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
        .ReturnsAsync((string?)null);

    // Act
    var recovered = await SecretsServiceFactory.RetrieveSecretWithRecoveryAsync(
        mockSecretsService,
        "Visual.Password",
        async () => (null, null, false), // User cancelled
        Substitute.For<ILogger>()
    );

    // Assert
    recovered.Should().BeNull();
}
```

---

## Performance Targets

- **Dialog Display Time**: <200ms from error detection to visible dialog
- **Credential Validation**: <500ms (network call to Visual API if enabled)
- **Save Operation**: <300ms (`StoreSecretAsync()` to OS-native storage)
- **Dialog Responsiveness**: <100ms button click to action start

---

## Related Documentation

- **Feature Specification**: `specs/002-environment-and-configuration/spec.md` (FR-013, NFR-012)
- **Secrets Service Contract**: `specs/002-environment-and-configuration/contracts/README.md` (ISecretsService)
- **Implementation Plan**: `specs/002-environment-and-configuration/plan.md`
- **Theme Guidelines**: `.github/instructions/Themes.instructions.md` (Theme V2 tokens)
- **Platform Security**: Windows DPAPI docs, Android KeyStore API docs

---

**Last Updated**: 2025-10-06
**Status**: ✅ Design Complete
**Implementation**: Ready for Phase 3 UI development
