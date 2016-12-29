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
    public class RecipientsTable : TableContext
    {
        public RecipientsTable(HeapOnNode heap, SubnodeBTree subnodeBTree) : base(heap, subnodeBTree)
        {

        }

        public MessageRecipient GetRecipient(int rowIndex)
        {
            MessageRecipient result = new MessageRecipient();
            result.DisplayName = GetStringProperty(rowIndex, PropertyID.PidTagDisplayName);
            result.EmailAddress = GetStringProperty(rowIndex, PropertyID.PidTagEmailAddress);
            RecipientFlags recipientFlags = (RecipientFlags)GetInt32Property(rowIndex, PropertyID.PidTagRecipientFlags);
            result.IsOrganizer = ((recipientFlags & RecipientFlags.MeetingOrganizer) > 0);
            return result;
        }

        public void AddRecipient(PSTFile file, MessageRecipient recipient)
        {
            AddRecipient(file, recipient.DisplayName, recipient.EmailAddress, recipient.IsOrganizer);
        }

        public void AddRecipient(PSTFile file, string displayName, string emailAddress, bool isOrganizer)
        {
            // http://social.msdn.microsoft.com/Forums/en-US/os_binaryfile/thread/a5f9c653-40f5-4638-85d3-00c54607d984/
            // dwRowID must be > 0:
            uint rowID = (uint)RowCount + 1; // good enough for now
            int rowIndex = AddRow(rowID);
            SetStringProperty(rowIndex, PropertyID.PidTagDisplayName, displayName);
            SetStringProperty(rowIndex, PropertyID.PidTagAddressType, "SMTP");
            SetStringProperty(rowIndex, PropertyID.PidTagEmailAddress, emailAddress);
            SetBytesProperty(rowIndex, PropertyID.PidTagSearchKey, LittleEndianConverter.GetBytes(Guid.NewGuid()));
            SetInt32Property(rowIndex, PropertyID.PidTagRecipientType, (int)RecipientType.To);
            SetInt32Property(rowIndex, PropertyID.PidTagObjectType, (int)ObjectType.MailUser);
            SetInt32Property(rowIndex, PropertyID.PidTagDisplayType, 0);

            SetStringProperty(rowIndex, PropertyID.PidTagRecipientDisplayName, displayName);
            
            int recipientFlags = (int)RecipientFlags.SendableAttendee;
            if (isOrganizer)
            {
                recipientFlags |= (int)RecipientFlags.MeetingOrganizer;
            }
            SetInt32Property(rowIndex, PropertyID.PidTagRecipientFlags, recipientFlags);
            SetInt32Property(rowIndex, PropertyID.PidTagRecipientTrackStatus, 0);

            byte[] recipientEntryID = RecipientEntryID.GetEntryID(displayName, emailAddress).GetBytes();
            SetBytesProperty(rowIndex, PropertyID.PidTagEntryId, recipientEntryID);
            SetBytesProperty(rowIndex, PropertyID.PidTagRecipientEntryId, recipientEntryID);

            SetInt32Property(rowIndex, PropertyID.PidTagLtpRowId, (int)rowID);
            SetInt32Property(rowIndex, PropertyID.PidTagLtpRowVer, (int)file.Header.AllocateNextUniqueID());
        }
    }
}
