
namespace PSTFileFormat
{
    public enum PageTypeName : byte
    {
        ptypeBBT = 0x80,   // Block BTree page.
        ptypeNBT = 0x81,   // Node BTree page
        ptypeFMap = 0x82,  // Free Map page
        ptypePMap = 0x83,  // Allocation Page Map page
        ptypeAMap = 0x84,  // Allocation Map page
        ptypeFPMap = 0x85, // Free Page Map page
        ptypeDL = 0x86,    // Density List page
    }
}
