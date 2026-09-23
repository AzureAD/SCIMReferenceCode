using System.Text.Json;
using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using PlaywrightTests.Utils;

namespace PlaywrightTests;

[TestFixture]
public class GroupTests : PlaywrightTest
{
    private IAPIRequestContext? Request;
    static string PROTOCOL = Environment.GetEnvironmentVariable("PROTOCOL") ?? "http";
    static string SERVER = Environment.GetEnvironmentVariable("SERVER") ?? "localhost";
    static string PORT = Environment.GetEnvironmentVariable("PORT") ?? ":5000";
    static string API = Environment.GetEnvironmentVariable("API") ?? "scim";
    private static string _baseUrl = $"{PROTOCOL}://{SERVER}{PORT}/{API}/";

    [Test, Order(1)]
    public async Task PostEmptyGroup()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/group1.json");
        var groupResponse = await Request.PostAsync("Groups",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.EqualTo(201));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        SharedTestContext.Group1 = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.Group1));
        Assert.True(groupJson.Value.TryGetProperty("displayName", out var displayName));
        Assert.That(displayName.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.False(string.IsNullOrWhiteSpace(displayName.GetString()));
        ;
        Assert.True(groupJson.Value.TryGetProperty("members", out var members));
        Assert.That(members.GetArrayLength(), Is.EqualTo(0));
        Assert.True(groupJson.Value.TryGetProperty("schemas", out var schemas));
        Assert.That(schemas.GetArrayLength(), Is.EqualTo(1));
        Assert.That(schemas[0].ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(schemas[0].GetString(), Is.EqualTo("urn:ietf:params:scim:schemas:core:2.0:Group"));
        Assert.True(groupJson.Value.TryGetProperty("externalId", out var externalId));
        Assert.That(externalId.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.False(string.IsNullOrWhiteSpace(externalId.GetString()));
        Assert.Pass($"Group 1 with ID '{SharedTestContext.Group1}' added!");
    }

    [Test, Order(1)]
    public async Task CreateGroup2Users()
    {
        var data3 = await JsonLoader.LoadJsonDataAsync("./json/user3.json");
        var user3Response = await Request.PostAsync("Users",
            new()
            {
                DataString = data3,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(user3Response.Ok);
        Assert.That(user3Response.Status, Is.EqualTo(201));
        var user3Json = await user3Response.JsonAsync();
        Assert.True(user3Json.HasValue);
        Assert.True(user3Json!.Value.TryGetProperty("id", out var id3));
        Assert.That(id3.ValueKind, Is.EqualTo(JsonValueKind.String));
        SharedTestContext.User3 = id3.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.User3));


        var data4 = await JsonLoader.LoadJsonDataAsync("./json/user4.json");
        var user4Response = await Request.PostAsync("Users",
            new()
            {
                DataString = data4,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(user4Response.Ok);
        Assert.That(user4Response.Status, Is.EqualTo(201));

        var user4Json = await user4Response.JsonAsync();
        Assert.True(user4Json.HasValue);
        Assert.True(user4Json!.Value.TryGetProperty("id", out var id4));
        Assert.That(id4.ValueKind, Is.EqualTo(JsonValueKind.String));
        SharedTestContext.User4 = id4.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.User4));


        Assert.Pass($"User3 and User4 with IDs '{SharedTestContext.User3}', '{SharedTestContext.User4}' added!");
    }

    [Test, Order(2)]
    public async Task PostGroup2WithMembers()
    {
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.User3));
        var data = await JsonLoader.LoadJsonDataAsync("./json/group2.json", "id3", SharedTestContext.User3!);
        var groupResponse = await Request.PostAsync("Groups",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.EqualTo(201));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        SharedTestContext.Group2 = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.Group2));
        Assert.True(groupJson.Value.TryGetProperty("displayName", out var displayName));
        Assert.That(displayName.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.False(string.IsNullOrWhiteSpace(displayName.GetString()));

        // Check if members were added
        Assert.True(groupJson.Value.TryGetProperty("members", out var members));
        Assert.That(members.GetArrayLength(), Is.EqualTo(1));
        Assert.True(members[0].TryGetProperty("value", out var value));
        Assert.That(value.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.False(string.IsNullOrWhiteSpace(value.GetString()));

        Assert.True(groupJson.Value.TryGetProperty("schemas", out var schemas));
        Assert.That(schemas.GetArrayLength(), Is.EqualTo(1));
        Assert.That(schemas[0].ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(schemas[0].GetString(), Is.EqualTo("urn:ietf:params:scim:schemas:core:2.0:Group"));
        Assert.True(groupJson.Value.TryGetProperty("externalId", out var externalId));
        Assert.That(externalId.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.False(string.IsNullOrWhiteSpace(externalId.GetString()));
        Assert.Pass($"Group 2 with ID '{SharedTestContext.Group2}' added!");
    }

    [Test, Order(3)]
    public async Task GetGroups()
    {
        var groupsResponse = await Request.GetAsync("Groups");
        Assert.True(groupsResponse.Ok);
        Assert.That(groupsResponse.Status, Is.EqualTo(200));

        var groupsJson = await groupsResponse.JsonAsync();
        Assert.True(groupsJson.HasValue);
        Assert.True(groupsJson!.Value.TryGetProperty("totalResults", out var totalResults));
        Assert.That(totalResults.GetInt32(), Is.EqualTo(2));
        Assert.True(groupsJson.Value.TryGetProperty("itemsPerPage", out var itemsPerPage));
        Assert.That(itemsPerPage.GetInt32(), Is.EqualTo(2));
        Assert.True(groupsJson.Value.TryGetProperty("startIndex", out var startIndex));
        Assert.That(startIndex.GetInt32(), Is.EqualTo(1));
        Assert.True(groupsJson.Value.TryGetProperty("schemas", out var schemas));
        Assert.That(schemas.GetArrayLength(), Is.EqualTo(1));
        Assert.True(groupsJson.Value.TryGetProperty("Resources", out var resources));
        Assert.That(resources.GetArrayLength(), Is.EqualTo(2));
        Assert.That(schemas[0].ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(schemas[0].GetString(), Is.EqualTo("urn:ietf:params:scim:api:messages:2.0:ListResponse"));

        foreach (var resource in resources.EnumerateArray())
        {
            Assert.True(resource.TryGetProperty("id", out var id));
            Assert.That(id.GetString(), Is.AnyOf([SharedTestContext.Group1, SharedTestContext.Group2]));
        }
    }

    [Test, Order(4)]
    public async Task PostGroup3()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/group3.json");
        var groupResponse = await Request.PostAsync("Groups",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.EqualTo(201));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        SharedTestContext.Group3 = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.Group3));
        Assert.True(groupJson.Value.TryGetProperty("displayName", out var displayName));
        Assert.That(displayName.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.False(string.IsNullOrWhiteSpace(displayName.GetString()));

        Assert.True(groupJson.Value.TryGetProperty("members", out var members));
        Assert.That(members.GetArrayLength(), Is.EqualTo(0));
        Assert.True(groupJson.Value.TryGetProperty("schemas", out var schemas));
        Assert.That(schemas.GetArrayLength(), Is.EqualTo(1));
        Assert.That(schemas[0].ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(schemas[0].GetString(), Is.EqualTo("urn:ietf:params:scim:schemas:core:2.0:Group"));
        Assert.True(groupJson.Value.TryGetProperty("externalId", out var externalId));
        Assert.That(externalId.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.False(string.IsNullOrWhiteSpace(externalId.GetString()));
        Assert.Pass($"Group 3 with ID '{SharedTestContext.Group3}' added!");
    }

    [Test, Order(5)]
    public async Task PutGroup3()
    {
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.Group3));
        var data = await JsonLoader.LoadJsonDataAsync("./json/put_group3.json",["groupid3", "id3", "id4"] ,[SharedTestContext.Group3, SharedTestContext.User3, SharedTestContext.User4]);
        var groupResponse = await Request.PutAsync($"Groups/{SharedTestContext.Group3}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.EqualTo(200));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(id.GetString(), Is.EqualTo(SharedTestContext.Group3));
        Assert.True(groupJson.Value.TryGetProperty("displayName", out var displayName));
        Assert.That(displayName.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.False(string.IsNullOrWhiteSpace(displayName.GetString()));
        Assert.That(displayName.GetString(), Is.EqualTo("putName"));
        Assert.False(groupJson.Value.TryGetProperty("externalId", out _));

        Assert.True(groupJson.Value.TryGetProperty("members", out var members));
        Assert.That(members.GetArrayLength(), Is.EqualTo(2));
        foreach (var member in members.EnumerateArray())
        {
            Assert.True(member.TryGetProperty("value", out var value));
            Assert.That(value.ValueKind, Is.EqualTo(JsonValueKind.String));
            Assert.That(value.GetString(), Is.AnyOf([SharedTestContext.User3, SharedTestContext.User4]));
        }
        Assert.Pass($"Group 3 with ID '{SharedTestContext.Group3}' updated!");
    }

    [Test, Order(6)]
    public async Task GetGroup3()
    {
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.Group3));
        var groupResponse = await Request.GetAsync($"Groups/{SharedTestContext.Group3}");
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.EqualTo(200));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(id.GetString(), Is.EqualTo(SharedTestContext.Group3));
        Assert.True(groupJson.Value.TryGetProperty("displayName", out var displayName));
        Assert.That(displayName.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.False(string.IsNullOrWhiteSpace(displayName.GetString()));
        Assert.That(displayName.GetString(), Is.EqualTo("putName"), $"displayName should be 'putName', but received '{displayName.GetString()}'!");
        Assert.False(groupJson.Value.TryGetProperty("externalId", out _));

        Assert.True(groupJson.Value.TryGetProperty("members", out var members));
        Assert.That(members.GetArrayLength(), Is.EqualTo(2));
        foreach (var member in members.EnumerateArray())
        {
            Assert.True(member.TryGetProperty("value", out var value));
            Assert.That(value.ValueKind, Is.EqualTo(JsonValueKind.String));
            Assert.That(value.GetString(), Is.AnyOf([SharedTestContext.User3, SharedTestContext.User4]));
        }
        Assert.Pass($"Group 3 with ID '{SharedTestContext.Group3}' validated!");
    }

    [Test, Order(7)]
    public async Task PatchAddUser4Group1()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/patch_add_user_group.json", "id", SharedTestContext.User4!);
        var groupResponse = await Request.PatchAsync($"Groups/{SharedTestContext.Group1}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.AnyOf([200, 204]));
    }

    [Test, Order(8)]
    public async Task PatchRemoveUser4Group1()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/patch_rm_user_group.json", "id", SharedTestContext.User4!);
        var groupResponse = await Request.PatchAsync($"Groups/{SharedTestContext.Group1}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.AnyOf([200, 204]));
    }

    [Test, Order(9)]
    public async Task PatchAddUser4Group1Again()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/patch_add_user_group.json", "id", SharedTestContext.User4!);
        var groupResponse = await Request.PatchAsync($"Groups/{SharedTestContext.Group1}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.AnyOf([200, 204]));
    }

    [Test, Order(10)]
    public async Task GetGroup1()
    {
        var groupResponse = await Request.GetAsync($"Groups/{SharedTestContext.Group1}");
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.EqualTo(200));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("members", out var members));
        Assert.That(members.GetArrayLength(), Is.EqualTo(1));
        Assert.True(members[0].TryGetProperty("value", out var value));
        Assert.That(value.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(value.GetString(), Is.EqualTo(SharedTestContext.User4));
    }

    [Test, Order(11)]
    public async Task PatchRemoveAllUsersGroup1()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/patch_rm_all_users_group.json");
        var groupResponse = await Request.PatchAsync($"Groups/{SharedTestContext.Group1}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.AnyOf([200, 204]));
    }

    [Test, Order(12)]
    public async Task GetNoUsersGroup1()
    {
        var groupResponse = await Request.GetAsync($"Groups/{SharedTestContext.Group1}");
        Assert.True(groupResponse.Ok);
        Assert.That(groupResponse.Status, Is.EqualTo(200));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("members", out var members));
        Assert.That(members.GetArrayLength(), Is.EqualTo(0));
    }

    [SetUp]
    public async Task SetUpApiTesting()
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

    [Test]
    public async Task DeleteUser3()
    {
        if (SharedTestContext.User3 == null)
        {
            Assert.Pass("User3 not found!");
            return;
        }
        var userResponse = await Request.DeleteAsync($"Users/{SharedTestContext.User3}");
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(204));
        SharedTestContext.User3 = null;

    }

    [Test]
    public async Task DeleteUser4()
    {
        if (SharedTestContext.User4 == null)
        {
            Assert.Pass("User4 not found!");
            return;
        }
        var userResponse = await Request.DeleteAsync($"Users/{SharedTestContext.User4}");
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(204));
        SharedTestContext.User4 = null;
    }

    [Test]
    public async Task DeleteGroup1()
    {
        if (SharedTestContext.Group1 == null)
        {
            Assert.Pass("Group1 not found!");
            return;
        }

        var userResponse = await Request.DeleteAsync($"Groups/{SharedTestContext.Group1}");
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(204));
        SharedTestContext.Group1 = null;
    }

    [Test]
    public async Task DeleteGroup2()
    {
        if (SharedTestContext.Group2 == null)
        {
            Assert.Pass("Group2 not found!");
            return;
        }
        var userResponse = await Request.DeleteAsync($"Groups/{SharedTestContext.Group2}");
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(204));
        SharedTestContext.Group2 = null;
    }
    [Test]
    public async Task DeleteGroup3()
    {
        if (SharedTestContext.Group3 == null)
        {
            Assert.Pass("Group3 not found!");
            return;
        }
        var userResponse = await Request.DeleteAsync($"Groups/{SharedTestContext.Group3}");
        Assert.True(userResponse.Ok);
        Assert.That(userResponse.Status, Is.EqualTo(204));
        SharedTestContext.Group3 = null;
    }


    [TearDown]
    public async Task TearDownAPITesting()
    {
        await Request.DisposeAsync();
    }
}
