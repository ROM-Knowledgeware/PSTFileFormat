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
    public class CalendarHelper
    {
        public static int CalculateNumberOfOccurences(DateTime startDate, DateTime lastInstanceStartDate, RecurrenceType recurrenceType, int period, int day)
        {
            startDate = DateTimeUtils.GetDayStart(startDate);
            lastInstanceStartDate = DateTimeUtils.GetDayStart(lastInstanceStartDate);

            if (recurrenceType == RecurrenceType.EveryNDays)
            {
                    TimeSpan ts = lastInstanceStartDate - startDate;
                    return ((int)ts.TotalDays) / period + 1;
            }
            else if (recurrenceType == RecurrenceType.EveryWeekday)
            {
                DaysOfWeekFlags weekdays = DateTimeHelper.Weekdays;
                return CalculateNumberOfOccurencesInWeek(startDate, lastInstanceStartDate, period, weekdays);
            }
            else if (recurrenceType == RecurrenceType.EveryNWeeks)
            {
                return CalculateNumberOfOccurencesInWeek(startDate, lastInstanceStartDate, period, (DaysOfWeekFlags)day);
            }
            else if (recurrenceType == RecurrenceType.EveryNMonths ||
                     recurrenceType == RecurrenceType.EveryNthDayOfEveryNMonths)
            {
                int numberOfMonths = DateTimeHelper.GetMonthSpan(startDate, lastInstanceStartDate);

                return numberOfMonths / period + 1; // extra day
            }
            else
            {
                int numberOfYears = lastInstanceStartDate.Year - startDate.Year;
                return numberOfYears / period + 1;
            }
        }

        public static int CalculateNumberOfOccurencesInWeek(DateTime startDate, DateTime lastInstanceStartDate, int period, DaysOfWeekFlags daysOfWeek)
        {
            TimeSpan ts = lastInstanceStartDate - startDate;
            int totalDays = (int)ts.TotalDays + 1;
            int daysOfWeekCount = GetSetBitCount((int)daysOfWeek);
            int numberOfWeeks = (totalDays / 7);
            int extraDays = totalDays - numberOfWeeks * 7;
            int extraOccurences = 0;

            DateTime date = DateTimeUtils.GetDayStart(startDate).AddDays(numberOfWeeks * 7);
            while (date <= lastInstanceStartDate)
            {
                if ((DateTimeHelper.GetDayOfWeek(date) & daysOfWeek) > 0)
                {
                    extraOccurences++;
                }
                date = date.AddDays(1);
            }
            return numberOfWeeks * daysOfWeekCount / period + extraOccurences;
        }

        public static int GetSetBitCount(long lValue)
        {
            int iCount = 0;

            //Loop the value while there are still bits
            while (lValue != 0)
            {
                //Remove the end bit
                lValue = lValue & (lValue - 1);

                //Increment the count
                iCount++;
            }

            //Return the count
            return iCount;
        }
    }
}
