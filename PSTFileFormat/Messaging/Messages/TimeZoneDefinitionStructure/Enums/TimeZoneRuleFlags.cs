using System;

namespace PSTFileFormat
{
    [Flags]
    public enum TimeZoneRuleFlags
    {
        RecurringTimeZoneRule = 0x01, // TZRULE_FLAG_RECUR_CURRENT_TZREG, This flag specifies that this rule (4) is associated with a recurring series
        EffectiveTimeZoneRule = 0x02, // TZRULE_FLAG_EFFECTIVE_TZREG
    }
}
