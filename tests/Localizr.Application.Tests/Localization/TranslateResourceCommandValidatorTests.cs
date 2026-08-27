using Localizr.Application.Localization.Commands;
using Localizr.Application.Localization.Validators;
using FluentAssertions;

namespace Localizr.Application.Tests.Localization;

/// <summary>Contains tests for translation command validation.</summary>
public sealed class TranslateResourceCommandValidatorTests
{
    private readonly TranslateResourceCommandValidator _validator = new();

    /// <summary>Verifies that an empty resource collection is rejected.</summary>
    [Fact]
    public async Task ValidateAsync_WhenResourcesAreEmpty_ShouldFail()
    {
        TranslateResourceCommand command = new(new Dictionary<string, string>(), "pt-BR");

        (await _validator.ValidateAsync(command)).IsValid.Should().BeFalse();
    }

    /// <summary>Verifies that an empty culture is rejected.</summary>
    [Fact]
    public async Task ValidateAsync_WhenCultureIsEmpty_ShouldFail()
    {
        TranslateResourceCommand command = new(new Dictionary<string, string> { ["Hello"] = "Hello" }, string.Empty);

        (await _validator.ValidateAsync(command)).IsValid.Should().BeFalse();
    }

    /// <summary>Verifies that an invalid culture is rejected.</summary>
    [Fact]
    public async Task ValidateAsync_WhenCultureIsInvalid_ShouldFail()
    {
        TranslateResourceCommand command = new(new Dictionary<string, string> { ["Hello"] = "Hello" }, "not-a-culture");

        (await _validator.ValidateAsync(command)).IsValid.Should().BeFalse();
    }

    /// <summary>Verifies that a valid translation command is accepted.</summary>
    [Fact]
    public async Task ValidateAsync_WhenCommandIsValid_ShouldSucceed()
    {
        TranslateResourceCommand command = new(new Dictionary<string, string> { ["Hello"] = "Hello" }, "pt-BR");

        (await _validator.ValidateAsync(command)).IsValid.Should().BeTrue();
    }
}
