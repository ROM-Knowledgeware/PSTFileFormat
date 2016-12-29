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
using System.Text;
using Utilities;

namespace PSTFileFormat
{
    public class AdjustmentRuleHelper
    {
        public static SystemTime GetStandardDate(TimeZoneInfo.AdjustmentRule rule)
        {
            return FromTransitionTime(rule.DateStart.Year, rule.DateEnd.Year, rule.DaylightTransitionEnd);
        }

        public static SystemTime GetDaylightDate(TimeZoneInfo.AdjustmentRule rule)
        {
            return FromTransitionTime(rule.DateStart.Year, rule.DateEnd.Year, rule.DaylightTransitionStart);
        }

        // Details about the conversion process:
        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms725481%28v=vs.85%29.aspx
        public static SystemTime FromTransitionTime(int startYear, int endYear, TimeZoneInfo.TransitionTime transitionTime)
        {
            SystemTime result = new SystemTime();
            if (transitionTime.IsFixedDateRule)
            {
                if (startYear == endYear)
                {
                    // If the wYear member is not zero, the transition date is absolute; it will only occur one time.
                    int year = startYear;
                    int month = transitionTime.Month;
                    int day = transitionTime.Day;
                    // this will calculate DayOfWeek
                    TimeSpan timeOfDay = transitionTime.TimeOfDay.TimeOfDay;
                    result.DateTime = new DateTime(year, month, day).Add(timeOfDay);
                    // When rule is fixed-date, wDayOfWeek (which is redundant) is unused and set to 0
                    // http://social.msdn.microsoft.com/Forums/en-US/os_binaryfile/thread/d5ebf7d3-f6a9-429d-8f27-7ec2bdec440f
                    result.wDayOfWeek = 0;
                }
                else
                {
                    // The SystemTime structure (and registry) do not support this
                    throw new Exception("Cannot create transition time with absolute date that spans multiple years");
                }
            }
            else
            {
                // If the wYear member is zero, it is a relative date that occurs yearly.
                result.wYear = 0; // Note: TimeZoneRuleStructure have an additional wYear parameter besides the one in stStandardDate / stDaylightDate
                result.wMonth = (ushort)transitionTime.Month;
                // the wDay member is set to indicate the occurrence of the day of the week within the month
                result.wDay = (ushort)transitionTime.Week;
                result.wDayOfWeek = (ushort)transitionTime.DayOfWeek;
                result.TimeOfDay = transitionTime.TimeOfDay.TimeOfDay;
            }
            return result;
        }
    }
}
