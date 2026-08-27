using Localizr.Infrastructure.Localization.Services;

namespace Localizr.UnitTests.Localization;

/// <summary>Contains unit tests for the translation usage tracker.</summary>
public sealed class GoogleTranslateUsageTrackerTests
{
    /// <summary>Verifies positive usage is accumulated.</summary>
    [Fact]
    public void RecordUsage_WhenPositive_ShouldAccumulateCharacters()
    {
        GoogleTranslateUsageTracker tracker = new();

        tracker.RecordUsage(10);
        tracker.RecordUsage(25);

        Assert.Equal(35, tracker.GetUsedCharacters());
    }

    /// <summary>Verifies non-positive usage does not change the accumulated count.</summary>
    [Fact]
    public void RecordUsage_WhenNonPositive_ShouldIgnoreValue()
    {
        GoogleTranslateUsageTracker tracker = new();

        tracker.RecordUsage(10);
        tracker.RecordUsage(0);
        tracker.RecordUsage(-5);

        Assert.Equal(10, tracker.GetUsedCharacters());
    }
}
