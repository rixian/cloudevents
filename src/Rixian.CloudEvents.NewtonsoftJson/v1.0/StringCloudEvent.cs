// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

namespace Rixian.CloudEvents
{
    using Newtonsoft.Json;

    /// <summary>
    /// A cloud event with string data.
    /// </summary>
    public class StringCloudEvent : CloudEvent
    {
        /// <summary>
        /// Gets or sets the string payload.
        /// </summary>
        [JsonRequired]
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, Order = int.MinValue + 7)]
        public string? Data { get; set; }
    }
}
