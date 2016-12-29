
namespace PSTFileFormat
{
    public enum PropertyID : ushort
    {
        /* Message / Appointment related */
        PidTagNameidBucketCount = 0x0001,    // Int32
        PidTagAlternateRecipientAllowed = 0x0002, // Boolean
        PidTagNameidStreamGuid = 0x0002,     // Binary
        PidTagNameidStreamEntry = 0x0003,    // Binary
        PidTagNameidStreamString = 0x0004,   // Binary
        PidTagImportance = 0x0017,
        PidTagMessageClass = 0x001A,
        PidTagOriginatorDeliveryReportRequested = 0x0023,
        PidTagPriority = 0x0026,
        PidTagReadReceiptRequested = 0x0029,
        PidTagReplyTime = 0x0030,            // DateTime
        PidTagRecipientReassignmentProhibited = 0x002B, // Boolean
        PidTagSensitivity = 0x0036,          // Int32, 0 = Normal, 1 = Personal, 2 = Private, 3 = Confidential
        PidTagSubject = 0x0037,
        PidTagClientSubmitTime = 0x0039,
        PidTagSentRepresentingSearchKey = 0x003B,
        PidTagSentRepresentingEntryId = 0x0041,
        PidTagSentRepresentingName = 0x0042,
        PidTagMessageToMe = 0x0057,          // Boolean
        PidTagMessageCcMe = 0x0058,          // Boolean
        PidTagStartDate = 0x0060,            // DateTime
        PidTagEndDate = 0x0061,              // DateTime
        PidTagOwnerAppointmentId = 0x0062,   // Int32
        PidTagResponseRequested = 0x0063,
        PidTagSentRepresentingAddressType = 0x0064,
        PidTagSentRepresentingEmailAddress = 0x0065,
        PidTagConversationTopic = 0x0070,
        PidTagConversationIndex = 0x0071,
        PidTagRecipientType = 0x0C15,        // Int32
        PidTagReplyRequested = 0x0C17,
        PidTagSenderEntryId = 0x0C19,
        PidTagSenderName = 0x0C1A,
        PidTagSenderSearchKey = 0x0C1D,
        PidTagSenderAddressType = 0x0C1E,    // 'SMTP'
        PidTagSenderEmailAddress = 0x0C1F,
        PidTagDeleteAfterSubmit = 0x0E01,
        PidTagDisplayCc = 0x0E03,            // String
        PidTagDisplayTo = 0x0E04,            // String
        PidTagMessageDeliveryTime = 0x0E06,
        PidTagMessageFlags = 0x0E07,
        PidTagMessageSize = 0x0E08,
        PidTagResponsibility = 0x0E0F,       // Boolean
        PidTagMessageStatus = 0x0E17,        // Int32
        PidTagAttachSize = 0x0E20,           // Int32, a.k.a. PidTagAttachmentSize
        PidTagToDoItemFlags = 0x0E2B,        // Int32
        PidTagReplItemid = 0x0E30,           // Int32
        PidTagReplChangenum = 0x0E33,        // Int64
        PidTagReplVersionHistory = 0x0E34,   // Binary
        PidTagReplFlags = 0x0E38,            // Int32
        PidTagReplCopiedfromVersionhistory = 0x0E3C, // Binary
        PidTagReplCopiedfromItemid = 0x0E3D, // Binary
        Unknown0x0EA2 = 0x0EA2,              // Int32
        PidTagRecordKey = 0x0FF9,            // Binary
        PidTagObjectType = 0x0FFE,           // Int32
        PidTagEntryId = 0x0FFF,              // Binary

        PidTagBody = 0x1000,                 // String
        PidTagNameidBucketBase = 0x1000,     // Binary
        PidTagRtfCompressed = 0x1009,        // Binary
        PidTagInternetMessageId = 0x1035,    // String
        PidTagIconIndex = 0x1080,
        PidTagLastVerbExecuted = 0x1081,     // Int32
        PidTagLastVerbExecutionTime = 0x1082, // DateTime
        PidTagFlagStatus = 0x1090,           // Int32
        PidTagFollowupIcon = 0x1095,         // Int32
        PidTagItemTemporaryFlags = 0x1097,   // Int32
        PidTagAttributeHidden = 0x10F4,      // Boolean

        PidTagDisplayName = 0x3001,          // String, Display name of sub-Folder object
        PidTagAddressType = 0x3002,          // String
        PidTagEmailAddress = 0x3003,         // String
        PidTagComment = 0x3004,              // String
        PidTagCreationTime = 0x3007,         // DateTime
        PidTagLastModificationTime = 0x3008, // DateTime
        PidTagSearchKey = 0x300B,            // Binary
        PidTagConversationId = 0x3013,       // Binary
        
        /* Folder related */
        PidTagContentCount = 0x3602,         // Int32
        PidTagContentUnreadCount = 0x3603,   // Int32
        PidTagSubfolders = 0x360A,           // Boolean
        PidTagContainerClass = 0x3613,       // Container class of the sub-Folder object
        PidTagExtendedFolderFlags = 0x36DA,  // Binary
        PR_NET_FOLDER_FLAGS = 0x36DE,        // Int32

        /* Attachment related */
        PidTagAttachData = 0x3701,           // Binary or Object (Node), Note: PidTagAttachDataBinary or PidTagAttachDataObject have the same propertyID
        PidTagAttachEncoding = 0x3702,       // String (Empty)
        PidTagAttachExtension = 0x3703,      // String
        PidTagAttachFilename = 0x3704,       // String
        PidTagAttachMethod = 0x3705,         // Int32
        PidTagAttachLongFilename = 0x3707,   // String
        PidTagAttachPathname = 0x3708,       // String
        PidTagAttachRendering = 0x3709,      // Binary, WMF icon
        PidTagAttachTag = 0x370A,            // Binary
        PidTagRenderingPosition = 0x370B,    // Int32
        PidTagAttachLongPathname = 0x370D,   // String
        PidTagAttachMimeTag = 0x370E,        // String
        PidTagAttachAdditionalInformation = 0x370F, // Binary
        PidTagAttachContentBase = 0x3711,    // String
        PidTagAttachContentId = 0x3712,      // String
        PidTagAttachContentLocation = 0x3713,// String
        PidTagAttachFlags = 0x3714,          // Int32
        PidTagAttachPayloadProviderGuidString = 0x3719, // String
        PidTagAttachPayloadClass = 0x371A,   // String
        PidTagDisplayType = 0x3900,          // Int32
        PidTagAddressBookDisplayNamePrintable = 0x39FF, // String
        PidTagSendRichInfo = 0x3A40,         // Boolean
        PidTagSendInternetEncoding = 0x3A71, // Int32
        PidTagInternetCodepage = 0x3FDE,     // Int32, Indicates the code page used for the PidTagBody property
        PidTagMessageLocaleId = 0x3FF1,      // Int32
        
        PidTagRecipientDisplayName = 0x5FF6, // String
        PidTagRecipientEntryId = 0x5FF7,     // Binary
        PidTagRecipientTrackStatusTime = 0x5FFB, // DateTime 
        PidTagRecipientFlags = 0x5FFD,       // Int32
        PidTagRecipientTrackStatus = 0x5FFF, // Int32

        PR_COM_CRIT_EID = 0x6001,            // String, (recipients TC, email address)
        PidTagSecureSubmitFlags = 0x65C6,    // Int32
        MessageTotalAttachmentSize = 0x6604, // Int32
        PR_INTERNET_MESSAGE_FORMAT = 0x6610, // Int32, added when a first participant was added
        PidTagUserEntryId = 0x6619,          // String, Note: this is not PidTagUserEntryId (Binary)
        PidTagPstHiddenCount = 0x6635,       // Int32
        PidTagPstHiddenUnread = 0x6636,      // Int32
        PR_LONGTERM_ENTRYID_FROM_TABLE = 0x6670, // Binary

        PR_MANAGED_FOLDER_INFORMATION = 0x672D, // Int32
        Unknown0x6730 = 0x6730,              // Int64
        PR_MANAGED_FOLDER_STORAGE_QUOTA  = 0x6731, // Int32
        PR_MANAGED_FOLDER_ID = 0x6732,       // String
        PR_MANAGED_FOLDER_COMMENT = 0x6733,  // String
        PidTagLtpRowId = 0x67F2,             // Int32, Appear in hierarchy table (TC), this is the NodeID
        PidTagLtpRowVer = 0x67F3,            // Int32
        Unknown0x6909 = 0x6909,              // Int32
        PR_SECURITY_FLAGS = 0x6E01,          // Int32

        PidTagAttachmentLinkId = 0x7FFA,     // Int32
        PidTagExceptionStartTime = 0x7FFB,   // DateTime, Timezone time!
        PidTagExceptionEndTime = 0x7FFC,     // DateTime, Timezone time!
        PidTagAttachmentFlags = 0x7FFD,      // Int32
        PidTagAttachmentHidden = 0x7FFE,     // Boolean
        PidTagExceptionReplaceTime = 0x7FF9, // DateTime, Original StartDT in UTC
        PidTagAttachmentContactPhoto = 0x7FFF, // Boolean

        // Note: >= 0x8000 are reserved for named properties
    }
}
