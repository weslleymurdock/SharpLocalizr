using Bunit;
using Localizr.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace Localizr.UnitTests.Blazor;

/// <summary>Contains bUnit tests for the application home page.</summary>
public sealed class HomePageTests : BunitContext
{
    /// <summary>Initializes the MudBlazor services required by the page components.</summary>
    public HomePageTests()
    {
        Services.AddMudServices();
    }

    /// <summary>Verifies that the home page renders the application welcome message.</summary>
    [Fact]
    public void Render_ShouldDisplayWelcomeMessage()
    {
        var cut = Render<HomePage>();

        Assert.Contains("Welcome to SharpLocalizr!", cut.Markup);
    }
}
