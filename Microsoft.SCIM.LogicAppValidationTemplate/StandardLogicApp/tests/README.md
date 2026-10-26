# Local authentication-selection regression tests

## Purpose and scope

[minimal-auth-selection.test.cjs](minimal-auth-selection.test.cjs) checks the
single-authentication-selection behavior and Google exception reporting in the
actual workflow JSON files.
It is developer verification, not a deployed workflow or a prerequisite for an
ISV's cloud validation run.

The tests do not contact Azure, Microsoft Graph, an authorization server or a
SCIM endpoint. They do not require credentials, create resources or run
provisioning. Passing these checks is not proof of a successful live OAuth or
federation exchange, or full gallery readiness.

## Prerequisites

- Node.js with the built-in `node:test` runner and `structuredClone`. Node.js
  22 or later is recommended. No npm packages or `npm install` are required.
- Git available on `PATH`.
- PowerShell 7 (`pwsh`) available on `PATH` for the offline deployment tests.
  This is a test/deployment tool, not a dependency of the deployed workflow.
- A repository checkout or worktree with the merged Google baseline commit
  `fde52cc606880c703e01d14ab7d36323a872b2e7` and the earlier
  `8bdf02e700b8c4e667c6eee6924010274a58af8f` collector baseline available in its local history.
  A downloaded source ZIP or shallow clone missing that commit is insufficient
  for the preservation checks.

`.cjs` identifies a CommonJS JavaScript file. The test uses Node.js built-in
modules only.

## Where and how to run

Open a PowerShell terminal at the **repository/worktree root**, the folder that
contains `Microsoft.SCIM.LogicAppValidationTemplate`, and run:

```powershell
node --test .\Microsoft.SCIM.LogicAppValidationTemplate\StandardLogicApp\tests\minimal-auth-selection.test.cjs
```

Alternatively, from this `tests` directory:

```powershell
node --test .\minimal-auth-selection.test.cjs
```

Paths to the workflow files are resolved relative to the test file, not the
terminal's current directory. The two commands run the same twelve tests.

Expected summary:

```text
tests 12
pass 12
fail 0
```

Exact formatting and timing depend on the Node.js version and test reporter.
Exit code `0` means the tests passed; a nonzero exit code indicates a failure.

## What the tests check

| Test | Coverage |
|---|---|
| Selected branch and `EnabledTests` | OAuth and federation choices, blank/invalid selection, and existing suite/individual selectors |
| Selected-result aggregation | A failed, skipped or missing selected result cannot be rescued by a passing unselected method |
| Parent readiness | Only selected `PASSED` satisfies authentication readiness; an independent User failure still fails the parent; other semantic predicates match the baseline |
| Google WIF reporting | Individual diagnostic results stay unchanged, including `SKIPPED` for Google federation. The existing Google + `FederatedIdentity` exception passes only the authentication aggregate and retains its bypass reason; other providers still require the selected diagnostic result. No individual fields are added. |
| Google pagination eligibility | Only enabled, explicitly skipped Google pagination receives aggregate `PASSED`; missing/failed results and non-Google or customer-excluded skips are not promoted. Individual results remain unchanged. |
| Google pagination readiness | Aggregate results reach overall readiness and final metadata; group failures remain optional warnings, and independent semantic/runtime failures still block success. |
| Google empty-update provenance | Only a previously nonempty Google payload emptied by filtering receives the exception marker; already-empty/null payloads and other providers do not. |
| Google nested-update filtering | Removing the last required blank/null nested source prunes its empty parent and reaches the existing pass exception; populated siblings, `false`, `0`, missing sources, and non-Google behavior are preserved. |
| Google empty-update reporting | Successful create/provisioning remains mandatory; only eligible empty updates bypass the update phase and report `PASSED` with an explicit explanation. Normal updates and cleanup dependencies are preserved. |
| Preservation and consistency | Existing nested protocol actions, unrelated suite logic and collector remain unchanged, except the intentional direct-user-update propagation wait adjustment; documentation uses the selector name; the template version is blank; final results identify the choice |
| Version and timing output | Native result uses the deployed version; timestamps come from single captures; epoch/duration calculations cover whole seconds, subsecond truncation and midnight crossing |
| Offline deployment | The PowerShell test below verifies version injection, preserved parameters, read-back and preflight failures without cloud calls |

[version-deployment.test.ps1](version-deployment.test.ps1) is invoked by the
offline deployment Node test. It parses the deployment script and loads only the tested
functions, mocking Azure CLI and HTTP requests. It also invokes guarded
preflight failure paths that exit before any Azure operation.
To run it separately from this directory:

```powershell
pwsh -NoProfile -File .\version-deployment.test.ps1
```

## How workflow expressions are checked

The file reads conditions and result expressions directly from the workflow
definitions rather than keeping separate JavaScript copies of their logic.
A small evaluator supplies simulated parameter values, variables and action
outputs for the expression subset exercised by these tests.

This helper is **not the Azure Logic Apps engine**. It does not execute HTTP
actions, reproduce every expression-language rule, model lazy branch evaluation,
or validate scheduling, retries, managed identity, token lifetime or live deployment.
Expression changes outside its supported subset may require a deliberate helper
update and a real runtime check.

## Version and baseline maintenance

### Bundle version: do not hard-code a release number in the test

The test reads [VERSION](../VERSION) and checks that it is nonempty.
[Orchestrator_Parameters.json](../Orchestrator_Parameters.json) intentionally has
an empty `LogicAppVersion.value`; deployment generates its value from VERSION.
The offline PowerShell tests check injection, LF/CRLF trimming, future-version
values, preservation of nested data/expressions, unchanged source files,
missing/empty inputs, version read-back mismatch, and rejection of
`-SkipParameters`. No literal release number is asserted.

For a future version bump, update only VERSION. The generated upload and native
result carry that value; the source template remains blank. Tests do not enforce
a version-format policy or prove that a release number increased.

### Comparison baseline: intentionally pinned

The Git revision `8bdf02e` is intentionally fixed. It is the source baseline from
before the selection feature, not a release number. Reading whatever happens to
be latest master would make the preservation assertions change without review.

Future intentional protocol or semantic changes may require updating these
preservation assertions and their baseline. Review those changes explicitly;
do not simply move the baseline to hide a failing assertion.

## Troubleshooting

| Symptom | Next action |
|---|---|
| `node` or `git` not found | Install or select the prerequisite and reopen the terminal so its PATH is available |
| Unknown Git revision / missing baseline object | Use a normal repository checkout with the baseline commit in local history |
| Version assertion fails | Verify VERSION is nonempty, the template version is blank, and injection/read-back checks pass; do not hard-code a release number |
| Workflow-preservation assertion fails | Inspect the named action/file and decide whether its change is intentional |
| Expression parser/assertion fails | Inspect the expression and the helper's supported subset; do not assume this alone establishes a cloud runtime failure |

Use a separate authorized, isolated Logic App run to establish actual cloud
behavior. Keep its source version, deployed hashes, exclusions and native
results distinct from these local test results.
