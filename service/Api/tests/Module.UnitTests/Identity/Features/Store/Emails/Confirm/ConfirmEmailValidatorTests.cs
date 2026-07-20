using FluentValidation.TestHelper;

using Module.Identity.Features.Store.Emails.Confirm;

namespace Module.UnitTests.Identity.Features.Store.Emails.Confirm;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "EmailConfirmation")]
public class ConfirmEmailValidatorTests
{
    [Fact(DisplayName = "Validator: rejects empty UserId")]
    public void Validator_EmptyUserId_ReturnsError()
    {
        var validator = new ConfirmEmail.Validator();
        var command = new ConfirmEmail.Command(new ConfirmEmail.Request
        {
            UserId = Guid.Empty,
            Token = "valid-token"
        });

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.UserId);
    }

    [Fact(DisplayName = "Validator: accepts valid UserId and Token")]
    public void Validator_ValidInput_Passes()
    {
        var validator = new ConfirmEmail.Validator();
        var command = new ConfirmEmail.Command(new ConfirmEmail.Request
        {
            UserId = Guid.NewGuid(),
            Token = "valid-token"
        });

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
