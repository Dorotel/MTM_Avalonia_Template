# MTM Avalonia Template Application

A cross-platform manufacturing data management application built with Avalonia UI and .NET 9.0.

## Features

- ✅ **Three-Stage Boot Sequence** - Optimized startup with splash screen and progress feedback
- ✅ **Cross-Platform** - Runs on Windows, Linux, and Android
- ✅ **Offline-First** - Works with cached data when Visual ERP server is unavailable
- ✅ **Secure Credentials** - OS-native secure storage (DPAPI, Keychain, KeyStore)
- ✅ **Comprehensive Logging** - Structured logging with OpenTelemetry support
- ✅ **Performance Optimized** - <10s boot time, <100MB memory usage
- ✅ **Theme Support** - Light/Dark/Auto themes with high contrast mode
- ✅ **Accessibility** - Screen reader support and keyboard navigation

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- MySQL 5.7+ (MAMP recommended for development)
- Visual ERP API Toolkit credentials

### Build and Run

```powershell
# Clone the repository
git clone https://github.com/Dorotel/MTM_Avalonia_Template.git
cd MTM_Avalonia_Template

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run desktop application
dotnet run --project src/MTM_Template_Application.Desktop

# Run tests
dotnet test
```

## Spec-Driven Development

This project uses **GitHub Spec Kit** for specification-driven development. Each feature has:

- **spec.md** - Functional requirements and acceptance criteria
- **plan.md** - Technical architecture and implementation approach
- **tasks.md** - Detailed task breakdown with completion tracking
- **quickstart.md** - Validation scenarios for testing

### Feature Validation

When a feature reaches 100% task completion, run the validation script:

```powershell
# Validate current feature (auto-detects from branch)
.\.specify\scripts\powershell\validate-implementation.ps1

# Validate specific feature
.\.specify\scripts\powershell\validate-implementation.ps1 001-boot-sequence-splash

# Generate JSON output for CI/CD
.\.specify\scripts\powershell\validate-implementation.ps1 -Json

# Strict mode (fail on warnings)
.\.specify\scripts\powershell\validate-implementation.ps1 -Strict
```

The validation script performs:
- ✅ Task completion verification (100% required)
- ✅ Functional requirement mapping
- ✅ Constitutional compliance audit
- ✅ Build validation (clean build required)
- ✅ Test execution (all tests must pass)
- ✅ Code quality assessment
- ✅ Documentation validation
- ✅ Security and performance review

**Validation Thresholds:**
- **PASS**: Ready for merge (0 blocking issues)
- **CONDITIONAL PASS**: Merge with follow-up tasks (<5 non-critical issues)
- **FAIL**: Cannot merge (blocking issues exist)

See `.github/prompts/validate-implementation.prompt.md` for detailed validation criteria.

## Boot Sequence

The application uses a three-stage boot sequence for optimal startup performance:

### Stage 0: Splash Screen (Target: <1s)
- Display splash window immediately
- Initialize watchdog timer (10s timeout)
- Show progress bar and status messages
- Theme-less rendering for fast display

### Stage 1: Services Initialization (Target: <3s)
1. **Configuration Service** - Load settings with layered precedence
2. **Secrets Service** - Retrieve credentials from OS-native storage
3. **Logging Service** - Initialize Serilog + OpenTelemetry
4. **Diagnostics Service** - Run health checks (storage, permissions, network)
5. **Data Layer** - Initialize MySQL, Visual API, HTTP clients
6. **Cache Service** - Load Visual master data (Parts, Locations, Warehouses)
7. **Core Services** - Message bus, validation, mapping
8. **Localization** - Load culture and resource dictionaries
9. **Theme Service** - Detect OS dark mode preference
10. **Navigation Service** - Initialize routing and history

### Stage 2: Application Ready (Target: <1s)
- Hide splash screen with fade animation
- Apply Theme V2 semantic tokens
- Construct main application shell
- Navigate to home screen
- Persist boot metrics

**Total Boot Time: <10 seconds**

## Architecture

### Technology Stack

- **Framework**: C# .NET 9.0 with nullable reference types
- **UI**: Avalonia 11.3+ (cross-platform XAML)
- **MVVM**: CommunityToolkit.Mvvm 8.3+
- **Database**: MySQL.Data (Visual ERP read-only access)
- **Caching**: In-memory with LZ4 compression
- **Logging**: Serilog + OpenTelemetry semantic conventions
- **Resilience**: Polly (exponential backoff, circuit breakers)
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Testing**: xUnit + FluentAssertions + NSubstitute

### Project Structure

```
MTM_Avalonia_Template/
├── MTM_Template_Application/          # Shared Avalonia app
│   ├── ViewModels/                    # MVVM ViewModels
│   ├── Views/                         # Avalonia XAML views
│   ├── Models/                        # Domain models
│   ├── Services/                      # Business logic & API clients
│   │   ├── Boot/                      # Boot orchestration
│   │   ├── Configuration/             # Config management
│   │   ├── Secrets/                   # Credential storage
│   │   ├── Logging/                   # Structured logging
│   │   ├── Diagnostics/               # Health checks
│   │   ├── DataLayer/                 # MySQL + Visual + HTTP
│   │   ├── Cache/                     # LZ4-compressed cache
│   │   ├── Core/                      # Message bus, validation
│   │   ├── Localization/              # Culture + resources
│   │   ├── Theme/                     # Theme management
│   │   └── Navigation/                # Navigation service
│   └── Extensions/                    # DI extensions
├── MTM_Template_Application.Desktop/  # Desktop entry point
├── MTM_Template_Application.Android/  # Android entry point
├── tests/                             # Unit + integration tests
├── specs/                             # Feature specifications
├── docs/                              # Documentation
└── .github/
    ├── mamp-database/                 # Database schema documentation
    │   ├── schema-tables.json         # Complete table structures
    │   ├── stored-procedures.json     # Stored procedure definitions
    │   ├── functions.json             # User-defined functions
    │   ├── views.json                 # Database views
    │   ├── indexes.json               # Index documentation
    │   ├── sample-data.json           # Test data
    │   ├── connection-info.json       # Connection settings
    │   └── migrations-history.json    # Migration versioning
    └── workflows/                     # GitHub Actions CI/CD

```

### Database Documentation (`.github/mamp-database/`)

**Critical Infrastructure**: All MAMP MySQL 5.7 database objects are documented in `.github/mamp-database/` JSON files as the **single source of truth** for schema structure.

#### Purpose

- **Schema Accuracy**: Prevents runtime errors from incorrect table/column names (case-sensitive)
- **Type Safety**: Documents exact data types to prevent type mismatches
- **Performance**: Tracks indexes to avoid performance degradation
- **Maintenance**: Documents stored procedures, functions, and views
- **Migration Tracking**: Maintains version history for schema changes

#### Mandatory Workflow

1. **Before writing code**: ALWAYS read `schema-tables.json` to verify table/column names and types
2. **During development**: Reference JSON files for exact schema (case-sensitive: `Users`, `UserId`, `PreferenceKey`)
3. **After database changes**: IMMEDIATELY update corresponding JSON file(s) with new/modified objects
4. **Before PR**: Ensure `lastUpdated` timestamp is current and `version` field incremented
5. **After feature completion**: Run database audit to verify accuracy

#### JSON File Structure

| File                      | Purpose                                                                           | Updated When                       |
| ------------------------- | --------------------------------------------------------------------------------- | ---------------------------------- |
| `schema-tables.json`      | Complete table structures with columns, types, constraints, indexes, foreign keys | Any table/column/constraint change |
| `stored-procedures.json`  | Stored procedure signatures, parameters, and logic                                | Procedure created/modified         |
| `functions.json`          | User-defined function definitions and return types                                | Function created/modified          |
| `views.json`              | Database view SQL definitions                                                     | View created/modified              |
| `indexes.json`            | Index documentation with performance notes                                        | Index added/removed/modified       |
| `sample-data.json`        | Test data used in development and testing                                         | Sample data changes                |
| `connection-info.json`    | Connection settings and environment configs                                       | Connection parameters change       |
| `migrations-history.json` | Version history with semantic versioning                                          | Any schema change                  |

#### Example: Reading Schema Before Code

```csharp
// ✅ CORRECT - Reference schema-tables.json first
// Verified from .github/mamp-database/schema-tables.json:
//   Table: Users (PascalCase)
//   Columns: UserId (INT, PK), Username (VARCHAR(100)), IsActive (BOOLEAN)

var query = @"
    SELECT UserId, Username, IsActive
    FROM Users
    WHERE IsActive = TRUE";

// ❌ INCORRECT - Guessing schema (may cause runtime errors)
var query = "SELECT user_id, user_name FROM users"; // Wrong case!
```

#### Automated Validation

The GitHub Action workflow `.github/workflows/database-schema-audit.yml` runs automatically to:
- ✅ Validate JSON structure and required fields
- ✅ Check version format (semantic versioning)
- ✅ Verify `lastUpdated` timestamp is current (<30 days)
- ✅ Validate table count matches actual tables
- ✅ Ensure each table has required metadata
- ⚠️ Warn if schema documentation is outdated

See [Constitution Principle VIII](.specify/memory/constitution.md#viii-mamp-mysql-database-documentation-non-negotiable-) for complete requirements.

## Configuration

### Environment Variables

```powershell
# Set environment
$env:MTM_ENVIRONMENT = "Development"

# Override log level
$env:MTM_LOG_LEVEL = "Debug"

# Custom database connection
$env:MTM_DATABASE_HOST = "localhost"
$env:MTM_DATABASE_PORT = "3306"
```

### Command-Line Arguments

```powershell
# Override log level
.\MTM_Template_Application.exe --LogLevel Debug

# Set custom timeout
.\MTM_Template_Application.exe --Database:Timeout 60

# Setup credentials
.\MTM_Template_Application.exe --setup-credentials
```

### Configuration Precedence

1. Command-line arguments (highest priority)
2. Environment variables
3. User config (`config.user.json`)
4. Application config (`config.base.json`)
5. Defaults (lowest priority)

## Performance

### Boot Performance Targets

| Metric                | Target   | Measured       |
| --------------------- | -------- | -------------- |
| Stage 0 (Splash)      | <1s      | ✅ Verified     |
| Stage 1 (Services)    | <3s      | ✅ Verified     |
| Stage 2 (Application) | <1s      | ✅ Verified     |
| **Total Boot Time**   | **<10s** | **✅ Verified** |

### Memory Budget

| Component          | Target     | Notes                        |
| ------------------ | ---------- | ---------------------------- |
| Cache (compressed) | ~40MB      | LZ4 compression (~3:1 ratio) |
| Core Services      | ~30MB      | DI container, logging, pools |
| Framework          | ~30MB      | Avalonia UI, .NET runtime    |
| **Total**          | **<100MB** | **During startup**           |

## Offline Mode

When Visual ERP server is unavailable:

1. ✅ Application starts normally with cached data
2. ✅ Warning banner displays: "Working offline - data may be stale"
3. ✅ Cache age indicator shows last update time
4. ✅ Most features remain functional (read-only)
5. ✅ Write operations queued for sync when reconnected
6. ✅ Automatic reconnection attempts with exponential backoff

## Testing

### Run All Tests

```powershell
# All tests (unit + integration)
dotnet test

# Unit tests only
dotnet test --filter "Category=Unit"

# Integration tests only
dotnet test --filter "Category=Integration"

# Performance tests
dotnet test --filter "Category=Performance"
```

### Validation Scenarios

See [specs/001-boot-sequence-splash/quickstart.md](specs/001-boot-sequence-splash/quickstart.md) for 9 executable validation scenarios including:

1. Normal boot sequence (happy path)
2. Configuration loading and override precedence
3. Credential storage and validation
4. Diagnostic checks and issue detection
5. Visual master data caching
6. Logging and telemetry
7. Error handling and recovery
8. Accessibility and localization
9. Performance validation

## Documentation

- [Boot Sequence Guide](docs/BOOT-SEQUENCE.md) - Complete startup architecture
- [Troubleshooting](docs/TROUBLESHOOTING-CATALOG.md) - Common issues and solutions
- [Spec-Kit Guides](docs/Specify%20Guides/) - Feature specification workflow
- [Copilot Instructions](.github/copilot-instructions.md) - AI-assisted development patterns

## Contributing

This project uses **Spec-Driven Development (SDD)** with GitHub Spec Kit:

1. Create feature specification: `/specify`
2. Clarify requirements: `/clarify`
3. Generate technical plan: `/plan`
4. Break down into tasks: `/tasks`
5. Execute implementation: `/implement`
6. Analyze consistency: `/analyze`

All specifications are in `specs/` directory.

## License

[Your License Here]

## Contact

- **Author**: John Koll
- **GitHub**: [@Dorotel](https://github.com/Dorotel)
- **Project**: [MTM_Avalonia_Template](https://github.com/Dorotel/MTM_Avalonia_Template)

---

**Status**: 🚀 Feature 001 (Boot Sequence) Implementation Complete
