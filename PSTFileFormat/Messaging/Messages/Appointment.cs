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
    public abstract class Appointment : MessageObject
    {
        protected Appointment(PSTNode node) : base(node)
        {
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
        }

        public abstract void SetStartAndDuration(DateTime startDTUtc, int duration);

        public void SetOriginalTimeZone(TimeZoneInfo staticTimeZone)
        {
            int effectiveYear = DateTime.Now.Year;
            TimeZoneInfo.AdjustmentRule[] adjustmentRules = staticTimeZone.GetAdjustmentRules();
            if (adjustmentRules.Length == 1)
            {
                if (adjustmentRules[0].DateStart.Year == adjustmentRules[0].DateEnd.Year)
                {
                    effectiveYear = adjustmentRules[0].DateStart.Year;
                }
            }
            SetOriginalTimeZone(staticTimeZone, null, effectiveYear);
        }

        /// <param name="dynamicTimeZone">can be set to null if not available</param>
        public abstract void SetOriginalTimeZone(TimeZoneInfo staticTimeZone, TimeZoneInfo dynamicTimeZone, int effectiveYear);

        public abstract DateTime StartDTUtc
        {
            get;
            set;
        }

        public abstract int Duration
        {
            get;
        }

        /// <summary>
        /// Outlook 2007 SP3 has a weird bug:
        /// On a day that has a DST transition rule such as 02:00 -> 03:00,
        /// Appointment that start at 02:30 and end at 03:00 will have a negative UTC duration.
        /// (as reflected by ClipStart and ClipEnd).
        /// Outlook will later ignore such appointments.
        /// </summary>
        public bool IsDurationValid
        {
            get
            {
                if (Duration < 0)
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// - This property is not required.
        /// - The number of minutes from start to end in timezone time. (UNDOCUMENTED)
        /// - Single appointment: 
        ///   Due to daylight savings, there could be a difference between the timezone duration and the UTC duration.
        /// </summary>
        public int AppointmentDuration
        {
            get
            {
                return PC.GetInt32Property(PropertyNames.PidLidAppointmentDuration, 0);
            }
            set
            {
                PC.SetInt32Property(PropertyNames.PidLidAppointmentDuration, (int)value);
            }
        }

        /// <summary>
        /// Single Appointment: start date and time of the event in UTC.
        /// Recurring Appointment: midnight on the date of the first instance in UTC
        /// Note: for single appointments, StartWhole has precedence.
        /// </summary>
        protected DateTime ClipStart
        {
            get
            {
                return PC.GetDateTimeProperty(PropertyNames.PidLidClipStart, DateTime.MinValue);
            }
            set
            {
                PC.SetDateTimeProperty(PropertyNames.PidLidClipStart, value);
            }
        }

        /// <summary>
        /// Single Appointment: end date and time of the event in UTC.
        /// Recurring Appointment: midnight on the date of the last instance in UTC
        /// (unless the recurring series has no end, in which case the value must be 31 August 4500, 11:59)
        /// Note: for single appointments, EndWhole has precedence.
        /// </summary>
        protected DateTime ClipEnd
        {
            get
            {
                return PC.GetDateTimeProperty(PropertyNames.PidLidClipEnd, DateTime.MinValue);
            }
            set
            {
                PC.SetDateTimeProperty(PropertyNames.PidLidClipEnd, value);
            }
        }

        /// <summary>
        /// Start date and time of the first instance (in UTC)
        /// </summary>
        protected DateTime CommonStart
        {
            get
            {
                return PC.GetDateTimeProperty(PropertyNames.PidLidCommonStart, DateTime.MinValue);
            }
            set
            {
                PC.SetDateTimeProperty(PropertyNames.PidLidCommonStart, value);
            }
        }

        /// <summary>
        /// End date and time of the first instance (in UTC)
        /// </summary>
        protected DateTime CommonEnd
        {
            get
            {
                return PC.GetDateTimeProperty(PropertyNames.PidLidCommonEnd, DateTime.MinValue);
            }
            set
            {
                PC.SetDateTimeProperty(PropertyNames.PidLidCommonEnd, value);
            }
        }

        /// <summary>
        /// Start date and time of the first instance (in UTC)
        /// </summary>
        protected DateTime StartWhole
        {
            get
            {
                return PC.GetDateTimeProperty(PropertyNames.PidLidAppointmentStartWhole, DateTime.MinValue);
            }
            set
            {
                PC.SetDateTimeProperty(PropertyNames.PidLidAppointmentStartWhole, value);
            }
        }

        /// <summary>
        /// End date and time of the first instance (in UTC)
        /// </summary>
        protected DateTime EndWhole
        {
            get
            {
                return PC.GetDateTimeProperty(PropertyNames.PidLidAppointmentEndWhole, DateTime.MinValue);
            }
            set
            {
                PC.SetDateTimeProperty(PropertyNames.PidLidAppointmentEndWhole, value);
            }
        }

        /// <summary>
        /// Maximum length: 255 characters
        /// </summary>
        public string Location
        {
            get
            {
                return PC.GetStringProperty(PropertyNames.PidLidLocation);
            }
            set
            {
                PC.SetStringProperty(PropertyNames.PidLidLocation, value);
            }
        }

        public int Color
        {
            get
            {
                return PC.GetInt32Property(PropertyNames.PidLidAppointmentColor, 0);
            }
            set
            {
                PC.SetInt32Property(PropertyNames.PidLidAppointmentColor, value);
            }
        }

        public MeetingType ConferencingType
        {
            set
            {
                PC.SetInt32Property(PropertyNames.PidLidConferencingType, (int)value);
            }
        }

        public bool IsPrivate
        {
            get
            {
                return PC.GetBooleanProperty(PropertyNames.PidLidPrivate, false);
            }
            set
            {
                PC.SetBooleanProperty(PropertyNames.PidLidPrivate, value);
            }
        }

        /// <summary>
        /// true if represents a recurring series.
        /// </summary>
        protected bool Recurring
        {
            get
            {
                return PC.GetBooleanProperty(PropertyNames.PidLidRecurring, false);
            }
            set
            {
                PC.SetBooleanProperty(PropertyNames.PidLidRecurring, value);
            }
        }

        /// <summary>
        /// true if associated with a recurring series
        /// </summary>
        protected bool IsRecurring
        {
            get
            {
                return PC.GetBooleanProperty(PropertyNames.PidLidIsRecurring, false);
            }
            set
            {
                PC.SetBooleanProperty(PropertyNames.PidLidIsRecurring, value);
            }
        }

        /// <summary>
        /// Specify whether this is an all day event
        /// </summary>
        public bool AppointmentSubType
        {
            get
            {
                return PC.GetBooleanProperty(PropertyNames.PidLidAppointmentSubType, false);
            }
            set
            {
                PC.SetBooleanProperty(PropertyNames.PidLidAppointmentSubType, value);
            }
        }

        public bool IsReminderSet
        {
            set
            {
                PC.SetBooleanProperty(PropertyNames.PidLidReminderSet, value);
            }
        }

        public BusyStatus BusyStatus
        {
            get
            {
                Nullable<int> result = PC.GetInt32Property(PropertyNames.PidLidBusyStatus);
                if (result.HasValue)
                {
                    return (BusyStatus)result.Value;
                }
                return BusyStatus.Avaiable;
            }
            set
            {
                PC.SetInt32Property(PropertyNames.PidLidBusyStatus, (int)value);
            }
        }

        public bool InvitationsHaveBeenSent
        {
            set
            {
                PC.SetBooleanProperty(PropertyNames.PidLidFInvited, value);
            }
        }

        public int StateFlags
        {
            set
            {
                PC.SetInt32Property(PropertyNames.PidLidAppointmentStateFlags, value);
            }
        }

        public bool SmartNoAttach
        {
            set
            {
                PC.SetBooleanProperty(PropertyNames.PidLidSmartNoAttach, value);
            }
        }

        /// <summary>
        /// Outlook 2010 will use PidLidTimeZoneDescription in the following case:
        /// In order for Outlook 2010 to recalculate the UTC time of an appointment, it needs to know the current timezone of the appointment as well as
        /// the historic one (PidLidTimeZoneStruct), so if PidLidAppointmentTimeZoneDefinitionStartDisplay is not present, this propery will be used.
        /// </summary>
        public string TimeZoneDescription
        {
            get
            {
                return PC.GetStringProperty(PropertyNames.PidLidTimeZoneDescription);
            }
            set
            {
                PC.SetStringProperty(PropertyNames.PidLidTimeZoneDescription, value);
            }
        }

        public abstract TimeZoneInfo OriginalTimeZone
        {
            get;
        }

        public abstract bool HasTimeZoneDefinition
        {
            get;
        }

        /// <summary>
        /// Created by Outlook 2003 SP3 and above.
        /// Used by Outlook 2007 to display single appointments.
        /// Outlook 2010 will use this to display recurring appointments.
        /// If this property is missing, the current local time zone is assumed.
        /// http://msdn.microsoft.com/en-us/library/cc765598.aspx
        /// 
        /// the value of the PidLidAppointmentTimeZoneDefinitionStartDisplay property is to be used for all time properties,
        /// including the PidLidAppointmentEndWhole property.
        /// http://msdn.microsoft.com/en-us/library/ee219440%28v=exchg.80%29
        /// </summary>
        public TimeZoneDefinitionStructure TimeZoneDefinitionStartDisplay
        {
            get
            {
                byte[] bytes = PC.GetBytesProperty(PropertyNames.PidLidAppointmentTimeZoneDefinitionStartDisplay);
                if (bytes != null)
                {
                    TimeZoneDefinitionStructure structure = new TimeZoneDefinitionStructure(bytes);
                    return structure;
                }
                return null;
            }
            set
            {
                PC.SetBytesProperty(PropertyNames.PidLidAppointmentTimeZoneDefinitionStartDisplay, value.GetBytes());
            }
        }

        /// <summary>
        /// Created by Outlook 2003 SP3 and above.
        /// If this property is missing, the time zone specified by the PidLidAppointmentTimeZoneDefinitionStartDisplay property is used,
        /// If the latter is missing or invalid, the current local time zone is assumed.
        /// http://msdn.microsoft.com/en-us/library/cc842082%28v=office.12%29.aspx
        /// </summary>
        public TimeZoneDefinitionStructure TimeZoneDefinitionEndDisplay
        {
            get
            {
                byte[] bytes = PC.GetBytesProperty(PropertyNames.PidLidAppointmentTimeZoneDefinitionEndDisplay);
                if (bytes != null)
                {
                    TimeZoneDefinitionStructure structure = new TimeZoneDefinitionStructure(bytes);
                    return structure;
                }
                return null;
            }
            set
            {
                PC.SetBytesProperty(PropertyNames.PidLidAppointmentTimeZoneDefinitionEndDisplay, value.GetBytes());
            }
        }

        public bool IsAllDayEvent
        {
            get
            {
                return AppointmentSubType;
            }
            set
            {
                AppointmentSubType = value;
            }
        }

        public static Appointment GetAppointment(PSTFile file, NodeID nodeID)
        {
            PSTNode node = file.GetNode(nodeID);
            NamedPropertyContext pc = node.PC;
            if (pc != null)
            {
                bool recurring = pc.GetBooleanProperty(PropertyNames.PidLidRecurring, false);
                if (recurring)
                {
                    return new RecurringAppointment(node);
                }
                else
                {
                    return new SingleAppointment(node);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
