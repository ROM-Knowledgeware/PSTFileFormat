using System;

namespace PSTFileFormat
{
    public class PropertySetGuid
    {
        // http://msdn.microsoft.com/en-us/library/ee219487%28v=exchg.80%29.aspx
        public static readonly Guid PS_MAPI = new Guid("{00020328-0000-0000-C000-000000000046}");
        public static readonly Guid PS_PUBLIC_STRINGS = new Guid("{00020329-0000-0000-C000-000000000046}");
                
        public static readonly Guid PS_INTERNET_HEADERS = new Guid("{00020386-0000-0000-C000-000000000046}");
        public static readonly Guid PSETID_Common = new Guid("{00062008-0000-0000-C000-000000000046}");
        public static readonly Guid PSETID_Appointment = new Guid("{00062002-0000-0000-C000-000000000046}");
        public static readonly Guid PSETID_Meeting = new Guid("{6ED8DA90-450B-101B-98DA-00AA003F1305}");
    }
}
