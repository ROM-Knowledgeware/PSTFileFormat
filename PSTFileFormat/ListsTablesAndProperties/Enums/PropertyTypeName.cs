
namespace PSTFileFormat
{
    public enum PropertyTypeName
    {
        PtypInteger16 = 0x0002,
        PtypInteger32 = 0x0003,
        PtypFloating32 = 0x0004,
        PtypFloating64 = 0x0005,
        PtypErrorCode = 0x000A,  // 4 bytes
        PtypBoolean = 0x000B,
        PtypObject = 0x000D,
        PtypInteger64 = 0x0014,
        PtypString8 = 0x001E,
        PtypString = 0x001F,
        PtypTime = 0x0040,
        PtypGuid = 0x0048,
        PtypBinary = 0x0102,
        PtypMultiString = 0x101F,
    }
}
