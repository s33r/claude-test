namespace Erin.Core;

/// <summary>
/// An immutable object that specifies a date according to its year, ISO Week, and day offset.
/// Provides easy conversion between ISODateIndex objects and standard DateTime objects.
/// </summary>
/// <remarks>
/// ISO 8601 week date format:
/// - Weeks start on Monday (day offset 0)
/// - Week 1 is the week containing the first Thursday of the year
/// - Day offset ranges from 0 (Monday) to 6 (Sunday)
/// </remarks>
public sealed class ISODateIndex : IEquatable<ISODateIndex>, IComparable<ISODateIndex>
{
    /// <summary>
    /// Gets the ISO year.
    /// </summary>
    public int Year { get; }

    /// <summary>
    /// Gets the ISO week number (1-53).
    /// </summary>
    public int Week { get; }

    /// <summary>
    /// Gets the day offset within the week (0 = Monday, 6 = Sunday).
    /// </summary>
    public int DayOffset { get; }

    /// <summary>
    /// Initializes a new instance of the ISODateIndex class.
    /// </summary>
    /// <param name="year">The ISO year.</param>
    /// <param name="week">The ISO week number (1-53).</param>
    /// <param name="dayOffset">The day offset (0 = Monday, 6 = Sunday).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when week is not between 1 and 53, or dayOffset is not between 0 and 6.
    /// </exception>
    public ISODateIndex(int year, int week, int dayOffset)
    {
        if (week < 1 || week > 53)
        {
            throw new ArgumentOutOfRangeException(nameof(week), week, "Week must be between 1 and 53.");
        }

        if (dayOffset < 0 || dayOffset > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(dayOffset), dayOffset, "Day offset must be between 0 (Monday) and 6 (Sunday).");
        }

        Year = year;
        Week = week;
        DayOffset = dayOffset;

        // Validate that the week exists in the given year
        int maxWeeksInYear = GetWeeksInYear(year);
        if (week > maxWeeksInYear)
        {
            throw new ArgumentOutOfRangeException(nameof(week), week, $"Year {year} has only {maxWeeksInYear} ISO weeks.");
        }
    }

    /// <summary>
    /// Creates an ISODateIndex from a DateTime object.
    /// </summary>
    /// <param name="date">The DateTime to convert.</param>
    /// <returns>A new ISODateIndex representing the date.</returns>
    public static ISODateIndex FromDateTime(DateTime date)
    {
        (int year, int week) = GetISOWeekAndYear(date);
        int dayOffset = GetDayOffset(date.DayOfWeek);
        return new ISODateIndex(year, week, dayOffset);
    }

    /// <summary>
    /// Converts this ISODateIndex to a DateTime object.
    /// </summary>
    /// <returns>The DateTime representation of this ISO date.</returns>
    public DateTime ToDateTime()
    {
        // Find January 4th of the ISO year (always in week 1)
        DateTime jan4 = new DateTime(Year, 1, 4);

        // Find the Monday of week 1
        int jan4DayOffset = GetDayOffset(jan4.DayOfWeek);
        DateTime week1Monday = jan4.AddDays(-jan4DayOffset);

        // Calculate the target date
        int daysToAdd = (Week - 1) * 7 + DayOffset;
        return week1Monday.AddDays(daysToAdd);
    }

    /// <summary>
    /// Gets the number of ISO weeks in a given year.
    /// </summary>
    /// <param name="year">The year to check.</param>
    /// <returns>Either 52 or 53.</returns>
    public static int GetWeeksInYear(int year)
    {
        // A year has 53 weeks if:
        // - Thursday falls on December 31st, or
        // - Wednesday falls on December 31st in a leap year
        DateTime dec31 = new DateTime(year, 12, 31);
        (int isoYear, int week) = GetISOWeekAndYear(dec31);

        if (isoYear == year)
        {
            return week;
        }

        return 52;
    }

    /// <summary>
    /// Converts a DayOfWeek to an ISO day offset (0 = Monday, 6 = Sunday).
    /// </summary>
    private static int GetDayOffset(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => 0,
            DayOfWeek.Tuesday => 1,
            DayOfWeek.Wednesday => 2,
            DayOfWeek.Thursday => 3,
            DayOfWeek.Friday => 4,
            DayOfWeek.Saturday => 5,
            DayOfWeek.Sunday => 6,
            _ => throw new ArgumentException("Invalid day of week.", nameof(dayOfWeek))
        };
    }

    /// <summary>
    /// Gets the ISO week number and year for a given date.
    /// </summary>
    private static (int year, int week) GetISOWeekAndYear(DateTime date)
    {
        // ISO 8601: Week 1 is the week containing the first Thursday
        // Find which Thursday belongs to this week
        int dayOffset = GetDayOffset(date.DayOfWeek);
        DateTime thursday = date.AddDays(3 - dayOffset);

        int isoYear = thursday.Year;

        // Find January 4th (always in week 1)
        DateTime jan4 = new DateTime(isoYear, 1, 4);
        int jan4Offset = GetDayOffset(jan4.DayOfWeek);
        DateTime week1Monday = jan4.AddDays(-jan4Offset);

        // Calculate week number
        int daysDiff = (int)(date - week1Monday).TotalDays;
        int weekNumber = (daysDiff / 7) + 1;

        return (isoYear, weekNumber);
    }

    /// <summary>
    /// Adds a specified number of days to this ISODateIndex.
    /// </summary>
    /// <param name="days">The number of days to add.</param>
    /// <returns>A new ISODateIndex representing the resulting date.</returns>
    public ISODateIndex AddDays(int days)
    {
        DateTime dt = ToDateTime().AddDays(days);
        return FromDateTime(dt);
    }

    /// <summary>
    /// Adds a specified number of weeks to this ISODateIndex.
    /// </summary>
    /// <param name="weeks">The number of weeks to add.</param>
    /// <returns>A new ISODateIndex representing the resulting date.</returns>
    public ISODateIndex AddWeeks(int weeks)
    {
        return AddDays(weeks * 7);
    }

    public bool Equals(ISODateIndex? other)
    {
        if (other is null) return false;
        return Year == other.Year && Week == other.Week && DayOffset == other.DayOffset;
    }

    public override bool Equals(object? obj)
    {
        return obj is ISODateIndex other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Year, Week, DayOffset);
    }

    public int CompareTo(ISODateIndex? other)
    {
        if (other is null) return 1;

        int yearComparison = Year.CompareTo(other.Year);
        if (yearComparison != 0) return yearComparison;

        int weekComparison = Week.CompareTo(other.Week);
        if (weekComparison != 0) return weekComparison;

        return DayOffset.CompareTo(other.DayOffset);
    }

    public override string ToString()
    {
        string dayName = DayOffset switch
        {
            0 => "Mon",
            1 => "Tue",
            2 => "Wed",
            3 => "Thu",
            4 => "Fri",
            5 => "Sat",
            6 => "Sun",
            _ => "???"
        };

        return $"{Year}-W{Week:D2}-{DayOffset} ({dayName})";
    }

    public static bool operator ==(ISODateIndex? left, ISODateIndex? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(ISODateIndex? left, ISODateIndex? right)
    {
        return !(left == right);
    }

    public static bool operator <(ISODateIndex? left, ISODateIndex? right)
    {
        return left is null ? right is not null : left.CompareTo(right) < 0;
    }

    public static bool operator <=(ISODateIndex? left, ISODateIndex? right)
    {
        return left is null || left.CompareTo(right) <= 0;
    }

    public static bool operator >(ISODateIndex? left, ISODateIndex? right)
    {
        return left is not null && left.CompareTo(right) > 0;
    }

    public static bool operator >=(ISODateIndex? left, ISODateIndex? right)
    {
        return left is null ? right is null : left.CompareTo(right) >= 0;
    }
}
