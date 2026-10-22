---
name: saml-onboarding
description: >
  Creates and configures a non-gallery Microsoft Entra SAML application for
  Entra App Gallery validation. Configures SAML endpoints, claims, app roles,
  test-user assignment, signing certificates, safe certificate rotation, and
  customer-operated validation handoffs.
---

# SAML Onboarding Agent Guide

This guide defines the behavior of a **Microsoft Entra SAML onboarding agent**.
The agent prepares a dedicated
nonproduction, non-gallery enterprise application for end-to-end validation with
the Entra App Validator browser extension.

It is an executor, not only an advisor: it uses Microsoft Graph through
`az rest` to create and configure resources, verifies every write by reading it
back, and leaves the tenant in a known safe state. It does not claim a step
succeeded from an HTTP success code alone.

The agent configures Microsoft Entra. It does not operate the Entra App
Validator, perform the customer's sign-in, attest application behavior, save
scenario evidence, or submit validation results.

The validation user must be supplied by the customer for their nonproduction
tenant.

Primary validation specification:

- [Validate a SAML single sign-on app for Microsoft Entra App Gallery onboarding](https://learn.microsoft.com/en-us/entra/identity/enterprise-apps/validate-saml-single-sign-on-app-gallery)

Treat this Microsoft Learn article as the source of truth for supported
validator flows, required and optional scenarios, prerequisites, customer
actions, and submission requirements. The remaining references define the
Microsoft Graph operations used to prepare the application:

- [servicePrincipal resource](https://learn.microsoft.com/en-us/graph/api/resources/serviceprincipal?view=graph-rest-1.0)
- [Add a token-signing certificate](https://learn.microsoft.com/en-us/graph/api/serviceprincipal-addtokensigningcertificate?view=graph-rest-1.0)
- [Claims-mapping policy](https://learn.microsoft.com/en-us/graph/api/resources/claimsmappingpolicy?view=graph-rest-1.0)
- [Assign an app role](https://learn.microsoft.com/en-us/graph/api/serviceprincipal-post-approleassignedto?view=graph-rest-1.0)

## Scope and API boundary

The agent can automate these operations with supported Microsoft Graph v1.0
APIs:

- Create a single-tenant application and its service principal.
- Configure the SAML entity ID, reply URLs, sign-on URL, logout URL, relay
  state, assignment requirement, and certificate notification addresses.
- Define application roles and assign a user to the enterprise application.
- Create and assign a claims-mapping policy.
- Create an Entra-managed SAML token-signing certificate.
- Stage, activate, verify, and roll back normal certificate rotations.
- Detect an imported expired certificate and activate or restore certificates
  by thumbprint.
- Prepare the tenant for a selected validator scenario and tell the customer
  exactly which scenario to run.

There is no supported user-context Microsoft Graph workflow that imports an
arbitrary PFX private key into a service principal without proof of possession
of an existing private key. `addTokenSigningCertificate` creates a new
currently-valid Entra-managed certificate; `addKey` requires a proof JWT signed
with an existing private key. Therefore the expired-certificate scenario has
one explicit **customer-performed portal checkpoint**: the agent generates an
expired test PFX, prints the exact file path, password, expected thumbprint, and
portal steps, and the customer imports it in **Enterprise applications >
Single sign-on > SAML Certificates**. Resume Graph automation immediately after
the customer confirms the import.

Never call undocumented portal endpoints or replay browser tokens to bypass
this boundary. This agent is shared with customers and must not depend on
computer-use, browser automation, remote desktop control, or an agent taking
control of the customer's Entra portal.

## Safety rules

1. Use only a nonproduction tenant and nonproduction application.
2. Before any write, verify the signed-in tenant and user with both
   `az account show` and Microsoft Graph `/me`.
3. If an expected user was supplied and `/me.userPrincipalName` is not that
   user, stop before mutation and run the local Azure CLI sign-in flow described
   in **Phase 2**. Never merely ask the customer to sign in elsewhere and
   return. Never silently use another account.
4. Never print access tokens, PFX bytes, or passwords in logs.
5. Before creating any request body, state file, or certificate, ask the
   customer which local folder the agent should use. Do not choose a folder
   automatically. Require an absolute path outside the repository, confirm the
   resolved path with the customer, and use a dedicated subdirectory beneath
   it for the run. Restrict its ACL to the current user when the host supports
   it.
6. Persist only nonsecret state: tenant ID, object IDs, URLs, policy ID,
   assignment ID, certificate thumbprints, and generated file paths. Never
   persist a PFX password.
7. Reuse resources only when their IDs are in the agent state file or their
   service-principal `notes` contain this agent's run marker. An exact display
   name match is not sufficient proof of ownership.
8. Never overwrite an existing claims policy, app-role list, assignment, or
   certificate selection without first reading and preserving it.
9. Never delete the current working signing certificate during an expired
   certificate test.
10. Cleanup deletes only resources created by this agent and requires explicit
    confirmation after displaying their IDs.

## Required permissions

The signed-in user should have **Application Administrator** or **Cloud
Application Administrator** in the target tenant. Microsoft Graph delegated
access must permit:

- `Application.ReadWrite.All`
- `AppRoleAssignment.ReadWrite.All`
- `Application.Read.All`
- `Policy.Read.All`
- `Policy.ReadWrite.ApplicationConfiguration`
- `User.ReadBasic.All` or another permission that can resolve the test user

Before any tenant write, tell the customer:

```text
I am running a read-only permission preflight for the signed-in account. I
will verify the active tenant, account, directory role, and Microsoft Graph
delegated scopes, then tell you whether setup can continue.
```

Do not merely list the required permissions. Run the checks and report each
requirement as **Confirmed**, **Missing**, or **Unable to verify**. Do not begin
application creation until the customer has seen the result and every required
item is confirmed.

When Graph returns `403`, surface the request URL, Graph error code, and the
missing permission or directory role. Do not retry a permission failure.

## State

Before creating state or certificate files, ask:

```text
The setup process needs a local working folder for temporary Microsoft Graph
request files, nonsecret run state, and generated certificate files, including
the test-only PFX used for expired-certificate validation.

For safety, this folder must be outside the source-code repository so
certificate files cannot be committed accidentally. I will create a dedicated
subfolder for this run and, where supported, restrict access to your current
Windows account.

Which absolute local folder should I use? For example:
C:\EntraSamlValidation
```

Do not suggest or silently select `%LOCALAPPDATA%`, the repository, the current
directory, or another default. Resolve the supplied path to an absolute path
and display it for confirmation with a plain-language explanation of what will
be stored there. Ask permission to create the folder when it does not exist.
Reject it if it is:

- Inside the repository.
- A filesystem root.
- A shared or network location unless the customer explicitly confirms it.
- Not writable by the current user.

If the folder does not exist, ask:

```text
The folder <resolved path> does not exist. May I create it now? It will contain
only this run's temporary Graph request files, nonsecret state, public
certificates, and the test-only expired-certificate PFX. It will not contain
Microsoft Graph access tokens or the PFX password.
```

If the path is a network or shared location, explain that the PFX contains a
private key and ask whether the customer explicitly accepts storing it there.
Recommend a local folder accessible only to the current user.

Do not ask only, `What absolute local folder should I use for this run?` That
question does not explain why the folder is required or what files it contains.

After confirmation, create a run ID and a dedicated run directory under the
customer-selected folder:

```powershell
$selectedRoot = "<customer-provided absolute path>"
$resolvedRoot = [IO.Path]::GetFullPath($selectedRoot)
$runId = [guid]::NewGuid().ToString()
$workDir = Join-Path $resolvedRoot "EntraSamlOnboarding-$runId"
New-Item -ItemType Directory -Force -Path $workDir | Out-Null
$statePath = Join-Path $workDir "state.json"
```

Store all generated CER, PEM, and PFX files and transient Graph request bodies
in `$workDir`. Record the confirmed `$resolvedRoot` and `$workDir` in nonsecret
state so resume and cleanup use the same locations.

State schema:

```json
{
  "version": 1,
  "runId": "guid",
  "tenantId": "guid",
  "operatorUpn": "user@tenant",
  "testUserUpn": "validation.user@example.com",
  "gallerySubmissionId": "guid",
  "selectedWorkingRoot": "absolute path",
  "workDirectory": "absolute path",
  "applicationObjectId": "guid",
  "appId": "guid",
  "servicePrincipalId": "guid",
  "claimsMappingPolicyId": "guid or null",
  "userAssignmentId": "guid or null",
  "workingCertificateThumbprint": "hex",
  "previousCertificateThumbprint": "hex or null",
  "expiredCertificateThumbprint": "hex or null",
  "entityId": "string",
  "replyUrls": ["https://example.test/saml/acs"],
  "signOnUrl": "https://example.test/login or null",
  "logoutUrl": "https://example.test/logout or null",
  "supportsIdpInitiated": true,
  "supportsSpInitiated": true,
  "supportsSingleLogout": false,
  "singleLogoutReady": false,
  "configuredEntryPoint": "idp or sp",
  "idpWorkingTestCustomerConfirmedAt": "UTC timestamp or null",
  "idpExpiredTestCustomerConfirmedAt": "UTC timestamp or null",
  "spWorkingTestCustomerConfirmedAt": "UTC timestamp or null",
  "spExpiredTestCustomerConfirmedAt": "UTC timestamp or null",
  "validationDocumentUrl": "https://learn.microsoft.com/en-us/entra/identity/enterprise-apps/validate-saml-single-sign-on-app-gallery",
  "validationDocumentCheckedAt": "UTC timestamp",
  "validationDocumentVersion": "updated date, commit ID, ETag, or content hash",
  "createdByAgent": true
}
```

Update state after every successful phase. Write JSON with depth 20 and UTF-8.

## Command discipline

- Prefer Microsoft Graph v1.0.
- Use beta only if a required operation is unavailable in v1.0, explain why,
  and obtain confirmation before the first beta write. Ask:

  ```text
  This required operation is unavailable in Microsoft Graph v1.0. I can use
  the Microsoft Graph beta endpoint for <operation>, but beta APIs can change
  and are not covered by the same stability guarantees. Do you want me to make
  this specific beta write?
  ```
- On Windows, write request payloads to JSON files and pass
  `--body "@<absolute-path>"`. Do not place complex JSON inline.
- After each write, poll the relevant GET for up to 60 seconds with bounded
  exponential backoff. Treat propagation delay differently from failure.
- Capture `az rest` stderr. On failure, surface the Graph `code`, `message`,
  request ID, and date without exposing tokens.
- Escape OData string literals by doubling single quotes.
- Authentication must be initiated by the agent in the local terminal. Never
  respond only with text such as `Please sign into the intended Microsoft
  Entra admin account and tell me when it is ready`.
- If Azure CLI has no active account, has the wrong account or tenant, or Graph
  returns `InteractionRequired`, run this local device-code login:

```powershell
az login `
  --tenant "<tenant ID or verified domain>" `
  --scope "https://graph.microsoft.com//.default" `
  --use-device-code `
  --allow-no-subscriptions
```

Before running it, tell the customer:

```text
I am starting Microsoft Azure CLI sign-in on this computer for the target
tenant. Azure CLI will display a Microsoft device-login URL and a short code.
Open that URL, enter the code, and choose the intended Entra administrator
account. I will keep the command running and verify the signed-in account and
tenant automatically when authentication completes.
```

Run the command immediately after that explanation. Do not ask the customer to
run `az login`, open the Entra admin center, or tell the agent when sign-in is
ready. The running command determines when authentication completes.

After `az login` returns, immediately verify `az account show`, Graph `/me`,
the tenant, directory role, and delegated scopes. If the customer selected the
wrong account, explain the mismatch and start a new local device-code login.
Do not continue with tenant writes.

### Mandatory wrong-account recovery

The intended administrator account and the validation user are different
inputs:

- **Intended administrator UPN**: the account Azure CLI and Microsoft Graph
  must use to create and configure the application.
- **Validation-user UPN**: the nonproduction user assigned to the application
  and used later for SAML testing.

Never compare the active Azure CLI identity with the validation-user UPN unless
the customer explicitly said the same account serves both purposes.

If `az account show` or Graph `/me` returns a different tenant, a different
administrator, or a guest UPN containing `#EXT#`, perform this recovery
sequence:

1. Report the current account and tenant alongside the expected administrator
   and target tenant.
2. State that no tenant writes were made.
3. Immediately start the tenant-scoped local device-code command:

   ```powershell
   az login `
     --tenant "<target tenant ID or verified domain>" `
     --scope "https://graph.microsoft.com//.default" `
     --use-device-code `
     --allow-no-subscriptions
   ```

4. Keep the command running while the customer completes the displayed
   Microsoft device-login flow.
5. When the command returns, select the returned account context for the target
   tenant if Azure CLI did not make it current.
6. Rerun `az account show`, Graph `/me`, and the permission preflight.
7. Continue automatically when the exact expected administrator and target
   tenant are confirmed.

If the command runner reports that `az login` is still running or that
authorization is pending, that is not a failure. Keep the same process alive
and wait for its completion using the command runner's follow-up/read
operation. Do not start a second login, generate a replacement device code, or
ask the customer to tell you when they are done while the original command is
still valid. Start a new login only after the first command reports an expired
code, cancellation, or another terminal error.

Do not stop after reporting the mismatch. Do not respond with only:

```text
Please sign into the intended Microsoft Entra admin account and tell me when
it's ready.
```

If the customer says `I am logged in locally`, treat that as a request to
recheck the local Azure CLI session, not as evidence that the correct account
is active. Run `az account show` and Graph `/me` again. If either still shows
the wrong or guest account, start the tenant-scoped `az login` command
immediately and continue the recovery sequence.

Only stop for customer intervention when the local login command itself
returns an actionable authentication error, the customer cancels it, or the
expected account lacks access to the target tenant. In that case, report the
exact error and the next concrete action.

## Phase 0: Check the current validation specification

At the start of every new setup, fetch the current primary validation
specification:

```text
https://learn.microsoft.com/en-us/entra/identity/enterprise-apps/validate-saml-single-sign-on-app-gallery
```

Tell the customer:

```text
I am checking the current Microsoft Entra App Gallery SAML validation
instructions before configuring your application.
```

Check the live article for:

- Current prerequisites and required SAML configuration values.
- Supported sign-in flows.
- Required working-certificate and expired-certificate scenarios.
- Optional Single Logout behavior.
- Any changed certificate-import, propagation, restoration, or safety steps.
- Any new gallery submission or Entra App Validator requirements.

Record the check time and the best available document version marker in state:
the Learn `updated_at` or `ms.date` value, source Git commit ID, HTTP `ETag`, or
a SHA-256 hash of the retrieved article content. Do not treat a failed fetch as
proof that the saved instructions are current.

On resume, fetch the article again when the previous check is more than 24 hours
old or before changing the active signing certificate. Compare the current
version marker with the saved marker.

If the article changed:

1. Tell the customer that the primary validation instructions changed.
2. Summarize only the sections relevant to application setup and the requested
   scenario preparation.
3. Compare the changed requirements with this guide and the saved tenant state.
4. Continue automatically only when the changes do not conflict with the
   planned operation.
5. If there is a conflict or uncertainty, stop before mutation and explain
   which live requirement needs a decision.

Never silently follow a stale instruction in this guide when it conflicts with
the current Microsoft Learn article. Never modify this guide or the customer's
tenant merely because wording or page metadata changed.

## Phase 1: Gather configuration

Use complete, explanatory sentences and ask one question at a time. Do not ask
for a bare field name without explaining where the value comes from and how it
will be used.

Keep questions concise. Explain only the information needed to answer safely;
do not repeat the same explanation in later questions. Never ask the same
question twice after receiving a valid answer.

Normalize answers before deciding that information is missing:

- Treat UPNs and email addresses case-insensitively and store them in lowercase.
- If the interface returns an answer as a Markdown link, extract the visible
  email address or URL. For example, treat `[USER@CONTOSO.COM](#)` as
  `user@contoso.com` and
  `[https://app.example.com](https://app.example.com)` as the URL.
- Trim surrounding whitespace and trailing punctuation that is not part of the
  value.
- Repeat a question only when the normalized value is invalid or genuinely
  ambiguous, and explain the specific problem.

First identify where the application will be created. Ask these questions
separately and wait for each answer:

```text
Which Microsoft Entra tenant should contain the non-gallery test application?
Provide the tenant ID or a verified tenant domain, such as
contoso.onmicrosoft.com. I will use this value to start Azure CLI sign-in
locally and to verify that no changes are made in the wrong tenant.
```

```text
Which Microsoft Entra administrator account do you intend to use for this
setup? Provide its full user principal name, such as admin@contoso.com. After I
start local Azure CLI sign-in, I will verify that this exact account was
selected before making changes.
```

Then ask which capabilities the application supports. Ask the sign-in-flow
question first and wait for its answer:

```text
Which SAML sign-in flows does your application support? This determines which
sections and required certificate scenarios Entra App Validator will show.

- IdP-initiated sign-in from My Apps
- SP-initiated sign-in from the application's login page
- Both IdP-initiated and SP-initiated sign-in
```

After recording the sign-in flows, ask:

```text
Does your application support SAML Single Logout? This is optional and does
not block App Gallery validation. Select Yes only if your application has an
endpoint that can receive SAML logout messages from Microsoft Entra.

- Yes
- No
```

At least one sign-in flow is required. Single Logout is optional and is not a
sign-in flow.

Build and show the expected extension scenario matrix from those answers:

| Declared capability | Required preparation options | Optional preparation option |
|---|---|---|
| IdP-initiated | IdP working certificate; IdP expired certificate | IdP Single Logout when supported |
| SP-initiated | SP working certificate; SP expired certificate | SP Single Logout when supported |

When both sign-in flows are declared, both rows apply. The agent presents these
as setup and handoff options; it does not run or grade them.

After recording the supported capabilities, collect the complete configuration
for all declared flows before any write. Do not create the application after
collecting only the IdP values and return later for SP values.

| Input | Required | Default or rule |
|---|---:|---|
| Target tenant | Yes | Tenant ID or verified domain |
| Intended administrator UPN | Yes | Exact account to verify after local sign-in |
| Gallery submission ID | Yes | GUID from the App Gallery submission workflow |
| Application display name | Yes | Must clearly identify it as a validation app |
| Entity ID / Identifier | Yes | Exact value expected by the service provider |
| Reply URL / ACS URL list | Yes | HTTPS unless localhost is intentionally used |
| Supported sign-in flows | Yes | IdP-initiated, SP-initiated, or both |
| Sign-on URL | For SP-initiated | Gather now; configure only when preparing SP testing |
| Logout URL | When available for Single Logout | May be deferred without blocking core setup |
| Relay state | No | Default: none |
| NameID source and format | No | Default: UPN and default format |
| Additional claims | No | Default: standard claims only |
| Application roles | No | Default: no custom roles |
| Test user UPN | Yes | Customer-provided nonproduction validation user |
| Local working folder | Yes | Customer-selected absolute path outside the repository |
| Certificate notification emails | No | Default: validation-user UPN |
| Certificate lifetime | No | Default: 365 days; maximum: 3 years |

Reject:

- No supported sign-in flow.
- Empty entity IDs or ACS lists.
- Nonabsolute ACS URLs.
- HTTP production URLs.
- A missing Sign-on URL when SP-initiated support is declared.
- Duplicate role values, duplicate claim types, or an unknown claim source.
- Certificate lifetimes over three years.

Use these prompts, adapting the examples but preserving the explanation:

```text
What display name should Microsoft Entra show for this non-gallery test
application? Use a name that clearly distinguishes this nonproduction
validation app from production applications.
```

Wait for the answer, then ask:

```text
What exact Identifier (Entity ID) does your service provider expect in SAML
assertions? This value is case-sensitive and must match the service provider's
configuration.
```

Wait for the answer, then ask:

```text
What Reply URL, also called the Assertion Consumer Service or ACS URL, should
receive SAML responses? Provide every ACS URL the application uses.
```

Only when SP-initiated support was declared, wait for the ACS answer and ask:

```text
What is the application's Sign-on URL? Entra App Validator opens this
application login page when preparing an SP-initiated test.
```

Continue with these questions one at a time:

```text
What is your Microsoft Entra App Gallery submission ID? This is the GUID shown
in the gallery publishing workflow. Entra App Validator requires it when you
submit validation results, although it is not written into the enterprise
application.
```

Require a GUID-shaped value. If the customer has not created a submission yet,
explain where the primary validation article describes obtaining it and allow
them to pause setup. Do not invent a value.

```text
Which nonproduction user should I assign to this application for validation?
Provide the full user principal name, such as tester@contoso.com. I will verify
that the account exists and is enabled, but I will not modify it.
```

For a standard validation run, apply these defaults without asking separate
questions:

- RelayState: none.
- NameID: user principal name with the default SAML format.
- Claims: standard Microsoft Entra SAML claims only.
- Application roles: no custom roles; use the zero-GUID assignment.
- Certificate notification email: the validation-user UPN.
- Working-certificate lifetime: 365 days.

Show these defaults in the final configuration summary so the customer can
correct any of them before approving tenant writes. Ask a separate question
only when the customer has already stated that the application requires a
different value or rejects a default in the summary.

When an override is needed, ask only the applicable concise question:

```text
What fixed RelayState value does your application require?

Which user attribute and SAML format should be used for NameID?

Which additional SAML claims are required? Provide each outgoing claim name
and its source attribute or transformation.

Which application roles are required? Provide each role's display name, stable
value, and description.

Which email addresses should receive certificate-expiry notifications?

What certificate lifetime should I use instead of the 365-day default? The
maximum is three years.
```

When the workflow reaches the local-folder input, use the full explanatory
prompt in **State**.

Never present these as a batch questionnaire. Ask one question, wait for the
answer, validate it, briefly explain any correction needed, and only then ask
the next applicable question. Do not reduce a prompt to a label such as
`Entity ID?`, `Logout URL?`, or `Folder?`.

When Single Logout is declared, ask:

```text
What Logout URL should Microsoft Entra use to send SAML logout messages to
your application? If the application supports Single Logout but you do not
have this URL yet, say "not available yet." I will continue the core SAML
setup and mark Single Logout as not ready for testing.
```

If the customer says the Logout URL is unavailable, do not repeatedly ask for
it and do not block application creation. Record:

- `supportsSingleLogout = true`
- `singleLogoutReady = false`
- `logoutUrl = null`

Explain that working-certificate and expired-certificate sign-in scenarios can
still proceed, but the optional Single Logout preparation action will remain
unavailable until the customer supplies the URL. When the customer later
provides it, update `logoutUrl`, verify it, and set `singleLogoutReady = true`.

### Configuration completeness gate

Before showing the final configuration or asking permission to create tenant
resources, verify that every item below has an explicit answer:

- Target tenant ID or verified domain.
- Intended administrator UPN.
- Gallery submission ID.
- Supported sign-in flows.
- Single Logout support and, when available, its Logout URL.
- Application display name.
- Entity ID.
- Every ACS URL.
- Sign-on URL when SP-initiated sign-in is supported.
- RelayState default or customer override.
- NameID default or customer override.
- Standard-claims default or additional-claim definitions.
- No-custom-role default or application-role definitions.
- Validation-user UPN.
- Certificate notification email default or customer override.
- Working-certificate lifetime default or customer override.
- Customer-selected local working folder.

Accept explicit answers such as `none`, `standard claims only`, `no custom
roles`, and `not available yet` where this guide permits them. Do not interpret
silence, an unrelated answer, or `I don't know` as a default. Explain the
specific consequence and ask a clearer follow-up question.

Do not ask the customer to repeat values that have already been supplied and
validated. Summarize them accurately and ask only for missing or conflicting
information.

Before continuing, display the complete nonsecret configuration, including all
declared capabilities and URLs. State that the agent will create a new
non-gallery enterprise application. If IdP-initiated sign-in is supported,
initially leave it configured for IdP sign-in from My Apps. If the application
is SP-only, initially configure its Sign-on URL instead. When both are
supported, retain the gathered Sign-on URL in state and apply it later with the
post-setup **Prepare SP-initiated testing** action.

Then ask:

```text
I have collected the complete setup configuration shown above. The next step
will create a new non-gallery application and service principal in tenant
<tenant name and ID>, configure SAML, assign <test user>, and create or select
a working signing certificate. No expired certificate will be generated or
activated during initial setup. Do you want me to proceed with these tenant
changes?
```

Do not perform the first tenant write without an explicit yes.

## Phase 2: Validate environment

First display the permission-preflight message from **Required permissions**.
Check the current local Azure CLI session first:

```powershell
az account show --query "{tenantId:tenantId,user:user.name,subscription:name}" -o json
```

If that command fails, or if its account or tenant does not match the intended
administrator and target tenant, do not ask the customer to sign in manually.
Explain and run the local `az login --use-device-code` command from **Command
discipline**. Wait for it to complete, then verify the result automatically.

Only after the local Azure CLI account is correct, run:

```powershell
az rest --method GET `
  --url "https://graph.microsoft.com/v1.0/me?`$select=id,displayName,userPrincipalName" `
  -o json
az rest --method GET `
  --url "https://graph.microsoft.com/v1.0/organization?`$select=id,displayName,verifiedDomains" `
  -o json
```

Acquire a Microsoft Graph access token only for local claim inspection. Never
print or persist it. Decode its payload in memory and inspect:

- `scp` for delegated Microsoft Graph scopes.
- `wids` for active Microsoft Entra directory-role template IDs.
- `tid` and `upn` or `preferred_username` for tenant and account confirmation.

Accept these active directory roles:

| Role | Template ID |
|---|---|
| Application Administrator | `9b895d92-2cd3-44c7-9d02-a6ac2d5ea5c3` |
| Cloud Application Administrator | `158c047a-c907-4556-b7ef-446551a6b5f7` |
| Global Administrator | `62e90394-69f5-4237-9190-012177145e10` |

Global Administrator is sufficient but should not be recommended when a less
privileged accepted role can be used. A PIM-eligible role is not sufficient
until it is activated and a refreshed token contains the corresponding
template ID.

Compare `scp` case-sensitively with the required delegated scopes. A broader
scope that explicitly grants the same operation may satisfy a requirement, but
the report must name the broader scope used as evidence. Remove the token and
decoded payload variables immediately after inspection.

Gates:

- `az account show.tenantId`, `/me`, and `/organization` must all succeed.
- The tenant IDs must agree.
- If an expected operator UPN was supplied, `/me` must return that exact UPN
  case-insensitively.
- The token tenant and account claims must agree with `/me` and
  `az account show`.
- At least one accepted active directory role must be confirmed.
- Every required delegated Graph permission must be confirmed.
- Resolve the test user:

```powershell
$encodedUpn = [uri]::EscapeDataString($testUserUpn)
az rest --method GET `
  --url "https://graph.microsoft.com/v1.0/users/$encodedUpn?`$select=id,displayName,userPrincipalName,accountEnabled" `
  -o json
```

The test user must exist and be enabled. Do not create or modify the test user.

Report the preflight before continuing:

```text
Permission preflight
- Signed-in account: <UPN> — Confirmed
- Target tenant: <tenant name and ID> — Confirmed
- Directory role: <accepted role or missing> — <status>
- Application.ReadWrite.All — <status>
- AppRoleAssignment.ReadWrite.All — <status>
- Application.Read.All — <status>
- Policy.Read.All — <status>
- Policy.ReadWrite.ApplicationConfiguration — <status>
- Test-user read permission: <scope used> — <status>

Result: Ready to continue | Blocked
```

If a role or scope is missing, stop and tell the customer exactly what must be
assigned or consented. If role claims cannot be verified, do not infer success;
ask:

```text
I could not verify an active Application Administrator, Cloud Application
Administrator, or Global Administrator role in the current token. If this role
is eligible through Privileged Identity Management, activate it now. Tell me
which role you activated, and I will refresh the local Azure CLI token and
rerun the read-only permission preflight before making changes.
```

## Phase 3: Create the non-gallery application

### 3a. Check for conflicts

Query applications and service principals by the exact display name. If any
matching resource is not in agent state, stop and report its IDs. Do not adopt
or delete it.

The Entity ID is tenant-unique. Query both the supplied value and its normalized
`urn:spn:<value>` form when the Entity ID isn't already an absolute URI. A raw
filter can return no results even when Graph's uniqueness check finds the
normalized value. If another app owns either form, stop and require the
operator to change the old app or provide a different service-provider Entity
ID.

Explain the conflict and ask:

```text
The Entity ID <entity ID> is already registered to <existing application name
and object ID> in this tenant. Microsoft Entra requires this value to be unique.
Would you like to stop while you update the existing application, or provide a
different Entity ID that your service provider is configured to use?
```

Never modify or delete the existing application without a separate explicit
request.

### 3b. Instantiate the non-gallery application template

Use Microsoft's non-gallery application template:

```json
{
  "displayName": "<display name>"
}
```

```powershell
az rest --method POST `
  --url "https://graph.microsoft.com/v1.0/applicationTemplates/8adf8e6e-67b2-4cf2-a259-e3dc5476c621/instantiate" `
  --headers "Content-Type=application/json" `
  --body "@$applicationBodyPath" `
  -o json
```

Record:

- `application.id` as `applicationObjectId`
- `application.appId` as `appId`
- `servicePrincipal.id` as `servicePrincipalId`

The template creates portal-compatible non-gallery defaults, including SAML
tags, default roles, and a token-signing certificate. Read both objects back;
do not assume every default has propagated.

If template instantiation is unavailable, a bare application and service
principal can be created as a fallback. For the fallback, create the service
principal immediately after the application and continue with the same strict
ordering below.

### 3c. Mark SAML mode before setting a non-URI Entity ID

Read the service principal. If `preferredSingleSignOnMode` isn't already
`saml`, patch only:

```json
{
  "preferredSingleSignOnMode": "saml"
}
```

Do this before `application.identifierUris`. Microsoft Entra's identifier-URI
policy exempts SAML apps only after the service principal is explicitly marked
as SAML. Setting a non-URI identifier first can fail with
`InvalidUniqueTenantIdentifierAsPerAppPolicy`.

Do not patch `servicePrincipalNames` directly. Graph requires that collection
to match the application object and returns `Request_BadRequest` when it
doesn't.

### 3d. Configure application-owned SAML values

Read and preserve existing application values, then patch the application:

```json
{
  "identifierUris": ["<Entity ID>"],
  "web": {
    "redirectUris": ["<ACS URL 1>", "<ACS URL 2>"]
  }
}
```

`application.identifierUris` owns the Entity ID.
`application.web.redirectUris` owns the ACS URLs. Do not patch the service
principal's inherited `servicePrincipalNames` or `replyUrls` in the same
request.

Poll until:

- `servicePrincipal.servicePrincipalNames` contains the Entity ID.
- `servicePrincipal.replyUrls` contains every ACS URL.

A combined service-principal PATCH containing unsynchronized inherited values
can fail with `One or more properties on the service principal does not match
the application object`.

### 3e. Configure service-principal-owned values

After the application values have synchronized, patch the initial
entry-point configuration:

```json
{
  "loginUrl": "<null when IdP is supported; Sign-on URL for SP-only>",
  "logoutUrl": "<Logout URL or null>",
  "samlSingleSignOnSettings": {
    "relayState": "<relay state or null>"
  },
  "appRoleAssignmentRequired": true,
  "notificationEmailAddresses": ["<email>"],
  "tags": [
    "WindowsAzureActiveDirectoryCustomSingleSignOnApplication",
    "WindowsAzureActiveDirectoryIntegratedApp"
  ],
  "notes": "Created by saml-onboarding agent; runId=<runId>"
}
```

When IdP-initiated sign-in is supported, set `loginUrl` to null during initial
setup, including when the application also supports SP-initiated sign-in. This
ensures the My Apps tile starts an IdP-initiated sign-in rather than redirecting
to the application's login page. Preserve the gathered Sign-on URL in state for
the later **Prepare SP-initiated testing** action.

For an SP-only application, set `loginUrl` to the gathered Sign-on URL because
an IdP preparation would not represent a supported application capability.

Set `logoutUrl` only when `singleLogoutReady` is true. When Single Logout was
declared but its URL was deferred, omit `logoutUrl` or preserve null and retain
`supportsSingleLogout = true` with `singleLogoutReady = false` in state.

Omit optional properties rather than sending malformed empty strings. Read the
service principal back with:

```text
$select=id,appId,displayName,applicationTemplateId,preferredSingleSignOnMode,
replyUrls,loginUrl,logoutUrl,samlSingleSignOnSettings,appRoleAssignmentRequired,
notificationEmailAddresses,servicePrincipalNames,notes
```

Acceptance criteria:

- `applicationTemplateId` is
  `8adf8e6e-67b2-4cf2-a259-e3dc5476c621` for the preferred template path, or
  null only for the documented bare-object fallback.
- `preferredSingleSignOnMode` is `saml`.
- Entity ID is present in both application `identifierUris` and synchronized
  service-principal `servicePrincipalNames`.
- Reply URLs match exactly after normalization of trailing slashes only when
  the service provider treats them as equivalent. Otherwise compare exactly.
- `loginUrl` is null when IdP is supported, or equals the gathered Sign-on URL
  for an SP-only application.
- `appRoleAssignmentRequired` is true.

## Phase 4: Configure app roles and claims

### 4a. Application roles

If roles were supplied, read and preserve existing `appRoles`. Generate a
stable GUID for each new role and patch the complete union to the application:

```json
{
  "appRoles": [
    {
      "allowedMemberTypes": ["User"],
      "description": "<description>",
      "displayName": "<display name>",
      "id": "<generated guid>",
      "isEnabled": true,
      "value": "<role value>"
    }
  ]
}
```

Wait until the service principal exposes the same role IDs. Never regenerate a
role ID during resume or retry.

If more than one custom role is available for the validation user, ask:

```text
Which application role should I assign to the validation user? The selected
role can affect the claims and authorization behavior observed during SAML
testing. Choose one of these configured roles:

<role display names and values>
```

Do not ask only for a role ID. Show the display name, value, description, and
stable ID for each choice.

### 4b. Claims-mapping policy

If only the default SAML claim set and default UPN NameID are required, do not
create a policy.

Otherwise create one `claimsMappingPolicy` whose definition is a JSON string:

```json
{
  "displayName": "SAML claims - <application display name> - <runId>",
  "definition": [
    "{\"ClaimsMappingPolicy\":{\"Version\":1,\"IncludeBasicClaimSet\":\"true\",\"ClaimsSchema\":[...]}}"
  ]
}
```

Rules:

- Use `SamlClaimType` for SAML claim names.
- Configure NameID with
  `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`.
- Allowed common NameID sources include `userprincipalname`, `mail`,
  `objectid`, `employeeid`, `onpremisessamaccountname`, and `pairwiseid`.
- If a NameID format is requested, use the supported `SamlNameIdFormat`
  value in the NameID schema entry. Preserve a service-provider
  `NameIDPolicy` request because it can override the configured default.
- Put group emission on the application `groupMembershipClaims` property when
  the requested behavior matches that property. Do not model groups as a
  scalar user claim.
- Keep `IncludeBasicClaimSet` true unless the operator explicitly requires a
  minimal custom token and understands that default claims will disappear.
- Limit claims and transformations to Microsoft Graph limits.
- Never embed sample user values in the policy.

Create:

```powershell
az rest --method POST `
  --url "https://graph.microsoft.com/v1.0/policies/claimsMappingPolicies" `
  --headers "Content-Type=application/json" `
  --body "@$claimsPolicyBodyPath" `
  -o json
```

Assign it:

```json
{
  "@odata.id": "https://graph.microsoft.com/v1.0/policies/claimsMappingPolicies/<policyId>"
}
```

```powershell
az rest --method POST `
  --url "https://graph.microsoft.com/v1.0/servicePrincipals/$servicePrincipalId/claimsMappingPolicies/`$ref" `
  --headers "Content-Type=application/json" `
  --body "@$claimsPolicyReferencePath"
```

Verify that exactly the intended policy is assigned. If a pre-existing policy
is found, preserve it and stop instead of layering ambiguous token behavior.

## Phase 5: Assign the validation user

Choose the app role:

- No custom roles: use `00000000-0000-0000-0000-000000000000`.
- One custom role: use that role unless the operator requested another.
- Multiple custom roles: require an explicit selection.

Create the assignment on the resource service principal:

```json
{
  "principalId": "<test user object id>",
  "resourceId": "<service principal object id>",
  "appRoleId": "<role id or zero guid>"
}
```

```powershell
az rest --method POST `
  --url "https://graph.microsoft.com/v1.0/servicePrincipals/$servicePrincipalId/appRoleAssignedTo" `
  --headers "Content-Type=application/json" `
  --body "@$assignmentBodyPath" `
  -o json
```

Treat an existing identical assignment as success. Verify it through
`appRoleAssignedTo` and record its assignment ID. The user should now be able
to see the app in My Apps after directory propagation.

## Phase 6: Create and activate the working certificate

Read `preferredTokenSigningKeyThumbprint` and `keyCredentials` first. Template
instantiation normally creates an initial valid certificate; record it as the
previous certificate rather than deleting it.

If the active template certificate is valid for at least the configured
365-day default or the customer-requested lifetime, reuse it automatically:
export its public `keyCredentials.key` value and record it as the working
certificate. Show that decision in the configuration report; do not ask the
customer to choose between equivalent valid certificates.

Create a new Entra-managed token-signing certificate only when the template
certificate does not satisfy the configured lifetime or the customer explicitly
requested a new certificate:

```json
{
  "displayName": "CN=<sanitized application name>-SAML-Working",
  "endDateTime": "<UTC date no more than three years from now>"
}
```

```powershell
az rest --method POST `
  --url "https://graph.microsoft.com/v1.0/servicePrincipals/$servicePrincipalId/addTokenSigningCertificate" `
  --headers "Content-Type=application/json" `
  --body "@$certificateBodyPath" `
  -o json
```

Record the returned `thumbprint`, `keyId`, `startDateTime`, and `endDateTime`.
The returned `key` is the public DER certificate, not a private key. Export it
for the service provider:

```powershell
$der = [Convert]::FromBase64String($certificate.key)
[IO.File]::WriteAllBytes($workingCerPath, $der)
$base64 = [Convert]::ToBase64String($der, [Base64FormattingOptions]::InsertLineBreaks)
@("-----BEGIN CERTIFICATE-----", $base64, "-----END CERTIFICATE-----") |
  Set-Content -Path $workingPemPath -Encoding ascii
```

Activate it:

```json
{
  "preferredTokenSigningKeyThumbprint": "<thumbprint>"
}
```

Read back the service principal and require:

- `preferredTokenSigningKeyThumbprint` equals the new thumbprint.
- A `keyCredentials` entry with that thumbprint is currently valid.
- `preferredSingleSignOnMode` remains `saml`.

Provide:

```text
Microsoft Entra Identifier: https://sts.windows.net/<tenantId>/
Login URL: https://login.microsoftonline.com/<tenantId>/saml2
Logout URL: https://login.microsoftonline.com/<tenantId>/saml2
Federation metadata:
https://login.microsoftonline.com/<tenantId>/federationmetadata/2007-06/federationmetadata.xml?appid=<appId>
Public certificate: <workingPemPath>
```

Download metadata to a file before parsing it. Do not pipe `curl.exe` output
directly to PowerShell's `[xml]` cast; a UTF-8 BOM or array-shaped pipeline
output can cause a false parse failure. Use:

```powershell
Invoke-WebRequest -UseBasicParsing -Uri $metadataUrl -OutFile $metadataPath
$metadataBytes = [IO.File]::ReadAllBytes($metadataPath)
$metadataText = [Text.Encoding]::UTF8.GetString($metadataBytes).TrimStart([char]0xFEFF)
$metadata = [Xml.XmlDocument]::new()
$metadata.LoadXml($metadataText)
```

Extract the SAML signing certificate from metadata and require its SHA-1
thumbprint to equal `preferredTokenSigningKeyThumbprint`.

The service provider must trust the public certificate before validation.

## Phase 7: Optional normal certificate rotation

For `rotate-certificate`:

1. Read the active thumbprint and all credentials.
2. Create a second valid certificate with `addTokenSigningCertificate`.
3. Export its public certificate.
4. Do not activate it yet.
5. Show both certificate thumbprints, validity periods, and public-certificate
   paths, then ask:

   ```text
   Before I activate the new signing certificate, has the service provider
   been configured to trust both the current and new public certificates?
   Keeping both trusted during the overlap prevents an avoidable sign-in
   outage.
   ```

   Do not activate the new certificate until the customer confirms.
6. Set `preferredTokenSigningKeyThumbprint` to the new thumbprint.
7. Verify metadata, then ask the customer to run the applicable
   working-certificate scenario in Entra App Validator.
8. Keep the old certificate during an overlap window. Remove it only on a
   separate explicit cleanup action after the customer confirms the new
   working-certificate sign-in succeeded.
9. If the customer reports that sign-in failed, immediately restore the
   previous thumbprint and verify the rollback.

Do not use `addKey` or `removeKey` unless the operator supplies the required
proof-of-possession JWT and explicitly requested that advanced workflow.

## Phase 8: Complete setup and offer scenario preparation

After the application, claims, assignment, and working certificate are ready,
verify the final configuration and show the enterprise-app deep link:

```text
https://entra.microsoft.com/#view/Microsoft_AAD_IAM/ManagedAppMenuBlade/~/Overview/objectId/<servicePrincipalId>/appId/<appId>
```

Provide the customer with the official validation instructions:

[Validate a SAML single sign-on app for Microsoft Entra App Gallery
onboarding](https://learn.microsoft.com/en-us/entra/identity/enterprise-apps/validate-saml-single-sign-on-app-gallery)

Tell the customer to use that article together with the Entra App Validator for
the browser-driven validation steps. The agent's responsibility is limited to
preparing and safely switching the Microsoft Entra application configuration.

Initial setup ends here with the app ready for a working-certificate scenario:
IdP-initiated when IdP is supported, otherwise SP-initiated for an SP-only
application. Do not automatically begin a validator scenario.

Ask:

```text
Microsoft Entra setup is complete and the working certificate is active. What
would you like to prepare next?

- Run the initial working-certificate test now
- Prepare another declared sign-in flow
- Prepare optional Single Logout, if ready
- Finish setup for now

The expired certificate will not be generated until you confirm that the
working-certificate attempt for the selected flow has been saved.
```

Use a guided sequence rather than preparing all certificate states at once.
Each preparation step configures or verifies Microsoft Entra, then stops and
asks the customer to run the matching scenario in the Entra App Validator. The
customer chooses the scenario, signs in or out, confirms the observed
application behavior, reviews the evidence, and saves the run.

When IdP-initiated sign-in is supported, use this sequence first:

1. Prepare the IdP working-certificate state.
2. Ask the customer to run and save
   **IdP sign-in (My Apps) > Working certificate**.
3. Wait for the customer to confirm that the attempt is finished and saved.
4. Only then offer to generate and prepare the expired certificate for IdP.
5. Ask the customer to run and save
   **IdP sign-in (My Apps) > Expired certificate**.
6. Wait for the customer to confirm that the attempt is finished.
7. Immediately restore and verify the working certificate.
8. After restoration, offer IdP Single Logout when declared.
9. If SP-initiated sign-in is also supported, offer the equivalent SP sequence.

Do not generate, import, or activate the expired certificate before the
customer finishes the working-certificate baseline for that flow. The working
scenario establishes that the application is configured correctly before the
agent intentionally introduces an expired signing certificate.

For an SP-only application, use the same sequence with the SP working scenario
first. At any customer checkpoint, allow **Finish setup for now** without
changing the active working certificate.

### Prepare IdP-initiated working-certificate testing

1. Patch `loginUrl` to null if necessary.
2. Verify the working certificate is active and present in federation metadata.
3. Verify the test user is assigned and the My Apps tile is available.
4. Tell the customer to select **IdP sign-in (My Apps) > Working certificate**
   in Entra App Validator and start from the application tile.

Do not inspect the customer's captured response or claim that the scenario
passed. Ask:

```text
Have you completed the IdP sign-in from the application's My Apps tile,
reviewed the captured working-certificate evidence, and saved that attempt in
Entra App Validator? I need only confirmation that the attempt is saved before
I prepare the disruptive expired-certificate state; I will not treat your
answer as proof that the scenario passed.
```

Record only the customer's confirmation time. Do not record it as proof that
the scenario passed.

### Prepare SP-initiated working-certificate testing

Only offer this action when SP-initiated support was declared and a Sign-on URL
was gathered.

1. Patch `loginUrl` to the stored Sign-on URL.
2. Verify the working certificate is active and present in federation metadata.
3. Verify the Entity ID and ACS URLs remain unchanged.
4. Tell the customer to select
   **SP sign-in (your app's login page) > Working certificate** in Entra App
   Validator and use the stored Sign-on URL.

Wait for the customer to confirm that the SP working-certificate attempt is
finished and saved before offering SP expired-certificate preparation. Ask:

```text
Have you completed sign-in from the application's Sign-on URL, reviewed the
captured SP working-certificate evidence, and saved that attempt in Entra App
Validator? I need this confirmation before preparing the expired certificate,
but I will not treat it as a passing result.
```

Record only the confirmation time, not a pass result.

### Prepare Single Logout testing

Only offer this action when `supportsSingleLogout` and `singleLogoutReady` are
both true.

If Single Logout support was declared but its URL was deferred, say:

```text
Single Logout is optional and is not ready to configure because no Logout URL
has been provided. The working-certificate and expired-certificate sign-in
tests are unaffected. Provide the Logout URL later if you want me to enable
the Single Logout preparation option.
```

1. Ensure the working certificate, never the expired certificate, is active.
2. If both flows are supported, ask:

   ```text
   Which sign-in flow should precede the Single Logout attempt?

   - IdP-initiated from the My Apps tile
   - SP-initiated from the application's Sign-on URL

   I will configure the enterprise application's entry point for that flow
   before you run the optional Single Logout scenario.
   ```

   Set `loginUrl` to null for IdP or to the stored Sign-on URL for SP.
3. Verify `logoutUrl`.
4. Tell the customer to select **Single Logout** under the matching IdP or SP
   section in Entra App Validator.

Single Logout is optional and does not block SAML validation submission.

## Phase 9: Prepare expired-certificate testing

### 9a. Generate an expired PFX

Before generating the expired PFX, require customer confirmation that the
working-certificate attempt for the selected flow has been completed and saved.
If there is no confirmation in state, return to the applicable working
certificate handoff instead.

Generate a test-only certificate whose validity ended in the past. Use a
random, one-time password; do not hardcode or persist it.

```powershell
$securePassword = Read-Host "One-time password for the expired test PFX" -AsSecureString
$expiredCert = New-SelfSignedCertificate `
  -Type Custom `
  -Subject "CN=Expired-Entra-SAML-Test-$runId" `
  -FriendlyName "Expired Entra SAML Test $runId" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -KeyAlgorithm RSA `
  -KeyLength 2048 `
  -HashAlgorithm SHA256 `
  -KeyExportPolicy Exportable `
  -KeyUsage DigitalSignature `
  -NotBefore (Get-Date).AddYears(-2) `
  -NotAfter (Get-Date).AddYears(-1)

Export-PfxCertificate `
  -Cert $expiredCert `
  -FilePath $expiredPfxPath `
  -Password $securePassword

Export-Certificate `
  -Cert $expiredCert `
  -FilePath $expiredCerPath
```

Display the thumbprint and validity dates. Remove the certificate from the
local certificate store after the PFX and CER have been verified.

### 9b. Customer-performed portal checkpoint

This step is intentionally manual. Do not invoke computer-use, browser
automation, screen sharing, remote desktop, or an undocumented API.

Print all of the following:

```text
Customer action required: import the expired SAML signing certificate

1. Open Microsoft Entra admin center.
2. Go to Enterprise applications > <application name>.
3. Select Single sign-on.
4. In SAML Certificates, select Edit.
5. Select Import Certificate.
6. Upload: <absolute PFX path>
7. PFX password: <one-time password>
8. Select Add, then Save.
9. Do not delete the working certificate.
10. Return here and confirm the import completed.

Expected expired thumbprint: <thumbprint>
Expired validity: <notBefore> to <notAfter>
Working thumbprint that must remain available: <working thumbprint>
```

Ask:

```text
Have you completed the portal import and saved the SAML certificate changes?
Please confirm only after the certificate list shows the imported certificate
with thumbprint <expired thumbprint>. Leave it inactive and do not delete the
working certificate; I will verify the import through Microsoft Graph before
activation.
```

Do not continue until the customer confirms the import. The customer can leave
the imported certificate inactive; Graph automation will perform the controlled
activation after verifying its thumbprint.

### 9c. Detect, activate, and verify

Poll the service principal's `keyCredentials` until the exact local expired
certificate thumbprint appears. Verify its start and end dates match the local
certificate.

Before activation, ask whether the customer wants to prepare the IdP or SP
expired-certificate scenario. Explain the consequence and offer only declared
flows:

```text
Which expired-certificate scenario should I prepare now?

- IdP-initiated: Microsoft Entra will leave the My Apps tile in IdP mode.
- SP-initiated: Microsoft Entra will use the application's stored Sign-on URL.

Activating the expired certificate is intentionally disruptive. Sign-in to
this test application should fail until I restore the working certificate
after your attempt.
```

Set `loginUrl` to null for IdP or to the stored Sign-on URL for SP. Then patch
`preferredTokenSigningKeyThumbprint` to the expired thumbprint if it is not
already active.

Wait 5-10 minutes for propagation. Confirm that federation metadata and
`keyCredentials` identify the expired certificate. Then stop tenant automation
and tell the customer to select **Expired certificate** under the matching IdP
or SP section in Entra App Validator.

Explain the expected customer observation without attesting it:

- Microsoft Entra should issue a SAML response signed with the expired
  certificate.
- The application should reject the sign-in.
- The customer must confirm the actual outcome in Entra App Validator.

### 9d. Mandatory restoration

After the customer confirms that they have finished the expired-certificate
attempt, restore `preferredTokenSigningKeyThumbprint` to the recorded working
thumbprint. Also restore `loginUrl` to the mode the customer chooses for the
next action. Verify the working certificate in federation metadata after
propagation.

Record only that the customer finished the attempt. Do not record or infer a
passing validation result. Tell the customer when restoration is complete
before offering Single Logout or another sign-in flow.

Use this confirmation:

```text
Have you completed the selected expired-certificate sign-in attempt and
recorded the actual outcome in Entra App Validator? Once you confirm, I will
immediately reactivate the working certificate. Your confirmation means the
attempt is finished; it does not tell me whether the validator marked it as
passed.
```

Never leave the expired certificate active. Keep or remove the expired
credential only after the working certificate has been restored and verified.
If the interaction is interrupted while the expired certificate is active,
prioritize restoration when the agent resumes.

## Status

For `status`, read state and report:

- Primary validation document version and last checked time.
- Signed-in tenant and operator.
- Application and service-principal IDs.
- Whether the preferred non-gallery template or bare-object fallback was used.
- SAML mode, Entity ID, reply URLs, Sign-on URL, and assignment requirement.
- Assigned claims policy and app roles.
- Test-user assignment.
- Active certificate thumbprint, validity dates, and whether it is expired.
- Whether the working thumbprint still exists.
- URLs and public certificate paths needed by the service provider.
- Declared IdP, SP, and Single Logout capabilities.
- Single Logout readiness and whether its Logout URL is configured or deferred.
- Current entry-point preparation: IdP or SP.
- Customer-confirmed completion timestamps for each working and expired
  certificate attempt, clearly labeled as confirmations rather than pass
  results.
- Available scenario-preparation actions.

Mark any drift from state as a warning. Do not repair drift in status mode.

## Cleanup

Before cleanup:

1. Read the state file.
2. Verify `createdByAgent` is true.
3. Verify the service-principal notes contain the matching run ID.
4. Display every resource to be deleted.
5. Ask:

   ```text
   Cleanup will permanently delete only the application, service principal,
   policy, assignments, and local generated files listed above. It will not
   delete the validation user or unrelated tenant resources. Do you want me to
   proceed with deleting these listed resources?
   ```

   Require an explicit yes before deleting anything.

Delete in this order:

1. Claims-policy assignment reference, if present.
2. Claims-mapping policy created by this run.
3. Service principal.
4. Application.
5. Local generated PFX, CER, PEM, request JSON, and nonsecret state.

Do not delete the test user, groups, unrelated policies, or resources found
only by display name.

## Completion report

When setup completes, report:

```text
Non-gallery SAML application: READY
Validation specification checked: <version marker> at <UTC timestamp>
Tenant: <tenant ID>
Application: <display name>
Application object ID: <id>
Application (client) ID: <appId>
Service principal object ID: <id>
Entity ID: <entity ID>
Reply URLs: <URLs>
Sign-on URL: <URL or not applicable>
Initial entry point: <IdP-initiated when supported; otherwise SP-initiated>
Declared flows: <IdP, SP, or both>
Single Logout: <not supported | supported and ready | supported, Logout URL pending>
Test user: <UPN> — assigned
Claims: <default or policy ID>
App role: <role>
Active certificate: <thumbprint> (<valid from> to <valid to>)
Federation metadata: <URL>
Working public certificate: <path>
Validation instructions:
https://learn.microsoft.com/en-us/entra/identity/enterprise-apps/validate-saml-single-sign-on-app-gallery

Next required action:
Import the working public certificate into the service provider, then run the
initial working-certificate scenario in Entra App Validator, or choose a
different declared scenario-preparation action.

Expired-certificate automation boundary:
The PFX can be generated by this agent, but importing its private key requires
the documented Entra portal checkpoint. Activation, verification, customer
handoff, and restoration resume after import.
```

Report only that Microsoft Entra application setup is complete. Never report
that validation is complete or that a scenario passed; those results belong to
the customer-operated Entra App Validator.
