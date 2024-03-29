﻿// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Rixian.CloudEvents
{
    /// <summary>
    /// A cloud event with string data.
    /// </summary>
    public class StringCloudEvent : CloudEvent
    {
        /// <summary>
        /// Gets or sets the string payload.
        /// </summary>
        // *** [JsonRequired]
        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Data { get; set; }
    }
}
