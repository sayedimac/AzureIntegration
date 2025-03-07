using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using AzureIntegration.Services;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder; // Add this line
using Microsoft.AspNetCore.Http; // Add this line
using Microsoft.Net.Http.Headers; // Add this line
using System.Net.Http.Headers; // Add this line

var builder = WebApplication.CreateBuilder(args);

IConfiguration _configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var clientId = _configuration["ClientId"];
var clientSecret = _configuration["ClientSecret"];
var tenantId = _configuration["TenantId"];
var subscriptionId = _configuration["SubscriptionId"];

IConfidentialClientApplication authapp = ConfidentialClientApplicationBuilder.Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
    .Build();

string[] scopes = ["https://management.azure.com/.default"];
var authResult = authapp.AcquireTokenForClient(scopes).ExecuteAsync();


builder.Services.AddHttpClient("AzureManage", httpClient =>
{
    httpClient.BaseAddress = new Uri($"https://management.azure.com/subscriptions/{subscriptionId}/");
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.Result.AccessToken);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddEnvironmentVariables();

// Register the HTTP client
builder.Services.AddHttpClient();

// Register the configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
