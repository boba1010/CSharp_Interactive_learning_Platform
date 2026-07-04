using CSharp_Interactive_Learning_App.Blazor;
using CSharp_Interactive_Learning_App.Blazor.Services;
using CSharp_Interactive_Learning_App.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILocalStorage, LocalStorage>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISecureStorage, SecureStorage>();
builder.Services.AddScoped<IBattleService, BattleService>();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddScoped<ApiClient>();

await builder.Build().RunAsync();
