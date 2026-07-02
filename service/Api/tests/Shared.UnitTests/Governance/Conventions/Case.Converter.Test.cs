using Shared.Governance.Conventions;

namespace Shared.UnitTests.Governance.Conventions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Extensions")]
public class CaseConverterTests
{
    // ----- ToPascalCase ------------------------------------------------------
    public class ToPascalCase
    {
        [Theory]
        [InlineData("first_name", "FirstName")]
        [InlineData("first_name_two", "FirstNameTwo")]
        [InlineData("first-name", "FirstName")]
        [InlineData("first-name-two", "FirstNameTwo")]
        [InlineData("hello_world_test", "HelloWorldTest")]
        [InlineData("hello-world-test", "HelloWorldTest")]
        public void FromSnakeOrKebab_ReturnsPascalCase(string input, string expected)
        {
            input.ToPascalCase().Should().Be(expected);
        }

        [Theory]
        [InlineData("firstName", "FirstName")]
        [InlineData("camelCase", "CamelCase")]
        [InlineData("alreadyPascal", "AlreadyPascal")]
        public void FromCamelOrPascal_ReturnsPascalCase(string input, string expected)
        {
            input.ToPascalCase().Should().Be(expected);
        }

        // Acronyms with separators are split and handled correctly
        [Theory]
        [InlineData("xml_parser", "XmlParser")]
        [InlineData("html_parser", "HtmlParser")]
        [InlineData("XML_parser", "XmlParser")]      // "XML" → "Xml"
        [InlineData("parse_XML", "ParseXml")]
        [InlineData("parse-xml", "ParseXml")]
        public void WithSeparatedAcronyms_HandlesCorrectly(string input, string expected)
        {
            input.ToPascalCase().Should().Be(expected);
        }

        // Acronyms without separators are NOT split – they remain as‑is
        [Theory]
        [InlineData("XMLParser", "XMLParser")]      // not all upper → unchanged
        [InlineData("ParseXML", "ParseXML")]
        [InlineData("HTMLParser", "HTMLParser")]
        public void WithEmbeddedAcronyms_KeepsOriginalCase(string input, string expected)
        {
            input.ToPascalCase().Should().Be(expected);
        }

        [Theory]
        [InlineData("ABC", "Abc")]                  // all upper → only first stays upper
        [InlineData("ABc", "ABc")]                  // not all upper → unchanged
        [InlineData("aBc", "ABc")]
        public void MixedCaseWords_BehaveAsExpected(string input, string expected)
        {
            input.ToPascalCase().Should().Be(expected);
        }

        [Theory]
        [InlineData("_first_name", "FirstName")]
        [InlineData("first_name_", "FirstName")]
        [InlineData("___", "___")]
        [InlineData("---", "---")]
        public void EdgeSeparatorCases_ReturnSensible(string input, string expected)
        {
            input.ToPascalCase().Should().Be(expected);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        [InlineData("\t", "")]
        public void NullOrWhitespace_ReturnsEmptyString(string? input, string expected)
        {
            input.ToPascalCase().Should().Be(expected);
        }
    }

    // ----- ToCamelCase -------------------------------------------------------
    public class ToCamelCase
    {
        [Theory]
        [InlineData("FirstName", "firstName")]
        [InlineData("FirstNameTwo", "firstNameTwo")]
        [InlineData("XmlParser", "xmlParser")]       // Pascal with separated acronym
        [InlineData("XMLParser", "xMLParser")]       // no separator → keeps case
        [InlineData("ParseXML", "parseXML")]         // no separator → keeps case
        public void FromPascal_ReturnsCamelCase(string input, string expected)
        {
            input.ToCamelCase().Should().Be(expected);
        }

        [Theory]
        [InlineData("first_name", "firstName")]
        [InlineData("xml_parser", "xmlParser")]
        [InlineData("first-name", "firstName")]
        [InlineData("XML_parser", "xmlParser")]      // separated → "XmlParser" → "xmlParser"
        public void FromSnakeOrKebab_ReturnsCamelCase(string input, string expected)
        {
            input.ToCamelCase().Should().Be(expected);
        }

        [Fact]
        public void SingleCharacter_ReturnsLowercase()
        {
            "A".ToCamelCase().Should().Be("a");
        }

        [Theory]
        [InlineData("alreadyCamel", "alreadyCamel")]
        [InlineData("camelCaseTest", "camelCaseTest")]
        public void AlreadyCamel_RemainsUnchanged(string input, string expected)
        {
            input.ToCamelCase().Should().Be(expected);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        [InlineData("\n", "")]
        public void NullOrWhitespace_ReturnsEmptyString(string? input, string expected)
        {
            input.ToCamelCase().Should().Be(expected);
        }
    }

    // ----- ToSnakeCase -------------------------------------------------------
    public class ToSnakeCase
    {
        [Theory]
        [InlineData("FirstName", "first_name")]
        [InlineData("XMLParser", "xml_parser")]      // detects case changes
        [InlineData("ParseXML", "parse_xml")]
        [InlineData("FirstName2", "first_name2")]
        [InlineData("XmlParser", "xml_parser")]
        [InlineData("HTMLParser", "html_parser")]
        public void FromPascal_ReturnsSnakeCase(string input, string expected)
        {
            input.ToSnakeCase().Should().Be(expected);
        }

        [Theory]
        [InlineData("firstName", "first_name")]
        [InlineData("xmlParser", "xml_parser")]
        [InlineData("parseXML", "parse_xml")]
        public void FromCamel_ReturnsSnakeCase(string input, string expected)
        {
            input.ToSnakeCase().Should().Be(expected);
        }

        [Theory]
        [InlineData("first_name", "first_name")]
        [InlineData("xml_parser", "xml_parser")]
        [InlineData("already_snake", "already_snake")]
        public void AlreadySnake_RemainsUnchanged(string input, string expected)
        {
            input.ToSnakeCase().Should().Be(expected);
        }

        [Theory]
        [InlineData("first-name", "first_name")]
        [InlineData("first-name-two", "first_name_two")]
        public void FromKebab_ConvertsToSnake(string input, string expected)
        {
            input.ToSnakeCase().Should().Be(expected);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        public void NullOrWhitespace_ReturnsEmptyString(string? input, string expected)
        {
            input.ToSnakeCase().Should().Be(expected);
        }
    }

    // ----- ToKebabCase -------------------------------------------------------
    public class ToKebabCase
    {
        [Theory]
        [InlineData("FirstName", "first-name")]
        [InlineData("XMLParser", "xml-parser")]
        [InlineData("ParseXML", "parse-xml")]
        [InlineData("XmlParser", "xml-parser")]
        [InlineData("HTMLParser", "html-parser")]
        public void FromPascal_ReturnsKebabCase(string input, string expected)
        {
            input.ToKebabCase().Should().Be(expected);
        }

        [Theory]
        [InlineData("firstName", "first-name")]
        [InlineData("xmlParser", "xml-parser")]
        [InlineData("parseXML", "parse-xml")]
        public void FromCamel_ReturnsKebabCase(string input, string expected)
        {
            input.ToKebabCase().Should().Be(expected);
        }

        [Theory]
        [InlineData("first_name", "first-name")]
        [InlineData("xml_parser", "xml-parser")]
        public void FromSnake_ReturnsKebabCase(string input, string expected)
        {
            input.ToKebabCase().Should().Be(expected);
        }

        [Theory]
        [InlineData("first-name", "first-name")]
        [InlineData("already-kebab", "already-kebab")]
        public void AlreadyKebab_RemainsUnchanged(string input, string expected)
        {
            input.ToKebabCase().Should().Be(expected);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        public void NullOrWhitespace_ReturnsEmptyString(string? input, string expected)
        {
            input.ToKebabCase().Should().Be(expected);
        }
    }
}