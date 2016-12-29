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
    public class RecurringAppointment : Appointment
    {
        private DateTime m_startDTUtc; // In UTC
        private int m_duration; // in minutes (can be more than 24 hours)
        public RecurrenceType RecurrenceType;
        private DateTime m_lastInstanceStartDate;
        public bool EndAfterNumberOfOccurences;
        public int Period;
        public int Day; // Day of Week / Day Of Month
        public DayOccurenceNumber DayOccurenceNumber;
        public List<DateTime> DeletedInstanceDates = new List<DateTime>(); // In timezone time
        public List<ExceptionInfoStructure> ExceptionList = new List<ExceptionInfoStructure>();

        public RecurringAppointment(PSTNode node) : base(node)
        {
            this.Recurring = true;
            byte[] bytes = PC.GetBytesProperty(PropertyNames.PidLidAppointmentRecur);
            if (bytes != null)
            {
                ReadRecurrencePattern(bytes);
            }
        }

        private void ReadRecurrencePattern(byte[] bytes)
        {
            AppointmentRecurrencePatternStructure structure = AppointmentRecurrencePatternStructure.GetRecurrencePatternStructure(bytes);
            m_startDTUtc = structure.GetStartDTUtc(this.OriginalTimeZone);
            m_duration = structure.Duration;
            RecurrenceType = structure.RecurrenceType;
            m_lastInstanceStartDate = structure.LastInstanceStartDate;
            Period = structure.PeriodInRecurrenceTypeUnits;

            if (structure.EndType == RecurrenceEndType.EndAfterNOccurrences)
            {
                EndAfterNumberOfOccurences = true;
            }

            switch (structure.RecurFrequency)
            {
                case RecurrenceFrequency.Daily:
                    {
                        break;
                    }
                case RecurrenceFrequency.Weekly:
                    {
                        Day = (int)((WeeklyRecurrencePatternStructure)structure).DaysOfWeek;
                        break;
                    }
                case RecurrenceFrequency.Monthly:
                    {
                        if (structure.PatternType == PatternType.Month)
                        {
                            Day = (int)((MonthlyRecurrencePatternStructure)structure).DayOfMonth;
                        }
                        else // MonthNth
                        {
                            Day = (int)((MonthlyRecurrencePatternStructure)structure).DayOfWeek;
                            DayOccurenceNumber = ((MonthlyRecurrencePatternStructure)structure).DayOccurenceNumber;
                        }
                        break;
                    }
                case RecurrenceFrequency.Yearly:
                    {
                        if (structure.PatternType == PatternType.Month)
                        {
                            Day = (int)((YearlyRecurrencePatternStructure)structure).DayOfMonth;
                        }
                        else // MonthNth
                        {
                            Day = (int)((YearlyRecurrencePatternStructure)structure).DayOfWeek;
                            DayOccurenceNumber = ((YearlyRecurrencePatternStructure)structure).DayOccurenceNumber;
                        }
                    }
                    break;
            }

            DeletedInstanceDates = structure.DeletedInstanceDates;
            ExceptionList = structure.ExceptionList;
        }

        private AppointmentRecurrencePatternStructure GetRecurrencePattern(TimeZoneInfo timezone)
        {
            AppointmentRecurrencePatternStructure structure;
            RecurrenceFrequency recurrenceFrequency = RecurrenceTypeHelper.GetRecurrenceFrequency(RecurrenceType);
            PatternType patternType = RecurrenceTypeHelper.GetPatternType(RecurrenceType);
            
            switch (recurrenceFrequency)
            {
                case RecurrenceFrequency.Daily:
                    structure = new DailyRecurrencePatternStructure();
                    break;
                case RecurrenceFrequency.Weekly:
                    structure = new WeeklyRecurrencePatternStructure();
                    ((WeeklyRecurrencePatternStructure)structure).DaysOfWeek = (DaysOfWeekFlags)Day;
                    break;
                case RecurrenceFrequency.Monthly:
                    structure = new MonthlyRecurrencePatternStructure();
                    if (patternType == PatternType.Month)
                    {
                        ((MonthlyRecurrencePatternStructure)structure).DayOfMonth = (uint)Day;
                    }
                    else
                    {
                        ((MonthlyRecurrencePatternStructure)structure).DayOfWeek = (OutlookDayOfWeek)Day;
                        ((MonthlyRecurrencePatternStructure)structure).DayOccurenceNumber = DayOccurenceNumber;
                    }
                    break;
                case RecurrenceFrequency.Yearly:
                    structure = new YearlyRecurrencePatternStructure();
                    if (patternType == PatternType.Month)
                    {
                        ((YearlyRecurrencePatternStructure)structure).DayOfMonth = (uint)Day;
                    }
                    else
                    {
                        ((YearlyRecurrencePatternStructure)structure).DayOfWeek = (OutlookDayOfWeek)Day;
                        ((YearlyRecurrencePatternStructure)structure).DayOccurenceNumber = DayOccurenceNumber;
                    }
                    break;
                default:
                    throw new InvalidRecurrencePatternException("Recurrence frequency is invalid");
            }
            structure.PatternType = patternType;
            structure.PeriodInRecurrenceTypeUnits = Period;
            structure.SetStartAndDuration(m_startDTUtc, m_duration, timezone);
            DateTime startDTZone = TimeZoneInfo.ConvertTimeFromUtc(m_startDTUtc, timezone);
            structure.FirstDateTimeInDays = AppointmentRecurrencePatternStructure.CalculateFirstDateTimeInDays(recurrenceFrequency, patternType, Period, startDTZone);
            structure.LastInstanceStartDate = m_lastInstanceStartDate;
            if (EndAfterNumberOfOccurences)
            {
                structure.OccurrenceCount = (uint)CalendarHelper.CalculateNumberOfOccurences(StartDTUtc, m_lastInstanceStartDate, this.RecurrenceType, Period, Day);
                structure.EndType = RecurrenceEndType.EndAfterNOccurrences;
            }
            else if (m_lastInstanceStartDate.Year >= 4500)
            {
                structure.EndType = RecurrenceEndType.NeverEnd;
                structure.OccurrenceCount = 10;
            }
            else
            {
                structure.EndType = RecurrenceEndType.EndAfterDate;
                // OccurrenceCount should not be 0, or otherwise Outlook 2003's Appointment recurrence window is not loaded properly
                structure.OccurrenceCount = 10;
            }
            
            structure.DeletedInstanceDates = DeletedInstanceDates;
            foreach (ExceptionInfoStructure exception in ExceptionList)
            {
                structure.ModifiedInstanceDates.Add(DateTimeUtils.GetDayStart(exception.NewStartDT));
            }
            structure.ExceptionList = ExceptionList;
            return structure;
        }

        public ModifiedAppointmentInstance GetModifiedInstance(int attachmentIndex)
        {
            AttachmentObject attachmentObject = this.GetAttachmentObject(attachmentIndex);
            Subnode subnode = attachmentObject.AttachedNode;
            if (subnode != null)
            {
                ModifiedAppointmentInstance modifiedInstance = new ModifiedAppointmentInstance(subnode);
                return modifiedInstance;
            }
            return null;
        }

        public override void SaveChanges()
        {
            TimeZoneInfo timezone = this.OriginalTimeZone;
            if (timezone == null)
            {
                // if time zone was not provided, store current system time zone definition
                timezone = TimeZoneInfoUtils.GetSystemStaticTimeZone(TimeZoneInfo.Local.Id);
            }
            AppointmentRecurrencePatternStructure structure = GetRecurrencePattern(timezone);

            PC.SetBytesProperty(PropertyNames.PidLidAppointmentRecur, structure.GetBytes(this.File.WriterCompatibilityMode));

            base.SaveChanges();
        }

        public override void SetStartAndDuration(DateTime startDTUtc, int duration)
        {
            this.StartDTUtc = startDTUtc;
            m_duration = duration;
            this.AppointmentDuration = duration;

            DateTime endDT = startDTUtc.AddMinutes(duration);
            this.EndWhole = endDT;
            this.CommonEnd = endDT;
        }

        /// <param name="dynamicTimeZone">can be set to null if not available</param>
        public override void SetOriginalTimeZone(TimeZoneInfo staticTimeZone, TimeZoneInfo dynamicTimeZone, int effectiveYear)
        {
            this.TimeZoneStructure = TimeZoneStructure.FromTimeZoneInfo(staticTimeZone);
            this.TimeZoneDescription = staticTimeZone.DisplayName;
            if (this.File.WriterCompatibilityMode >= WriterCompatibilityMode.Outlook2003SP3)
            {
                TimeZoneInfo.AdjustmentRule effectiveRule = AdjustmentRuleUtils.GetFirstRule(staticTimeZone);
                TimeZoneInfo definitionTimezone = (dynamicTimeZone == null) ? staticTimeZone : dynamicTimeZone;
                TimeZoneDefinitionStructure timeZoneDefinitionStartDisplay = TimeZoneDefinitionStructure.FromTimeZoneInfo(definitionTimezone, effectiveRule, effectiveYear, false);
                this.TimeZoneDefinitionStartDisplay = timeZoneDefinitionStartDisplay;
                // We do not set TimeZoneDefinitionEndDisplay, so Outlook will use TimeZoneDefinitionStartDisplay.

                // Only Outlook 2007 uses PidLidAppointmentTimeZoneDefinitionRecur
                if (this.File.WriterCompatibilityMode >= WriterCompatibilityMode.Outlook2007RTM &&
                    this.File.WriterCompatibilityMode < WriterCompatibilityMode.Outlook2010RTM)
                {
                    TimeZoneDefinitionStructure timeZoneDefinitionRecurStructure = TimeZoneDefinitionStructure.FromTimeZoneInfo(definitionTimezone, effectiveRule, effectiveYear, true);
                    this.TimeZoneDefinitionRecurStructure = timeZoneDefinitionRecurStructure;
                }
            }
        }

        public override DateTime StartDTUtc
        {
            get
            {
                return m_startDTUtc;
            }
            set
            {
                m_startDTUtc = value;
                this.CommonStart = value;
                this.StartWhole = value;

                DateTime startDTZone = TimeZoneInfo.ConvertTimeFromUtc(value, this.OriginalTimeZone);
                DateTime midnightDTZone = DateTimeUtils.GetDayStart(startDTZone);
                this.ClipStart = TimeZoneInfo.ConvertTimeToUtc(midnightDTZone, this.OriginalTimeZone);
            }
        }


        /// <summary>
        /// Note:
        /// 1. For the purpose of calculating the start time of recurring appointments
        ///    Outlook 2003 uses PidLidTimeZoneStruct, however, this is a simple structure that can hold
        ///    a single DST (daylight savings time) rule, which does not reflect the fact that over time,
        ///    time zone rules can change.
        /// 2. Outlook 2007 uses PidLidAppointmentTimeZoneDefinitionRecur, which contains multiple DST rules,
        ///    However, it only uses a single rule (the effective rule) for all calculations.
        /// 3. If DST is enabled but the appointment does not contain a rule,
        ///     Outlook (both 2003 and 2007) will use the OS static timezone rule.
        /// </summary>
        public override TimeZoneInfo OriginalTimeZone
        {
            get
            {
                TimeZoneStructure timezoneStructure = this.TimeZoneStructure;
                TimeZoneDefinitionStructure timezoneDefinitionStructure = this.TimeZoneDefinitionRecurStructure;

                if (timezoneStructure == null) // no legacy structure, even if there is Outlook 2007+ structure, it's not consistent
                {
                    // assume current system time zone definition
                    return TimeZoneInfoUtils.GetSystemStaticTimeZone(TimeZoneInfo.Local.Id);
                }
                else // legacy structure present
                {
                    if (timezoneDefinitionStructure != null)
                    {
                        // If the PidLidAppointmentTimeZoneDefinitionRecur property is not set or is
                        // inconsistent with the associated PidLidTimeZoneStruct structure, the values in the
                        // PidLidTimeZoneStruct property are used to determine the effective time zone rule.
                        if (timezoneDefinitionStructure.IsConsistent(timezoneStructure))
                        {
                            return timezoneDefinitionStructure.ToTimeZoneInfo();
                        }
                    }
                    
                    string timezoneDisplayName = this.TimeZoneDescription;
                    // I've encountered cases where instead of '(GMT+2:00) Jerusalem', the stored value was
                    // '(GMT+02:00) Israel Standard Time' or
                    // '(GMT+02:00) (GMT+02:00) Israel Standard Time'

                    // Should we try to get the timezone key name from TimeZoneDefinitionStartDisplay?
                    string timezoneKeyName = TimeZoneInfo.Local.Id;
                    if (!String.IsNullOrEmpty(timezoneDisplayName))
                    {
                        TimeZoneInfo temp = TimeZoneInfoUtils.FindSystemTimeZoneByDisplayName(timezoneDisplayName);
                        if (temp != null)
                        {
                            timezoneKeyName = temp.Id;
                        }
                    }

                    return timezoneStructure.ToTimeZoneInfo(timezoneKeyName);
                }
            }
        }

        public override bool HasTimeZoneDefinition
        {
           get 
           {
               return (this.TimeZoneStructure != null || this.TimeZoneDefinitionRecurStructure != null);
           }
        }

        /// <summary>
        /// Legacy - Outlook 2003
        /// </summary>
        public TimeZoneStructure TimeZoneStructure
        {
            get
            {
                byte[] bytes = PC.GetBytesProperty(PropertyNames.PidLidTimeZoneStruct);
                if (bytes != null)
                {
                    TimeZoneStructure structure = new TimeZoneStructure(bytes);
                    return structure;
                }
                return null;
            }
            set
            {
                PC.SetBytesProperty(PropertyNames.PidLidTimeZoneStruct, value.GetBytes());
            }
        }

        /// <summary>
        /// Written by Outlook 2007 and above,
        /// This should be equivalent to TimeZoneDefinitionStartDisplay
        /// (except the TZRULE_FLAG_RECUR_CURRENT_TZREG flag).
        /// </summary>
        public TimeZoneDefinitionStructure TimeZoneDefinitionRecurStructure
        {
            get
            {
                byte[] bytes = PC.GetBytesProperty(PropertyNames.PidLidAppointmentTimeZoneDefinitionRecur);
                if (bytes != null)
                {
                    TimeZoneDefinitionStructure structure = new TimeZoneDefinitionStructure(bytes);
                    return structure;
                }
                return null;
            }
            set
            {
                PC.SetBytesProperty(PropertyNames.PidLidAppointmentTimeZoneDefinitionRecur, value.GetBytes());
            }
        }

        public void AddModifiedInstanceAttachment(DateTime originalStartDTUtc, int originalDuration, DateTime newStartDTUtc, int duration, string subject, string location, BusyStatus busyStatus, int color, MessagePriority priority, TimeZoneInfo timezone)
        {
            AttachmentObject attachment = AttachmentObject.CreateNewExceptionAttachmentObject(this.File, this.SubnodeBTree);
            ModifiedAppointmentInstance modifiedInstance = ModifiedAppointmentInstance.CreateNewModifiedInstance(attachment.File, attachment.SubnodeBTree);
            modifiedInstance.SetStartAndDuration(newStartDTUtc, duration);
            modifiedInstance.AlternateRecipientAllowed = true;
            modifiedInstance.Subject = subject;
            modifiedInstance.Location = location;
            //modifiedInstance.Sensitivity = 0;
            modifiedInstance.Priority = priority;
            modifiedInstance.BusyStatus = busyStatus;
            modifiedInstance.Color = color;
            modifiedInstance.MessageFlags = MessageFlags.MSGFLAG_READ;
            modifiedInstance.ExceptionReplaceTime = originalStartDTUtc;
            modifiedInstance.StartDate = originalStartDTUtc;
            modifiedInstance.EndDate = originalStartDTUtc.AddMinutes(originalDuration);

            modifiedInstance.SaveChanges(attachment.SubnodeBTree);
            attachment.StoreModifiedInstance(modifiedInstance, timezone);
            attachment.SaveChanges(this.SubnodeBTree);
            this.AddAttachment(attachment);
        }

        public DateTime LastInstanceStartDate
        {
            get
            {
                return m_lastInstanceStartDate;
            }
            set
            {
                m_lastInstanceStartDate = DateTimeUtils.GetDayStart(value);
                // DateTimeKind.Local means the current timezone on the client computer, while we use the timezone specified by the appointment
                DateTime midnightDTZone = DateTime.SpecifyKind(m_lastInstanceStartDate, DateTimeKind.Unspecified);
                this.ClipEnd = TimeZoneInfo.ConvertTimeToUtc(midnightDTZone, this.OriginalTimeZone);
            }
        }

        [Obsolete]
        public DateTime LastInstanceStartDTUtc
        {
            get
            {
                // We take daylight saving into account:
                TimeSpan zoneTimeOfDay = TimeZoneInfo.ConvertTimeFromUtc(StartDTUtc, this.OriginalTimeZone).TimeOfDay;
                DateTime lastInstanceStartDTZone = TimeZoneInfoUtils.SetValidTimeOfDay(m_lastInstanceStartDate, zoneTimeOfDay, this.OriginalTimeZone);
                return TimeZoneInfoUtils.AdvancedConvertToUtc(lastInstanceStartDTZone, this.OriginalTimeZone, true);
            }
        }

        [Obsolete]
        public DateTime LastInstanceEndDTUtc
        {
            get
            {
                return LastInstanceStartDTUtc.AddMinutes(this.Duration);
            }
        }

        /// <summary>
        /// The timespan in minutes from the start time to end time in timezone time.
        /// It will differ from the actual duration (UTC) if an appointment instance is spanning both standard time and daylight time.
        /// </summary>
        public override int Duration
        {
            get 
            {
                return m_duration;
            }
        }

        public static RecurringAppointment CreateNewRecurringAppointment(PSTFile file, NodeID parentNodeID)
        {
            return CreateNewRecurringAppointment(file, parentNodeID, Guid.NewGuid());
        }

        public static RecurringAppointment CreateNewRecurringAppointment(PSTFile file, NodeID parentNodeID, Guid searchKey)
        {
            MessageObject message = CreateNewMessage(file, FolderItemTypeName.Appointment, parentNodeID, searchKey);
            RecurringAppointment appointment = new RecurringAppointment(message);
            appointment.MessageFlags = MessageFlags.MSGFLAG_READ;
            appointment.InternetCodepage = 1255;
            appointment.MessageDeliveryTime = DateTime.UtcNow;
            appointment.ClientSubmitTime = DateTime.UtcNow;
            appointment.SideEffects = SideEffectsFlags.seOpenForCtxMenu | SideEffectsFlags.seOpenToMove | SideEffectsFlags.seOpenToCopy | SideEffectsFlags.seCoerceToInbox | SideEffectsFlags.seOpenToDelete;
            appointment.Importance = MessageImportance.Normal;
            appointment.Priority = MessagePriority.Normal;
            appointment.ConferencingType = MeetingType.WindowsNetmeeting;

            appointment.IconIndex = IconIndex.RecurringAppointment;
            return appointment;
        }
    }
}
