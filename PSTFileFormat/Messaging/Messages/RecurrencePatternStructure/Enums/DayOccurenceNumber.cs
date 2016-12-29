
namespace PSTFileFormat
{
    public enum DayOccurenceNumber : ushort
    {
        NotApplicable = 0, // We need this for serialization
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4,
        Last = 5,
    }
}
