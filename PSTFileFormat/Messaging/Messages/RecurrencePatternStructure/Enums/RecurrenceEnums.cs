using System;

namespace PSTFileFormat
{
    public enum RecurrenceFrequency : ushort
    {
        Daily = 0x200A,
        Weekly = 0x200B,
        Monthly = 0x200C,
        Yearly = 0x200D,
    }

    public enum PatternType : ushort
    {
        Day = 0x0000,
        Week = 0x0001,
        Month = 0x0002,
        MonthNth = 0x03, // i.e. the fourth monday of every month / the fourth monday of may
    }

    public enum RecurrenceEndType : uint
    {
        EndAfterDate = 0x00002021,
        EndAfterNOccurrences = 0x00002022,
        NeverEnd = 0x00002023,
    }
}
