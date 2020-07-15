// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

namespace Rixian.CloudEvents
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Extension methods for working with Cloud Event conversions.
    /// </summary>
    internal static class CloudEventExtensions
    {
        /// <summary>
        /// Try to get and then remove a value from a dictionary.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key to look for.</param>
        /// <returns>The dictionary value.</returns>
        public static JToken? TryGetRemoveValue(this IDictionary<string, JToken?>? dict, string key)
        {
            if (dict is null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (dict.TryGetValue(key, out JToken? token))
            {
                dict.Remove(key);
                return token;
            }

            return null;
        }

        /// <summary>
        /// Adds a value to a dictionary as a JToken.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key to look for.</param>
        /// <param name="value">The value to insert.</param>
        public static void AddValue(this IDictionary<string, JToken?>? dict, string key, object value)
        {
            if (dict is null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            dict.Add(key, JToken.FromObject(value));
        }

        /// <summary>
        /// Converts a JToken to a dictionary if it is a JObject.
        /// </summary>
        /// <param name="token">The JToken.</param>
        /// <returns>The dictionary.</returns>
        public static IDictionary<string, JToken?>? AsDictionary(this JToken? token)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (token is JObject jobj)
            {
                return jobj;
            }

            return null;
        }
    }
}
