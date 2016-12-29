
namespace PSTFileFormat
{
    public enum bCryptMethodName : byte
    {
        NDB_CRYPT_NONE = 0x00,    // No Encryption
        NDB_CRYPT_PERMUTE = 0x01, // Compressible Encryption
        NDB_CRYPT_CYCLIC = 0x02,  // HighEncryption
    }
}
