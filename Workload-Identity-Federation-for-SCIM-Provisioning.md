# Workload Identity Federation: A New Authentication Method for SCIM Provisioning

**Date:** 9th June 2026

---

## Table of Contents

1. [Overview](#overview)
2. [What is Workload Identity Federation?](#what-is-workload-identity-federation)
3. [Why JWT Bearer Assertion for Provisioning?](#why-jwt-bearer-assertion-for-provisioning)
4. [WIF Configuration Flow (3-Step Admin Setup)](#wif-configuration-flow-3-step-admin-setup)
5. [How the JWT Bearer Assertion Flow Works](#how-the-jwt-bearer-assertion-flow-works)
6. [What Information ISVs Receive from Entra ID](#what-information-isvs-receive-from-entra-id)
7. [What ISVs Must Do to Support This](#what-isvs-must-do-to-support-this)
8. [SAP SuccessFactors Example](#sap-successfactors-example)
9. [JWT Bearer Assertion Request & Response Format](#jwt-bearer-assertion-request--response-format)
10. [Key Benefits](#key-benefits)
11. [Comparison: Legacy vs. Workload Identity Federation](#comparison-legacy-vs-workload-identity-federation)
12. [Security Considerations](#security-considerations)
13. [FAQ](#faq)
14. [References](#references)

---

## Overview

**Workload Identity Federation** (WIF) is a new authentication method that enables the Microsoft Entra Provisioning Service (SyncFabric) to authenticate to ISV SCIM endpoints **without storing long-lived secrets** (client secrets, passwords, or bearer tokens). Instead, it uses the **OAuth 2.0 JWT Bearer Assertion** profile (based on [RFC 7523](https://datatracker.ietf.org/doc/html/rfc7523)) to dynamically obtain short-lived access tokens by presenting a signed JWT assertion to the ISV's token endpoint.

This approach was first implemented for SAP SuccessFactors provisioning and is being extended to other ISV integrations in the Entra provisioning gallery.

---

## What is Workload Identity Federation?

Workload Identity Federation establishes a **trust relationship** between Microsoft Entra ID (as the identity provider) and an external application's token endpoint (the ISV/SaaS app). Instead of exchanging static credentials like a username/password or a long-lived OAuth bearer token, the flow works as follows:

1. **Microsoft Entra ID** acts as a **trusted token issuer**
2. The **ISV's SCIM endpoint** trusts tokens issued by Entra ID
3. **No secrets are stored** in the provisioning configuration — the trust is based on cryptographic verification of signed JWT tokens

This is a fundamental shift from the traditional model where admins had to:
- Generate an API token in the ISV's admin console
- Paste that token into the Entra provisioning configuration
- Manually rotate the token before it expires

---

## Why JWT Bearer Assertion for Provisioning?

| Problem with Legacy Approach | How JWT Bearer Assertion Solves It |
|---|---|
| Static bearer tokens expire and cause provisioning outages | Short-lived tokens are obtained dynamically per provisioning cycle |
| Secrets stored in provisioning config can be leaked | No secrets stored — trust is based on federated identity |
| Manual token rotation burden on IT admins | Fully automated, zero-touch token lifecycle |
| Difficult to audit and track credential usage | JWT assertion events are logged in both Entra and ISV audit logs |
| No standard protocol — each ISV has custom token generation | Follows OAuth 2.0 JWT Bearer Assertion (RFC 7523) standard |

---

## WIF Configuration Flow (3-Step Admin Setup)

The administrator must perform **3 steps** to set up the integration for their app. The administrator must have at least the **Application Administrator** role on the customer's tenant.

### Step 1: Configure ISV App and Workload Identity in Entra Portal

In the Entra App Gallery, the administrator:

1. Creates an app for provisioning for that ISV and configures the Connectivity
2. On the Connectivity Page, picks **"Workload Identity Federation"** as the auth method
3. Clicks **"Select Workload Identity"** — customers can register a new Workload Identity App or reuse an existing one they have already used to set up with the ISV

After the access app is configured, the UX displays the following values to be **copied to the ISV Portal**:

| Value | Format |
|---|---|
| **Issuer (iss)** | `https://login.microsoftonline.com/<TenantID>/v2.0` |
| **JWKS URL** | `https://login.microsoftonline.com/<TenantID>/discovery/v2.0/keys` |
| **Subject (sub)** | `<Sync Fabric Workload Identity 1P app object ID>` |
| **Audience (aud)** | `api://{WorkloadIdentity_appid}/.default` |

### Step 2: Set up SCIM Client with JWKS in ISV Portal

In the ISV portal, the administrator sets up a client for the ISV's SCIM endpoint. As part of setting up the auth integration, the administrator copies the values from Step 1 (issuer, JWKS URL, subject, audience) into the ISV portal.

The ISV portal will then display the following values, needed for Step 3:

| Value | Description |
|---|---|
| **Client ID** | An ID that identifies this specific integration for this customer on the ISV side |
| **Token URL** | The endpoint where the provisioning service can present the Entra-issued token to get an access token for the SCIM endpoint |
| **SCIM URL** | The endpoint to be used for users/groups provisioning |

### Step 3: Complete Setup in Entra

Back in the Entra App Gallery:

1. Copy **Client ID**, **Token URL**, and **SCIM URL** from the ISV portal to the Connectivity Page
2. Click **"Test Connection"** — this invokes the backend to perform all the token exchange steps and validate the integration
3. **Save** the configuration — the app is now ready to configure provisioning

---

## How the JWT Bearer Assertion Flow Works

```
┌─────────────────┐         ┌──────────────────────┐         ┌─────────────────┐
│                 │         │                      │         │                 │
│  Microsoft      │   1.    │  ISV Token Endpoint  │         │  ISV SCIM       │
│  Entra ID       │────────>│  (OAuth 2.0)         │         │  Endpoint       │
│  Provisioning   │         │                      │         │                 │
│  Service        │<────────│                      │         │                 │
│                 │   2.    │                      │         │                 │
│                 │         └──────────────────────┘         │                 │
│                 │                                          │                 │
│                 │   3. SCIM requests with access_token     │                 │
│                 │─────────────────────────────────────────>│                 │
│                 │                                          │                 │
│                 │<─────────────────────────────────────────│                 │
│                 │   4. SCIM response                       │                 │
└─────────────────┘                                          └─────────────────┘
```

### Step-by-Step Flow:

1. **Entra ID Provisioning Service** sends a **JWT Bearer assertion request** to the ISV's OAuth 2.0 token endpoint:
   - Includes a **`client_assertion`** — a signed JWT issued by Microsoft Entra ID
   - Uses grant type: `client_credentials` with `client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer`
   - The JWT assertion serves as proof of client identity — no client secret is needed

2. **ISV's Token Endpoint** validates the JWT assertion:
   - Verifies the JWT signature using Microsoft's public OIDC keys
   - Validates the `issuer`, `audience`, and `subject` claims
   - Issues an **access token** back to the Entra provisioning service

3. **Entra Provisioning Service** uses the issued access token as a **Bearer token** in SCIM API calls (GET, POST, PATCH, DELETE) to the ISV's SCIM endpoint

4. **ISV's SCIM Endpoint** processes the provisioning requests (create users, update attributes, disable accounts, etc.)

---

## What Information ISVs Receive from Entra ID

When Entra ID sends the JWT bearer assertion request, the **JWT assertion (client_assertion)** contains the following key claims:

### JWT Claims in the Entra-Issued Assertion

| Claim | Description | Example Value |
|---|---|---|
| `aud` (Audience) | Workload Identity App ID | `api://b5ba7a93-4452-4522-aeb4-a2b5da870c16` |
| `iss` (Issuer) | Customer tenant v2 issuer endpoint | `https://login.microsoftonline.com/ce5f061f-abe6-4e40-9615-301f87bcb7f0/v2.0` |
| `sub` (Subject) | Sync Fabric Workload Identity 1P app object ID | `<Sync Fabric Workload Identity 1P app object ID>` |
| `oid` (Object ID) | Workload Identity Object ID | `d2f8ee76-c549-45b8-a143-f5b640669704` |
| `appid` | Workload Identity App ID | `b5ba7a93-4452-4522-aeb4-a2b5da870c16` |
| `tid` (Tenant ID) | Customer Tenant ID | `ce5f061f-abe6-4e40-9615-301f87bcb7f0` |
| `iat` (Issued At) | Token issue timestamp | `1772175916` |
| `nbf` (Not Before) | Token not valid before | `1772175916` |
| `exp` (Expiration) | Token expiry timestamp | `1772179816` |
| `ver` | Token version | `2.0` |

### Example Token Payload

```json
{
  "aud": "api://b5ba7a93-4452-4522-aeb4-a2b5da870c16",
  "iss": "https://login.microsoftonline.com/ce5f061f-abe6-4e40-9615-301f87bcb7f0/v2.0",
  "iat": 1772175916,
  "nbf": 1772175916,
  "exp": 1772179816,
  "appid": "b5ba7a93-4452-4522-aeb4-a2b5da870c16",
  "appidacr": "2",
  "idp": "https://login.microsoftonline.com/ce5f061f-abe6-4e40-9615-301f87bcb7f0/v2.0",
  "oid": "d2f8ee76-c549-45b8-a143-f5b640669704",
  "sub": "<Sync Fabric Workload Identity 1P app object ID>",
  "tid": "ce5f061f-abe6-4e40-9615-301f87bcb7f0",
  "ver": "2.0"
}
```

> [!NOTE]
> The `iss` and `sub` claims in the Entra-issued token identify the **customer's tenant** and the **Sync Fabric Workload Identity 1P app object**, respectively. The ISV validates these against the values provided during the 3-step configuration.

### Additional Context Provided

- **OIDC Discovery Endpoint**: ISVs can fetch Microsoft's signing keys from:
  `https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration`
- **JWKS URI**: Public keys for signature verification:
  `https://login.microsoftonline.com/{tenantId}/discovery/v2.0/keys`
- **Tenant ID**: Identifies which customer tenant is provisioning users

---

## What ISVs Must Do to Support This

ISVs need to implement **three key capabilities** to support Workload Identity Federation:

### 1. Support JWKS-Based JWT Validation

ISVs must validate the Entra-issued JWT assertion using Microsoft's published JWKS (JSON Web Key Set):

- **Fetch signing keys** from the JWKS endpoint provided during configuration
- **Periodically check** for key updates (keys may rotate at any time)
- **Cache keys by `kid`** (Key ID) — match the `kid` in the JWT header to the correct public key for signature verification

### 2. Allow Customers to Configure Expected Claims

The ISV portal must allow administrators to configure the expected claim values for each integration:

- **`aud` (Audience)** — the audience value the ISV expects in incoming JWTs
- **`sub` (Subject)** — the subject identifier for the Sync Fabric Workload Identity 1P app
- **JWKS URL** — the endpoint to fetch Microsoft's signing keys

These values are provided by the Entra portal during Step 1 of the configuration flow and entered by the administrator in Step 2.

### 3. Issue Short-Lived Access Tokens

After successful JWT validation, the ISV token endpoint should:

- Issue an **access token with a lifetime between 1–6 hours**
- Scope the token to SCIM operations for the identified customer
- Return the token in standard OAuth 2.0 format:

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIs...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "scim"
}
```

#### Token Request Format (from Entra to ISV)

The Entra Provisioning Service sends the following request to the ISV's token endpoint:

```
POST /oauth2/token HTTP/1.1
Host: auth.isv-app.com
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id={client_id_from_ISV_portal}
&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer
&client_assertion=<signed JWT from Microsoft Entra ID>
&scope=scim
```

> **Note:** This follows **RFC 7523 Section 2.2** — the JWT is used for *client authentication* with the `client_credentials` grant type. The `client_assertion` parameter contains the Entra-issued JWT, and `client_assertion_type` indicates it is a JWT bearer assertion.

---

## SAP SuccessFactors Example

SAP SuccessFactors is the first application to support this new authentication model for SCIM provisioning from Microsoft Entra ID.

### Configuration Flow:

1. **Admin navigates** to Entra Admin Center → Enterprise Applications → SAP SuccessFactors
2. **Selects "Provisioning"** and chooses **Workload Identity Federation** as the authentication method (instead of basic auth or bearer token)
3. **Entra ID automatically**:
   - Creates a federated identity credential on the provisioning app's service principal
   - Configures the JWT bearer assertion parameters (audience, issuer, subject mapping)
4. **No secrets to manage** — the admin only provides the SuccessFactors SCIM endpoint URL
5. **Provisioning cycles** automatically perform JWT bearer assertion authentication before each sync

### What Changes for SAP:

- SAP's SCIM endpoint accepts the Entra-issued JWT via the JWT bearer assertion flow
- SAP validates the JWT against Microsoft's public OIDC keys
- SAP issues a scoped access token for provisioning operations
- No API keys or basic auth credentials are stored or exchanged

---

## JWT Bearer Assertion Request & Response Format

### JWT Bearer Assertion Request (from Entra to ISV)

```http
POST /oauth2/token HTTP/1.1
Host: auth.successfactors.example.com
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials
&client_id={client_id_from_ISV_portal}
&client_assertion_type=urn%3Aietf%3Aparams%3Aoauth%3Aclient-assertion-type%3Ajwt-bearer
&client_assertion=eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6Ijk...
&scope=scim.readwrite
```

### Token Response (from ISV to Entra)

```json
{
  "access_token": "sl.Adf8sHg7jKl3nM...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "scim.readwrite"
}
```

### Subsequent SCIM Request (from Entra to ISV)

```http
GET /scim/v2/Users?filter=userName eq "john.doe@contoso.com"
Host: scim.successfactors.example.com
Authorization: Bearer sl.Adf8sHg7jKl3nM...
```

---

## Key Benefits

### For IT Administrators
- ✅ **Zero secret management** — no more bearer tokens to paste and rotate
- ✅ **Reduced provisioning outages** — no more expired token failures
- ✅ **Simplified setup** — just select the auth method and provide the SCIM URL
- ✅ **Better security posture** — short-lived tokens, no stored credentials

### For ISVs / SaaS Providers
- ✅ **Standards-based** — uses OAuth 2.0 JWT Bearer Assertion (RFC 7523)
- ✅ **Reduced support burden** — fewer "expired token" support tickets
- ✅ **Multi-tenant ready** — tenant identification via JWT claims
- ✅ **Audit trail** — every JWT assertion grant is logged and traceable

### For Security Teams
- ✅ **No long-lived credentials** in provisioning configuration
- ✅ **Cryptographic trust** instead of shared secrets
- ✅ **Full auditability** of JWT assertion issuance and usage
- ✅ **Automatic credential rotation** built into the protocol

---

## Comparison: Legacy vs. Workload Identity Federation

| Aspect | Legacy (Bearer Token / Basic Auth) | Workload Identity Federation |
|---|---|---|
| **Authentication Type** | Static bearer token or username/password | OAuth 2.0 JWT Bearer Assertion (RFC 7523) |
| **Secret Storage** | Token/password stored in Entra config | No secrets stored |
| **Token Lifetime** | Long-lived (months/years) | Short-lived (~1 hour per cycle) |
| **Rotation** | Manual, admin-driven | Automatic, per provisioning cycle |
| **Outage Risk** | High (expired tokens) | Minimal (dynamic token refresh) |
| **Protocol Standard** | Proprietary per ISV | RFC 7523 (JWT Bearer Assertion Profile) |
| **Setup Complexity** | Generate token in ISV → paste in Entra | Select auth method → done |
| **Multi-Tenant Isolation** | Token-per-tenant | JWT claims identify tenant |
| **Audit Trail** | Limited | Full JWT assertion logging |

---

## Security Considerations

1. **JWT Signature Verification**: ISVs MUST validate the JWT signature using Microsoft's published JWKS keys. Never accept unsigned or self-signed tokens.

2. **Audience Validation**: Always verify the `aud` claim matches your registered application. Reject tokens with unexpected audiences.

3. **Issuer Validation**: Confirm the `iss` claim matches the expected Microsoft Entra ID v2 issuer format: `https://login.microsoftonline.com/{tenantId}/v2.0`

4. **Token Expiry**: Always check `exp` and `nbf` claims. Reject expired or not-yet-valid tokens.

5. **Tenant Isolation**: Use the `tid` claim to ensure provisioning operations are scoped to the correct customer tenant. Cross-tenant data leaks are a critical risk if this is not enforced.

6. **Rate Limiting**: Implement rate limiting on the token endpoint to prevent abuse.

7. **TLS Requirement**: All JWT assertion and SCIM communications MUST use TLS 1.2 or higher.

---

## FAQ

### How does the ISV identify which customer is making the request?

The customer's tenant can be identified in two ways:
- The **tenant ID** may be embedded in the ISV's token URL path (e.g., `https://auth.isv.com/{tenantId}/oauth2/token`)
- The **`client_id`** in the request body uniquely identifies the customer's integration on the ISV side

### What is the recommended key caching strategy?

ISVs should **cache signing keys by `kid`** (Key ID). When a JWT arrives, match the `kid` from the JWT header to a cached key. If no matching key is found, fetch the latest keys from the JWKS endpoint.

### How often do Microsoft's signing keys rotate?

Keys can rotate **at any time** without prior notice. ISVs should not assume a fixed rotation schedule. Instead, use the `kid` field to detect when a new key is needed and fetch the updated key set from the JWKS endpoint.

### What should the ISV do if the key endpoint is unavailable?

If the JWKS endpoint is temporarily unavailable:
- **Use cached keys** if a matching `kid` is found in the cache
- **Fail the request** only if the JWT references a `kid` that is not in the cache and the JWKS endpoint cannot be reached

---

## References

| Resource | Link |
|---|---|
| RFC 7523 — JWT Profile for OAuth 2.0 Client Authentication and Authorization Grants | [https://datatracker.ietf.org/doc/html/rfc7523](https://datatracker.ietf.org/doc/html/rfc7523) |
| RFC 7521 — Assertion Framework for OAuth 2.0 | [https://datatracker.ietf.org/doc/html/rfc7521](https://datatracker.ietf.org/doc/html/rfc7521) |
| Microsoft Entra Workload Identity Federation | [https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation) |
| Configure Federated Identity Credentials | [https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation-create-trust](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation-create-trust) |
| SCIM Provisioning with Microsoft Entra | [https://learn.microsoft.com/en-us/entra/identity/app-provisioning/use-scim-to-provision-users-and-groups](https://learn.microsoft.com/en-us/entra/identity/app-provisioning/use-scim-to-provision-users-and-groups) |
| SAP SuccessFactors Integration Reference | [https://learn.microsoft.com/en-us/entra/identity/app-provisioning/sap-successfactors-integration-reference](https://learn.microsoft.com/en-us/entra/identity/app-provisioning/sap-successfactors-integration-reference) |
| Microsoft Identity Platform OIDC Metadata | `https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration`|
---

*Last updated: June 9, 2026*
