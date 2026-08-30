using System.Text.Json;
using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using PlaywrightTests.Utils;

namespace PlaywrightTests;

[TestFixture]
public class UserTests : PlaywrightTest
{
    private IAPIRequestContext? Request;
    static string PROTOCOL = Environment.GetEnvironmentVariable("PROTOCOL") ?? "http";
    static string SERVER = Environment.GetEnvironmentVariable("SERVER") ?? "localhost";
    static string PORT = Environment.GetEnvironmentVariable("PORT") ?? ":5000";
    static string API = Environment.GetEnvironmentVariable("API") ?? "scim";
    private static string _baseUrl = $"{PROTOCOL}://{SERVER}{PORT}/{API}/";

    [Test, Order(1)]
    public async Task PostUser()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user1.json");
        var userResponse = await Request.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(201));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        SharedTestContext.User1 = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.User1));
        Assert.Pass($"User with ID '{SharedTestContext.User1}' added!");
    }

    [Test, Order(1)]
    public async Task PostEnterpriseUser()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user2.json");
        var userResponse = await Request.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(201));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        SharedTestContext.User2 = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.User2));
        Assert.Pass($"User with ID '{SharedTestContext.User2}' added!");
    }

    [Test, Order(2)]
    public async Task GetUser1()
    {
        var userResponse = await Request.GetAsync($"Users/{SharedTestContext.User1}");
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(200));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(id.ToString(), Is.EqualTo(SharedTestContext.User1));
    }

    [Test, Order(2)]
    public async Task GetUser2()
    {
        var userResponse = await Request.GetAsync($"Users/{SharedTestContext.User2}");
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(200));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(id.ToString(), Is.EqualTo(SharedTestContext.User2));
    }

    [Test, Order(2)]
    public async Task GetUserAttributes()
    {
        var usersResponse = await Request.GetAsync("Users?attributes=userName,emails");
        Assert.True(usersResponse.Ok);
        Assert.That(usersResponse.Status, Is.EqualTo(200));

        var usersJson = await usersResponse.JsonAsync();
        Assert.True(usersJson.HasValue);
        Assert.True(usersJson.Value.TryGetProperty("totalResults", out var totalResults));
        Assert.That(totalResults.GetInt32(), Is.EqualTo(2));
        Assert.True(usersJson.Value.TryGetProperty("Resources", out var resources));
        Assert.That(resources.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(resources.GetArrayLength(), Is.EqualTo(2));
        foreach (var resource in resources.EnumerateArray())
        {
            Assert.True(resource.TryGetProperty("id", out var id));
            Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
            var idValue = id.GetString();
            Assert.That(idValue, Is.AnyOf([SharedTestContext.User1, SharedTestContext.User2]));
        }
    }

    [Test, Order(2)]
    public async Task GetUserFilters()
    {
        var usersResponse = await Request.GetAsync("Users/?filter=DisplayName+eq+%22BobIsAmazing%22");
        Assert.True(usersResponse.Ok);
        Assert.That(usersResponse.Status, Is.EqualTo(200));

        var usersJson = await usersResponse.JsonAsync();
        Assert.True(usersJson.HasValue);
        Assert.True(usersJson.Value.TryGetProperty("totalResults", out var totalResults));
        Assert.That(totalResults.GetInt32(), Is.EqualTo(1));
        Assert.True(usersJson.Value.TryGetProperty("Resources", out var resources));
        Assert.That(resources.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(resources.GetArrayLength(), Is.EqualTo(1));
        foreach (var resource in resources.EnumerateArray())
        {
            Assert.True(resource.TryGetProperty("id", out var id));
            Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
            var idValue = id.GetString();
            Assert.That(idValue, Is.EqualTo(SharedTestContext.User1));
        }
    }

    [Test, Order(3)]
    public async Task PatchUser1()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/patch_user1.json");
        var patchResponse = await Request.PatchAsync($"Users/{SharedTestContext.User1}", new()
        {
            DataString = data,
            Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
        });
        Assert.True(patchResponse.Ok);
        Assert.That(patchResponse.Status, Is.EqualTo(200));

        var userJson = await patchResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ToString(), Is.EqualTo(SharedTestContext.User1));
        Assert.True(userJson.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("ryan3"));
    }

    [Test, Order(4)]
    public async Task CheckUser1Patch()
    {
        var patchResponse = await Request.GetAsync($"Users/{SharedTestContext.User1}");
        Assert.True(patchResponse.Ok);
        Assert.That(patchResponse.Status, Is.EqualTo(200));

        var userJson = await patchResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ToString(), Is.EqualTo(SharedTestContext.User1));
        Assert.True(userJson.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("ryan3"));
    }

    [Test, Order(4)]
    public async Task PutUser2()
    {
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.User2));
        var data = await JsonLoader.LoadJsonDataAsync("./json/put_user2.json", "id2", SharedTestContext.User2!);
        var putResponse = await Request.PutAsync($"Users/{SharedTestContext.User2}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(putResponse.Ok);
        Assert.That(putResponse.Status, Is.EqualTo(200));

        var userJson = await putResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ToString(), Is.EqualTo(SharedTestContext.User2));
        Assert.True(userJson.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("UserNameReplace2"));
    }

    [Test, Order(5)]
    public async Task CheckUser2Put()
    {
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.User2));
        var userResponse = await Request.GetAsync($"Users/{SharedTestContext.User2}");
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(200));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ToString(), Is.EqualTo(SharedTestContext.User2));
        Assert.True(userJson.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("UserNameReplace2"));
    }

    [SetUp]
    public async Task SetUpApiTesting()
    {
        await CreateApiRequestContext();
        await GetAccessToken();
    }

    private async Task CreateApiRequestContext()
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

    [Test]
    public async Task DeleteUser1()
    {
        var userResponse = await Request.DeleteAsync($"Users/{SharedTestContext.User1}");
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(204));
    }

    [Test]
    public async Task DeleteUser2()
    {
        var userResponse = await Request.DeleteAsync($"Users/{SharedTestContext.User2}");
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(204));
    }

    [TearDown]
    public async Task TearDownAPITesting()
    {
        await Request.DisposeAsync();
    }
}
