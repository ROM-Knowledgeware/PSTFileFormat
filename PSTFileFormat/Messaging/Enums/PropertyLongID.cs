
namespace PSTFileFormat
{
    /// <summary>
    /// For named properties only, not to be confused with Property ID
    /// </summary>
    public enum PropertyLongID : uint
    {
        PidLidGlobalObjectId = 0x00000003,        // Binary
        PidLidIsRecurring = 0x00000005,           // Boolean
        Unknown0x0021 = 0x00000021,               // Int32
        PidLidCleanGlobalObjectId = 0x00000023,   // Binary
        PidLidTaskStatus = 0x00008101,            // Int32
        PidLidPercentComplete = 0x00008102,       // Float64
        PidLidTeamTask = 0x00008103,              // Boolean
        PidLidTaskStartDate = 0x00008104,         // DateTime
        PidLidTaskDueDate = 0x00008105,           // DateTime
        PidLidTaskActualEffort = 0x00008110,      // Int32
        PidLidTaskEstimatedEffort = 0x00008111,   // Int32
        PidLidTaskVersion = 0x00008112,           // Int32
        PidLidTaskState = 0x00008113,             // Int32
        PidLidTaskComplete = 0x0000811C,          // Boolean
        PidLidTaskAssigner = 0x00008121,          // String
        PidLidTaskOrdinal = 0x00008123,           // Int32
        PidLidTaskNoCompute = 0x00008124,         // Boolean
        PidLidTaskFRecurring = 0x00008126,        // Boolean
        PidLidTaskRole = 0x00008127,              // String
        PidLidTaskOwnership = 0x00008129,         // Int32
        PidLidTaskAcceptanceState = 0x0000812A,   // Int32
        PidLidTaskFFixOffline = 0x0000812C,       // Boolean
        PidLidSendMeetingAsIcal = 0x00008200,     // Boolean
        PidLidAppointmentSequence = 0x00008201,   // Int32
        PidLidBusyStatus = 0x00008205,            // Int32
        PidLidFExceptionalBody = 0x00008206,      // Boolean
        PidLidAppointmentAuxiliaryFlags = 0x00008207, // Int32
        PidLidLocation = 0x00008208,              // String
        PidLidMeetingWorkspaceUrl = 0x00008209,   // String
        PidLidAppointmentEndWhole = 0x0000820E,   // DateTime
        PidLidAppointmentStartWhole = 0x0000820D, // DateTime
        PidLidAppointmentDuration = 0x00008213,   // Int32
        PidLidAppointmentColor = 0x00008214,      // Int32
        PidLidAppointmentSubType = 0x00008215,    // Boolean, specify whether this is an all day event
        PidLidAppointmentRecur = 0x00008216,      // Binary
        PidLidAppointmentStateFlags = 0x00008217, // Int32
        PidLidResponseStatus = 0x00008218,        // Int32
        PidLidRecurring = 0x00008223,             // Boolean
        PidLidIntendedBusyStatus = 0x00008224,    // Int32
        PidLidExceptionReplaceTime = 0x00008228,  // DateTime
        PidLidFInvited = 0x00008229,              // Boolean
        PidLidFExceptionalAttendees = 0x0000822B, // Boolean
        PidLidRecurrenceType = 0x00008231,        // Int32
        PidLidRecurrencePattern = 0x00008232,     // String
        PidLidTimeZoneStruct = 0x00008233,        // Binary
        PidLidTimeZoneDescription = 0x00008234,   // String
        PidLidClipStart = 0x00008235,             // DateTime
        PidLidClipEnd = 0x00008236,               // DateTime
        PidLidAutoFillLocation = 0x0000823A,      // Boolean
        PidLidConferencingType = 0x00008241,      // Int32
        PidLidDirectory = 0x00008242,             // String
        PidLidOrganizerAlias = 0x00008243,        // String
        PidLidAutoStartCheck = 0x00008244,        // Boolean
        PidLidAutoStartWhen = 0x00008245,         // Int32, deprecated
        PidLidAllowExternalCheck = 0x00008246,    // Boolean
        PidLidCollaborateDoc = 0x00008247,        // String
        PidLidNetShowUrl = 0x00008248,            // String
        PidLidOnlinePassword = 0x00008249,        // String
        PidLidAppointmentProposedDuration = 0x00008256, // Int32
        PidLidAppointmentCounterProposal = 0x00008257, // Boolean
        PidLidAppointmentProposalNumber = 0x00008259, // Int32
        PidLidAppointmentNotAllowPropose = 0x0000825A, // Boolean
        PidLidAppointmentTimeZoneDefinitionStartDisplay = 0x0000825E, // Binary
        PidLidAppointmentTimeZoneDefinitionEndDisplay = 0x0000825F, // Binary
        PidLidAppointmentTimeZoneDefinitionRecur = 0x00008260, // Binary
        PidLidReminderDelta = 0x00008501,         // Int32
        PidLidReminderTime = 0x00008502,          // DateTime
        PidLidReminderSet = 0x00008503,           // Boolean
        PidLidPrivate = 0x00008506,               // Boolean
        PidLidAgingDontAgeMe = 0x0000850E,        // Boolean
        PidLidSideEffects = 0x00008510,           // Int32
        PidLidSmartNoAttach = 0x00008514,         // Boolean
        PidLidCommonStart = 0x00008516,           // DateTime
        PidLidCommonEnd = 0x00008517,             // DateTime
        PidLidTaskMode = 0x00008518,              // Int32
        PidLidCurrentVersion = 0x00008552,        // Int32
        PidLidCurrentVersionName = 0x00008554,    // String
        PidLidReminderSignalTime = 0x00008560,    // DateTime
        PidLidHeaderItem = 0x00008578,            // Int32
        PidLidInternetAccountName = 0x00008580,   // String
        PidLidInternetAccountStamp = 0x00008581,  // String
        PidLidContactLinkName = 0x00008586,       // String
        Unknown0x000085a8 = 0x000085a8,           // Binary, appears at appointment contents table (Outlook 2010)
        PidLidValidFlagStringProof = 0x000085BF,  // DateTime
        PidLidRemoteAttachment = 0x00008F07,      // Boolean
    }
}
