// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

#nullable disable
namespace Rixian.CloudEvents.Tests.V02
{
    using System;
    using System.Collections.Generic;
    using System.Text;

#pragma warning disable CS0618 // Type or member is obsolete
    public class TestCloudEvent : CloudEventV0_2
#pragma warning restore CS0618 // Type or member is obsolete
    {
        public string Test { get; set; }
    }
}
