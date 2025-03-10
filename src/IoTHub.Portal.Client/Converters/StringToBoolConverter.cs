// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Converters
{
    public class StringToBoolConverter : BoolConverter<string>
    {
        public StringToBoolConverter()
        {
            SetFunc = OnSet;
            GetFunc = OnGet;
        }

#pragma warning disable CA1308 // Normalize strings to uppercase
        private string OnGet(bool? value) => value?.ToString()?.ToLowerInvariant() ?? string.Empty;
#pragma warning restore CA1308 // Normalize strings to uppercase

        private bool? OnSet(string arg)
        {
            if (!bool.TryParse(arg, out var value))
            {
                return null;
            }

            return value;
        }
    }
}
