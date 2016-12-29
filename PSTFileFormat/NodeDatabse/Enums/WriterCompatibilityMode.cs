
namespace PSTFileFormat
{
    public enum WriterCompatibilityMode
    {
        Outlook2003RTM,
        Outlook2003SP3, // Will write TimeZoneDefinitionStartDisplay / EndDisplay
        Outlook2007RTM, // Do not use DList
        Outlook2007SP2, // Use DList
        Outlook2010RTM,
    }
}
