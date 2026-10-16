# entra-scim-provisioning-validation-logic-app-template
# Logic App Test Documentation

This document explains:

1. Input parameters and what values are expected.
2. Each test: what it does, key steps, wait times, and how success/failure is determined.

## Standard Logic App agent: authentication-aware validation

Use the canonical [SCIM onboarding agent](StandardLogicApp/scim-onboarding.agent.md) and its complete, same-revision bundle. The Standard implementation deploys **seven workflows**: Orchestrator, Initialization, UserTests, GroupTests, SCIMTests, Authentication, and the shared SCIMToken provider. There are still **25 reportable tests**. [VERSION](StandardLogicApp/VERSION) is the sole overall and agent version; bump it for every bundle change and synchronize `LogicAppVersion`.

At startup, declare the authentication methods actually supported by the endpoint and available in the validation environment:

| Environment | `supportedAuthenticationMethods` | `scimAuthenticationMethod` |
|-------------|----------------------------------|----------------------------|
| OAuth only | `["OAuth2ClientCredentials"]` | `OAuth2ClientCredentials` |
| Federation only | `["WorkloadIdentityFederation"]` | `WorkloadIdentityFederation` |
| Both | Both gallery method values | Choose which authenticates core SCIM requests |
| Static token only | `["Bearer"]` | `Bearer` |

The agent accepts initial parameters or asks a specific multiple-choice question, then collects only applicable credentials. Undeclared authentication tests are **Not applicable** and make no token requests. A declared method with missing credentials is blocked/failed, not silently excluded.

The authentication readiness policy is **AND across selected methods**: every declared method must pass its test. An undeclared method is Not applicable; a selected failure cannot be hidden by another success. `Authentication_Workflow` isolates these checks to keep SCIMTests below the 500-action limit. `SCIMTests_Workflow` explicitly forwards `authenticationModelResult`; a missing result fails closed in the Orchestrator. Other mandatory functional failures still block readiness. Static-token-only functional testing produces `NOT_GALLERY_READY`.

For OAuth-only or federation-only runs, set `scimBearerToken` to `""`. The shared token workflow acquires access tokens, and each test suite refreshes its cache before expiry. There is no static-token fallback. Sending an OAuth access token in an HTTP Bearer header does not mean the endpoint supports caller-supplied static tokens. Entra provisioning connectivity is configured and validated separately with the supported method.

See the [Standard setup walkthrough](StandardLogicApp/SetupLogicApp-Standard-Agent.md) and [test overview](StandardLogicApp/SCIM-Validation-Test-Overview.md) for the current behavior. The generic legacy test descriptions below do not override the Standard authentication contract.

The [authentication change guide](StandardLogicApp/Authentication-Changes.md) explains capability selection, token transport, workflow limits, and verification boundaries. Portable reporting is a separate follow-up change.

---

## 1. Input Parameters

### `scimEndpoint`
- **What it is**: Base URL of your SCIM service endpoint.
- **Expected value**: A valid HTTPS URL, e.g. `https://scim.example.com/tenant1`.
- **Used for**: Fetching All SCIM `Users` and `Groups` to validate in tests.

### `scimBearerToken`
- **What it is**: Bearer token for authenticating to the SCIM endpoint.
- **Expected value**: A valid token recognized by your SCIM service. Make sure it has long validity, longer that the time it takes to complete testing.
- **Used for**: `Authorization: Bearer {token}` header on SCIM HTTP calls.

### `testUserDomain`
- **What it is**: The domain that will be appended to the test user’s UPN/email.
- **Expected value**: Your Azure AD / Entra ID domain, e.g. `contoso.onmicrosoft.com`.
- **Used for**: Constructing user identifiers like `testuser1@{testUserDomain}`.

### `defaultPassword`
- **What it is**: Default password for created test users.
- **Expected value**: A strong test-only password.
- **Used for**: Initial password assigned to test users when applicable.

### `defaultUserProperties`
- **What it is**: JSON object with default attributes for a test user.
- **Key fields** (examples, may vary slightly):
  - `displayName`
  - `mailNickname`
  - `givenName`, `surname`
  - `employeeId`, `employeeOrgData`
  - `identities` (e.g. Azure AD identity)
  - `passwordProfile.password`
- **Expected value**: Valid user attributes accepted by Microsoft Graph / SCIM. Whatever value is provided for these attribute will be used while creating the user. if no value is provided, the test generates a random value for the attribute.
- **Used for**: Creating a baseline user object before sending it to SCIM/Graph.

### `defaultGroupProperties`
- **What it is**: JSON object with default attributes for a test group.
- **Key fields** (examples):
  - `displayName`
  - `mailNickname`
- **Expected value**: Valid group attributes accepted by Microsoft Graph / SCIM.
- **Used for**: Creating a baseline group object in group-related tests.

### Other common parameters
- **Prefix / suffix parameters for test names or IDs** (if present in template):
  - Used to ensure test entities are unique per run.
- **Timeout / wait duration parameters** (if present):
  - Used to control how long the workflow waits between actions or verifications.

---

## 2. Test Scenarios

Below are the main tests and their behavior. Names are generic; they map to the scopes/steps you see in the `WorkingTemplate.json` logic app.

### 2.1 Create User Test

**Goal**  
Verify that a new user can be created successfully via SCIM and/or Microsoft Graph, and that it is visible on subsequent reads.

**High-level steps**
1. **Prepare user payload**
   - Merge `defaultUserProperties` with run-specific values (e.g. unique `userPrincipalName` or `mailNickname`).
   - Optionally include `passwordProfile.password` (from `defaultUserProperties` or `defaultPassword`).

2. **Call SCIM `POST /Users`**
   - URL: `{scimEndpoint}/Users`
   - Auth: `Authorization: Bearer {scimBearerToken}`
   - Body: Prepared user payload in SCIM format.

3. **Check create response**
   - Expect HTTP status in the 2xx range (commonly `201 Created`).
   - Capture returned `id` or external identifier.

4. **Wait for propagation**
   - Logic app waits a short period (e.g. 10–60 seconds).
   - Purpose: Allow backend directory / SCIM service to commit the change.

5. **Verify via read**
   - Call `GET {scimEndpoint}/Users/{id}` or search by unique attribute (e.g. UPN or externalId).
   - Optionally verify via Microsoft Graph if the test is cross-validating SCIM → Graph.

**Success criteria**
- User create call returns a success code.
- Follow-up read returns the created user with expected core attributes:
  - Identifier (`id`/`userPrincipalName`).
  - Key profile attributes from `defaultUserProperties`.

**Failure conditions**
- Create call returns non-2xx (e.g. 4xx/5xx).
- Read after wait:
  - Returns `404 Not Found`, or
  - Returns incomplete/mismatched data.
- Any exception/timeout in the HTTP actions.

---

### 2.2 Update User Test

**Goal**  
Validate that existing user attributes can be updated and are correctly reflected on read.

**High-level steps**
1. **Locate or create base user**
   - Either use the user from the Create User Test or ensure there is a baseline user via an initial lookup.

2. **Prepare update payload**
   - Modify selected fields from `defaultUserProperties`, e.g.:
     - `displayName`
     - `jobTitle`
     - `department` or `employeeOrgData`
   - Keep the identity field (e.g. `id` or UPN) stable.

3. **Call SCIM `PATCH /Users/{id}` or `PUT /Users/{id}`**
   - URL: `{scimEndpoint}/Users/{id}`
   - Auth: Bearer token.
   - Body: Only changed attributes or full user object, depending on SCIM implementation.

4. **Wait for propagation**
   - Short wait (e.g. 10–60 seconds) for changes to persist.

5. **Verify via read**
   - Call `GET {scimEndpoint}/Users/{id}`.
   - Compare key updated fields to expected values.

**Success criteria**
- Update request succeeds with 2xx status.
- Post-update read shows the new values for the targeted attributes.

**Failure conditions**
- Update call fails (4xx/5xx).
- Read does not reflect updated values after wait.
- Missing or inconsistent attributes relative to what was sent.

---

### 2.3 Deactivate/Delete User Test

**Goal**  
Confirm that a user can be deactivated or deleted and no longer appears as active.

**High-level steps**
1. **Locate target user**
   - Typically the user created in the Create User Test, identified via `id` or unique UPN.

2. **Issue delete/deactivate request**
   - Either:
     - `DELETE {scimEndpoint}/Users/{id}`, or
     - `PATCH /Users/{id}` to set `active:false` (depending on SCIM implementation).

3. **Wait for propagation**
   - Short wait (e.g. 10–60 seconds) for deactivation to propagate.

4. **Verify status**
   - If using hard delete: `GET {scimEndpoint}/Users/{id}` is expected to return `404`.
   - If using soft delete / deactivation:
     - Read user and confirm `active:false` or equivalent flag.
   - Optional: Verify user is removed/disabled in Graph.

**Success criteria**
- Deactivation/delete request succeeds.
- Follow-up read matches expected state:
  - 404/not found for hard delete; or
  - `active:false` (or disabled flag) for soft delete.

**Failure conditions**
- Delete/deactivate request fails.
- User remains active/accessible contrary to expectations.
- Inconsistent responses across SCIM and Graph (if both are checked).

---

### 2.4 Create Group Test

**Goal**  
Validate that a group can be created with expected attributes.

**High-level steps**
1. **Prepare group payload**
   - Start from `defaultGroupProperties`.
   - Ensure unique name (e.g. prefix/suffix for test run).

2. **Call SCIM `POST /Groups`**
   - URL: `{scimEndpoint}/Groups`
   - Auth: Bearer token.
   - Body: Group payload.

3. **Check create response**
   - Expect 2xx (typically `201 Created`).
   - Capture returned group `id`.

4. **Wait for propagation**
   - Short wait for group to appear in backend directory.

5. **Verify via read**
   - `GET {scimEndpoint}/Groups/{id}`.
   - Confirm key attributes (display name, etc.) match input.

**Success criteria**
- Group create call succeeds.
- Follow-up read shows group with correct name and properties.

**Failure conditions**
- Create call fails.
- Read does not return the group or returns incorrect attributes.

---

### 2.5 Add User to Group Test

**Goal**  
Ensure that membership operations work correctly (user can be added to a group and membership is visible).

**High-level steps**
1. **Ensure prerequisites**
   - A test user exists (from Create User Test).
   - A test group exists (from Create Group Test).

2. **Issue membership add**
   - SCIM `PATCH /Groups/{groupId}` with `members` array update, or equivalent.
   - Alternatively, Graph call adding member to group if the template uses Graph for membership.

3. **Wait for propagation**
   - Wait for membership changes to persist (e.g. 10–60 seconds).

4. **Verify membership**
   - `GET {scimEndpoint}/Groups/{groupId}`.
   - Check `members` contains the expected user `id`.
   - Optionally verify via Graph group membership API.

**Success criteria**
- Membership add request succeeds.
- Follow-up read shows user included in group’s member list.

**Failure conditions**
- Membership request fails.
- User not present in group membership list after wait.
- Inconsistent membership info between SCIM and Graph (if both are checked).

---

### 2.6 Remove User from Group Test

**Goal**  
Validate that membership removal works and user is no longer in group’s member list.

**High-level steps**
1. **Precondition**
   - User is currently a member of the test group.

2. **Issue membership remove**
   - SCIM `PATCH /Groups/{groupId}` removing the user from the `members` array, or Graph equivalent.

3. **Wait for propagation**
   - Short wait to allow backend to update membership.

4. **Verify membership**
   - `GET {scimEndpoint}/Groups/{groupId}`.
   - Confirm that the test user is no longer listed in `members`.

**Success criteria**
- Removal operation returns success status.
- Group’s membership no longer lists the test user.

**Failure conditions**
- Removal request fails.
- Membership still includes the user after wait.

---

### 2.7 End-to-End Lifecycle Test (Optional Combined Flow)

Some templates combine the above into an end-to-end test:

1. **Create user.**
2. **Create group.**
3. **Add user to group.**
4. **Verify user and group in Graph/SCIM.**
5. **Update user attributes.**
6. **Remove user from group.**
7. **Deactivate/delete user.**

**Success criteria**
- All intermediate steps report success.
- Final state matches expectations (e.g. user deactivated, group cleaned if required).

**Failure conditions**
- Any intermediate action fails or verification does not match expected outcome.

---

## 3. Timing and Wait Behavior

- After **writes** (create/update/delete/membership change), the logic app:
  - Waits a configurable period (commonly tens of seconds) before verifying.
  - This is to account for directory and SCIM backend propagation delays.
- Verification is typically **single shot**:
  - Perform one or a small number of reads after the wait.
  - If still not consistent, mark test as failed.

---

This document should allow a reader to understand, at a high level, what each test does, what inputs are needed, how long the workflow waits between operations, and how it decides if each test passes or fails.
