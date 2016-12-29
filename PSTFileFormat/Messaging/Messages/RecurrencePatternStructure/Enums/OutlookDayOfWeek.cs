
namespace PSTFileFormat
{
    public enum OutlookDayOfWeek : uint
    {
        Sunday = 0x01,
        Monday = 0x02,
        Tuesday = 0x04,
        Wednesday = 0x08,
        Thursday = 0x10,
        Friday = 0x20,
        Saturday = 0x40,
        Weekday = 0x3E,    // e.g. the fourth weekday
        WeekendDay = 0x41, // e.g. the fourth weekend day
        Day = 0x7F,        // e.g. the fourth day
    }
}
