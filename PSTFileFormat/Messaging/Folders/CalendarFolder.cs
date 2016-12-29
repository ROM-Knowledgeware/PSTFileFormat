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
    public class CalendarFolder : PSTFolder
    {
        public CalendarFolder(PSTNode node) : base(node)
        {
        }

        public Appointment GetAppointment(int index)
        {
            TableContext tc = GetContentsTable();
            if (tc != null)
            {
                if (index < tc.RowCount)
                {
                    // dwRowID is the MessageID
                    NodeID nodeID = new NodeID(tc.GetRowID(index));
                    Appointment appointment = Appointment.GetAppointment(this.File, nodeID);
                    return appointment;
                }
            }
            return null;
        }

        public override void AddContentTableColumns(NamedTableContext contentsTable)
        {
            base.AddContentTableColumns(contentsTable);

            contentsTable.AddPropertyColumnIfNotExist(PropertyID.PidTagCreationTime, PropertyTypeName.PtypTime);

            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidRecurring, PropertyTypeName.PtypBoolean);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidAppointmentEndWhole, PropertyTypeName.PtypTime);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidSideEffects, PropertyTypeName.PtypInteger32);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidHeaderItem, PropertyTypeName.PtypInteger32);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidAppointmentStateFlags, PropertyTypeName.PtypInteger32);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidFInvited, PropertyTypeName.PtypBoolean);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidAppointmentColor, PropertyTypeName.PtypInteger32);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidTimeZoneStruct, PropertyTypeName.PtypBinary);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidLocation, PropertyTypeName.PtypString);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidReminderSet, PropertyTypeName.PtypBoolean);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidAppointmentRecur, PropertyTypeName.PtypBinary);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidAppointmentSubType, PropertyTypeName.PtypBoolean);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidBusyStatus, PropertyTypeName.PtypInteger32);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidAppointmentStartWhole, PropertyTypeName.PtypTime);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidMeetingWorkspaceUrl, PropertyTypeName.PtypString);

            if (this.File.WriterCompatibilityMode >= WriterCompatibilityMode.Outlook2010RTM)
            {
                contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidTimeZoneDescription, PropertyTypeName.PtypString);
                contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidAppointmentTimeZoneDefinitionRecur, PropertyTypeName.PtypBinary);
                contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidAppointmentTimeZoneDefinitionStartDisplay, PropertyTypeName.PtypBinary);
                contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidAppointmentTimeZoneDefinitionEndDisplay, PropertyTypeName.PtypBinary);
            }
        }

        public int AppointmentCount
        {
            get
            {
                return this.MessageCount;
            }
        }
    }
}
