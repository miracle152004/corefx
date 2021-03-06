// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace System.IO.Tests
{
    public static partial class PathTests
    {
        [Theory, MemberData(nameof(GetDirectoryName_NonControl_Test_Data))]
        public static void GetDirectoryName_NonControl_Span(string path)
        {
            Assert.Equal(string.Empty, new string(Path.GetDirectoryName(path.AsReadOnlySpan())));
        }

        [Theory, MemberData(nameof(GetDirectoryName_NonControl_Test_Data))]
        public static void GetDirectoryName_NonControlWithSeparator_Span(string path)
        {
            Assert.Equal(path, new string(Path.GetDirectoryName(Path.Combine(path, path).AsReadOnlySpan())));
        }

        [Theory, MemberData(nameof(GetDirectoryName_Test_Data))]
        public static void GetDirectoryName_Span(string path, string expected)
        {
            if (path != null)
                Assert.Equal(expected, new string(Path.GetDirectoryName(path.AsReadOnlySpan())));
        }

        [PlatformSpecific(TestPlatforms.Windows)]  // Tests Windows-specific paths
        [Theory, MemberData(nameof(GetDirectoryName_Windows_Test_Data))]
        public static void GetDirectoryName_Windows_Span(string path, string expected)
        {
            Assert.Equal(expected ?? string.Empty, new string(Path.GetDirectoryName(path.AsReadOnlySpan())));
        }

        [Fact]
        public static void GetDirectoryName_CurrentDirectory_Span()
        {
            string curDir = Directory.GetCurrentDirectory();
            Assert.Equal(curDir, new string(Path.GetDirectoryName(Path.Combine(curDir, "baz").AsReadOnlySpan())));
            Assert.Equal(string.Empty, new string(Path.GetDirectoryName(Path.GetPathRoot(curDir).AsReadOnlySpan())));
        }

        [Theory, MemberData(nameof(GetExtension_Test_Data))]
        public static void GetExtension_Span(string path, string expected)
        {
            if (path != null)
            {
                path = path.Replace('/', Path.DirectorySeparatorChar);

                Assert.Equal(expected, new string(Path.GetExtension(path.AsReadOnlySpan())));
                Assert.Equal(!string.IsNullOrEmpty(expected), Path.HasExtension(path.AsReadOnlySpan()));
            }
        }

        [Theory]
        [MemberData(nameof(GetFileName_TestData))]
        public static void GetFileName_Span(string path, string expected)
        {
            if (path != null)
                Assert.Equal(expected, new string(Path.GetFileName(path.AsReadOnlySpan())));
        }

        [Theory]
        [MemberData(nameof(GetFileNameWithoutExtension_TestData))]
        public static void GetFileNameWithoutExtension_Span(string path, string expected)
        {
            if(path != null)
                Assert.Equal(expected, new string(Path.GetFileNameWithoutExtension(path.AsReadOnlySpan())));
        }

        [Fact]
        public static void GetPathRoot_Empty_Span()
        {
            Assert.Equal(string.Empty, new string(Path.GetPathRoot(ReadOnlySpan<char>.Empty)));
        }

        [PlatformSpecific(TestPlatforms.Windows)]  // Tests UNC
        [Theory, MemberData(nameof(GetPathRoot_Windows_UncAndExtended_Test_Data))]
        public static void GetPathRoot_Windows_UncAndExtended_Span(string value, string expected)
        {
            Assert.True(Path.IsPathRooted(value.AsReadOnlySpan()));
            Assert.Equal(expected, new string(Path.GetPathRoot(value.AsReadOnlySpan())));
        }

        [PlatformSpecific(TestPlatforms.Windows)]
        [Theory, MemberData(nameof(GetPathRoot_Windows_UncAndExtended_WithLegacySupport_Test_Data))]
        public static void GetPathRoot_Windows_UncAndExtended_WithLegacySupport_Span(string normalExpected, string legacyExpected, string value)
        {
            Assert.True(Path.IsPathRooted(value.AsReadOnlySpan()));

            string expected = PathFeatures.IsUsingLegacyPathNormalization() ? legacyExpected : normalExpected;
            Assert.Equal(expected, new string(Path.GetPathRoot(value.AsReadOnlySpan())));
        }

        [PlatformSpecific(TestPlatforms.Windows)]  // Tests Windows-specific path convention
        [Theory, MemberData(nameof(GetPathRoot_Windows_Test_Data))]
        public static void GetPathRoot_Windows_Span(string value, string expected)
        {
            Assert.True(Path.IsPathRooted(value.AsReadOnlySpan()));
            Assert.Equal(expected, new string(Path.GetPathRoot(value.AsReadOnlySpan())));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public static void IsPathRooted_Span(string path)
        {
            Assert.False(Path.IsPathRooted(path.AsReadOnlySpan()));
        }

        // Testing invalid drive letters !(a-zA-Z)
        [PlatformSpecific(TestPlatforms.Windows)]
        [Theory, MemberData(nameof(IsPathRooted_Windows_Invalid_Test_Data))]
        [SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework, "Bug fixed on Core where it would return true if the first char is not a drive letter followed by a VolumeSeparatorChar coreclr/10297")]
        public static void IsPathRooted_Windows_Invalid_Span(string value)
        {
            Assert.False(Path.IsPathRooted(value.AsReadOnlySpan()));
        }

        [Fact]
        public static void GetInvalidPathChars_Span()
        {
            Assert.All(Path.GetInvalidPathChars(), c =>
            {
                string bad = c.ToString();
                Assert.Equal(string.Empty, new string(Path.GetDirectoryName(bad.AsReadOnlySpan())));
                Assert.Equal(string.Empty, new string(Path.GetExtension(bad.AsReadOnlySpan())));
                Assert.Equal(bad, new string(Path.GetFileName(bad.AsReadOnlySpan())));
                Assert.Equal(bad, new string(Path.GetFileNameWithoutExtension(bad.AsReadOnlySpan())));
                Assert.Equal(string.Empty, new string(Path.GetPathRoot(bad.AsReadOnlySpan())));
                Assert.False(Path.IsPathRooted(bad.AsReadOnlySpan()));
            });
        }

        [Theory, MemberData(nameof(GetDirectoryName_Unix_Test_Data))]
        [PlatformSpecific(TestPlatforms.AnyUnix)]  // Tests Unix-specific paths
        public static void GetDirectoryName_Unix_Span(string path, string expected)
        {
            Assert.Equal(expected ?? string.Empty, new string(Path.GetDirectoryName(path.AsReadOnlySpan())));
        }

        [PlatformSpecific(TestPlatforms.AnyUnix)]  // Checks Unix-specific special characters in directory path
        [Theory, MemberData(nameof(GetDirectoryName_ControlCharacters_Unix_Test_Data))]
        public static void GetDirectoryName_ControlCharacters_Unix_Span(char ch, int count, string file)
        {
            Assert.Equal(new string(ch, count), new string(Path.GetDirectoryName(Path.Combine(new string(ch, count), file).AsReadOnlySpan())));
        }

        [PlatformSpecific(TestPlatforms.AnyUnix)]  // Checks file extension behavior on Unix
        [Theory, MemberData(nameof(GetExtension_Unix_Test_Data))]
        public static void GetExtension_Unix_Span(string path, string expected)
        {
            Assert.Equal(expected, new string(Path.GetExtension(path.AsReadOnlySpan())));
            Assert.Equal(!string.IsNullOrEmpty(expected), Path.HasExtension(path.AsReadOnlySpan()));
        }

        [PlatformSpecific(TestPlatforms.AnyUnix)]  // Tests Unix-specific valid file names
        [Theory, MemberData(nameof(GetFileName_Unix_Test_Data))]
        public static void GetFileName_Unix_Span(string file)
        {
            Assert.Equal(file, new string(Path.GetFileName(file).AsReadOnlySpan()));
        }

        [Fact]
        public static void GetFileNameWithSpaces_Unix_Span()
        {
            Assert.Equal("fi  le", new string(Path.GetFileName(Path.Combine("b \r\n ar", "fi  le").AsReadOnlySpan())));
        }
    }
}
