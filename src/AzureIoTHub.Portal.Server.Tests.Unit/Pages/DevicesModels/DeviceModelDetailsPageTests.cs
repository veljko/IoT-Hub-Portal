﻿using AzureIoTHub.Portal.Client.Pages.DeviceModels;
using AzureIoTHub.Portal.Server.Tests.Unit.Helpers;
using AzureIoTHub.Portal.Shared.Models;
using AzureIoTHub.Portal.Shared.Models.V10;
using AzureIoTHub.Portal.Shared.Models.V10.DeviceModel;
using AzureIoTHub.Portal.Shared.Models.V10.LoRaWAN.LoRaDeviceModel;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Interop;
using MudBlazor.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Unit.Pages
{
    [TestFixture]
    public class DeviceModelDetaislPageTests
    {
        private Bunit.TestContext testContext;
        private string mockModelId = Guid.NewGuid().ToString();

        private MockRepository mockRepository;
        private Mock<IDialogService> mockDialogService;
        private FakeNavigationManager mockNavigationManager;
        private MockHttpMessageHandler mockHttpClient;

        private string apiBaseUrl => $"/api/models/{mockModelId}";
        private string lorawanApiBaseUrl => $"/api/lorawan/models/{mockModelId}";

        [SetUp]
        public void SetUp()
        {
            this.testContext = new Bunit.TestContext();

            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockDialogService = this.mockRepository.Create<IDialogService>();
            this.mockHttpClient = testContext.Services
                                            .AddMockHttpClient();

            testContext.Services.AddSingleton(this.mockDialogService.Object);

            testContext.Services.AddMudServices();

            testContext.JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);
            testContext.JSInterop.SetupVoid("mudPopover.connect", _ => true);
            testContext.JSInterop.SetupVoid("Blazor._internal.InputFile.init", _ => true);
            testContext.JSInterop.Setup<BoundingClientRect>("mudElementRef.getBoundingClientRect", _ => true);
            testContext.JSInterop.Setup<IEnumerable<BoundingClientRect>>("mudResizeObserver.connect", _ => true);

            mockNavigationManager = testContext.Services.GetRequiredService<FakeNavigationManager>();

            this.mockHttpClient.AutoFlush = true;
        }

        private IRenderedComponent<TComponent> RenderComponent<TComponent>(params ComponentParameter[] parameters)
         where TComponent : IComponent
        {
            return this.testContext.RenderComponent<TComponent>(parameters);
        }

        [Test]
        public void Click_On_Save_Should_Post_Device_Model_Data()
        {
            // Arrange
            var expectedProperties = Enumerable.Range(0, 2)
                .Select(x => new DeviceProperty
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    IsWritable = true,
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = Shared.Models.DevicePropertyType.Double
                }).ToArray();

            var expectedModel = SetupMockDeviceModel(properties: expectedProperties);

            this.mockHttpClient.When(HttpMethod.Put, $"{ apiBaseUrl}")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<DeviceModel>(jsonContent.Value);
                    var deviceModel = jsonContent.Value as DeviceModel;

                    Assert.AreEqual(expectedModel.ModelId, deviceModel.ModelId);
                    Assert.AreEqual(expectedModel.Name, deviceModel.Name);
                    Assert.AreEqual(expectedModel.Description, deviceModel.Description);
                    Assert.AreEqual(expectedModel.SupportLoRaFeatures, false);
                    Assert.AreEqual(expectedModel.IsBuiltin, false);

                    return true;
                })
                .RespondText(string.Empty);

            this.mockHttpClient
                .When(HttpMethod.Post, $"{ apiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<List<DeviceProperty>>(jsonContent.Value);
                    var properties = jsonContent.Value as IEnumerable<DeviceProperty>;

                    Assert.AreEqual(expectedProperties.Count(), properties.Count());

                    foreach (var expectedProperty in expectedProperties)
                    {
                        var property = properties.Single(x => x.Name == expectedProperty.Name);

                        Assert.AreEqual(expectedProperty.Name, property.Name);
                        Assert.AreEqual(expectedProperty.DisplayName, property.DisplayName);
                        Assert.AreEqual(expectedProperty.PropertyType, property.PropertyType);
                    }

                    return true;
                })
                .RespondText(string.Empty);

            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), mockModelId));

            var saveButton = cut.WaitForElement("#SaveButton");

            // Act
            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models"));

            // Assert            
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void Click_On_Add_Property_Should_Add_NewProperty()
        {
            // Arrange
            var propertyName = Guid.NewGuid().ToString();
            var displayName = Guid.NewGuid().ToString();

            var expectedModel = SetupMockDeviceModel(properties: new DeviceProperty[0]);

            this.mockHttpClient.When(HttpMethod.Put, $"{ apiBaseUrl}")
                .RespondText(string.Empty);

            this.mockHttpClient.When(HttpMethod.Post, $"{ apiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<List<DeviceProperty>>(jsonContent.Value);
                    var properties = jsonContent.Value as IEnumerable<DeviceProperty>;

                    Assert.AreEqual(1, properties.Count());

                    var property = properties.Single(x => x.Name == propertyName);

                    Assert.AreEqual(propertyName, property.Name);
                    Assert.AreEqual(displayName, property.DisplayName);
                    Assert.AreEqual(DevicePropertyType.Boolean, property.PropertyType);
                    Assert.IsTrue(property.IsWritable);

                    return true;
                })
                .RespondText(string.Empty);

            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), mockModelId));

            var saveButton = cut.WaitForElement("#SaveButton");
            var addPropertyButton = cut.WaitForElement("#addPropertyButton");

            // Act
            addPropertyButton.Click();

            cut.WaitForElement($"#property- #{nameof(DeviceProperty.Name)}").Change(propertyName);

            var propertyCssSelector = $"#property-";

            cut.Find($"{propertyCssSelector} #{nameof(DeviceProperty.DisplayName)}").Change(displayName);
            cut.Find($"{propertyCssSelector} #{nameof(DeviceProperty.PropertyType)}").Change(DevicePropertyType.Boolean.ToString());
            cut.Find($"{propertyCssSelector} #{nameof(DeviceProperty.IsWritable)}").Change(true);

            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models"));

            // Assert
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void Click_On_Remove_Property_Should_Remove_The_Property()
        {
            // Arrange
            var propertyName = Guid.NewGuid().ToString();
            var displayName = Guid.NewGuid().ToString();

            var expectedModel = SetupMockDeviceModel(properties: new DeviceProperty[]
            {
                new DeviceProperty
                {
                    Name   = propertyName,
                    DisplayName = displayName,
                    PropertyType = DevicePropertyType.String,
                    IsWritable = true
                }
            });

            this.mockHttpClient.When(HttpMethod.Put, $"{ apiBaseUrl}")
                .RespondText(string.Empty);

            this.mockHttpClient.When(HttpMethod.Post, $"{ apiBaseUrl}/properties")
                .With(m =>
                {
                    Assert.IsAssignableFrom<JsonContent>(m.Content);
                    var jsonContent = m.Content as JsonContent;

                    Assert.IsAssignableFrom<List<DeviceProperty>>(jsonContent.Value);
                    var properties = jsonContent.Value as IEnumerable<DeviceProperty>;

                    Assert.AreEqual(0, properties.Count());

                    return true;
                })
                .RespondText(string.Empty);

            var cut = RenderComponent<DeviceModelDetailPage>
                    (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), mockModelId));

            var saveButton = cut.WaitForElement("#SaveButton");
            var removePropertyButton = cut.WaitForElement("#DeletePropertyButton");

            // Act
            removePropertyButton.Click();

            saveButton.Click();
            cut.WaitForState(() => this.mockNavigationManager.Uri.EndsWith("/device-models"));

            // Assert
            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void When_Present_Model_Details_Should_Display_Properties()
        {
            // Arrange
            var properties = Enumerable.Range(0, 10)
                .Select(x => new DeviceProperty
                {
                    DisplayName = Guid.NewGuid().ToString(),
                    IsWritable = true,
                    Name = Guid.NewGuid().ToString(),
                    PropertyType = Shared.Models.DevicePropertyType.Double
                }).ToArray();

            SetupMockDeviceModel(properties: properties);

            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), mockModelId));

            // Act
            cut.WaitForElement("#form", 1.Seconds());

            // Assert
            foreach (var item in properties)
            {
                var propertyCssSelector = $"#property-{item.Name}";

                cut.Find(propertyCssSelector);
                Assert.AreEqual(item.DisplayName, cut.Find($"{propertyCssSelector} #{nameof(item.DisplayName)}").Attributes["value"].Value);
                Assert.AreEqual(item.Name, cut.Find($"{propertyCssSelector} #{nameof(item.Name)}").Attributes["value"].Value);
                Assert.AreEqual(item.PropertyType.ToString(), cut.Find($"{propertyCssSelector} #{nameof(item.PropertyType)}").Attributes["value"].Value);
                Assert.AreEqual(item.IsWritable.ToString().ToLower(), cut.Find($"{propertyCssSelector} #{nameof(item.IsWritable)}").Attributes["aria-checked"].Value);
            }

            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void When_Lora_Feature_Is_Disabled_Model_Details_Should_Not_Display_LoRaWAN_Tab()
        {
            // Arrange
            SetupMockDeviceModel();

            var cut = RenderComponent<DeviceModelDetailPage>
                (ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), mockModelId));

            // Act

            cut.WaitForElement("#form", 1.Seconds());

            // Assert
            var tabs = cut.FindAll(".mud-tabs .mud-tab");
            Assert.AreEqual(1, tabs.Count);
            Assert.AreEqual("General", tabs.Single().TextContent);

            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        [Test]
        public void When_Lora_Feature_Is_Enabled_Model_Details_Should_Display_LoRaWAN_Tab()
        {
            // Arrange
            SetupMockLoRaWANDeviceModel();

            // Act
            var cut = RenderComponent<DeviceModelDetailPage>(
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.ModelID), mockModelId),
                    ComponentParameter.CreateParameter(nameof(DeviceModelDetailPage.IsLoRa), true));

            cut.WaitForElement("#form");

            // Assert
            var tabs = cut.FindAll(".mud-tabs .mud-tab");
            Assert.AreEqual(2, tabs.Count);
            Assert.AreEqual("General", tabs[0].TextContent);
            Assert.AreEqual("LoRaWAN", tabs[1].TextContent);

            this.mockHttpClient.VerifyNoOutstandingExpectation();
        }

        private DeviceModel SetupMockDeviceModel(DeviceProperty[] properties = null)
        {
            var deviceModel = new DeviceModel
            {
                ModelId = this.mockModelId,
                Name = this.mockModelId,
                Description = Guid.NewGuid().ToString(),
                IsBuiltin = false,
                ImageUrl = $"http://fake.local/{this.mockModelId}",
                SupportLoRaFeatures = false
            };

            this.mockHttpClient.When(apiBaseUrl)
                                .RespondJson(deviceModel);

            this.mockHttpClient.When($"{apiBaseUrl}/avatar")
                    .RespondText($"http://fake.local/{this.mockModelId}");

            this.mockHttpClient.When($"{apiBaseUrl}/properties")
                .RespondJson(properties ?? new DeviceProperty[0]);

            return deviceModel;
        }

        private LoRaDeviceModel SetupMockLoRaWANDeviceModel(DeviceProperty[] properties = null, DeviceModelCommand[] commands = null)
        {
            var deviceModel = new LoRaDeviceModel
            {
                ModelId = this.mockModelId,
                Name = this.mockModelId,
                Description = Guid.NewGuid().ToString(),
                IsBuiltin = false,
                ImageUrl = $"http://fake.local/{this.mockModelId}",
                SupportLoRaFeatures = true
            };

            this.mockHttpClient.When(HttpMethod.Get, lorawanApiBaseUrl)
                    .RespondJson(deviceModel);

            this.mockHttpClient.When(HttpMethod.Get, $"{lorawanApiBaseUrl}/avatar")
                    .RespondText($"http://fake.local/{this.mockModelId}");

            this.mockHttpClient.When(HttpMethod.Get, $"{lorawanApiBaseUrl}/commands")
                .RespondJson(commands ?? new DeviceModelCommand[0]);

            this.mockHttpClient.When(HttpMethod.Get, $"{lorawanApiBaseUrl}/properties")
                    .RespondJson(properties ?? new DeviceProperty[0]);

            this.mockHttpClient.Fallback
                .With(m =>
                {
                    Debugger.Break();
                    return true;
                });

            return deviceModel;
        }
    }
}
