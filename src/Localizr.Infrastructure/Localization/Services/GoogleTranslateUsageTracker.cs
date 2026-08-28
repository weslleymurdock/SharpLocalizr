namespace Localizr.Infrastructure.Localization.Services;

/// <summary>Tracks translation characters consumed by the current application instance.</summary>
public sealed class GoogleTranslateUsageTracker
{
    private readonly Lock _sync = new();
    private DateOnly _month = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
    private long _usedCharacters;

    /// <summary>Records characters submitted to Google Cloud Translation.</summary>
    /// <param name="characterCount">The number of characters submitted.</param>
    public void RecordUsage(long characterCount)
    {
        if (characterCount <= 0)
        {
            return;
        }

        lock (_sync)
        {
            EnsureCurrentMonth();
            _usedCharacters += characterCount;
        }
    }

    /// <summary>Gets the number of characters consumed by this application instance in the current month.</summary>
    /// <returns>The number of characters recorded.</returns>
    public long GetUsedCharacters()
    {
        lock (_sync)
        {
            EnsureCurrentMonth();
            return _usedCharacters;
        }
    }

    private void EnsureCurrentMonth()
    {
        DateOnly currentMonth = new(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        if (_month == currentMonth)
        {
            return;
        }

        _month = currentMonth;
        _usedCharacters = 0;
    }
}
