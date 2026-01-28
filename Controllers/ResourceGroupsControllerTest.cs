using AzureIntegration.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AzureIntegration.Tests.Controllers
{
    public class ResourceGroupsControllerTest
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public ResourceGroupsControllerTest()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockConfiguration = new Mock<IConfiguration>();
        }

        [Fact]
        public async Task GetAzureManagementGroups_ReturnsManagementGroups_WhenApiCallSucceeds()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"
                {
                    ""value"": [
                        {
                            ""id"": ""/providers/Microsoft.Management/managementGroups/mg1"",
                            ""type"": ""Microsoft.Management/managementGroups"",
                            ""properties"": {
                                ""displayName"": ""Management Group 1""
                            }
                        },
                        {
                            ""id"": ""/providers/Microsoft.Management/managementGroups/mg2"",
                            ""type"": ""Microsoft.Management/managementGroups"",
                            ""properties"": {
                                ""displayName"": ""Management Group 2""
                            }
                        }
                    ]
                }")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory
                .Setup(factory => factory.CreateClient("AzureServices"))
                .Returns(httpClient);

            var controller = new ResourceGroupsController(_mockHttpClientFactory.Object, _mockConfiguration.Object);

            // Act
            var result = await controller.ManagementGroups();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<AzureManagementGroup>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("Management Group 1", model[0].Name);
            Assert.Equal("/providers/Microsoft.Management/managementGroups/mg1", model[0].Id);
            Assert.Equal("Microsoft.Management/managementGroups", model[0].Type);
            Assert.Equal("Management Group 2", model[1].Name);
        }

        [Fact]
        public async Task GetAzureManagementGroups_ReturnsEmptyList_WhenNoValuePropertyInResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{ ""someOtherProperty"": [] }")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory
                .Setup(factory => factory.CreateClient("AzureServices"))
                .Returns(httpClient);

            var controller = new ResourceGroupsController(_mockHttpClientFactory.Object, _mockConfiguration.Object);

            // Act
            var result = await controller.ManagementGroups();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<AzureManagementGroup>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task GetAzureManagementGroups_HandlesNullValues_InResponseProperties()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"
                {
                    ""value"": [
                        {
                            ""id"": null,
                            ""type"": null,
                            ""properties"": {
                                ""displayName"": null
                            }
                        }
                    ]
                }")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory
                .Setup(factory => factory.CreateClient("AzureServices"))
                .Returns(httpClient);

            var controller = new ResourceGroupsController(_mockHttpClientFactory.Object, _mockConfiguration.Object);

            // Act
            var result = await controller.ManagementGroups();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<AzureManagementGroup>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal(string.Empty, model[0].Name);
            Assert.Equal(string.Empty, model[0].Id);
            Assert.Equal(string.Empty, model[0].Type);
        }

        [Fact]
        public async Task GetAzureManagementGroups_CallsCorrectEndpoint()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            HttpRequestMessage capturedRequest = null;

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"{ ""value"": [] }")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory
                .Setup(factory => factory.CreateClient("AzureServices"))
                .Returns(httpClient);

            var controller = new ResourceGroupsController(_mockHttpClientFactory.Object, _mockConfiguration.Object);

            // Act
            await controller.ManagementGroups();

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal(HttpMethod.Get, capturedRequest.Method);
            Assert.Equal("providers/Microsoft.Management/managementGroups?api-version=2020-05-01",
                capturedRequest.RequestUri.ToString());
        }
    }
}