using System;

namespace PSTFileFormat
{
    [Flags]
    public enum SearchUpdateDescriptorFlags : ushort
    {
        SUDF_PRIORITY_LOW = 0x0001,
        SUDF_PRIORITY_HIGH = 0x0002,
        SUDF_SEARCH_RESTART = 0x0004,
        SUDF_NAME_CHANGED = 0x0008,
        SUDF_MOVE_OUT_TO_IN = 0x0010,
        SUDF_MOVE_IN_TO_IN = 0x0020,
        SUDF_MOVE_IN_TO_OUT = 0x0040,
        SUDF_MOVE_OUT_TO_OUT = 0x0080,
        SUDF_SPAM_CHECK_SERVER = 0x0100,
        SUDF_SET_DEL_NAME = 0x0200,
        SUDF_SRCH_DONE = 0x0400,
        SUDF_DOMAIN_CHECKED = 0x8000,
    }
}
