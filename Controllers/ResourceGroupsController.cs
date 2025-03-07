using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
<<<<<<< HEAD
using System.Net.Http.Headers;
using System.Text.Json;
using AzureIntegration.Services;

namespace AzureIntegration.Controllers
{

    public class ResourceGroupsController : Controller

    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private string clientId;
        private string clientSecret;
        private string tenantId;
        private string subscriptionId;

        public ResourceGroupsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IActionResult> Details(string name)
        {
            var resources = await GetResourcesAsync(name);
            return View(resources);
=======
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
>>>>>>> 058c5a172ca0442c36f64d4dccc359928241967c
        }

        public async Task<IActionResult> Index()
        {
            var resourceGroups = await GetResourceGroupsAsync();
            return View(resourceGroups);
        }

        private async Task<List<ResourceGroup>> GetResourceGroupsAsync()
        {
<<<<<<< HEAD
            var httpClient = _httpClientFactory.CreateClient("AzureManage");

            var httpResponseMessage = await httpClient.GetAsync(
            $"resourcegroups?api-version=2021-04-01");


            var jsonDocument = JsonDocument.Parse(httpResponseMessage.Content.ReadAsStringAsync().Result);

            var resourceGroups = new List<ResourceGroup>();

            if (jsonDocument.RootElement.TryGetProperty("value", out JsonElement resourceGroupsElement))
            {
                foreach (var resourceGroup in resourceGroupsElement.EnumerateArray())
                {
                    var name = resourceGroup.GetProperty("name").GetString();
                    var location = resourceGroup.GetProperty("location").GetString();
                    resourceGroups.Add(new ResourceGroup
                    {
                        Name = name,
                        Location = location
                    });
                }
            }
            return resourceGroups;
        }


        private async Task<List<AzureResource>> GetResourcesAsync(string name)
        {

            var httpClient = _httpClientFactory.CreateClient("AzureManage");

            var httpResponseMessage = await httpClient.GetAsync(
            $"resourceGroups/{name}/resources?api-version=2021-04-01");

            var jsonDocument = JsonDocument.Parse(httpResponseMessage.Content.ReadAsStringAsync().Result);

            var theResources = new List<AzureResource>();

            if (jsonDocument.RootElement.TryGetProperty("value", out JsonElement resourceGroupsElement))
            {
                foreach (var resources in resourceGroupsElement.EnumerateArray())
                {
                    var resourcename = resources.GetProperty("name").GetString();
                    var location = resources.GetProperty("location").GetString();
                    var theType = resources.GetProperty("type").GetString();
                    theResources.Add(new AzureResource { Name = resourcename, Location = location, Type = theType });
                }
            }
            return theResources;
        }
    }
}

=======
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
>>>>>>> 058c5a172ca0442c36f64d4dccc359928241967c
