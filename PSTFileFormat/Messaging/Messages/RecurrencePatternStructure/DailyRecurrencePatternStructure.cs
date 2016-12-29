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
    public enum DailyPatternType : ushort
    {
        EveryDay = 0,
        EveryWeekday = 1,
    }

    public class DailyRecurrencePatternStructure : AppointmentRecurrencePatternStructure
    {
        //public DaysOfWeekFlags DaysOfWeek; // when DailyPattern == EveryWeekday

        public DailyRecurrencePatternStructure()
        {
            RecurFrequency = RecurrenceFrequency.Daily;
        }

        public DailyRecurrencePatternStructure(byte[] buffer) : base(buffer)
        {}

        public override void ReadPatternTypeSpecific(byte[] buffer, ref int offset)
        {
            // we start reading from offset 22
            if (PatternType == PatternType.Day)
            {
            }
            else if (PatternType == PatternType.Week) // EveryWeekday
            {
                DaysOfWeekFlags DaysOfWeek = (DaysOfWeekFlags)LittleEndianReader.ReadUInt32(buffer, ref offset);
                if (DaysOfWeek != DateTimeHelper.Weekdays)
                {
                    throw new InvalidRecurrencePatternException("Invalid DaysOfWeek for Daily Recurrence Pattern");
                }
            }
            else
            {
                throw new InvalidRecurrencePatternException("Invalid Pattern Type");
            }
        }

        public override void WritePatternTypeSpecific(Stream stream)
        {
            if (PatternType == PatternType.Day)
            {
            }
            else if (PatternType == PatternType.Week) // EveryWeekday
            {
                LittleEndianWriter.WriteUInt32(stream, (uint)DateTimeHelper.Weekdays);
            }
            else
            {
                throw new InvalidRecurrencePatternException("Invalid Pattern Type");
            }
        }

        [Obsolete]
        public int PeriodInDays
        {
            get
            {
                if (PatternType == PatternType.Day)
                {
                    return (int)(Period / 1440);
                }
                else
                {
                    return 7;
                }
            }
            set
            {
                if (PatternType == PatternType.Day)
                {
                    Period = (uint)value * 1440;
                }
                else
                {
                    Period = (uint)value / 7;
                }
            }
        }

        [Obsolete]
        public int DaysToSkip
        {
            get
            {
                return (int)(FirstDateTime / 1440);
            }
        }
    }
}
