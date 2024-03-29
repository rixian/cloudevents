// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

#nullable disable
namespace Rixian.CloudEvents.Tests
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
        public static string GetFolder(TestSpecVersion version)
        {
            if (version == TestSpecVersion.V1_0)
            {
                return "./V10Tests/samples";
            }

            throw new ArgumentOutOfRangeException(nameof(version));
        }

        [Theory]
        [InlineData("json1.json", TestSpecVersion.V1_0)]
        [InlineData("json2.json", TestSpecVersion.V1_0)]
        public void TestJsonFiles(string fileName, TestSpecVersion specVersion)
        {
            string json = File.ReadAllText($@"{GetFolder(specVersion)}/json/{fileName}");
            ICloudEvent evnt = CloudEvent.DeserializeAny(json);

            VerifyType(evnt, specVersion);
        }

        [Theory]
        [InlineData("string1.json", TestSpecVersion.V1_0)]
        public void TestStringFilesV02(string fileName, TestSpecVersion specVersion)
        {
            var json = File.ReadAllText($@"{GetFolder(specVersion)}/string/{fileName}");
            ICloudEvent evnt = CloudEvent.DeserializeAny(json);

            VerifyType(evnt, specVersion);
        }

        private static void VerifyType(ICloudEvent cloudEvent, TestSpecVersion version)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (version == TestSpecVersion.V1_0)
            {
                cloudEvent.GetType().Should().BeAssignableTo<CloudEvent>();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(version));
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
