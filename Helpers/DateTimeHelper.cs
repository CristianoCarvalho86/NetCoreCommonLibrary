using System.Globalization;

namespace NetCoreCommonLibrary.Helpers;

/// <summary>
/// Provides helper methods for working with dates and times in .NET applications.
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Formats a DateTime as a short date string (yyyy-MM-dd).
    /// </summary>
    public static string FormatShortDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Formats a DateTime as a long date string (e.g., "Monday, January 1, 2023").
    /// </summary>
    public static string FormatLongDate(DateTime date)
    {
        return date.ToString("D"); // Culture-specific long date pattern
    }

    /// <summary>
    /// Formats a DateTime as a short time string (HH:mm).
    /// </summary>
    public static string FormatShortTime(DateTime date)
    {
        return date.ToString("HH:mm");
    }

    /// <summary>
    /// Formats a DateTime as a long time string (HH:mm:ss).
    /// </summary>
    public static string FormatLongTime(DateTime date)
    {
        return date.ToString("HH:mm:ss");
    }

    /// <summary>
    /// Formats a DateTime as a full date and time string (yyyy-MM-dd HH:mm).
    /// </summary>
    public static string FormatDateTime(DateTime date)
    {
        return date.ToString("yyyy-MM-dd HH:mm");
    }

    /// <summary>
    /// Formats a TimeSpan as a human-readable string.
    /// </summary>
    public static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
        {
            return $"{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m";
        }
        else if (timeSpan.TotalHours >= 1)
        {
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m";
        }
        else if (timeSpan.TotalMinutes >= 1)
        {
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
        else
        {
            return $"{timeSpan.Seconds}s";
        }
    }

    /// <summary>
    /// Returns a relative time string (e.g., "2 hours ago", "yesterday").
    /// </summary>
    public static string GetRelativeTime(DateTime date)
    {
        var now = DateTime.UtcNow; // Use UtcNow for consistency
        var timeSpan = now - date.ToUniversalTime();

        if (timeSpan.TotalMinutes < 1)
        {
            return "just now";
        }
        else if (timeSpan.TotalMinutes < 60)
        {
            return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 1.5 ? "s" : "")} ago";
        }
        else if (timeSpan.TotalHours < 24)
        {
            return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 1.5 ? "s" : "")} ago";
        }
        else if (timeSpan.TotalDays < 7)
        {
            if ((int)timeSpan.TotalDays == 1)
            {
                return "yesterday";
            }
            return $"{(int)timeSpan.TotalDays} days ago";
        }
        else if (timeSpan.TotalDays < 30.44) // Average days in month
        {
            int weeks = (int)(timeSpan.TotalDays / 7);
            return $"{weeks} week{(weeks > 1 ? "s" : "")} ago";
        }
        else if (timeSpan.TotalDays < 365.25) // Average days in year
        {
            int months = (int)(timeSpan.TotalDays / 30.44);
            return $"{months} month{(months > 1 ? "s" : "")} ago";
        }
        else
        {
            int years = (int)(timeSpan.TotalDays / 365.25);
            return $"{years} year{(years > 1 ? "s" : "")} ago";
        }
    }

    /// <summary>
    /// Checks if a date is today (based on local time).
    /// </summary>
    public static bool IsToday(DateTime date)
    {
        return date.Date == DateTime.Today;
    }

     /// <summary>
    /// Checks if a date is today (based on UTC time).
    /// </summary>
    public static bool IsTodayUtc(DateTime date)
    {
        return date.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Gets the current week number of the year according to ISO 8601 standard.
    /// </summary>
    public static int GetIso8601WeekNumber(DateTime date)
    {
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday) {
            date = date.AddDays(3);
        }
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    /// <summary>
    /// Gets the first day of the week (Monday) for a given date.
    /// </summary>
    public static DateTime GetFirstDayOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Gets the last day of the week (Sunday) for a given date.
    /// </summary>
    public static DateTime GetLastDayOfWeek(DateTime date)
    {
        int diff = (7 - (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(diff).Date; // Sunday is 6 days after Monday
    }

    /// <summary>
    /// Gets the first day of the month for a given date.
    /// </summary>
    public static DateTime GetFirstDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind);
    }

    /// <summary>
    /// Gets the last day of the month for a given date.
    /// </summary>
    public static DateTime GetLastDayOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59, 999, date.Kind);
    }
}
