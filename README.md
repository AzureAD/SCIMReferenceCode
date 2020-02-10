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

## Prerequisites

1. [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) (required)
2. [.NET core 3.1 or above](https://dotnet.microsoft.com/download/dotnet-core/3.1) (required)
3. [IIS](https://www.microsoft.com/download/details.aspx?id=48264) (required)
4. [Postman](https://www.getpostman.com/downloads/) (optional)

## Clone the repo and build your SCIM endpoint

The solution is located in the ScimReferenceApi folder and can be built and run from VisualStudio locally or hosted in the cloud.

#### Steps to run the solution locally
1. Click **"Clone or download"** and click **"Open in Desktop"** OR copy the link.
 
2. If you chose to copy the link, open Visual Studio and choose **"Clone or check out code**. 

3. Use the copied link from Github to make a local copy of all files.

4. The Solution Explorer should open. Navigate to **Microsoft.SCIM.sln** view by double-clicking on it.

5. Click **IIS Express** to execute. The project will launch as a web page with the local host URL.

#### Steps to host the solution in the Azure
1. Open Visual Studio and sign into the account that has access to your hosting resources. 
2. While in the **SCIMReference.sln** view, right-click the **SCIMReferenceApi** file in the Solution Explorer and select **"Publish"**.

    ![Cloud Publiosh](Screenshots/CloudPublish.png)

3. Click create profile. Make sure **App Service** and **"Create new"** is selected. 

    ![Cloud Publish 2](Screenshots/CloudPublish2.png)
    
4. Walk through the options in the dialog. 
5. Rename the app to a desired name of your choice. The name is used for both the app name and the SCIM Endpoint URL.

    ![Cloud Publish 3](Screenshots/CloudPublish3.png)

6. Select the resource group and plan you would like to use and click **"Publish"**.

All the endpoints are are at the **{host}/scim/** directory and can be interacted with standard HTTP requests. The **/scim/** route can be modified in the **ControllerConstant.cs** file located in **AzureADProvisioningSCIMreference > ScimReferenceApi > Controllers**.

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

## Test your SCIM endpoint
Provided below are test cases that you can use to ensure that your SCIM endpoint is compliant with the SCIM RFC. 

#### Postman instructions
1. Download the [Postman client](https://www.getpostman.com/downloads/).
2. Import the Postman collection by copying the link [here](https://aka.ms/ProvisioningPostman) and pasting it into Postman as shown below:

    ![Postman](Screenshots/Postman.png)

3. Create a Postman environment for testing by specifying the following variables below:
    * **If running the project locally**:

        |Variable|Value|
        |---|---|
        |Server|localhost|
        |Port|*The port you are using (e.g. **:44355**)|
        |API|scim|
            
    * **If hosting the endpoint in Azure**:
        
        |Variable|Value|
        |---|---|
        |Server|scimreferenceapi19.azurewebsites.net|
        |Port||
        |API|scim|

4. Turn off SSL Cert verification by navigating to  **File > Settings > General > SSL certificate verification**.

    ![Postman2](Screenshots/Postman2.png)

5. Ensure that you are authorized to make requests to the endpoint:
    * **Option 1**: Turn off authorization for your endpoint (this is fine for testing purposes, but there must be some form of authorization for apps being used by customers in production).
    * **Option 2**: POST to key endpoint to retrieve a token.

6. Run your tests!

#### Tests executed

|Test|Description|
|---|---|
|CRUD operations on a Resource|Test that resources can be made, modified and deleted.|
|Resource filtering|Test that specific resources are located and returned by filtered value (e.g. **?filters=DisplayName+eq+%22BobIsAmazing%22**).|
|Attribute filtering|Test that specific attributes are located and returned (e.g. **?attributes=userName,emails**).|

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

## Common scenarios
|Scenario|How-to|
|---|---|
|Enable or disable authorization|**Steps**<br/>1. Navigate to the **UsersController.cs** or **GroupController.cs** files located in **ScimReferenceApi > Controllers**.<br/>2. Comment or uncomment out the authorize command.|
|Add additional filterable attributes|**Steps**<br/>1. Navigate to the **FilterUsers.cs** or **FilterGroups.cs** files located in **ScimReferenceApi > Protocol**.<br/>2. Update the method to include the attributes that you would like to support filtering for. |
|Support additional user resource extensions|**Steps**<br/>1. Copy the **EnterpriseUser.cs** file located in **ScimReferenceApi > Schemas**.<br/>2. Rename the class to your custom extension name (e.g. customExtensionName.cs)<br/>3. Update the schema to match the desired naming convention.<br/>4. Repeat steps 1 - 3 with the **EnterpriseAttributes.cs** file (located in ScimReferenceApi > Schemas > Attributes) and update it with the attributes that you need.|

## Contents


| File/folder       | Description                                |
|-------------------|--------------------------------------------|
| `ScimRefrenceAPI` | Sample source code.                        |
| `Screenshots`     | Screenshots for README.      |
| `.gitignore`      | Define what to ignore at commit time.      |
| `CHANGELOG.md`    | List of changes to the sample.             |
| `CONTRIBUTING.md` | Guidelines for contributing to the sample. |
| `README.md`       | This README file.                          |
| `LICENSE`         | The license for the sample.                |

## Contributing to the reference code

This project welcomes contributions and suggestions! Like other open source contributions, you will need to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When submitting a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g. status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
