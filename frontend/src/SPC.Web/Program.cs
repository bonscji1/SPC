using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SPC.Core.Repositories;
using SPC.Core.Services;
using SPC.Web;
using SPC.Web.Repositories;
using SPC.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<RecipeDraftService>();
builder.Services.AddSingleton<IPortionCalculator, PortionCalculator>();
builder.Services.AddSingleton<IEnergyCalculator, EnergyCalculator>();
builder.Services.AddScoped<IBrowserLocalStorage, BrowserLocalStorage>();
builder.Services.AddScoped<IRecipeRepository, LocalStorageRecipeRepository>();
builder.Services.AddScoped<IIngredientRepository, LocalStorageIngredientRepository>();
builder.Services.AddScoped<IUserProfileRepository, LocalStorageUserProfileRepository>();
builder.Services.AddScoped<ActiveProfileService>();

await builder.Build().RunAsync();
