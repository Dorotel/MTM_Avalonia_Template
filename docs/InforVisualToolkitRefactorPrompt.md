# Refactor Prompt: Infor VISUAL API Toolkit Integration

## Objective
Refactor the MTM Avalonia application so that every interaction with the Infor VISUAL system uses the official Infor VISUAL API Toolkit instead of HTTP endpoints. Deliver a maintainable, testable integration layer that honors toolkit contracts, database session discipline, and the project’s Avalonia/MVVM architecture.

## Background Snapshot
- Current implementation relies on custom HTTP endpoints to reach VISUAL.
- The VISUAL API Toolkit (assemblies such as `VmfgCore.dll`, `VmfgShared.dll`, `VmfgInventory.dll`, `VmfgShopFloor.dll`) is a .NET 4.x, x86-first technology that communicates directly with the VISUAL database and business object layer.
- Toolkit access patterns center on `Lsa.Data.Dbms` for database sessions plus the `BusinessDocument`/`BusinessTransaction` object model for CRUD and transactional workflows.
- Connection configuration is defined through `Database.Config` and established with `Dbms.OpenLocal`, `Dbms.OpenDirect`, or `Dbms.OpenLocalSSO` depending on deployment mode.

## Non-Negotiable Requirements
1. **Use Toolkit Sessions**
   - Instantiate `Lsa.Data.Dbms` with the correct provider configuration and call `OpenLocal` (desktop) or `OpenDirect` (service) with validated credentials.
   - Enforce single-owner disposal semantics: always wrap DBMS objects in `using` blocks or explicit `Dispose` calls.
2. **Business Objects Only**
   - Perform operations through toolkit classes (for example `Lsa.Vmfg.Shared.Contact`, `Lsa.Vmfg.Inventory.Part`, `Lsa.Vmfg.ShopFloor.WorkOrderTransaction`).
   - Use their `Load`, `Find`, `Prepare`, `Validate`, and `Save` patterns rather than issuing ad-hoc SQL or HTTP.
3. **Auto Numbering & Services**
   - Retrieve identifiers through toolkit services (`AutoNumber`, `ServiceUnitCost`, `GeneralQuery`, etc.)
   - Respect transaction boundaries via `BusinessTransaction.Prepare()` and `BusinessTransaction.Save()`.
4. **Security & Authentication**
   - Store VISUAL credentials in the existing secrets service and hydrate `Dbms` open calls with runtime values.
   - Do not log connection strings or passwords; log success/failure with correlation IDs only.
5. **Architecture Alignment**
   - Keep DI-driven service structure and MVVM separation intact.
   - Expose toolkit-backed operations through repository/service abstractions so ViewModels remain unaware of toolkit specifics.

## Environment & Tooling Constraints
- Toolkit assemblies are staged in `docs/Visual Files/ReferenceFiles/` (`Database.config`, `LsaCore.dll`, `LsaShared.dll`, `VmfgFinancials.dll`, `VmfgInventory.dll`, `VmfgPurchasing.dll`, `VmfgSales.dll`, `VmfgShared.dll`, `VmfgShopFloor.dll`, `VmfgTrace.dll`). Use this folder as the canonical source when wiring build/package steps.
- Toolkit assemblies target .NET Framework 4.x (32-bit). Create an interop boundary:
  - Preferred: isolate toolkit calls inside a dedicated .NET 4.8 x86 worker (Windows Service or console host) and communicate via named pipes/gRPC.
  - Alternate (if platform matrix allows): use AnyCPU launcher with `Prefer32Bit` for Windows builds and spawn toolkit operations on a separate AppDomain.
- Ship toolkit DLLs and `Database.Config` alongside the desktop app (respect Infor licensing rules).
- Update build scripts/tasks to copy toolkit binaries into output directories while remaining excluded from source control if proprietary.

## Implementation Blueprint
1. **Audit Current Integration Layer**
   - Catalog all existing services making HTTP calls to VISUAL.
   - Map each endpoint to corresponding toolkit class/method.
2. **Create Toolkit Interop Module**
   - New project: `MTM_Template_Application.Integration.VisualToolkit` (target net48, x86).
   - Reference all required toolkit assemblies and encapsulate `DbmsFactory`, session pooling, and authentication.
   - Implement adapters for frequently used business documents (e.g., Part, Customer, WorkOrder).
3. **Session & Transaction Management**
   - Centralize connection handling with retry and timeout policies (Polly can wrap interop boundary if accessible).
   - Ensure each business action follows the pattern: open session → instantiate business object → load/prepare → set fields → validate → save → dispose.
4. **Replace HTTP Gateways**
   - For each existing HTTP-based repository, substitute calls with toolkit-backed operations via new interop services.
   - Maintain DTOs but populate them from toolkit datasets/rows.
5. **Error Handling & Logging**
   - Translate toolkit exceptions using `ExceptionManager`/`ExceptionInfo` into domain-specific errors.
   - Surface actionable messages to UI while preserving stack traces in logs.
6. **Configuration & Secrets**
   - Extend configuration service to capture VISUAL server, database instance, site, and licensing mode.
   - Integrate with secrets service for username/password or SSO tokens.
7. **Testing Strategy**
   - Unit-test adapters with mocks that emulate toolkit interfaces.
   - Add integration tests guarded by `[SkippableFact]` that run against a VISUAL test database (use environment variable to gate execution).
   - Provide smoke-test harness enabling QA to validate core transactions (part query, work order issue, shipment booking).
8. **Deployment Adjustments**
   - Update installers or CI packaging to include toolkit binaries.
   - Document prerequisites: Visual client runtime, OLE DB providers, 32-bit dependency requirements.

## Acceptance Criteria
- No remaining HTTP calls to VISUAL endpoints; all replaced with toolkit integration.
- Toolkit session management passes code review for resource safety and nullability compliance.
- Critical workflows (inventory lookup, work order issuance, shipment confirmation) operate through toolkit adapters with automated tests.
- Configuration UI/CLI supports specifying VISUAL connection parameters and persists them via existing services.
- Observability: logs capture session open/close, transaction IDs, and error categories without leaking credentials.

## Risks & Mitigations
- **x86 Toolkit Dependency**: Document and enforce Windows-only execution path or provide cross-platform shim instructions.
- **Licensing/Distribution**: Coordinate with stakeholders to store toolkit DLLs securely and exclude them from public repos.
- **Learning Curve**: Provide developer onboarding notes referencing `Guide - User Manual`, `Reference - Core`, `Reference - Inventory`, `Reference - Shared Library`, and `Reference - Shop Floor`.

## Reference Material
- `docs/Visual Files/Guides/Guide - User Manual.txt`
- `docs/Visual Files/Guides/Reference - Core.txt`
- `docs/Visual Files/Guides/Reference - Development Guide.txt`
- `docs/Visual Files/Guides/Reference - Inventory.txt`
- `docs/Visual Files/Guides/Reference - Shared Library.txt`
- `docs/Visual Files/Guides/Reference - Shop Floor.txt`
