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
    public class RecurrenceTypeHelper
    {
        public static RecurrenceType GetRecurrenceType(RecurrenceFrequency recurrenceFrequency, PatternType patternType)
        {
            if (recurrenceFrequency == RecurrenceFrequency.Daily && patternType == PatternType.Day)
            {
                return RecurrenceType.EveryNDays;
            }
            else if (recurrenceFrequency == RecurrenceFrequency.Daily && patternType == PatternType.Week)
            {
                return RecurrenceType.EveryWeekday;
            }
            else if (recurrenceFrequency == RecurrenceFrequency.Weekly && patternType == PatternType.Week)
            {
                return RecurrenceType.EveryNWeeks;
            }
            else if (recurrenceFrequency == RecurrenceFrequency.Monthly && patternType == PatternType.Month)
            {
                return RecurrenceType.EveryNMonths;
            }
            else if (recurrenceFrequency == RecurrenceFrequency.Monthly && patternType == PatternType.MonthNth)
            {
                return RecurrenceType.EveryNthDayOfEveryNMonths;
            }
            else if (recurrenceFrequency == RecurrenceFrequency.Yearly && patternType == PatternType.Month)
            {
                return RecurrenceType.EveryNYears;
            }
            else if (recurrenceFrequency == RecurrenceFrequency.Yearly && patternType == PatternType.MonthNth)
            {
                return RecurrenceType.EveryNthDayOfEveryNYears;
            }
            else
            {
                throw new InvalidRecurrencePatternException("Invalid combination of RecurrenceFrequency and PatternType");
            }
        }

        public static RecurrenceFrequency GetRecurrenceFrequency(RecurrenceType recurrenceType)
        {
            switch (recurrenceType)
            { 
                case RecurrenceType.EveryNDays:
                case RecurrenceType.EveryWeekday:
                    return RecurrenceFrequency.Daily;
                case RecurrenceType.EveryNWeeks:
                    return RecurrenceFrequency.Weekly;
                case RecurrenceType.EveryNMonths:
                case RecurrenceType.EveryNthDayOfEveryNMonths:
                    return RecurrenceFrequency.Monthly;
                case RecurrenceType.EveryNYears:
                case RecurrenceType.EveryNthDayOfEveryNYears:
                    return RecurrenceFrequency.Yearly;
                default:
                    throw new ArgumentException("Invalid recurrence type");
            }
        }

        public static PatternType GetPatternType(RecurrenceType recurrenceType)
        {
            switch (recurrenceType)
            {
                case RecurrenceType.EveryNDays:
                    return PatternType.Day;
                case RecurrenceType.EveryWeekday:
                case RecurrenceType.EveryNWeeks:
                    return PatternType.Week;
                case RecurrenceType.EveryNMonths:
                case RecurrenceType.EveryNYears:
                    return PatternType.Month;
                case RecurrenceType.EveryNthDayOfEveryNMonths:
                case RecurrenceType.EveryNthDayOfEveryNYears:
                    return PatternType.MonthNth;
                default:
                    throw new ArgumentException("Invalid recurrence type");
            }
        }
    }
}
