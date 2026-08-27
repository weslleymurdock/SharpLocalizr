using FluentAssertions;
using Localizr.Application.Common.Responses;
using Localizr.Application.Identity.Abstractions;
using Localizr.Application.Identity.Commands;
using Localizr.Application.Identity.Handlers;
using Localizr.Application.Identity.Queries;
using Localizr.Application.Identity.Responses;
using NSubstitute;

namespace Localizr.UnitTests.Identity;

/// <summary>Verifies the Identity Mediator handler behavior.</summary>
public sealed class IdentityHandlersTests
{
    private readonly IIdentityService identityService = Substitute.For<IIdentityService>();
    private readonly IdentityHandlers handler;
    private readonly CancellationToken cancellationToken = new CancellationTokenSource().Token;

    /// <summary>Initializes the handler under test with a mocked identity service.</summary>
    public IdentityHandlersTests()
    {
        handler = new IdentityHandlers(identityService);
    }

    /// <summary>Verifies registration delegates all command data and the cancellation token.</summary>
    [Fact]
    public async Task Register_ShouldDelegateToService()
    {
        IdentityResultResponse expected = IdentityResultResponse.Success();
        identityService.RegisterAsync("user@example.com", "Password1!", cancellationToken).Returns(expected);

        IdentityResultResponse result = await handler.Handle(new RegisterCommand("user@example.com", "Password1!"), cancellationToken);

        result.Should().Be(expected);
        await identityService.Received(1).RegisterAsync("user@example.com", "Password1!", cancellationToken);
    }

    /// <summary>Verifies successful login returns the service token response.</summary>
    [Fact]
    public async Task Login_WhenServiceReturnsTokens_ShouldSucceed()
    {
        TokenResponse tokens = new("Bearer", "access", 3600, "refresh");
        identityService.LoginAsync("user@example.com", "Password1!", "123456", null, cancellationToken).Returns(tokens);

        Response<TokenResponse> result = await handler.Handle(new LoginCommand("user@example.com", "Password1!", "123456"), cancellationToken);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(tokens);
        await identityService.Received(1).LoginAsync("user@example.com", "Password1!", "123456", null, cancellationToken);
    }

    /// <summary>Verifies failed login is converted to an application failure response.</summary>
    [Fact]
    public async Task Login_WhenServiceReturnsNull_ShouldFail()
    {
        identityService.LoginAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), cancellationToken)
            .Returns((TokenResponse?)null);

        Response<TokenResponse> result = await handler.Handle(new LoginCommand("user@example.com", "wrong", null, "recovery"), cancellationToken);

        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Errors.Should().ContainSingle().Which.Should().Be("Invalid credentials.");
    }

    /// <summary>Verifies successful refresh returns the replacement token pair.</summary>
    [Fact]
    public async Task Refresh_WhenServiceReturnsTokens_ShouldSucceed()
    {
        TokenResponse tokens = new("Bearer", "access", 3600, "refresh");
        identityService.RefreshAsync("refresh-token", cancellationToken).Returns(tokens);

        Response<TokenResponse> result = await handler.Handle(new RefreshTokenCommand("refresh-token"), cancellationToken);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(tokens);
    }

    /// <summary>Verifies an invalid refresh token becomes a failure response.</summary>
    [Fact]
    public async Task Refresh_WhenServiceReturnsNull_ShouldFail()
    {
        identityService.RefreshAsync("invalid", cancellationToken).Returns((TokenResponse?)null);

        Response<TokenResponse> result = await handler.Handle(new RefreshTokenCommand("invalid"), cancellationToken);

        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.Should().Be("Invalid refresh token.");
    }

    /// <summary>Verifies token revocation delegates to the identity service.</summary>
    [Fact]
    public async Task Revoke_ShouldReturnServiceResult()
    {
        identityService.RevokeAsync("access-token", cancellationToken).Returns(true);

        Response<bool> result = await handler.Handle(new RevokeTokenCommand("access-token"), cancellationToken);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().BeTrue();
        await identityService.Received(1).RevokeAsync("access-token", cancellationToken);
    }

    /// <summary>Verifies email confirmation delegates all command data.</summary>
    [Fact]
    public async Task ConfirmEmail_ShouldReturnServiceResult()
    {
        identityService.ConfirmEmailAsync("user-id", "code", "new@example.com", cancellationToken).Returns(true);

        Response<bool> result = await handler.Handle(new ConfirmEmailCommand("user-id", "code", "new@example.com"), cancellationToken);

        result.Data.Should().BeTrue();
        await identityService.Received(1).ConfirmEmailAsync("user-id", "code", "new@example.com", cancellationToken);
    }

    /// <summary>Verifies confirmation email resend delegates the request.</summary>
    [Fact]
    public async Task ResendConfirmationEmail_ShouldDelegateToService()
    {
        IdentityResultResponse expected = IdentityResultResponse.Success();
        identityService.ResendConfirmationEmailAsync("user@example.com", cancellationToken).Returns(expected);

        IdentityResultResponse result = await handler.Handle(new ResendConfirmationEmailCommand("user@example.com"), cancellationToken);

        result.Should().Be(expected);
    }

    /// <summary>Verifies password recovery delegates the email and cancellation token.</summary>
    [Fact]
    public async Task ForgotPassword_ShouldDelegateToService()
    {
        IdentityResultResponse expected = IdentityResultResponse.Success();
        identityService.ForgotPasswordAsync("user@example.com", cancellationToken).Returns(expected);

        IdentityResultResponse result = await handler.Handle(new ForgotPasswordCommand("user@example.com"), cancellationToken);

        result.Should().Be(expected);
    }

    /// <summary>Verifies password reset delegates all command data.</summary>
    [Fact]
    public async Task ResetPassword_ShouldDelegateToService()
    {
        IdentityResultResponse expected = IdentityResultResponse.Success();
        identityService.ResetPasswordAsync("user@example.com", "reset-code", "Password2!", cancellationToken).Returns(expected);

        IdentityResultResponse result = await handler.Handle(new ResetPasswordCommand("user@example.com", "reset-code", "Password2!"), cancellationToken);

        result.Should().Be(expected);
    }

    /// <summary>Verifies a missing identity is converted to a not-found application response.</summary>
    [Fact]
    public async Task GetIdentityInfo_WhenServiceReturnsNull_ShouldFail()
    {
        identityService.GetInfoAsync("user-id", cancellationToken).Returns((IdentityInfoResponse?)null);

        Response<IdentityInfoResponse> result = await handler.Handle(new GetIdentityInfoQuery("user-id"), cancellationToken);

        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Errors.Should().ContainSingle().Which.Should().Be("User not found.");
    }

    /// <summary>Verifies an existing identity is returned unchanged.</summary>
    [Fact]
    public async Task GetIdentityInfo_WhenServiceReturnsIdentity_ShouldSucceed()
    {
        IdentityInfoResponse expected = new("user@example.com", true);
        identityService.GetInfoAsync("user-id", cancellationToken).Returns(expected);

        Response<IdentityInfoResponse> result = await handler.Handle(new GetIdentityInfoQuery("user-id"), cancellationToken);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(expected);
    }

    /// <summary>Verifies identity information updates delegate all command values.</summary>
    [Fact]
    public async Task UpdateIdentityInfo_ShouldDelegateToService()
    {
        IdentityResultResponse expected = IdentityResultResponse.Success();
        identityService.UpdateInfoAsync("user-id", "new@example.com", "Password2!", "Password1!", cancellationToken).Returns(expected);

        IdentityResultResponse result = await handler.Handle(
            new UpdateIdentityInfoCommand("user-id", "new@example.com", "Password2!", "Password1!"),
            cancellationToken);

        result.Should().Be(expected);
    }

    /// <summary>Verifies invalid two-factor configuration is converted to a failure response.</summary>
    [Fact]
    public async Task ConfigureTwoFactor_WhenServiceReturnsNull_ShouldFail()
    {
        identityService.ConfigureTwoFactorAsync("user-id", true, "123456", true, false, false, cancellationToken)
            .Returns((TwoFactorResponse?)null);

        Response<TwoFactorResponse> result = await handler.Handle(
            new ConfigureTwoFactorCommand("user-id", true, "123456", true, false, false),
            cancellationToken);

        result.Succeeded.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Errors.Should().ContainSingle().Which.Should().Be("Invalid 2FA configuration.");
    }

    /// <summary>Verifies successful two-factor configuration returns the service response.</summary>
    [Fact]
    public async Task ConfigureTwoFactor_WhenServiceReturnsConfiguration_ShouldSucceed()
    {
        TwoFactorResponse expected = new("shared-key", 8, ["code"], true, false);
        identityService.ConfigureTwoFactorAsync("user-id", true, "123456", false, false, false, cancellationToken).Returns(expected);

        Response<TwoFactorResponse> result = await handler.Handle(
            new ConfigureTwoFactorCommand("user-id", true, "123456", false, false, false),
            cancellationToken);

        result.Succeeded.Should().BeTrue();
        result.Data.Should().Be(expected);
    }
}
