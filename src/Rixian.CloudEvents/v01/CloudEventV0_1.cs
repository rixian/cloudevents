// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

#nullable disable
namespace Rixian.CloudEvents
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A basic CloudEvent.
    /// </summary>
    [Obsolete("Use the latest version of CloudEvents.")]
    public class CloudEventV0_1
    {
        private const string JsonMimeType = "application/json";
        private const string PlainTextMimeType = "text/plain";
        private const string OctetStreamMimeType = "application/octet-stream";

        /// <summary>
        /// Gets or sets the type of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("eventType")]
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the event type version of this cloud event. Optional.
        /// </summary>
        [JsonProperty("eventTypeVersion", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string EventTypeVersion { get; set; }

#pragma warning disable CA1822 // Mark members as static
        /// <summary>
        /// Gets the specification version of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("cloudEventsVersion")]
        public string CloudEventsVersion => "0.1";
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        /// Gets or sets the source of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("source")]
        public Uri Source { get; set; }

        /// <summary>
        /// Gets or sets the unique id of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("eventId")]
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets the schema url of this cloud event. Optional.
        /// </summary>
        [JsonProperty("schemaUrl", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public Uri SchemaUrl { get; set; }

        /// <summary>
        /// Gets or sets the time of this cloud event serialized according to RFC 3339. Optional.
        /// </summary>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        [JsonProperty("eventTime", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public DateTimeOffset? EventTime { get; set; }

        /// <summary>
        /// Gets or sets the content type of this cloud event. Optional.
        /// </summary>
        [JsonProperty("contentType", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the extension values of this cloud event. Optional.
        /// </summary>
        [JsonProperty("extensions", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public JToken Extensions { get; set; }

        /// <summary>
        /// Deserializes a JSON string into a cloud event.
        /// </summary>
        /// <param name="json">The JSON to deserialize.</param>
        /// <returns>A cloud event.</returns>
        public static CloudEventV0_1 Deserialize(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentOutOfRangeException(nameof(json), Properties.Resources.NullOrEmptyStringExceptionMessage);
            }

            var jobj = JObject.Parse(json);
            if (!jobj.ContainsKey("data"))
            {
                return jobj.ToObject<CloudEventV0_1>();
            }

            var contentType = jobj.Value<string>("contentType")?.Trim();

            // SPEC: Section 3.1 - Paragraph 3
            // https://github.com/cloudevents/spec/blob/v0.1/json-format.md#31-special-handling-of-the-data-attribute
            if (contentType != null && (string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase) || contentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)))
            {
                return jobj.ToObject<JsonCloudEventV0_1>();
            }

            try
            {
                // Is there something better than simply trying to deserialize it?
                // Maybe we can inspect the mime type?
                return jobj.ToObject<BinaryCloudEventV0_1>();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return jobj.ToObject<StringCloudEventV0_1>();
            }
        }

        /// <summary>
        /// Creates a generic cloud event with minimal information.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <returns>A generic cloud event.</returns>
        public static CloudEventV0_1 CreateGenericCloudEvent(string eventType, string eventTypeVersion, Uri source) => CreateGenericCloudEvent(eventType, eventTypeVersion, source, null);

        /// <summary>
        /// Creates a generic cloud event with minimal information.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="extensions">The extentions to include.</param>
        /// <returns>A generic cloud event.</returns>
        public static CloudEventV0_1 CreateGenericCloudEvent(string eventType, string eventTypeVersion, Uri source, JToken extensions)
        {
            // Should there be some reasonable upper bound on the payload size?
            return new CloudEventV0_1
            {
                EventId = Guid.NewGuid().ToString(),
                EventTime = DateTimeOffset.UtcNow,
                EventType = eventType,
                EventTypeVersion = eventTypeVersion,
                Source = source,
                Extensions = extensions,
            };
        }

        /// <summary>
        /// Creates a cloud event with a JSON payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The JSON payload.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, JToken payload) => CreateCloudEvent(eventType, eventTypeVersion, source, payload, JsonMimeType, null);

        /// <summary>
        /// Creates a cloud event with a JSON payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The JSON payload.</param>
        /// <param name="contentType">The content type of the JSON payload.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, JToken payload, string contentType) => CreateCloudEvent(eventType, eventTypeVersion, source, payload, contentType, null);

        /// <summary>
        /// Creates a cloud event with a JSON payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The JSON payload.</param>
        /// <param name="extensions">The extentions to include.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, JToken payload, JToken extensions) => CreateCloudEvent(eventType, eventTypeVersion, source, payload, JsonMimeType, extensions);

        /// <summary>
        /// Creates a cloud event with a JSON payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The JSON payload.</param>
        /// <param name="contentType">The content type of the JSON payload.</param>
        /// <param name="extensions">The extentions to include.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, JToken payload, string contentType, JToken extensions)
        {
            // Should there be some reasonable upper bound on the payload size?
            return new JsonCloudEventV0_1
            {
                EventId = Guid.NewGuid().ToString(),
                EventTime = DateTimeOffset.UtcNow,
                EventType = eventType,
                EventTypeVersion = eventTypeVersion,
                Source = source,
                ContentType = contentType,
                Data = payload,
                Extensions = extensions,
            };
        }

        /// <summary>
        /// Creates a cloud event with a string payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The string payload.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, string payload) => CreateCloudEvent(eventType, eventTypeVersion, source, payload, PlainTextMimeType, null);

        /// <summary>
        /// Creates a cloud event with a string payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The string payload.</param>
        /// <param name="contentType">The content type of the string payload.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, string payload, string contentType) => CreateCloudEvent(eventType, eventTypeVersion, source, payload, contentType, null);

        /// <summary>
        /// Creates a cloud event with a string payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The string payload.</param>
        /// <param name="extensions">The extentions to include.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, string payload, JToken extensions) => CreateCloudEvent(eventType, eventTypeVersion, source, payload, PlainTextMimeType, extensions);

        /// <summary>
        /// Creates a cloud event with a string payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The string payload.</param>
        /// <param name="contentType">The content type of the string payload.</param>
        /// <param name="extensions">The extentions to include.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, string payload, string contentType, JToken extensions)
        {
            // Should there be some reasonable upper bound on the payload size?
            return new StringCloudEventV0_1
            {
                EventId = Guid.NewGuid().ToString(),
                EventTime = DateTimeOffset.UtcNow,
                EventType = eventType,
                EventTypeVersion = eventTypeVersion,
                Source = source,
                ContentType = contentType,
                Data = payload,
                Extensions = extensions,
            };
        }

        /// <summary>
        /// Creates a cloud event with a binary payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The binary payload.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, byte[] payload) => CreateCloudEvent(eventType, eventTypeVersion, source, payload, OctetStreamMimeType, null);

        /// <summary>
        /// Creates a cloud event with a binary payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The binary payload.</param>
        /// <param name="contentType">The content type of the binary payload.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, byte[] payload, string contentType) => CreateCloudEvent(eventType, eventTypeVersion, source, payload, contentType, null);

        /// <summary>
        /// Creates a cloud event with a binary payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The binary payload.</param>
        /// <param name="extensions">The extentions to include.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, byte[] payload, JToken extensions) => CreateCloudEvent(eventType, eventTypeVersion, source, payload, OctetStreamMimeType, extensions);

        /// <summary>
        /// Creates a cloud event with a binary payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventTypeVersion">The version of the event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The binary payload.</param>
        /// <param name="contentType">The content type of the binary payload.</param>
        /// <param name="extensions">The extentions to include.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEventV0_1 CreateCloudEvent(string eventType, string eventTypeVersion, Uri source, byte[] payload, string contentType = "application/octet-stream", JToken extensions = null)
        {
            // Should there be some reasonable upper bound on the payload size?
            return new BinaryCloudEventV0_1
            {
                EventId = Guid.NewGuid().ToString(),
                EventTime = DateTimeOffset.UtcNow,
                EventType = eventType,
                EventTypeVersion = eventTypeVersion,
                Source = source,
                ContentType = contentType,
                Data = payload,
                Extensions = extensions,
            };
        }
    }
}
