using System.Text.Json;
using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using PlaywrightTests.Utils;

namespace PlaywrightTests;

[TestFixture]
public class UserAndGroupTestsWithGarbage : PlaywrightTest
{
    private IAPIRequestContext? Request;
    static readonly string PROTOCOL = Environment.GetEnvironmentVariable("PROTOCOL") ?? "http";
    static readonly string SERVER = Environment.GetEnvironmentVariable("SERVER") ?? "localhost";
    static readonly string PORT = Environment.GetEnvironmentVariable("PORT") ?? ":5000";
    static readonly string API = Environment.GetEnvironmentVariable("API") ?? "scim";
    private static string _baseUrl = $"{PROTOCOL}://{SERVER}{PORT}/{API}/";

    [Test, Order(1)]
    public async Task PostUserOMalley()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user_omalley.json");
        var userResponse = await Request!.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(201));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        SharedTestContext.UserOMalley = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.UserOMalley));
        Assert.True(userJson.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("OMalley"));
        Assert.True(userJson.Value.TryGetProperty("active", out var active));
        Assert.That(active.GetBoolean(), Is.True);
        Assert.Pass($"User OMalley with ID '{SharedTestContext.UserOMalley}' added!");
    }

    [Test, Order(1)]
    public async Task PostUserEmp1WithStringActive()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user_emp1.json");
        var userResponse = await Request!.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(201));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.True(userJson.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("emp1"));
        Assert.True(userJson.Value.TryGetProperty("active", out var active));
        Assert.That(active.GetBoolean(), Is.True);
        SharedTestContext.UserEmp1 = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.UserEmp1));
        Assert.Pass($"User with ID '{SharedTestContext.UserEmp1}' added!");
    }

    [Test, Order(2)]
    public async Task GetUsers()
    {
        var userResponse = await Request!.GetAsync("Users");
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(200));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("totalResults", out var totalResults));
        Assert.That(totalResults.GetInt32(), Is.EqualTo(2));
        Assert.True(userJson.Value.TryGetProperty("Resources", out var resources));
        Assert.That(resources.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(resources.GetArrayLength(), Is.EqualTo(2));
        foreach (var resource in resources.EnumerateArray())
        {
            Assert.True(resource.TryGetProperty("id", out var id));
            Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
            var idValue = id.GetString();
            Assert.That(idValue, Is.AnyOf([SharedTestContext.UserOMalley, SharedTestContext.UserEmp1]));
        }
    }

    [Test, Order(3)]
    public async Task PostUserEmp2()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user_emp2.json");
        var userResponse = await Request!.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(201));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.True(userJson.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("emp2"));

        SharedTestContext.UserEmp2 = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.UserEmp2));
        Assert.Pass($"User Emp2 with ID '{SharedTestContext.UserEmp2}' added!");
    }

    [Test, Order(3)]
    public async Task PostUserEmp3()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user_emp3.json");
        var userResponse = await Request!.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(201));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.True(userJson.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("emp3"));

        SharedTestContext.UserEmp3 = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.UserEmp3));
        Assert.Pass($"User Emp3 with ID '{SharedTestContext.UserEmp3}' added!");
    }

    [Test, Order(3)]
    public async Task PostUserNoName()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user_noname.json");
        var userResponse = await Request!.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.False(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(400));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("detail", out var detail));
        Assert.That(detail.GetString(),
            Is.EqualTo("Request is unparsable, syntactically incorrect, or violates schema."));
        Assert.True(userJson.Value.TryGetProperty("status", out var status));
        Assert.That(status.GetInt32(), Is.EqualTo(400));
    }

    [Test, Order(3)]
    public async Task PostUserJunk()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user_junk.json");
        var userResponse = await Request!.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.False(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(400));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("detail", out var detail));
        Assert.That(detail.GetString(),
            Is.EqualTo("Request is unparsable, syntactically incorrect, or violates schema."));
        Assert.True(userJson.Value.TryGetProperty("status", out var status));
        Assert.That(status.GetInt32(), Is.EqualTo(400));
    }

    [Test, Order(3)]
    public async Task PutOMalleyNoUserName()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/put_omalley_nouname.json", "1stuserid",
            SharedTestContext.UserOMalley!);
        var userResponse = await Request!.PutAsync($"Users/{SharedTestContext.UserOMalley}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.False(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(400));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("detail", out var detail));
        Assert.That(detail.GetString(),
            Is.EqualTo("Request is unparsable, syntactically incorrect, or violates schema."));
        Assert.True(userJson.Value.TryGetProperty("status", out var status));
        Assert.That(status.GetInt32(), Is.EqualTo(400));
    }

    [Test, Order(3)]
    public async Task PutOMalleyTypo()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/put_omalley_typo.json", "1stuserid",
            SharedTestContext.UserOMalley!);
        var userResponse = await Request!.PutAsync($"Users/{SharedTestContext.UserOMalley}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(200));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.False(userJson!.Value.TryGetProperty("Addresses", out _));
    }

    [Test, Order(3)]
    public async Task PostEnterpriseUser()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user_enterprise.json");
        var userResponse = await Request!.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(201));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.True(userJson.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("enterprise"));
        Assert.True(userJson.Value.TryGetProperty("active", out var active));
        Assert.That(active.GetBoolean(), Is.True);
        SharedTestContext.UserEnterprise = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.UserEnterprise));
        Assert.Pass($"User Enterprise with ID '{SharedTestContext.UserEnterprise}' added!");
    }

    [Test, Order(4)]
    public async Task PostUserEmp3Exists()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user_emp3.json");
        var userResponse = await Request!.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.False(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(409));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("detail", out _));
        Assert.True(userJson.Value.TryGetProperty("status", out var status));
        Assert.That(status.GetInt32(), Is.EqualTo(409));
    }

    [Test, Order(4)]
    public async Task PatchUserOMalleyUsername()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/patch_omalley_uname.json", "1stuserid",
            SharedTestContext.UserOMalley!);
        var patchResponse = await Request!.PatchAsync($"Users/{SharedTestContext.UserOMalley}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(patchResponse.Ok, await patchResponse.TextAsync());
        Assert.That(patchResponse.Status, Is.AnyOf([200, 204]));
    }

    [Test, Order(4)]
    public async Task PatchUserOMalleyActive()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/patch_omalley_active.json", "1stuserid",
            SharedTestContext.UserOMalley!);
        var patchResponse = await Request!.PatchAsync($"Users/{SharedTestContext.UserOMalley}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(patchResponse.Ok, await patchResponse.TextAsync());
        Assert.That(patchResponse.Status, Is.AnyOf([200, 204]));
    }

    [Test, Order(5)]
    public async Task CheckUserOMalleyPatch()
    {
        var userResponse = await Request!.GetAsync($"Users/{SharedTestContext.UserOMalley}");
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(200));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ToString(), Is.EqualTo(SharedTestContext.UserOMalley));
        Assert.True(userJson.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("newusername"));
        Assert.True(userJson.Value.TryGetProperty("active", out var active));
        Assert.That(active.GetBoolean(), Is.False);
    }

    [Test, Order(6)]
    public async Task PutUserOMalley()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/put_omalley.json", "1stuserid",
            SharedTestContext.UserOMalley!);
        var userResponse = await Request!.PutAsync($"Users/{SharedTestContext.UserOMalley}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(200));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("userName", out var userName));
        Assert.That(userName.GetString(), Is.EqualTo("OMalley"));
        Assert.True(userJson.Value.TryGetProperty("active", out var active));
        Assert.That(active.GetBoolean(), Is.False);
        Assert.True(userJson.Value.TryGetProperty("addresses", out var addresses));
        Assert.That(addresses.GetArrayLength(), Is.EqualTo(2));
        Assert.True(addresses[0].TryGetProperty("country", out var country));
        Assert.That(country.GetString(), Is.EqualTo("Germany"));
    }

    [Test, Order(6)]
    public async Task GetUsersPaginate()
    {
        var usersResponse = await Request!.GetAsync("Users?startIndex=1&count=2");
        Assert.True(usersResponse.Ok, await usersResponse.TextAsync());
        Assert.That(usersResponse.Status, Is.EqualTo(200));

        var usersJson = await usersResponse.JsonAsync();
        Assert.True(usersJson.HasValue);
        Assert.True(usersJson!.Value.TryGetProperty("totalResults", out var totalResults));
        Assert.That(totalResults.GetInt32(), Is.EqualTo(5));
        Assert.True(usersJson.Value.TryGetProperty("itemsPerPage", out var itemsPerPage));
        Assert.That(itemsPerPage.GetInt32(), Is.EqualTo(2));
        Assert.True(usersJson.Value.TryGetProperty("startIndex", out var startIndex));
        Assert.That(startIndex.GetInt32(), Is.EqualTo(1));
        Assert.True(usersJson.Value.TryGetProperty("Resources", out var resources));
        Assert.That(resources.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(resources.GetArrayLength(), Is.EqualTo(2));
        foreach (var resource in resources.EnumerateArray())
        {
            Assert.True(resource.TryGetProperty("id", out var id));
            Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
            var idValue = id.GetString();
            Assert.That(idValue,
                Is.AnyOf([
                    SharedTestContext.UserOMalley, SharedTestContext.UserEmp1, SharedTestContext.UserEmp2,
                    SharedTestContext.UserEmp3, SharedTestContext.UserEnterprise
                ]));
        }
    }

    [Test, Order(6)]
    public async Task GetUsersAttributes()
    {
        var usersResponse = await Request!.GetAsync("Users?attributes=userName,emails");
        Assert.True(usersResponse.Ok, await usersResponse.TextAsync());
        Assert.That(usersResponse.Status, Is.EqualTo(200));

        var usersJson = await usersResponse.JsonAsync();
        Assert.True(usersJson.HasValue);
        Assert.True(usersJson!.Value.TryGetProperty("totalResults", out var totalResults));
        Assert.That(totalResults.GetInt32(), Is.EqualTo(5));
        Assert.True(usersJson.Value.TryGetProperty("Resources", out var resources));
        Assert.That(resources.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(resources.GetArrayLength(), Is.EqualTo(5));
        foreach (var resource in resources.EnumerateArray())
        {
            Assert.True(resource.TryGetProperty("id", out var id));
            Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
            var idValue = id.GetString();
            Assert.That(idValue,
                Is.AnyOf([
                    SharedTestContext.UserOMalley, SharedTestContext.UserEmp1, SharedTestContext.UserEmp2,
                    SharedTestContext.UserEmp3, SharedTestContext.UserEnterprise
                ]));
        }
    }

    [Test, Order(6)]
    public async Task PostUserEmp3Again()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/user_emp3.json");
        var userResponse = await Request!.PostAsync("Users",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.False(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(409));

        var userJson = await userResponse.JsonAsync();
        Assert.True(userJson.HasValue);
        Assert.True(userJson!.Value.TryGetProperty("detail", out _));
        Assert.True(userJson.Value.TryGetProperty("status", out var status));
        Assert.That(status.GetInt32(), Is.EqualTo(409));
    }

    [Test, Order(6)]
    public async Task FilterEqAndValue()
    {
        var usersResponse =
            await Request!.GetAsync(
                "Users?filter=name.FamilyName eq Employee and (emails.Value co example.com or emails.Value co example.org)");
        Assert.That(usersResponse.Status, Is.AnyOf([200, 400]));

        var usersJson = await usersResponse.JsonAsync();
        Assert.True(usersJson.HasValue);
        if (usersResponse.Status == 200)
        {
            Assert.True(usersJson!.Value.TryGetProperty("totalResults", out var totalResults));
            Assert.That(totalResults.GetInt32(), Is.EqualTo(4));
            Assert.True(usersJson.Value.TryGetProperty("Resources", out var resources));
            Assert.That(resources.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(resources.GetArrayLength(), Is.EqualTo(4));
            foreach (var resource in resources.EnumerateArray())
            {
                Assert.True(resource.TryGetProperty("id", out var id));
                Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
                var idValue = id.GetString();
                Assert.That(idValue,
                    Is.AnyOf([
                        SharedTestContext.UserEmp1, SharedTestContext.UserEmp2, SharedTestContext.UserEmp3,
                        SharedTestContext.UserEnterprise
                    ]));
            }
        }
        else
        {
            Assert.True(usersJson!.Value.TryGetProperty("status", out var status));
            Assert.That(status.GetInt32(), Is.EqualTo(400));
            Assert.True(usersJson.Value.TryGetProperty("scimType", out var scimType));
            Assert.That(scimType.GetString(), Is.EqualTo("invalidFilter"));
            Assert.True(usersJson.Value.TryGetProperty("detail", out var detail));
            Assert.That(detail.GetString(),
                Does.StartWith("This filter operator is not supported").Or
                    .StartWith("Filtering by this attribute is not supported"), detail.GetString());
            Assert.Warn($"WARNING: {detail.GetString()}");
        }
    }

    [Test, Order(6)]
    public async Task FilterStartsWith()
    {
        var usersResponse = await Request!.GetAsync("Users?filter=userName sw O");
        Assert.That(usersResponse.Status, Is.AnyOf([200, 400]));

        var usersJson = await usersResponse.JsonAsync();
        Assert.True(usersJson.HasValue);
        if (usersResponse.Status == 200)
        {
            Assert.True(usersJson!.Value.TryGetProperty("totalResults", out var totalResults));
            Assert.That(totalResults.GetInt32(), Is.EqualTo(1));
            Assert.True(usersJson.Value.TryGetProperty("Resources", out var resources));
            Assert.That(resources.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(resources.GetArrayLength(), Is.EqualTo(1));
            foreach (var resource in resources.EnumerateArray())
            {
                Assert.True(resource.TryGetProperty("id", out var id));
                Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
                var idValue = id.GetString();
                Assert.That(idValue, Is.EqualTo(SharedTestContext.UserOMalley));
            }
        }
        else
        {
            Assert.True(usersJson!.Value.TryGetProperty("status", out var status));
            Assert.That(status.GetInt32(), Is.EqualTo(400));
            Assert.True(usersJson.Value.TryGetProperty("scimType", out var scimType));
            Assert.That(scimType.GetString(), Is.EqualTo("invalidFilter"));
            Assert.True(usersJson.Value.TryGetProperty("detail", out var detail));
            Assert.That(detail.GetString(),
                Does.StartWith("This filter operator is not supported").Or
                    .StartWith("Filtering by this attribute is not supported"), detail.GetString());
            Assert.Warn($"WARNING: {detail.GetString()}");
        }
    }

    [Test, Order(6)]
    public async Task FilterGreaterThan()
    {
        var usersResponse = await Request!.GetAsync("Users?filter=meta.Created gt 2015-10-10T14:38:21.8617979-07:00");
        Assert.That(usersResponse.Status, Is.AnyOf([200, 400]));

        var usersJson = await usersResponse.JsonAsync();
        Assert.True(usersJson.HasValue);
        if (usersResponse.Status == 200)
        {
            Assert.True(usersJson!.Value.TryGetProperty("totalResults", out var totalResults));
            Assert.That(totalResults.GetInt32(), Is.EqualTo(5));
            Assert.True(usersJson.Value.TryGetProperty("Resources", out var resources));
            Assert.That(resources.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(resources.GetArrayLength(), Is.EqualTo(5));
            foreach (var resource in resources.EnumerateArray())
            {
                Assert.True(resource.TryGetProperty("id", out var id));
                Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
                var idValue = id.GetString();
                Assert.That(idValue,
                    Is.AnyOf([
                        SharedTestContext.UserOMalley, SharedTestContext.UserEmp1, SharedTestContext.UserEmp2,
                        SharedTestContext.UserEmp3, SharedTestContext.UserEnterprise
                    ]));
            }
        }
        else
        {
            Assert.True(usersJson!.Value.TryGetProperty("status", out var status));
            Assert.That(status.GetInt32(), Is.EqualTo(400));
            Assert.True(usersJson.Value.TryGetProperty("scimType", out var scimType));
            Assert.That(scimType.GetString(), Is.EqualTo("invalidFilter"));
            Assert.True(usersJson.Value.TryGetProperty("detail", out var detail));
            Assert.That(detail.GetString(),
                Does.StartWith("This filter operator is not supported").Or
                    .StartWith("Filtering by this attribute is not supported"), detail.GetString());
            Assert.Warn($"WARNING: {detail.GetString()}");
        }
    }

    // Group tests

    [Test, Order(3)]
    public async Task PostGroup()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/group1garbage.json");
        var groupResponse = await Request!.PostAsync("Groups",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(groupResponse.Ok, await groupResponse.TextAsync());
        Assert.That(groupResponse.Status, Is.EqualTo(201));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        SharedTestContext.Group1Garbage = id.GetString();
        Assert.False(string.IsNullOrWhiteSpace(SharedTestContext.Group1Garbage));
        Assert.Pass($"Group1Garbage with ID '{SharedTestContext.Group1Garbage}' added!");
    }

    [Test, Order(4)]
    public async Task PatchGroupAddJunkUsers()
    {
        var data1 = await JsonLoader.LoadJsonDataAsync("./json/patch_add_junk_user_group.json",
            ["1stgroupid", "junkuserid"], [SharedTestContext.Group1Garbage!, "string id 1"]);
        var patch1Response = await Request!.PatchAsync($"Groups/{SharedTestContext.Group1Garbage}",
            new()
            {
                DataString = data1,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(patch1Response.Ok, await patch1Response.TextAsync());
        Assert.That(patch1Response.Status, Is.AnyOf([200, 204]));

        var data2 = await JsonLoader.LoadJsonDataAsync("./json/patch_add_junk_user_group.json",
            ["1stgroupid", "junkuserid"], [SharedTestContext.Group1Garbage!, "string id 2"]);
        var patch2Response = await Request!.PatchAsync($"Groups/{SharedTestContext.Group1Garbage}",
            new()
            {
                DataString = data2,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(patch2Response.Ok, await patch2Response.TextAsync());
        Assert.That(patch2Response.Status, Is.AnyOf([200, 204]));
    }

    [Test, Order(5)]
    public async Task GetGroup()
    {
        var groupResponse = await Request!.GetAsync($"Groups/{SharedTestContext.Group1Garbage}");
        Assert.True(groupResponse.Ok, await groupResponse.TextAsync());
        Assert.That(groupResponse.Status, Is.EqualTo(200));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.True(groupJson.Value.TryGetProperty("displayName", out var displayName));
        Assert.That(displayName.GetString(), Is.EqualTo("Group 1"));
        Assert.True(groupJson.Value.TryGetProperty("members", out var members));
        Assert.That(members.GetArrayLength(), Is.EqualTo(2));
        Assert.True(members[0].TryGetProperty("value", out var value1));
        Assert.That(value1.GetString(), Is.EqualTo("string id 1"));
        Assert.True(members[1].TryGetProperty("value", out var value2));
        Assert.That(value2.GetString(), Is.EqualTo("string id 2"));
    }

    [Test, Order(5)]
    public async Task GetGroupExcludeMembers()
    {
        var groupResponse =
            await Request!.GetAsync($"Groups/{SharedTestContext.Group1Garbage}?excludedAttributes=members");
        Assert.True(groupResponse.Ok, await groupResponse.TextAsync());
        Assert.That(groupResponse.Status, Is.EqualTo(200));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("id", out var id));
        Assert.That(id.ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.True(groupJson.Value.TryGetProperty("displayName", out var displayName));
        Assert.That(displayName.GetString(), Is.EqualTo("Group 1"));

        if (groupJson.Value.TryGetProperty("members", out _))
        {
            Assert.Warn("WARNING: Members should be excluded!");
        }
    }

    [Test, Order(6)]
    public async Task PutGroup()
    {
        var data = await JsonLoader.LoadJsonDataAsync("./json/put_group1garbage.json", "1stgroupid",
            SharedTestContext.Group1Garbage!);
        var groupResponse = await Request!.PutAsync($"Groups/{SharedTestContext.Group1Garbage}",
            new()
            {
                DataString = data,
                Headers = new KeyValuePair<string, string>[] { new("Content-Type", "application/json") }
            });
        Assert.True(groupResponse.Ok, await groupResponse.TextAsync());
        Assert.That(groupResponse.Status, Is.EqualTo(200));

        var groupJson = await groupResponse.JsonAsync();
        Assert.True(groupJson.HasValue);
        Assert.True(groupJson!.Value.TryGetProperty("displayName", out var displayName));
        Assert.That(displayName.GetString(), Is.EqualTo("Tiffany Ortiz"));
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
        var response = await Request!.GetAsync("Token");
        Assert.True(response.Ok, await response.TextAsync());

        var tokenJsonResponse = response.JsonAsync().Result;
        if (!tokenJsonResponse.HasValue)
        {
            throw new Exception("No token found in response.");
        }

        if (tokenJsonResponse.Value.TryGetProperty("token", out var token))
        {
            if (token.ValueKind == JsonValueKind.String)
            {
                // Add authorization token to all requests.
                // Assuming personal access token available in the environment.
                headers.Add("Authorization", $"Bearer {token}");
                await Request!.DisposeAsync();
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
    public async Task DeleteUserOMalley()
    {
        if (SharedTestContext.UserOMalley == null)
        {
            Assert.Warn("UserOMalley not found!");
            return;
        }

        var userResponse = await Request!.DeleteAsync($"Users/{SharedTestContext.UserOMalley}");
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(204));
        SharedTestContext.UserOMalley = null;
    }

    [Test]
    public async Task DeleteUserEmp1()
    {
        if (SharedTestContext.UserEmp1 == null)
        {
            Assert.Warn("UserEmp1 not found!");
            return;
        }

        var userResponse = await Request!.DeleteAsync($"Users/{SharedTestContext.UserEmp1}");
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(204));
        SharedTestContext.UserEmp1 = null;
    }

    [Test]
    public async Task DeleteUserEmp2()
    {
        if (SharedTestContext.UserEmp2 == null)
        {
            Assert.Warn("UserEmp2 not found!");
            return;
        }

        var userResponse = await Request!.DeleteAsync($"Users/{SharedTestContext.UserEmp2}");
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(204));
        SharedTestContext.UserEmp2 = null;
    }

    [Test]
    public async Task DeleteUserEmp3()
    {
        if (SharedTestContext.UserEmp3 == null)
        {
            Assert.Warn("UserEmp3 not found!");
            return;
        }

        var userResponse = await Request!.DeleteAsync($"Users/{SharedTestContext.UserEmp3}");
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(204));
        SharedTestContext.UserEmp3 = null;
    }

    [Test]
    public async Task DeleteUserEnterprise()
    {
        if (SharedTestContext.UserEnterprise == null)
        {
            Assert.Warn("UserEnterprise not found!");
            return;
        }

        var userResponse = await Request!.DeleteAsync($"Users/{SharedTestContext.UserEnterprise}");
        Assert.True(userResponse.Ok, await userResponse.TextAsync());
        Assert.That(userResponse.Status, Is.EqualTo(204));
        SharedTestContext.UserEnterprise = null;
    }

    [Test]
    public async Task DeleteGroup1Garbage()
    {
        if (SharedTestContext.Group1Garbage == null)
        {
            Assert.Warn("Group1Garbage not found!");
            return;
        }

        var groupResponse = await Request!.DeleteAsync($"Groups/{SharedTestContext.Group1Garbage}");
        Assert.True(groupResponse.Ok, await groupResponse.TextAsync());
        Assert.That(groupResponse.Status, Is.EqualTo(204));
        SharedTestContext.Group1Garbage = null;
    }

    [TearDown]
    public async Task TearDownApiTesting()
    {
        await Request!.DisposeAsync();
    }
}
