// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

#nullable disable
namespace Rixian.CloudEvents
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Converter for reading JSON as a cloud event.
    /// </summary>
    [Obsolete("Use the latest version of CloudEvents.")]
    public class CloudEventV0_2JsonConverter : CustomCreationConverter<CloudEventV0_2>
    {
        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (objectType == typeof(CloudEventV0_2))
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

                // Load JObject from stream
                JObject jobj = JObject.Load(reader);
                var cloudEvent = CloudEventV0_2.Deserialize(jobj);

                if (existingValue != null && existingValue is CloudEventV0_2 existingEvent)
                {
                    existingEvent.Id = cloudEvent.Id;
                    existingEvent.Source = cloudEvent.Source;
                    existingEvent.SchemaUrl = cloudEvent.SchemaUrl;
                    existingEvent.ContentType = cloudEvent.ContentType;
                    existingEvent.Time = cloudEvent.Time;
                    existingEvent.Type = cloudEvent.Type;

                    // TODO: Data field
                }

                return cloudEvent;
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }

        /// <inheritdoc/>
        public override CloudEventV0_2 Create(Type objectType)
        {
            return Activator.CreateInstance(objectType) as CloudEventV0_2;
        }
    }
}
