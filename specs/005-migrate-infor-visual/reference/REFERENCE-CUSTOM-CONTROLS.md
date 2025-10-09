# Reference: Custom Controls to Extract

**Date**: October 8, 2025

**Purpose**: Catalog of custom controls to extract from DebugTerminalWindow.axaml and new controls to create

---

## Extraction Threshold Rule

**Constitution Principle XI**: Extract to reusable custom control when:

- Pattern occurs **3+ times** across the codebase
- Naming convention: `PurposeTypeControl` (e.g., `StatusCardControl`, `MetricDisplayControl`)
- Documentation required in `docs/UI-CUSTOM-CONTROLS-CATALOG.md`

---

## Controls to Extract from DebugTerminalWindow.axaml

### 1. StatusCard Control

**Purpose**: Display status information in a card layout with title and content

**Occurrences**: 15+ in DebugTerminalWindow.axaml

**Current Pattern**:

```xml
<Border Background="{DynamicResource SystemControlBackgroundBaseLowBrush}"
    BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
    BorderThickness="1"
    CornerRadius="8"
    Padding="20">
    <StackPanel Spacing="16">
        <TextBlock Text="[Title]"
            FontSize="18"
            FontWeight="SemiBold"
            Foreground="{DynamicResource SystemControlForegroundBaseHighBrush}" />
        <!-- Content -->
    </StackPanel>
</Border>
```

**Proposed API**:

```xml
<controls:StatusCard Title="Performance Snapshot"
    Icon="{StaticResource PerformanceIcon}"
    Content="{CompiledBinding PerformanceContent}" />
```

**Properties**:

- `Title` (string): Card title
- `Icon` (PathGeometry?): Optional icon
- `Content` (object): Content to display
- `ContentTemplate` (DataTemplate?): Custom content template

---

### 2. MetricDisplay Control

**Purpose**: Display labeled metric with value and optional status indicator

**Occurrences**: 20+ in DebugTerminalWindow.axaml

**Current Pattern**:

```xml
<Grid ColumnDefinitions="200,*,150">
    <TextBlock Grid.Column="0" Text="[Label]"
        FontWeight="SemiBold"
        Foreground="{DynamicResource SystemControlForegroundBaseMediumHighBrush}" />
    <TextBlock Grid.Column="1"
        Text="{CompiledBinding Value}"
        Foreground="{DynamicResource SystemControlForegroundBaseMediumHighBrush}" />
    <TextBlock Grid.Column="2"
        Text="[Status]"
        Foreground="{DynamicResource SystemAccentColor}" />
</Grid>
```

**Proposed API**:

```xml
<controls:MetricDisplay Label="Total Memory"
    Value="{CompiledBinding TotalMemoryMB}"
    Unit="MB"
    Status="{CompiledBinding MemoryStatus}"
    StatusColor="{CompiledBinding MemoryStatusColor}" />
```

**Properties**:

- `Label` (string): Metric label
- `Value` (object): Metric value
- `Unit` (string?): Optional unit suffix
- `Status` (string?): Optional status text
- `StatusColor` (Brush?): Status indicator color

---

### 3. ErrorListPanel Control

**Purpose**: Display list of errors with severity indicators

**Occurrences**: 5+ in DebugTerminalWindow.axaml

**Current Pattern**:

```xml
<Border Background="{DynamicResource SystemControlBackgroundBaseLowBrush}"
    BorderBrush="{DynamicResource SystemControlForegroundBaseMediumLowBrush}"
    BorderThickness="1">
    <ScrollViewer MaxHeight="300">
        <ItemsControl ItemsSource="{CompiledBinding Errors}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Grid ColumnDefinitions="Auto,*,Auto">
                        <!-- Error icon -->
                        <!-- Error message -->
                        <!-- Timestamp -->
                    </Grid>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </ScrollViewer>
</Border>
```

**Proposed API**:

```xml
<controls:ErrorListPanel Errors="{CompiledBinding ErrorHistory}"
    MaxHeight="300"
    ShowTimestamps="True"
    ShowSeverityIcons="True" />
```

**Properties**:

- `Errors` (IEnumerable<ErrorLogEntry>): Error collection
- `MaxHeight` (double): Maximum height before scrolling
- `ShowTimestamps` (bool): Show/hide timestamps
- `ShowSeverityIcons` (bool): Show/hide severity icons

---

### 4. ConnectionHealthBadge Control

**Purpose**: Display connection status with colored indicator

**Occurrences**: 8+ in DebugTerminalWindow.axaml

**Current Pattern**:

```xml
<Border Background="{DynamicResource SuccessBrush}"
    CornerRadius="12"
    Padding="8,4">
    <TextBlock Text="Connected"
        FontSize="12"
        Foreground="White" />
</Border>
```

**Proposed API**:

```xml
<controls:ConnectionHealthBadge Status="{CompiledBinding DatabaseConnectionStatus}"
    Label="{CompiledBinding DatabaseConnectionLabel}" />
```

**Properties**:

- `Status` (ConnectionStatus enum): Connected, Disconnected, Connecting, Error
- `Label` (string): Badge label

**Status Colors**:

- Connected: Green
- Disconnected: Gray
- Connecting: Yellow
- Error: Red

---

### 5. BootTimelineChart Control

**Purpose**: Visual timeline of boot stages with duration bars

**Occurrences**: 3+ in DebugTerminalWindow.axaml

**Current Pattern**:

```xml
<ItemsControl ItemsSource="{CompiledBinding BootTimeline}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Grid ColumnDefinitions="150,*,100">
                <TextBlock Grid.Column="0" Text="{CompiledBinding StageName}" />
                <Border Grid.Column="1" Background="{DynamicResource SystemAccentColor}"
                    Width="{CompiledBinding DurationWidth}" />
                <TextBlock Grid.Column="2" Text="{CompiledBinding DurationText}" />
            </Grid>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

**Proposed API**:

```xml
<controls:BootTimelineChart Timeline="{CompiledBinding BootTimeline}"
    MaxDuration="{CompiledBinding MaxBootDuration}"
    ShowTargets="True" />
```

**Properties**:

- `Timeline` (IEnumerable<BootTimelineEntry>): Boot stage data
- `MaxDuration` (long): Maximum duration for scaling
- `ShowTargets` (bool): Show/hide target duration indicators

---

## New Controls to Create

### 6. SettingsCategory Control

**Purpose**: Group settings with collapsible header

**Use Case**: Settings screen organization

**Proposed API**:

```xml
<controls:SettingsCategory Title="Visual ERP Integration"
    Icon="{StaticResource VisualIcon}"
    IsExpanded="True">
    <!-- Settings content -->
</controls:SettingsCategory>
```

**Properties**:

- `Title` (string): Category title
- `Icon` (PathGeometry?): Optional icon
- `IsExpanded` (bool): Expanded state
- `Content` (object): Settings content

---

### 7. SettingRow Control

**Purpose**: Single setting row with label, description, and input control

**Use Case**: Settings screen individual settings

**Proposed API**:

```xml
<controls:SettingRow Label="API Base URL"
    Description="Base URL for Visual ERP API endpoints"
    InputControl="{Binding ApiUrlInput}" />
```

**Properties**:

- `Label` (string): Setting label
- `Description` (string?): Optional description
- `InputControl` (Control): Input control (TextBox, ComboBox, etc.)

---

### 8. NavigationMenuItem Control

**Purpose**: Side panel navigation menu item with icon and label

**Use Case**: Debug Terminal modernized navigation

**Proposed API**:

```xml
<controls:NavigationMenuItem Label="Performance"
    Icon="{StaticResource PerformanceIcon}"
    IsSelected="{CompiledBinding IsPerformanceSelected}"
    Command="{CompiledBinding NavigateToPerformanceCommand}" />
```

**Properties**:

- `Label` (string): Menu item label
- `Icon` (PathGeometry): Menu icon
- `IsSelected` (bool): Selected state
- `Command` (ICommand): Navigation command

---

### 9. ActionButtonGroup Control

**Purpose**: Group of action buttons with consistent styling

**Use Case**: Debug Terminal action buttons (Export, Clear, etc.)

**Proposed API**:

```xml
<controls:ActionButtonGroup>
    <controls:ActionButton Label="Export Diagnostics"
        Icon="{StaticResource ExportIcon}"
        Command="{CompiledBinding ExportCommand}" />
    <controls:ActionButton Label="Clear Errors"
        Icon="{StaticResource ClearIcon}"
        Command="{CompiledBinding ClearErrorsCommand}" />
</controls:ActionButtonGroup>
```

**Properties**:

- `Orientation` (Orientation): Horizontal/Vertical
- `Spacing` (double): Space between buttons

---

### 10. ConfigurationErrorDialog Control

**Purpose**: Modal dialog for configuration errors with recovery options

**Use Case**: MainWindow.axaml TODO (Feature 002)

**Proposed API**:

```xml
<controls:ConfigurationErrorDialog IsVisible="{CompiledBinding HasConfigError}"
    ErrorMessage="{CompiledBinding ConfigErrorMessage}"
    RecoveryOptions="{CompiledBinding RecoveryOptions}"
    OnRecoverySelected="{CompiledBinding OnRecoverySelectedCommand}" />
```

**Properties**:

- `ErrorMessage` (string): Error description
- `RecoveryOptions` (IEnumerable<string>): Recovery action options
- `OnRecoverySelected` (ICommand): Recovery action command

---

## Control Library Structure

```
MTM_Template_Application/Controls/
├── Cards/
│   ├── StatusCard.axaml
│   └── StatusCard.axaml.cs
├── Metrics/
│   ├── MetricDisplay.axaml
│   ├── BootTimelineChart.axaml
│   └── ConnectionHealthBadge.axaml
├── Settings/
│   ├── SettingsCategory.axaml
│   └── SettingRow.axaml
├── Navigation/
│   └── NavigationMenuItem.axaml
├── Dialogs/
│   └── ConfigurationErrorDialog.axaml
├── Lists/
│   └── ErrorListPanel.axaml
└── Buttons/
    ├── ActionButton.axaml
    └── ActionButtonGroup.axaml
```

---

## Testing Strategy

Each custom control must have:

1. **Unit tests**: Property validation, command execution
2. **Visual tests**: Preview in Avalonia previewer
3. **Integration tests**: Usage in actual views
4. **Documentation**: Entry in `docs/UI-CUSTOM-CONTROLS-CATALOG.md`

**Coverage Target**: 80%+ per Constitution Principle IV

---

## Implementation Order

1. **StatusCard** - Most widely used (15+ occurrences)
2. **MetricDisplay** - High usage (20+ occurrences)
3. **ErrorListPanel** - Critical for diagnostics
4. **ConnectionHealthBadge** - Simple, quick win
5. **BootTimelineChart** - Visualization component
6. **SettingsCategory** - New settings UI
7. **SettingRow** - New settings UI
8. **NavigationMenuItem** - Debug Terminal rewrite
9. **ActionButtonGroup** - Debug Terminal rewrite
10. **ConfigurationErrorDialog** - MainWindow TODO

---

## Custom Control Catalog TODO

After creating controls, update `docs/UI-CUSTOM-CONTROLS-CATALOG.md` with:

- Control name and purpose
- API documentation (properties, events, commands)
- Usage examples
- Visual preview screenshots
- Best practices
- Known limitations
