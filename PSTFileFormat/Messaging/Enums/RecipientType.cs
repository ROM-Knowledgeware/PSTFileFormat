
namespace PSTFileFormat
{
    // http://msdn.microsoft.com/en-us/library/ee200746%28v=exchg.80%29.aspx
    public enum RecipientType : uint
    {
        To = 0x0001,  // Required Atendee
        Cc = 0x0002,  // Optional Atendee
        Bcc = 0x0003, // Resource (Room or Equipment)
    }
}
