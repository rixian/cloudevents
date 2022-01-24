// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

namespace Rixian.CloudEvents
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A cloud event with JSON data.
    /// </summary>
    public class JsonCloudEvent : CloudEvent
    {
        /// <summary>
        /// Gets or sets the JSON payload.
        /// </summary>
        [JsonRequired]
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, Order = int.MinValue + 7)]
        public JToken? Data { get; set; }
    }
}
