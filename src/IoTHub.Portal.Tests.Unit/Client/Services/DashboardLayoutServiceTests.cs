// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Tests.Unit.Client.Services
{
    [TestFixture]
    public class DashboardLayoutServiceTests
    {
        [Test]
        public void RefreshDashboardShouldRaiseRefreshDashboardOccurredEvent()
        {
            // Arrange
            var receivedEvents = new List<string>();
            var dashboardLayoutService = new DashboardLayoutService();
            dashboardLayoutService.RefreshDashboardOccurred += (sender, _) =>
            {
                receivedEvents.Add(sender?.GetType().ToString());
            };

            // Act
            dashboardLayoutService.RefreshDashboard();

            // Assert
            _ = receivedEvents.Count.Should().Be(1);
            _ = receivedEvents.First().Should().Be(typeof(DashboardLayoutService).ToString());
        }
    }
}
