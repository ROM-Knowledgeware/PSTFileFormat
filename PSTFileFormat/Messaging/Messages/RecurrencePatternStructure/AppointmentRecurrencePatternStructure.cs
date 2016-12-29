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
    // http://msdn.microsoft.com/en-us/library/ee237146%28v=exchg.80%29.aspx
    public abstract class AppointmentRecurrencePatternStructure
    {
        //public const ushort Outlook1997VersionSignature = 0x3006;
        //public const ushort Outlook2000VersionSignature = 0x3006;
        //public const ushort Outlook2002VersionSignature = 0x3007;
        public const ushort Outlook2003VersionSignature = 0x3008;
        public const ushort Outlook2007VersionSignature = 0x3009;
        public const ushort Outlook2010VersionSignature = 0x3009;
        
        // If the recurrence does not have an end date, the value of the EndDate field MUST be set to 0x5AE980DF
        // DateTimeKind.Local means the current timezone on the client computer, while we use the timezone specified by the appointment
        public static readonly DateTime NoEndDate = DateTimeHelper.GetDateTime(0x5AE980DF, DateTimeKind.Unspecified);

        /* Start of RecurrencePattern */
        public ushort ReaderVersion = 0x3004;
        public ushort WriterVersion = 0x3004;
        public RecurrenceFrequency RecurFrequency;
        public PatternType PatternType;
        public ushort CalendarType;
        public uint FirstDateTime;
        public uint Period;
        public uint SlidingFlag;
        // PatternTypeSpecific
        public RecurrenceEndType EndType;
        public uint OccurrenceCount;
        public uint FirstDOW; // first day of week
        // uint DeletedInstanceCount;
        public List<DateTime> DeletedInstanceDates = new List<DateTime>(); // The original start date
        // uint ModifiedInstanceCount; // NumberOfNewDates can be lower than NumberOfExceptions if an appointment has been deleted
        public List<DateTime> ModifiedInstanceDates = new List<DateTime>(); // The new start date
        private DateTime StartDate; // Timezone date
        private DateTime EndDate; // Date of the start of the last appointment in the series
        /* End of RecurrencePattern */
        public uint ReaderVersion2 = 0x3006;
        public uint WriterVersion2 = Outlook2003VersionSignature;
        private uint StartTimeOffset; // In minutes, Timezone time
        private uint EndTimeOffset; // In minutes, Timezone time
        // ushort NumberOfExceptions repeat
        public List<ExceptionInfoStructure> ExceptionList = new List<ExceptionInfoStructure>();

        public AppointmentRecurrencePatternStructure()
        { 
        }

        public AppointmentRecurrencePatternStructure(byte[] buffer)
        {
            int position = 0;
            ReaderVersion = LittleEndianReader.ReadUInt16(buffer, ref position);
            WriterVersion = LittleEndianReader.ReadUInt16(buffer, ref position);
            RecurFrequency = (RecurrenceFrequency)LittleEndianReader.ReadUInt16(buffer, ref position);
            PatternType = (PatternType)LittleEndianReader.ReadUInt16(buffer, ref position);
            CalendarType = LittleEndianReader.ReadUInt16(buffer, ref position);
            FirstDateTime = LittleEndianReader.ReadUInt32(buffer, ref position);
            Period = LittleEndianReader.ReadUInt32(buffer, ref position);
            SlidingFlag = LittleEndianReader.ReadUInt32(buffer, ref position);

            ReadPatternTypeSpecific(buffer, ref position);

            EndType = (RecurrenceEndType)LittleEndianReader.ReadUInt32(buffer, ref position);
            if ((uint)EndType == 0xFFFFFFFF) // SHOULD be 0x00002023 but can be 0xFFFFFFFF
            {
                EndType = RecurrenceEndType.NeverEnd;
            }
            OccurrenceCount = LittleEndianReader.ReadUInt32(buffer, ref position);
            FirstDOW = LittleEndianReader.ReadUInt32(buffer, ref position);
            uint DeletedInstanceCount = LittleEndianReader.ReadUInt32(buffer, ref position);
            for (int index = 0; index < DeletedInstanceCount; index++)
            {
                DateTime date = DateTimeHelper.ReadDateTimeFromMinutes(buffer, ref position);
                DeletedInstanceDates.Add(date);
            }

            uint ModifiedInstanceCount = LittleEndianReader.ReadUInt32(buffer, ref position);
            if (ModifiedInstanceCount > DeletedInstanceCount)
            {
                throw new InvalidRecurrencePatternException("Invalid structure format");
            }
            for (int index = 0; index < ModifiedInstanceCount; index++)
            {
                DateTime date = DateTimeHelper.ReadDateTimeFromMinutes(buffer, ref position);
                ModifiedInstanceDates.Add(date);
            }

            StartDate = DateTimeHelper.ReadDateTimeFromMinutes(buffer, ref position);
            // end date will be in 31/12/4500 23:59:00 if there is no end date
            EndDate = DateTimeHelper.ReadDateTimeFromMinutes(buffer, ref position);

            ReaderVersion2 = LittleEndianReader.ReadUInt32(buffer, ref position);
            WriterVersion2 = LittleEndianReader.ReadUInt32(buffer, ref position);

            StartTimeOffset = LittleEndianReader.ReadUInt32(buffer, ref position);
            EndTimeOffset = LittleEndianReader.ReadUInt32(buffer, ref position);
            ushort exceptionCount = LittleEndianReader.ReadUInt16(buffer, ref position);
            if (exceptionCount != ModifiedInstanceCount)
            {
                // This MUST be the same value as the value of the ModifiedInstanceCount in the associated ReccurencePattern structure
                throw new InvalidRecurrencePatternException("Invalid structure format");
            }
            for (int index = 0; index < exceptionCount; index++)
            {
                ExceptionInfoStructure exception = new ExceptionInfoStructure(buffer, position);
                ExceptionList.Add(exception);
                position += exception.RecordLength;
            }

            // Outlook 2003 SP3 was observed using a signature of older version (0x3006) when there was no need for extended exception
            if (WriterVersion2 >= Outlook2003VersionSignature)
            {
                uint reservedBlock1Size = LittleEndianReader.ReadUInt32(buffer, ref position);
                position += (int)reservedBlock1Size;

                foreach (ExceptionInfoStructure exception in ExceptionList)
                {
                    exception.ReadExtendedException(buffer, ref position, WriterVersion2);
                }

                uint reservedBlock2Size = LittleEndianReader.ReadUInt32(buffer, ref position);
                position += (int)reservedBlock2Size;
            }
        }

        public abstract void ReadPatternTypeSpecific(byte[] buffer, ref int offset);

        public abstract void WritePatternTypeSpecific(Stream stream);

        public byte[] GetBytes(WriterCompatibilityMode writerCompatibilityMode)
        {
            switch (writerCompatibilityMode)
            { 
                case WriterCompatibilityMode.Outlook2007RTM:
                case WriterCompatibilityMode.Outlook2007SP2:
                    WriterVersion2 = Outlook2007VersionSignature;
                    break;
                case WriterCompatibilityMode.Outlook2010RTM:
                    WriterVersion2 = Outlook2010VersionSignature;
                    break;
                default:
                    WriterVersion2 = Outlook2003VersionSignature;
                    break;
            }
            if (ModifiedInstanceDates.Count > DeletedInstanceDates.Count)
            {
                throw new InvalidRecurrencePatternException("Invalid exception data");
            }

            if (ExceptionList.Count != ModifiedInstanceDates.Count)
            {
                throw new InvalidRecurrencePatternException("Invalid exception data");
            }

            MemoryStream stream = new MemoryStream();
            LittleEndianWriter.WriteUInt16(stream, ReaderVersion);
            LittleEndianWriter.WriteUInt16(stream, WriterVersion);
            LittleEndianWriter.WriteUInt16(stream, (ushort)RecurFrequency);
            LittleEndianWriter.WriteUInt16(stream, (ushort)PatternType);
            LittleEndianWriter.WriteUInt16(stream, CalendarType);
            LittleEndianWriter.WriteUInt32(stream, FirstDateTime);
            LittleEndianWriter.WriteUInt32(stream, Period);
            LittleEndianWriter.WriteUInt32(stream, SlidingFlag);
            WritePatternTypeSpecific(stream);

            LittleEndianWriter.WriteUInt32(stream, (uint)EndType);
            LittleEndianWriter.WriteUInt32(stream, OccurrenceCount);
            LittleEndianWriter.WriteUInt32(stream, FirstDOW);
            LittleEndianWriter.WriteUInt32(stream, (uint)DeletedInstanceDates.Count);
            // [MS-OXOCAL] DeletedInstanceDates - The dates are ordered from earliest to latest
            foreach (DateTime date in ListUtils.GetSorted(DeletedInstanceDates))
            {
                DateTimeHelper.WriteDateTimeInMinutes(stream, date);
            }

            LittleEndianWriter.WriteUInt32(stream, (uint)ModifiedInstanceDates.Count);
            // [MS-OXOCAL] ModifiedInstanceDates - The dates are ordered from earliest to latest
            foreach (DateTime date in ListUtils.GetSorted(ModifiedInstanceDates))
            {
                DateTimeHelper.WriteDateTimeInMinutes(stream, date);
            }

            DateTimeHelper.WriteDateTimeInMinutes(stream, StartDate);
            DateTimeHelper.WriteDateTimeInMinutes(stream, EndDate);

            LittleEndianWriter.WriteUInt32(stream, ReaderVersion2);
            LittleEndianWriter.WriteUInt32(stream, WriterVersion2);

            LittleEndianWriter.WriteUInt32(stream, StartTimeOffset);
            LittleEndianWriter.WriteUInt32(stream, EndTimeOffset);

            LittleEndianWriter.WriteUInt16(stream, (ushort)ExceptionList.Count);
            foreach (ExceptionInfoStructure exception in ExceptionList)
            {
                exception.WriteBytes(stream);
            }

            uint reservedBlock1Size = 0;
            LittleEndianWriter.WriteUInt32(stream, reservedBlock1Size);

            foreach (ExceptionInfoStructure exception in ExceptionList)
            {
                exception.WriteExtendedException(stream, writerCompatibilityMode);
            }

            uint reservedBlock2Size = 0;
            LittleEndianWriter.WriteUInt32(stream, reservedBlock2Size);

            return stream.ToArray();
        }

        public void SetStartAndDuration(DateTime startDTUtc, int duration, TimeZoneInfo timezone)
        {
            SetStartDTUtc(startDTUtc, timezone);
            DateTime startDate = DateTimeUtils.GetDayStart(StartDTZone);
            TimeSpan ts = StartDTZone - startDate;
            StartTimeOffset = (uint)ts.TotalMinutes;
            EndTimeOffset = (uint)(duration + StartTimeOffset);
        }

        public DateTime GetStartDTUtc(TimeZoneInfo timezone)
        {
            return TimeZoneInfo.ConvertTimeToUtc(StartDTZone, timezone);
        }

        public void SetStartDTUtc(DateTime startDTUtc, TimeZoneInfo timezone)
        {
            StartDTZone = TimeZoneInfo.ConvertTimeFromUtc(startDTUtc, timezone);
        }

        /// <summary>
        /// StartDT of the first appointment, timezone time]
        /// </summary>
        public DateTime StartDTZone
        {
            get
            {
                DateTime result = StartDate.AddMinutes(StartTimeOffset);
                // DateTimeKind.Local means the current timezone on the client computer, while we use the timezone specified by the appointment
                result = DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
                return result;
            }
            set
            {
                StartDate = DateTimeUtils.GetDayStart(value);
                StartDate = DateTime.SpecifyKind(StartDate, DateTimeKind.Unspecified);
                TimeSpan ts = value - StartDate;
                StartTimeOffset = (uint)ts.TotalMinutes;
            }
        }

        /// <summary>
        /// The timespan in minutes from the start time to end time in timezone time.
        /// It will differ from the actual duration (UTC) if an appointment instance is spanning both standard time and daylight time.
        /// </summary>
        public int Duration
        {
            get
            {
                return (int)(EndTimeOffset - StartTimeOffset);
            }
        }

        public DateTime LastInstanceStartDate
        {
            get
            {
                // end date will be 31/12/4500 23:59:00 if there is no end date
                return DateTimeUtils.GetDayStart(EndDate);
            }
            set
            {
                EndDate = DateTimeUtils.GetDayStart(value);
                if (EndDate.Year >= 4500)
                {
                    EndDate = NoEndDate;
                }
            }
        }

        public RecurrenceType RecurrenceType
        {
            get
            {
                return RecurrenceTypeHelper.GetRecurrenceType(RecurFrequency, PatternType);
            }
        }

        public virtual int PeriodInRecurrenceTypeUnits
        {
            get
            {
                if (RecurrenceType == RecurrenceType.EveryNDays)
                {
                    return (int)(Period / 1440);
                }
                else if (RecurrenceType == RecurrenceType.EveryNYears ||
                         RecurrenceType == RecurrenceType.EveryNthDayOfEveryNYears)
                {
                    return (int)Period / 12;
                }
                else
                {
                    return (int)Period;
                }
            }
            set
            {
                if (RecurrenceType == RecurrenceType.EveryNDays)
                {
                    Period = (uint)value * 1440;
                }
                else if (RecurrenceType == RecurrenceType.EveryNYears ||
                         RecurrenceType == RecurrenceType.EveryNthDayOfEveryNYears)
                {
                    Period = (uint)value * 12;
                }
                else
                {
                    Period = (uint)value;
                }
            }
        }

        public int FirstDateTimeInDays
        {
            get
            {
                return (int)(FirstDateTime / 1440);
            }
            set
            {
                FirstDateTime = (uint)value * 1440;
            }
        }

        public static AppointmentRecurrencePatternStructure GetRecurrencePatternStructure(byte[] buffer)
        {
            RecurrenceFrequency frequency = (RecurrenceFrequency)LittleEndianConverter.ToInt16(buffer, 4);
            switch (frequency)
            { 
                case RecurrenceFrequency.Daily:
                    return new DailyRecurrencePatternStructure(buffer);
                case RecurrenceFrequency.Weekly:
                    return new WeeklyRecurrencePatternStructure(buffer);
                case RecurrenceFrequency.Monthly:
                    return new MonthlyRecurrencePatternStructure(buffer);
                case RecurrenceFrequency.Yearly:
                    return new YearlyRecurrencePatternStructure(buffer);
                default:
                    throw new InvalidRecurrencePatternException("Invalid recurrence pattern frequency");
            }
        }

        // http://msdn.microsoft.com/en-us/library/ee203303%28v=exchg.80%29
        /// <param name="period">In recurrence type units or recurrence frequency unit</param>
        /// <param name="startDTZone">Note: The date component of startDTZone may be different than the date of startDTUtc</param>
        public static int CalculateFirstDateTimeInDays(RecurrenceFrequency recurrenceFrequency, PatternType patternType, int period, DateTime startDTZone)
        {
            DateTime minimumDate = new DateTime(1601, 1, 1);
            switch (recurrenceFrequency)
            {
                case RecurrenceFrequency.Daily:
                    {
                        int daysSince1601 = (int)(startDTZone - minimumDate).TotalDays;
                        return daysSince1601 % period;
                    }
                case RecurrenceFrequency.Weekly:
                    {
                        int daysSince1601 = (int)(DateTimeUtils.GetWeekStart(startDTZone) - minimumDate).TotalDays;
                        int periodInDays = period * 7;
                        return daysSince1601 % periodInDays;
                    }
                case RecurrenceFrequency.Monthly:
                    {
                        int monthSpan = DateTimeHelper.GetMonthSpan(minimumDate, startDTZone);
                        int remainder = monthSpan % period;
                        return (int)(minimumDate.AddMonths(remainder) - minimumDate).TotalDays;
                    }
                case RecurrenceFrequency.Yearly:
                    {
                        int monthSpan = DateTimeHelper.GetMonthSpan(minimumDate, startDTZone);
                        int remainder = monthSpan % (period * 12);
                        return (int)(minimumDate.AddMonths(remainder) - minimumDate).TotalDays;
                    }
                default:
                    throw new ArgumentException("Invalid Recurrence Frequency");
            }
        }
    }
}
