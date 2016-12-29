using System;

namespace PSTFileFormat
{
    /// <summary>
    /// This enum represents the combinations of RecurrenceFrequency and PatternType
    /// </summary>
    public enum RecurrenceType : ushort
    {
        None = 0,
        EveryNDays = 1,
        EveryWeekday = 2,
        EveryNWeeks = 3,
        EveryNMonths = 4,
        EveryNthDayOfEveryNMonths = 5,
        EveryNYears = 6,              // Outlook 2007 GUI is no longer limited to 'every year' after applying KB950219 or SP2
        EveryNthDayOfEveryNYears = 7, // Outlook 2007 GUI is no longer limited to 'every year' after applying KB950219 or SP2
    }
}
