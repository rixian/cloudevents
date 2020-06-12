// Copyright (c) Rixian. All rights reserved.
// Licensed under the Apache License, Version 2.0 license. See LICENSE file in the project root for full license information.

namespace Rixian.CloudEvents.Tests.V10
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Xunit;

    public class ValidationTests
    {
        [Fact]
        public void Basic_Success()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeTrue();
        }

        #region id Field Tests
        [Fact]
        public void Id_Guid_Success()
        {
            var json = @"
{
  'id': '00717CE8-D29E-4C2E-84D4-A9E026575778',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext'
}
";
            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeTrue();
        }

        [Fact]
        public void Id_Empty_Fail()
        {
            var json = @"
{
  'id': '',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext'
}
";
            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        [Fact]
        public void Id_None_Fail()
        {
            var json = @"
{
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext'
}
";
            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }
        #endregion

        #region type Field Tests
        [Fact]
        public void Type_Empty_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': '',
  'specversion': '1.0',
  'source': '/mycontext'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        [Fact]
        public void Type_None_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'specversion': '1.0',
  'source': '/mycontext'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }
        #endregion

        #region specversion Field Tests
        [Fact]
        public void SpecVersion_Empty_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '',
  'source': '/mycontext'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        [Fact]
        public void SpecVersion_None_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'source': '/mycontext'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        [Fact]
        public void SpecVersion_Incorrect_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '0.1',
  'source': '/mycontext'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }
        #endregion

        #region source Field Tests
        [Fact]
        public void Source_FullUri_Success()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': 'https://example.com/foo'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeTrue();
        }

        [Fact]
        public void Source_Empty_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': ''
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        [Fact]
        public void Source_None_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        [Fact]
        public void Source_Invalid_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '`~!@#$%^&*()-_=+[{]};:'"",<.>/?'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }
        #endregion

        #region time Field Tests
        [Fact]
        public void Time_Basic_Success()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'time': '2019-04-13T15:07:00.2031033+00:00'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeTrue();
        }

        [Fact]
        public void Time_Empty_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'time': ''
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        [Fact]
        public void Time_Invalid_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'time': 'ABCDEFG'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        [Fact]
        public void Time_NoTimeZone_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'time': '2019-04-13T15:07:00.2031033'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }
        #endregion

        #region dataschema Field Tests
        [Fact]
        public void DataSchema_FullUri_Success()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'dataschema': 'https://example.com/foo'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeTrue();
        }

        [Fact]
        public void DataSchema_Empty_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'dataschema': ''
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        [Fact]
        public void DataSchema_Invalid_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'dataschema': '`~!@#$%^&*()-_=+[{]};:'"",<.>/?'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }
        #endregion

        #region contenttype Field Tests
        [Fact]
        public void ContentType_Basic_Success()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'datacontenttype': 'text/plain'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeTrue();
        }

        [Fact]
        public void ContentType_Empty_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'datacontenttype': ''
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        [Theory]
        [InlineData("aaaaaa")]
        [InlineData("pdf")]
        [InlineData("application//pdf")]
        public void ContentType_Invalid_Fail(string contentType)
        {
            var json = $@"
{{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'datacontenttype': '{contentType}'
}}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }
        #endregion

        #region data Field Tests
        [Fact]
        public void Data_String_Basic_Success()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'data': 'This is some text...'
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeTrue();
        }

        [Fact]
        public void Data_String_Empty_Fail()
        {
            var json = @"
{
  'id': 'C234-1234-1234',
  'type': 'com.example.someevent',
  'specversion': '1.0',
  'source': '/mycontext',
  'data': ''
}";

            Tuple<bool, System.Collections.Generic.IReadOnlyList<string>> validationResults = CloudEvent.ValidateJsonDetailed(json);
            validationResults.Item1.Should().BeFalse();
        }

        /*
        [Fact]
        public void Data_String_BLNS_Success()
        {
            var blns = GetBlns();

            foreach (var str in blns)
            {
                var json = $@"
 {{
   'id': 'C234-1234-1234',
   'type': 'com.example.someevent',
   'specversion': '1.0',
   'source': '/mycontext',
   'data': '{str}'
 }}";

                var validationResults = CloudEvent.ValidateJsonDetailed(json);
                validationResults.Item1.Should().BeTrue();
            }
        }
        */
        #endregion

        private static string[] GetBlns()
        {
            var lines = File.ReadAllLines(@"blns.txt");
            return lines.Where(l => l.StartsWith("#", StringComparison.OrdinalIgnoreCase) == false).Where(l => string.IsNullOrEmpty(l) == false).ToArray();
        }
    }
}
