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
    public class AttachmentObject : Subnode
    {
        public AttachmentObject(Subnode subnode)
            : base(subnode.File, subnode.SubnodeID, subnode.DataTree, subnode.SubnodeBTree)
        {
        }

        public Subnode AttachedNode
        {
            get
            {
                PropertyContext pc = this.PC;
                if (pc != null)
                {
                    Subnode subnode = pc.GetObjectProperty(PropertyID.PidTagAttachData);
                    return subnode;
                }
                return null;
            }
        }

        public void StoreModifiedInstance(ModifiedAppointmentInstance modifiedInstance, TimeZoneInfo timezone)
        {
            PC.SetDateTimeProperty(PropertyID.PidTagExceptionReplaceTime, modifiedInstance.StartDTUtc);
            PC.SetDateTimeProperty(PropertyID.PidTagExceptionStartTime, modifiedInstance.GetStartDTZone(timezone));
            PC.SetDateTimeProperty(PropertyID.PidTagExceptionEndTime, modifiedInstance.GetEndDTZone(timezone));
            int dataSize;
            if (this.File.WriterCompatibilityMode < WriterCompatibilityMode.Outlook2007SP2)
            {
                dataSize = modifiedInstance.PC.GetTotalLengthOfAllProperties();
            }
            else
            {
                dataSize = modifiedInstance.DataTree.TotalDataLength;
            }
            // The length must include the message size as well, so we add it as placeholder
            PC.SetInt32Property(PropertyID.PidTagAttachSize, 0);
            int attachmentSize = this.PC.GetTotalLengthOfAllProperties() + dataSize;

            PC.SetObjectProperty(PropertyID.PidTagAttachData, modifiedInstance.SubnodeID, dataSize);
            PC.SetInt32Property(PropertyID.PidTagAttachSize, attachmentSize);
        }

        public static AttachmentObject CreateNewAttachmentObject(PSTFile file, SubnodeBTree subnodeBTree)
        {
            PropertyContext pc = PropertyContext.CreateNewPropertyContext(file);
            pc.SaveChanges();

            NodeID pcNodeID = file.Header.AllocateNextNodeID(NodeTypeName.NID_TYPE_ATTACHMENT);
            subnodeBTree.InsertSubnodeEntry(pcNodeID, pc.DataTree, pc.SubnodeBTree);

            Subnode subnode = new Subnode(file, pcNodeID, pc.DataTree, pc.SubnodeBTree);
            return new AttachmentObject(subnode);
        }

        public static AttachmentObject CreateNewExceptionAttachmentObject(PSTFile file, SubnodeBTree subnodeBTree)
        {
            AttachmentObject attachment = CreateNewAttachmentObject(file, subnodeBTree);
            attachment.PC.SetStringProperty(PropertyID.PidTagDisplayName, "Untitled");
            attachment.PC.SetStringProperty(PropertyID.PidTagAttachEncoding, String.Empty);
            //attachment.PC.SetBytesProperty(PropertyID.PidTagAttachRendering, AppointmentAttachRendering);

            attachment.PC.SetInt32Property(PropertyID.PidTagAttachMethod, (int)AttachMethod.EmbeddedMessage);
            attachment.PC.SetInt32Property(PropertyID.PidTagAttachFlags, 0);
            attachment.PC.SetInt32Property(PropertyID.PidTagAttachmentFlags, (int)AttachmentFlags.afException);
            attachment.PC.SetInt32Property(PropertyID.PidTagAttachmentLinkId, 0);
            attachment.PC.SetInt32Property(PropertyID.PidTagRenderingPosition, -1);
            
            attachment.PC.SetBooleanProperty(PropertyID.PidTagAttachmentHidden, true);
            attachment.PC.SetBooleanProperty(PropertyID.PidTagAttachmentContactPhoto, false);

            attachment.CreateSubnodeBTreeIfNotExist();

            return attachment;
        }
    }
}
