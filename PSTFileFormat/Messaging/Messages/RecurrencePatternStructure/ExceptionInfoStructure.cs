/* Copyright (C) 2012-2016 ROM Knowledgeware. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * 
 * Maintainer: Tal Aloni <tal@kmrom.com>
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;

namespace PSTFileFormat
{
    // http://msdn.microsoft.com/en-us/library/ee157416%28v=exchg.80%29
    [Flags]
    public enum OverrideFlags : ushort
    { 
        ModifiedSubject = 0x0001,
        ModifiedStateFlags = 0x0002, // with other people
        ModifiedReminderTime = 0x0004,
        ModifiedIsReminderSet = 0x0008,
        ModifiedLocation = 0x0010,
        ModifiedBustStatus = 0x0020,
        ModifiedHasAttachment = 0x0040,
        ModifiedIsAllDayEvent = 0x0080,
        ModifiedColor = 0x0100,
        ModifiedBody = 0x0200,
    }

    public class ExceptionInfoStructure
    {
        public DateTime NewStartDT;      // In timezone time
        public DateTime NewEndDT;        // In timezone time
        public DateTime OriginalStartDT; // In timezone time, this field contains both date and time!
        public OverrideFlags Flags;
        public string Subject = String.Empty;
        public uint AppointmentStateFlags; // PidLidAppointmentStateFlags
        public uint ReminderTime; // In minutes
        public bool IsReminderSet;
        public string Location = String.Empty;
        public BusyStatus BusyStatus;
        public bool HasAttachment;
        public bool IsAllDayEvent; // PidLidAppointmentSubType
        public uint Color;

        public ExceptionInfoStructure()
        { 
        }

        public ExceptionInfoStructure(byte[] buffer, int offset)
        {
            int position = offset;
            NewStartDT = DateTimeHelper.ReadDateTimeFromMinutes(buffer, ref position);
            NewEndDT = DateTimeHelper.ReadDateTimeFromMinutes(buffer, ref position);
            OriginalStartDT = DateTimeHelper.ReadDateTimeFromMinutes(buffer, ref position);
            Flags = (OverrideFlags)LittleEndianReader.ReadUInt16(buffer, ref position);

            if (HasModifiedSubject)
            {
                ushort fieldLength = LittleEndianReader.ReadUInt16(buffer, ref position);
                ushort numberOfCharacters = LittleEndianReader.ReadUInt16(buffer, ref position);
                Subject = Encoding.Default.GetString(buffer, position, numberOfCharacters);
                position += numberOfCharacters;
            }

            if (HasModifiedStateFlags)
            {
                AppointmentStateFlags = LittleEndianReader.ReadUInt32(buffer, ref position);
            }

            if (HasModifiedReminderTime)
            {
                ReminderTime = LittleEndianReader.ReadUInt32(buffer, ref position);
            }

            if (HasModifiedIsReminderSet)
            {
                IsReminderSet = LittleEndianReader.ReadUInt32(buffer, ref position) == 0;
            }

            if (HasModifiedLocation)
            {
                ushort fieldLength = LittleEndianReader.ReadUInt16(buffer, ref position);
                ushort numberOfCharacters = LittleEndianReader.ReadUInt16(buffer, ref position);
                Location = Encoding.Default.GetString(buffer, position, numberOfCharacters);
                position += numberOfCharacters;
            }

            if (HasModifiedBusyStatus)
            {
                BusyStatus = (BusyStatus)LittleEndianReader.ReadUInt32(buffer, ref position);
            }

            if (HasModifiedHasAttachment)
            {
                HasAttachment = LittleEndianReader.ReadUInt32(buffer, ref position) == 0;
            }

            if (HasModifiedIsAllDayEvent)
            {
                IsAllDayEvent = LittleEndianReader.ReadUInt32(buffer, ref position) == 0;
            }

            if (HasModifiedColor)
            {
                Color = LittleEndianReader.ReadUInt32(buffer, ref position);
            }
        }

        public void WriteBytes(Stream stream)
        {
            DateTimeHelper.WriteDateTimeInMinutes(stream, NewStartDT);
            DateTimeHelper.WriteDateTimeInMinutes(stream, NewEndDT);
            DateTimeHelper.WriteDateTimeInMinutes(stream, OriginalStartDT);
            LittleEndianWriter.WriteUInt16(stream, (ushort)Flags);

            if (HasModifiedSubject)
            {
                LittleEndianWriter.WriteUInt16(stream, (ushort)(Subject.Length + 1));
                LittleEndianWriter.WriteUInt16(stream, (ushort)Subject.Length);
                ByteWriter.WriteAnsiString(stream, Subject, Subject.Length);
            }

            if (HasModifiedStateFlags)
            {
                LittleEndianWriter.WriteUInt32(stream, AppointmentStateFlags);
            }

            if (HasModifiedReminderTime)
            {
                LittleEndianWriter.WriteUInt32(stream, ReminderTime);
            }

            if (HasModifiedIsReminderSet)
            {
                LittleEndianWriter.WriteUInt32(stream, Convert.ToUInt32(IsReminderSet));
            }

            if (HasModifiedLocation)
            {
                LittleEndianWriter.WriteUInt16(stream, (ushort)(Location.Length + 1));
                LittleEndianWriter.WriteUInt16(stream, (ushort)Location.Length);
                ByteWriter.WriteAnsiString(stream, Location, Location.Length);
            }

            if (HasModifiedBusyStatus)
            {
                LittleEndianWriter.WriteUInt32(stream, (uint)BusyStatus);
            }

            if (HasModifiedHasAttachment)
            {
                LittleEndianWriter.WriteUInt32(stream, Convert.ToUInt32(HasAttachment));
            }

            if (HasModifiedIsAllDayEvent)
            {
                LittleEndianWriter.WriteUInt32(stream, Convert.ToUInt32(IsAllDayEvent));
            }

            if (HasModifiedColor)
            {
                LittleEndianWriter.WriteUInt32(stream, Color);
            }
        }

        // ExtendedException Structure:
        // http://msdn.microsoft.com/en-us/library/ee159855%28v=exchg.80%29
        public void ReadExtendedException(byte[] buffer, ref int offset, uint writerVersion2)
        {
            if (writerVersion2 >= AppointmentRecurrencePatternStructure.Outlook2007VersionSignature)
            {
                // Change highlight structure:
                // http://msdn.microsoft.com/en-us/library/ee202316%28v=exchg.80%29
                uint changeHighlightSize = LittleEndianReader.ReadUInt32(buffer, ref offset);
                if (changeHighlightSize < 4)
                {
                    throw new InvalidRecurrencePatternException("Invalid ChangeHighlightSize");
                }
                uint changeHighlightValue = LittleEndianReader.ReadUInt32(buffer, ref offset);
                offset += (int)changeHighlightSize - 4;
            }

            uint reservedBlockEE1Size = LittleEndianReader.ReadUInt32(buffer, ref offset);
            offset += (int)reservedBlockEE1Size;

            if (HasModifiedSubject || HasModifiedLocation)
            {
                DateTime extensionNewStartDT = DateTimeHelper.ReadDateTimeFromMinutes(buffer, ref offset);
                DateTime extensionNewEndDT = DateTimeHelper.ReadDateTimeFromMinutes(buffer, ref offset);
                DateTime extensionOriginalStartDT = DateTimeHelper.ReadDateTimeFromMinutes(buffer, ref offset);
                if (extensionNewStartDT != NewStartDT ||
                    extensionNewEndDT != NewEndDT ||
                    extensionOriginalStartDT != OriginalStartDT)
                {
                    throw new InvalidRecurrencePatternException("Extension timestamp mismatch");
                }

                if (HasModifiedSubject)
                {
                    ushort numberOfCharacters = LittleEndianReader.ReadUInt16(buffer, ref offset);
                    Subject = ByteReader.ReadUTF16String(buffer, ref offset, numberOfCharacters);
                }

                if (HasModifiedLocation)
                {
                    ushort numberOfCharacters = LittleEndianReader.ReadUInt16(buffer, ref offset);
                    Location = ByteReader.ReadUTF16String(buffer, ref offset, numberOfCharacters);
                }

                uint reservedBlockEE2Size = LittleEndianReader.ReadUInt32(buffer, ref offset);
                offset += (int)reservedBlockEE2Size;
            }
        }

        // ExtendedException Structure:
        // http://msdn.microsoft.com/en-us/library/ee159855%28v=exchg.80%29
        public void WriteExtendedException(Stream stream, WriterCompatibilityMode writerCompatibilityMode)
        {
            if (writerCompatibilityMode >= WriterCompatibilityMode.Outlook2007RTM)
            {
                // Change highlight structure:
                uint changeHighlightSize = 4;
                LittleEndianWriter.WriteUInt32(stream, changeHighlightSize);
                int changeHighlightValue = 0; // Apparently Outlook 2010 uses 0 regardless of the actual changes
                LittleEndianWriter.WriteInt32(stream, changeHighlightValue);
            }

            uint reservedBlockEE1Size = 0;
            LittleEndianWriter.WriteUInt32(stream, reservedBlockEE1Size);

            if (HasModifiedSubject || HasModifiedLocation)
            {
                DateTimeHelper.WriteDateTimeInMinutes(stream, NewStartDT);
                DateTimeHelper.WriteDateTimeInMinutes(stream, NewEndDT);
                DateTimeHelper.WriteDateTimeInMinutes(stream, OriginalStartDT);

                if (HasModifiedSubject)
                {
                    LittleEndianWriter.WriteUInt16(stream, (ushort)Subject.Length);
                    ByteWriter.WriteBytes(stream, Encoding.Unicode.GetBytes(Subject));
                }

                if (HasModifiedLocation)
                {
                    LittleEndianWriter.WriteUInt16(stream, (ushort)Location.Length);
                    ByteWriter.WriteBytes(stream, Encoding.Unicode.GetBytes(Location));
                }
                uint reservedBlockEE2Size = 0;
                LittleEndianWriter.WriteUInt32(stream, reservedBlockEE2Size);
            }
        }

        public void SetStartAndDuration(DateTime startDTUtc, int duration, TimeZoneInfo timezone)
        {
            NewStartDT = TimeZoneInfo.ConvertTimeFromUtc(startDTUtc, timezone);
            NewEndDT = NewStartDT.AddMinutes(duration);
        }

        public bool HasModifiedSubject
        {
            get
            {
                return (Flags & OverrideFlags.ModifiedSubject) > 0;
            }
            set
            {
                Flags &= ~OverrideFlags.ModifiedSubject;
                if (value)
                {
                    Flags |= OverrideFlags.ModifiedSubject;
                }
            }
        }

        public bool HasModifiedStateFlags
        {
            get
            {
                return (Flags & OverrideFlags.ModifiedStateFlags) > 0;
            }
        }

        public bool HasModifiedReminderTime
        {
            get
            {
                return (Flags & OverrideFlags.ModifiedReminderTime) > 0;
            }
        }

        public bool HasModifiedIsReminderSet
        {
            get
            {
                return (Flags & OverrideFlags.ModifiedIsReminderSet) > 0;
            }
        }

        public bool HasModifiedLocation
        {
            get
            {
                return (Flags & OverrideFlags.ModifiedLocation) > 0;
            }
            set
            {
                Flags &= ~OverrideFlags.ModifiedLocation;
                if (value)
                {
                    Flags |= OverrideFlags.ModifiedLocation;
                }
            }
        }

        public bool HasModifiedBusyStatus
        {
            get
            {
                return (Flags & OverrideFlags.ModifiedBustStatus) > 0;
            }
            set
            {
                Flags &= ~OverrideFlags.ModifiedBustStatus;
                if (value)
                {
                    Flags |= OverrideFlags.ModifiedBustStatus;
                }
            }
        }

        public bool HasModifiedHasAttachment
        {
            get
            {
                return (Flags & OverrideFlags.ModifiedHasAttachment) > 0;
            }
        }

        public bool HasModifiedIsAllDayEvent
        {
            get
            {
                return (Flags & OverrideFlags.ModifiedIsAllDayEvent) > 0;
            }
        }

        public bool HasModifiedColor
        {
            get
            {
                return (Flags & OverrideFlags.ModifiedColor) > 0;
            }
            set
            {
                Flags &= ~OverrideFlags.ModifiedColor;
                if (value)
                {
                    Flags |= OverrideFlags.ModifiedColor;
                }
            }
        }

        public bool HasExtendedException
        {
            get
            {
                return (HasModifiedSubject || HasModifiedLocation);
            }
        }

        public int RecordLength
        {
            get
            {
                int length = 14;
                if (HasModifiedSubject)
                {
                    length += 4;
                    length += Subject.Length;
                }
                if (HasModifiedStateFlags)
                {
                    length += 4;
                }
                if (HasModifiedReminderTime)
                {
                    length += 4;
                }
                if (HasModifiedIsReminderSet)
                {
                    length += 4;
                }
                if (HasModifiedLocation)
                {
                    length += 4;
                    length += Location.Length;
                }
                if (HasModifiedBusyStatus)
                {
                    length += 4;
                }
                if (HasModifiedHasAttachment)
                {
                    length += 4;
                }
                if (HasModifiedIsAllDayEvent)
                {
                    length += 4;
                }
                if (HasModifiedColor)
                {
                    length += 4;
                }
                return length;
            }
        }
        
        [Obsolete]
        public int GetExtendedExceptionLength(WriterCompatibilityMode writerCompatibilityMode)
        {
            int length = 4;
            if (HasExtendedException)
            {
                if (writerCompatibilityMode >= WriterCompatibilityMode.Outlook2007RTM)
                {
                    length += 8;
                }
                length += 16;
                if (HasModifiedSubject)
                {
                    length += 2;
                    length += Subject.Length * 2;
                }
                if (HasModifiedLocation)
                {
                    length += 2;
                    length += Location.Length * 2;
                }
            }
            return length;
        }

        public DateTime GetOriginalStartDTUtc(TimeZoneInfo timezone)
        {
            // Outlook 2007 SP3 can store invalid OriginalStartDT
            return TimeZoneInfoUtils.SafeConvertToUtc(OriginalStartDT, timezone);
        }

        public void SetOriginalStartDTUtc(DateTime originalStartDTUtc, TimeZoneInfo timezone)
        {
            OriginalStartDT = TimeZoneInfo.ConvertTimeFromUtc(originalStartDTUtc, timezone);
        }

        public DateTime GetNewStartDTUtc(TimeZoneInfo timezone)
        {
            // Outlook 2007 SP3 can store invalid NewStartDT
            return TimeZoneInfoUtils.SafeConvertToUtc(NewStartDT, timezone);
        }

        public void SetNewStartDTUtc(DateTime newStartDTUtc, TimeZoneInfo timezone)
        {
            NewStartDT = TimeZoneInfo.ConvertTimeFromUtc(newStartDTUtc, timezone);
        }

        public DateTime GetNewEndDTUtc(TimeZoneInfo timezone)
        {
            // Outlook 2007 SP3 can store invalid NewEndDT
            return TimeZoneInfoUtils.SafeConvertToUtc(NewEndDT, timezone);
        }

        /// <summary>
        /// The timespan in minutes from the start time to end time in timezone time.
        /// It will differ from the actual duration (UTC) if an appointment instance is spanning both standard time and daylight time.
        /// </summary>
        public int Duration
        {
            get
            {
                TimeSpan ts = NewEndDT - NewStartDT;
                return (int)ts.TotalMinutes;
            }
        }

        /// <summary>
        /// This should be further improved
        /// </summary>
        // http://msdn.microsoft.com/en-us/library/ee204210%28v=exchg.80%29
        public int ChangeHighlight
        {
            get
            { 
                int result = 0;
                if (HasModifiedLocation)
                {
                    result |= 0x08;
                }
                if (HasModifiedSubject)
                {
                    result |= 0x10;
                }
                return result;
            }
        }
    }
}
