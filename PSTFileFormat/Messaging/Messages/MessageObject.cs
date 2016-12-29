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
    public partial class MessageObject : PSTNode
    {
        private AttachmentTable m_attachmentTable; // for buffering purposes
        private RecipientsTable m_recipientsTable; // for buffering purposes
        
        public MessageObject(PSTNode node) : base(node)
        {
        }

        public override void SaveChanges()
        {
            PC.SetDateTimeProperty(PropertyID.PidTagLastModificationTime, DateTime.UtcNow);

            if (File.WriterCompatibilityMode >= WriterCompatibilityMode.Outlook2010RTM)
            {
                string conversationTopic = this.ConversationTopic;
                if (conversationTopic != null)
                {
                    this.ConversationId = CalculateConversationID(conversationTopic);
                }
            }

            if (m_recipientsTable != null)
            {
                m_recipientsTable.SaveChanges(SubnodeBTree, new NodeID((uint)InternalNodeName.NID_RECIPIENT_TABLE));
            }

            if (m_attachmentTable != null)
            {
                m_attachmentTable.SaveChanges(SubnodeBTree, new NodeID((uint)InternalNodeName.NID_ATTACHMENT_TABLE));
            }

            int sizeOfAttachments = GetTotalSizeOfAllAttachments();
            if (sizeOfAttachments > 0)
            {
                PC.SetInt32Property(PropertyID.MessageTotalAttachmentSize, sizeOfAttachments);
            }
            else
            {
                // We have to make sure the property was not already exist
                PC.RemoveProperty(PropertyID.MessageTotalAttachmentSize);
            }

            // The length must include the message size as well, so we add it as placeholder
            PC.SetInt32Property(PropertyID.PidTagMessageSize, 0);
            int messageSize;
            if (this.File.WriterCompatibilityMode < WriterCompatibilityMode.Outlook2007SP2)
            {
                messageSize = PC.GetTotalLengthOfAllProperties() + sizeOfAttachments;
            }
            else
            {
                PC.FlushToDataTree();
                messageSize = this.DataTree.TotalDataLength;
                if (this.SubnodeBTree != null)
                {
                    // We should only call this method after all subnode changes has been saves
                    messageSize += this.SubnodeBTree.GetDataLengthOfAllSubnodes();
                }
            }
            PC.SetInt32Property(PropertyID.PidTagMessageSize, messageSize);
            
            base.SaveChanges();
        }

        public void CreateRecipientsTableIfNotExist()
        {
            CreateSubnodeBTreeIfNotExist();
            Subnode subnode = SubnodeBTree.GetSubnode((uint)InternalNodeName.NID_RECIPIENT_TABLE);
            if (subnode == null)
            { 
                PSTNode template = this.File.GetNode(InternalNodeName.NID_RECIPIENT_TABLE);
                NodeID nodeID = new NodeID((uint)InternalNodeName.NID_RECIPIENT_TABLE);
                SubnodeBTree.InsertSubnodeEntry(nodeID, template.DataTree, null);
                File.BlockBTree.IncrementBlockEntryReferenceCount(template.DataTree.RootBlock.BlockID);

                this.RecipientsTable.AddPropertyColumnIfNotExist(PropertyID.PidTagRecipientDisplayName, PropertyTypeName.PtypString);
                this.RecipientsTable.AddPropertyColumnIfNotExist(PropertyID.PidTagRecipientEntryId, PropertyTypeName.PtypBinary);
                this.RecipientsTable.AddPropertyColumnIfNotExist(PropertyID.PidTagRecipientFlags, PropertyTypeName.PtypInteger32);
                this.RecipientsTable.AddPropertyColumnIfNotExist(PropertyID.PidTagRecipientTrackStatus, PropertyTypeName.PtypInteger32);
            }
        }

        public void CreateAttachmentTableIfNotExist()
        {
            CreateSubnodeBTreeIfNotExist();
            Subnode subnode = SubnodeBTree.GetSubnode((uint)InternalNodeName.NID_ATTACHMENT_TABLE);
            if (subnode == null)
            {
                PSTNode template = this.File.GetNode(InternalNodeName.NID_ATTACHMENT_TABLE);
                NodeID nodeID = new NodeID((uint)InternalNodeName.NID_ATTACHMENT_TABLE);
                SubnodeBTree.InsertSubnodeEntry(nodeID, template.DataTree, null);
                File.BlockBTree.IncrementBlockEntryReferenceCount(template.DataTree.RootBlock.BlockID);
            }
        }

        public void AddRecipient(MessageRecipient recipient)
        { 
            List<MessageRecipient> recipients = new List<MessageRecipient>();
            recipients.Add(recipient);
            AddRecipients(recipients);
        }

        public void AddRecipients(List<MessageRecipient> recipients)
        {
            if (recipients.Count == 0)
            {
                return;
            }

            CreateRecipientsTableIfNotExist();
            foreach (MessageRecipient recipient in recipients)
            {
                RecipientsTable.AddRecipient(this.File, recipient);
            }

            StringBuilder builder = new StringBuilder();
            foreach (MessageRecipient recipient in recipients)
            {
                if (builder.Length != 0)
                {
                    builder.Append("; ");
                }
                builder.Append(recipient.DisplayName);
            }

            if (this.DisplayTo == null)
            {
                this.DisplayTo = builder.ToString();
            }
            else
            {
                this.DisplayTo += "; " + builder.ToString();
            }
        }

        public void AddAttachment(AttachmentObject attachment)
        {
            CreateAttachmentTableIfNotExist();
            MessageFlags flags = this.MessageFlags;
            if ((flags & MessageFlags.MSGFLAG_HASATTACH) == 0)
            {
                flags |= MessageFlags.MSGFLAG_HASATTACH;
                this.MessageFlags = flags;
            }

            AttachmentTable.AddAttachment(this.File, attachment);
        }

        public AttachmentTable AttachmentTable
        {
            get
            {
                if (m_attachmentTable == null)
                {
                    // attachment table is not always present (new appointments don't always have it)
                    if (this.SubnodeBTree != null)
                    {
                        Subnode subnode = this.SubnodeBTree.GetSubnode((uint)InternalNodeName.NID_ATTACHMENT_TABLE);
                        if (subnode != null)
                        {
                            DataTree dataTree = subnode.DataTree;
                            if (dataTree != null)
                            {
                                SubnodeBTree subnodeBTree = subnode.SubnodeBTree;
                                HeapOnNode heap = new HeapOnNode(dataTree);
                                m_attachmentTable = new AttachmentTable(heap, subnodeBTree);
                            }
                        }
                    }
                }
                return m_attachmentTable;
            }
        }

        public RecipientsTable RecipientsTable
        {
            get
            {
                if (m_recipientsTable == null)
                {
                    // Recipients table context is not always present (new appointments don't always have it)
                    if (this.SubnodeBTree != null)
                    {
                        Subnode subnode = this.SubnodeBTree.GetSubnode((uint)InternalNodeName.NID_RECIPIENT_TABLE);
                        if (subnode != null)
                        {
                            DataTree dataTree = subnode.DataTree;
                            if (dataTree != null)
                            {
                                SubnodeBTree subnodeBTree = subnode.SubnodeBTree;
                                HeapOnNode heap = new HeapOnNode(dataTree);
                                m_recipientsTable = new RecipientsTable(heap, subnodeBTree);
                            }
                        }
                    }
                }
                return m_recipientsTable;
            }
        }

        public AttachmentObject GetAttachmentObject(int attachmentIndex)
        {
            if (this.AttachmentTable != null)
            {
                uint subnodeID = this.AttachmentTable.GetAttachmentSubnodeID(attachmentIndex);
                Subnode subnode = this.SubnodeBTree.GetSubnode(subnodeID);
                AttachmentObject result = new AttachmentObject(subnode);
                return result;
            }
            else
            {
                return null;
            }
        }

        public MessageRecipient GetRecipient(int recipientIndex)
        {
            if (this.RecipientsTable != null)
            {
                return this.RecipientsTable.GetRecipient(recipientIndex);
            }
            else
            {
                return null;
            }
        }

        public int GetTotalSizeOfAllAttachments()
        {
            int result = 0;
            for (int index = 0; index < this.AttachmentCount; index++)
            {
                AttachmentObject attachment = GetAttachmentObject(index);
                Nullable<int> attachmentSize = attachment.PC.GetInt32Property(PropertyID.PidTagAttachSize);
                if (attachmentSize.HasValue)
                {
                    result += attachmentSize.Value;
                }
            }
            return result;
        }

        public int AttachmentCount
        {
            get
            {
                if (this.AttachmentTable != null)
                {
                    return this.AttachmentTable.RowCount;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int RecipientCount
        {
            get
            {
                if (this.RecipientsTable != null)
                {
                    return this.RecipientsTable.RowCount;
                }
                else
                {
                    return 0;
                }
            }
        }

        public static MessageObject CreateNewMessage(PSTFile file, FolderItemTypeName folderItemType, NodeID parentNodeID)
        {
            return CreateNewMessage(file, folderItemType, parentNodeID, Guid.NewGuid());
        }

        public static MessageObject CreateNewMessage(PSTFile file, FolderItemTypeName folderItemType, NodeID parentNodeID, Guid searchKey)
        {
            // [MS-PST] The following properties must be present in any valid Message object PC:
            PropertyContext pc = PropertyContext.CreateNewPropertyContext(file);
            pc.SetStringProperty(PropertyID.PidTagMessageClass, GetMessageClass(folderItemType));
            
            pc.SetInt32Property(PropertyID.PidTagMessageFlags, 0);
            pc.SetInt32Property(PropertyID.PidTagMessageStatus, 0);
            pc.SetDateTimeProperty(PropertyID.PidTagCreationTime, DateTime.UtcNow);
            pc.SetDateTimeProperty(PropertyID.PidTagLastModificationTime, DateTime.UtcNow);
            pc.SetDateTimeProperty(PropertyID.PidTagClientSubmitTime, DateTime.UtcNow);
            pc.SetDateTimeProperty(PropertyID.PidTagMessageDeliveryTime, DateTime.UtcNow);
            byte[] conversationIndex = ConversationIndexHeader.GenerateNewConversationIndex().GetBytes();
            pc.SetBytesProperty(PropertyID.PidTagConversationIndex, conversationIndex);

            // PidTagSearchKey is apparently a GUID
            pc.SetBytesProperty(PropertyID.PidTagSearchKey, LittleEndianConverter.GetBytes(searchKey));
            
            pc.SaveChanges();

            NodeID pcNodeID = file.Header.AllocateNextNodeID(NodeTypeName.NID_TYPE_NORMAL_MESSAGE);
            file.NodeBTree.InsertNodeEntry(pcNodeID, pc.DataTree, pc.SubnodeBTree, parentNodeID);

            // NOTE: According to [MS-PST], A Recipient Table MUST exist for any Message object,
            //       However, in practice even outlook itself does not always create it.
            PSTNode pcNode = new PSTNode(file, pcNodeID, pc.DataTree, pc.SubnodeBTree);
            MessageObject message = new MessageObject(pcNode);

            return message;
        }

        public static string GetMessageClass(FolderItemTypeName folderItemType)
        {
            switch (folderItemType)
            {
                case FolderItemTypeName.Appointment:
                    return "IPM.Appointment";
                case FolderItemTypeName.Contact:
                    return "IPM.Contact";
                case FolderItemTypeName.Journal:
                    return "IPM.Journal";
                case FolderItemTypeName.Note:
                    return "IPM.Note";
                case FolderItemTypeName.StickyNote:
                    return "IPM.StickyNote";
                case FolderItemTypeName.Task:
                    return "IPM.Task";
                default:
                    return null;
            }
        }

        public static MessageObject GetMessage(PSTFile file, NodeID nodeID)
        {
            PSTNode node = file.GetNode(nodeID);
            return new MessageObject(node);
        }

        public static byte[] CalculateConversationID(string conversationTopic)
        { 
            // we assume PidTagConversationIndexTracking is false

            // The application MUST use up to 255 of the first nonzero characters of
            // the little-endian UTF-16 representation of the PidTagConversationTopic property
            byte[] data = UnicodeEncoding.Unicode.GetBytes(conversationTopic.ToUpper());
            if (data.Length > 255)
            {
                byte[] temp = new byte[255];
                Array.Copy(data, 0, temp, 0, 255);
                data = temp;
            }

            System.Security.Cryptography.MD5CryptoServiceProvider md5Provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash = md5Provider.ComputeHash(data);
            return hash;
        }
    }
}
