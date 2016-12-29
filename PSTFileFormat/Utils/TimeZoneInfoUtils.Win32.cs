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

namespace Utilities
{
    public partial class TimeZoneInfoUtils
    {
        public static TimeZoneInfo GetSystemStaticTimeZone(string keyName)
        {
            RegistryTimeZoneInformation information = RegistryTimeZoneUtils.GetStaticTimeZoneInformation(keyName);
            TimeZoneInfo.AdjustmentRule rule = AdjustmentRuleUtils.GetStaticAdjustmentRule(information);
            string displayName;
            string standardDisplayName;
            string daylightDisplayName;
            displayName = RegistryTimeZoneUtils.GetDisplayName(keyName, out standardDisplayName, out daylightDisplayName);

            TimeSpan baseUtcOffset = new TimeSpan(0, -(information.Bias + information.StandardBias), 0);
            if (rule == null)
            {
                // null will only be returned if there is no daylight saving
                return TimeZoneInfo.CreateCustomTimeZone(keyName, baseUtcOffset, displayName, standardDisplayName);
            }
            else
            {
                return TimeZoneInfo.CreateCustomTimeZone(keyName, baseUtcOffset, displayName, standardDisplayName, daylightDisplayName, new TimeZoneInfo.AdjustmentRule[] { rule });
            }
        }
    }
}
