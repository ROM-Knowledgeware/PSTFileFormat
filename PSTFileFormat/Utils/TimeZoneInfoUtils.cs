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
using Microsoft.Win32;

namespace Utilities
{
    public partial class TimeZoneInfoUtils
    {
        public static bool AreEquivalent(TimeZoneInfo a, TimeZoneInfo b)
        {
            return (a.BaseUtcOffset == b.BaseUtcOffset && a.HasSameRules(b));
        }

        public static DateTime SafeConvertToUtc(DateTime dateTime, TimeZoneInfo sourceTimeZone)
        {
            // time can be invalid when setting the clock one hour forward
            if (sourceTimeZone.IsInvalidTime(dateTime))
            {
                TimeZoneInfo.AdjustmentRule rule = AdjustmentRuleUtils.GetAdjustmentRuleForTime(sourceTimeZone.GetAdjustmentRules(), dateTime.Date);
                // rule cannot be null for invalid time
                return TimeZoneInfo.ConvertTimeToUtc(dateTime.Add(rule.DaylightDelta), sourceTimeZone);
            }
            else
            {
                return TimeZoneInfo.ConvertTimeToUtc(dateTime, sourceTimeZone);
            }
        }

        public static DateTime AdvancedConvertToUtc(DateTime dateTime, TimeZoneInfo sourceTimeZone, bool assumeDaylightTimeForAmbiguousTime)
        {
            DateTime result = SafeConvertToUtc(dateTime, sourceTimeZone);

            // time can be ambiguous when setting the clock one hour backward
            if (sourceTimeZone.IsAmbiguousTime(dateTime) && assumeDaylightTimeForAmbiguousTime)
            {
                TimeZoneInfo.AdjustmentRule rule = AdjustmentRuleUtils.GetAdjustmentRuleForTime(sourceTimeZone.GetAdjustmentRules(), dateTime.Date);
                // ConvertTimeToUtc will assume standard time, we want it to assume daylight time
                // rule cannot be null for ambiguous time
                result = result.Add(-rule.DaylightDelta);
            }

            return result;
        }

        public static DateTime SetValidTimeOfDay(DateTime dateTime, TimeSpan timeOfDay, TimeZoneInfo timezone)
        {
            dateTime = DateTimeUtils.SetTimeOfDay(dateTime, timeOfDay);
            // time can be invalid when setting the clock one hour forward
            if (timezone.IsInvalidTime(dateTime))
            {
                TimeZoneInfo.AdjustmentRule rule = AdjustmentRuleUtils.GetAdjustmentRuleForTime(timezone.GetAdjustmentRules(), dateTime.Date);
                // rule cannot be null for invalid time
                dateTime = dateTime.Add(rule.DaylightDelta);
            }
            
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            return dateTime;
        }

        public static TimeZoneInfo FindSystemTimeZoneByDisplayName(string displayName)
        {
            foreach (TimeZoneInfo timezone in TimeZoneInfo.GetSystemTimeZones())
            {
                if (timezone.DisplayName == displayName)
                {
                    return timezone;
                }
            }
            return null;
        }
    }
}
