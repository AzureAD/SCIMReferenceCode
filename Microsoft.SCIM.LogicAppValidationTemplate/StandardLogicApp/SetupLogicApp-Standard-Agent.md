# Overview

Welcome to the pilot for **self-service validation of your provisioning
integration with Azure Logic apps**!

The Entra App Provisioning and Single Sign-On teams are currently
working on building a revamped onboarding experience where ISVs can
self-service onboard their provisioning or SSO integrations to the
Microsoft Entra app gallery. This will enable you to bring your
application into the Microsoft ecosystem faster and more efficiently
than ever before.

The self-service onboarding experience will consist of multiple
components:

1.  A process to self-service validate that your provisioning
    integration is ready to onboard to the Microsoft Entra app gallery
    via a provided Azure Logic app template

2.  A process to self-service validate that your SSO integration is
    ready to onboard to the Microsoft Entra app gallery via a browser
    extension

3.  An intake form in the Entra portal where you can submit a publishing
    request for your SSO and/or provisioning application

This document walks you through **\#1**, the process of self-service
validating that your provisioning integration is ready to onboard to the
Microsoft Entra app gallery. We are seeking your feedback on the
validation experience, including what you enjoy and what we can improve.

## Disclaimer

This feature is currently in PREVIEW. This information relates to a
pre-release product that may be substantially modified before it's
released. Microsoft makes no warranties, expressed or implied, with
respect to the information provided here.

## Support for preview

Microsoft Premier support will not provide support during the pilot. If
you have questions or feedback to provide, you may reach out to the
feature team managing this pilot at <aaduserprovisioning@microsoft.com>.

# Onboarding requirements

*<u>Technical requirements</u>*

For your application to be eligible to onboard to the Microsoft Entra
app gallery, your provisioning integration must meet the following
requirements:

- Submit the [SCIM Provisioning Testing Request Form for ISV
  Developers](https://forms.microsoft.com/pages/responsepage.aspx?id=v4j5cvGGr0GRqy180BHbR3elR4YvzS1IhaP_XITThvJUODY1UTJSSUFXTzFYMTQ0SkxSWTY4OTYzRi4u&route=shorturl)
  to allowlist your test tenant for the ISV Onboarding application.
  After submitting, wait for confirmation from the Microsoft team that
  your tenant has been approved before proceeding with any further
  onboarding steps.

- Support a SCIM 2.0 user or group endpoint (only one is required, but
  supporting both a user and group endpoint is recommended)

- Support the OAuth 2.0 Client Credentials grant as your primary
  authentication method

  - Note: Client Credentials is not required to participate in this
    pilot (i.e. you need to use ***long lived* bearer token** to test
    the Logic app). However, Client Credentials will be required to
    onboard to the Microsoft Entra app gallery

  - Currently, Client Credentials is the only authentication method we
    support for requests to onboard new provisioning integrations to the
    Microsoft Entra app gallery

  - Requirements for Client Credentials: An admin portal where a
    customer can generate a client ID and secret

  - Best practices: Support the ability to rotate secrets and delete old
    secrets

- Support updating multiple group memberships with a single PATCH
  request

- Support at least 25 requests per second per tenant to ensure that
  users and groups can be provisioned and deprovisioned without delay

- On querying for a nonexistent user with filter query, your server
  should respond with success and empty results. (on contrast with bad
  request as we see in some SCIM implementation)

- Your SCIM endpoint does not require features that Microsoft does not
  support today. Examples of features that the non-gallery SCIM app does
  not currently support:

  - Verbose PATCH calls

  - Support for batching calls (i.e. including multiple add operations
    in the same PATCH call)

  - Rate limiting

*<u>Validation requirements</u>*

This document provides you with instructions on how to self-service
validate your application, so that it is ready to onboard to the
Microsoft Entra app gallery. Once you complete the instructions in this
document, you will have completed the following pre-requisites:

- You should have set up a non-gallery SCIM app with a successful sync.
  This step requires:

  - A SCIM endpoint. If you need guidance on how to develop a SCIM
    endpoint, you can refer to our public documentation:
    <https://learn.microsoft.com/entra/identity/app-provisioning/use-scim-to-provision-users-and-groups>

  - An Entra ID tenant. If you don’t already have one, you can follow
    the instructions here to create one:
    <https://learn.microsoft.com/entra/identity-platform/quickstart-create-new-tenant>

  - You must have at least an **Application Administrator** role in the
    Entra ID tenant.

  - If your app will support only group provisioning, an [Entra ID
    Premium P1
    license](https://learn.microsoft.com/entra/fundamentals/licensing)
    is required for group-only provisioning to function (a P1 license is
    not required if “Provision all” is selected). A trial license will
    work. *Note: If you have an [M365 E3 or E5
    license](https://www.microsoft.com/microsoft-365/enterprise/microsoft-365-plans-and-pricing),
    Entra Premium is included as part of those license packages.*

- You should complete a successful run of our Logic app validation
  template, with no errors returned. This step requires:

  - In the same tenant where your non-gallery SCIM app is hosted, an
    Azure subscription for Logic app testing. The Logic app template
    functions on a [standard
    model](https://learn.microsoft.com/azure/logic-apps/single-tenant-overview-compare),
    meaning that you will likely incur a small monetary cost as a result
    of running the Logic app. This cost is expected to be small (less
    than 10 USD per month on an [Azure pay-as-you-go
    subscription](https://azure.microsoft.com/pricing/purchase-options/azure-account/search?icid=hybrid-cloud&ef_id=_k_CjwKCAiA8vXIBhAtEiwAf3B-gwGo45BSp9MkHu1_SsZHPGytGsYgUoGgKhwiVKvh4pCybuz62bqCnhoCYlgQAvD_BwE_k_&OCID=AIDcmm5edswduu_SEM__k_CjwKCAiA8vXIBhAtEiwAf3B-gwGo45BSp9MkHu1_SsZHPGytGsYgUoGgKhwiVKvh4pCybuz62bqCnhoCYlgQAvD_BwE_k_&gad_source=1&gad_campaignid=21496728177&gbraid=0AAAAADcJh_siQ5FaD4VnPUpZunMKSJ2sy&gclid=CjwKCAiA8vXIBhAtEiwAf3B-gwGo45BSp9MkHu1_SsZHPGytGsYgUoGgKhwiVKvh4pCybuz62bqCnhoCYlgQAvD_BwE)).

  - You must have permissions to create a Logic app under the
    appropriate subscription and resource group. This will require at
    least a [Logic app
    contributor](https://learn.microsoft.com/azure/logic-apps/logic-apps-securing-a-logic-app?tabs=azure-portal)
    role, but more permissions may be required depending on whether you
    also need to create a subscription and resource group.

  - If the Get API call do not have any results, the SCIM server should
    return 0 results but not a Bad Request.

*<u>Publishing requirements</u>*

While not required to participate in this pilot, the following is
required to complete the self-service publishing experience once it
becomes available for Private Preview in CY2026:

- Your tenant must be registered as a partner in Microsoft Partner
  Center and enrolled in the Microsoft AI Cloud Partner program.

- You must have documentation for your SCIM endpoint ready to publish.
  Once Private Preview starts, a documentation template will be
  available for you to use.

  - End customers should be able to access documentation about your SCIM
    endpoint on both your website and the Microsoft Learn website.

- Please provide us with engineering and support contacts for us to
  refer end customers to once your application is published to the
  Microsoft Entra app gallery.

# Two Ways to Validate Your Integration

You can validate your SCIM provisioning integration using either of two
methods:

|  | **Method A: AI Agent (Automated)** | **Method B: Manual Setup** |
|----|----|----|
| **What** | Use the `scim-onboarding.agent.md` file with an AI tool — the agent automates the entire process conversationally | Follow step-by-step instructions in this document to manually create resources, deploy the Logic App, configure parameters, and run tests |
| **Time** | 30–60 minutes (guided, mostly automated) | 1–3 hours (manual steps) |
| **Skills needed** | Basic familiarity with an AI chat tool | Azure Portal, Entra Portal, PowerShell/CLI |
| **Best for** | Faster setup, automated debugging and re-runs | Full control over every step |

**Both methods produce the same result:** a validated Logic App run that
you can submit to Microsoft.

Choose your preferred method: - **Method A** — Continue reading the
sections below for Agent-Automated setup - **Method B** — Jump to the
section **“Manual Setup”** later in this document

------------------------------------------------------------------------

# Method A: Agent-Automated Setup

The **SCIM Onboarding Agent** is an AI-powered agent that automates the
entire validation workflow. Instead of following 28 manual steps, you
have a conversation with the agent — it creates all Azure and Entra
resources, deploys the Logic App, runs tests, diagnoses failures, and
generates submission artifacts.

## What the Agent Does

The agent handles the entire validation process through a simple
conversation. Here’s what happens at a high level:

1.  **Asks you a few questions** — Your SCIM endpoint URL, bearer token,
    and authentication method, confirming Schema, Input parameters
    required for testing.
2.  **Sets up everything automatically** — Creates the Entra app, Azure
    resources, Logic App, assigning permissions — all the manual portal
    steps
3.  **Runs the tests** — Triggers all validation tests and monitors
    progress
4.  **Fixes problems** — If tests fail due to known issues, the agent
    fixes them and re-runs automatically, or provide recommendations for
    fixes in scim server
5.  **Gives you results** — Shows which tests passed/failed and
    generates the validation results that need to be submitted to
    Microsoft

You don’t need to navigate any portal or run any script — just answer
the agent’s questions and approve the commands it runs.

## Prerequisites

Before using the agent, ensure you have:

6.  **Azure CLI** installed and logged in — [Install Azure
    CLI](https://aka.ms/installazurecli)

- az login

7.  **Application Administrator** role in your Entra ID tenant

8.  **Azure subscription** with Logic App Contributor permissions

9.  Your **SCIM endpoint URL** (e.g., `https://scim.example.com/v2`)

10. A **long-lived bearer token** for your SCIM endpoint

11. The `scim-onboarding.agent.md` file (provided with this package)

## Setup Instructions

### Option A: Using Cline (VS Code Extension)

#### Setting Up Your AI Agent

#### The SCIM onboarding agent is a single instructions file that any AI coding agent can execute. You bring your own agent host and model — we provide the agent.

#### **Step 1:** Choose an AI agent host

#### Use any AI coding agent you already have, such as VS Code with GitHub Copilot, Cursor, Windsurf, Cline, Claude Code, or similar.

#### **Step 2:** Choose an AI model

#### The agent performs multi-step reasoning — collecting inputs, creating Azure resources, running tests, and debugging failures across Logic App workflows. Use a capable model:

|           |                           |
|-----------|---------------------------|
| Provider  | Minimum recommended model |
| Anthropic | Claude Opus 4 or later    |
| OpenAI    | GPT-4.1 or later          |
| Google    | Gemini 2.5 Pro or later   |

#### Smaller or older models may skip required inputs, fail to drill into errors, or retry failures without diagnosing the root cause.

#### **Step 3: \[Note:** these are just recommendation, at the agent is just a prompt file, feel free to load it anyway you can.\] 

#### Load the agent 

####  1. Create a project folder (e.g., C:\scim-validation)

####  2. Inside that folder, create the subfolder .github\agents\\

####  3. Place scim-onboarding.agent.md inside .github\agents\\

#### Your folder should look like:

####  C:\scim-validation\\

####  └── .github\\

####  └── agents\\

####  └── scim-onboarding.agent.md

####  1. Open this folder in your agent host:

####  - VS Code with GitHub Copilot — open the folder in VS Code. Copilot auto-discovers agents from .github/agents/. Invoke with @scim-onboarding in Copilot Chat.

####  - Cline / Cursor / Windsurf — open the folder, then load scim-onboarding.agent.md as a system prompt or custom instructions file.

####  - Claude Code — open the folder, then reference as context: @scim-onboarding.agent.md

####  - Other hosts — copy the file contents into your agent's system prompt or instructions field.

#### **Step 4:** Start

#### Send this message to the agent:

####  Validate my SCIM integration for Entra app gallery onboarding

#### The agent will ask for your SCIM endpoint, bearer token, OAuth credentials (if applicable), and guide you through the entire validation workflow.

When the agent requests to run commands, review and approve them

#### Step 5: Interact with the Agent

The agent will guide you through the process conversationally. Here’s
what to expect:

    Agent: What is your SCIM endpoint URL?
    You:   https://api.myapp.com/scim/v2

    Agent: What is your bearer token?
    You:   eyJhbGciOiJSUzI1NiIs...

    Agent: Does your SCIM endpoint use OAuth client credentials or a static bearer token?
    You:   Static bearer token

    Agent: ✅ Azure CLI — Logged in as admin@contoso.onmicrosoft.com
           ✅ SCIM endpoint — HTTP 200
           ✅ /Users — Supported
           ✅ /Groups — Supported
           
           I'll now create the Azure resources and deploy the Logic App...

    [Agent automatically creates everything and runs tests]

    Agent: ✅ VALIDATION RESULTS
           Create_User_Test: SUCCESS
           Update_User_Test: SUCCESS
           ...

### Option B: Using GitHub Copilot Chat (VS Code)

If you have **GitHub Copilot** with agent mode enabled:

#### Step 1: Set Up

- Ensure GitHub Copilot and Copilot Chat are installed in VS Code
- Place the `scim-onboarding.agent.md` file in your project folder
- Open the folder in VS Code

#### Step 2: Run

- Open **Copilot Chat** (Ctrl+Shift+I or click the Copilot icon)
- Switch to **Agent mode** (click the mode selector at the top of the
  chat panel)
- Reference the agent file and start:

<!-- -->

    @workspace Use the instructions in scim-onboarding.agent.md to validate my SCIM provisioning integration. Start the full validation workflow.

4.  Follow the prompts and approve command executions as requested

### Option C: Using Any AI Agent That Supports Tool Use

The `scim-onboarding.agent.md` file is a standard agent instruction
file. It can be used with any AI agent platform that supports: -
Reading/writing files - Executing CLI commands - Conversational
interaction

Simply provide the agent file as system instructions and ensure the
agent has access to a terminal with Azure CLI installed and
authenticated.

## Questions the Agent Will Ask You

The agent will ask you these questions one at a time during the
conversation. Have the answers ready before you start.

### Question 1: SCIM Endpoint URL

> “What is your SCIM endpoint URL?”

Provide the base URL of your SCIM 2.0 endpoint. This is the URL that
Microsoft Entra ID will send provisioning requests to.

**Example:** `https://api.myapp.com/scim/v2`

**Important:** Do NOT include the `?aadOptscim062020` feature flag in
this URL. If your endpoint requires this flag, it should only be
configured in the Entra portal’s Tenant URL field, not here.

### Question 2: Bearer Token

> “What is your bearer token for the SCIM endpoint?”

Provide a long-lived bearer token that the Logic App will use to
authenticate against your SCIM endpoint.

**Important:** The token must remain valid for at least 2 hours (the
test run can take 60–90 minutes). If your token is a short-lived JWT,
the tests will fail partway through. Use a token that lasts at least 24
hours.

### Question 3: Authentication Method

> “Does your SCIM endpoint use OAuth client credentials or a static
> bearer token?”

- **Static bearer token** — Most common for pilot testing. The agent
  will note that `Validate_Credentials_Test` will be skipped (this is
  expected).
- **OAuth client credentials** — If you choose this, the agent will ask
  4 follow-up questions:
  - **Client ID** — Your OAuth application’s client ID
  - **Client Secret** — Your OAuth application’s client secret
  - **Token Endpoint URL** — e.g., `https://auth.myapp.com/oauth/token`
  - **OAuth Scope** — The scope required for SCIM access (leave empty if
    not applicable)

### Question 4: Azure Subscription Selection

> “Which Azure subscription would you like to use?”

The agent will list your available Azure subscriptions. Pick the one
where the Logic App resources will be created. If you only have one
subscription, the agent selects it automatically.

### Question 5: Attribute Mapping Review

> “Would you like to keep these default attribute mappings, or do you
> want to customize them in the Entra portal first?”

After creating the SCIM app, the agent **fetches the default attribute
mappings** that Microsoft Entra automatically created and displays them
to you in a table:

    USER ATTRIBUTE MAPPINGS:
    | #  | Entra ID Source             | SCIM Target Attribute  |
    |----|-----------------------------|------------------------|
    | 1  | userPrincipalName           | userName               |
    | 2  | Switch([IsSoftDeleted],...) | active                 |
    | 3  | displayName                 | displayName            |
    | 4  | surname                     | name.familyName        |
    | 5  | givenName                   | name.givenName         |
    | .. | ...                         | ...                    |

These are the defaults provided by Entra. You then choose: - **“Keep the
default mappings — they look correct”** — Use this if your SCIM server
supports all the listed attributes. - **“I want to customize — let me go
to the Entra portal”** — The agent will give you portal instructions.
After you save your changes, come back and tell the agent you’re done.

After you confirm (either keeping defaults or after customizing), the
agent **fetches the mappings again** and displays the final schema for
one more confirmation: - **“Yes, this is correct — proceed with
testing”** - **“I want to make more changes — let me go back to the
portal”** (loops back) - **“Reset to defaults”**

The agent will not start testing until you explicitly confirm the final
schema.

### Question 6: Attribute Value Restrictions

> “Does your SCIM server have any restrictions on attribute values?”

The agent will first check your `/Schemas` endpoint for any
`canonicalValues` restrictions it can detect automatically. Then it asks
you to confirm or add more.

**Examples of restrictions:** - `jobTitle` must be one of: `"Engineer"`,
`"Manager"`, `"Director"` - `department` must be one of:
`"Engineering"`, `"Sales"`, `"Marketing"` - `employeeType` must be
`"Employee"` or `"Contractor"`

If you have restrictions, tell the agent the exact allowed values. It
will configure the test user profiles to use valid values — otherwise
the tests **will fail** with schema validation errors.

If you have no restrictions, say **“No restrictions”**.

# Method B: Manual Setup

## Pre-run: Setup

### Set up ISV Onboarding app

As mentioned in the [Requirements section](#_Requirements), before you
validate your provisioning integration, you must set up an ISV
Onboarding app with your desired configuration and start a successful
sync with that app. This section describes how to do so.

### Requirements

In the [Onboarding requirements section](#onboarding-requirements),
review the *Validation requirements* list to ensure that you have
everything you need to set up an ISV Onboarding app. You can get the
more information about the ISV Onboarding App setup and its settings
[ISVOnboarding Application
Guide.docx](https://microsoft-my.sharepoint-df.com/:w:/p/mandals/cQqsmnl-L4-hQpSTZ3Ihb8m_EgUCsIR-RpsQX2Z8Od38R5X6hA?isSPOFile=1&ovuser=72f988bf-86f1-41af-91ab-2d7cd011db47%2Cv-vishnudesu%40microsoft.com&wdExp=TEAMS-TREATMENT&web=1&TeamsCID=0ec6323a-c049-49ba-b6f6-441b5925c34d&clickparams=eyJBcHBOYW1lIjoiVGVhbXMtRGVza3RvcCIsIkFwcFZlcnNpb24iOiI0OS8yNjA1MTQxNjcxMyJ9&linkOpenTime=1780692576945).
Follow the steps below to get started.

### Instructions

1.  Sign in to the Entra portal at
    [entra.microsoft.com](https://entra.microsoft.com).

2.  Select **Enterprise applications \> New application \> Search for
    “Entra Gallery Provisioning Test App”**

<img src="./media/image1.png"
style="width:6.5in;height:1.85347in" />

<img src="./media/image2.png"
style="width:6.51031in;height:1.47177in" />

3.  Enter the name of your app, and click
    **Create**<img src="./media/image3.png"
    style="width:5.71761in;height:6.89336in" />

4.  Take note of the **Object ID** (this will be referred to as
    servicePrincipalID in the logic App).

<img src="./media/image4.png"
style="width:6.5in;height:5.90903in" />

5.  Set **Provisioning Mode,** Select “New Configuration”, enter your
    OAuth Client Credentails details, and select **Test Connection**.

6.  

<img src="./media/image5.png"
style="width:6.5in;height:1.93264in" />

<img src="./media/image6.png"
style="width:6.5in;height:2.95486in" />

7.  Create a provisioning job by creating connection and set up schema
    by navigating to **Provisioning \> Mappings \> Provision Users**.
    For more details on how to customize schema, you can check out our
    public documentation here: [Tutorial - Customize Microsoft Entra
    attribute mappings in Application Provisioning - Microsoft Entra ID
    \| Microsoft
    Learn](https://learn.microsoft.com/en-us/entra/identity/app-provisioning/customize-application-attributes).

<img src="./media/image7.png"
style="width:6.5in;height:3.48889in" />

**Prune the schema** with only attributes/mappings required and
supported by the ISV. Select the checkbox **Show Advanced Options.**
Then the **Edit attribute list** link will be displayed. Select the link
and verify the attributes. Update/delete the attributes depending on the
schema supported by the ISV endpoint.

<img src="./media/image8.png"
style="width:6.5in;height:2.31319in" />

<img src="./media/image9.png"
style="width:6.5in;height:2.77778in" />

The schema can be exported by selecting “Review your schema here”. Then
select “Download” from the open schema editor.

<img src="./media/image10.png"
style="width:6.5in;height:2.74097in" />

<img src="./media/image11.png"
style="width:6.5in;height:2.50694in" />

8.  In the **Overview** page, select **Start Provisioning** to start a
    provisioning job. If the provisioning job commences without errors,
    you are ready to move on to the next section.

<img src="./media/image12.png"
style="width:6.5in;height:3.36875in" />

9.  **Optional:** Once you’ve successfully started a provisioning job,
    submit an allow list request for faster sync cycles via this form:
    [Allow List for Self-Service Validation of Provisioning Integration
    (Pilot) – Fill out
    form](https://forms.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR1xPIYfdXw5FhHBIH8BxY9ZUQ0w0UEczQTdLT1gzUTIyVzVNOUFHTjNGQS4u).
    The Entra App Provisioning team will then work on allow listing your
    tenant and provisioning job. Once complete, you will have access to
    sync cycles that run more frequently than the standard 40-minute
    sync cycle, allowing you to test and iterate upon your provisioning
    integration quickly.

### Set up a Logic app for running automated tests

Once you have set up a non-gallery SCIM app and started the sync, you
will use our provided Logic app template to validate your provisioning
integration and ensure that it is ready to publish to the Microsoft
Entra app gallery. Logic app runs user tests and group tests on ISVs
behalf by using the non-gallery SCIM app that you set up. 

Once we release the full private preview for the full onboarding and
publishing experience, a successful run of the Logic app template will
allow you to submit a publishing request for your provisioning
integration, after which we will review and deploy your app.

### Requirements

In the [Onboarding requirements section](#onboarding-requirements),
review the *Validation requirements* list to ensure that you have
everything you need to set up a Logic app.

### Instructions

1.  Sign in to the Azure portal at <https://portal.azure.com>. You
    should use the same tenant as the one where you set up your
    non-gallery SCIM app.

2.  Use the search bar to navigate to the **Subscriptions** blade.

<img src="./media/image13.png" style="width:6.5in;height:1in" />

3.  Select the appropriate Azure subscription and create a resource
    group. This is the subscription and resource group that your Logic
    app will be attached to.

<img src="./media/image14.png"
style="width:6.5in;height:2.10903in" />

<img src="./media/image15.png"
style="width:6.5in;height:3.20833in" />

4.  Use the searchbar to navigate to the **Logic app**s blade.

5.  Select **Add \> WorkFlow Service Plan(Standard)**. *Note: The Logic
    app functioning on a Standard model means that you may be billed on
    your Azure description depending on level of usage. The amount is
    expected to be small—see the [Onboarding requirements
    section](#onboarding-requirements) for more details, under*
    Validation requirements*.*

<img src="./media/image16.png"
style="width:6.5in;height:1.76944in" />

<img src="./media/image17.png"
style="width:6.5in;height:2.18264in" />

6.  Provide Name , select the ResourceGroup created earlier.

<img src="./media/image18.png"
style="width:6.5in;height:8.72153in" />

7.  Got to “Storage” tab. Change the “Blob service Diagnostics settings”
    to configure now and select the DefaultWorkspace.

<img src="./media/image19.png"
style="width:6.22597in;height:8.3698in" />

8.  Keep them as it is for the rest of the settings. Configure the
    settings of your Logic app as desired. Once you are done, click
    **Review + create**.

<img src="./media/image20.png"
style="width:5.30614in;height:8.22103in" />

9.  Once the Logic app finishes deploying, open the Logic app.

<img src="./media/image21.png"
style="width:6.5in;height:1.84583in" />

10. Select “Logic Apps” in the Entra Portal. Select the created Logic
    app.

11. Download the **logicAppTemplate.json **file from
    the** Microsoft.SCIM.LogicAppValidationTemplate** folder of our GitHub repository: <u>https://github.com/AzureAD/SCIMReferenceCode/tree/master/Microsoft.SCIM.LogicAppValidationTemplate</u> (copy/paste
    this URL into your browser). *Note: The folder includes
    a **README.md** file that lists out the various tests that the Logic
    app will run. This may be helpful for your reference.*

12. In the Logic app, select **Development Tools \> Logic app code
    view**. Copy/paste the code from the template in the previous step
    and click **Save**. The **Logic app designer** view should then
    update with the various test cases that our template will
    automatically run for you.

<img src="./media/image22.png"
style="width:6.50278in;height:3.18082in" />

13. You may choose to use Azure CLI or PowerShell for the following
    steps. Download all the files from
    [SCIMReferenceCode/Microsoft.SCIM.LogicAppValidationTemplate/StandardLogicApp
    at master · AzureAD/SCIMReferenceCode ·
    GitHub](https://github.com/AzureAD/SCIMReferenceCode/tree/master/Microsoft.SCIM.LogicAppValidationTemplate/StandardLogicApp).
    Keep all the Files in the same folder.

<img src="./media/image23.png"
style="width:6.5in;height:2.57361in" />

14. Open Azure CLI, Select all the files and upload all the files to
    Azure.

<img src="./media/image24.png"
style="width:6.5in;height:1.42708in" />

<img src="./media/image25.png"
style="width:6.5158in;height:3.72207in" />

<img src="./media/image26.png"
style="width:6.5in;height:4.96111in" />

15. Get the Subscription, ResourceGroup and LogicAppName from the
    Overview page.

<img src="./media/image27.png"
style="width:6.5in;height:2.60972in" />

16. Run the following command

.\Deploy-LogicAppWorkflows.ps1 \`

-SubscriptionId \$subscriptionId \`

-ResourceGroup \$resourceGroupName \`

-LogicAppName \$LogicAppName

<img src="./media/image28.png"
style="width:5.94875in;height:2.43784in" />

All the Workflows of the Logicapp are deployed as below.

<img src="./media/image29.png"
style="width:6.5in;height:2.28264in" />

<img src="./media/image30.png"
style="width:6.5in;height:4.06458in" />

<img src="./media/image31.png"
style="width:6.5in;height:4.75208in" />

17. Next, we will enable system-assigned managed identity for secure
    resource access. Select **Settings \> Identity**.

<img src="./media/image32.png"
style="width:6.5in;height:4.16806in" />

<img src="./media/image33.png"
style="width:6.5in;height:2.20347in" />

18. Set the **Status** in the **System assigned** tab to **On**. Select
    **Yes** in the confirmation dialog that pops up.

<img src="./media/image34.png"
style="width:6.5in;height:2.86181in" />

19. Select **Save**.

<img src="./media/image35.png"
style="width:6.5in;height:3.02431in" />

20. Take note of the object ID of the managed identity. You will need
    this object ID for the script that you will run in a few steps.

<img src="./media/image36.png"
style="width:6.5in;height:3.59792in" />

21. Now let’s work on granting the owner role to the Logic app. Select
    **Azure role assignments**.

22. In the **Azure role assignments** page, click on **Add role
    assignment** and select the **Owner** role.

<img src="./media/image37.png"
style="width:6.5in;height:2.28889in" />

<img src="./media/image38.png"
style="width:5.16964in;height:5.3232in" />

<img src="./media/image39.png"
style="width:6.5in;height:1.27083in" />

<img src="./media/image40.png"
style="width:6.5in;height:1.275in" />

Once the owner role has been granted to the Logic app, you can now work
on assigning the proper permissions to the Logic app so that it can
invoke various Graph queries as part of the automated tests it will run
(the Logic app will create, update, and delete users and groups, query
provisioning logs, etc.).

You may choose to use Azure CLI or PowerShell for the following steps.

23. Go to the sample script provided in the
    [appendix](#script-for-assigning-permissions-to-your-logic-app) of
    this document. Copy the script for your records, and update the
    value of the **\$miObjId** field with the object ID of your Logic
    app’s managed identity.

<img src="./media/image41.png"
style="width:6.5in;height:3.02708in" />

24. Run the script using the command-line interface of your choice. If
    using a UI like Azure Cloud Shell that provides you with an option
    to upload a file, you may opt to copy the script into a file, upload
    the file, then run the script.

*<u>How to upload and run a script using Azure Cloud Shell</u>*

<img src="./media/image42.png"
style="width:6.5in;height:1.84653in" />

<img src="./media/image43.png"
style="width:6.5in;height:2.33681in" />

<img src="./media/image44.png"
style="width:6.5in;height:4.56528in" />

<img src="./media/image45.png"
style="width:6.5in;height:2.05764in" />
<img src="./media/image46.png"
style="width:6.5in;height:2.66181in" />

Once the script successfully runs, you will have assigned all the
necessary roles to the managed identity of your Logic app.

### Logic App Explanation:

The Logic App is built on Azure Logic Apps Standard and is divided into
separate sections. It consists of 5 workflows that work together using a
nested workflow architecture. The Orchestrator workflow is the entry
point, and it calls the other workflows as child workflows.
Initialization workflow initializes the required steps to run the tests
in the Logic App. <img src="./media/image47.png"
style="width:2.89624in;height:5.76122in" />

The next section contains the tests. Tests are bundled into user and
group and scim workflows. All the User Tests are in ‘UserTests_workflow’
and Group tests in ‘GroupTests_worklfow’ and SCIM tests in
“SCIMTests_workflow’. Each test can be run individually or all together
using the \`EnabledTests\` parameter.

<img src="./media/image48.png"
style="width:6.5in;height:4.99792in" />

You can select each workflow and can view the tests it has in the
Designer.Each test can be further drilldown by selecting the down arrow
and to get into details of stages and the actions. Each stage and action
can be drilled down till the inputs and outputs are displayed for each
action.

<img src="./media/image49.png"
style="width:5.57369in;height:6.90721in" />

<img src="./media/image50.png"
style="width:6.5in;height:3.89583in" />

The last section in the Orchestartor_workflow is for post run results
evaluation.

<img src="./media/image51.png"
style="width:4.78192in;height:2.77122in" />

## Run: Steps to Run Logic app

Before we run your Logic app, let’s provide values for your Logic app’s
required run parameters. Save the Logic app after updating parameters
before Run. Open Orchestrator WorkFlow in Designer and Update the
Parameters .

<img src="./media/image52.png"
style="width:6.5in;height:0.92083in" />

### Providing Values To Parameters

25. The **servicePrincipalId** is the **objectId** of the ISV onboarding
    app you created in the [previous section](#_Set_up_your).

<img src="./media/image53.png"
style="width:4.50063in;height:2.63578in" />

26. Enter your SCIM endpoint.

    1.  **Note**: don’t include feature flags like aadOptscim062020 in
        the scim endpoint here. Even if you have to configure your non
        gallery app with feature flags.

<img src="./media/image54.png"
style="width:4.52146in;height:2.71913in" />

27. Enter your SCIM bearer token.

<img src="./media/image55.png"
style="width:4.4277in;height:2.67746in" />

28. Under **testUserDomain**, enter a verified domain that belongs to
    your tenant. This domain will be used to create test users in Entra
    ID and provision them to your SCIM endpoint as part of the automated
    tests that the Logic app template will run. *Note: A Logic app
    template that successfully completes all tests will clean up any
    test users that were created during that run. If the Logic app
    template does not complete a full, clean run, test users may not be
    cleaned up. For example, stubs of the test user accounts will remain
    in your tenant if the Logic app template fails the Delete User tests
    or if you choose to interrupt the Logic app template before it has
    the chance to complete delete operations.*

<img src="./media/image56.png"
style="width:4.57356in;height:2.6462in" />

29. Under defaultUserProperties give the different sets of user
    Properties values to test. The Logic App takes one choose one set of
    the defaultUserProperties to create User and another set for
    updating User. Selection is random based on no. of sets.

<img src="./media/image57.png"
style="width:6.19878in;height:4.03181in" />

<img src="./media/image58.png"
style="width:4.6875in;height:6.5in" />

**  **

30. **EnabledTests** can take one of the below values. We support
    running all tests in parallel, running individual tests, or running
    tests related to only users or only groups. ***Only one value should
    be provided.***

**“All” -** All tests will run

**“UserTests” -** All of the User Tests will run. Groups and SCIM Tests
are skipped.

**“GroupTests” –** All of the Group Tests will run. User and SCIMTests
are skipped.

**“SCIMTests” –** All of the SCIM tests will run. User and Group test
will be skipped

> "All",

                    "UserTests",

                    "GroupTests",

                    "SCIMTests",

                    "Create_User_Test",

                    "Update_User_Test",

                    "Delete_User_Test",

                    "Disable_User_Test",

                    "User_Update_Manager_Test",

                    "Restore_User_Test",

                    "POD_User_Test",

                    "Create_Group_Test",

                    "Update_Group_Test",

                    "Delete_Group_Test",

                    "Group_Update_Add_Member_Test",

                    "Group_Update_Remove_Member_Test",

                    "POD_Group_Test",

                    "Restore_Group_Test",

"Schema_Discoverability_Test",

"SCIM_Null_Update_Test",

"SCIM_User_Create_Test",

"SCIM_Group_Create_Test",

"SCIM_User_Update_Test",

"SCIM_Group_Update_Test",

"SCIM_Update_Manager_Test",

"SCIM_User_Pagination_Test",

"SCIM_Group_Pagination_Test",

"Validate_Credentials_Test",

"Federated_Identity_Test"

<img src="./media/image59.png"
style="width:4.4277in;height:2.54202in" />

31. **IsSoftDeleted** — This parameter is automatically determined by the Logic App
    based on whether the `active` attribute is present in the target directory
    schema. You do not need to set this manually. The `Disable_User_Test` always
    runs and tests soft-delete (PATCH `accountEnabled=false`). The `Delete_User_Test`
    is optional — a failure produces a WARNING but does not fail the overall run.

<img src="./media/image60.png"
style="width:4.37561in;height:2.31282in" />

32. Update scimClientId with client id.

<img src="./media/image61.png"
style="width:4.49021in;height:2.52119in" />

33. Update client secret

<img src="./media/image62.png"
style="width:4.43812in;height:2.54202in" />

34. Update ISV token endpoint

<img src="./media/image63.png"
style="width:4.40686in;height:2.71913in" />

## Run the Logic App

35. You’re now ready to run the Logic app! Navigate to **WorkFlows\>**
    Select **Orchestrtor_workflow**,
    <img src="./media/image64.png"
    style="width:6.5in;height:3.10139in" />

36. From the Orchestartor_workflow’s designer, select “**Run**”

<img src="./media/image65.png"
style="width:6.5in;height:2.47153in" />

### Post-Run: Verify the Runs and the next steps

## Verify the Runs

37. You can view logs of your runs in the **Runs history** blade. When
    clicking on an entry in **Runs history**, you check the final
    results of that entry, including the list of tests that were run,
    alongside status and any errors that may have come up.

<img src="./media/image66.png"
style="width:6.5in;height:1.42639in" />

## Debugging

38. Debugging Logic App:

Check the Final_TestResults action of the Orchestrator_workflow’s run to
learn about the tests and their results.

<img src="./media/image67.png"
style="width:6.5in;height:2.60764in" />

In Final_TestResults -\> Select 'Show raw Outputs’.

<img src="./media/image68.png"
style="width:5.61111in;height:5.53819in" />

<img src="./media/image69.png"
style="width:6.5in;height:0.73611in" />

For each test, “testResult” shows the success / failure / skipped. In
case of failure the phase and action name for the failure is displayed.
Copy the action name. “Ctrl + Click” on the runLink. It opens the child
workflows run. Search the action name and can debug and look furthermore
for error details. Tip section below shows how to debug further and
search for specific actions. Identify the failures from failed actions
inputs/outputs give further details about why that call is failed.
Verify if the schema is valid and all the parameters are set according
to the Schema. Fix the parameters or schema and run the logic app again.

Note on SKIPPED tests: The following SKIPPED results are expected and
correct, no action is needed. The Logic App automatically detects these
capabilities during initialization and skips tests that do not apply.

\(1\) User_Update_Manager_Test: Skipped when the manager attribute is
not present in the target directory schema. Only applies if your app
supports manager provisioning.

\(2\) SCIM_Update_Manager_Test: Skipped when the manager attribute is
not present in the target directory schema. This test validates direct
SCIM PATCH calls to set, change, and remove the manager attribute.

\(3\) Federated_Identity_Test: Skipped when OAuth client credentials
are not configured. Only applies if your app uses Workload Identity
Federation.

Note on WARNING results: The following tests produce a WARNING (not
FAILED) if they fail — they do not block overall validation:

- Delete_User_Test (hard delete is optional)
- Delete_Group_Test (group delete is optional)
- Restore_Group_Test (group restore is optional)
- SCIM_Group_Pagination_Test (group pagination is optional)

Note on Group tests: Group provisioning is mandatory. If your application
does not support groups (no Group schema in /Schemas endpoint), all group
tests will FAIL (not skip). Your application must support both Users and
Groups provisioning.

“provisioningErrorDetails” gives the glimpse of Error information in
case of failure.

*Tip:* More details about the run can be found by drilling down to the
test definition and checking the input/output. Here’s a sample of how
the output may look like:

<img src="./media/image70.png"
style="width:6.5in;height:1.76458in" />

*Another tip:* Go to the run you want to debug further. In the left you
can query for a specific stage / action on the magnifying glass icon.

<img src="./media/image71.png"
style="width:6.5in;height:3.65625in" />

# Understanding the Test Results

### The Logic App runs 25 tests across three workflows: 7 User tests, 7 Group tests, and 11 SCIM compliance tests. "For detailed descriptions of each test and what they validate, see the SCIM Validation Test Overview: [SCIMReferenceCode/Microsoft.SCIM.LogicAppValidationTemplate/StandardLogicApp at master · AzureAD/SCIMReferenceCode · GitHub](https://github.com/AzureAD/SCIMReferenceCode/tree/master/Microsoft.SCIM.LogicAppValidationTemplate/StandardLogicApp)"

### Not all tests will run for every application — the Logic App automatically detects what your app supports and skips tests that do not apply. 

### This section explains how to find and read the results. 

### Where to Find the Results

**If using the agent (Method A):** The agent fetches and displays the
results automatically. No portal navigation needed.

**If using manual setup (Method B):** 1. In the Azure portal, go to your
Logic App → **Workflows** → **Orchestrator_Workflow** 2. Click **Run
history** and select your run 3. Find the action called
`Final_TestResults` (near the bottom of the workflow) 4. Click on it →
select **Show raw outputs** 5. The JSON output contains all test results

### The Results JSON Structure

Each test result in the `Final_TestResults` output looks like this:

    {
      "testName": "Create_User_Test",
      "testResult": "success",
      "provisioningErrorDetails": "",
      "recommendationUrl": "",
      "runLink": "https://portal.azure.com/#view/...",
      "message": "Click the runLink and search for the action Compose_Final_Results for more info."
    }

Here’s what each field means:

| Field | What It Tells You |
|----|----|
| `testName` | The name of the test (e.g., `Create_User_Test`, `Update_Group_Test`) |
| `testResult` | `"success"` if the test passed. If the test failed, this contains the failure description including the phase and action name that failed (e.g., `"FAILED - [Delete Phase] Failed Action: Delete_Step5_Delete_Group_By_Id"`) |
| `provisioningErrorDetails` | Empty if the test passed. On failure, contains the HTTP status code, error body, and error message from the Graph API or SCIM endpoint call that failed. **This is the most important field for debugging.** |
| `recommendationUrl` | A link to Microsoft documentation that may help resolve the issue |
| `runLink` | A direct link to the child workflow run in the Azure portal. Click this to drill into the workflow designer and inspect individual action inputs/outputs. |
| `message` | Instructions on how to find more details in the workflow run |

### Possible Test Result Values

| Result | Meaning | Action Required? |
|----|----|----|
| `"success"` | Test passed — your SCIM endpoint handled the operation correctly | ✅ None |
| `"FAILED - [Phase] Failed Action: <action_name>"` | Test failed at a specific action. The phase (e.g., Create Phase, Update Phase, Delete Phase) and action name tell you exactly where it broke. | ❌ Yes — see Debugging below |
| `"skipped"` | Test was skipped because a prerequisite was not met (e.g., `User_Update_Manager_Test` skipped when manager attribute is not in target schema, or `Federated_Identity_Test` skipped when OAuth is not configured) | ✅ None (if intentional) |
| `"Token acquisition failed"` | Only for `Validate_Credentials_Test` — the OAuth token request failed. Expected when using a static bearer token. | ✅ None (if using static token) |
| `"The SCIM schema does not support all attributes..."` | `Schema_Discoverability_Test` found that your SCIM schema doesn’t advertise all the attributes in the provisioning mappings. The details show how many are supported vs mapped. | ⚠️ Prune your attribute mappings to match |

### Sample Results — All Tests Passed

    {
      "testResults": [
        { "testName": "Create_User_Test", "testResult": "success" },
        { "testName": "Update_User_Test", "testResult": "success" },
        { "testName": "Disable_User_Test", "testResult": "success" },
        { "testName": "Delete_User_Test", "testResult": "success" },
        { "testName": "User_Update_Manager_Test", "testResult": "success" },
        { "testName": "Restore_User_Test", "testResult": "success" },
        { "testName": "POD_User_Test", "testResult": "success" },
        { "testName": "Create_Group_Test", "testResult": "success" },
        { "testName": "Update_Group_Test", "testResult": "success" },
        { "testName": "Delete_Group_Test", "testResult": "success" },
        { "testName": "Group_Update_Add_Member_Test", "testResult": "success" },
        { "testName": "Group_Update_Remove_Member_Test", "testResult": "success" },
        { "testName": "POD_Group_Test", "testResult": "success" },
        { "testName": "Restore_Group_Test", "testResult": "success" },
        { "testName": "Schema_Discoverability_Test", "testResult": "success" },
        { "testName": "SCIM_Null_Update_Test", "testResult": "success" },
        { "testName": "SCIM_User_Create_Test", "testResult": "success" },
        { "testName": "SCIM_Group_Create_Test", "testResult": "success" },
        { "testName": "SCIM_User_Update_Test", "testResult": "success" },
        { "testName": "SCIM_Group_Update_Test", "testResult": "success" },
        { "testName": "SCIM_Update_Manager_Test", "testResult": "success" },
        { "testName": "SCIM_User_Pagination_Test", "testResult": "success" },
        { "testName": "SCIM_Group_Pagination_Test", "testResult": "success" },
        { "testName": "Validate_Credentials_Test", "testResult": "Token acquisition failed" },
        { "testName": "Federated_Identity_Test", "testResult": "skipped" }
      ],
      "overallResult": "Failed"
    }

> **Note:** Even with 23/25 tests passing, the `overallResult` shows
> `"Failed"` because `Validate_Credentials_Test` did not pass. This is
> expected when using a static bearer token — it does not block
> onboarding. `Federated_Identity_Test` is skipped when OAuth is not configured.

### Sample Results — With a Failure

    {
      "testName": "Delete_Group_Test",
      "testResult": "FAILED - [Delete Phase] Failed Action: Delete_Step5_Delete_Group_By_Id",
      "provisioningErrorDetails": {
        "provisioningLogs": {
          "statusCode": 403,
          "body": {
            "error": {
              "code": "Authorization_RequestDenied",
              "message": "Insufficient privileges to complete the operation."
            }
          }
        }
      }
    }

**How to read this failure:** - `testResult` tells you it failed during
the **Delete Phase** at the action `Delete_Step5_Delete_Group_By_Id` -
`provisioningErrorDetails` shows the actual HTTP error: **403** with
`Authorization_RequestDenied` — the Logic App’s managed identity doesn’t
have sufficient permissions - **Fix:** Assign the missing Graph API
permission to the managed identity and re-run

### How to Debug a Failed Test

- **Read** `provisioningErrorDetails` in the results JSON — this usually
  tells you the root cause (HTTP status code + error message)
- **Click the** `runLink` — this opens the child workflow run in the
  Azure portal
- **Search for the failed action name** (from `testResult`) in the
  workflow designer
- **Click the failed action** → check **Inputs** (what was sent) and
  **Outputs** (what came back)
- The HTTP response body in the outputs contains the exact error from
  the Graph API or your SCIM endpoint

**If using the agent (Method A):** The agent does steps 1–5
automatically and tells you the root cause and fix.

### What “Passing” Means for Onboarding

To proceed with gallery onboarding, all applicable tests must pass. The
following are acceptable exceptions: -

- Validate_Credentials_Test or Federated_Identity_Test failing when
  using a static bearer token (OAuth or WIF will be required for
  production). At least one of these two must PASS.

- User_Update_Manager_Test and SCIM_Update_Manager_Test being SKIPPED
  when the manager attribute is not present in the target directory
  schema. The Logic App checks whether manager is in the target
  directory attributes and automatically skips these tests if it is not.

- Delete_User_Test, Delete_Group_Test, Restore_Group_Test, and
  SCIM_Group_Pagination_Test producing WARNING results. These are
  optional tests — a failure does not block onboarding.

## Automatic Failure Diagnosis

If tests fail, the agent automatically:

- Fetches the `Final_TestResults` from the Orchestrator workflow
- Drills into child workflow actions to find the actual HTTP error
- Matches against known issue patterns
- For auto-fixable issues (e.g., missing permissions, schema validation
  errors, feature flags in endpoint), applies the fix and re-runs
  automatically
- For ISV-side issues (e.g., SCIM filter not supported, 404 on empty
  queries), explains exactly what to fix

### Common Auto-Fixed Issues

| Issue | What the Agent Does |
|----|----|
| `aadOptscim062020` feature flag in endpoint | Removes the flag from parameters, re-runs |
| Missing Graph API permission | Assigns the missing permission, waits for propagation, re-runs |
| Schema validation error (canonical values) | Extracts allowed values, updates user profiles, re-runs |
| Missing fields in `defaultUserProperties` | Adds the missing property to all user profiles, re-runs |

### Issues Requiring Your Action

| Issue | What You Need to Do |
|----|----|
| Bearer token expired | Provide a new long-lived token |
| SCIM filter not supported | Implement filter support on matching properties |
| 404 on empty filter queries | Return 200 + empty results (mandatory) |
| Group PATCH not supported | Implement multi-member PATCH on /Groups |
| Rate limiting (429) | Support ≥25 req/s |

------------------------------------------------------------------------

## 

# Submit test results 

Once you have completed the validation process and passed 100% of all
tests, you can now submit your validation
results. This step is required, whether you used the agent-automated or
manual method to validate your provisioning integration. 

You will submit results via the **private preview developer blade** for
ISV self-service onboarding in the Entra admin portal. Once
you submit the results, the Microsoft team will have access to your
validation tests, as well as your application details. 

## What happens next? 

During private preview, once we receive your validation results, the
Microsoft team will work with you to deploy your application to the
Microsoft Entra app gallery.  

When our publisher experience is released during public preview, the
deployment process will be automated. When the validation results
are submitted through the developer blade, those results will be matched
with the corresponding submission request in the publisher experience,
and the application will be automatically deployed to the Microsoft
Entra app gallery in preview mode. 

## Requirements 

To view the developer blade in the non-gallery application, your tenant
will have to be allow listed for private preview. Your tenant should
have been allow listed as part of the [intake
process](bookmark://_Step_1:_Enroll). 

## Instructions 

Here’s how to find the developer blade and submit validation results: 

10. Sign in to the Entra portal
    at [entra.microsoft.com](https://entra.microsoft.com/). 

11. Paste the following URL into your
    browser: [https://entra.microsoft.com/?Microsoft_AAD_Connect_Provisioning=tip&feature.enableSelfServiceOnboardingDeveloperPortal=true&feature.consoletelemetry=true#view/Microsoft_AAD_IAM/StartboardApplicationsMenuBlade/~/AppAppsPreview](https://entra.microsoft.com/?Microsoft_AAD_Connect_Provisioning=tip&feature.enableSelfServiceOnboardingDeveloperPortal=true&feature.consoletelemetry=true%23view/Microsoft_AAD_IAM/StartboardApplicationsMenuBlade/~/AppAppsPreview).
    Once your tenant is allow listed, this URL will
    allow your allow listed tenant to view the private preview developer
    blade. 

12. Select **Enterprise applications \> All applications**. Select the
    non-gallery application that either (a) the AI-powered agent created
    if you used the [agent-automated
    method](bookmark://_Method_A:_Agent-Automated) of creating
    and validating a non-gallery application, or (b) you created
    yourself if you followed the [manual set-up
    method](bookmark://_Method_B:_Manual). 

13. Navigate to **Provisioning \> Submit validation results**, and click
    on the **Submit validation results** button in the command bar. 

<img src="./media/image78.png"
style="width:6.5in;height:1.46944in" /> 

You will be directed to a form where you will submit the details of the
Logic App that was used for validation (depending on the set-up method
you followed, this Logic App may have been created automatically by our
agent or by you). 

14. Under the Validation results tab: 

<!-- -->

1.  Input the [request ID you received from the Microsoft
    team](bookmark://_Step_1:_Enroll) after you submitted your intake
    request in the **Submission request ID** field. *Note: This is a
    placeholder request ID that we generated for the purpose of Private
    Preview. Once we release the publisher experience, you would input
    the ID of the submission request initialized by your organization.
    This ID allows us to map the developer validation results to the
    correct submission request.* 

2.  Select **Yes** for the question in the **Run a Logic App** section. 

3.  Input the subscription, resource group, and name of the Logic App
    used to validate your provisioning integration. 

4.  Input the run ID in the **Run ID **field. *Note: The Run ID must be
    associated with a run of the Logic App template where 100% of test
    cases pass. If you are having trouble finding the Run ID, read [this
    section](bookmark://_How_do_I).* 

<img src="./media/image79.png"
style="width:6.5in;height:5.03056in" /> 

<img src="./media/image80.png"
style="width:6.35417in;height:3.52083in" /> 

15. Under the **Preview **tab, review the application details that we
    extracted from your validation results. Verify that all application
    details, including attribute mappings and job settings, are correct
    before proceeding. *Note: You will not be able to edit the form
    after submission.* 

16. Under the **Attestations** tab, confirm that you meet all
    requirements listed and select **Yes** for all required
    fields. *Note: You will not be able to proceed if there are any
    questions where you do not respond **Yes**. These attestations all
    relate to security requirements that we are not able to check
    programmatically using the Logic App template.* 

17. Under the **Review + submit** tab, click **Submit**. 

Once you have gotten to this point, congratulations! You have completed
the required steps to self-service validate that your application is
ready to onboard to the Microsoft Entra app gallery, and you have made
sure that your results are accessible to the Microsoft team. At this
point, please **contact the Microsoft team** to let us know
that you’ve completed all the steps listed in this guide, and we will
work with you to deploy your application to the gallery. 

## How do I find the Run ID? 

Having trouble finding a Logic App Run ID that is associated with a run
where 100% of test cases passed? Here’s how you can find it. 

1.  Sign in to the Azure portal
    at [portal.azure.com](https://portal.azure.com/). 

2.  Type “logic app” into the search bar and click on **Logic apps**. 

3.  Once you’re in the **Logic apps** blade, click on the Logic App that
    was created by either the agent or you when you were validating your
    provisioning integration. 

4.  Navigate to **Workflows \> Orchestrator_Workflow**. 

<img src="./media/image81.png"
style="width:6.5in;height:2.60833in" /> 

5.  Select **Run history**. 

6.  Identify your latest run where the status is listed
    as **Succeeded**. Click on the copy icon to copy the value listed in
    the **Identifier** column. *Note: You can verify that the run passed
    100% of test cases by clicking on the hyperlinked ID. This will take
    you to the designer view of the **Orchestrator_Workflow**, where you
    can scroll to the bottom to check the **Final_TestResults** stage.
    If the **Final_TestResults** stage completed with no errors, the
    run passed all validation tests.* 

Congratulations, you just found the Run ID associated with a successful
run of your Logic App! This is the Run ID you’ll use
when submitting validation results through the [developer
blade](bookmark://_Instructions). <img src="./media/image82.png"
style="width:6.5in;height:3.14653in" /> <img src="./media/image83.png"
style="width:6.5in;height:2.81528in" />

# Frequently Asked Questions: 

Note: Below are some of the known issues and most probable explanations.
Each issue could be caused by many other reasons too.

1.  **Why do errors occur when the aadOptscim062020 feature flag is used
    with a Logic App SCIM endpoint?**

**Explanation  **
The aadOptscim062020 feature flag is supported only for Microsoft Entra
ID provisioning scenarios. This flag is not required in logic app
configuration. When it is configured on a Logic App SCIM endpoint, SCIM
GET requests may fail with Bad Request errors.

**Resolution  **
Remove the aadOptscim062020 flag from the Logic App SCIM endpoint
configuration.  
Configure this flag only while setting up connection in non gallery app
by going to:

Microsoft Entra ID → Enterprise Application → Provisioning → Tenant URL

2.  **Why do Logic App test runs fail intermittently due to
    authentication issues?**

**Explanation  **
Logic App test runs can exceed the lifetime of short-lived access
tokens. When a token expires during execution, authentication failures
may occur.

**Resolution  **
Use long-lived access tokens when running Logic App tests.

3.  **Why does the Get_Templates action return an Unauthorized error?**

**Explanation  **
The Logic App’s Managed Identity does not have sufficient permissions to
access the required template resources.

**Resolution  **
Assign the appropriate roles to the Managed Identity and allow time for
permission changes to take effect.  
If the issue continues, recreate the Logic App and reconfigure the
Managed Identity and permissions.

4.  **Why do SCIM requests with attribute-based filters fail?**

**Explanation  **
Microsoft Entra ID can issue SCIM GET requests with filters on **any
attribute configured as a matching property**. When such a request is
sent, the target SCIM endpoint is expected to support filtering on that
matching property.

If the SCIM server does not support filtering on one or more matching
properties configured in Entra ID, the filtered request may fail and
appear as an error in provisioning logs.

**Resolution  **
Ensure that the SCIM endpoint supports filtered GET requests for **all
attributes configured as matching properties** in:

Enterprise Application → Provisioning → Attribute Mappings

**Examples**

- **Email as a matching property**  
  If emails\[type eq "work"\].value is configured as a matching
  property, the SCIM endpoint must support filtering on this attribute.
  The following example API call **must be supported**:

> GET /scim/v2/Users?filter=emails\[type eq "work"\].value eq
> "user@contoso.com"

5.  **Why do Logic App tests fail with a SCIM 409 Conflict error?**

**Possible Explanation  **
In some scenarios, the SCIM service may not respond as expected to
update requests, which can result in retry attempts and a 409 Conflict
error.

**Resolution  **
Re-run the test to confirm whether the issue is transient.  
If the issue occurs consistently, verify that the SCIM service responds
correctly to update requests and does not introduce response delays.

6.  **Why does a Logic App SCIM GET API call fail when querying a
    nonexistent user?**

**Explanation**  
The SCIM specification allows a server to return a 404 Not Found
response when a queried user does not exist. Although this behavior is
SCIM compliant, it is not supported by this system.

This system requires SCIM servers to return a **successful response (200
OK) with zero results** for filter-based queries that do not match any
users. If a SCIM server returns a 404 Not Found response for a
nonexistent user, the Logic App SCIM GET call fails due to
incompatibility with the expected response behavior.

**Resolution**  
As part of onboarding, ensure that the SCIM endpoint is configured to
return a **200 OK response with zero results** (for example,
totalResults: 0 and an empty Resources array) when a queried user does
not exist.

This behavior is a **mandatory onboarding requirement** and is validated
during the onboarding checklist process.

# Provide feedback

Once you get a chance to test the pilot, please fill out the following
feedback form: [Feedback Form for Self-Service Validation of
Provisioning Integrations (Pilot) – Fill out
form](https://forms.microsoft.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbR1xPIYfdXw5FhHBIH8BxY9ZUNTJTRkc0SDUxOTdHSFk5UEZQVkVZRjhTMy4u)

In the form, you may specify whether you are interested in participating
in a follow-up feedback session with the Entra App Provisioning feature
team. In this feedback session, we would ask you more questions about
your experience.

We’re excited to hear more from you! Thank you for participating in our
pilot—your insights help us make Microsoft Entra ID better.

# Appendix

## Script for assigning permissions to your Logic app

[SCIMReferenceCode/Microsoft.SCIM.LogicAppValidationTemplate/AssignRolesTOManagedIdentity-LogicApps
1.ps1 at master · AzureAD/SCIMReferenceCode ·
GitHub](https://github.com/AzureAD/SCIMReferenceCode/blob/master/Microsoft.SCIM.LogicAppValidationTemplate/AssignRolesTOManagedIdentity-LogicApps%201.ps1)

## Script for Logic App Validation

[SCIMReferenceCode/Microsoft.SCIM.LogicAppValidationTemplate/StandardLogicApp/ValidateLogicAppRun-Standard.ps1
at master ·
AzureAD/SCIMReferenceCode](https://github.com/AzureAD/SCIMReferenceCode/blob/master/Microsoft.SCIM.LogicAppValidationTemplate/StandardLogicApp/ValidateLogicAppRun-Standard.ps1)
