// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

#nullable disable
namespace Rixian.CloudEvents.Tests.V10
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class DeserializeTests
    {
        [Theory]
        [InlineData("json1.json")]
        [InlineData("json2.json")]
        public void TestJsonFiles(string fileName)
        {
            var json = File.ReadAllText($@"./V10Tests/samples/json/{fileName}");
            var evnt = CloudEvent.Deserialize(json);

            evnt.Should().BeOfType<JsonCloudEvent>();
        }

        [Fact]
        public void Json_V02_File01()
        {
            var json = File.ReadAllText($@"./V02Tests/samples/json/json1.json");
            CloudEvent evnt = CloudEvent.Deserialize(json);

            evnt.Should().BeOfType<JsonCloudEvent>();
            evnt.DataContentType.Should().Be("application/json");
            evnt.DataSchema.Should().BeNull();
            evnt.Id.Should().Be("C234-1234-1234");
            evnt.Source.Should().Be("/mycontext");
            evnt.Subject.Should().BeNull();
            evnt.Time.Should().Be(new DateTimeOffset(2018, 04, 05, 17, 31, 00, TimeSpan.Zero));
            evnt.Type.Should().Be("com.example.someevent");

            JsonCloudEvent jsonEvnt = (JsonCloudEvent)evnt;
            jsonEvnt.Data.Should().BeAssignableTo<JObject>();
            JObject data = (JObject)jsonEvnt.Data;

            data.Should().ContainKey("appinfoA");
            data["appinfoA"].Value<string>().Should().Be("abc");

            data.Should().ContainKey("appinfoB");
            data["appinfoB"].Value<int>().Should().Be(123);

            data.Should().ContainKey("appinfoC");
            data["appinfoC"].Value<bool>().Should().Be(true);
        }

        [Fact]
        public void Json_V01_File01()
        {
            var json = File.ReadAllText($@"./V01Tests/samples/json/json1.json");
            CloudEvent evnt = CloudEvent.Deserialize(json);

            evnt.Should().BeOfType<JsonCloudEvent>();
            evnt.DataContentType.Should().Be("application/json");
            evnt.DataSchema.Should().BeNull();
            evnt.Id.Should().Be("C234-1234-1234");
            evnt.Source.Should().Be("/mycontext");
            evnt.Subject.Should().BeNull();
            evnt.Time.Should().Be(new DateTimeOffset(2018, 04, 05, 17, 31, 00, TimeSpan.Zero));
            evnt.Type.Should().Be("com.example.someevent");

            evnt.ExtensionAttributes.Should().ContainKey("eventTypeVersion");
            evnt.ExtensionAttributes["eventTypeVersion"].Value<string>().Should().Be("1.0");

            evnt.ExtensionAttributes.Should().ContainKey("comExampleExtension");
            evnt.ExtensionAttributes["comExampleExtension"].Value<string>().Should().Be("value");

            JsonCloudEvent jsonEvnt = (JsonCloudEvent)evnt;
            jsonEvnt.Data.Should().BeAssignableTo<JObject>();
            JObject data = (JObject)jsonEvnt.Data;

            data.Should().ContainKey("appinfoA");
            data["appinfoA"].Value<string>().Should().Be("abc");

            data.Should().ContainKey("appinfoB");
            data["appinfoB"].Value<int>().Should().Be(123);

            data.Should().ContainKey("appinfoC");
            data["appinfoC"].Value<bool>().Should().Be(true);
        }

        [Fact]
        public void Json_V02_File02()
        {
            var json = File.ReadAllText($@"./V02Tests/samples/json/json2.json");
            CloudEvent evnt = CloudEvent.Deserialize(json);

            evnt.Should().BeOfType<JsonCloudEvent>();
            evnt.DataContentType.Should().Be("application/json");
            evnt.DataSchema.Should().BeNull();
            evnt.Id.Should().Be("C234-1234-1234");
            evnt.Source.Should().Be("/mycontext");
            evnt.Subject.Should().BeNull();
            evnt.Time.Should().Be(new DateTimeOffset(2018, 04, 05, 17, 31, 00, TimeSpan.Zero));
            evnt.Type.Should().Be("com.example.someevent");

            JsonCloudEvent jsonEvnt = (JsonCloudEvent)evnt;
            jsonEvnt.Data.Should().BeAssignableTo<JArray>();
            JArray data = (JArray)jsonEvnt.Data;

            data.Should().HaveCount(6);
        }

        [Fact]
        public void Json_V01_File02()
        {
            var json = File.ReadAllText($@"./V01Tests/samples/json/json2.json");
            CloudEvent evnt = CloudEvent.Deserialize(json);

            evnt.Should().BeOfType<JsonCloudEvent>();
            evnt.DataContentType.Should().Be("application/json");
            evnt.DataSchema.Should().BeNull();
            evnt.Id.Should().Be("C234-1234-1234");
            evnt.Source.Should().Be("/mycontext");
            evnt.Subject.Should().BeNull();
            evnt.Time.Should().Be(new DateTimeOffset(2018, 04, 05, 17, 31, 00, TimeSpan.Zero));
            evnt.Type.Should().Be("com.example.someevent");

            evnt.ExtensionAttributes.Should().ContainKey("eventTypeVersion");
            evnt.ExtensionAttributes["eventTypeVersion"].Value<string>().Should().Be("1.0");

            evnt.ExtensionAttributes.Should().ContainKey("comExampleExtension");
            evnt.ExtensionAttributes["comExampleExtension"].Value<string>().Should().Be("value");

            JsonCloudEvent jsonEvnt = (JsonCloudEvent)evnt;
            jsonEvnt.Data.Should().BeAssignableTo<JArray>();
            JArray data = (JArray)jsonEvnt.Data;

            data.Should().HaveCount(6);
        }

        [Theory]
        [InlineData("string1.json")]
        public void TestStringFilesV02(string fileName)
        {
            var json = File.ReadAllText($@"./V10Tests/samples/string/{fileName}");
            var evnt = CloudEvent.Deserialize(json);

            evnt.Should().BeOfType<StringCloudEvent>();
        }

        [Theory]
        [InlineData("none1.json")]
        public void TestNoDataFiles(string fileName)
        {
            var json = File.ReadAllText($@"./V10Tests/samples/none/{fileName}");
            var evnt = CloudEvent.Deserialize(json);

            evnt.Should().BeOfType<CloudEvent>();
            evnt.Should().NotBeOfType<JsonCloudEvent>();
            evnt.Should().NotBeOfType<BinaryCloudEvent>();
            evnt.Should().NotBeOfType<StringCloudEvent>();
        }

        [Theory]
        [InlineData("custom1.json")]
        public void CustomEvent(string fileName)
        {
            var json = File.ReadAllText($@"./V10Tests/samples/custom/{fileName}");
            TestCloudEvent evnt = JsonConvert.DeserializeObject<TestCloudEvent>(json);

            evnt.Should().NotBeNull();
            evnt.Should().BeOfType<TestCloudEvent>();
        }

        [Theory]
        [InlineData("AAAAAA")]
        public void BinaryEvent_ContainsData_Success(string data)
        {
            BinaryCloudEvent evnt = CloudEvent.CreateCloudEvent("test", new Uri("/", UriKind.RelativeOrAbsolute), Encoding.UTF8.GetBytes(data));

            evnt.Should().NotBeNull();
            evnt.Should().BeOfType<BinaryCloudEvent>();

            var jobj = JObject.FromObject(evnt);

            // Can explicitly deserialize to binary
            BinaryCloudEvent evnt2 = jobj.ToObject<BinaryCloudEvent>();
            evnt2.Should().NotBeNull();
            evnt2.Data.Should().NotBeNull();

            // Without a type provided this should deserialize to a binary event
            var evnt3 = CloudEvent.Deserialize(jobj.ToString());
            evnt3.Should().NotBeNull();
            evnt3.Should().BeOfType<BinaryCloudEvent>();

            CloudEvent evnt4 = JsonConvert.DeserializeObject<CloudEvent>(jobj.ToString());
            evnt4.Should().NotBeNull();
            evnt4.Should().BeOfType<BinaryCloudEvent>();
        }

        [Fact]
        public void BinaryEvent_NoData_Success()
        {
            BinaryCloudEvent evnt = CloudEvent.CreateCloudEvent("test", new Uri("/", UriKind.RelativeOrAbsolute), (byte[])null);

            evnt.Should().NotBeNull();
            evnt.Should().BeOfType<BinaryCloudEvent>();

            var jobj = JObject.FromObject(evnt);

            // Can explicitly deserialize to binary even without data present
            BinaryCloudEvent evnt2 = jobj.ToObject<BinaryCloudEvent>();
            evnt2.Should().NotBeNull();
            evnt2.Data.Should().BeNull();

            // Without a type provided this should deserialize to a generic event
            var evnt3 = CloudEvent.Deserialize(jobj.ToString());
            evnt3.Should().NotBeNull();
            evnt3.Should().BeOfType<CloudEvent>();

            CloudEvent evnt4 = JsonConvert.DeserializeObject<CloudEvent>(jobj.ToString());
            evnt4.Should().NotBeNull();
            evnt4.Should().BeOfType<CloudEvent>();
        }

        [Theory]
        [InlineData("custombinary1.json")]
        public void CustomBinaryEvent_Success(string fileName)
        {
            var json = File.ReadAllText($@"./V10Tests/samples/custom/{fileName}");
            TestBinaryEvent evnt = JsonConvert.DeserializeObject<TestBinaryEvent>(json);

            evnt.Should().NotBeNull();
            evnt.Should().BeOfType<TestBinaryEvent>();

            var dataString = Encoding.UTF8.GetString(evnt.Data);
            dataString.Should().Be("This is a test!");

            evnt.SampleField1.Should().Be("ABCDEFG");
        }

        [Theory]
        [InlineData("pdf1.pdf", "application/pdf")]
        public void BinaryEvent_LargeData_Success(string fileName, string dataContentType)
        {
            var data = File.ReadAllBytes($@"./V10Tests/samples/binary/{fileName}");
            BinaryCloudEvent evnt = CloudEvent.CreateCloudEvent("test", new Uri("/", UriKind.RelativeOrAbsolute), data, dataContentType, null);

            evnt.Should().NotBeNull();
            evnt.Should().BeOfType<BinaryCloudEvent>();
            evnt.DataContentType.Should().Be(dataContentType);

            evnt.Data.Length.Should().Be(data.Length);

            var json = JsonConvert.SerializeObject(evnt, Formatting.Indented);
        }
    }
}
