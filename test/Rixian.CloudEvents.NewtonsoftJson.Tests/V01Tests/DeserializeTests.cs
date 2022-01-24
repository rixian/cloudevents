// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

#pragma warning disable CS0618 // Type or member is obsolete
namespace Rixian.CloudEvents.Tests.V01
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
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
            var json = File.ReadAllText($@"./V01Tests/samples/json/{fileName}");
            var evnt = CloudEventV0_1.Deserialize(json);
            Assert.IsType<JsonCloudEventV0_1>(evnt);
        }

        [Theory]
        [InlineData("binary1.json")]
        public void TestBinaryFiles(string fileName)
        {
            var json = File.ReadAllText($@"./V01Tests/samples/binary/{fileName}");
            var evnt = CloudEventV0_1.Deserialize(json);
            Assert.IsType<BinaryCloudEventV0_1>(evnt);
        }

        [Theory]
        [InlineData("string1.json")]
        public void TestStringFiles(string fileName)
        {
            var json = File.ReadAllText($@"./V01Tests/samples/string/{fileName}");
            var evnt = CloudEventV0_1.Deserialize(json);
            Assert.IsType<StringCloudEventV0_1>(evnt);
        }

        [Theory]
        [InlineData("none1.json")]
        public void TestNoDataFiles(string fileName)
        {
            var json = File.ReadAllText($@"./V01Tests/samples/none/{fileName}");
            var evnt = CloudEventV0_1.Deserialize(json);
            Assert.IsType<CloudEventV0_1>(evnt);
            Assert.IsNotType<JsonCloudEventV0_1>(evnt);
            Assert.IsNotType<BinaryCloudEventV0_1>(evnt);
            Assert.IsNotType<StringCloudEventV0_1>(evnt);
        }
    }
}
#pragma warning restore CS0618 // Type or member is obsolete
