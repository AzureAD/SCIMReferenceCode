# Local authentication-selection regression tests

## Purpose and scope

[minimal-auth-selection.test.cjs](minimal-auth-selection.test.cjs) checks the
single-authentication-selection behavior in the actual workflow JSON files.
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
- A repository checkout or worktree with the pre-feature baseline commit
  `8bdf02e700b8c4e667c6eee6924010274a58af8f` available in its local history.
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
terminal's current directory. The two commands run the same four tests.

Expected summary:

```text
tests 4
pass 4
fail 0
```

Exact formatting and timing depend on the Node.js version and test reporter.
Exit code `0` means the tests passed; a nonzero exit code indicates a failure.

## What the four tests check

| Test | Coverage |
|---|---|
| Selected branch and `EnabledTests` | OAuth and federation choices, blank/invalid selection, and existing suite/individual selectors |
| Selected-result aggregation | A failed, skipped or missing selected result cannot be rescued by a passing unselected method |
| Parent readiness | Only selected `PASSED` satisfies authentication readiness; an independent User failure still fails the parent; other semantic predicates match the baseline |
| Preservation and consistency | Existing nested protocol actions, other suite definitions and scripts remain unchanged; documentation uses the selector name; the version parameter agrees with VERSION; final results identify the choice |

## How workflow expressions are checked

The file reads conditions and result expressions directly from the workflow
definitions rather than keeping separate JavaScript copies of their logic.
A small evaluator supplies simulated parameter values, variables and action
outputs for the expression subset exercised by these tests.

This helper is **not the Azure Logic Apps engine**. It does not execute HTTP
actions, reproduce every expression-language rule, model lazy branch evaluation,
or validate scheduling, retries, managed identity, token lifetime or deployment.
Expression changes outside its supported subset may require a deliberate helper
update and a real runtime check.

## Version and baseline maintenance

### Bundle version: do not hard-code a release number in the test

The test reads [VERSION](../VERSION), checks that it is nonempty, and compares it
with `LogicAppVersion.value` in
[Orchestrator_Parameters.json](../Orchestrator_Parameters.json). It does not assert
a literal release such as `14.0`.

For a future version bump, update the authoritative VERSION file and the existing
parameter default together. The test then verifies consistency without needing a
third version edit. It does not enforce a version-format policy or prove that a
release number increased.

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
| Version assertion fails | Compare VERSION with the shared `LogicAppVersion` default; do not restore a hard-coded test version |
| Workflow-preservation assertion fails | Inspect the named action/file and decide whether its change is intentional |
| Expression parser/assertion fails | Inspect the expression and the helper's supported subset; do not assume this alone establishes a cloud runtime failure |

Use a separate authorized, isolated Logic App run to establish actual cloud
behavior. Keep its source version, deployed hashes, exclusions and native
results distinct from these local test results.
