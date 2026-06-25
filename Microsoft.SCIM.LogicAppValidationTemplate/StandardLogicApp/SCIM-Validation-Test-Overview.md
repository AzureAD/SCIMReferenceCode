# SCIM Provisioning Validation — Test Overview

## Purpose

The SCIM Validation Logic App runs **23 automated tests** against an ISV's SCIM 2.0 endpoint to verify it is ready for publication in the Microsoft Entra app gallery. Tests cover the full provisioning lifecycle — user and group CRUD, attribute mappings, soft delete, provision-on-demand, direct SCIM compliance, and credential validation.

The Logic App is deployed as a Standard Azure Logic App with 5 workflows that execute in parallel, completing a full validation run in 30–60 minutes.

---

## Architecture

```
Orchestrator_Workflow (entry point)
 ├── Initialization_Workflow     — reads sync schema, builds dynamic test bodies
 ├── UserTests_Workflow          — 7 tests (parallel with Group/SCIM)
 ├── GroupTests_Workflow         — 7 tests (parallel with User/SCIM)
 └── SCIMTests_Workflow          — 9 tests (parallel with User/Group)
      └── Final_TestResults      — aggregates 23 results, determines pass/fail
```

**Dynamic capability detection:** The Initialization workflow reads the provisioning schema and automatically determines which tests apply based on the ISV's attribute mappings:

| Capability | How detected | Tests affected |
|------------|-------------|----------------|
| Group support | Group object mapping enabled in sync rules | 7 group-related tests |
| Manager support | `manager` attribute in User mappings | User_Update_Manager_Test |
| Soft delete support | `active` attribute in User mappings | Disable_User_Test |

Tests that don't apply are reported as **SKIPPED** (not failures).

---

## Test Inventory (22 Tests)

### User Provisioning Tests (UserTests_Workflow) — 7 tests

| # | Test Name | What It Validates |
|---|-----------|-------------------|
| 1 | **Create_User_Test** | Creates a user in Entra ID, triggers a provisioning cycle, and verifies the user appears on the SCIM endpoint via `GET /Users?filter=userName eq "..."`. Validates the full create pipeline: Entra → Provisioning Engine → SCIM POST /Users → verification. |
| 2 | **Update_User_Test** | Modifies mapped attributes (e.g., jobTitle, department) on an existing provisioned user in Entra ID, triggers a sync cycle, and verifies the updated values are reflected on the SCIM endpoint via PATCH. |
| 3 | **Disable_User_Test** | Sets `accountEnabled=false` on a provisioned user in Entra ID, triggers a sync cycle, and verifies the SCIM endpoint receives `active: false`. **Skipped** if the ISV's schema has no `active` attribute mapping. |
| 4 | **Delete_User_Test** | Deletes a provisioned user from Entra ID, triggers a sync cycle, and verifies the user is removed from the SCIM endpoint (hard delete via DELETE /Users/{id} or soft delete depending on configuration). |
| 5 | **User_Update_Manager_Test** | Assigns a manager to a provisioned user and verifies the SCIM endpoint receives the manager reference update. **Skipped** if the ISV's schema has no `manager` attribute mapping. |
| 6 | **Restore_User_Test** | Validates the unassign → reassign lifecycle. Creates and provisions a user (Phase 1), unassigns the app role to trigger deprovisioning — verifies the user disappears from the SCIM endpoint (Phase 2), then reassigns the app role and verifies the user is re-provisioned (Phase 3). Checks whether the SCIM identity link is preserved across the cycle. |
| 7 | **POD_User_Test** | Tests **Provision on Demand** — creates a user, assigns them to the app, calls the Graph `provisionOnDemand` API, and verifies the user appears on the SCIM endpoint without waiting for a full sync cycle. |

### Group Provisioning Tests (GroupTests_Workflow) — 7 tests

| # | Test Name | What It Validates |
|---|-----------|-------------------|
| 8 | **Create_Group_Test** | Creates a group in Entra ID, assigns it to the app, triggers a sync cycle, and verifies the group appears on the SCIM endpoint via `GET /Groups?filter=displayName eq "..."`. |
| 9 | **Update_Group_Test** | Modifies group attributes (e.g., displayName) and verifies the change propagates to the SCIM endpoint. |
| 10 | **Delete_Group_Test** | Removes a group and verifies it is deleted from the SCIM endpoint. |
| 11 | **Group_Update_Add_Member_Test** | Adds a user as a member of a provisioned group and verifies the SCIM endpoint receives a PATCH with the member addition. Validates multi-member PATCH support. |
| 12 | **Group_Update_Remove_Member_Test** | Removes a member from a group and verifies the SCIM endpoint receives the member removal PATCH. |
| 13 | **POD_Group_Test** | Tests **Provision on Demand** for groups — creates a group, assigns it to the app, calls `provisionOnDemand`, and verifies the group appears on the SCIM endpoint. |
| 14 | **Restore_Group_Test** | Unassigns a group from the provisioning app, triggers a sync cycle (group should be deleted from SCIM endpoint), then reassigns and re-provisions. Validates that the SCIM endpoint handles group re-creation correctly. Includes a 30-second delay before AppRole reassignment. |

> **Note:** All 7 group tests are **skipped** if the ISV's schema does not have an enabled Group object mapping.

### SCIM Compliance Tests (SCIMTests_Workflow) — 9 tests

| # | Test Name | What It Validates |
|---|-----------|-------------------|
| 15 | **Schema_Discoverability_Test** | Calls `GET /Schemas` on the SCIM endpoint and flattens the response into attribute name strings (e.g., `emails[type eq "work"].value`, `name.givenName`, `roles[primary eq "True"].value`). Compares against the target directory attributes from the provisioning job schema. Reports any missing attributes. Uses case-insensitive pipe-delimited matching. |
| 16 | **SCIM_Null_Update_Test** | Sends a PATCH request that sets an attribute to `null` and verifies the SCIM endpoint handles null/empty attribute updates without error (HTTP 200). This is a common compliance gap. |
| 17 | **SCIM_User_Create_Test** | Directly calls `POST /Users` on the SCIM endpoint (bypassing the Entra provisioning engine) with a well-formed SCIM user body built from the ISV's schema. Verifies HTTP 201 and a valid response body. |
| 18 | **SCIM_User_Update_Test** | Directly calls `PATCH /Users/{id}` on the SCIM endpoint with attribute updates. Update values are auto-generated (`upd-{guid}`) unless overridden via `scimTargetUserValues[1]`. Verifies the endpoint accepts standard SCIM PATCH operations. |
| 19 | **SCIM_Group_Create_Test** | Directly calls `POST /Groups` with a SCIM group body. **Skipped** if groups are not supported. |
| 20 | **SCIM_Group_Update_Test** | Directly calls `PATCH /Groups/{id}` with attribute updates. **Skipped** if groups are not supported. |
| 21 | **SCIM_User_Pagination_Test** | Ensures ≥11 users exist on the endpoint (creates throwaway users from `initializationData.scimUserBody` if needed), then paginates `/Users?startIndex=N&count=5` across multiple pages. Verifies `startIndex`, `totalResults`, and page traversal. Cleans up created users afterward. |
| 22 | **SCIM_Group_Pagination_Test** | Ensures ≥11 groups exist on the endpoint (creates throwaway groups from `initializationData.scimGroupBody` if needed), then paginates `/Groups?startIndex=N&count=5` across multiple pages. Verifies `startIndex`, `totalResults`, and page traversal. Cleans up created groups afterward. **Skipped** if groups are not supported. |
| 23 | **Validate_Credentials_Test** | Tests the OAuth 2.0 Client Credentials flow — acquires a token from the ISV's token endpoint using client ID/secret, then validates the SCIM connection. **Skipped** when `scimTokenEndpoint` is empty (static bearer token setup). |

---

## Test Categories Summary

| Category | Tests | Exercises |
|----------|-------|-----------|
| **User Lifecycle** | 7 | Full CRUD + Manager + Restore + POD via Entra provisioning engine |
| **Group Lifecycle** | 7 | Full CRUD + Membership + POD + Restore via Entra provisioning engine |
| **SCIM Direct Compliance** | 8 | Direct HTTP calls to SCIM endpoint — schema, CRUD, null update, user & group pagination |
| **Credential Validation** | 1 | OAuth client credentials flow |
| **Total Scored** | **23** |

---

## What "Passing" Means

| Scenario | Acceptable? |
|----------|-------------|
| All 23 tests: `success` | **Ready for gallery submission** |
| Group tests: `SKIPPED` (no group mapping) | Acceptable if ISV only supports /Users |
| Disable_User_Test: `SKIPPED` (no `active` mapping) | Acceptable — ISV should document |
| Manager test: `SKIPPED` (no `manager` mapping) | Acceptable — ISV should document |
| Validate_Credentials_Test: `SKIPPED` | Expected with static bearer token — no OAuth configured |
| Schema_Discoverability_Test: `FAILED` with missing attrs | Must add missing attributes to `/Schemas` response |
| Pagination tests: `FAILED` | Must implement pagination per RFC 7644 §3.4.2.4 |
| Any test: `FAILED` | **Must fix before submission** |

---

## How to Run

### Method A: AI Agent (Automated, 30–60 min)
Load `scim-onboarding.agent.md` into any AI coding agent (VS Code Copilot, Cursor, Claude Code, etc.) and send: *"Validate my SCIM integration."* The agent handles everything conversationally.

### Method B: Manual Setup (1–3 hours)
Follow the step-by-step instructions in `SetupLogicApp-Standard-Agent.docx` to manually create resources, deploy the Logic App, configure parameters, and trigger tests.

Both methods produce the same output: a `validation-result-<RunId>.json` file to submit to Microsoft at [aaduserprovisioning@microsoft.com](mailto:aaduserprovisioning@microsoft.com).

---

## Key Onboarding Requirements Validated

- SCIM 2.0 user endpoint (group endpoint recommended)
- Filter queries on matching properties return 200 (not 404)
- Empty filter queries return 200 + empty results
- Multi-member PATCH on /Groups (if groups supported)
- ≥25 requests/second throughput
- OAuth 2.0 Client Credentials for production (static token accepted for pilot)

---

*Document version: June 2026 — Covers Logic App validation template v4 with 23 tests across 5 workflows. Includes User & Group Pagination, Restore tests, Schema_Discoverability_Test v2 with flatten loops, scimTargetUserValues, and Provision on Demand.*
