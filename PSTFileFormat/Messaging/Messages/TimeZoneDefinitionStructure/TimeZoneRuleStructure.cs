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
    // http://msdn.microsoft.com/en-us/library/ee160657%28v=exchg.80%29
    public class TimeZoneRuleStructure
    {
        public const int Length = 66;

        public byte MajorVersion = 0x02;
        public byte MinorVersion = 0x01;
        public ushort Reserved = 0x003E;
        public TimeZoneRuleFlags TZRuleFlags;
        public ushort wYear; // Specifies the year in which this rule is scheduled to take effect.
        // 14 unused bytes
        public int lBias; // The offset in minutes from UTC, will be -120 for GMT+2:00
        public int lStandardBias; // The offset in minutes from the value of the lBias field during standard time
        public int lDaylightBias; // The offset in minutes from the value of the lBias field during daylight saving time.
        public SystemTime stStandardDate = new SystemTime();
        public SystemTime stDaylightDate = new SystemTime();

        public TimeZoneRuleStructure()
        {
        }

        public TimeZoneRuleStructure(byte[] buffer, int offset)
        {
            MajorVersion = ByteReader.ReadByte(buffer, offset + 0);
            MinorVersion = ByteReader.ReadByte(buffer, offset + 1);
            Reserved = LittleEndianConverter.ToUInt16(buffer, offset + 2);
            TZRuleFlags = (TimeZoneRuleFlags)LittleEndianConverter.ToUInt16(buffer, offset + 4);
            wYear = LittleEndianConverter.ToUInt16(buffer, offset + 6);
            // 14 unused bytes
            lBias = LittleEndianConverter.ToInt32(buffer, offset + 22);
            lStandardBias = LittleEndianConverter.ToInt32(buffer, offset + 26);
            lDaylightBias = LittleEndianConverter.ToInt32(buffer, offset + 30);
            stStandardDate = new SystemTime(buffer, offset + 34);
            stDaylightDate = new SystemTime(buffer, offset + 50);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            ByteWriter.WriteByte(buffer, offset + 0, MajorVersion);
            ByteWriter.WriteByte(buffer, offset + 1, MinorVersion);
            LittleEndianWriter.WriteUInt16(buffer, offset + 2, Reserved);
            LittleEndianWriter.WriteUInt16(buffer, offset + 4, (ushort)TZRuleFlags);
            LittleEndianWriter.WriteUInt16(buffer, offset + 6, wYear);
            // 14 unused bytes
            LittleEndianWriter.WriteInt32(buffer, offset + 22, lBias);
            LittleEndianWriter.WriteInt32(buffer, offset + 26, lStandardBias);
            LittleEndianWriter.WriteInt32(buffer, offset + 30, lDaylightBias);
            stStandardDate.WriteBytes(buffer, offset + 34);
            stDaylightDate.WriteBytes(buffer, offset + 50);
        }

        public TimeZoneInfo.AdjustmentRule ToAdjustmentRule()
        { 
            DateTime startDate = DateTimeUtils.GetYearStart(wYear).Date;
            DateTime endDate = DateTimeUtils.GetYearEnd(wYear).Date;
            return AdjustmentRuleUtils.CreateAdjustmentRule(startDate, endDate, lBias, lStandardBias, lDaylightBias, stStandardDate, stDaylightDate);
        }

        /// <summary>
        /// Translate this TimeZoneRuleStructure to a static AdjustmentRule,
        /// Similar to the static rule Windows 2000 \ XP originally used, and Outlook still uses.
        /// </summary>
        public TimeZoneInfo.AdjustmentRule ToStaticAdjustmentRule()
        {
            // Static rule can either be fixed-date (wYear is not zero) or floating-date (wYear is zero).
            // Static fixed-date rule can only occur once (single year).
            if (stStandardDate.wYear != 0)
            {
                if (stDaylightDate.wYear == stStandardDate.wYear)
                {
                    return AdjustmentRuleUtils.CreateStaticAdjustmentRule(stStandardDate.wYear, lBias, lStandardBias, lDaylightBias, stStandardDate, stDaylightDate);
                }
                else // fixed-date rule that spans multiple years
                {
                    throw new Exception("Fixed-Date TimeZoneRuleStructure spans more than one year and cannot be converted to static AdjustmentRule");
                }
            }
            else
            {
                return AdjustmentRuleUtils.CreateStaticAdjustmentRule(lBias, lStandardBias, lDaylightBias, stStandardDate, stDaylightDate);
            }
        }

        public TimeSpan BaseUtcOffset
        { 
            get
            {
                // baseUtcOffset should be 120 for GMT+2:00
                return new TimeSpan(0, -(lBias + lStandardBias), 0);
            }
        }

        public void SetBias(TimeSpan baseUtcOffset, TimeSpan daylightDelta)
        {
            lBias = -(int)baseUtcOffset.TotalMinutes;
            lStandardBias = 0;
            lDaylightBias = -(int)daylightDelta.TotalMinutes;
        }

        public bool IsRecurringTimeZoneRule
        {
            get
            {
                return (TZRuleFlags & TimeZoneRuleFlags.RecurringTimeZoneRule) > 0;
            }
        }

        public bool IsEffectiveTimeZoneRule
        {
            get
            {
                return (TZRuleFlags & TimeZoneRuleFlags.EffectiveTimeZoneRule) > 0;
            }
        }
    }
}
