---
page_type: sample
languages:
- csharp
products:
- dotnet
description: SCIM provisioning reference code  
urlFragment: "update-this-to-unique-url-stub"
---

# SCIM Reference Code

<!-- 
Guidelines on README format: https://review.docs.microsoft.com/help/onboard/admin/samples/concepts/readme-template?branch=master

Guidance on onboarding samples to docs.microsoft.com/samples: https://review.docs.microsoft.com/help/onboard/admin/samples/process/onboarding?branch=master

Taxonomies for products and languages: https://review.docs.microsoft.com/new-hope/information-architecture/metadata/taxonomies?branch=master
-->

Use this reference code to get started on building a [SCIM](https://docs.microsoft.com/azure/active-directory/manage-apps/use-scim-to-provision-users-and-groups) endpoint. It contains guidance on how to implement:

1. Basic requirements for CRUD operations on a user and group object (also known as resources in SCIM). 
2. Optional features such as filtering and pagination. 

> [!NOTE]
> This code is intended to help you get started building your SCIM endpoint and is provided "AS IS." It is intended as a reference and there is no guarantee of it being actively maintained or supported. 

## Capabilities 

|Endpoint|Description|
|---|---|
|/User|**Perform CRUD operations on a user resource:** <br/> 1. Create <br/> 2. Update <br/> 3. Delete <br/> 4. Get <br/> 5. List <br/> 6. Filter|
|/Group|**Perform CRUD operations on a group resource:** <br/> 1. Create <br/> 2. Update <br/> 3. Delete <br/> 4. Get <br/> 5. List <br/> 6. Filter |
|/Schemas|**Retrieve one or more supported schemas.**<br/>The set of attributes of a resource supported by each service provider can vary. (e.g. Service Provider A supports “name”, “title”, and “emails” while Service Provider B supports “name”, “title”, and “phoneNumbers” for users).|
|/ResourceTypes|**Retrieve supported resource types.**<br/>The number and types of resources supported by each service provider can vary. (e.g. Service Provider A supports users while Service Provider B supports users and groups).|
|/ServiceProviderConfig|**Retrieve service provider's SCIM configuration**<br/>The SCIM features supported by each service provider can vary. (e.g. Service Provider A supports Patch operations while Service Provider B supports Patch Operations and Schema Discovery).|

## Getting Started
The `Microsoft.SystemForCrossDomainIdentityManagement` project contains the code base for building a SCIM API. The `Microsoft.SCIM.Sample` project is there as a sample for using the project. A step by step guide for starting up with the project can be found [here](docs/get-started.md)

## Navigating the reference code

This reference code was developed as a .Net core MVC web API for SCIM provisioning. The three main folders are Schemas, Controllers, and Protocol.

1. The **Schemas** folder includes:
    * The models for the User and Group resources along with some abstract classes like Schematized for shared functionality.
    * An Attributes folder which contains the class definitions for complex attributes of Users and Groups such as addresses.
2. The **Controllers** folder contains:
    * The controllers for the various SCIM endpoints. Resource controllers include HTTP verbs to perform CRUD operations on the resource (GET, POST, PUT, PATCH, DELETE). 
    * Controllers rely on services to perform the actions.
3.	The **Services** folder contains logic for actions relating to the way resources are queried and updated.
    * The service methods are exposed via the IProviderService interface.
    * The reference code has services to return users and groups.
    * The services are based on Entity Framework and DbContext is defined by the class ScimContext.
3. The **Protocol** folder contains logic for actions relating to the way resources are returned according to the SCIM RFC such as:
    * Returning multiple resources as a list.
    * Returning only specific resources based on a filter.
    * Turning a query into a list of linked lists of single filters.
    * Turning a PATCH request into an operation with attributes pertaining to the value path. 
    * Defining the type of operation that can be used to apply changes to resource objects.

### Contents
| File/folder       | Description                                |
|-------------------|--------------------------------------------|
| `ScimRefrenceAPI` | Sample source code.                        |
| `Screenshots`     | Screenshots for README.      |
| `.gitignore`      | Define what to ignore at commit time.      |
| `CHANGELOG.md`    | List of changes to the sample.             |
| `CONTRIBUTING.md` | Guidelines for contributing to the sample. |
| `README.md`       | This README file.                          |
| `LICENSE`         | The license for the sample.                |

## Common scenarios
|Scenario|How-to|
|---|---|
|Enable or disable authorization|**Steps**<br/>1. Navigate to the **UsersController.cs** or **GroupController.cs** files located in **ScimReferenceApi > Controllers**.<br/>2. Comment or uncomment out the authorize command.|
|Add additional filterable attributes|**Steps**<br/>1. Navigate to the **FilterUsers.cs** or **FilterGroups.cs** files located in **ScimReferenceApi > Protocol**.<br/>2. Update the method to include the attributes that you would like to support filtering for. |
|Support additional user resource extensions|**Steps**<br/>1. Copy the **EnterpriseUser.cs** file located in **ScimReferenceApi > Schemas**.<br/>2. Rename the class to your custom extension name (e.g. customExtensionName.cs)<br/>3. Update the schema to match the desired naming convention.<br/>4. Repeat steps 1 - 3 with the **EnterpriseAttributes.cs** file (located in ScimReferenceApi > Schemas > Attributes) and update it with the attributes that you need.|

## Authorization
The SCIM standard leaves authentication and authorization relatively open. You could use cookies, basic authentication, TLS client authentication, or any of the other methods listed [here](https://tools.ietf.org/html/rfc7644#section-2). You should take into consideration security and industry best practices when choosing an authentication/authorization method. Avoid insecure methods such as username and password in favor of more secure methods such as OAuth. Azure AD supports long-lived bearer tokens (for gallery and non-gallery applications) as well as the OAuth authorization grant (for applications published in the app gallery). This reference code allows you to either turn authorization off to simplify testing, generate a bearer token, or bring your own bearer token. 

**Option 1**: Turn off authorization (this should only be used for testing) 
* Navigate to the **UsersController.cs** or **GroupController.cs** files located in **ScimReferenceApi > Controllers**.<br/>2. Comment out the authorize command.

**Option 2**: Get a bearer token signed by Microsoft security bearer (should only be used for testing, not in production) 
* Post to to the key endpoint with the string "SecureLogin" to retrieve a token. The token is valid for 120 minutes (the validity can be changed in the key controller). 

**Option 3**: Bring your own token
* **Option 3a**: Generate your own token that matches the specifications of the reference code. 
  * By default the issuer, audience, and signer must be "Microsoft.Security.Bearer"
  * These are defaults to get started testing quickly. They should not be relied on in production. 
* **Option 3b**: Generate your own token and update the specifications of the reference code to match your token. 
  * Change the specifications in the configure service section of the startup.cs class.
  * Specify the authorization settings you would like to validate. 
  * Generate a token on your own that matches those specifications. 

Provided below are test cases that you can use to ensure that your SCIM endpoint is compliant with the SCIM RFC. 


## Contributing to the reference code

This project welcomes contributions and suggestions! Like other open source contributions, you will need to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When submitting a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g. status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
