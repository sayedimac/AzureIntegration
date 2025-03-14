using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using AzureIntegration.Services;
using Microsoft.VisualBasic;
using System.Runtime.CompilerServices;


namespace AzureIntegration.Controllers
{

    public class ResourceGroupsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ResourceGroupsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IActionResult> Details(string name)
        {
            var resources = await GetResourcesAsync(name);
            return View(resources);
        }

        public async Task<IActionResult> ManagementGroups()
        {
            var azureManagementGroups = await GetAzureManagementGroups();
            return View(azureManagementGroups);
        }

        public async Task<IActionResult> Index()
        {
            var resourceGroups = await GetResourceGroupsAsync();
            return View(resourceGroups);
        }

        private async Task<List<ResourceGroup>> GetResourceGroupsAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("AzureServices");
            var subscriptionId = _configuration["SubscriptionId"];
            var httpResponseMessage = await httpClient.GetAsync(
            $"subscriptions/{subscriptionId}/resourcegroups?api-version=2021-04-01");


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
                        Name = name ?? string.Empty,
                        Location = location ?? string.Empty
                    });
                }
            }
            return resourceGroups;
        }


        private async Task<List<AzureResource>> GetResourcesAsync(string name)
        {

            var httpClient = _httpClientFactory.CreateClient("AzureServices");
            var subscriptionId = _configuration["SubscriptionId"];

            var httpResponseMessage = await httpClient.GetAsync(
            $"subscriptions/{subscriptionId}/resourceGroups/{name}/resources?api-version=2021-04-01");

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

        private async Task<List<AzureManagementGroup>> GetAzureManagementGroups()
        {
            var httpClient = _httpClientFactory.CreateClient("AzureServices");

            var httpResponseMessage = await httpClient.GetAsync(
            $"providers/Microsoft.Management/managementGroups?api-version=2020-05-01");

            var jsonDocument = JsonDocument.Parse(httpResponseMessage.Content.ReadAsStringAsync().Result);

            var azureManagementGroups = new List<AzureManagementGroup>();

            if (jsonDocument.RootElement.TryGetProperty("value", out JsonElement managementGroupsElement))
            {
                foreach (var managementGroup in managementGroupsElement.EnumerateArray())
                {
                    var name = managementGroup.GetProperty("properties").GetProperty("displayName").GetString();
                    //var name = managementGroup. .GetProperty("displayName").GetString();
                    var id = managementGroup.GetProperty("id").GetString();
                    var type = managementGroup.GetProperty("type").GetString();
                    azureManagementGroups.Add(new AzureManagementGroup
                    {
                        Name = name ?? string.Empty,
                        Type = type ?? string.Empty,
                        Id = id ?? string.Empty,

                    });
                }
            }
            return azureManagementGroups;
        }

    }
}

