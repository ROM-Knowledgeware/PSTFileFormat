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
using System.Globalization;
using System.Text;

namespace Utilities
{
    public partial class AdjustmentRuleUtils
    {
        public static TimeZoneInfo.AdjustmentRule CreateStaticAdjustmentRule(int bias, int standardBias, int daylightBias, SystemTime standardDate, SystemTime daylightDate)
        {
            return CreateAdjustmentRule(DateTime.MinValue.Date, DateTime.MaxValue.Date, bias, standardBias, daylightBias, standardDate, daylightDate);
        }

        public static TimeZoneInfo.AdjustmentRule CreateStaticAdjustmentRule(int year, int bias, int standardBias, int daylightBias, SystemTime standardDate, SystemTime daylightDate)
        {
            DateTime ruleStartDate = DateTimeUtils.GetYearStart(year).Date;
            DateTime ruleEndDate = DateTimeUtils.GetYearEnd(year).Date;
            return CreateAdjustmentRule(ruleStartDate, ruleEndDate, bias, standardBias, daylightBias, standardDate, daylightDate);
        }

        // Details about the conversion process:
        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms725481%28v=vs.85%29.aspx
        /// <returns>null if no daylight savings</returns>
        public static TimeZoneInfo.AdjustmentRule CreateAdjustmentRule(DateTime ruleStartDate, DateTime ruleEndDate, int bias, int standardBias, int daylightBias, SystemTime standardDate, SystemTime daylightDate)
        {
            TimeSpan baseUtcOffset = new TimeSpan(0, -(bias + standardBias), 0);
            // Note about stStandardDate:
            // If the time zone does not support daylight saving time, the wMonth member in the SYSTEMTIME structure MUST be zero
            if (standardDate.wMonth == 0)
            {
                // If the time zone does not support daylight saving time, the wMonth field in the SYSTEMTIME structure MUST be zero
                return null;
            }
            else
            {
                TimeSpan daylightUtcOffset = new TimeSpan(0, -(bias + daylightBias), 0);
                TimeSpan daylightDelta = daylightUtcOffset - baseUtcOffset;
                TimeZoneInfo.TransitionTime daylightTransitionStart;
                TimeZoneInfo.TransitionTime daylightTransitionEnd;
                if (standardDate.wYear == 0)
                {
                    // If the wYear member is zero, it is a relative date (floating date) that occurs yearly.

                    // Note:
                    // wDay is set to indicate the occurrence of the day of the week within the month
                    // (1 to 5, where 5 indicates the final occurrence during the month if that day of
                    // the week does not occur 5 times).
                    int daylightStartWeek = daylightDate.wDay;
                    int daylightEndWeek = standardDate.wDay;
                    daylightTransitionStart = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1).Add(daylightDate.TimeOfDay), (int)daylightDate.wMonth, daylightStartWeek, daylightDate.DayOfWeek);
                    daylightTransitionEnd = TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1).Add(standardDate.TimeOfDay), (int)standardDate.wMonth, daylightEndWeek, standardDate.DayOfWeek);
                }
                else
                {
                    // If the wYear member is not zero, the transition date is absolute; it will only occur one time.
                    daylightTransitionStart = TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1).Add(daylightDate.TimeOfDay), (int)daylightDate.wMonth, (int)daylightDate.wDay);
                    daylightTransitionEnd = TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1).Add(standardDate.TimeOfDay), (int)standardDate.wMonth, (int)standardDate.wDay);
                }

                TimeZoneInfo.AdjustmentRule rule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(ruleStartDate, ruleEndDate, daylightDelta, daylightTransitionStart, daylightTransitionEnd);
                return rule;
            }
        }

        public static TimeZoneInfo.AdjustmentRule GetAdjustmentRuleForTime(TimeZoneInfo.AdjustmentRule[] rules, DateTime dateTime)
        {
            DateTime date = dateTime.Date;
            for (int index = 0; index < rules.Length; index++)
            {
                if (rules[index].DateStart <= date &&
                    rules[index].DateEnd >= date)
                {
                    return rules[index];
                }
            }
            return null;
        }

        public static TimeZoneInfo.AdjustmentRule GetFirstRule(TimeZoneInfo timezone)
        {
            TimeZoneInfo.AdjustmentRule[] rules = timezone.GetAdjustmentRules();
            if (rules.Length > 0)
            {
                return rules[0];
            }
            return null;
        }

        // http://msdn.microsoft.com/en-us/library/system.timezoneinfo.transitiontime.isfixeddaterule.aspx
        public static DateTime GetTransitionDateTime(TimeZoneInfo.TransitionTime transition, int year)
        {
            // For non-fixed date rules, get local calendar
            GregorianCalendar calendar = new GregorianCalendar();
            // Get first day of week for transition
            // For example, the 3rd week starts no earlier than the 15th of the month
            int startOfWeek = transition.Week * 7 - 6;
            // What day of the week does the month start on?
            int firstDayOfWeek = (int)calendar.GetDayOfWeek(new DateTime(year, transition.Month, 1));
            // Determine how much start date has to be adjusted
            int transitionDay;
            int changeDayOfWeek = (int)transition.DayOfWeek;

            if (firstDayOfWeek <= changeDayOfWeek)
            {
                transitionDay = startOfWeek + (changeDayOfWeek - firstDayOfWeek);
            }
            else
            {
                transitionDay = startOfWeek + (7 - firstDayOfWeek + changeDayOfWeek);
            }

            // Adjust for months with no fifth week
            if (transitionDay > calendar.GetDaysInMonth(year, transition.Month))
            {
                transitionDay -= 7;
            }
            return new DateTime(year, transition.Month, transitionDay).Add(transition.TimeOfDay.TimeOfDay);
        }   



        public static bool IsTransitionEquivalent(TimeZoneInfo.AdjustmentRule a, TimeZoneInfo.AdjustmentRule b)
        {
            return (a.DaylightTransitionStart.Equals(b.DaylightTransitionStart) &&
                    a.DaylightTransitionEnd.Equals(b.DaylightTransitionEnd) &&
                    a.DaylightDelta == b.DaylightDelta);
        }
    }
}
