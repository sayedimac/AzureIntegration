using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.Json;
using AzureIntegration.Services;
using System.Collections.Generic;

namespace AzureIntegration.Controllers
{
    public class ResourceGroupsController : Controller
    {
        private readonly IConfiguration _configuration;

        public ResourceGroupsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var resourceGroups = await GetResourceGroupsAsync();
            return View(resourceGroups);
        }

        private async Task<List<ResourceGroup>> GetResourceGroupsAsync()
        {
            var clientId = _configuration["AzureAd:ClientId"];
            var clientSecret = _configuration["AzureAd:ClientSecret"];
            var tenantId = _configuration["AzureAd:TenantId"];
            var subscriptionId = _configuration["AzureAd:SubscriptionId"];

            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();

            string[] scopes = new string[] { "https://management.azure.com/.default" };
            var authResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                var response = await client.GetAsync($"https://management.azure.com/subscriptions/{subscriptionId}/resourcegroups?api-version=2021-04-01");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);

                var resourceGroups = new List<ResourceGroup>();
                if (jsonDocument.RootElement.TryGetProperty("value", out JsonElement resourceGroupsElement))
                {
                    foreach (var resourceGroup in resourceGroupsElement.EnumerateArray())
                    {
                        var name = resourceGroup.GetProperty("name").GetString();
                        var location = resourceGroup.GetProperty("location").GetString();
                        resourceGroups.Add(new ResourceGroup { Name = name, Location = location });
                    }
                }

                return resourceGroups;
            }
        }
    }
}
