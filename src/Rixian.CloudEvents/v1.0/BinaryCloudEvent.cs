﻿// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace Rixian.CloudEvents
{
    /// <summary>
    /// A cloud event with binary data.
    /// </summary>
    public class BinaryCloudEvent : CloudEvent
    {
#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// Gets or sets the binary payload.
        /// </summary>
        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public byte[] Data { get; set; } = default!;
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
