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
    public class YearlyRecurrencePatternStructure : AppointmentRecurrencePatternStructure
    {
        public uint DayOfMonth;
        public OutlookDayOfWeek DayOfWeek;
        public DayOccurenceNumber DayOccurenceNumber;
        
        public YearlyRecurrencePatternStructure()
        {
            RecurFrequency = RecurrenceFrequency.Yearly;
        }

        public YearlyRecurrencePatternStructure(byte[] buffer) : base(buffer)
        {
        }

        public override void ReadPatternTypeSpecific(byte[] buffer, ref int offset)
        {
            // we start reading from offset 22
            if (PatternType == PatternType.Month) // i.e. the 23rd of may
            {
                DayOfMonth = LittleEndianReader.ReadUInt32(buffer, ref offset);
            }
            else if (PatternType == PatternType.MonthNth) // i.e. the fourth monday of may
            {
                DayOfWeek = (OutlookDayOfWeek)LittleEndianReader.ReadUInt32(buffer, ref offset);
                DayOccurenceNumber = (DayOccurenceNumber)LittleEndianReader.ReadUInt32(buffer, ref offset);
            }
            else
            {
                throw new InvalidRecurrencePatternException("Invalid Pattern Type");
            }
        }

        public override void WritePatternTypeSpecific(Stream stream)
        {
            if (PatternType == PatternType.Month) // i.e. the 23rd of may
            {
                LittleEndianWriter.WriteUInt32(stream, DayOfMonth);
            }
            else if (PatternType == PatternType.MonthNth) // i.e. the fourth monday of may
            {
                LittleEndianWriter.WriteUInt32(stream, (uint)DayOfWeek);
                LittleEndianWriter.WriteUInt32(stream, (uint)DayOccurenceNumber);
            }
            else
            {
                throw new InvalidRecurrencePatternException("Invalid Pattern Type");
            }
        }
    }
}
