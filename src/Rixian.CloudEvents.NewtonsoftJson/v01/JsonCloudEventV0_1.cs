// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

#nullable disable
namespace Rixian.CloudEvents
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A cloud event with JSON data.
    /// </summary>
    [Obsolete("Use the latest version of CloudEvents.")]
    public class JsonCloudEventV0_1 : CloudEventV0_1
    {
        /// <summary>
        /// Gets or sets the JSON payload.
        /// </summary>
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public JToken Data { get; set; }
    }
}
