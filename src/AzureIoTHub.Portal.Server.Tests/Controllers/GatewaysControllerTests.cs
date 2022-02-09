﻿using AzureIoTHub.Portal.Server.Controllers;
using AzureIoTHub.Portal.Server.Managers;
using AzureIoTHub.Portal.Server.Services;
using AzureIoTHub.Portal.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Controllers
{
    [TestFixture]
    public class GatewaysControllerTests
    {
        private MockRepository mockRepository;

        private Mock<IConfiguration> mockConfiguration;
        private Mock<ILogger<GatewaysController>> mockLogger;
        private Mock<RegistryManager> mockRegistryManager;
        private Mock<IConnectionStringManager> mockConnectionStringManager;
        private Mock<IDeviceService> mockDeviceService;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
            this.mockLogger = this.mockRepository.Create<ILogger<GatewaysController>>();
            this.mockRegistryManager = this.mockRepository.Create<RegistryManager>();
            this.mockConnectionStringManager = this.mockRepository.Create<IConnectionStringManager>();
            this.mockDeviceService = this.mockRepository.Create<IDeviceService>();
        }

        private GatewaysController CreateGatewaysController()
        {
            return new GatewaysController(
                this.mockConfiguration.Object,
                this.mockLogger.Object,
                this.mockRegistryManager.Object,
                this.mockConnectionStringManager.Object,
                this.mockDeviceService.Object);
        }

        [Test]
        public async Task Get_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var twin = new Twin("aaa");
            twin.Tags["purpose"] = "test";

            this.mockDeviceService.Setup(x => x.GetAllEdgeDevice())
                .ReturnsAsync(new[]
                {
                    twin
                });

            this.mockDeviceService.Setup(x => x.GetDeviceTwin(It.Is<string>(c => c == twin.DeviceId)))
                .ReturnsAsync(twin);

            var gatewaysController = this.CreateGatewaysController();

            // Act
            var result = await gatewaysController.Get();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<List<GatewayListItem>>(okObjectResult.Value);
            var gatewayList = okObjectResult.Value as List<GatewayListItem>;
            Assert.IsNotNull(gatewayList);
            Assert.AreEqual(1, gatewayList.Count);
            var gateway = gatewayList[0];
            Assert.IsNotNull(gateway);
            Assert.AreEqual(twin.DeviceId, gateway.DeviceId);
            Assert.AreEqual("test", gateway.Type);
            Assert.AreEqual(0, gateway.NbDevices);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetSymmetricKey_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var gatewaysController = this.CreateGatewaysController();
            this.mockConnectionStringManager.Setup(c => c.GetSymmetricKey("aaa", "bbb"))
                .ReturnsAsync("dfhjkfdgh");

            // Act
            var result = await gatewaysController.GetSymmetricKey("aaa", "bbb");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsAssignableFrom<OkObjectResult>(result);
            var okObjectResult = result as ObjectResult;

            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(200, okObjectResult.StatusCode);
            Assert.IsNotNull(okObjectResult.Value);
            Assert.IsAssignableFrom<string>(okObjectResult.Value);
            Assert.AreEqual("dfhjkfdgh", okObjectResult.Value);

            this.mockRepository.VerifyAll();
        }

        [Test]
        public async Task CreateGatewayAsync_StateUnderTest_ExpectedBehavior()
        {
            await Task.CompletedTask;
            Assert.Inconclusive();
        }

        [Test]
        public async Task UpdateDeviceAsync_StateUnderTest_ExpectedBehavior()
        {
            await Task.CompletedTask;
            Assert.Inconclusive();
        }

        [Test]
        public async Task DeleteDeviceAsync_StateUnderTest_ExpectedBehavior()
        {
            await Task.CompletedTask;
            Assert.Inconclusive();
        }

        [Test]
        public async Task ExecuteMethode_StateUnderTest_ExpectedBehavior()
        {
            await Task.CompletedTask;
            Assert.Inconclusive();
        }
    }
}
