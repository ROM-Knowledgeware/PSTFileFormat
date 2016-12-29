
namespace PSTFileFormat
{
    public enum MessagePriority : uint
    {
        Normal = 0x00000000,
        Urgent = 0x00000001,
        NotUrgent = 0xFFFFFFFF
    }
}
