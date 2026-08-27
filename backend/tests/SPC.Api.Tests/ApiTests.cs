using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using SPC.Core.Auth;
using SPC.Core.Models;
using Testcontainers.PostgreSql;

namespace SPC.Api.Tests;

public sealed class ApiFixture : IAsyncLifetime
{
    public const string JwtKey = "test-signing-key-that-is-32-chars!";

    private PostgreSqlContainer? _postgres;

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("spc")
            .WithUsername("spc")
            .WithPassword("spc")
            .Build();
        await _postgres.StartAsync();

        Factory = new SpcApiFactory(_postgres.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }

    public HttpClient CreateClient() => Factory.CreateClient();

    public async Task<(string Username, string Password, string AccessToken)> SignUpAsync(HttpClient client)
    {
        var username = $"user-{Guid.NewGuid():N}";
        const string password = "secret";
        var response = await client.PostAsJsonAsync("/api/auth/signup", new { username, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException("Sign-up response was empty.");
        return (username, password, body.AccessToken);
    }

    public async Task<string> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        return doc.RootElement.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Login response missing accessToken.");
    }
}

public sealed class SpcApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("Jwt:Key", ApiFixture.JwtKey);
        builder.UseSetting("Jwt:Issuer", "spc");
        builder.UseSetting("Jwt:Audience", "spc");
        builder.UseEnvironment("Development");
    }
}

[CollectionDefinition("api")]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>;

[Collection("api")]
public sealed class AuthTests(ApiFixture fixture)
{
    [Fact]
    public async Task Signup_then_login_returns_jwt()
    {
        using var client = fixture.CreateClient();
        var (username, password, token) = await fixture.SignUpAsync(client);
        Assert.False(string.IsNullOrWhiteSpace(token));

        var again = await fixture.LoginAsync(client, username, password);
        Assert.False(string.IsNullOrWhiteSpace(again));
    }

    [Fact]
    public async Task Login_with_wrong_password_is_unauthorized()
    {
        using var client = fixture.CreateClient();
        var (username, _, _) = await fixture.SignUpAsync(client);
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            username,
            password = "nope",
        });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Recipes_without_token_are_unauthorized()
    {
        using var client = fixture.CreateClient();
        var response = await client.GetAsync("/api/recipes");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Signup_duplicate_username_is_conflict()
    {
        using var client = fixture.CreateClient();
        var (username, password, _) = await fixture.SignUpAsync(client);
        var response = await client.PostAsJsonAsync("/api/auth/signup", new
        {
            username,
            password,
        });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Signup_blank_is_bad_request()
    {
        using var client = fixture.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/signup", new
        {
            username = "  ",
            password = "",
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

[Collection("api")]
public sealed class RecipeTests(ApiFixture fixture)
{
    [Fact]
    public async Task Save_and_load_recipe_for_signed_up_account()
    {
        using var client = fixture.CreateClient();
        var (_, _, token) = await fixture.SignUpAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var recipe = new RecipeDto
        {
            Id = Guid.NewGuid(),
            Name = "Test soup",
            MealType = MealType.Lunch,
        };
        recipe.FamilyId = recipe.Id;

        var save = await client.PutAsJsonAsync("/api/recipes", recipe);
        save.EnsureSuccessStatusCode();

        var loaded = await client.GetFromJsonAsync<RecipeDto>($"/api/recipes/{recipe.Id}");
        Assert.NotNull(loaded);
        Assert.Equal("Test soup", loaded.Name);

        var list = await client.GetFromJsonAsync<PagedResult<RecipeFamilyGroup>>("/api/recipes?page=1&pageSize=10");
        Assert.NotNull(list);
        Assert.Contains(list.Items, family => family.Primary.Id == recipe.Id);
    }

    [Fact]
    public async Task One_account_does_not_see_another_accounts_recipes()
    {
        using var client = fixture.CreateClient();
        var (_, _, firstToken) = await fixture.SignUpAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firstToken);

        var recipe = new RecipeDto
        {
            Id = Guid.NewGuid(),
            Name = "First-user stew",
            MealType = MealType.Dinner,
        };
        recipe.FamilyId = recipe.Id;
        var save = await client.PutAsJsonAsync("/api/recipes", recipe);
        save.EnsureSuccessStatusCode();

        var (_, _, secondToken) = await fixture.SignUpAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secondToken);
        var list = await client.GetFromJsonAsync<PagedResult<RecipeFamilyGroup>>("/api/recipes?page=1&pageSize=10");
        Assert.NotNull(list);
        Assert.DoesNotContain(list.Items, family => family.Primary.Id == recipe.Id);
    }
}
