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
    public class AttachmentTable : TableContext
    {
        public AttachmentTable(HeapOnNode heap, SubnodeBTree subnodeBTree) : base(heap, subnodeBTree)
        { 

        }

        public uint GetAttachmentSubnodeID(int attachmentIndex)
        {
            uint subnodeID = GetRowID(attachmentIndex);
            return subnodeID;
        }

        public void AddAttachment(PSTFile file, AttachmentObject attachment)
        {
            AddAttachmentTableColumns(file);
            int rowIndex = AddRow(attachment.SubnodeID.Value);
            UpdateAttachment(file, attachment, rowIndex);
        }

        private void UpdateAttachment(PSTFile file, AttachmentObject attachment)
        {
            int rowIndex = GetRowIndex(attachment.SubnodeID.Value);
            if (rowIndex >= 0)
            {
                UpdateAttachment(file, attachment, rowIndex);
            }
        }

        private void AddAttachmentTableColumns(PSTFile file)
        {
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagDisplayName, PropertyTypeName.PtypString);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachExtension, PropertyTypeName.PtypString);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachLongFilename, PropertyTypeName.PtypString);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachPathname, PropertyTypeName.PtypString);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachTag, PropertyTypeName.PtypBinary);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachLongPathname, PropertyTypeName.PtypString);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachMimeTag, PropertyTypeName.PtypString);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachAdditionalInformation, PropertyTypeName.PtypBinary);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachContentBase, PropertyTypeName.PtypString);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachContentId, PropertyTypeName.PtypString);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachContentLocation, PropertyTypeName.PtypString);
            //this.AddPropertyColumnIfNotExist(PropertyID.Unknown0x6909, PropertyTypeName.PtypInteger32);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachmentLinkId, PropertyTypeName.PtypInteger32);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagExceptionStartTime, PropertyTypeName.PtypTime);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagExceptionEndTime, PropertyTypeName.PtypTime);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachmentFlags, PropertyTypeName.PtypInteger32);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachmentHidden, PropertyTypeName.PtypBoolean);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagExceptionReplaceTime, PropertyTypeName.PtypTime);
            this.AddPropertyColumnIfNotExist(PropertyID.PidTagAttachmentContactPhoto, PropertyTypeName.PtypBoolean);
        }

        private void UpdateAttachment(PSTFile file, AttachmentObject attachment, int rowIndex)
        {
            TableContextHelper.CopyProperties(attachment.PC, this, rowIndex);

            this.SetInt32Property(rowIndex, PropertyID.PidTagLtpRowId, (int)attachment.SubnodeID.Value);
            // UNDOCUMENTED - Outlook uses dwUnique for PidTagLtpRowVer
            this.SetInt32Property(rowIndex, PropertyID.PidTagLtpRowVer, (int)file.Header.AllocateNextUniqueID());
        }
    }
}
