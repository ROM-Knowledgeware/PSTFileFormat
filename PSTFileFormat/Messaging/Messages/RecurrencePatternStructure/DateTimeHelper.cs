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
    public class DateTimeHelper
    {
        public const DaysOfWeekFlags Weekdays = DaysOfWeekFlags.Monday | DaysOfWeekFlags.Tuesday | DaysOfWeekFlags.Wednesday | DaysOfWeekFlags.Thursday | DaysOfWeekFlags.Friday;

        public static DateTime ReadDateTimeFromMinutes(byte[] buffer, ref int offset)
        {
            uint minutesSince1601 = LittleEndianReader.ReadUInt32(buffer, ref offset);
            return GetDateTime(minutesSince1601, DateTimeKind.Unspecified);
        }

        public static DateTime ToDateTimeFromMinutes(byte[] buffer, int offset)
        {
            uint minutesSince1601 = LittleEndianConverter.ToUInt32(buffer, offset);
            return GetDateTime(minutesSince1601, DateTimeKind.Unspecified);
        }

        public static DateTime GetDateTime(uint minutesSince1601, DateTimeKind kind)
        {
            long fileTimeUtc = (long)minutesSince1601 * 60 * 10000000;
            DateTime result = DateTime.FromFileTimeUtc(fileTimeUtc);
            result = DateTime.SpecifyKind(result, kind); // We read as UTC to avoid conversion
            return result;
        }
        
        public static void WriteDateTimeInMinutes(Stream stream,  DateTime dt)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc); // We write as UTC to avoid conversion
            uint minutesSince1601 = GetMinutesSince1601(dt);
            LittleEndianWriter.WriteUInt32(stream, minutesSince1601);
        }

        public static void WriteDateTimeInMinutes(byte[] buffer, ref int offset, DateTime dt)
        {
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc); // We write as UTC to avoid conversion
            uint minutesSince1601 = GetMinutesSince1601(dt);
            LittleEndianWriter.WriteUInt32(buffer, ref offset, minutesSince1601);
        }

        public static uint GetMinutesSince1601(DateTime dt)
        {
            uint minutesSince1601 = (uint)(dt.ToFileTimeUtc() / (60 * 10000000));
            return minutesSince1601;
        }

        /// <summary>
        /// Calculate the Months needed to add to startDate in order for it to be in the same year and month as endDate
        /// </summary>
        public static int GetMonthSpan(DateTime startDate, DateTime endDate)
        {
            int result = 0;

            result = (endDate.Year - startDate.Year) * 12;
            result += endDate.Month - startDate.Month;

            return result;
        }

        public static DaysOfWeekFlags GetDayOfWeek(DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return DaysOfWeekFlags.Sunday;
                case DayOfWeek.Monday:
                    return DaysOfWeekFlags.Monday;
                case DayOfWeek.Tuesday:
                    return DaysOfWeekFlags.Tuesday;
                case DayOfWeek.Wednesday:
                    return DaysOfWeekFlags.Wednesday;
                case DayOfWeek.Thursday:
                    return DaysOfWeekFlags.Thursday;
                case DayOfWeek.Friday:
                    return DaysOfWeekFlags.Friday;
                default:
                case DayOfWeek.Saturday:
                    return DaysOfWeekFlags.Saturday;
            }
        }
    }
}
