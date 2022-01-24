// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

#nullable disable
#pragma warning disable CS0618 // Type or member is obsolete
namespace Rixian.CloudEvents.Tests.V02
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class JsonConverterTests
    {
        [Theory]
        [InlineData("json1.json")]
        [InlineData("json2.json")]
        public void TestJsonFiles(string fileName)
        {
            var json = File.ReadAllText($@"./V02Tests/samples/json/{fileName}");
            var evnt = CloudEventV0_2.Deserialize(json);

            CloudEventV0_2 newEvnt = JsonConvert.DeserializeObject<CloudEventV0_2>(JsonConvert.SerializeObject(evnt));

            newEvnt.Should().BeOfType<JsonCloudEventV0_2>();
        }

        [Theory]
        [InlineData("string1.json")]
        public void TestStringFilesV02(string fileName)
        {
            var json = File.ReadAllText($@"./V02Tests/samples/string/{fileName}");
            var evnt = CloudEventV0_2.Deserialize(json);

            var json2 = JsonConvert.SerializeObject(evnt);
            CloudEventV0_2 newEvnt = JsonConvert.DeserializeObject<CloudEventV0_2>(json2);

            newEvnt.Should().BeOfType<StringCloudEventV0_2>();
        }

        [Theory]
        [InlineData("none1.json")]
        public void TestNoDataFiles(string fileName)
        {
            var json = File.ReadAllText($@"./V02Tests/samples/none/{fileName}");
            var evnt = CloudEventV0_2.Deserialize(json);

            CloudEventV0_2 newEvnt = JsonConvert.DeserializeObject<CloudEventV0_2>(JsonConvert.SerializeObject(evnt));

            newEvnt.Should().BeOfType<CloudEventV0_2>();
            newEvnt.Should().NotBeOfType<JsonCloudEventV0_2>();
            newEvnt.Should().NotBeOfType<BinaryCloudEventV0_2>();
            newEvnt.Should().NotBeOfType<StringCloudEventV0_2>();
        }

        [Theory]
        [InlineData("custom1.json")]
        public void CustomEvent(string fileName)
        {
            var json = File.ReadAllText($@"./V02Tests/samples/custom/{fileName}");
            TestCloudEvent evnt = JsonConvert.DeserializeObject<TestCloudEvent>(json);

            evnt.Should().NotBeNull();
            evnt.Should().BeOfType<TestCloudEvent>();
        }

        [Theory]
        [InlineData("AAAAAA")]
        public void BinaryEvent_ContainsData_Success(string data)
        {
            BinaryCloudEventV0_2 evnt = CloudEventV0_2.CreateCloudEvent("test", new Uri("/", UriKind.RelativeOrAbsolute), Encoding.UTF8.GetBytes(data));
            CloudEventV0_2 newEvnt = JsonConvert.DeserializeObject<CloudEventV0_2>(JsonConvert.SerializeObject(evnt));

            newEvnt.Should().NotBeNull();
            newEvnt.Should().BeOfType<BinaryCloudEventV0_2>();

            var jobj = JObject.FromObject(newEvnt);

            // Can explicitly deserialize to binary
            BinaryCloudEventV0_2 evnt2 = jobj.ToObject<BinaryCloudEventV0_2>();
            evnt2.Should().NotBeNull();
            evnt2.Data.Should().NotBeNull();

            // Without a type provided this should deserialize to a binary event
            var evnt3 = CloudEventV0_2.Deserialize(jobj.ToString());
            evnt3.Should().NotBeNull();
            evnt3.Should().BeOfType<BinaryCloudEventV0_2>();
        }

        [Fact]
        public void BinaryEvent_NoData_Success()
        {
            BinaryCloudEventV0_2 evnt = CloudEventV0_2.CreateCloudEvent("test", new Uri("/", UriKind.RelativeOrAbsolute), (byte[])null);
            evnt.Should().NotBeNull();
            evnt.Should().BeOfType<BinaryCloudEventV0_2>();

            CloudEventV0_2 newEvnt = JsonConvert.DeserializeObject<CloudEventV0_2>(JsonConvert.SerializeObject(evnt));

            newEvnt.Should().NotBeNull();
            newEvnt.Should().BeOfType<CloudEventV0_2>();

            var jobj = JObject.FromObject(newEvnt);

            // Can explicitly deserialize to binary even without data present
            BinaryCloudEventV0_2 evnt2 = jobj.ToObject<BinaryCloudEventV0_2>();
            evnt2.Should().NotBeNull();
            evnt2.Data.Should().BeNull();

            // Without a type provided this should deserialize to a generic event
            var evnt3 = CloudEventV0_2.Deserialize(jobj.ToString());
            evnt3.Should().NotBeNull();
            evnt3.Should().BeOfType<CloudEventV0_2>();
        }

        [Theory]
        [InlineData("custombinary1.json")]
        public void CustomBinaryEvent_Success(string fileName)
        {
            var json = File.ReadAllText($@"./V02Tests/samples/custom/{fileName}");
            TestBinaryEvent evnt = JsonConvert.DeserializeObject<TestBinaryEvent>(json);

            evnt.Should().NotBeNull();
            evnt.Should().BeOfType<TestBinaryEvent>();

            var dataString = Encoding.UTF8.GetString(evnt.Data);
            dataString.Should().Be("This is a test!");

            evnt.SampleField1.Should().Be("ABCDEFG");
        }

        [Theory]
        [InlineData("pdf1.pdf", "application/pdf")]
        public void BinaryEvent_LargeData_Success(string fileName, string contentType)
        {
            var data = File.ReadAllBytes($@"./V02Tests/samples/binary/{fileName}");
            BinaryCloudEventV0_2 evnt = CloudEventV0_2.CreateCloudEvent("test", new Uri("/", UriKind.RelativeOrAbsolute), data, contentType, null);
            BinaryCloudEventV0_2 newEvnt = JsonConvert.DeserializeObject<BinaryCloudEventV0_2>(JsonConvert.SerializeObject(evnt));

            newEvnt.Should().NotBeNull();
            newEvnt.Should().BeOfType<BinaryCloudEventV0_2>();
            newEvnt.ContentType.Should().Be(contentType);

            newEvnt.Data.Length.Should().Be(data.Length);

            var json = JsonConvert.SerializeObject(newEvnt, Formatting.Indented);
        }
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
