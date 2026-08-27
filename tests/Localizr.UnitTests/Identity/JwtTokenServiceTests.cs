using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Localizr.Infrastructure.Identity.Options;
using Localizr.Infrastructure.Identity.Services;
using Microsoft.Extensions.Options;

namespace Localizr.UnitTests.Identity;

/// <summary>Contains unit tests for JWT creation, validation, and revocation.</summary>
public sealed class JwtTokenServiceTests
{
    private static readonly JwtOptions JwtSettings = new()
    {
        Key = "01234567890123456789012345678901",
        Issuer = "SharpLocalizr.Tests",
        Audience = "SharpLocalizr.Tests",
        AccessTokenLifetime = TimeSpan.FromMinutes(10),
        RefreshTokenLifetime = TimeSpan.FromHours(1)
    };

    /// <summary>Verifies access and refresh tokens contain the expected claims.</summary>
    [Fact]
    public void CreateTokens_ShouldCreateSignedAccessAndRefreshTokens()
    {
        JwtTokenService service = CreateService(new RevokedTokenStore());
        var response = service.CreateTokens("user-1", "user@example.com", ["Admin", "User"], [new Claim("custom", "value")]);

        Assert.Equal("Bearer", response.TokenType);
        Assert.True(response.ExpiresIn > 0);
        Assert.NotEmpty(response.AccessToken);
        Assert.NotEmpty(response.RefreshToken);

        JwtSecurityToken access = new JwtSecurityTokenHandler().ReadJwtToken(response.AccessToken);
        JwtSecurityToken refresh = new JwtSecurityTokenHandler().ReadJwtToken(response.RefreshToken);
        Assert.Equal(JwtTokenTypes.Access, access.Claims.Single(x => x.Type == "token_type").Value);
        Assert.Equal(JwtTokenTypes.Refresh, refresh.Claims.Single(x => x.Type == "token_type").Value);
        Assert.Contains(access.Claims, x => x.Type == ClaimTypes.Role && x.Value == "Admin");
        Assert.Contains(access.Claims, x => x.Type == "custom" && x.Value == "value");
    }

    /// <summary>Verifies a valid token produces its claims principal.</summary>
    [Fact]
    public void ValidateToken_WhenTokenIsValid_ShouldReturnPrincipal()
    {
        JwtTokenService service = CreateService(new RevokedTokenStore());
        var tokens = service.CreateTokens("user-1", "user@example.com", [], []);
        ClaimsPrincipal? principal = service.ValidateToken(tokens.AccessToken);

        Assert.NotNull(principal);
        Assert.Equal("user-1", principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub));
    }

    /// <summary>Verifies empty tokens are rejected.</summary>
    [Fact]
    public void ValidateToken_WhenTokenIsEmpty_ShouldReturnNull() => Assert.Null(CreateService(new RevokedTokenStore()).ValidateToken(string.Empty));

    /// <summary>Verifies malformed tokens are rejected.</summary>
    [Fact]
    public void ValidateToken_WhenTokenIsMalformed_ShouldReturnNull() => Assert.Null(CreateService(new RevokedTokenStore()).ValidateToken("not-a-jwt"));

    /// <summary>Verifies tokens signed with the wrong key are rejected.</summary>
    [Fact]
    public void ValidateToken_WhenSignatureIsInvalid_ShouldReturnNull()
    {
        JwtTokenService source = CreateService(new RevokedTokenStore());
        var tokens = source.CreateTokens("user-1", "user@example.com", [], []);
        JwtOptions otherOptions = new() { Key = "abcdefghijklmnopqrstuvwxyz012345", Issuer = JwtSettings.Issuer, Audience = JwtSettings.Audience };
        JwtTokenService service = new(Microsoft.Extensions.Options.Options.Create(otherOptions), new RevokedTokenStore());

        Assert.Null(service.ValidateToken(tokens.AccessToken));
    }

    /// <summary>Verifies tokens with an invalid issuer are rejected.</summary>
    [Fact]
    public void ValidateToken_WhenIssuerIsInvalid_ShouldReturnNull()
    {
        JwtTokenService source = CreateService(new RevokedTokenStore());
        var tokens = source.CreateTokens("user-1", "user@example.com", [], []);
        JwtOptions otherOptions = new() { Key = JwtSettings.Key, Issuer = "OtherIssuer", Audience = JwtSettings.Audience };
        JwtTokenService service = new(Microsoft.Extensions.Options.Options.Create(otherOptions), new RevokedTokenStore());

        Assert.Null(service.ValidateToken(tokens.AccessToken));
    }

    /// <summary>Verifies tokens with an invalid audience are rejected.</summary>
    [Fact]
    public void ValidateToken_WhenAudienceIsInvalid_ShouldReturnNull()
    {
        JwtTokenService source = CreateService(new RevokedTokenStore());
        var tokens = source.CreateTokens("user-1", "user@example.com", [], []);
        JwtOptions otherOptions = new() { Key = JwtSettings.Key, Issuer = JwtSettings.Issuer, Audience = "OtherAudience" };
        JwtTokenService service = new(Microsoft.Extensions.Options.Options.Create(otherOptions), new RevokedTokenStore());

        Assert.Null(service.ValidateToken(tokens.AccessToken));
    }

    /// <summary>Verifies expired tokens are rejected when lifetime validation is enabled.</summary>
    [Fact]
    public void ValidateToken_WhenTokenIsExpired_ShouldRejectLifetime()
    {
        JwtOptions options = new() { Key = JwtSettings.Key, Issuer = JwtSettings.Issuer, Audience = JwtSettings.Audience, AccessTokenLifetime = TimeSpan.FromSeconds(-1), RefreshTokenLifetime = TimeSpan.FromSeconds(-1) };
        JwtTokenService service = new(Microsoft.Extensions.Options.Options.Create(options), new RevokedTokenStore());
        var tokens = service.CreateTokens("user-1", "user@example.com", [], []);

        Assert.Null(service.ValidateToken(tokens.AccessToken));
    }

    /// <summary>Verifies lifetime validation can be disabled for an otherwise validly signed token.</summary>
    [Fact]
    public void ValidateToken_WhenLifetimeValidationIsDisabled_ShouldReturnExpiredToken()
    {
        JwtOptions options = new() { Key = JwtSettings.Key, Issuer = JwtSettings.Issuer, Audience = JwtSettings.Audience, AccessTokenLifetime = TimeSpan.FromSeconds(-1), RefreshTokenLifetime = TimeSpan.FromSeconds(-1) };
        JwtTokenService service = new(Microsoft.Extensions.Options.Options.Create(options), new RevokedTokenStore());
        var tokens = service.CreateTokens("user-1", "user@example.com", [], []);

        Assert.NotNull(service.ValidateToken(tokens.AccessToken, validateLifetime: false));
    }

    /// <summary>Verifies a revoked token is rejected even when its signature is valid.</summary>
    [Fact]
    public void ValidateToken_WhenTokenIsRevoked_ShouldReturnNull()
    {
        RevokedTokenStore store = new();
        JwtTokenService service = CreateService(store);
        var tokens = service.CreateTokens("user-1", "user@example.com", [], []);
        store.Revoke(service.GetTokenId(tokens.AccessToken)!, DateTimeOffset.UtcNow.AddMinutes(5));

        Assert.Null(service.ValidateToken(tokens.AccessToken));
    }

    /// <summary>Verifies token identifiers can be read from valid tokens.</summary>
    [Fact]
    public void GetTokenId_WhenTokenIsValid_ShouldReturnIdentifier()
    {
        JwtTokenService service = CreateService(new RevokedTokenStore());
        var tokens = service.CreateTokens("user-1", "user@example.com", [], []);
        Assert.NotNull(service.GetTokenId(tokens.AccessToken));
    }

    /// <summary>Verifies malformed token identifiers return null.</summary>
    [Fact]
    public void GetTokenId_WhenTokenIsInvalid_ShouldReturnNull() => Assert.Null(CreateService(new RevokedTokenStore()).GetTokenId(string.Empty));

    /// <summary>Verifies expiration can be read from a valid token.</summary>
    [Fact]
    public void GetExpiration_WhenTokenIsValid_ShouldReturnExpiration()
    {
        JwtTokenService service = CreateService(new RevokedTokenStore());
        var tokens = service.CreateTokens("user-1", "user@example.com", [], []);
        DateTimeOffset? expiration = service.GetExpiration(tokens.AccessToken);

        Assert.True(expiration.HasValue);
        Assert.True(expiration > DateTimeOffset.UtcNow);
    }

    /// <summary>Verifies malformed token expiration requests return null.</summary>
    [Fact]
    public void GetExpiration_WhenTokenIsInvalid_ShouldReturnNull() => Assert.Null(CreateService(new RevokedTokenStore()).GetExpiration(string.Empty));

    /// <summary>Verifies a weak signing key is rejected.</summary>
    [Fact]
    public void CreateTokens_WhenKeyIsTooShort_ShouldThrow()
    {
        JwtOptions options = new() { Key = "short", Issuer = JwtSettings.Issuer, Audience = JwtSettings.Audience };
        JwtTokenService service = new(Microsoft.Extensions.Options.Options.Create(options), new RevokedTokenStore());

        Assert.Throws<InvalidOperationException>(() => service.CreateTokens("user", "user@example.com", [], []));
    }

    /// <summary>Verifies active revocations are reported and expired revocations are removed.</summary>
    [Fact]
    public void RevokedTokenStore_ShouldHandleActiveAndExpiredEntries()
    {
        RevokedTokenStore store = new();
        Assert.False(store.IsRevoked("missing"));
        store.Revoke("active", DateTimeOffset.UtcNow.AddMinutes(1));
        Assert.True(store.IsRevoked("active"));
        store.Revoke("expired", DateTimeOffset.UtcNow.AddSeconds(-1));
        Assert.False(store.IsRevoked("expired"));
        store.Revoke("active", DateTimeOffset.UtcNow.AddSeconds(-1));
        Assert.False(store.IsRevoked("active"));
    }

    private static JwtTokenService CreateService(RevokedTokenStore store)
        => new(Microsoft.Extensions.Options.Options.Create(JwtSettings), store);
}
