// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

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
    [JsonConverter(typeof(CloudEventJsonConverter))]
    public class CloudEvent
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
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the type of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("type", Order = int.MinValue + 1)]
        public string? Type { get; set; }

#pragma warning disable CA1822 // Mark members as static
        /// <summary>
        /// Gets the specification version of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("specversion", Order = int.MinValue + 2)]
        public string SpecVersion => "1.0";
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        /// Gets or sets the source of this cloud event. Required.
        /// </summary>
        [JsonRequired]
        [JsonProperty("source", Order = int.MinValue + 3)]
        public Uri? Source { get; set; }

        /// <summary>
        /// Gets or sets the time of this cloud event serialized according to RFC 3339. Optional.
        /// </summary>
        [JsonConverter(typeof(IsoDateTimeConverter))]
        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, Order = int.MinValue + 4)]
        public DateTimeOffset? Time { get; set; }

        /// <summary>
        /// Gets or sets the schema url of this cloud event. Optional.
        /// </summary>
        [JsonProperty("dataschema", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, Order = int.MinValue + 5)]
        public Uri? DataSchema { get; set; }

        /// <summary>
        /// Gets or sets the subject of this cloud event. Optional.
        /// </summary>
        [JsonProperty("subject", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, Order = int.MinValue + 6)]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the content type of this cloud event. Optional.
        /// </summary>
        [JsonProperty("datacontenttype", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, Order = int.MinValue + 6)]
        public string? DataContentType { get; set; }

        /// <summary>
        /// Gets or sets the undefined extension attributes. Optional.
        /// </summary>
        [JsonExtensionData]
#pragma warning disable CA2227 // Collection properties should be read only
        public Dictionary<string, JToken>? ExtensionAttributes { get; set; }
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

                var containsSubject = jobj.ContainsKey("subject");
                var containsTime = jobj.ContainsKey("time");
                var containsSchemaUrl = jobj.ContainsKey("dataschema");
                var containsData = jobj.ContainsKey("data");
                var containsContentType = jobj.ContainsKey("datacontenttype");

                // var containsDataContentEncoding = jobj.ContainsKey("datacontentencoding");
                var id = jobj["id"]?.ToString();
                var type = jobj["type"]?.ToString();
                var specVersion = jobj["specversion"]?.ToString();
                var source = jobj["source"]?.ToString();

                var subject = jobj["subject"]?.ToString();
                var time = jobj["time"]?.ToString();
                var dataSchema = jobj["dataschema"]?.ToString();
                var data = jobj["data"]?.ToString();
                var dataContentType = jobj["datacontenttype"]?.ToString();

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
                // Required, non-empty string set to 1.0
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
                else if (string.Equals(specVersion, "1.0", StringComparison.OrdinalIgnoreCase) == false)
                {
                    result = false;
                    errors.Add("Required field 'specversion' must contain the value '1.0'");
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

                // [subject]
                // Optional, non-empty string
                if (containsSubject)
                {
                    if (subject == null)
                    {
                        result = false;
                        errors.Add("Optional field 'subject' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(subject))
                    {
                        result = false;
                        errors.Add("Optional field 'subject' is present and therefore must contain a value.");
                    }
                }

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
                        errors.Add("Optional field 'time' is present and therefore must contain a value.");
                    }
                    else if (rfc3339Regex.IsMatch(time) == false)
                    {
                        result = false;
                        errors.Add("Optional field 'time' must adhere to the format specified in RFC 3339.");
                    }
                }

                // [dataschema]
                // Optional, non-null Uri
                if (containsSchemaUrl)
                {
                    if (dataSchema == null)
                    {
                        result = false;
                        errors.Add("Optional field 'dataschema' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(dataSchema))
                    {
                        result = false;
                        errors.Add("Optional field 'dataschema' is present and therefore must contain a value.");
                    }
                    else if (Uri.TryCreate(dataSchema, UriKind.RelativeOrAbsolute, out Uri schemaUri) == false)
                    {
                        result = false;
                        errors.Add("Optional field 'dataschema' must contain a valid Uri.");
                    }
                }

                // [datacontenttype]
                // Optional, non-empty string
                if (containsContentType)
                {
                    if (dataContentType == null)
                    {
                        result = false;
                        errors.Add("Optional field 'datacontenttype' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(dataContentType))
                    {
                        result = false;
                        errors.Add("Optional field 'datacontenttype' must contain a value.");
                    }
                    else if (rfc2046Regex.IsMatch(dataContentType) == false)
                    {
                        result = false;
                        errors.Add("Optional field 'datacontenttype' must adhere to the format specified in RFC 2046.");
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
        public static CloudEvent Deserialize(string json)
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
        public static CloudEvent Deserialize(JObject jobj)
        {
            if (jobj == null)
            {
                throw new ArgumentNullException(nameof(jobj));
            }

            Type actualType = GetEventType(jobj);
            var cloudEvent = (CloudEvent)Activator.CreateInstance(actualType);
            JsonConvert.PopulateObject(jobj.ToString(), cloudEvent);
            return cloudEvent;
        }

        /// <summary>
        /// Creates a generic cloud event with minimal information.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <returns>A generic cloud event.</returns>
        public static CloudEvent CreateGenericCloudEvent(string eventType, Uri source)
        {
            // Should there be some reasonable upper bound on the data size?
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow,
                Type = eventType,
                Source = source,
            };
        }

        /// <summary>
        /// Creates a cloud event with a JSON data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The JSON data.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEvent CreateCloudEvent(string eventType, Uri source, JToken data) =>
            CreateCloudEvent(eventType, source, data, JsonMimeType, null);

        /// <summary>
        /// Creates a cloud event with a JSON data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The JSON data.</param>
        /// <param name="dataSchema">The schema URL for this cloud event.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEvent CreateCloudEvent(string eventType, Uri source, JToken data, Uri? dataSchema) =>
            CreateCloudEvent(eventType, source, data, JsonMimeType, dataSchema);

        /// <summary>
        /// Creates a cloud event with a JSON data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The JSON data.</param>
        /// <param name="dataContentType">The content type of the JSON data.</param>
        /// <param name="dataSchema">The schema URL for this cloud event.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEvent CreateCloudEvent(string eventType, Uri source, JToken data, string? dataContentType, Uri? dataSchema)
        {
            // Should there be some reasonable upper bound on the data size?
            return new JsonCloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow,
                Type = eventType,
                Source = source,
                DataSchema = dataSchema,
                DataContentType = dataContentType,
                Data = data,
            };
        }

        /// <summary>
        /// Creates a cloud event with a string data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The string data.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEvent CreateCloudEvent(string eventType, Uri source, string data) =>
            CreateCloudEvent(eventType, source, data, PlainTextMimeType, null);

        /// <summary>
        /// Creates a cloud event with a string data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The string data.</param>
        /// <param name="dataSchema">The schema URL for this cloud event.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEvent CreateCloudEvent(string eventType, Uri source, string data, Uri? dataSchema) =>
            CreateCloudEvent(eventType, source, data, PlainTextMimeType, dataSchema);

        /// <summary>
        /// Creates a cloud event with a string data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The string data.</param>
        /// <param name="dataContentType">The content type of the JSON data.</param>
        /// <param name="dataSchema">The schema URL for this cloud event.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEvent CreateCloudEvent(string eventType, Uri source, string data, string? dataContentType, Uri? dataSchema)
        {
            // Should there be some reasonable upper bound on the data size?
            return new StringCloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow,
                Type = eventType,
                Source = source,
                DataSchema = dataSchema,
                DataContentType = dataContentType,
                Data = data,
            };
        }

        /// <summary>
        /// Creates a cloud event with a binary data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The binary data.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEvent CreateCloudEvent(string eventType, Uri source, byte[]? data) =>
            CreateCloudEvent(eventType, source, data, OctetStreamMimeType, null);

        /// <summary>
        /// Creates a cloud event with a binary data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The binary data.</param>
        /// <param name="dataSchema">The schema URL for this cloud event.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEvent CreateCloudEvent(string eventType, Uri source, byte[]? data, Uri? dataSchema) =>
            CreateCloudEvent(eventType, source, data, OctetStreamMimeType, dataSchema);

        /// <summary>
        /// Creates a cloud event with a binary data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The binary data.</param>
        /// <param name="dataContentType">The content type of the binary data.</param>
        /// <param name="dataSchema">The schema URL for this cloud event.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEvent CreateCloudEvent(string eventType, Uri source, byte[]? data, string? dataContentType, Uri? dataSchema)
        {
            // Should there be some reasonable upper bound on the data size?
            return new BinaryCloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTimeOffset.UtcNow,
                Type = eventType,
                Source = source,
                DataSchema = dataSchema,
                DataContentType = dataContentType,
                Data = data,
            };
        }

        /// <summary>
        /// Gets the type of a raw JSON cloud event.
        /// </summary>
        /// <param name="jobj">The <see cref="JObject"/> data.</param>
        /// <returns>The <see cref="Type"/> of the event.</returns>
        internal static Type GetEventType(JObject jobj)
        {
            if (jobj == null)
            {
                throw new ArgumentNullException(nameof(jobj));
            }

            if (jobj.ContainsKey("data"))
            {
                var dataContentType = jobj.Value<string>("datacontenttype")?.Trim();

                // SPEC: Section 3.1 - Paragraph 3
                // https://github.com/cloudevents/spec/blob/v0.1/json-format.md#31-special-handling-of-the-data-attribute
                if (dataContentType != null && (string.Equals(dataContentType, "application/json", StringComparison.OrdinalIgnoreCase) || dataContentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)))
                {
                    return typeof(JsonCloudEvent);
                }
                else if (jobj.ContainsKey("data"))
                {
                    var data = jobj["data"]?.ToString();
                    if (base64Regex.IsMatch(data))
                    {
                        return typeof(BinaryCloudEvent);
                    }
                    else
                    {
                        return typeof(StringCloudEvent);
                    }
                }
            }

            return typeof(CloudEvent);
        }
    }
}
