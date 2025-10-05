# Troubleshooting Catalog

> **Purpose:** Speed up resolution for operators and IT.
> **Usage:** Reference when issues arise or abnormal behavior occurs.
> **Dependencies:** Logging, health checks, diagnostics, telemetry.
> **Priority:** Medium-High

---

## For AI Agents

- **Intent:** Provide a concise symptom → cause → action table for Visual Toolkit connectivity, MySQL/API, scanning, printing, and cache issues.
- **Dependencies:** Telemetry, health endpoints.
- **Consumers:** Support teams, operators.
- **Non-functionals:** Clear language, short actionable steps, links to detailed logs.

---

## Troubleshooting Sections

### Visual Offline / Toolkit Error

| Symptom                         | Cause                      | Action                                                                                         |
| ------------------------------- | -------------------------- | ---------------------------------------------------------------------------------------------- |
| Visual error, offline banner    | Credentials not validated  | Check `APPLICATION_USER.NAME` (nvarchar(20)), validate locally                                 |
| Toolkit endpoint unreachable    | Network/firewall/allowlist | Confirm endpoint reachability, review allowlist (`PART`: 30-char max, `LOCATION`: 15-char max) |
| Data not updating               | Using stale cache          | Use cached master data (check last sync timestamp in banner)                                   |
| Write attempts to Visual tables | Read-only access required  | Verify no writes to Visual tables (`PART`, `LOCATION`, `WAREHOUSE`, `SHOP_RESOURCE`, `SITE`)   |
| Foreign key errors              | Cache mismatch             | Check FK cache: `LOCATION→WAREHOUSE` (FK Line: 427), `PART_LOCATION` (FK Lines: 459-460)       |
| Projection fails                | Table/field mismatch       | Notify IT with specific table/field reference                                                  |

---

### MySQL Unavailable

| Symptom                 | Cause                        | Action                                                      |
| ----------------------- | ---------------------------- | ----------------------------------------------------------- |
| Cannot connect to MySQL | MAMP not running, wrong port | Confirm MAMP running, port 3306, check MySQL service status |
| API tests fail          | Firewall or endpoint issue   | Switch to API for tests, review firewall rules              |
| Wrong database targeted | Misconfiguration             | Verify local app database (not Visual) is the target        |

---

### Android Cannot Reach API

| Symptom                       | Cause                   | Action                                                              |
| ----------------------------- | ----------------------- | ------------------------------------------------------------------- |
| API unreachable from emulator | Wrong IP, TLS, firewall | Use `10.0.2.2` for emulator, check TLS certificates, firewall rules |

---

### Scanning Fails

| Symptom          | Cause                     | Action                                                                  |
| ---------------- | ------------------------- | ----------------------------------------------------------------------- |
| Barcode not read | Symbology/camera/lighting | Confirm symbology, camera permission, ambient lighting, hardware status |

---

### Printing Fails

| Symptom         | Cause                                  | Action                                                                              |
| --------------- | -------------------------------------- | ----------------------------------------------------------------------------------- |
| Print job fails | Printer offline, wrong language/driver | Ensure printer online, correct language (ZPL), driver settings, test-print template |

---

## Clarification Questions

- **Q:** Allow operators to send diagnostic bundles to IT?
  - **Why:** Faster support.
  - **Suggested:** Yes, with consent and redaction.
  - **Reason:** Balance privacy and speed.
  - **Options:** [A] Yes [B] No

---

> **Formatting Standards:**
>
> - Use semantic headings and tables for clarity.
> - Avoid hardcoded values; reference schema and field names.
> - Link to logs and diagnostics where possible.
> - Ensure cross-platform terminology (Windows desktop, Android; macOS/Linux deferred).
> - Use concise, actionable steps.
