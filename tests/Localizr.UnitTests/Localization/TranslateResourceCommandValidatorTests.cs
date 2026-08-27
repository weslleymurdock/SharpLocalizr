using FluentValidation.TestHelper;
using Localizr.Application.Localization.Commands;
using Localizr.Application.Localization.Validators;

namespace Localizr.UnitTests.Localization;

/// <summary>Contains unit tests for translation command validation.</summary>
public sealed class TranslateResourceCommandValidatorTests
{
    private readonly TranslateResourceCommandValidator _validator = new();

    /// <summary>Verifies that an empty resource collection is rejected.</summary>
    [Fact]
    public async Task ValidateAsync_WhenResourcesAreEmpty_ShouldFail()
    {
        TranslateResourceCommand command = new(new Dictionary<string, string>(), "pt-BR");

        TestValidationResult<TranslateResourceCommand> result =
            await _validator.TestValidateAsync(command, TestContext.Current.CancellationToken);

        result.ShouldHaveValidationErrorFor(x => x.Resources);
    }

    /// <summary>Verifies that an empty culture is rejected.</summary>
    [Fact]
    public async Task ValidateAsync_WhenCultureIsEmpty_ShouldFail()
    {
        TranslateResourceCommand command = new(new Dictionary<string, string> { ["Hello"] = "Hello" }, string.Empty);

        TestValidationResult<TranslateResourceCommand> result =
            await _validator.TestValidateAsync(command, TestContext.Current.CancellationToken);

        result.ShouldHaveValidationErrorFor(x => x.Culture);
    }

    /// <summary>Verifies that an invalid culture is rejected.</summary>
    [Fact]
    public async Task ValidateAsync_WhenCultureIsInvalid_ShouldFail()
    {
        TranslateResourceCommand command = new(new Dictionary<string, string> { ["Hello"] = "Hello" }, "invalid_culture_name");

        TestValidationResult<TranslateResourceCommand> result =
            await _validator.TestValidateAsync(command, TestContext.Current.CancellationToken);

        result.ShouldHaveValidationErrorFor(x => x.Culture);
    }

    /// <summary>Verifies that a valid translation command is accepted.</summary>
    [Fact]
    public async Task ValidateAsync_WhenCommandIsValid_ShouldSucceed()
    {
        TranslateResourceCommand command = new(new Dictionary<string, string> { ["Hello"] = "Hello" }, "pt-BR");

        TestValidationResult<TranslateResourceCommand> result =
            await _validator.TestValidateAsync(command, TestContext.Current.CancellationToken);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
