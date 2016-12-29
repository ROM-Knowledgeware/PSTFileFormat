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
    // http://msdn.microsoft.com/en-us/library/ee158467%28v=exchg.80%29.aspx
    public class TimeZoneDefinitionStructure
    {
        public byte MajorVersion = 0x02;
        public byte MinorVersion = 0x01;
        // public ushort cbHeader; // The number of bytes contained in the Reserved, cchKeyName, KeyName, and cRules fields
        public ushort Reserved = 0x0002;
        // ushort cchKeyName;
        public string KeyName;
        // ushort cRules;
        public TimeZoneRuleStructure[] TZRules;

        public TimeZoneDefinitionStructure()
        { 
        }

        public TimeZoneDefinitionStructure(byte[] buffer)
        {
            int offset = 0;
            MajorVersion = ByteReader.ReadByte(buffer, ref offset);
            MinorVersion = ByteReader.ReadByte(buffer, ref offset);
            ushort cbHeader = LittleEndianReader.ReadUInt16(buffer, ref offset);
            Reserved = LittleEndianReader.ReadUInt16(buffer, ref offset);
            ushort cchKeyName = LittleEndianReader.ReadUInt16(buffer, ref offset);
            KeyName = ByteReader.ReadUTF16String(buffer, ref offset, cchKeyName);
            ushort cRules = LittleEndianReader.ReadUInt16(buffer, ref offset);
            TZRules = new TimeZoneRuleStructure[cRules];
            for (int index = 0; index < cRules; index++)
            {
                TZRules[index] = new TimeZoneRuleStructure(buffer, offset);
                offset += TimeZoneRuleStructure.Length;
            }
        }

        public byte[] GetBytes()
        {
            int length = 10 + KeyName.Length * 2 + TZRules.Length * TimeZoneRuleStructure.Length;
            byte[] buffer = new byte[length];
            int offset = 0;
            ByteWriter.WriteByte(buffer, ref offset, MajorVersion);
            ByteWriter.WriteByte(buffer, ref offset, MinorVersion);
            LittleEndianWriter.WriteUInt16(buffer, ref offset, (ushort)(6 + KeyName.Length * 2));
            LittleEndianWriter.WriteUInt16(buffer, ref offset, Reserved);
            LittleEndianWriter.WriteUInt16(buffer, ref offset, (ushort)KeyName.Length);
            ByteWriter.WriteUTF16String(buffer, ref offset, KeyName, KeyName.Length);
            LittleEndianWriter.WriteUInt16(buffer, ref offset, (ushort)TZRules.Length);
            for (int index = 0; index < TZRules.Length; index++)
            {
                TZRules[index].WriteBytes(buffer, offset);
                offset += TimeZoneRuleStructure.Length;
            }

            return buffer;
        }

        /// <summary>
        /// Despite holding several adjustment rules, Outlook will only use the rule that is marked as effective.
        /// (see http://msdn.microsoft.com/en-us/library/ff960389.aspx )
        /// We should ignore all other rules to be consistent with Outlook (all versions).
        /// </summary>
        public TimeZoneInfo ToTimeZoneInfo()
        {
            TimeZoneRuleStructure effectiveTimeZoneRuleStructure = GetEffectiveTimeZoneRule();
            if (effectiveTimeZoneRuleStructure == null)
            {
                throw new InvalidPropertyException("No timezone rule has been marked as effective");
            }

            TimeZoneInfo.AdjustmentRule effectiveTimeZoneRule = effectiveTimeZoneRuleStructure.ToStaticAdjustmentRule();

            string standardDisplayName;
            string daylightDisplayName;
            string timezoneID = KeyName;
            if (KeyName == String.Empty)
            {
                // It's invalid to have an empty timezone ID, use the ID of current timezone
                timezoneID = TimeZoneInfo.Local.Id;
            }
            string displayName = RegistryTimeZoneUtils.GetDisplayName(timezoneID, out standardDisplayName, out daylightDisplayName);
            
            if (effectiveTimeZoneRule == null)
            {
                // no daylight savings
                return TimeZoneInfo.CreateCustomTimeZone(timezoneID, effectiveTimeZoneRuleStructure.BaseUtcOffset, displayName, standardDisplayName);
            }
            else
            {
                TimeZoneInfo.AdjustmentRule[] adjustmentRules = new TimeZoneInfo.AdjustmentRule[] { effectiveTimeZoneRule };
                return TimeZoneInfo.CreateCustomTimeZone(timezoneID, effectiveTimeZoneRuleStructure.BaseUtcOffset, displayName, standardDisplayName, daylightDisplayName, adjustmentRules);
            }
        }

        public TimeZoneRuleStructure GetEffectiveTimeZoneRule()
        {
            // Note: Outlook stores a first and last 'no adjustment' rules
            for (int index = 0; index < TZRules.Length; index++)
            {
                TimeZoneRuleStructure ruleStructure = TZRules[index];
                if (ruleStructure.IsEffectiveTimeZoneRule)
                {
                    return ruleStructure;
                }
            }
            return null;
        }
        
        /// <summary>
        /// If the effective TZRule structure's lBias, lStandardBias, lDaylightBias,
        /// stStandardDate, and stDaylightDate fields are not equal to the corresponding
        /// fields in the PidLidTimeZoneStruct property,
        /// the PidLidAppointmentTimeZoneDefinitionRecur and PidLidTimeZoneStruct properties
        /// are considered inconsistent.
        /// </summary>
        public bool IsConsistent(TimeZoneStructure structure)
        {
            TimeZoneRuleStructure effectiveTimeZoneRule = null;

            foreach(TimeZoneRuleStructure timeZoneRule in TZRules)
            {
                if (timeZoneRule.IsEffectiveTimeZoneRule)
                {
                    effectiveTimeZoneRule = timeZoneRule;
                    break;
                }
            }

            if (effectiveTimeZoneRule == null)
            {
                return false;
            }
            else
            {
                return (effectiveTimeZoneRule.lBias == structure.lBias &&
                        effectiveTimeZoneRule.lStandardBias == structure.lStandardBias &&
                        effectiveTimeZoneRule.lDaylightBias == structure.lDaylightBias &&
                        effectiveTimeZoneRule.stStandardDate == structure.stStandardDate &&
                        effectiveTimeZoneRule.stDaylightDate == structure.stDaylightDate);
            }
        }

        /// Note about how Outlook selects the effective adjustment rule: 
        /// For backward compatibility reasons, the effective rule is always the static timezone
        /// rule used by the OS.
        /// It's possible however that there is no dynamic rule that matches the static rule,
        /// If that's the case, Outlook will replace the dynamic rule of the current year
        /// with the static rule, which will be marked as the effective rule.
        /// <param name="effectiveRule">null if there are no daylight savings</param>
        public static TimeZoneDefinitionStructure FromTimeZoneInfo(TimeZoneInfo timezone, TimeZoneInfo.AdjustmentRule effectiveRule, int effectiveRuleYear, bool isRecurringTimeZoneRule)
        {
            TimeZoneInfo.AdjustmentRule[] adjustmentRules = timezone.GetAdjustmentRules();
            if (effectiveRule != null)
            {
                if ((effectiveRule.DaylightTransitionStart.IsFixedDateRule || effectiveRule.DaylightTransitionEnd.IsFixedDateRule) &&
                    effectiveRule.DateStart.Year < effectiveRule.DateEnd.Year)
                {
                    // a rule can either be fixed-date (wYear is not zero) or floating-date (wYear is zero)
                    // a fixed-date rule cannot span multiple years.
                    throw new NotImplementedException("Effective DST rule cannot be fixed-date and span multiple years");
                }

                if (adjustmentRules.Length == 0)
                {
                    // effective rule != null means timezone rule is defined, but the timezone has no such rule
                    throw new ArgumentException("Time zone does not match effective rule");
                }
            }

            TimeZoneRuleFlags effectiveRuleFlags = TimeZoneRuleFlags.EffectiveTimeZoneRule;
            if (isRecurringTimeZoneRule)
            {
                effectiveRuleFlags |= TimeZoneRuleFlags.RecurringTimeZoneRule;
            }

            TimeZoneRuleStructure[] tzRules;

            if (adjustmentRules.Length == 0)
            {
                // at this point effectiveRule must be null
                TimeZoneRuleStructure ruleStructure = new TimeZoneRuleStructure();
                ruleStructure.SetBias(timezone.BaseUtcOffset, new TimeSpan());
                ruleStructure.wYear = 1601; // That's what Outlook 2007 SP3 uses
                ruleStructure.TZRuleFlags = effectiveRuleFlags;
                tzRules = new TimeZoneRuleStructure[] { ruleStructure };
            }
            else
            {
                List<TimeZoneRuleStructure> rules = new List<TimeZoneRuleStructure>();
                // First we will create a list of the dynamic rules:
                
                // we have an effective rule and at least one dynamic rule
                int firstRuleYear = adjustmentRules[0].DateStart.Year;
                if (firstRuleYear > 1)
                {
                    TimeZoneRuleStructure firstRule = new TimeZoneRuleStructure();
                    firstRule.SetBias(timezone.BaseUtcOffset, adjustmentRules[0].DaylightDelta);
                    firstRule.wYear = (ushort)(firstRuleYear - 1);
                    rules.Add(firstRule);
                }

                for (int index = 0; index < adjustmentRules.Length; index++)
                {
                    TimeZoneInfo.AdjustmentRule adjustmentRule = adjustmentRules[index];
                    TimeZoneRuleStructure rule = new TimeZoneRuleStructure();
                    rule.wYear = (ushort)adjustmentRule.DateStart.Year; ;
                    rule.SetBias(timezone.BaseUtcOffset, adjustmentRule.DaylightDelta);
                    rule.stStandardDate = AdjustmentRuleHelper.GetStandardDate(adjustmentRule);
                    rule.stDaylightDate = AdjustmentRuleHelper.GetDaylightDate(adjustmentRule);

                    rules.Add(rule);
                }

                int lastRuleYear = adjustmentRules[adjustmentRules.Length - 1].DateEnd.Year;
                if (lastRuleYear < 9999)
                {
                    TimeZoneRuleStructure lastRule = new TimeZoneRuleStructure();
                    lastRule.SetBias(timezone.BaseUtcOffset, adjustmentRules[adjustmentRules.Length - 1].DaylightDelta);
                    lastRule.wYear = (ushort)(lastRuleYear + 1);
                    rules.Add(lastRule);
                }

                // Outlook will try to find a rule that match the static rule, and will use the last rule that fits
                
                // now we will replace a dynamic rule with the appropriate static rule

                // find the index for the rule
                int effectiveRuleIndex = 0;
                for (int index = 0; index < rules.Count; index++)
                {
                    if (effectiveRuleYear >= rules[index].wYear)
                    {
                        effectiveRuleIndex = index;
                    }
                }

                // prepare the effective rule structure
                TimeZoneRuleStructure ruleStructure = new TimeZoneRuleStructure();
                ruleStructure.wYear = rules[effectiveRuleIndex].wYear;
                ruleStructure.TZRuleFlags = effectiveRuleFlags;

                if (effectiveRule == null)
                {
                    // We should have a no-daylight-savings rule
                    ruleStructure.SetBias(timezone.BaseUtcOffset, new TimeSpan());
                }
                else
                {
                    ruleStructure.SetBias(timezone.BaseUtcOffset, effectiveRule.DaylightDelta);
                    ruleStructure.stStandardDate = AdjustmentRuleHelper.GetStandardDate(effectiveRule);
                    ruleStructure.stDaylightDate = AdjustmentRuleHelper.GetDaylightDate(effectiveRule);
                }

                rules[effectiveRuleIndex] = ruleStructure;
                tzRules = rules.ToArray();
            }

            TimeZoneDefinitionStructure structure = new TimeZoneDefinitionStructure();
            structure.KeyName = timezone.Id;
            structure.TZRules = tzRules;
            return structure;
        }
    }
}
