using Shared.Governance.Conventions;

namespace Shared.UnitTests.Governance.Conventions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Extensions")]
public class Base64ExtensionsTests
{
    public class ToBase64
    {
        [Fact]
        public void WithPlainText_ReturnsBase64EncodedString()
        {
            var result = "Hello World".ToBase64();

            result.Should().Be("SGVsbG8gV29ybGQ=");
        }

        [Fact]
        public void WithUnicodeText_ReturnsBase64EncodedString()
        {
            var result = "\u00F1".ToBase64();

            result.Should().Be("w7E=");
        }

        [Fact]
        public void WithEmptyString_ReturnsEmptyString()
        {
            var result = "".ToBase64();

            result.Should().Be("");
        }

        [Fact]
        public void WithNull_ThrowsArgumentNullException()
        {
            Func<string> act = () => ((string?)null)!.ToBase64();

            act.Should().Throw<ArgumentNullException>();
        }
    }

    public class FromBase64
    {
        [Fact]
        public void WithValidBase64_ReturnsDecodedString()
        {
            var result = "SGVsbG8gV29ybGQ=".FromBase64();

            result.Should().Be("Hello World");
        }

        [Fact]
        public void WithUnicodeBase64_ReturnsDecodedString()
        {
            var result = "w7E=".FromBase64();

            result.Should().Be("\u00F1");
        }

        [Fact]
        public void Roundtrip_ToBase64ThenFromBase64_ReturnsOriginal()
        {
            var original = "Hello World! 123 \u00F1 \u00FF";
            var encoded = original.ToBase64();
            var decoded = encoded.FromBase64();

            decoded.Should().Be(original);
        }

        [Fact]
        public void WithEmptyString_ReturnsEmptyString()
        {
            var result = "".FromBase64();

            result.Should().Be("");
        }

        [Fact]
        public void WithNull_ThrowsArgumentNullException()
        {
            Func<string> act = () => ((string?)null)!.FromBase64();

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WithInvalidBase64_ThrowsFormatException()
        {
            Func<string> act = () => "!!!invalid".FromBase64();

            act.Should().Throw<FormatException>();
        }
    }

    public class TryFromBase64
    {
        [Fact]
        public void WithValidBase64_ReturnsTrueAndDecodedString()
        {
            var result = "SGVsbG8gV29ybGQ=".TryFromBase64(out var decoded);

            result.Should().BeTrue();
            decoded.Should().Be("Hello World");
        }

        [Fact]
        public void WithNull_ReturnsFalseAndEmptyString()
        {
            var result = ((string?)null)!.TryFromBase64(out var decoded);

            result.Should().BeFalse();
            decoded.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void WithNullOrWhitespace_ReturnsFalseAndEmptyString(string input)
        {
            var result = input.TryFromBase64(out var decoded);

            result.Should().BeFalse();
            decoded.Should().BeEmpty();
        }

        [Fact]
        public void WithInvalidBase64_ReturnsFalseAndEmptyString()
        {
            var result = "!!!invalid".TryFromBase64(out var decoded);

            result.Should().BeFalse();
            decoded.Should().BeEmpty();
        }
    }

    public class ToBase64Url
    {
        [Fact]
        public void WithPlainText_ReturnsUrlSafeBase64WithoutPadding()
        {
            var result = "Hello World".ToBase64Url();

            result.Should().Be("SGVsbG8gV29ybGQ");
            result.Should().NotContain("=");
        }

        [Fact]
        public void WithInputNeedingPadding_StripsEqualsSigns()
        {
            var result = "a".ToBase64Url();

            result.Should().Be("YQ");
            result.Should().NotContain("=");
        }

        [Fact]
        public void WithPlusAndSlashInBase64_ReplacesWithMinusAndUnderscore()
        {
            var result = "\u0BFF".ToBase64Url();

            result.Should().Be("4K-_");
            result.Should().NotContain("+");
            result.Should().NotContain("/");
            result.Should().NotContain("=");
        }

        [Fact]
        public void WithEmptyString_ReturnsEmptyString()
        {
            var result = "".ToBase64Url();

            result.Should().Be("");
        }

        [Fact]
        public void WithNull_ThrowsArgumentNullException()
        {
            Func<string> act = () => ((string?)null)!.ToBase64Url();

            act.Should().Throw<ArgumentNullException>();
        }
    }

    public class FromBase64Url
    {
        [Fact]
        public void WithValidBase64Url_ReturnsDecodedString()
        {
            var result = "SGVsbG8gV29ybGQ".FromBase64Url();

            result.Should().Be("Hello World");
        }

        [Fact]
        public void WithRestoredPadding_ReturnsDecodedString()
        {
            var result = "YQ".FromBase64Url();

            result.Should().Be("a");
        }

        [Fact]
        public void WithMinusAndUnderscore_RestoresPlusAndSlash()
        {
            var result = "4K-_".FromBase64Url();

            result.Should().Be("\u0BFF");
        }

        [Fact]
        public void Roundtrip_ToBase64UrlThenFromBase64Url_ReturnsOriginal()
        {
            var original = "Hello World! 123 \u00F1 \u00FF \u0BFF";
            var encoded = original.ToBase64Url();
            var decoded = encoded.FromBase64Url();

            decoded.Should().Be(original);
        }

        [Fact]
        public void WithNull_ThrowsArgumentNullException()
        {
            Func<string> act = () => ((string?)null)!.FromBase64Url();

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WithInvalidBase64Url_ThrowsFormatException()
        {
            Func<string> act = () => "!!!invalid".FromBase64Url();

            act.Should().Throw<FormatException>();
        }
    }

    public class TryFromBase64Url
    {
        [Fact]
        public void WithValidBase64Url_ReturnsTrueAndDecodedString()
        {
            var result = "SGVsbG8gV29ybGQ".TryFromBase64Url(out var decoded);

            result.Should().BeTrue();
            decoded.Should().Be("Hello World");
        }

        [Fact]
        public void WithNull_ReturnsFalseAndEmptyString()
        {
            var result = ((string?)null)!.TryFromBase64Url(out var decoded);

            result.Should().BeFalse();
            decoded.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void WithNullOrWhitespace_ReturnsFalseAndEmptyString(string input)
        {
            var result = input.TryFromBase64Url(out var decoded);

            result.Should().BeFalse();
            decoded.Should().BeEmpty();
        }

        [Fact]
        public void WithInvalidBase64Url_ReturnsFalseAndEmptyString()
        {
            var result = "!!!invalid".TryFromBase64Url(out var decoded);

            result.Should().BeFalse();
            decoded.Should().BeEmpty();
        }
    }
}
