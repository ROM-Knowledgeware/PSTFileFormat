/* Copyright (C) 2012-2016 ROM Knowledgeware. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * 
 * Maintainer: Tal Aloni <tal@kmrom.com>
 */
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace PSTFileFormat
{
    public class SingleAppointment : Appointment // represents single-instance appointment
    {
        public SingleAppointment(PSTNode node) : base(node)
        {
            this.Recurring = false;
            //this.IsRecurring = false;
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
        }

        public override void SetStartAndDuration(DateTime startDTUtc, int duration)
        {
            // We work in UTC, this way we avoid any oddities that are due to daylight savings
            DateTime endDTUtc = startDTUtc.AddMinutes(duration);

            this.StartDTUtc = startDTUtc;
            this.EndDTUtc = endDTUtc;

            // To prevent daylight savings mistakes, we simply do not set PidLidAppointmentDuration
        }

        /// <param name="dynamicTimeZone">can be set to null if not available</param>
        public override void SetOriginalTimeZone(TimeZoneInfo staticTimeZone, TimeZoneInfo dynamicTimeZone, int effectiveYear)
        {
            this.TimeZoneDescription = staticTimeZone.DisplayName; // Outlook 2003 RTM writes this for single appointments as well
            if (this.File.WriterCompatibilityMode >= WriterCompatibilityMode.Outlook2003SP3)
            {
                TimeZoneInfo.AdjustmentRule effectiveRule = AdjustmentRuleUtils.GetFirstRule(staticTimeZone);
                TimeZoneInfo definitionTimezone = (dynamicTimeZone == null) ? staticTimeZone : dynamicTimeZone;
                this.TimeZoneDefinitionStartDisplay = TimeZoneDefinitionStructure.FromTimeZoneInfo(definitionTimezone, effectiveRule, effectiveYear, false);
                // We do not set TimeZoneDefinitionEndDisplay, so Outlook will use TimeZoneDefinitionStartDisplay.
            }
        }

        /// <summary>
        /// Outlook will use StartWhole (part of the contents table since Outlook 2003) over ClipStart, and so should we.
        /// According to MS-OXOCAL, ClipStart & CommonStart MUST be equal to StartWhole,
        /// In practive however, I've encountered a case where ClipStart wasn't equal
        /// to StartWhole for a single appointments. (CommonStart was equal to StartWhole in that specific case).
        /// </summary>
        public override DateTime StartDTUtc
        {
            get
            {
                return this.StartWhole;
            }
            set
            {
                this.ClipStart = value;
                this.CommonStart = value;
                this.StartWhole = value;
            }
        }

        public DateTime EndDTUtc
        {
            get
            {
                return this.EndWhole;
            }
            set
            {
                this.ClipEnd = value;
                this.CommonEnd = value;
                this.EndWhole = value;
            }
        }

        public override int Duration
        {
            get
            {
                TimeSpan ts = EndDTUtc - StartDTUtc;
                return (int)ts.TotalMinutes;
            }
        }

        public override TimeZoneInfo OriginalTimeZone
        {
            get
            {
                TimeZoneDefinitionStructure timezoneDefinitionStructure = this.TimeZoneDefinitionStartDisplay;
                
                if (timezoneDefinitionStructure == null) // No Outlook 2007+ structure
                {
                    // assume current system time zone definition
                    return TimeZoneInfoUtils.GetSystemStaticTimeZone(TimeZoneInfo.Local.Id);
                }
                else // Outlook 2007+ structure present
                {
                    return timezoneDefinitionStructure.ToTimeZoneInfo();
                }
            }
        }

        public override bool HasTimeZoneDefinition
        {
            get 
            {
                return (this.TimeZoneDefinitionStartDisplay != null);
            }
        }

        public static SingleAppointment CreateNewSingleAppointment(PSTFile file, NodeID parentNodeID)
        { 
            return CreateNewSingleAppointment(file, parentNodeID, Guid.NewGuid());
        }

        public static SingleAppointment CreateNewSingleAppointment(PSTFile file, NodeID parentNodeID, Guid searchKey)
        {
            MessageObject message = CreateNewMessage(file, FolderItemTypeName.Appointment, parentNodeID, searchKey);
            SingleAppointment appointment = new SingleAppointment(message);
            appointment.MessageFlags = MessageFlags.MSGFLAG_READ;
            appointment.InternetCodepage = 1255;
            appointment.MessageDeliveryTime = DateTime.UtcNow;
            appointment.ClientSubmitTime = DateTime.UtcNow;
            appointment.SideEffects = SideEffectsFlags.seOpenForCtxMenu | SideEffectsFlags.seOpenToMove | SideEffectsFlags.seOpenToCopy | SideEffectsFlags.seCoerceToInbox | SideEffectsFlags.seOpenToDelete;
            appointment.Importance = MessageImportance.Normal;
            appointment.Priority = MessagePriority.Normal;
            appointment.ConferencingType = MeetingType.WindowsNetmeeting;

            appointment.IconIndex = IconIndex.SingleInstanceAppointment;
            return appointment;
        }
    }
}
