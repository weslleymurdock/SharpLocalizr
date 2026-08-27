using Localizr.Application.Identity.Abstractions;
using Localizr.Application.Identity.Commands;
using Localizr.Application.Identity.Validators;
using NSubstitute;

namespace Localizr.UnitTests.Identity;

/// <summary>Contains focused tests for identity command validation rules.</summary>
public sealed class IdentityValidatorsTests
{
    /// <summary>Verifies registration accepts a valid email and password.</summary>
    [Fact]
    public async Task Register_WhenValid_ShouldPass()
    {
        IIdentityService service = Substitute.For<IIdentityService>();
        service.EmailExistsAsync("user@example.com", Arg.Any<CancellationToken>()).Returns(false);

        var result = await new RegisterCommandValidator(service).ValidateAsync(
            new RegisterCommand("user@example.com", "Password1!"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    /// <summary>Verifies registration rejects an empty email and password.</summary>
    [Fact]
    public async Task Register_WhenRequiredValuesAreEmpty_ShouldFail()
    {
        IIdentityService service = Substitute.For<IIdentityService>();

        var result = await new RegisterCommandValidator(service).ValidateAsync(
            new RegisterCommand(string.Empty, string.Empty),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.Email));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.Password));
    }

    /// <summary>Verifies registration rejects malformed email addresses.</summary>
    [Fact]
    public async Task Register_WhenEmailIsInvalid_ShouldFail()
    {
        IIdentityService service = Substitute.For<IIdentityService>();

        var result = await new RegisterCommandValidator(service).ValidateAsync(
            new RegisterCommand("not-an-email", "Password1!"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.Email));
    }

    /// <summary>Verifies registration rejects short passwords.</summary>
    [Fact]
    public async Task Register_WhenPasswordIsTooShort_ShouldFail()
    {
        IIdentityService service = Substitute.For<IIdentityService>();

        var result = await new RegisterCommandValidator(service).ValidateAsync(
            new RegisterCommand("user@example.com", "short"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.Password));
    }

    /// <summary>Verifies registration rejects an email already in use.</summary>
    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ShouldFail()
    {
        IIdentityService service = Substitute.For<IIdentityService>();
        service.EmailExistsAsync("user@example.com", Arg.Any<CancellationToken>()).Returns(true);

        var result = await new RegisterCommandValidator(service).ValidateAsync(
            new RegisterCommand("user@example.com", "Password1!"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorCode == "CONFLICT");
    }

    /// <summary>Verifies login accepts a valid command without a second-factor code.</summary>
    [Fact]
    public async Task Login_WhenValid_ShouldPass()
    {
        var result = await new LoginCommandValidator().ValidateAsync(
            new LoginCommand("user@example.com", "Password1!"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    /// <summary>Verifies login rejects invalid email and password values.</summary>
    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ShouldFail()
    {
        var result = await new LoginCommandValidator().ValidateAsync(
            new LoginCommand(string.Empty, "short"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginCommand.Email));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginCommand.Password));
    }

    /// <summary>Verifies login rejects simultaneous authenticator and recovery codes.</summary>
    [Fact]
    public async Task Login_WhenBothTwoFactorCodesAreProvided_ShouldFail()
    {
        var result = await new LoginCommandValidator().ValidateAsync(
            new LoginCommand("user@example.com", "Password1!", "123456", "recovery"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == string.Empty);
    }

    /// <summary>Verifies refresh-token validation.</summary>
    [Fact]
    public async Task RefreshToken_WhenTokenIsEmpty_ShouldFail()
    {
        var result = await new RefreshTokenCommandValidator().ValidateAsync(
            new RefreshTokenCommand(string.Empty),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    /// <summary>Verifies access-token revocation validation.</summary>
    [Fact]
    public async Task RevokeToken_WhenTokenIsEmpty_ShouldFail()
    {
        var result = await new RevokeTokenCommandValidator().ValidateAsync(
            new RevokeTokenCommand(string.Empty),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    /// <summary>Verifies email confirmation requires both identifiers and codes.</summary>
    [Fact]
    public async Task ConfirmEmail_WhenRequiredValuesAreMissing_ShouldFail()
    {
        var result = await new ConfirmEmailCommandValidator().ValidateAsync(
            new ConfirmEmailCommand(string.Empty, string.Empty),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    /// <summary>Verifies confirmation resend validation.</summary>
    [Fact]
    public async Task ResendConfirmation_WhenEmailIsInvalid_ShouldFail()
    {
        var result = await new ResendConfirmationEmailCommandValidator().ValidateAsync(
            new ResendConfirmationEmailCommand("invalid"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    /// <summary>Verifies password recovery validation.</summary>
    [Fact]
    public async Task ForgotPassword_WhenEmailIsInvalid_ShouldFail()
    {
        var result = await new ForgotPasswordCommandValidator().ValidateAsync(
            new ForgotPasswordCommand("invalid"),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    /// <summary>Verifies password reset validation for every required field.</summary>
    [Fact]
    public async Task ResetPassword_WhenRequiredValuesAreMissing_ShouldFail()
    {
        var result = await new ResetPasswordCommandValidator().ValidateAsync(
            new ResetPasswordCommand(string.Empty, string.Empty, string.Empty),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Equal(3, result.Errors.Count);
    }

    /// <summary>Verifies password reset accepts a valid request.</summary>
    [Fact]
    public async Task ResetPassword_WhenValid_ShouldPass()
    {
        var result = await new ResetPasswordCommandValidator().ValidateAsync(
            new ResetPasswordCommand("user@example.com", "code", "Password1!"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    /// <summary>Verifies identity update requires the current password and validates optional changes.</summary>
    [Fact]
    public async Task UpdateIdentityInfo_WhenInvalid_ShouldFail()
    {
        var result = await new UpdateIdentityInfoCommandValidator().ValidateAsync(
            new UpdateIdentityInfoCommand("", "invalid", "short", ""),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateIdentityInfoCommand.UserId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateIdentityInfoCommand.OldPassword));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateIdentityInfoCommand.NewEmail));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateIdentityInfoCommand.NewPassword));
    }

    /// <summary>Verifies identity update accepts omitted optional changes.</summary>
    [Fact]
    public async Task UpdateIdentityInfo_WhenOptionalChangesAreOmitted_ShouldPass()
    {
        var result = await new UpdateIdentityInfoCommandValidator().ValidateAsync(
            new UpdateIdentityInfoCommand("user-id", null, null, "Password1!"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }
}
