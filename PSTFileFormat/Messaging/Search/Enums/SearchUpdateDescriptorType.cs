
namespace PSTFileFormat
{
    public enum SearchUpdateDescriptorType : ushort
    {
        SUDT_NULL = 0x00,
        SUDT_MSG_ADD = 0x01,
        SUDT_MSG_MOD = 0x02,
        SUDT_MSG_DEL = 0x03,
        SUDT_MSG_MOV = 0x04,
        SUDT_FLD_ADD = 0x05,
        SUDT_FLD_MOD = 0x06,
        SUDT_FLD_DEL = 0x07,
        SUDT_FLD_MOV = 0x08,
        SUDT_SRCH_ADD = 0x09,
        SUDT_SRCH_MOD = 0x0A,
        SUDT_SRCH_DEL = 0x0B,
        SUDT_MSG_ROW_MOD = 0x0C,
        SUDT_MSG_SPAM = 0x0D,
        SUDT_IDX_MSG_DEL = 0x0E,
        SUDT_MSG_IDX = 0x0F,
    }
}
