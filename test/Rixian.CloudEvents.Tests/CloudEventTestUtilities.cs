// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

#pragma warning disable CS0618 // Type or member is obsolete
namespace Rixian.CloudEvents.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class CloudEventTestUtilities
    {
        public static void ValidateCloudEvent(CloudEventV0_1 cloudEvent)
        {
            if (string.IsNullOrWhiteSpace(cloudEvent.EventType))
            {
                throw new Exception("The eventType property is required and cannot be null or empty.");
            }

            if (cloudEvent.EventTypeVersion != null && string.IsNullOrWhiteSpace(cloudEvent.EventType))
            {
                throw new Exception("The eventTypeVersion property must have a value if supplied.");
            }

            if (string.IsNullOrWhiteSpace(cloudEvent.CloudEventsVersion))
            {
                throw new Exception("The cloudEventsVersion property is required and cannot be null or empty.");
            }

            if (cloudEvent.Source == null)
            {
                throw new Exception("The source property is required.");
            }

            if (string.IsNullOrWhiteSpace(cloudEvent.EventId))
            {
                throw new Exception("The eventId property is required and cannot be null or empty.");
            }

            if (cloudEvent.ContentType != null && string.IsNullOrWhiteSpace(cloudEvent.ContentType))
            {
                throw new Exception("The contentType property must have a value if supplied.");
            }

            if (cloudEvent.Extensions != null && cloudEvent.Extensions.HasValues == false)
            {
                throw new Exception("The extensions property must have a value if supplied.");
            }
        }
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
