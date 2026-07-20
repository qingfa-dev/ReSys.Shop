using Shared.Governance.Conventions;

using Module.Identity.Features.Store.Emails.Change;

namespace Module.UnitTests.Identity.Features.Store.Emails.Change;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "EmailChange")]
public class ChangeEmailEncodingTests
{
    [Fact(DisplayName = "BugFix: BuildConfirmPath encodes token and email with base64url, decodable by ConfirmEmail's TryFromBase64Url")]
    public void BuildConfirmPath_EncodesBase64Url_DecodableByConfirmEmail()
    {
        var userId = Guid.NewGuid();
        var rawToken = "changetoken+/=";
        var rawEmail = "user@example.com";

        var path = ChangeEmail.BuildConfirmPath(userId, rawToken, rawEmail);

        var tokenFromUrl = ExtractQueryParam(path, "token");
        var emailFromUrl = ExtractQueryParam(path, "newEmail");

        var tokenOk = tokenFromUrl.TryFromBase64Url(out var decodedToken);
        var emailOk = emailFromUrl.TryFromBase64Url(out var decodedEmail);

        tokenOk.Should().BeTrue();
        emailOk.Should().BeTrue();
        decodedToken.Should().Be(rawToken);
        decodedEmail.Should().Be(rawEmail);
    }

    private static string ExtractQueryParam(string url, string param)
    {
        var query = url[(url.IndexOf('?') + 1)..];
        foreach (var pair in query.Split('&'))
        {
            var parts = pair.Split('=');
            if (parts[0] == param) return parts[1];
        }
        return string.Empty;
    }
}
