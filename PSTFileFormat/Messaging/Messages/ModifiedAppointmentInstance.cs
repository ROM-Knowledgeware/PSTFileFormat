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
    public partial class ModifiedAppointmentInstance : Subnode
    {
        public const string RecurringAppointmentExceptionMessageClass = "IPM.OLE.CLASS.{00061055-0000-0000-C000-000000000046}";
        
        public ModifiedAppointmentInstance(Subnode subnode)
            : base(subnode.File, subnode.SubnodeID, subnode.DataTree, subnode.SubnodeBTree)
        {

        }

        public override void SaveChanges()
        {
            PC.SetDateTimeProperty(PropertyID.PidTagLastModificationTime, DateTime.UtcNow);
            // The length must include the message size as well, so we add it as placeholder
            PC.SetInt32Property(PropertyID.PidTagMessageSize, 0);
            int messageSize;
            if (this.File.WriterCompatibilityMode < WriterCompatibilityMode.Outlook2007SP2)
            {
                messageSize = PC.GetTotalLengthOfAllProperties();
            }
            else
            {
                PC.FlushToDataTree();
                messageSize = this.DataTree.TotalDataLength;
            }
            PC.SetInt32Property(PropertyID.PidTagMessageSize, messageSize);

            base.SaveChanges();
        }
        
        public void SetStartAndDuration(DateTime startDTUtc, int duration)
        {
            this.StartDTUtc = startDTUtc;
            this.AppointmentDuration = duration;
            this.EndDTUtc = startDTUtc.AddMinutes(duration);
        }
        
        public DateTime StartDTUtc
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

        /// <summary>
        /// The timespan in minutes from the start time in UTC to end time in UTC.
        /// </summary>
        public int Duration
        {
            get
            {
                TimeSpan ts = EndDTUtc - StartDTUtc;
                return (int)ts.TotalMinutes;
            }
        }

        public DateTime GetStartDTZone(TimeZoneInfo timezone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(StartDTUtc, timezone);
        }

        public DateTime GetEndDTZone(TimeZoneInfo timezone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(EndDTUtc, timezone);
        }

        public static ModifiedAppointmentInstance CreateNewModifiedInstance(PSTFile file, SubnodeBTree attachmentSubnodeBTree)
        {
            PropertyContext pc = PropertyContext.CreateNewPropertyContext(file);
            pc.SetStringProperty(PropertyID.PidTagMessageClass, RecurringAppointmentExceptionMessageClass);
            pc.SetDateTimeProperty(PropertyID.PidTagCreationTime, DateTime.UtcNow);
            pc.SetDateTimeProperty(PropertyID.PidTagLastModificationTime, DateTime.UtcNow);
            
            // PidTagSearchKey is apparently a GUID
            pc.SetBytesProperty(PropertyID.PidTagSearchKey, LittleEndianConverter.GetBytes(Guid.NewGuid()));

            pc.SaveChanges();

            NodeID subnodeID = file.Header.AllocateNextNodeID(NodeTypeName.NID_TYPE_NORMAL_MESSAGE);
            attachmentSubnodeBTree.InsertSubnodeEntry(subnodeID, pc.DataTree, pc.SubnodeBTree);

            Subnode subnode = new Subnode(file, subnodeID, pc.DataTree, pc.SubnodeBTree);
            return new ModifiedAppointmentInstance(subnode);
        }
    }
}
