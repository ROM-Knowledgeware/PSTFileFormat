using System;

namespace PSTFileFormat
{
    [Flags]
    public enum RecipientFlags
    {
        SendableAttendee = 0x0001,
        MeetingOrganizer = 0x0002,
    }
}
