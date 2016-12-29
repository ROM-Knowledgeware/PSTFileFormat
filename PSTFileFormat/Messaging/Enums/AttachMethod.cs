
namespace PSTFileFormat
{
    public enum AttachMethod : uint
    {
        None = 0,
        ByValue = 1,
        ByReference = 2,
        ByReferenceOnly = 4,
        EmbeddedMessage = 5,
        Storage = 6,
    }
}
