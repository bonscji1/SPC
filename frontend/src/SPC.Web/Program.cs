using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SPC.Core.Auth;
using SPC.Core.Repositories;
using SPC.Core.Services;
using SPC.Web;
using SPC.Web.Auth;
using SPC.Web.Repositories;
using SPC.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, SpcAuthenticationStateProvider>();

builder.Services.AddScoped<AuthSession>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<BearerTokenHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<BearerTokenHandler>();
    handler.InnerHandler ??= new HttpClientHandler();

    var configured = builder.Configuration["ApiBaseUrl"];
    var baseAddress = string.IsNullOrWhiteSpace(configured)
        ? builder.HostEnvironment.BaseAddress
        : configured;
    if (!baseAddress.EndsWith('/'))
    {
        baseAddress += "/";
    }

    return new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };
});

builder.Services.AddSingleton<RecipeDraftService>();
builder.Services.AddSingleton<IPortionCalculator, PortionCalculator>();
builder.Services.AddSingleton<IEnergyCalculator, EnergyCalculator>();
builder.Services.AddScoped<IRecipeRepository, ApiRecipeRepository>();
builder.Services.AddScoped<CachedIngredientRepository>();
builder.Services.AddScoped<IIngredientRepository>(sp => sp.GetRequiredService<CachedIngredientRepository>());
builder.Services.AddScoped<IIngredientLibraryCache>(sp => sp.GetRequiredService<CachedIngredientRepository>());
builder.Services.AddScoped<IUserProfileRepository, ApiUserProfileRepository>();
builder.Services.AddScoped<ActiveProfileService>();

var host = builder.Build();
await host.Services.GetRequiredService<IAuthService>().RestoreAsync();
await host.RunAsync();
