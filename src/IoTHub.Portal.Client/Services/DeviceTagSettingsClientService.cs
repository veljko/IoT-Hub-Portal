// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Services
{
    public class DeviceTagSettingsClientService : IDeviceTagSettingsClientService
    {
        private readonly HttpClient http;

        public DeviceTagSettingsClientService(HttpClient http)
        {
            this.http = http;
        }

        public Task CreateOrUpdateDeviceTag(DeviceTagDto deviceTag)
        {
            var deviceTagAsJson = JsonSerializer.Serialize(deviceTag);
            var content = new StringContent(deviceTagAsJson, Encoding.UTF8, "application/json");

            return this.http.PatchAsync("api/settings/device-tags", content);
        }

        public Task DeleteDeviceTagByName(string deviceTagName)
        {
            return this.http.DeleteAsync($"api/settings/device-tags/{deviceTagName}");
        }

        public async Task<IList<DeviceTagDto>> GetDeviceTags()
        {
            return await this.http.GetFromJsonAsync<List<DeviceTagDto>>("api/settings/device-tags") ?? new List<DeviceTagDto>();
        }

    }
}
