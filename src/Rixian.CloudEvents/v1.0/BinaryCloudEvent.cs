// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

namespace Rixian.CloudEvents
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// A cloud event with binary data.
    /// </summary>
    public class BinaryCloudEvent : CloudEvent
    {
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        /// <summary>
        /// Gets or sets the binary payload.
        /// </summary>
        [JsonPropertyName("data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public byte[]? Data { get; set; } = default!;
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
