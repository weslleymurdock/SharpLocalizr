using Localizr.Application.Common.Responses;
using Localizr.Application.Identity.Responses;

namespace Localizr.UnitTests.Common;

/// <summary>Contains tests for common application response factories.</summary>
public sealed class ResponseTests
{
    /// <summary>Verifies successful responses preserve data and contain no errors.</summary>
    [Fact]
    public void Success_ShouldReturnSuccessfulResponse()
    {
        Response<int> response = Response.Success(42);

        Assert.True(response.Succeeded);
        Assert.Equal(42, response.Data);
        Assert.Empty(response.Errors);
    }

    /// <summary>Verifies enumerable failures preserve all error messages.</summary>
    [Fact]
    public void FailureEnumerable_ShouldReturnErrors()
    {
        Response<int> response = Response.Failure<int>(new[] { "first", "second" });

        Assert.False(response.Succeeded);
        Assert.Null(response.Data);
        Assert.Equal(["first", "second"], response.Errors);
    }

    /// <summary>Verifies parameter-array failures preserve all error messages.</summary>
    [Fact]
    public void FailureParams_ShouldReturnErrors()
    {
        Response<int> response = Response.Failure<int>("first", "second");

        Assert.False(response.Succeeded);
        Assert.Null(response.Data);
        Assert.Equal(["first", "second"], response.Errors);
    }

    /// <summary>Verifies identity success and failure factories.</summary>
    [Fact]
    public void IdentityResultFactories_ShouldCreateExpectedResults()
    {
        IdentityResultResponse success = IdentityResultResponse.Success();
        IdentityResultResponse failure = IdentityResultResponse.Failure(["error"]);

        Assert.True(success.Succeeded);
        Assert.Empty(success.Errors);
        Assert.False(failure.Succeeded);
        Assert.Equal(["error"], failure.Errors);
    }
}
