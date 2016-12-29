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
        /// <summary>
        /// This will return the static timezone adjustment rule the the OS uses.
        /// We assume that the static rule is used by one of the dynamic adjustment rules.
        /// </summary>
        public static TimeZoneInfo.AdjustmentRule FindSystemStaticAdjustmentRule(string keyName)
        {
            RegistryTimeZoneInformation information = RegistryTimeZoneUtils.GetStaticTimeZoneInformation(keyName);
            if (information == null)
            {
                throw new TimeZoneNotFoundException();
            }
            else
            {
                return GetStaticAdjustmentRule(information);
            }
        }

        public static TimeZoneInfo.AdjustmentRule GetStaticAdjustmentRule(RegistryTimeZoneInformation information)
        {
            if (RegistryTimeZoneUtils.IsDaylightSavingsEnabled())
            {
                return CreateStaticAdjustmentRule(information.Bias, information.StandardBias, information.DaylightBias, information.StandardDate, information.DaylightDate);
            }
            else
            {
                // We cant have an AdjustmentRule if there is no DST
                return null;
            }
        }
    }
}
