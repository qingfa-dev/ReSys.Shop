using Module.Identity.Features.Admin.Shared.Validators;

namespace Module.UnitTests.Identity.Features.Admin.Users.Shared.Validators;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserRolesValidation")]
public class UserRoleValidationsTests
{
    private readonly TestValidator _validator = new();

    [Fact(DisplayName = "Validation: Should have error when roles list is empty")]
    public void ApplyRoleCollectionRules_WhenEmpty_ShouldHaveError()
    {
        var model = new TestModel { Roles = [] };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Roles);
    }

    [Fact(DisplayName = "Validation: Should have error when roles list is null")]
    public void ApplyRoleCollectionRules_WhenNull_ShouldHaveError()
    {
        var model = new TestModel { Roles = null! };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Roles);
    }

    [Fact(DisplayName = "Validation: Should have error when role name is empty")]
    public void ApplyRoleCollectionRules_WhenRoleNameIsEmpty_ShouldHaveError()
    {
        var model = new TestModel { Roles = ["", "ValidRole"] };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor("Roles[0]");
    }

    [Fact(DisplayName = "Validation: Should pass when roles are valid")]
    public void ApplyRoleCollectionRules_WhenValid_ShouldNotHaveError()
    {
        var model = new TestModel { Roles = ["Admin", "User"] };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    private sealed class TestModel
    {
        public IEnumerable<string> Roles { get; set; } = [];
    }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Roles).ApplyRoleCollectionRules();
        }
    }
}
