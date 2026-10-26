# SCIM Provisioning Validation — Test Overview

## Purpose

The SCIM Validation Logic App runs **25 automated tests** against an ISV's SCIM 2.0 endpoint to verify it is ready for publication in the Microsoft Entra app gallery. Tests cover the full provisioning lifecycle — user and group CRUD, attribute mappings, soft delete, provision-on-demand, direct SCIM compliance, credential validation, and federated identity.

The Logic App is deployed as a Standard Azure Logic App with 5 workflows that execute in parallel, completing a full validation run in 30–60 minutes.

---

## Architecture

```
Orchestrator_Workflow (entry point)
 ├── Initialization_Workflow     — reads sync schema, builds dynamic test bodies
 ├── UserTests_Workflow          — 7 tests (parallel with Group/SCIM)
 ├── GroupTests_Workflow         — 7 tests (parallel with User/SCIM)
 └── SCIMTests_Workflow          — 11 tests (parallel with User/Group)
      └── Final_TestResults      — aggregates 25 results, determines pass/fail
```

**Dynamic capability detection:** The Initialization workflow reads the provisioning schema and automatically determines which tests apply based on the ISV's attribute mappings:

| Capability | How detected | Tests affected |
|------------|-------------|----------------|
| Group support | Group schema (`urn:ietf:params:scim:schemas:core:2.0:Group`) in `/Schemas` response | 7 group-related tests + SCIM_Group_Create/Update/Pagination |
| Manager support | `manager` attribute in User mappings / target directory attributes | User_Update_Manager_Test, SCIM_Update_Manager_Test |
| Soft delete support | `active` attribute in User mappings | Disable_User_Test (always runs — mandatory) |

Tests that don't apply are reported as **SKIPPED** (not failures). However, group tests report **FAILED** (not SKIPPED) if `/Schemas` doesn't return the Group schema — group provisioning is mandatory.

---

## Test Inventory (25 Tests)

### User Provisioning Tests (UserTests_Workflow) — 7 tests

| # | Test Name | Category | What It Validates | Pass Criteria |
|---|-----------|----------|-------------------|---------------|
| 1 | **Create_User_Test** | mandatory | Creates a user in Entra ID, assigns app role, triggers provisioning cycle, verifies user appears on SCIM endpoint. | Provisioning audit log with `provisioningAction=Create` and `status=success` found; user found on SCIM via `GET /Users?filter=userName eq "..."`. |
| 2 | **Update_User_Test** | mandatory | Modifies mapped attributes (e.g., jobTitle, department) on a provisioned user in Entra ID, triggers sync, verifies updates on SCIM. | Provisioning audit log with `provisioningAction=Update` and `status=success` found; all updated attribute values match on SCIM endpoint. |
| 3 | **Disable_User_Test** | mandatory | Sets `accountEnabled=false` on a provisioned user, triggers sync, verifies SCIM receives `active: false`. Always runs — soft-delete support is required. | Provisioning audit log with `provisioningAction=Disable` and `status=success` found. |
| 4 | **Delete_User_Test** | optional | Soft-deletes user from Entra, purges from recycle bin, triggers sync, verifies user removed from SCIM. Failure = WARNING (does not block overall pass). | Provisioning audit log with `provisioningAction=Delete` and `status=success` found; user no longer on SCIM (404 or `totalResults=0`). |
| 5 | **User_Update_Manager_Test** | mandatory | Three-phase test: (1) assign manager, (2) change to different manager, (3) remove manager. Verifies provisioning logs capture each manager change. **Skipped** if no `manager` attribute mapping. | Provisioning logs found for each phase with `modifiedProperties` containing `manager`; `newValue` matches expected manager ID on set/change; `newValue` is empty on remove. |
| 6 | **Restore_User_Test** | mandatory | Three-phase unassign/reassign lifecycle: create+provision user (Phase 1), remove app role assignment to trigger deprovision (Phase 2), re-assign app role to re-provision (Phase 3). | Phase 1: Create provisioning log with `status=success`, user on SCIM. Phase 2: Delete provisioning log with `status=success`, user gone from SCIM. Phase 3: Create (restore) provisioning log with `status=success`, user back on SCIM. Result includes `(SoftDeleteRestore)` or `(UnassignReassign)` tag. |
| 7 | **POD_User_Test** | mandatory | Tests Provision on Demand — creates user, assigns to app, calls `POST /servicePrincipals/{id}/synchronization/jobs/{jobId}/provisionOnDemand`, verifies user appears on SCIM without waiting for sync cycle. | `provisionOnDemand` returns HTTP 200; provisioning logs confirm success; user found on SCIM endpoint. |

### Group Provisioning Tests (GroupTests_Workflow) — 7 tests

| # | Test Name | Category | What It Validates | Pass Criteria |
|---|-----------|----------|-------------------|---------------|
| 8 | **Create_Group_Test** | mandatory | Creates a group in Entra ID, assigns to app, triggers sync, verifies group on SCIM. **FAILED** (not skipped) if `/Schemas` doesn't return Group schema. | Provisioning audit log with `provisioningAction=Create` and `status=success`; group found on SCIM via `GET /Groups?filter=displayName eq "..."`. |
| 9 | **Update_Group_Test** | mandatory | Modifies group displayName/description, triggers sync, verifies update propagates to SCIM. | Provisioning audit log with `provisioningAction=Update` and `status=success`; updated displayName found on SCIM. |
| 10 | **Delete_Group_Test** | optional | Deletes group from Entra, verifies removal from SCIM. Failure = WARNING. | Provisioning audit log with `provisioningAction=Delete` and `status=success`; group returns `totalResults=0` on SCIM. |
| 11 | **Group_Update_Add_Member_Test** | mandatory | Adds a user member to a provisioned group, verifies SCIM receives PATCH with member addition. | Provisioning log with `provisioningAction=Update` and `modifiedProperties` containing `members`; `newValue` contains the user ID; `oldValue` is empty. |
| 12 | **Group_Update_Remove_Member_Test** | mandatory | Removes a member from a group, verifies SCIM receives the member removal. | Provisioning log with `modifiedProperties` containing `members`; `oldValue` contains the member ID; `newValue` is empty/null. |
| 13 | **POD_Group_Test** | mandatory | Tests Provision on Demand for groups — calls `provisionOnDemand`, verifies group on SCIM without sync cycle. **FAILED** (not skipped) if groups not supported. | `provisionOnDemand` returns HTTP 200; provisioning logs confirm success; group found on SCIM. |
| 14 | **Restore_Group_Test** | optional | Unassign/reassign lifecycle for groups (same 3-phase pattern as Restore_User_Test). Failure = WARNING. | Phase 1: Create+group on SCIM. Phase 2: Delete+group gone. Phase 3: Re-create+group back. Result tagged `(UnassignReassign)`. |

### SCIM Compliance Tests (SCIMTests_Workflow) — 11 tests

| # | Test Name | Category | What It Validates | Pass Criteria |
|---|-----------|----------|-------------------|---------------|
| 15 | **Schema_Discoverability_Test** | mandatory | Calls `GET /Schemas`, flattens response into attribute name strings (e.g., `emails[type eq "work"].value`), compares against target directory attributes from provisioning schema. | HTTP 200 from `/Schemas`; response contains `Resources` array; all mapped user and group attributes discoverable with case-insensitive matching; zero missing attributes. |
| 16 | **SCIM_Null_Update_Test** | mandatory | Two-phase null/empty handling: (1) PATCH with empty strings on optional attributes, verify they become null/empty; (2) PATCH with null values, verify they remain null/empty. | User created successfully; Phase 1 PATCH returns 200/204 and all target attributes are null/empty in verification GET; Phase 2 PATCH returns 200/204 and attributes remain null/empty; zero failed verifications. |
| 17 | **SCIM_User_Create_Test** | mandatory | Direct `POST /Users` with well-formed SCIM body (bypasses Entra provisioning engine). | HTTP 200 or 201; response contains non-empty `id`; returned `userName` matches sent `userName` (round-trip validation). |
| 18 | **SCIM_User_Update_Test** | mandatory | Direct `PATCH /Users/{id}` with auto-generated update values for eligible attributes (single-valued, string/integer/datetime/boolean, excluding groups/roles/id/schemas/meta/active). | PATCH returns 200/204; subsequent GET returns 200/204; all PATCHed attribute values match in the response; zero mismatches. Passes with `(no updatable attributes)` if no eligible attributes exist. |
| 19 | **SCIM_Group_Create_Test** | mandatory | Direct `POST /Groups` with SCIM group body. **FAILED** if groups not supported. | HTTP 200 or 201; response contains non-empty `id`; returned `displayName` matches sent `displayName`. |
| 20 | **SCIM_Group_Update_Test** | mandatory | Direct `PATCH /Groups/{id}` with attribute updates (single-valued, non-reference, excluding id/members/schemas/meta). | PATCH returns 200/204; GET returns 200/204; all PATCHed attributes match; zero mismatches. |
| 21 | **Validate_Credentials_Test** | mandatory when selected | OAuth 2.0 Client Credentials flow — acquires a token from the ISV's `scimTokenEndpoint`, sends credentials in the required `Header` or `Body` location, includes the optional space-delimited scope string, then validates SCIM access with the token. **Skipped** when this method is not selected. | Token endpoint returns 2xx with `access_token`; token validity between 60–360 minutes; SCIM endpoint returns 2xx when called with the token. |
| 22 | **Federated_Identity_Test** | mandatory when selected, except Google below | Workload identity federation — acquires Entra ID token, then exchanges it for ISV token via one of three flows: Google Service Account, Google STS, or generic federated endpoint. **Skipped** when unselected or covered by the temporary Google federation exception below. | Entra token acquisition returns 2xx with `access_token`; federated token exchange returns 2xx with valid token. Supports Google SA flow (STS exchange → SA impersonation), Google STS flow (direct exchange), and generic flow (client assertion). For the Google exception, diagnostics stay SKIPPED and only the aggregate authentication result is marked PASSED. |
| 23 | **SCIM_User_Pagination_Test** | mandatory, except Google below | Ensures ≥11 users exist (creates throwaway users if needed), then paginates `GET /Users?startIndex=N&count=5` across pages. Cleans up created users afterward. **Skipped** for Google endpoints. | At least 2 pages traversed; all pages return HTTP 200; response `startIndex` matches expected value (`page * 5 + 1`); zero failed page verifications. Google skips use the aggregate exception below. |
| 24 | **SCIM_Group_Pagination_Test** | optional | Same as user pagination but for `/Groups`. Failure = WARNING. **Skipped** if groups not supported or for Google endpoints. | At least 2 pages traversed; all pages return HTTP 200; response `startIndex` matches expected; zero failed verifications. Google skips use the aggregate exception below. |
| 25 | **SCIM_Update_Manager_Test** | mandatory | Direct SCIM PATCH test for manager attribute: set (Add), change (Replace), remove (Replace with empty). **Skipped** if `manager` not in target directory attributes. | Each PATCH returns 200/204; GET after each operation confirms manager value matches expected; all three operations (set/change/remove) succeed. |

---

## Test Categories Summary

The post-create and post-PATCH propagation waits in `SCIMTests_Workflow.json` are 30 seconds before dependent operations or verification reads, except `Delay_SCIM_Update_User_After_Patch`, which waits 90 seconds before verifying the direct user update. The longer wait accommodates observed propagation delays of approximately 60-65 seconds under concurrent full-suite load. Other null-update, group-update, and manager-update waits remain unchanged.

For connections whose saved `baseAddress` contains `googleapis.com` (case-insensitive), initialization reads `/Schemas` and identifies mapped required attributes and subattributes, also honoring `targetSchema.required`. `SCIM_Null_Update_Test` excludes those targets from both empty-string and null PATCH payloads and their verification lists. For example, a required `emails[type eq "other"].value` is not cleared; optional sibling fields and optional address or enterprise-extension fields remain eligible. Clearing a whole complex attribute is also excluded when it contains a subattribute declared required by either `/Schemas` or another mapping's `targetSchema.required`, even if the parent is optional. The same parent protection applies to blank mapped sources in the Entra update payload. Schema URNs are matched separately, so a required extension attribute does not exclude an optional core attribute with the same name.

The Google Entra update payload is protected separately: blank/null source values (including blank array entries) mapped to required targets are omitted, while populated updates, `false`, and `0` are retained. Removing the last mapped field from a nested object also removes its now-empty parent; populated siblings and unrelated objects are preserved. Omitted sources are listed in initialization's `googleSkippedUpdateSources` and excluded from update-value comparisons. If filtering removes all properties from a previously nonempty Google update body, initialization records `googleUpdateBodyEmptiedByFiltering`. After successful create/provisioning checks, `Update_User_Test` reports `PASSED` by this exception, with its existing result message explaining that no update PATCH or update verification was executed. An originally empty, null, or otherwise unqualified Google update body still fails validation before Graph PATCH; create/provisioning failures are never exempted. Other providers retain their previous update-request behavior, including sending an empty body if one is produced. This changes the validation workflow's Graph PATCH, not the provisioning service's saved mappings or future production behavior. Manager-removal lifecycle phases are unchanged.

If the Google null test has no eligible targets, neither clearing PATCH is sent. Its summary explicitly reports `clearingPhasesSkipped`, the reason, excluded required targets, and zero verified attributes. Successful setup can still pass; HTTP or verification failures are not exempted. Other providers retain their existing attribute-selection behavior.

| Category | Tests | Exercises |
|----------|-------|-----------|
| **User Lifecycle** | 7 | Full CRUD + Manager + Restore + POD via Entra provisioning engine |
| **Group Lifecycle** | 7 | Full CRUD + Membership + POD + Restore via Entra provisioning engine |
| **SCIM Direct Compliance** | 9 | Direct HTTP calls to SCIM endpoint — schema, CRUD, null update, user & group pagination, manager set/change/remove |
| **Authentication** | 2 | OAuth client credentials flow + workload identity federation |
| **Total** | **25** |

---

## What "Passing" Means

### Version and validation timing

`Final_TestResults` includes the deployed `LogicAppVersion`, derived during
deployment from the same bundle's VERSION file. It also includes
`validationStartedAtUtc` and `validationCompletedAtUtc`, their corresponding
`validationStartedAtEpochSeconds` and `validationCompletedAtEpochSeconds`,
and `validationDurationSeconds`. UTC timestamps are captured once at the first
Compose and at result evaluation; numeric values are derived from those captures.
Epoch seconds count from 1970-01-01 UTC, and duration is elapsed whole seconds.
These fields are not Azure's authoritative run-history start/end times and do
not affect pass/fail. Runs that never reach the final action have no final JSON.

### Overall Result Logic

The Orchestrator's `Evaluate_Test_Results` action computes `overallLogicAppResult` as follows:

- **`Succeeded`** — Existing mandatory/capability gates pass, AND the authentication model result is `PASSED` (selected diagnostic success or the temporary Google federation exception below).
- **`Failed`** — A required semantic gate does not pass, OR the selected authentication test is failed, skipped, or missing outside the Google exception, or the selection is invalid.

### Authentication Model (one selected method)

Set `selectedAuthenticationMethod` to one of the following values. Existing token-exchange actions are reused; the unselected test stays `SKIPPED` and makes no token request.

| Selection | Required test | Authentication result |
|-----------|---------------|-----------------------|
| `OAuth2ClientCredentials` | `Validate_Credentials_Test` | PASSED only when this test passes |
| `FederatedIdentity` | `Federated_Identity_Test` | PASSED when this test passes, or by the temporary Google exception below |
| Empty/invalid | Neither | FAILED |

Final results identify the selection. Outside the temporary Google exception, success of the other method cannot rescue a failed or skipped selected test. Existing non-authentication requirements remain unchanged.

#### Temporary Google workload identity exception

When the saved connection's `baseAddress` contains `googleapis.com` (case-insensitive) **and** `selectedAuthenticationMethod` is `FederatedIdentity`, both standalone authentication diagnostics remain `SKIPPED` and make no token requests. The aggregate `authenticationModelResult` is temporarily marked `PASSED` by exception. Child and final orchestrator results expose `authenticationModelBypassed: true` and `authenticationModelBypassReason` so this is distinguishable from a successful diagnostic.

The standalone diagnostic acquires a tenant-app token; it cannot represent Microsoft's first-party SyncFabric client identity when Google restricts the token's authorized party (`azp`). This exception does not relax that provider restriction or change the real connection. Existing mandatory Entra provisioning, provisioning-log, and SCIM checks must still pass for overall success. A passing aggregate authentication result alone does not establish E2E success or prove a fresh STS token exchange occurred.

Google connections selecting `OAuth2ClientCredentials`, non-Google connections, missing base addresses, and invalid method selections do not receive this exception. Standalone tests have not been made globally optional.

#### Google pagination aggregate exception

For saved connection base addresses containing `googleapis.com` (case-insensitive), an enabled pagination test explicitly reporting `SKIPPED` is accepted as `PASSED` in `paginationModelResults`. User pagination uses this aggregate for overall readiness; group pagination uses it for the optional-success aggregate. The individual test results remain `SKIPPED`, and the aggregate includes `googleSkipExceptionReason`.

This applies only when `EnabledTests` includes the respective pagination test (`All`, `SCIMTests`, or its individual selector). Customer-excluded tests, failed or missing results, and non-Google skips are not promoted. Other mandatory gates and child workflow failures still block overall success. Modified Google update/null checks continue to use their applicable validations, not a blanket skip exemption. Developer portal acceptance of these exceptions is not established by the local workflow.

`FederatedIdentity` is the selector name for Workload Identity Federation (WIF), matching the existing test/output naming. Selection does not reroute the other tests' credentials: direct semantic requests still use `scimBearerToken`; provisioning requests use the separately saved Entra connection. Tokens acquired inside either authentication test remain local to that test.

### Optional Tests (WARNING, not FAIL)

These 4 tests are `testCategory: optional`. When they fail, the `testResult` field shows `WARNING` instead of `FAILED`, and they do **not** block the overall `Succeeded` result:

| Test | Why optional |
|------|-------------|
| Delete_User_Test | Hard delete support varies by ISV |
| Delete_Group_Test | Hard delete support varies by ISV |
| Restore_Group_Test | Group unassign/reassign lifecycle |
| SCIM_Group_Pagination_Test | Group pagination is recommended but not required |

### Mandatory Tests That Can Be SKIPPED

| Test | Skip condition | Treated as |
|------|---------------|------------|
| User_Update_Manager_Test | `isManagerAttributeSupported = false` | SKIPPED (pass) |
| SCIM_Update_Manager_Test | `manager` not in target directory attributes | SKIPPED (pass) |
| Validate_Credentials_Test | Federated identity selected | Expected SKIPPED; selected federation must pass |
| Federated_Identity_Test | OAuth selected | Expected SKIPPED; selected OAuth must pass |

### Group Tests Are Mandatory

Unlike individual capability-dependent tests, group tests report **FAILED** (not SKIPPED) when `/Schemas` doesn't return the Group schema (`urn:ietf:params:scim:schemas:core:2.0:Group`). The error message is:

> `FAILED - [Initialization Phase] Schema_Discovery - /Schemas did not return Group schema. Group provisioning is mandatory.`

This applies to: Create_Group, Update_Group, Group_Update_Add_Member, Group_Update_Remove_Member, POD_Group, SCIM_Group_Create, SCIM_Group_Update.

### Decision Table

| Scenario | Overall Result | Action |
|----------|---------------|--------|
| All 25 tests: `success` | **Succeeded** | Ready for gallery submission |
| All mandatory pass, optional tests show `WARNING` | **Succeeded** | Ready — warnings are informational |
| OAuth selected + Validate_Credentials passes, Federated_Identity skipped | **Succeeded** | Ready if other required tests pass |
| Federated configured + Federated_Identity passes, Validate_Credentials skipped | **Succeeded** | Ready — only one auth test needed |
| Selected authentication test fails/skips or its result is missing | **Failed** | Must fix the selected method |
| Manager tests `SKIPPED` (no manager mapping) | **Succeeded** | Ready — ISV should document no manager support |
| Disable_User_Test `FAILED` | **Failed** | Must implement soft-delete (`active: false`) on SCIM endpoint |
| Group tests `FAILED` (no Group schema) | **Failed** | Must add Group support to SCIM endpoint |
| Any mandatory test `FAILED` | **Failed** | Must fix failing test(s) before submission |

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

*Document version: August 2026 — Covers Logic App validation template v5 with 25 tests across 5 workflows. Includes User & Group Pagination, Restore tests, Schema_Discoverability_Test v2, SCIM_Update_Manager_Test, Federated_Identity_Test, Validate_Credentials_Test, scimTargetUserValues, and Provision on Demand.*
