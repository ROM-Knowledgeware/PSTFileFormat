
namespace PSTFileFormat
{
    public enum BlockType : byte // btype
    {
        XBlock  = 0x01, // XBlock and XXBlock has the same btype and different cLevel
        XXBlock = 0x01, // XBlock and XXBlock has the same btype and different cLevel

        SLBLOCK = 0x02, // SLBLOCK and SIBLOCK has the same btype and different cLevel
        SIBLOCK = 0x02, // SLBLOCK and SIBLOCK has the same btype and different cLevel
    }
}
