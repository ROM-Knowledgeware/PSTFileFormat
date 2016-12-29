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
    public class WeeklyRecurrencePatternStructure : AppointmentRecurrencePatternStructure
    {
        public DaysOfWeekFlags DaysOfWeek;

        public WeeklyRecurrencePatternStructure()
        {
            RecurFrequency = RecurrenceFrequency.Weekly;
        }

        public WeeklyRecurrencePatternStructure(byte[] buffer) : base(buffer)
        {
        }

        public override void ReadPatternTypeSpecific(byte[] buffer, ref int offset)
        {
            // we start reading from offset 22
            if (PatternType == PatternType.Week) // specific days in week
            {
                DaysOfWeek = (DaysOfWeekFlags)LittleEndianReader.ReadUInt32(buffer, ref offset);
            }
            else
            {
                throw new InvalidRecurrencePatternException("Invalid Pattern Type");
            }
        }

        public override void WritePatternTypeSpecific(Stream stream)
        {
            if (PatternType == PatternType.Week) // specific days in week
            {
                LittleEndianWriter.WriteUInt32(stream, (uint)DaysOfWeek);
            }
            else
            {
                throw new InvalidRecurrencePatternException("Invalid Pattern Type");
            }
        }
    }
}
