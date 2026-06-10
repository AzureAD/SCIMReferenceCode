# 🤖 Microsoft Entra ID App Gallery — AI-Powered Validation Checklist

> **Purpose**: This document is designed for ISV developers to use with AI assistants (Copilot, ChatGPT, Claude, etc.) to validate their SaaS application against Microsoft Entra ID App Gallery requirements for **SSO** and **SCIM Provisioning** listing.
>
> **How to use**: Point your AI assistant to this document along with your application's configuration, code, or documentation. The AI will validate each requirement and provide a pass/fail report with remediation guidance.

---

## 📋 AI Instructions

```
You are an Application Validation Agent. Your job is to validate the developer's application
against the Microsoft Entra ID App Gallery listing requirements defined below.

For each requirement:
1. Ask the developer for evidence (code, config, documentation, endpoint URL, etc.)
2. Validate the evidence against the requirement criteria
3. Mark the requirement as: ✅ PASS | ❌ FAIL | ⚠️ NEEDS REVIEW | ⏭️ NOT APPLICABLE
4. For failures, provide specific remediation steps with links to Microsoft documentation
5. Track progress and generate a final validation report

IMPORTANT RULES:
- Do NOT skip any "Required" items — all must pass for gallery submission
- For "Recommended" items, flag them but don't block submission
- Ask clarifying questions when evidence is ambiguous
- Validate with real endpoint testing when possible (SCIM Validator, token endpoints, etc.)
- Generate a final summary report at the end with pass rate and blocking issues
```

---

## 🔐 PART 1: SSO Validation

The developer must choose their SSO protocol. Ask which one they support:

- **Option A**: SAML 2.0 / WS-Fed → Go to [Section 1A](#section-1a-saml-20--ws-fed-sso)
- **Option B**: OpenID Connect (OIDC) / OAuth 2.0 → Go to [Section 1B](#section-1b-openid-connect--oauth-20-sso)

> ⛔ **Password SSO applications are NOT accepted** in the App Gallery anymore. The application must support Federation (SAML/WS-Fed) or OIDC/OAuth 2.0.

---

### Section 1A: SAML 2.0 / WS-Fed SSO

#### 1A.1 — Authentication Requirements

| # | Requirement | Priority | Validation Prompt |
|---|------------|----------|-------------------|
| 1A.1.1 | Application supports **SAML 2.0 Protocol** in SP-initiated and/or IDP-initiated mode | **Required** | _"Show me your SAML configuration. Do you support SP-initiated SSO, IDP-initiated SSO, or both? Provide the SSO URL, Entity ID, and ACS URL."_ |
| 1A.1.2 | Application **validates the SAML token** — checks certificate key, certificate validity, Issuer, Audience, and user claims | **Required** | _"Show me the code or configuration where you validate the SAML assertion. Do you check: (a) signing certificate, (b) certificate expiry, (c) Issuer URI, (d) Audience restriction, (e) user claims like NameID?"_ |
| 1A.1.3 | SAML integration has been **tested with Microsoft Entra ID** using a non-gallery application | **Required** | _"Have you tested your SAML SSO with a Microsoft Entra ID non-gallery application? Provide screenshots or test results showing successful login flow."_ |
| 1A.1.4 | Application supports **SAML Single Logout (SLO)** | Recommended | _"Does your application support SAML Single Logout? Provide the SLO endpoint URL and show the logout flow."_ |
| 1A.1.5 | Application **fetches IDP SAML federation metadata** from Microsoft Entra ID metadata URL | Recommended | _"Does your application support automatic metadata refresh from the Microsoft Entra ID federation metadata URL? This enables automatic certificate rotation."_ |
| 1A.1.6 | Application provides **UI and APIs** for customers to configure SSO | Recommended | _"Show me the admin UI where customers configure SAML SSO settings (Entity ID, ACS URL, certificate upload, etc.)."_ |
| 1A.1.7 | Application provides ability to **enforce SSO** for the entire tenant with break-glass bypass | Recommended | _"Can administrators enforce SSO for all users? Is there a break-glass mechanism to bypass SSO in emergencies?"_ |

#### 1A.2 — ISV Requirements (SAML)

| # | Requirement | Priority | Validation Prompt |
|---|------------|----------|-------------------|
| 1A.2.1 | Application is published as a **SaaS or IaaS** model, customer-configurable | **Required** | _"Is your application deployed as SaaS (cloud-hosted) or distributed to customers (IaaS)? Can each customer configure their own instance?"_ |
| 1A.2.2 | **Engineering and support contact** established for App Gallery onboarding | **Required** | _"Provide your engineering contact (name, email) and support contact for App Gallery onboarding and post-onboarding support."_ |
| 1A.2.3 | **SAML SSO configuration documentation** is publicly available | **Required** | _"Provide the public URL to your SAML SSO configuration documentation. It must include: protocol details, configuration steps, supported identity providers, and troubleshooting."_ |
| 1A.2.4 | Meet **compliance requirements** for target clouds (Public, USGov, China, etc.) | Required (if targeting sovereign clouds) | _"Which clouds are you targeting? (Public, USGov, China, Germany, France, Singapore). What compliance certifications do you have?"_ |

---

### Section 1B: OpenID Connect / OAuth 2.0 SSO

#### 1B.1 — Authentication Requirements

| # | Requirement | Priority | Validation Prompt |
|---|------------|----------|-------------------|
| 1B.1.1 | Application supports **OpenID Connect protocol** using **OAuth 2.0 Authorization Code Grant flow** | **Required** | _"Show me your OIDC configuration. Are you using the Authorization Code Grant flow? Provide your redirect URI, client ID registration, and auth flow implementation."_ |
| 1B.1.2 | Application uses **Microsoft Entra ID V2 endpoint** (`login.microsoftonline.com/.../oauth2/v2.0/...`) | **Required** | _"Show me your auth endpoint URLs. Are you using the V2 endpoints? V1 endpoints (`oauth2/authorize`) are not accepted — must use V2 (`oauth2/v2.0/authorize`)."_ |
| 1B.1.3 | Application is configured as **multi-tenant** (recommended) or single-tenant | **Required** | _"Is your app registration set to multi-tenant (`signInAudience: AzureADMultipleOrgs`) or single-tenant? Multi-tenant is recommended for SaaS apps."_ |
| 1B.1.4 | Application uses **least privileged permissions** for Microsoft Graph APIs | **Required** | _"List all Microsoft Graph API permissions your application requests. For each permission, justify why it's needed. Are you using the least privileged permission for each API call?"_ |
| 1B.1.5 | Application uses **delegated permissions** (not application permissions) where possible | Required (if using MS Graph) | _"Are you using delegated permissions (user context) or application permissions (app-only context)? Application permissions should only be used when absolutely necessary. Justify any application permissions."_ |
| 1B.1.6 | Application uses **certificates** (not secrets) for client credentials flow | **Required** | _"If your app uses client credentials flow, are you using a certificate instead of a client secret? Secrets are not accepted."_ |
| 1B.1.7 | SPA applications do **NOT use OAuth 2.0 Implicit Grant Flow** | Recommended | _"If this is a SPA (Single Page Application), are you using Authorization Code flow with PKCE instead of Implicit Grant? Implicit Grant has security concerns."_ |
| 1B.1.8 | Application does NOT use **Resource Owner Password Credentials (ROPC)** flow | **Required** | _"Confirm your application does NOT use the ROPC flow (username/password direct grant). This flow is not recommended and should not be used."_ |
| 1B.1.9 | Application does NOT use **Device Authorization Grant** flow unless explicitly needed | **Required** | _"Does your application use the Device Code flow? If yes, justify why it's required for your scenario."_ |

#### 1B.2 — ISV Requirements (OIDC)

| # | Requirement | Priority | Validation Prompt |
|---|------------|----------|-------------------|
| 1B.2.1 | Application is published as **SaaS** model, customer-configurable | **Required** | _"Is your application deployed as SaaS? Can each customer configure their own instance?"_ |
| 1B.2.2 | Sign-in page has **"Sign in with Microsoft"** button following [branding guidelines](https://learn.microsoft.com/en-us/entra/identity-platform/howto-add-branding-in-apps) | Recommended | _"Show me your sign-in page. Does it have a 'Sign in with Microsoft' button? Does it follow Microsoft's branding guidelines (correct logo, button style, text)?"_ |
| 1B.2.3 | Application is **publisher verified** using MPN ID | **Required** | _"Has your application been publisher verified? Provide your Microsoft Partner Network (MPN) ID and show the verified publisher badge on your app registration."_ |
| 1B.2.4 | **Engineering and support contact** established | **Required** | _"Provide your engineering contact (name, email) and support contact for post-onboarding support."_ |
| 1B.2.5 | **OIDC/OAuth SSO configuration documentation** is publicly available | **Required** | _"Provide the public URL to your OIDC SSO configuration documentation."_ |
| 1B.2.6 | Meet **compliance requirements** for target clouds | Required (if targeting sovereign clouds) | _"Which clouds are you targeting? What compliance certifications do you have?"_ |
| 1B.2.7 | Application is NOT a **public client** application | **Required** | _"Is your app a confidential client (server-side) or public client (native/SPA without backend)? Microsoft Entra App Gallery doesn't onboard public client applications."_ |

---

## 👥 PART 2: SCIM Provisioning Validation

> SCIM Provisioning is optional but highly recommended. If the developer wants to list their app with SCIM provisioning support, ALL required items below must pass.

### 2.1 — SCIM API Requirements

| # | Requirement | Priority | Validation Prompt |
|---|------------|----------|-------------------|
| 2.1.1 | Application supports **SCIM 2.0 User endpoint** (`/Users`) | **Required** | _"Provide your SCIM 2.0 User endpoint URL. Show me a sample `GET /Users`, `POST /Users`, `PATCH /Users/{id}`, and `DELETE /Users/{id}` request/response."_ |
| 2.1.2 | Application supports **SCIM 2.0 Group endpoint** (`/Groups`) | Recommended | _"Do you support a SCIM Group endpoint? If yes, show a sample `GET /Groups`, `POST /Groups`, `PATCH /Groups/{id}` request/response."_ |
| 2.1.3 | SCIM endpoint supports at least **25 requests per second per tenant** | **Required** | _"What is the rate limit on your SCIM endpoint per tenant? It must support at least 25 requests/second. Provide load test results or configuration showing this capacity."_ |
| 2.1.4 | SCIM implementation validated with **[SCIM Validator tool](https://learn.microsoft.com/en-us/entra/identity/app-provisioning/scim-validator-tutorial)** | **Required** | _"Have you run the SCIM Validator tool against your endpoint? Provide the validation report showing all tests passed."_ |
| 2.1.5 | SCIM implementation tested with **non-gallery application** in Microsoft Entra ID | **Required** | _"Have you tested your SCIM provisioning using a non-gallery application template in Microsoft Entra ID? Provide screenshots showing successful user provisioning."_ |
| 2.1.6 | Application supports **soft delete or hard delete** of users (at least one) | **Required** | _"Does your SCIM endpoint support soft delete (setting `active: false`) or hard delete (`DELETE /Users/{id}`)? Which one? Show sample request/response."_ |
| 2.1.7 | Querying a **nonexistent user** returns success with 0 results (not an error) | **Required** | _"What does your SCIM endpoint return when querying a user that doesn't exist (e.g., `GET /Users?filter=userName eq "nonexistent@test.com"`)? It must return HTTP 200 with `totalResults: 0`, NOT a 400/404 error."_ |
| 2.1.8 | SCIM endpoint supports **Schema Discovery** (`/Schemas`, `/ResourceTypes`, `/ServiceProviderConfig`) | **Required** | _"Does your SCIM endpoint support schema discovery? Test these endpoints and show responses: `GET /Schemas`, `GET /ResourceTypes`, `GET /ServiceProviderConfig`."_ |
| 2.1.9 | Support **updating multiple group memberships** with a single PATCH | Recommended | _"Can your SCIM endpoint handle a single PATCH request that adds/removes multiple group members at once? Show a sample multi-member PATCH request."_ |
| 2.1.10 | Support for **SCIM Bulk APIs** (`/Bulk`) | Recommended | _"Does your SCIM endpoint support the Bulk API endpoint? This improves connector performance for large-scale provisioning."_ |

### 2.2 — SCIM Authentication Requirements

> ⛔ **Microsoft is NOT onboarding** any SCIM app with: long-lived bearer tokens, basic authentication, or Code Auth Grant flow.
> ✅ **Only OAuth 2.0 Client Credentials flow is accepted.**

| # | Requirement | Priority | Validation Prompt |
|---|------------|----------|-------------------|
| 2.2.1 | SCIM authentication uses **OAuth 2.0 Client Credentials flow** | **Required** | _"Show me your SCIM authentication implementation. Does it use OAuth 2.0 Client Credentials flow (grant_type=client_credentials)? Provide the token endpoint URL."_ |
| 2.2.2 | Application does **NOT use** basic auth, long-lived bearer tokens, or Code Auth Grant for SCIM | **Required** | _"Confirm your SCIM endpoint does NOT use: (a) Basic authentication, (b) Long-lived bearer tokens, (c) Authorization Code Grant flow. Only Client Credentials flow is accepted."_ |
| 2.2.3 | Customers are provided with **client_id, client_secret, auth token endpoint, and SCIM endpoint** | **Required** | _"Do you provide customers with: (a) client_id, (b) client_secret, (c) token endpoint URL, (d) SCIM endpoint URL — so they can configure this in Microsoft Entra ID?"_ |
| 2.2.4 | **Client secret expiry** is between 1 year and 3 years | **Required** | _"What is the expiry period of the client secret? It must be between 1 year and 3 years. Access tokens cannot be retrieved with expired credentials."_ |
| 2.2.5 | Ability to **rotate client secrets** — support multiple active secrets or new client_id/secret creation | **Required** | _"How do customers rotate secrets? Do you support: (a) multiple active secrets with deletion of old ones, or (b) creation of new client_id and client_secret?"_ |
| 2.2.6 | **Access token validity** is between 60 minutes (1 hour) and 6 hours | **Required** | _"What is the lifetime of the access token issued by your token endpoint? It must be at least 60 minutes and no more than 6 hours."_ |
| 2.2.7 | Client Credentials flow validated with **non-gallery app** or **SCIM Validator** | **Required** | _"Have you tested the Client Credentials authentication flow using either the non-gallery application template or SCIM Validator? Provide test results."_ |

### 2.3 — ISV Requirements (SCIM)

| # | Requirement | Priority | Validation Prompt |
|---|------------|----------|-------------------|
| 2.3.1 | **Engineering and support contact** for post-onboarding and future Microsoft outreach | **Required** | _"Provide your engineering contact (name, email) and support contact for SCIM-related issues post gallery onboarding."_ |
| 2.3.2 | **SCIM endpoint documentation** publicly available | **Required** | _"Provide the public URL to your SCIM endpoint documentation. It must describe supported resources, attributes, and configuration steps."_ |
| 2.3.3 | SCIM provisioning deployed to at least **100 mutual customers** using non-gallery approach | **Required** | _"How many customers are currently using your SCIM provisioning through the non-gallery approach in Microsoft Entra ID? You need at least 100 to qualify for gallery listing."_ |
| 2.3.4 | Provide at least **5 customer tenant IDs** for private preview testing | **Required** | _"Provide at least 5 customer Microsoft Entra tenant IDs who can participate in private preview testing of the gallery connector."_ |
| 2.3.5 | Meet **compliance requirements** for target clouds | Required (if targeting sovereign clouds) | _"If you're targeting sovereign clouds (USGov, China, Germany, France, Singapore), what compliance certifications do you have?"_ |

---

## 📄 PART 3: Documentation Requirements

> These apply regardless of SSO protocol or SCIM support.

| # | Requirement | Priority | Validation Prompt |
|---|------------|----------|-------------------|
| 3.1 | Documentation includes **introduction to SSO functionality** — protocols, version, SKU | **Required** | _"Does your public documentation include: supported SSO protocols, application version/SKU, and list of supported identity providers?"_ |
| 3.2 | **Licensing information** for the application | **Required** | _"Does your documentation clearly state licensing requirements for SSO/SCIM features?"_ |
| 3.3 | **Role-based access control** documentation for configuring SSO | **Required** | _"Does your documentation describe which admin roles are required to configure SSO?"_ |
| 3.4 | **Step-by-step SSO configuration guide** with UI screenshots | **Required** | _"Does your documentation include step-by-step configuration with screenshots showing expected values for SAML attributes or OIDC settings?"_ |
| 3.5 | **Testing steps** for pilot users | **Required** | _"Does your documentation include testing steps that a pilot user can follow to verify SSO is working?"_ |
| 3.6 | **Troubleshooting guide** with error codes and messages | **Required** | _"Does your documentation include a troubleshooting section with common error codes, messages, and resolution steps?"_ |
| 3.7 | **Support mechanisms** documented for end users | **Required** | _"Does your documentation describe how users can get support (support portal, email, phone, community forum)?"_ |
| 3.8 | **SCIM endpoint details** — supported resources and attributes | Required (if SCIM) | _"Does your documentation describe the SCIM endpoint, supported resources (Users, Groups), and supported attributes with their mapping?"_ |
| 3.9 | **OIDC permissions list** with business justifications | Required (if OIDC) | _"Does your documentation list all MS Graph permissions your app requests, along with a business justification for each?"_ |

---

## 🧪 PART 4: AI Validation Test Scenarios

> The AI assistant should walk through these test scenarios with the developer to verify end-to-end functionality.

### Test Scenario 1: SSO Login Flow
```
STEPS:
1. Configure the application with Microsoft Entra ID (non-gallery app)
2. Assign a test user to the application
3. Attempt SP-initiated login (if SAML) or redirect login (if OIDC)
4. Verify successful authentication and correct user claims
5. Verify logout flow (if SLO is supported)

EXPECTED: User is authenticated and redirected back to the app with correct identity claims.
EVIDENCE NEEDED: Screenshots or logs showing the complete login flow.
```

### Test Scenario 2: SCIM User Provisioning
```
STEPS:
1. Configure SCIM endpoint in Microsoft Entra ID (non-gallery app)
2. Configure Client Credentials authentication
3. Assign a user for provisioning
4. Start provisioning cycle
5. Verify user is created in the target application
6. Update user attributes in Entra ID
7. Verify attributes are synced to the target application
8. Unassign the user
9. Verify user is soft-deleted or hard-deleted in the target application

EXPECTED: Full user lifecycle (create, update, delete) works correctly.
EVIDENCE NEEDED: Provisioning logs from Microsoft Entra ID and user records from target app.
```

### Test Scenario 3: SCIM Error Handling
```
STEPS:
1. Query a non-existent user via SCIM filter
2. Send an invalid PATCH request
3. Send requests exceeding rate limits
4. Use an expired access token

EXPECTED:
- Non-existent user query returns HTTP 200 with totalResults: 0
- Invalid requests return appropriate error codes (400, 422)
- Rate limit returns HTTP 429 with Retry-After header
- Expired token returns HTTP 401

EVIDENCE NEEDED: API responses for each scenario.
```

### Test Scenario 4: Secret Rotation
```
STEPS:
1. Generate a new client_id/client_secret pair (or new secret)
2. Configure the new credentials in Microsoft Entra ID
3. Verify provisioning continues to work
4. Delete/revoke the old secret
5. Verify provisioning still works with the new secret only

EXPECTED: Zero-downtime secret rotation is possible.
EVIDENCE NEEDED: Successful provisioning logs before and after rotation.
```

---

## 📊 PART 5: Final Validation Report Template

```markdown
# Entra App Gallery Validation Report
**Application Name**: [YOUR APP NAME]
**Date**: [DATE]
**SSO Protocol**: [SAML 2.0 / OIDC / Both]
**SCIM Support**: [Yes / No]

## Summary
| Category                  | Total | Pass | Fail | N/A | Recommended (Skipped) |
|--------------------------|-------|------|------|-----|----------------------|
| SSO Authentication        |       |      |      |     |                      |
| SSO ISV Requirements      |       |      |      |     |                      |
| SCIM API Requirements     |       |      |      |     |                      |
| SCIM Auth Requirements    |       |      |      |     |                      |
| SCIM ISV Requirements     |       |      |      |     |                      |
| Documentation             |       |      |      |     |                      |
| **TOTAL**                 |       |      |      |     |                      |

## 🚫 Blocking Issues (Must Fix Before Submission)
1. [Requirement #] — [Description of failure and remediation]

## ⚠️ Recommended Improvements (Non-Blocking)
1. [Requirement #] — [Description and benefit of implementing]

## ✅ Ready for Submission: [YES / NO]

## Next Steps
1. Fix all blocking issues listed above
2. Submit application at: https://microsoft.sharepoint.com/teams/apponboarding/Apps
3. Join Microsoft Partner Network: https://partner.microsoft.com/explore/commercial
```

---

## 🔗 Reference Links

| Resource | URL |
|----------|-----|
| App Gallery Listing Guide | https://learn.microsoft.com/en-us/entra/identity/enterprise-apps/v2-howto-app-gallery-listing |
| Build a SCIM Endpoint | https://learn.microsoft.com/en-us/entra/identity/app-provisioning/use-scim-to-provision-users-and-groups |
| SCIM Validator Tool | https://learn.microsoft.com/en-us/entra/identity/app-provisioning/scim-validator-tutorial |
| OIDC Auth Code Flow | https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow |
| SAML Protocol Reference | https://learn.microsoft.com/en-us/entra/identity-platform/single-sign-on-saml-protocol |
| Microsoft Branding Guidelines | https://learn.microsoft.com/en-us/entra/identity-platform/howto-add-branding-in-apps |
| Publisher Verification | https://learn.microsoft.com/en-us/entra/identity-platform/publisher-verification-overview |
| Client Credentials Flow for SCIM | https://learn.microsoft.com/en-us/entra/identity/app-provisioning/use-scim-to-provision-users-and-groups#oauth-20-client-credentials-grant-flow |
| M365 Developer Program (Free Test Tenant) | https://developer.microsoft.com/en-us/microsoft-365/dev-program |
| Submit Application Request | https://microsoft.sharepoint.com/teams/apponboarding/Apps |
| Known Limitations — SCIM Provisioning | https://learn.microsoft.com/en-us/entra/identity/app-provisioning/known-issues?pivots=app-provisioning |

---

> **Last Updated**: May 2026
> **Source**: [Microsoft Entra App Gallery Listing Requirements](https://learn.microsoft.com/en-us/entra/identity/enterprise-apps/v2-howto-app-gallery-listing)
> **Note**: Requirements may change. Always verify against the latest Microsoft documentation before submission.
