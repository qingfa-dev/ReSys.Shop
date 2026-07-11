using Module.Identity.Features.Store.Auth.Register;
using Shared.Governance.Conventions;

namespace Module.UnitTests.Identity.Features.Store.Emails.Confirm;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "EmailConfirmation")]
public class VerificationTokenEncodingTests
{
    [Fact(DisplayName = "BuildVerificationPath should produce token decodable by ConfirmEmail's TryFromBase64Url")]
    public void EmailRegister_BuildVerificationPath_Produces_Token_Decodable_By_ConfirmEmail()
    {
        var userId = Guid.NewGuid();
        var token = ">>>"; // produces Pj4+ in base64 — a + char that would break URL transportation

        var path = EmailRegister.CommandHandler.BuildVerificationPath(userId, token);
        var encodedToken = path.Split("token=")[1];

        Base64Converter.TryFromBase64Url(encodedToken, out var decoded).Should().BeTrue();
        decoded.Should().Be(token);
    }

    [Fact(DisplayName = "BuildVerificationPath should use URL-safe base64 encoding")]
    public void BuildVerificationPath_ShouldUseUrlSafeBase64()
    {
        var userId = Guid.NewGuid();
        var token = ">>>";

        var path = EmailRegister.CommandHandler.BuildVerificationPath(userId, token);
        var encodedToken = path.Split("token=")[1];

        // URL-safe base64 should never contain + or /
        encodedToken.Should().NotContain("+");
        encodedToken.Should().NotContain("/");
    }
}
