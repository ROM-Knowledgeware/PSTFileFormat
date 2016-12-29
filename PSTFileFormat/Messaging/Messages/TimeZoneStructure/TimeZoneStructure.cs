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
    // http://msdn.microsoft.com/en-us/library/ee237092%28v=exchg.80%29.aspx
    public class TimeZoneStructure // PidLidTimeZoneStruct
    {
        public const int Length = 48;

        public int lBias;                 // The time zone's offset in minutes from UTC, will be -120 for GMT+2:00
        public int lStandardBias;         // The offset in minutes from the value of the lBias field during standard time
        public int lDaylightBias;         // The offset in minutes from the value of the lBias field during daylight saving time.
        // ushort wStandardYear;          // This field matches the stStandardDate's wYear member.
        public SystemTime stStandardDate; // This field contains the date and local time that indicate when to begin using the value specified in the lStandardBias field
        // ushort wDaylightYear;          // This field is equal to the value of the stDaylightDate's wYear field
        public SystemTime stDaylightDate; // This field contains the date and local time that indicate when to begin using the value specified in the lDaylightBias field

        public TimeZoneStructure()
        { 
        }

        public TimeZoneStructure(byte[] buffer)
        {
            lBias = LittleEndianConverter.ToInt32(buffer, 0);
            lStandardBias = LittleEndianConverter.ToInt32(buffer, 4);
            lDaylightBias = LittleEndianConverter.ToInt32(buffer, 8);
            ushort wStandardYear = LittleEndianConverter.ToUInt16(buffer, 12);
            stStandardDate = new SystemTime(buffer, 14);
            ushort wDaylightYear = LittleEndianConverter.ToUInt16(buffer, 30);
            stDaylightDate = new SystemTime(buffer, 32);

            if (stStandardDate.wYear != wStandardYear)
            {
                throw new InvalidPropertyException("Invalid TimeZoneStructure");
            }

            if (stDaylightDate.wYear != wDaylightYear)
            {
                throw new InvalidPropertyException("Invalid TimeZoneStructure");
            }
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[Length];
            LittleEndianWriter.WriteInt32(buffer, 0, lBias);
            LittleEndianWriter.WriteInt32(buffer, 4, lStandardBias);
            LittleEndianWriter.WriteInt32(buffer, 8, lDaylightBias);
            LittleEndianWriter.WriteUInt16(buffer, 12, stStandardDate.wYear);
            stStandardDate.WriteBytes(buffer, 14);
            LittleEndianWriter.WriteUInt16(buffer, 30, stDaylightDate.wYear);
            stDaylightDate.WriteBytes(buffer, 32);

            return buffer;
        }

        public TimeZoneInfo ToTimeZoneInfo(string timeZoneID)
        {
            // baseUtcOffset should be 120 for GMT+2:00
            TimeSpan baseUtcOffset = new TimeSpan(0, -(lBias + lStandardBias), 0);
            
            string standardDisplayName;
            string daylightDisplayName;
            string displayName = RegistryTimeZoneUtils.GetDisplayName(timeZoneID, out standardDisplayName, out daylightDisplayName);

            // Note about stStandardDate:
            // If the time zone does not support daylight saving time, the wMonth member in the SYSTEMTIME structure MUST be zero
            if (stStandardDate.wMonth == 0)
            {
                return TimeZoneInfo.CreateCustomTimeZone(timeZoneID, baseUtcOffset, displayName, standardDisplayName);
            }
            else
            {
                TimeZoneInfo.AdjustmentRule rule;
                if (stStandardDate.wYear == 0)
                {
                    // If the wYear member is zero, the date is interpreted as a relative date that occurs yearly
                    rule = AdjustmentRuleUtils.CreateStaticAdjustmentRule(lBias, lStandardBias, lDaylightBias, stStandardDate, stDaylightDate);
                }
                else
                {
                    // If the wYear member is not zero, the date is interpreted as an absolute date that only occurs once.
                    DateTime ruleStartDate = DateTimeUtils.GetYearStart(stStandardDate.wYear).Date;
                    DateTime ruleEndDate = DateTimeUtils.GetYearEnd(stStandardDate.wYear).Date;
                    rule = AdjustmentRuleUtils.CreateAdjustmentRule(ruleStartDate, ruleEndDate, lBias, lStandardBias, lDaylightBias, stStandardDate, stDaylightDate);
                }
                return TimeZoneInfo.CreateCustomTimeZone(timeZoneID, baseUtcOffset, displayName, standardDisplayName, daylightDisplayName, new TimeZoneInfo.AdjustmentRule[] { rule });
            }
        }

        public void SetBias(TimeSpan baseUtcOffset, TimeSpan daylightDelta)
        {
            lBias = -(int)baseUtcOffset.TotalMinutes;
            lStandardBias = 0;
            lDaylightBias = -(int)daylightDelta.TotalMinutes;
        }

        public static TimeZoneStructure FromTimeZoneInfo(TimeZoneInfo staticTimeZone)
        {
            TimeZoneInfo.AdjustmentRule[] rules = staticTimeZone.GetAdjustmentRules();
            if (rules.Length > 1)
            {
                throw new ArgumentException("Cannot create TimeZoneStructure from a time zone with multiple DST rules");
            }
            
            TimeZoneStructure structure = new TimeZoneStructure();
            if (rules.Length == 0)
            {
                // no daylight saving
                structure.SetBias(staticTimeZone.BaseUtcOffset, new TimeSpan());
                structure.stStandardDate = new SystemTime();
                structure.stDaylightDate = new SystemTime();
            }
            else
            {
                TimeZoneInfo.AdjustmentRule rule = rules[0];
                structure.SetBias(staticTimeZone.BaseUtcOffset, rule.DaylightDelta);
                structure.stStandardDate = AdjustmentRuleHelper.GetStandardDate(rule);
                structure.stDaylightDate = AdjustmentRuleHelper.GetDaylightDate(rule);
            }

            return structure;
        }
    }
}
