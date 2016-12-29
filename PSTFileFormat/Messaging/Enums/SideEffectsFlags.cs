
namespace PSTFileFormat
{
    public enum SideEffectsFlags
    {
        seOpenToDelete = 0x00000001,
        seNoFrame = 0x00000008,
        seCoerceToInbox = 0x00000010,
        seOpenToCopy = 0x00000020,
        seOpenToMove = 0x00000040,
        seOpenForCtxMenu = 0x00000100,
    }
}
