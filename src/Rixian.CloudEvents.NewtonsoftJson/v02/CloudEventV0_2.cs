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
    /// A basic CloudEvent.
    /// </summary>
    [Obsolete("Use the latest version of CloudEvents.")]
    [JsonConverter(typeof(CloudEventV0_2JsonConverter))]
    public class CloudEventV0_2 : ICloudEvent
    {
        private const string RFC3339RegexPattern = @"^([0-9]+)-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])[Tt]([01][0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9]|60)(\.[0-9]+)?(([Zz])|([\+|\-]([01][0-9]|2[0-3]):[0-5][0-9]))$";

        private const string RFC2046RegexPattern = @"[a-zA-Z0-9!#$%^&\\*_\\-\\+{}\\|'.`~]+/[a-zA-Z0-9!#$%^&\\*_\\-\\+{}\\|'.`~]+";

        // See: https://stackoverflow.com/a/475217
        private const string Base64RegexPattern = @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$";

        private const string JsonMimeType = "application/json";

        private const string PlainTextMimeType = "text/plain";

        private const string OctetStreamMimeType = "application/octet-stream";

        private static Regex rfc3339Regex = new Regex(RFC3339RegexPattern);
        private static Regex rfc2046Regex = new Regex(RFC2046RegexPattern);
        private static Regex base64Regex = new Regex(Base64RegexPattern);

        /// <summary>
        /// Gets or sets the unique id of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("id", Order = int.MinValue)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("type", Order = int.MinValue + 1)]
        public string Type { get; set; }

#pragma warning disable CA1822 // Mark members as static
        /// <summary>
        /// Gets the specification version of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("specversion", Order = int.MinValue + 2)]
        public string SpecVersion => "0.2";
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        /// Gets or sets the source of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("source", Order = int.MinValue + 3)]
        public Uri Source { get; set; }

        /// <summary>
        /// Gets or sets the time of this cloud event serialized according to RFC 3339. Optional.
        /// </summary>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, Order = int.MinValue + 4)]
        public DateTimeOffset? Time { get; set; }

        /// <summary>
        /// Gets or sets the schema url of this cloud event. Optional.
        /// </summary>
        [JsonProperty("schemaurl", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, Order = int.MinValue + 5)]
        public Uri SchemaUrl { get; set; }

        /// <summary>
        /// Gets or sets the content type of this cloud event. Required.
        /// </summary>
        [JsonProperty("contenttype", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, Order = int.MinValue + 6)]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the undefined extension attributes. Optional.
        /// </summary>
        [JsonExtensionData]
#pragma warning disable CA2227 // Collection properties should be read only
        public IDictionary<string, JToken> ExtensionAttributes { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Validates that a given JSON string is a cloud event.
        /// </summary>
        /// <param name="json">The JSON to validate.</param>
        /// <returns>True if the string is a valid cloud event, otherwise false.</returns>
        public static bool ValidateJson(string json) => ValidateJsonDetailed(json).Item1;

        /// <summary>
        /// Validates that a given JObject is a cloud event.
        /// </summary>
        /// <param name="jobj">The JObject to validate.</param>
        /// <returns>True if the string is a valid cloud event, otherwise false.</returns>
        public static bool ValidateJson(JObject jobj) => ValidateJsonDetailed(jobj).Item1;

        /// <summary>
        /// Validates that a given JSON string is a cloud event.
        /// </summary>
        /// <param name="json">The JSON to validate.</param>
        /// <returns>A tuple containing a flag indicating if the JSON string is a valid cloud event, and a list of all errors.</returns>
        public static Tuple<bool, IReadOnlyList<string>> ValidateJsonDetailed(string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject(
                    json,
                    new JsonSerializerSettings
                    {
                        DateParseHandling = DateParseHandling.None,
                    });
                return ValidateJsonDetailed(JObject.FromObject(obj));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return Tuple.Create<bool, IReadOnlyList<string>>(false, new[] { "Failed to parse json." });
            }
        }

        /// <summary>
        /// Validates that a given JObject is a cloud event.
        /// </summary>
        /// <param name="jobj">The JObject to validate.</param>
        /// <returns>A tuple containing a flag indicating if the JObject is a valid cloud event, and a list of all errors.</returns>
        public static Tuple<bool, IReadOnlyList<string>> ValidateJsonDetailed(JObject jobj)
        {
            if (jobj == null)
            {
                throw new ArgumentNullException(nameof(jobj));
            }

            var errors = new List<string>();
            try
            {
                bool result = true;

                var containsId = jobj.ContainsKey("id");
                var containsType = jobj.ContainsKey("type");
                var containsSpecVersion = jobj.ContainsKey("specversion");
                var containsSource = jobj.ContainsKey("source");

                // var containsSubject = jobj.ContainsKey("subject");
                var containsTime = jobj.ContainsKey("time");
                var containsSchemaUrl = jobj.ContainsKey("schemaurl");
                var containsData = jobj.ContainsKey("data");
                var containsContentType = jobj.ContainsKey("contenttype");

                // var containsDataContentEncoding = jobj.ContainsKey("datacontentencoding");
                var id = jobj["id"]?.ToString();
                var type = jobj["type"]?.ToString();
                var specVersion = jobj["specversion"]?.ToString();
                var source = jobj["source"]?.ToString();

                // var subject = jobj["subject"]?.ToString();
                var time = jobj["time"]?.ToString();
                var schemaUrl = jobj["schemaurl"]?.ToString();
                var data = jobj["data"]?.ToString();
                var contentType = jobj["contenttype"]?.ToString();

                // var dataContentEncoding = jobj["datacontentencoding"]?.ToString();

                // [id]
                // Required, non-empty string
                if (!containsId)
                {
                    result = false;
                    errors.Add("Required field 'id' is missing.");
                }
                else if (id == null)
                {
                    result = false;
                    errors.Add("Required field 'id' is null.");
                }
                else if (string.IsNullOrWhiteSpace(id))
                {
                    result = false;
                    errors.Add("Required field 'id' must contain a value.");
                }

                // [type]
                // Required, non-empty string
                if (!containsType)
                {
                    result = false;
                    errors.Add("Required field 'type' is missing.");
                }
                else if (type == null)
                {
                    result = false;
                    errors.Add("Required field 'type' is null.");
                }
                else if (string.IsNullOrWhiteSpace(type))
                {
                    result = false;
                    errors.Add("Required field 'type' must contain a value.");
                }

                // [specversion]
                // Required, non-empty string set to 0.2
                if (!containsSpecVersion)
                {
                    result = false;
                    errors.Add("Required field 'specversion' is missing.");
                }
                else if (specVersion == null)
                {
                    result = false;
                    errors.Add("Required field 'specversion' is null.");
                }
                else if (string.IsNullOrWhiteSpace(specVersion))
                {
                    result = false;
                    errors.Add("Required field 'specversion' must contain a value.");
                }
                else if (string.Equals(specVersion, "0.2", StringComparison.OrdinalIgnoreCase) == false)
                {
                    result = false;
                    errors.Add("Required field 'specversion' must contain the value '0.2'");
                }

                // [source]
                // Required, non-null Uri
                if (!containsSource)
                {
                    result = false;
                    errors.Add("Required field 'source' is missing.");
                }
                else if (source == null)
                {
                    result = false;
                    errors.Add("Required field 'source' is null.");
                }
                else if (string.IsNullOrWhiteSpace(source))
                {
                    result = false;
                    errors.Add("Required field 'source' must contain a value.");
                }
                else if (Uri.TryCreate(source, UriKind.RelativeOrAbsolute, out Uri sourceUri) == false)
                {
                    result = false;
                    errors.Add("Required field 'source' must contain a valid Uri.");
                }

                // //
                // // [subject]
                // // Optional, non-empty string
                // if (containsSubject)
                // {
                //     if (subject == null)
                //     {
                //         result = false;
                //         errors.Add("Optional field 'subject' is null.");
                //     }
                //     else if (string.IsNullOrWhiteSpace(subject))
                //     {
                //         result = false;
                //         errors.Add("Optional field 'subject' must contain a value.");
                //     }
                // }

                // [time]
                // Optional, non-empty string
                if (containsTime)
                {
                    if (time == null)
                    {
                        result = false;
                        errors.Add("Optional field 'time' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(time))
                    {
                        result = false;
                        errors.Add("Optional field 'time' must contain a value.");
                    }
                    else if (rfc3339Regex.IsMatch(time) == false)
                    {
                        result = false;
                        errors.Add("Optional field 'time' must adhere to the format specified in RFC 3339.");
                    }
                }

                // [schemaurl]
                // Optional, non-null Uri
                if (containsSchemaUrl)
                {
                    if (schemaUrl == null)
                    {
                        result = false;
                        errors.Add("Optional field 'schemaurl' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(schemaUrl))
                    {
                        result = false;
                        errors.Add("Required field 'schemaurl' must contain a value.");
                    }
                    else if (Uri.TryCreate(schemaUrl, UriKind.RelativeOrAbsolute, out Uri schemaUri) == false)
                    {
                        result = false;
                        errors.Add("Optional field 'schemaurl' must contain a valid Uri.");
                    }
                }

                // [contenttype]
                // Optional, non-empty string
                if (containsContentType)
                {
                    if (contentType == null)
                    {
                        result = false;
                        errors.Add("Optional field 'contenttype' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(contentType))
                    {
                        result = false;
                        errors.Add("Optional field 'contenttype' must contain a value.");
                    }
                    else if (rfc2046Regex.IsMatch(contentType) == false)
                    {
                        result = false;
                        errors.Add("Optional field 'contenttype' must adhere to the format specified in RFC 2046.");
                    }
                }

                // //
                // // [datacontentencoding]
                // // Optional, non-empty string
                // if (containsDataContentEncoding)
                // {
                //     if (dataContentEncoding == null)
                //     {
                //         result = false;
                //         errors.Add("Optional field 'datacontentencoding' is null.");
                //     }
                //     else if (string.IsNullOrWhiteSpace(dataContentEncoding))
                //     {
                //         result = false;
                //         errors.Add("Optional field 'datacontentencoding' must contain a value.");
                //     }
                // }

                // [data]
                // Optional, non-empty string
                if (containsData)
                {
                    if (data == null)
                    {
                        result = false;
                        errors.Add("Optional field 'data' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(data))
                    {
                        result = false;
                        errors.Add("Optional field 'data' must contain a value.");
                    }
                }

                return Tuple.Create<bool, IReadOnlyList<string>>(result, errors);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                errors.Add($"Exception while validating: {ex}");
                return Tuple.Create<bool, IReadOnlyList<string>>(false, errors);
            }
        }

        /// <summary>
        /// Deserializes a JSON string into a cloud event.
        /// </summary>
        /// <param name="json">The JSON to deserialize.</param>
        /// <returns>A cloud event.</returns>
        public static CloudEventV0_2 Deserialize(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentOutOfRangeException(nameof(json), Properties.Resources.NullOrEmptyStringExceptionMessage);
            }

            using (var sr = new StringReader(json))
            using (var jr = new JsonTextReader(sr))
            {
                var jobj = JObject.Parse(json);
                return Deserialize(jobj);
            }
        }

        /// <summary>
        /// Deserializes a JObject into a cloud event.
        /// </summary>
        /// <param name="jobj">The JObject to deserialize.</param>
        /// <returns>A cloud event.</returns>
        public static CloudEventV0_2 Deserialize(JObject jobj)
        {
            if (jobj == null)
            {
                throw new ArgumentNullException(nameof(jobj));
            }

            Type actualType = GetEventType(jobj);
            var cloudEvent = Activator.CreateInstance(actualType) as CloudEventV0_2;
            JsonConvert.PopulateObject(jobj.ToString(), cloudEvent);
            return cloudEvent;
        }

        /// <summary>
        /// Creates a generic cloud event with minimal information.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <returns>A generic cloud event.</returns>
        public static CloudEventV0_2 CreateGenericCloudEvent(string eventType, Uri source)
        {
            // Should there be some reasonable upper bound on the payload size?
            return new CloudEventV0_2
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow,
                Type = eventType,
                Source = source,
            };
        }

        /// <summary>
        /// Creates a cloud event with a JSON payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The JSON payload.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEventV0_2 CreateCloudEvent(string eventType, Uri source, JToken payload) =>
            CreateCloudEvent(eventType, source, payload, JsonMimeType, null);

        /// <summary>
        /// Creates a cloud event with a JSON payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The JSON payload.</param>
        /// <param name="schemaUrl">The schema URL for this cloud event.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEventV0_2 CreateCloudEvent(string eventType, Uri source, JToken payload, Uri schemaUrl) =>
            CreateCloudEvent(eventType, source, payload, JsonMimeType, schemaUrl);

        /// <summary>
        /// Creates a cloud event with a JSON payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The JSON payload.</param>
        /// <param name="contentType">The content type of the JSON payload.</param>
        /// <param name="schemaUrl">The schema URL for this cloud event.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEventV0_2 CreateCloudEvent(string eventType, Uri source, JToken payload, string contentType, Uri schemaUrl)
        {
            // Should there be some reasonable upper bound on the payload size?
            return new JsonCloudEventV0_2
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow,
                Type = eventType,
                Source = source,
                SchemaUrl = schemaUrl,
                ContentType = contentType,
                Data = payload,
            };
        }

        /// <summary>
        /// Creates a cloud event with a string payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The string payload.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEventV0_2 CreateCloudEvent(string eventType, Uri source, string payload) =>
            CreateCloudEvent(eventType, source, payload, PlainTextMimeType, null);

        /// <summary>
        /// Creates a cloud event with a string payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The string payload.</param>
        /// <param name="schemaUrl">The schema URL for this cloud event.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEventV0_2 CreateCloudEvent(string eventType, Uri source, string payload, Uri schemaUrl) =>
            CreateCloudEvent(eventType, source, payload, PlainTextMimeType, schemaUrl);

        /// <summary>
        /// Creates a cloud event with a string payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The string payload.</param>
        /// <param name="contentType">The content type of the JSON payload.</param>
        /// <param name="schemaUrl">The schema URL for this cloud event.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEventV0_2 CreateCloudEvent(string eventType, Uri source, string payload, string contentType, Uri schemaUrl)
        {
            // Should there be some reasonable upper bound on the payload size?
            return new StringCloudEventV0_2
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow,
                Type = eventType,
                Source = source,
                SchemaUrl = schemaUrl,
                ContentType = contentType,
                Data = payload,
            };
        }

        /// <summary>
        /// Creates a cloud event with a binary payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The binary payload.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEventV0_2 CreateCloudEvent(string eventType, Uri source, byte[] payload) =>
            CreateCloudEvent(eventType, source, payload, OctetStreamMimeType, null);

        /// <summary>
        /// Creates a cloud event with a binary payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The binary payload.</param>
        /// <param name="schemaUrl">The schema URL for this cloud event.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEventV0_2 CreateCloudEvent(string eventType, Uri source, byte[] payload, Uri schemaUrl) =>
            CreateCloudEvent(eventType, source, payload, OctetStreamMimeType, schemaUrl);

        /// <summary>
        /// Creates a cloud event with a binary payload.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="payload">The binary payload.</param>
        /// <param name="contentType">The content type of the binary payload.</param>
        /// <param name="schemaUrl">The schema URL for this cloud event.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEventV0_2 CreateCloudEvent(string eventType, Uri source, byte[] payload, string contentType, Uri schemaUrl)
        {
            // Should there be some reasonable upper bound on the payload size?
            return new BinaryCloudEventV0_2
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow,
                Type = eventType,
                Source = source,
                SchemaUrl = schemaUrl,
                ContentType = contentType,
                Data = payload,
            };
        }

        /// <summary>
        /// Gets the type of a raw JSON cloud event.
        /// </summary>
        /// <param name="jobj">The <see cref="JObject"/> payload.</param>
        /// <returns>The <see cref="Type"/> of the event.</returns>
        internal static Type GetEventType(JObject jobj)
        {
            if (jobj == null)
            {
                throw new ArgumentNullException(nameof(jobj));
            }

            if (jobj.ContainsKey("data"))
            {
                var contentType = jobj.Value<string>("contenttype")?.Trim();

                // SPEC: Section 3.1 - Paragraph 3
                // https://github.com/cloudevents/spec/blob/v0.1/json-format.md#31-special-handling-of-the-data-attribute
                if (contentType != null && (string.Equals(contentType, "application/json", StringComparison.OrdinalIgnoreCase) || contentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)))
                {
                    return typeof(JsonCloudEventV0_2);
                }
                else if (jobj.ContainsKey("data"))
                {
                    var data = jobj["data"]?.ToString();
                    if (base64Regex.IsMatch(data))
                    {
                        return typeof(BinaryCloudEventV0_2);
                    }
                    else
                    {
                        return typeof(StringCloudEventV0_2);
                    }
                }
            }

            return typeof(CloudEventV0_2);
        }
    }
}
