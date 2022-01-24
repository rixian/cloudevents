// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

namespace Rixian.CloudEvents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Extension methods for working with Cloud Event conversions.
    /// </summary>
    public static class CloudEventExtensions
    {
        /// <summary>
        /// Try to get and then remove a value from a dictionary.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key to look for.</param>
        /// <returns>The dictionary value.</returns>
        public static JsonElement? TryGetRemoveValue(this IDictionary<string, JsonElement?>? dict, string key)
        {
            if (dict is null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (dict.TryGetValue(key, out JsonElement? token))
            {
                dict.Remove(key);
                return token;
            }

            return null;
        }

        /// <summary>
        /// Adds a value to a dictionary as a JsonElement.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="value">The value to insert.</param>
        public static void AddValue(this IDictionary<string, JsonElement?>? dict, string key, object value)
        {
            if (dict is null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            dict.Add(key, JsonSerializer.SerializeToElement(value));
        }

        /// <summary>
        /// Converts a JsonElement to a dictionary if it is a JObject.
        /// </summary>
        /// <param name="token">The JsonElement.</param>
        /// <returns>The dictionary.</returns>
        public static IDictionary<string, JsonProperty>? AsDictionary(this JsonElement token)
        {
            if (token.ValueKind == JsonValueKind.Object)
            {
                return token.EnumerateObject().ToDictionary(p => p.Name);
            }

            return null;
        }
    }
}
