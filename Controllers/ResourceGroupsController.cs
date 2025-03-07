﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
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
        }

        public async Task<IActionResult> Index()
        {
            var resourceGroups = await GetResourceGroupsAsync();
            return View(resourceGroups);
        }

        private async Task<List<ResourceGroup>> GetResourceGroupsAsync()
        {
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

