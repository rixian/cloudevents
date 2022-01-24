// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

namespace Rixian.CloudEvents
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Mime;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A basic CloudEvent.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1011:Closing square brackets should be spaced correctly", Justification = "Need to fix this for nullable array types.")]
    public class CloudEvent : ICloudEvent
    {
        private const string RFC3339RegexPattern = @"^([0-9]+)-(0[1-9]|1[012])-(0[1-9]|[12][0-9]|3[01])[Tt]([01][0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9]|60)(\.[0-9]+)?(([Zz])|([\+|\-]([01][0-9]|2[0-3]):[0-5][0-9]))$";

        private const string RFC2046RegexPattern = @"[a-zA-Z0-9!#$%^&\\*_\\-\\+{}\\|'.`~]+/[a-zA-Z0-9!#$%^&\\*_\\-\\+{}\\|'.`~]+";

        // See: https://stackoverflow.com/a/475217
        private const string Base64RegexPattern = @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$";

        private static Regex rfc3339Regex = new Regex(RFC3339RegexPattern);
        private static Regex rfc2046Regex = new Regex(RFC2046RegexPattern);
        private static Regex base64Regex = new Regex(Base64RegexPattern);

        /// <summary>
        /// Gets or sets the unique id of this cloud event. Required.
        /// </summary>
        // *** [JsonRequired]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the type of this cloud event. Required.
        /// </summary>
        // *** [JsonRequired]
        [JsonPropertyName("type")]
        public string? Type { get; set; }

#pragma warning disable CA1822 // Mark members as static
        /// <summary>
        /// Gets the specification version of this cloud event. Required.
        /// </summary>
        // *** [JsonRequired]
        [JsonPropertyName("specversion")]
        public string SpecVersion => "1.0";
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        /// Gets or sets the source of this cloud event. Required.
        /// </summary>
        // *** [JsonRequired]
        [JsonPropertyName("source")]
        public Uri? Source { get; set; }

        /// <summary>
        /// Gets or sets the time of this cloud event serialized according to RFC 3339. Optional.
        /// </summary>
        [JsonPropertyName("time")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTimeOffset? Time { get; set; }

        /// <summary>
        /// Gets or sets the schema url of this cloud event. Optional.
        /// </summary>
        [JsonPropertyName("dataschema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public Uri? DataSchema { get; set; }

        /// <summary>
        /// Gets or sets the subject of this cloud event. Optional.
        /// </summary>
        [JsonPropertyName("subject")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the content type of this cloud event. Optional.
        /// </summary>
        [JsonPropertyName("datacontenttype")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? DataContentType { get; set; }

        /// <summary>
        /// Gets or sets the undefined extension attributes. Optional.
        /// </summary>
        [JsonExtensionData]
#pragma warning disable CA2227 // Collection properties should be read only
        public IDictionary<string, JsonElement>? ExtensionAttributes { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Validates that a given JSON string is a cloud event.
        /// </summary>
        /// <param name="json">The JSON to validate.</param>
        /// <returns>True if the string is a valid cloud event, otherwise false.</returns>
        public static bool ValidateJson(string json) => ValidateJsonDetailed(json).Item1;

        /// <summary>
        /// Validates that a given JsonElement is a cloud event.
        /// </summary>
        /// <param name="jobj">The JsonElement to validate.</param>
        /// <returns>True if the string is a valid cloud event, otherwise false.</returns>
        public static bool ValidateJson(JsonElement jobj) => ValidateJsonDetailed(jobj).Item1;

        /// <summary>
        /// Validates that a given JSON string is a cloud event.
        /// </summary>
        /// <param name="json">The JSON to validate.</param>
        /// <returns>A tuple containing a flag indicating if the JSON string is a valid cloud event, and a list of all errors.</returns>
        public static Tuple<bool, IReadOnlyList<string>> ValidateJsonDetailed(string json)
        {
            try
            {
                return ValidateJsonDetailed(JsonDocument.Parse(json).RootElement);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return Tuple.Create<bool, IReadOnlyList<string>>(false, new[] { "Failed to parse json." });
            }
        }

        /// <summary>
        /// Validates that a given JsonElement is a cloud event.
        /// </summary>
        /// <param name="jobj">The JsonElement to validate.</param>
        /// <returns>A tuple containing a flag indicating if the JsonElement is a valid cloud event, and a list of all errors.</returns>
        public static Tuple<bool, IReadOnlyList<string>> ValidateJsonDetailed(JsonElement jobj)
        {
            var errors = new List<string>();
            try
            {
                bool result = true;

                var containsId = jobj.TryGetProperty("id", out JsonElement idProp);
                var containsType = jobj.TryGetProperty("type", out JsonElement typeProp);
                var containsSpecVersion = jobj.TryGetProperty("specversion", out JsonElement specVersionProp);
                var containsSource = jobj.TryGetProperty("source", out JsonElement sourceProp);

                var containsSubject = jobj.TryGetProperty("subject", out JsonElement subjectProp);
                var containsTime = jobj.TryGetProperty("time", out JsonElement timeProp);
                var containsDataSchema = jobj.TryGetProperty("dataschema", out JsonElement dataSchemaProp);
                var containsData = jobj.TryGetProperty("data", out JsonElement dataProp);
                var containsDataContentType = jobj.TryGetProperty("datacontenttype", out JsonElement dataContentTypeProp);

                // [id]
                // Required, non-empty string
                if (!containsId)
                {
                    result = false;
                    errors.Add("Required field 'id' is missing.");
                }
                else if (idProp.ValueKind == JsonValueKind.Null)
                {
                    result = false;
                    errors.Add("Required field 'id' is null.");
                }
                else if (string.IsNullOrWhiteSpace(idProp.GetString()))
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
                else if (typeProp.ValueKind == JsonValueKind.Null)
                {
                    result = false;
                    errors.Add("Required field 'type' is null.");
                }
                else if (string.IsNullOrWhiteSpace(typeProp.GetString()))
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
                else if (specVersionProp.ValueKind == JsonValueKind.Null)
                {
                    result = false;
                    errors.Add("Required field 'specversion' is null.");
                }
                else if (string.IsNullOrWhiteSpace(specVersionProp.GetString()))
                {
                    result = false;
                    errors.Add("Required field 'specversion' must contain a value.");
                }
                else if (string.Equals(specVersionProp.GetString(), "1.0", StringComparison.OrdinalIgnoreCase) == false)
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
                else if (sourceProp.ValueKind == JsonValueKind.Null)
                {
                    result = false;
                    errors.Add("Required field 'source' is null.");
                }
                else if (string.IsNullOrWhiteSpace(sourceProp.GetString()))
                {
                    result = false;
                    errors.Add("Required field 'source' must contain a value.");
                }
                else if (Uri.TryCreate(sourceProp.GetString(), UriKind.RelativeOrAbsolute, out Uri? sourceUri) == false)
                {
                    result = false;
                    errors.Add("Required field 'source' must contain a valid Uri.");
                }

                // [subject]
                // Optional, non-empty string
                if (containsSubject)
                {
                    if (subjectProp.ValueKind == JsonValueKind.Null)
                    {
                        result = false;
                        errors.Add("Optional field 'subject' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(subjectProp.GetString()))
                    {
                        result = false;
                        errors.Add("Optional field 'subject' is present and therefore must contain a value.");
                    }
                }

                // [time]
                // Optional, non-empty string
                if (containsTime)
                {
                    if (timeProp.ValueKind == JsonValueKind.Null)
                    {
                        result = false;
                        errors.Add("Optional field 'time' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(timeProp.GetString()))
                    {
                        result = false;
                        errors.Add("Optional field 'time' is present and therefore must contain a value.");
                    }
                    else if (rfc3339Regex.IsMatch(timeProp.GetString() !) == false)
                    {
                        result = false;
                        errors.Add("Optional field 'time' must adhere to the format specified in RFC 3339.");
                    }
                }

                // [dataschema]
                // Optional, non-null Uri
                if (containsDataSchema)
                {
                    if (dataSchemaProp.ValueKind == JsonValueKind.Null)
                    {
                        result = false;
                        errors.Add("Optional field 'dataschema' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(dataSchemaProp.GetString()))
                    {
                        result = false;
                        errors.Add("Optional field 'dataschema' is present and therefore must contain a value.");
                    }
                    else if (Uri.TryCreate(dataSchemaProp.GetString(), UriKind.RelativeOrAbsolute, out Uri? schemaUri) == false)
                    {
                        result = false;
                        errors.Add("Optional field 'dataschema' must contain a valid Uri.");
                    }
                }

                // [datacontenttype]
                // Optional, non-empty string
                if (containsDataContentType)
                {
                    if (dataContentTypeProp.ValueKind == JsonValueKind.Null)
                    {
                        result = false;
                        errors.Add("Optional field 'datacontenttype' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(dataContentTypeProp.GetString()))
                    {
                        result = false;
                        errors.Add("Optional field 'datacontenttype' must contain a value.");
                    }
                    else if (rfc2046Regex.IsMatch(dataContentTypeProp.GetString() !) == false)
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
                    if (dataProp.ValueKind == JsonValueKind.Null)
                    {
                        result = false;
                        errors.Add("Optional field 'data' is null.");
                    }
                    else if (string.IsNullOrWhiteSpace(dataProp.GetString()))
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
        public static CloudEvent? Deserialize(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentOutOfRangeException(nameof(json), Properties.Resources.NullOrEmptyStringExceptionMessage);
            }

            var doc = JsonDocument.Parse(json);
            return Deserialize(doc.RootElement);
        }

        /// <summary>
        /// Deserializes a JsonElement into a cloud event.
        /// </summary>
        /// <param name="jobj">The JsonElement to deserialize.</param>
        /// <returns>A cloud event.</returns>
        public static CloudEvent? Deserialize(JsonElement jobj)
        {
            if (jobj.TryGetProperty("specversion", out JsonElement specversion))
            {
                if (string.Equals(specversion.ToString(), "1.0", StringComparison.OrdinalIgnoreCase))
                {
                    return DeserializeLatest(jobj);
                }
            }

            // Unknown spec version. Attempting to deserialize to the latest.
            return DeserializeLatest(jobj);
        }

        /// <summary>
        /// Deserializes a JSON string into a cloud event.
        /// </summary>
        /// <param name="json">The JSON to deserialize.</param>
        /// <returns>A cloud event.</returns>
        public static ICloudEvent? DeserializeAny(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentOutOfRangeException(nameof(json), Properties.Resources.NullOrEmptyStringExceptionMessage);
            }

            var doc = JsonDocument.Parse(json);
            return DeserializeAny(doc.RootElement);
        }

        /// <summary>
        /// Deserializes a JsonElement into a cloud event.
        /// </summary>
        /// <param name="jobj">The JsonElement to deserialize.</param>
        /// <returns>A cloud event.</returns>
        public static ICloudEvent? DeserializeAny(JsonElement jobj)
        {
            if (jobj.TryGetProperty("specversion", out JsonElement specversion))
            {
                if (string.Equals(specversion.ToString(), "1.0", StringComparison.OrdinalIgnoreCase))
                {
                    return DeserializeLatest(jobj);
                }
            }

            // Unknown spec version. Attempting to deserialize to the latest.
            return DeserializeLatest(jobj);
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
        public static JsonCloudEvent CreateCloudEvent(string eventType, Uri source, JsonElement data) =>
            CreateCloudEvent(eventType, source, data, MediaTypeNames.Application.Json, null);

        /// <summary>
        /// Creates a cloud event with a JSON data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The JSON data.</param>
        /// <param name="dataSchema">The schema URL for this cloud event.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEvent CreateCloudEvent(string eventType, Uri source, JsonElement data, Uri? dataSchema) =>
            CreateCloudEvent(eventType, source, data, MediaTypeNames.Application.Json, dataSchema);

        /// <summary>
        /// Creates a cloud event with a JSON data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The JSON data.</param>
        /// <param name="dataContentType">The content type of the JSON data.</param>
        /// <param name="dataSchema">The schema URL for this cloud event.</param>
        /// <returns>A JSON cloud event.</returns>
        public static JsonCloudEvent CreateCloudEvent(string eventType, Uri source, JsonElement data, string? dataContentType, Uri? dataSchema)
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
            CreateCloudEvent(eventType, source, data, MediaTypeNames.Text.Plain, null);

        /// <summary>
        /// Creates a cloud event with a string data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The string data.</param>
        /// <param name="dataSchema">The schema URL for this cloud event.</param>
        /// <returns>A string cloud event.</returns>
        public static StringCloudEvent CreateCloudEvent(string eventType, Uri source, string data, Uri? dataSchema) =>
            CreateCloudEvent(eventType, source, data, MediaTypeNames.Text.Plain, dataSchema);

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
            CreateCloudEvent(eventType, source, data, MediaTypeNames.Application.Octet, null);

        /// <summary>
        /// Creates a cloud event with a binary data.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="source">The event source.</param>
        /// <param name="data">The binary data.</param>
        /// <param name="dataSchema">The schema URL for this cloud event.</param>
        /// <returns>A binary cloud event.</returns>
        public static BinaryCloudEvent CreateCloudEvent(string eventType, Uri source, byte[]? data, Uri? dataSchema) =>
            CreateCloudEvent(eventType, source, data, MediaTypeNames.Application.Octet, dataSchema);

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
        /// <param name="jobj">The <see cref="JsonElement"/> data.</param>
        /// <returns>The <see cref="Type"/> of the event.</returns>
        internal static Type GetEventType(JsonElement jobj)
        {
            if (jobj.TryGetProperty("data", out JsonElement data))
            {
                var dataContentType = jobj.GetProperty("datacontenttype").GetString()?.Trim();

                // SPEC: Section 3.1 - Paragraph 3
                // https://github.com/cloudevents/spec/blob/v0.1/json-format.md#31-special-handling-of-the-data-attribute
                if (dataContentType != null && (string.Equals(dataContentType, "application/json", StringComparison.OrdinalIgnoreCase) || dataContentType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)))
                {
                    return typeof(JsonCloudEvent);
                }
                else
                {
                    var dataString = data.GetString();
                    if (base64Regex.IsMatch(dataString ?? string.Empty))
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

        private static CloudEvent? DeserializeLatest(JsonElement jobj)
        {
            Type actualType = GetEventType(jobj);

            var cloudEvent = JsonSerializer.Deserialize(jobj, actualType) as CloudEvent;
            return cloudEvent;
        }
    }
}
