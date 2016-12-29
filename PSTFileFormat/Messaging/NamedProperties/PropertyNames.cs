/* Copyright (C) 2012-2016 ROM Knowledgeware. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * 
 * Maintainer: Tal Aloni <tal@kmrom.com>
 */
using System;

namespace PSTFileFormat
{
    public class PropertyNames
    {
        public static readonly PropertyName PidLidTaskMode = new PropertyName(PropertyLongID.PidLidTaskMode, PropertySetGuid.PSETID_Common);
        public static readonly PropertyName PidLidSideEffects = new PropertyName(PropertyLongID.PidLidSideEffects, PropertySetGuid.PSETID_Common);
        public static readonly PropertyName PidLidCommonStart = new PropertyName(PropertyLongID.PidLidCommonStart, PropertySetGuid.PSETID_Common);
        public static readonly PropertyName PidLidCommonEnd = new PropertyName(PropertyLongID.PidLidCommonEnd, PropertySetGuid.PSETID_Common);
        public static readonly PropertyName PidLidPrivate = new PropertyName(PropertyLongID.PidLidPrivate, PropertySetGuid.PSETID_Common);
        public static readonly PropertyName PidLidReminderSet = new PropertyName(PropertyLongID.PidLidReminderSet, PropertySetGuid.PSETID_Common);
        public static readonly PropertyName PidLidHeaderItem = new PropertyName(PropertyLongID.PidLidHeaderItem, PropertySetGuid.PSETID_Common);
        public static readonly PropertyName PidLidSmartNoAttach = new PropertyName(PropertyLongID.PidLidSmartNoAttach, PropertySetGuid.PSETID_Common);	
        
        public static readonly PropertyName PidLidRecurring = new PropertyName(PropertyLongID.PidLidRecurring, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidLocation = new PropertyName(PropertyLongID.PidLidLocation, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidAppointmentColor = new PropertyName(PropertyLongID.PidLidAppointmentColor, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidConferencingType = new PropertyName(PropertyLongID.PidLidConferencingType, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidClipStart = new PropertyName(PropertyLongID.PidLidClipStart, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidClipEnd = new PropertyName(PropertyLongID.PidLidClipEnd, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidAppointmentStartWhole = new PropertyName(PropertyLongID.PidLidAppointmentStartWhole, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidAppointmentEndWhole = new PropertyName(PropertyLongID.PidLidAppointmentEndWhole, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidAppointmentSubType = new PropertyName(PropertyLongID.PidLidAppointmentSubType, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidBusyStatus = new PropertyName(PropertyLongID.PidLidBusyStatus, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidFInvited = new PropertyName(PropertyLongID.PidLidFInvited, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidAppointmentStateFlags = new PropertyName(PropertyLongID.PidLidAppointmentStateFlags, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidAppointmentRecur = new PropertyName(PropertyLongID.PidLidAppointmentRecur, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidTimeZoneDescription = new PropertyName(PropertyLongID.PidLidTimeZoneDescription, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidTimeZoneStruct = new PropertyName(PropertyLongID.PidLidTimeZoneStruct, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidAppointmentTimeZoneDefinitionRecur = new PropertyName(PropertyLongID.PidLidAppointmentTimeZoneDefinitionRecur, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidAppointmentTimeZoneDefinitionStartDisplay = new PropertyName(PropertyLongID.PidLidAppointmentTimeZoneDefinitionStartDisplay, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidAppointmentTimeZoneDefinitionEndDisplay = new PropertyName(PropertyLongID.PidLidAppointmentTimeZoneDefinitionEndDisplay, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidMeetingWorkspaceUrl = new PropertyName(PropertyLongID.PidLidMeetingWorkspaceUrl, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidAppointmentDuration = new PropertyName(PropertyLongID.PidLidAppointmentDuration, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidExceptionReplaceTime = new PropertyName(PropertyLongID.PidLidExceptionReplaceTime, PropertySetGuid.PSETID_Appointment);
        public static readonly PropertyName PidLidSendMeetingAsIcal = new PropertyName(PropertyLongID.PidLidSendMeetingAsIcal, PropertySetGuid.PSETID_Appointment);
        
        public static readonly PropertyName PidLidIsRecurring = new PropertyName(PropertyLongID.PidLidIsRecurring, PropertySetGuid.PSETID_Meeting);
    }
}
