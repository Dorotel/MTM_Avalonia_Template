---
description: 'Avalonia Theme System usage guide for UI development - ThemeV2 semantic tokens, ThemeService integration, and color guidelines'
applyTo: '**/*.axaml,**/*ViewModel.cs'
---

# Theme System Implementation Guide

Comprehensive guide for implementing themed UI elements using the MTM Avalonia Template theme system.

## Table of Contents

1. [Theme System Architecture](#theme-system-architecture)
2. [Using ThemeService in Code](#using-themeservice-in-code)
3. [ThemeV2 Semantic Token Reference](#themev2-semantic-token-reference)
4. [Control-Specific Color Guidelines](#control-specific-color-guidelines)
5. [Common UI Patterns](#common-ui-patterns)
6. [Validation Rules](#validation-rules)

---

## Theme System Architecture

### Overview

The application uses a **three-tier theme system**:

1. **OS-Level Detection**: `OSDarkModeMonitor` detects system dark mode changes (Windows Registry, Linux gsettings)
2. **Theme Management**: `ThemeService` manages theme state (Light/Dark/Auto) and broadcasts changes
3. **UI Binding**: XAML uses `{DynamicResource}` bindings to ThemeV2 semantic tokens

### Theme Modes

| Mode | Behavior |
|------|----------|
| **Light** | Always use light theme regardless of OS setting |
| **Dark** | Always use dark theme regardless of OS setting |
| **Auto** | Follow OS dark mode setting (default) |

### Key Services

#### `IThemeService`

```csharp
public interface IThemeService
{
    void SetTheme(string themeMode); // "Light", "Dark", or "Auto"
    ThemeConfiguration GetCurrentTheme();
    event EventHandler<ThemeChangedEventArgs>? OnThemeChanged;
}
```

#### `IOSDarkModeMonitor`

```csharp
public interface IOSDarkModeMonitor : IDisposable
{
    bool IsOSDarkMode(); // Current OS dark mode state
    event EventHandler<DarkModeChangedEventArgs>? OnDarkModeChanged;
}
```

---

## Using ThemeService in Code

### Dependency Injection

Services are registered in `Program.cs` via AppBuilder:

```csharp
services.AddSingleton<IOSDarkModeMonitor, OSDarkModeMonitor>();
services.AddSingleton<IThemeService, ThemeService>();
```

### ViewModel Integration

```csharp
public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private string _selectedTheme = "Auto";

    public SettingsViewModel(IThemeService themeService)
    {
        ArgumentNullException.ThrowIfNull(themeService);
        _themeService = themeService;

        // Get current theme
        var currentTheme = _themeService.GetCurrentTheme();
        SelectedTheme = currentTheme.ThemeMode;

        // Subscribe to theme changes
        _themeService.OnThemeChanged += OnThemeChanged;
    }

    [RelayCommand]
    private void ChangeTheme(string themeMode)
    {
        _themeService.SetTheme(themeMode); // "Light", "Dark", or "Auto"
    }

    private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        // React to theme change
        Debug.WriteLine($"Theme changed: {e.OldTheme} → {e.NewTheme} (IsDark: {e.IsDarkMode})");
    }
}
```

### Direct OS Dark Mode Detection

```csharp
public class SomeService
{
    private readonly IOSDarkModeMonitor _darkModeMonitor;

    public SomeService(IOSDarkModeMonitor darkModeMonitor)
    {
        _darkModeMonitor = darkModeMonitor;

        // Check current OS dark mode state
        bool isDark = _darkModeMonitor.IsOSDarkMode();

        // Subscribe to OS-level changes
        _darkModeMonitor.OnDarkModeChanged += (sender, e) =>
        {
            Debug.WriteLine($"OS dark mode: {e.IsDarkMode} at {e.ChangedAt}");
        };
    }
}
```

---

## ThemeV2 Semantic Token Reference

### CRITICAL: Always Use DynamicResource

❌ **NEVER** use hardcoded colors in XAML (except splash screens):

```xml
<!-- ❌ WRONG -->
<Border Background="#F5F5F5" Foreground="#212121" />
```

✅ **ALWAYS** use DynamicResource with semantic tokens:

```xml
<!-- ✅ CORRECT -->
<Border Background="{DynamicResource SystemChromeLowColor}"
        Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />
```

### Available Semantic Tokens

#### Accent Colors (Primary Branding)

| Token | Usage | Example |
|-------|-------|---------|
| `SystemAccentColor` | Primary brand color, action buttons, focus indicators | Title bars, primary buttons, status dots |
| `SystemAccentForegroundColor` | Text/icons on accent backgrounds | White text on blue title bar |
| `SystemAccentColorLight1` | Lighter accent variant | Hover states |
| `SystemAccentColorLight2` | Even lighter accent | Subtle highlights |
| `SystemAccentColorLight3` | Lightest accent | Info card backgrounds |
| `SystemAccentColorDark1` | Darker accent variant | Pressed states |
| `SystemAccentColorDark2` | Even darker accent | Values on accent backgrounds |

#### Chrome/Window Colors

| Token | Usage | Example |
|-------|-------|---------|
| `SystemChromeLowColor` | Window/page background, header sections | Main window background |
| `SystemChromeHighColor` | Elevated surfaces, secondary backgrounds | Code viewer backgrounds |
| `SystemChromeMediumColor` | Middle-tier surfaces | Toolbars |

#### Control Backgrounds

| Token | Usage | Example |
|-------|-------|---------|
| `SystemControlBackgroundBaseLowBrush` | Card backgrounds, content containers | Status cards, info panels |
| `SystemControlBackgroundBaseMediumLowBrush` | Subtle backgrounds | Input field backgrounds |
| `SystemControlBackgroundBaseMediumBrush` | Medium contrast backgrounds | Disabled controls |

#### Control Foregrounds (Text)

| Token | Usage | Example |
|-------|-------|---------|
| `SystemControlForegroundBaseHighBrush` | Primary text, headings, important content | Card titles, main text |
| `SystemControlForegroundBaseMediumHighBrush` | Standard body text | Paragraph text, list items |
| `SystemControlForegroundBaseMediumBrush` | Secondary text, labels | Form labels, metadata |
| `SystemControlForegroundBaseMediumLowBrush` | Subtle text, borders, separators | Dividers, borders |
| `SystemControlForegroundBaseLowBrush` | Disabled text | Greyed-out items |

#### List/Hover States

| Token | Usage | Example |
|-------|-------|---------|
| `SystemListLowColor` | List hover background, subtle interactions | Button hover (light) |
| `SystemListMediumColor` | List pressed/selected background | Button pressed |

#### Semantic State Colors

| Token | Usage | Example |
|-------|-------|---------|
| `SystemErrorTextColor` | Error states, destructive actions | Close button hover, error text |
| `SystemSuccessTextColor` | Success states (if available) | Success messages |
| `SystemWarningTextColor` | Warning states (if available) | Warning indicators |

---

## Control-Specific Color Guidelines

### Window & Page Structure

```xml
<!-- Main Window -->
<Window Background="{DynamicResource SystemChromeLowColor}">
    <!-- Title Bar -->
    <Border Background="{DynamicResource SystemAccentColor}">
        <TextBlock Foreground="{DynamicResource SystemAccentForegroundColor}" />
    </Border>

    <!-- Header Section -->
    <Border Background="{DynamicResource SystemChromeLowColor}"
            BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
            BorderThickness="0,0,0,1">
        <TextBlock Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />
    </Border>

    <!-- Content Area -->
    <Grid Background="{DynamicResource SystemChromeLowColor}" />

    <!-- Footer -->
    <Border Background="{DynamicResource SystemChromeLowColor}"
            BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
            BorderThickness="0,1,0,0">
        <TextBlock Foreground="{DynamicResource SystemControlForegroundBaseMediumBrush}" />
    </Border>
</Window>
```

### Cards & Containers

```xml
<!-- Standard Card -->
<Border Background="{DynamicResource SystemControlBackgroundBaseLowBrush}"
        BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
        BorderThickness="1"
        CornerRadius="6"
        Padding="16">
    <!-- Card Title -->
    <TextBlock Text="Title"
               FontSize="16"
               FontWeight="SemiBold"
               Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />

    <!-- Card Body Text -->
    <TextBlock Text="Content"
               Foreground="{DynamicResource SystemControlForegroundBaseMediumHighBrush}" />

    <!-- Card Metadata/Labels -->
    <TextBlock Text="Label:"
               Foreground="{DynamicResource SystemControlForegroundBaseMediumBrush}" />
</Border>
```

### Buttons

```xml
<!-- Primary Button (Accent) -->
<Button Background="{DynamicResource SystemAccentColor}"
        Foreground="{DynamicResource SystemAccentForegroundColor}"
        Content="Primary Action" />

<!-- Secondary Button (Transparent with Hover) -->
<Button Background="Transparent"
        Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}">
    <Button.Styles>
        <Style Selector="Button:pointerover">
            <Setter Property="Background" Value="{DynamicResource SystemListLowColor}" />
        </Style>
        <Style Selector="Button:pressed">
            <Setter Property="Background" Value="{DynamicResource SystemListMediumColor}" />
        </Style>
    </Button.Styles>
</Button>

<!-- Destructive Button (Close/Delete) -->
<Button Background="Transparent"
        Foreground="{DynamicResource SystemAccentForegroundColor}">
    <Button.Styles>
        <Style Selector="Button:pointerover">
            <Setter Property="Background" Value="{DynamicResource SystemErrorTextColor}" />
        </Style>
        <Style Selector="Button:pressed">
            <Setter Property="Background" Value="{DynamicResource SystemErrorTextColor}" />
        </Style>
    </Button.Styles>
</Button>
```

### Text Hierarchy

```xml
<!-- Page/Section Title (Large, Bold) -->
<TextBlock Text="Page Title"
           FontSize="22"
           FontWeight="Bold"
           Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />

<!-- Card/Component Title (Medium, SemiBold) -->
<TextBlock Text="Component Title"
           FontSize="16"
           FontWeight="SemiBold"
           Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />

<!-- Body Text (Standard) -->
<TextBlock Text="This is body text"
           FontSize="13"
           Foreground="{DynamicResource SystemControlForegroundBaseMediumHighBrush}" />

<!-- Labels/Metadata (Smaller, Subtle) -->
<TextBlock Text="Label:"
           FontSize="13"
           Foreground="{DynamicResource SystemControlForegroundBaseMediumBrush}" />

<!-- Captions/Footer Text (Smallest, Most Subtle) -->
<TextBlock Text="© 2025 Copyright"
           FontSize="11"
           Foreground="{DynamicResource SystemControlForegroundBaseMediumBrush}" />
```

### Status Indicators

```xml
<!-- Success/Active Status -->
<Border Background="{DynamicResource SystemAccentColor}"
        Width="6" Height="6"
        CornerRadius="3" />

<!-- With Status Text -->
<StackPanel Orientation="Horizontal" Spacing="8">
    <Border Background="{DynamicResource SystemAccentColor}"
            Width="6" Height="6"
            CornerRadius="3"
            VerticalAlignment="Center" />
    <TextBlock Text="Active"
               Foreground="{DynamicResource SystemControlForegroundBaseMediumHighBrush}" />
</StackPanel>
```

### Accent Info Cards

```xml
<!-- Info Card with Accent Background -->
<Border Background="{DynamicResource SystemAccentColorLight3}"
        BorderBrush="{DynamicResource SystemAccentColor}"
        BorderThickness="1"
        CornerRadius="4"
        Padding="12">
    <!-- Accent Label Text -->
    <TextBlock Text="Duration"
               FontSize="11"
               Foreground="{DynamicResource SystemAccentColorDark1}"
               FontWeight="Medium" />

    <!-- Accent Value Text -->
    <TextBlock Text="732ms"
               FontSize="18"
               FontWeight="Bold"
               Foreground="{DynamicResource SystemAccentColorDark2}" />
</Border>
```

### Code/Log Viewers

```xml
<!-- Code Viewer Background -->
<Border Background="{DynamicResource SystemChromeHighColor}"
        BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
        BorderThickness="1"
        CornerRadius="4">
    <ScrollViewer>
        <!-- Monospace Text -->
        <TextBlock Text="[00:45:05.482] Boot sequence started"
                   FontSize="10"
                   FontFamily="Consolas,Courier New,monospace"
                   Foreground="{DynamicResource SystemControlForegroundBaseMediumHighBrush}"
                   TextWrapping="NoWrap" />
    </ScrollViewer>
</Border>
```

### Separators & Borders

```xml
<!-- Horizontal Divider -->
<Border Height="1"
        Background="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
        Margin="0,8" />

<!-- Section Border -->
<Border BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
        BorderThickness="0,0,0,1"
        Padding="16" />
```

---

## Common UI Patterns

### Title Bar with Window Controls

```xml
<Border Background="{DynamicResource SystemAccentColor}"
        PointerPressed="TitleBar_PointerPressed">
    <Grid ColumnDefinitions="Auto,*,Auto">
        <!-- App Title -->
        <TextBlock Grid.Column="0"
                   Text="App Name"
                   Foreground="{DynamicResource SystemAccentForegroundColor}" />

        <!-- Window Control Buttons -->
        <StackPanel Grid.Column="2" Orientation="Horizontal">
            <!-- Minimize -->
            <Button Foreground="{DynamicResource SystemAccentForegroundColor}">
                <Button.Styles>
                    <Style Selector="Button:pointerover">
                        <Setter Property="Background" Value="{DynamicResource SystemListLowColor}" />
                    </Style>
                </Button.Styles>
            </Button>

            <!-- Close -->
            <Button Foreground="{DynamicResource SystemAccentForegroundColor}">
                <Button.Styles>
                    <Style Selector="Button:pointerover">
                        <Setter Property="Background" Value="{DynamicResource SystemErrorTextColor}" />
                    </Style>
                </Button.Styles>
            </Button>
        </StackPanel>
    </Grid>
</Border>
```

### Two-Column Layout with Cards

```xml
<Grid ColumnDefinitions="*,*" Margin="20">
    <!-- Left Column -->
    <StackPanel Grid.Column="0" Spacing="16" Margin="0,0,10,0">
        <Border Background="{DynamicResource SystemControlBackgroundBaseLowBrush}"
                BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
                BorderThickness="1"
                CornerRadius="6"
                Padding="16">
            <!-- Card content -->
        </Border>
    </StackPanel>

    <!-- Right Column -->
    <Border Grid.Column="1"
            Background="{DynamicResource SystemControlBackgroundBaseLowBrush}"
            BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
            BorderThickness="1"
            CornerRadius="6"
            Padding="16"
            Margin="10,0,0,0">
        <!-- Card content -->
    </Border>
</Grid>
```

---

## Validation Rules

### Constitutional Requirements

From Constitution IV (Theme V2 Semantic Tokens):

✅ **MUST**:
- Use `{DynamicResource System*}` for all colors in XAML
- Never use hardcoded hex colors (#RRGGBB) except in splash screens
- Separate styles into dedicated `.axaml` files when appropriate

❌ **MUST NOT**:
- Use hardcoded colors like `#F5F5F5`, `White`, `Black`, `Red`, etc.
- Use `{Binding}` without `x:CompileBindings="True"`
- Mix theme colors with hardcoded colors

### Validation Script Detection

The `validate-implementation.ps1` script checks for:
- Hardcoded hex colors: `#[0-9A-Fa-f]{6,8}`
- Named colors: `White`, `Black`, `Red`, `Blue`, `Green`, etc.
- **Exception**: Splash screen files are excluded from validation

### Testing Theme Changes

```csharp
// In tests, verify theme reactivity
[Fact]
public void ThemeService_WhenModeChanged_ShouldRaiseEvent()
{
    var themeService = GetThemeService();
    ThemeChangedEventArgs? capturedArgs = null;

    themeService.OnThemeChanged += (s, e) => capturedArgs = e;
    themeService.SetTheme("Dark");

    capturedArgs.Should().NotBeNull();
    capturedArgs!.NewTheme.Should().Be("Dark");
}
```

---

## Quick Reference Cheat Sheet

| Element | Background | Foreground | Border |
|---------|------------|------------|--------|
| **Window** | `SystemChromeLowColor` | - | - |
| **Title Bar** | `SystemAccentColor` | `SystemAccentForegroundColor` | - |
| **Header** | `SystemChromeLowColor` | `SystemControlForegroundBaseHighBrush` | `SystemControlForegroundBaseMediumLowBrush` |
| **Card** | `SystemControlBackgroundBaseLowBrush` | `SystemControlForegroundBaseMediumHighBrush` | `SystemControlForegroundBaseMediumLowBrush` |
| **Card Title** | - | `SystemControlForegroundBaseHighBrush` | - |
| **Body Text** | - | `SystemControlForegroundBaseMediumHighBrush` | - |
| **Label** | - | `SystemControlForegroundBaseMediumBrush` | - |
| **Button (Primary)** | `SystemAccentColor` | `SystemAccentForegroundColor` | - |
| **Button Hover** | `SystemListLowColor` | - | - |
| **Button Pressed** | `SystemListMediumColor` | - | - |
| **Status Dot** | `SystemAccentColor` | - | - |
| **Code Viewer** | `SystemChromeHighColor` | `SystemControlForegroundBaseMediumHighBrush` | `SystemControlForegroundBaseMediumLowBrush` |
| **Footer** | `SystemChromeLowColor` | `SystemControlForegroundBaseMediumBrush` | `SystemControlForegroundBaseMediumLowBrush` |

---

## Examples from Production Code

### MainWindow.axaml (Desktop)

```xml
<!-- Window Background -->
<Window Background="{DynamicResource SystemChromeLowColor}">
    <!-- Title Bar -->
    <Border Background="{DynamicResource SystemAccentColor}">
        <TextBlock Foreground="{DynamicResource SystemAccentForegroundColor}" />
    </Border>

    <!-- Content Cards -->
    <Border Background="{DynamicResource SystemControlBackgroundBaseLowBrush}"
            BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
            BorderThickness="1">
        <TextBlock Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />
    </Border>
</Window>
```

### MainView.axaml (Mobile)

```xml
<!-- Mobile Header -->
<Border Background="{DynamicResource SystemAccentColor}">
    <TextBlock Foreground="{DynamicResource SystemAccentForegroundColor}" />
</Border>

<!-- Mobile Content -->
<Border Background="{DynamicResource SystemChromeLowColor}">
    <Border Background="{DynamicResource SystemControlBackgroundBaseLowBrush}"
            BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}">
        <!-- Content -->
    </Border>
</Border>
```

---

## Troubleshooting

### Theme Not Updating Dynamically

**Problem**: Colors don't change when switching theme mode

**Solution**:
- Ensure using `{DynamicResource}` (NOT `{StaticResource}`)
- Verify ThemeService is registered in DI container
- Check that OSDarkModeMonitor is running (polling every 5 seconds)

### Colors Look Wrong in Light/Dark Mode

**Problem**: Text is unreadable or colors clash

**Solution**:
- Use semantic tokens (e.g., `SystemControlForegroundBaseHighBrush`)
- Never assume background color - always use appropriate contrast token
- Test in both light and dark modes

### Validation Failing with "Hardcoded Colors"

**Problem**: Validation script reports hardcoded color violations

**Solution**:
- Search for hex colors: `#[0-9A-Fa-f]{6,8}`
- Replace with appropriate `{DynamicResource}` token
- Verify splash screen files are excluded (should be)

---

**Last Updated**: 2025-10-05
**Constitution Version**: 1.1.0
**Theme System Version**: ThemeV2 (Avalonia FluentTheme-based)
