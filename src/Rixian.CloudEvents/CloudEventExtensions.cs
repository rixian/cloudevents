// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

namespace Rixian.CloudEvents
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json.Linq;

    internal static class CloudEventExtensions
    {
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

        public static JToken? TryGetRemoveValue(this JToken? parentToken, string key)
        {
            if (parentToken is null)
            {
                throw new ArgumentNullException(nameof(parentToken));
            }

            if (parentToken is JObject jobj)
            {
                if (jobj.TryGetValue(key, StringComparison.OrdinalIgnoreCase, out JToken token))
                {
                    jobj.Remove(key);
                    return token;
                }
            }

            return null;
        }

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
