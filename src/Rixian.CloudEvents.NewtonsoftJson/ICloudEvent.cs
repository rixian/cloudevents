// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

namespace Rixian.CloudEvents
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Base interface for common properties that are common accross versions.
    /// </summary>
    public interface ICloudEvent
    {
        /// <summary>
        /// Gets or sets the unique id of this cloud event. Required.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets the specification version of this cloud event. Required.
        /// </summary>
        public string SpecVersion { get; }

        /// <summary>
        /// Gets or sets the type of this cloud event. Required.
        /// </summary>
        public string? Type { get; set; }
    }
}
