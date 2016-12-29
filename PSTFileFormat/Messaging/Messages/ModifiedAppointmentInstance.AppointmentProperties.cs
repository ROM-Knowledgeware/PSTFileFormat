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

namespace PSTFileFormat
{
    public partial class ModifiedAppointmentInstance
    {
        /// <summary>
        /// - This property is not required.
        /// - The number of minutes from start to end in timezone time. (UNDOCUMENTED)
        /// - Due to daylight savings, there could be a difference between the timezone duration and the UTC duration.
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
        /// Start date and time of the first instance
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
        /// End date and time of the first instance
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
        /// Start date and time of the first instance
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
        /// End date and time of the first instance
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
        /// Specifies the date and time, in UTC, that the exception will replace
        /// </summary>
        public DateTime ExceptionReplaceTime
        {
            get
            {
                return PC.GetDateTimeProperty(PropertyNames.PidLidExceptionReplaceTime, DateTime.MinValue);
            }
            set
            {
                PC.SetDateTimeProperty(PropertyNames.PidLidExceptionReplaceTime, value);
            }
        }

        /// <summary>
        /// Original Start DT (not just date)
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return PC.GetDateTimeProperty(PropertyID.PidTagStartDate, DateTime.MinValue);
            }
            set
            {
                PC.SetDateTimeProperty(PropertyID.PidTagStartDate, value);
            }
        }

        /// <summary>
        /// Original End DT (not just date)
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                return PC.GetDateTimeProperty(PropertyID.PidTagEndDate, DateTime.MinValue);
            }
            set
            {
                PC.SetDateTimeProperty(PropertyID.PidTagEndDate, value);
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
    }
}
