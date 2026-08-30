using System.Text.Json;
using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;

namespace PlaywrightTests;

[TestFixture]
public class EndpointTests : PlaywrightTest
{
    private IAPIRequestContext? Request;
    static string PROTOCOL = Environment.GetEnvironmentVariable("PROTOCOL") ?? "http";
    static string SERVER = Environment.GetEnvironmentVariable("SERVER") ?? "localhost";
    static string PORT = Environment.GetEnvironmentVariable("PORT") ?? ":5000";
    static string API = Environment.GetEnvironmentVariable("API") ?? "scim";
    private static string _baseUrl = $"{PROTOCOL}://{SERVER}{PORT}/{API}/";

    [Test]
    public async Task SchemaIsReturned()
    {
        var schemaResponse = await Request.GetAsync("Schemas");
        Assert.True(schemaResponse.Ok);

        var schemaJsonResponse = await schemaResponse.TextAsync();

        Assert.IsFalse(string.IsNullOrWhiteSpace(schemaJsonResponse));
        Assert.IsTrue(schemaJsonResponse.Contains("User Account"));
    }

    [Test]
    public async Task GetEmptyUsers()
    {
        var emptyUsersResponse = await Request.GetAsync("Users");
        Assert.True(emptyUsersResponse.Ok);

        var emptyUsersJson = emptyUsersResponse.JsonAsync().Result;
        Assert.True(emptyUsersJson.HasValue);
        Assert.True(emptyUsersJson!.Value.TryGetProperty("totalResults", out var totalResults));
        Assert.That(totalResults.GetInt32(), Is.EqualTo(0));
        Assert.True(emptyUsersJson.Value.TryGetProperty("itemsPerPage", out var itemsPerPage));
        Assert.That(itemsPerPage.GetInt32(), Is.EqualTo(0));
        Assert.True(emptyUsersJson.Value.TryGetProperty("schemas", out var schemas));
        Assert.That(schemas.GetArrayLength(), Is.EqualTo(1));
        Assert.True(emptyUsersJson.Value.TryGetProperty("Resources", out var resources));
        Assert.That(resources.GetArrayLength(), Is.EqualTo(0));
        Assert.That(schemas[0].ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(schemas[0].GetString(), Is.EqualTo("urn:ietf:params:scim:api:messages:2.0:ListResponse"));
    }

    [Test]
    public async Task GetEmptyGroups()
    {
        var emptyGroupsResponse = await Request.GetAsync("Groups");
        Assert.True(emptyGroupsResponse.Ok);

        var emptyGroupsJson = emptyGroupsResponse.JsonAsync().Result;
        Assert.True(emptyGroupsJson.HasValue);
        Assert.True(emptyGroupsJson!.Value.TryGetProperty("totalResults", out var totalResults));
        Assert.That(totalResults.GetInt32(), Is.EqualTo(0));
        Assert.True(emptyGroupsJson.Value.TryGetProperty("itemsPerPage", out var itemsPerPage));
        Assert.That(itemsPerPage.GetInt32(), Is.EqualTo(0));
        Assert.True(emptyGroupsJson.Value.TryGetProperty("schemas", out var schemas));
        Assert.That(schemas.GetArrayLength(), Is.EqualTo(1));
        Assert.True(emptyGroupsJson.Value.TryGetProperty("Resources", out var resources));
        Assert.That(resources.GetArrayLength(), Is.EqualTo(0));
        Assert.That(schemas[0].ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(schemas[0].GetString(), Is.EqualTo("urn:ietf:params:scim:api:messages:2.0:ListResponse"));

        foreach (var resource in resources.EnumerateArray())
        {
            Assert.True(resource.TryGetProperty("endpoint", out _));
            Assert.True(resource.TryGetProperty("name", out _));
            Assert.True(resource.TryGetProperty("meta", out _));
            Assert.True(resource.TryGetProperty("schema", out _));
            Assert.True(resource.TryGetProperty("schemas", out _));
            Assert.True(resource.TryGetProperty("id", out _));
        }
    }

    [Test]
    public async Task GetResourceTypes()
    {
        var resourceTypesResponse = await Request.GetAsync("ResourceTypes");
        Assert.True(resourceTypesResponse.Ok);

        var resourceTypesResponseJson = resourceTypesResponse.JsonAsync().Result;
        Assert.True(resourceTypesResponseJson.HasValue);
        Assert.True(resourceTypesResponseJson!.Value.TryGetProperty("totalResults", out var totalResults));
        Assert.That(totalResults.GetInt32(), Is.EqualTo(2));
        Assert.True(resourceTypesResponseJson.Value.TryGetProperty("itemsPerPage", out var itemsPerPage));
        Assert.That(itemsPerPage.GetInt32(), Is.EqualTo(2));
        Assert.True(resourceTypesResponseJson.Value.TryGetProperty("startIndex", out var startIndex));
        Assert.That(startIndex.GetInt32(), Is.EqualTo(1));
        Assert.True(resourceTypesResponseJson.Value.TryGetProperty("schemas", out var schemas));
        Assert.That(schemas.GetArrayLength(), Is.EqualTo(1));
        Assert.True(resourceTypesResponseJson.Value.TryGetProperty("Resources", out var resources));
        Assert.That(resources.GetArrayLength(), Is.EqualTo(2));
        Assert.That(schemas[0].ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(schemas[0].GetString(), Is.EqualTo("urn:ietf:params:scim:api:messages:2.0:ListResponse"));
    }

    [Test]
    public async Task GetServiceProviderConfig()
    {
        var serviceProviderConfigResponse = await Request.GetAsync("ServiceProviderConfig");
        Assert.True(serviceProviderConfigResponse.Ok);

        var serviceProviderConfigResponseJson = serviceProviderConfigResponse.JsonAsync().Result;
        Assert.True(serviceProviderConfigResponseJson.HasValue);
        Assert.True(serviceProviderConfigResponseJson!.Value.TryGetProperty("authenticationSchemes", out var authenticationSchemes));
        Assert.True(serviceProviderConfigResponseJson.Value.TryGetProperty("meta", out var meta));
        Assert.True(serviceProviderConfigResponseJson.Value.TryGetProperty("bulk", out var bulk));
        Assert.True(serviceProviderConfigResponseJson.Value.TryGetProperty("eTag", out var eTag));
        Assert.True(serviceProviderConfigResponseJson.Value.TryGetProperty("filter", out var filter));
        Assert.True(serviceProviderConfigResponseJson.Value.TryGetProperty("patch", out var patch));
        Assert.True(serviceProviderConfigResponseJson.Value.TryGetProperty("sort", out var sort));
        Assert.True(serviceProviderConfigResponseJson.Value.TryGetProperty("schemas", out var xmlDataFormat));
    }



    [SetUp]
    public async Task SetUpAPITesting()
    {
        await CreateAPIRequestContext();
        await GetAccessToken();
    }

    private async Task CreateAPIRequestContext()
    {
        // Assuming personal access token available in the environment.

        Request = await this.Playwright.APIRequest.NewContextAsync(new()
        {
            // All requests we send go to this API endpoint.
            BaseURL = _baseUrl,
        });
    }

    private async Task GetAccessToken()
    {
        var headers = new Dictionary<string, string>();
        var response = await Request.GetAsync("Token");
        Assert.True(response.Ok);

        var tokenJsonResponse = response.JsonAsync().Result;
        if (!tokenJsonResponse.HasValue)
        {
            throw new Exception("No token found in response.");
        }

        if (tokenJsonResponse.Value.TryGetProperty("token", out var token) == true)
        {
            if (token.ValueKind == JsonValueKind.String)
            {
                // Add authorization token to all requests.
                // Assuming personal access token available in the environment.
                headers.Add("Authorization", $"Bearer {token}");
                await Request.DisposeAsync();
                Request = await this.Playwright.APIRequest.NewContextAsync(new()
                {
                    // All requests we send go to this API endpoint.
                    BaseURL = _baseUrl,
                    ExtraHTTPHeaders = headers
                });
            }
        }
    }

    [TearDown]
    public async Task TearDownAPITesting()
    {
        await Request.DisposeAsync();
    }
}
