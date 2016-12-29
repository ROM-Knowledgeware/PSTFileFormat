using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public class RegistryTimeZoneInformation // _REG_TZI_FORMAT
    {
        public const int Length = 44;

        public int Bias;
        public int StandardBias;
        public int DaylightBias;
        public SystemTime StandardDate;
        public SystemTime DaylightDate;

        public RegistryTimeZoneInformation(byte[] bytes)
        {
            if ((bytes == null) || (bytes.Length != 0x2c))
            {
                throw new ArgumentException("Invalid REG_TZI_FORMAT");
            }
            this.Bias = LittleEndianConverter.ToInt32(bytes, 0);
            this.StandardBias = LittleEndianConverter.ToInt32(bytes, 4);
            this.DaylightBias = LittleEndianConverter.ToInt32(bytes, 8);
            this.StandardDate = new SystemTime(bytes, 12);
            this.DaylightDate = new SystemTime(bytes, 28);
        }
    }
}
