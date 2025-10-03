# Android Testing Guide - MTM Avalonia Template

**Last Updated**: October 3, 2025
**Project**: MTM_Avalonia_Template
**Target Framework**: .NET 9.0 (`net9.0-android`)
**Minimum Android Version**: API Level 21 (Android 5.0)

---

## Overview

This guide explains how to set up your development environment to test the Android implementation of this Avalonia cross-platform application.

## Required Tools

### 1. .NET Android Workload ✅ (Essential)

Since this project targets `net9.0-android`, you must install the .NET Android workload:

```powershell
# Install .NET Android workload
dotnet workload install android
```

**What this installs:**

- Android SDK
- Android Emulator
- Android Build Tools
- Platform Tools (ADB, etc.)
- Java Development Kit (JDK)

**Verify installation:**

```powershell
dotnet workload list
```

You should see `android` in the installed workloads list.

---

### 2. Testing Options

You have two options for running/testing the Android app:

#### Option A: Android Emulator (Recommended for Development)

**Pros:**

- No physical device needed
- Easy to test multiple Android versions
- Faster iteration during development
- Integrated with Visual Studio/VS Code

**Setup:**

1. The Android Emulator is included with the Android workload
2. Create an emulator device using Android Device Manager:
   - Visual Studio: Tools → Android → Android Device Manager
   - Command Line: `avdmanager` (comes with Android SDK)

**Recommended Emulator Config:**

- Device: Pixel 5 or newer
- System Image: Android API 30+ (Android 11+)
- RAM: 4GB+
- Storage: 8GB+

**Create emulator via command line:**

```powershell
# List available system images
sdkmanager --list

# Download a system image (example: Android 13, x86_64)
sdkmanager "system-images;android-33;google_apis;x86_64"

# Create AVD
avdmanager create avd -n "Pixel5_API33" -k "system-images;android-33;google_apis;x86_64" -d "pixel_5"

# List available emulators
emulator -list-avds

# Start emulator
emulator -avd Pixel5_API33
```

#### Option B: Physical Android Device

**Pros:**

- Real hardware performance
- Accurate touch/sensor behavior
- No emulator overhead

**Setup:**

1. **Enable Developer Options** on your Android device:

   - Go to Settings → About Phone
   - Tap "Build Number" 7 times
   - Developer Options will appear in Settings

2. **Enable USB Debugging**:

   - Go to Settings → Developer Options
   - Enable "USB Debugging"
   - Enable "Install via USB" (if available)

3. **Connect Device**:
   - Connect via USB cable
   - Accept the "Allow USB Debugging" prompt on your device
   - Verify connection: `adb devices`

**Verify device connection:**

```powershell
# Check connected devices
adb devices

# Should show:
# List of devices attached
# <device-id>    device
```

---

### 3. Visual Studio 2022 (Optional but Recommended)

**Benefits:**

- Best Android debugging experience
- Integrated Android Device Manager
- Visual emulator controls
- Performance profiling tools

**Setup:**

1. Install Visual Studio 2022 (Community/Professional/Enterprise)
2. During installation, select:
   - **Mobile development with .NET** workload
   - This includes Avalonia support via extensions

**Avalonia Extension for VS 2022:**

- Extension: **Avalonia for Visual Studio 2022**
- Install from: Extensions → Manage Extensions → Search "Avalonia"
- Or download from: <https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaVS>

---

### 4. Android Studio (Optional - Advanced Emulator Management)

**Use Case:** If you need advanced emulator features or prefer Android Studio's AVD Manager UI.

**Download:** <https://developer.android.com/studio>

**Features:**

- Advanced AVD (Android Virtual Device) Manager
- Hardware profile customization
- Snapshot management
- Better GPU/sensor emulation controls

**Note:** You don't need the full Android Studio IDE—just the SDK/tools are sufficient for basic development.

---

## Building & Running the Android App

### Build the Android Project

```powershell
# Navigate to project root
cd C:\Users\jkoll\source\repos\MTM_Avalonia_Template

# Build Android project
dotnet build MTM_Template_Application.Android/MTM_Template_Application.Android.csproj -f net9.0-android

# Build in Release mode
dotnet build MTM_Template_Application.Android/MTM_Template_Application.Android.csproj -f net9.0-android -c Release
```

### Run on Emulator

```powershell
# Start emulator first (if not already running)
emulator -avd Pixel5_API33

# Deploy and run
dotnet run --project MTM_Template_Application.Android/MTM_Template_Application.Android.csproj -f net9.0-android
```

### Run on Physical Device

```powershell
# Ensure device is connected
adb devices

# Deploy and run
dotnet run --project MTM_Template_Application.Android/MTM_Template_Application.Android.csproj -f net9.0-android
```

### Using Visual Studio

1. Open `MTM_Template_Application.sln`
2. Set `MTM_Template_Application.Android` as the startup project (right-click → Set as Startup Project)
3. Select target device from dropdown (emulator or physical device)
4. Press F5 or click "Run"

---

## Debugging Android-Specific Code

### View Android Logs

```powershell
# Real-time logcat output
adb logcat

# Filter by app package
adb logcat | Select-String "MTM_Template_Application"

# Clear logs
adb logcat -c

# Save logs to file
adb logcat > android_logs.txt
```

### Common ADB Commands

```powershell
# Install APK manually
adb install -r path\to\app.apk

# Uninstall app
adb uninstall com.mtm.template

# Take screenshot
adb shell screencap -p /sdcard/screenshot.png
adb pull /sdcard/screenshot.png

# Record screen
adb shell screenrecord /sdcard/demo.mp4

# Check device info
adb shell getprop ro.build.version.release  # Android version
adb shell getprop ro.product.model           # Device model
```

---

## Troubleshooting

### Issue: "Android workload not found"

**Solution:**

```powershell
dotnet workload install android
dotnet workload restore
```

### Issue: "No devices found"

**Solution:**

- Emulator: Start an emulator first (`emulator -avd <name>`)
- Physical: Check USB connection, enable USB debugging
- Verify: `adb devices`

### Issue: "Build failed - Android SDK not found"

**Solution:**

```powershell
# Check Android SDK path
echo $env:ANDROID_SDK_ROOT

# Set if not configured (usually set automatically by workload)
$env:ANDROID_SDK_ROOT = "C:\Program Files\Android\android-sdk"
```

### Issue: Emulator is slow

**Solutions:**

- Enable hardware acceleration (Intel HAXM or AMD Hypervisor)
- Use x86_64 system images (not ARM)
- Allocate more RAM to emulator
- Use "Cold Boot" instead of "Quick Boot" if emulator hangs

### Issue: App crashes on startup

**Debug steps:**

1. Check logcat: `adb logcat`
2. Look for exceptions in logs
3. Verify `MainActivity.cs` is correctly configured
4. Check Android permissions in `AndroidManifest.xml`

---

## Project-Specific Configuration

### Current Android Project Settings

**File:** `MTM_Template_Application.Android/MTM_Template_Application.Android.csproj`

```xml
<TargetFramework>net9.0-android</TargetFramework>
<SupportedOSPlatformVersion>21</SupportedOSPlatformVersion>
```

**Minimum Requirements:**

- Android API Level 21 (Android 5.0 Lollipop)
- .NET 9.0 runtime

### Android-Specific Services

According to the implementation plan, these services need Android-specific implementations:

1. **AndroidSecretsService** - Uses Android KeyStore API
2. **AndroidServiceRegistration** - Dependency injection for Android platform
3. **Device-specific diagnostics** - Hardware capabilities, permissions

**Files to implement (see tasks.md):**

- `MTM_Template_Application/Services/Secrets/AndroidSecretsService.cs` (T111)
- `MTM_Template_Application.Android/Services/AndroidServiceRegistration.cs` (T133)

---

## VS Code Extension (Alternative to Visual Studio)

If using **VS Code** instead of Visual Studio:

### Required VS Code Extensions

1. **C# Dev Kit** (Microsoft)

   - ID: `ms-dotnettools.csdevkit`
   - Provides .NET debugging and project support

2. **Avalonia for VS Code** (AvaloniaUI)

   - ID: `AvaloniaTeam.vscode-avalonia`
   - XAML IntelliSense, previewer

3. **Android iOS Emulator** (DiemasMichiels)
   - ID: `DiemasMichiels.emulate`
   - Quick emulator launcher from VS Code

### Launch Configuration (`.vscode/launch.json`)

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Android",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build-android",
      "program": "${workspaceFolder}/MTM_Template_Application.Android/bin/Debug/net9.0-android/MTM_Template_Application.Android.dll",
      "cwd": "${workspaceFolder}",
      "console": "internalConsole",
      "stopAtEntry": false
    }
  ]
}
```

---

## Performance Considerations

### Android-Specific Optimizations

1. **AOT Compilation** (Ahead-of-Time):

   - Improves startup time
   - Increases APK size
   - Enable in Release builds

2. **Linking** (Trim unused code):

   - Reduces APK size
   - Test thoroughly before releasing

3. **Memory Management**:
   - Android has stricter memory limits
   - Target: <100MB (per project requirements)
   - Use memory profiling tools

### Enable AOT (Release builds)

Add to `.csproj`:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
  <AndroidEnableProfiledAot>true</AndroidEnableProfiledAot>
  <AndroidLinkMode>Full</AndroidLinkMode>
</PropertyGroup>
```

---

## Testing Checklist

Before deploying to production, verify:

- [ ] App launches in <10s on Android 11+ devices
- [ ] Boot sequence completes successfully (Stage 0 → 1 → 2)
- [ ] Splash screen displays correctly
- [ ] Android KeyStore secrets storage works
- [ ] Network connectivity detection works
- [ ] Storage permissions are granted
- [ ] App works in offline/cached-only mode
- [ ] Rotation/configuration changes don't crash app
- [ ] Memory usage stays under 100MB
- [ ] No ANR (Application Not Responding) errors

---

## Additional Resources

- **Avalonia Docs:** <https://docs.avaloniaui.net/>
- **Avalonia Android Guide:** <https://docs.avaloniaui.net/docs/guides/platforms/android>
- **.NET MAUI Android Docs:** <https://learn.microsoft.com/en-us/dotnet/maui/android/>
- **Android Developer Docs:** <https://developer.android.com/docs>
- **ADB Reference:** <https://developer.android.com/studio/command-line/adb>

---

## Quick Reference

### One-Command Setup

```powershell
# Install everything needed
dotnet workload install android

# Verify
dotnet workload list

# Build and run
dotnet build -f net9.0-android
dotnet run --project MTM_Template_Application.Android -f net9.0-android
```

### Daily Development Workflow

```powershell
# 1. Start emulator (or connect device)
emulator -avd Pixel5_API33

# 2. Verify connection
adb devices

# 3. Build and deploy
dotnet run --project MTM_Template_Application.Android -f net9.0-android

# 4. Watch logs
adb logcat | Select-String "MTM"
```

---

## Support

For issues specific to:

- **Avalonia Android:** <https://github.com/AvaloniaUI/Avalonia/issues>
- **.NET Android Workload:** <https://github.com/dotnet/android/issues>
- **This Project:** See `specs/001-boot-sequence-splash/tasks.md` for implementation status

---

**Last Updated:** October 3, 2025
**Maintained by:** MTM Development Team
