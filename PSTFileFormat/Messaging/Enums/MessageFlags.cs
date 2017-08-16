using System;
using System.Collections.Generic;
using System.Text;

namespace PSTFileFormat
{
    [Flags]
    public enum MessageFlags : int
    {
        MSGFLAG_READ = 0x01,
        MSGFLAG_UNMODIFIED = 0x02,
        MSGFLAG_SUBMIT = 0x04,
        MSGFLAG_UNSENT = 0x08,
        MSGFLAG_HASATTACH = 0x10,
    }
}
