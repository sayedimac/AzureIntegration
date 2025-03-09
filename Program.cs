using Microsoft.Identity.Client;
using System.Net.Http.Headers; // Add this line

var builder = WebApplication.CreateBuilder(args);

IConfiguration _configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var clientId = _configuration["ClientId"] ?? throw new ArgumentNullException("ClientId");
var clientSecret = _configuration["ClientSecret"] ?? throw new ArgumentNullException("ClientSecret");
var tenantId = _configuration["TenantId"] ?? throw new ArgumentNullException("TenantId");
var subscriptionId = _configuration["SubscriptionId"];

IConfidentialClientApplication authapp = ConfidentialClientApplicationBuilder.Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
    .Build();

string[] scopes = ["https://management.azure.com/.default"];
var authResult = await authapp.AcquireTokenForClient(scopes).ExecuteAsync();


builder.Services.AddHttpClient("AzureServices", httpClient =>
{
    httpClient.BaseAddress = new Uri($"https://management.azure.com/subscriptions/{subscriptionId}/");
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

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

await app.RunAsync();
