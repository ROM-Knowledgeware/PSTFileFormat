/* Copyright (C) 2012-2017 ROM Knowledgeware. All rights reserved.
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
    public class PSTFolder : PSTNode
    {
        private Nullable<int> m_contentCount; // We use it to update the parent hierarchy table in a single update
        private NamedTableContext m_contentsTable; // We use it for buffering purposes

        public PSTFolder(PSTNode node) : base(node)
        {
        }

        public override void SaveChanges()
        {
            if (m_contentCount.HasValue)
            {
                TableContext parentHierarchyTable = ParentFolder.GetHierarchyTable();
                int rowIndexOfFolder = parentHierarchyTable.GetRowIndex(NodeID.Value);
                parentHierarchyTable.SetInt32Property(rowIndexOfFolder, PropertyID.PidTagContentCount, m_contentCount.Value);
                parentHierarchyTable.SaveChanges(ParentFolder.GetHierarchyTableNodeID());
            }

            if (m_contentsTable != null)
            {
                m_contentsTable.SaveChanges(GetContentsTableNodeID());
            }
            base.SaveChanges();
        }

        public override void Delete()
        {
            for (int index = 0; index < MessageCount; index++)
            {
                MessageObject message = GetMessage(index);
                message.Delete();
            }
            base.Delete();
            GetHierarchyTableNode().Delete();
            GetContentsTableNode().Delete();
            GetAssociatedContentsTableNode().Delete();
        }

        public NodeID GetHierarchyTableNodeID()
        {
            return new NodeID(NodeTypeName.NID_TYPE_HIERARCHY_TABLE, NodeID.nidIndex);
        }

        public NodeID GetContentsTableNodeID()
        {
            return new NodeID(NodeTypeName.NID_TYPE_CONTENTS_TABLE, NodeID.nidIndex);
        }

        public NodeID GetAssociatedContentsTableNodeID()
        {
            return new NodeID(NodeTypeName.NID_TYPE_ASSOC_CONTENTS_TABLE, NodeID.nidIndex);
        }

        public PSTNode GetHierarchyTableNode()
        {
            return this.File.GetNode(GetHierarchyTableNodeID());
        }

        public PSTNode GetContentsTableNode()
        {
            return this.File.GetNode(GetContentsTableNodeID());
        }

        public PSTNode GetAssociatedContentsTableNode()
        {
            return this.File.GetNode(GetAssociatedContentsTableNodeID());
        }

        public TableContext GetHierarchyTable()
        {
            PSTNode node = GetHierarchyTableNode();
            return node.TableContext;
        }

        public NamedTableContext GetContentsTable()
        {
            if (m_contentsTable == null)
            {
                PSTNode node = GetContentsTableNode();
                m_contentsTable = node.NamedTableContext;
            }
            return m_contentsTable;
        }

        public TableContext GetAssociatedContentsTable()
        {
            PSTNode node = GetAssociatedContentsTableNode();
            return node.TableContext;
        }

        public List<PSTFolder> GetChildFolders()
        {
            TableContext tc = GetHierarchyTable();

            List<PSTFolder> result = new List<PSTFolder>();

            if (tc != null)
            {
                for (int index = 0; index < tc.RowCount; index++)
                {
                    // dwRowID is the NodeID
                    NodeID childNodeID = new NodeID(tc.GetRowID(index));
                    if (childNodeID.nidType == NodeTypeName.NID_TYPE_NORMAL_FOLDER)
                    {
                        PSTFolder childFolder = PSTFolder.GetFolder(File, childNodeID);
                        result.Add(childFolder);
                    }
                }
            }
            return result;
        }

        public PSTFolder FindChildFolder(string displayNameToFind)
        {
            TableContext tc = GetHierarchyTable();
            if (tc != null)
            {
                for (int index = 0; index < tc.RowCount; index++)
                {
                    string displayName = tc.GetStringProperty(index, PropertyID.PidTagDisplayName);
                    if (displayName == displayNameToFind)
                    {
                        NodeID childNodeID = new NodeID(tc.GetRowID(index));
                        return PSTFolder.GetFolder(File, childNodeID);
                    }
                }
            }

            return null;
        }

        #region Get Property
        public Nullable<bool> GetMessageBooleanProperty(int rowIndex, PropertyID propertyID)
        {
            NamedTableContext tc = GetContentsTable();
            int columnIndex = tc.FindColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypBoolean);
            if (columnIndex >= 0)
            {
                return tc.GetBooleanProperty(rowIndex, columnIndex);
            }
            else
            {
                return GetMessage(rowIndex).PC.GetBooleanProperty(propertyID);
            }
        }

        public Nullable<bool> GetMessageBooleanProperty(int rowIndex, PropertyName propertyName)
        {
            NamedTableContext tc = GetContentsTable();
            int columnIndex = tc.FindColumnIndexByPropertyTag(propertyName, PropertyTypeName.PtypBoolean);
            if (columnIndex >= 0)
            {
                return tc.GetBooleanProperty(rowIndex, columnIndex);
            }
            else
            {
                return GetMessage(rowIndex).PC.GetBooleanProperty(propertyName);
            }
        }

        public Nullable<DateTime> GetMessageDateTimeProperty(int rowIndex, PropertyID propertyID)
        {
            NamedTableContext tc = GetContentsTable();
            int columnIndex = tc.FindColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypTime);
            if (columnIndex >= 0)
            {
                return tc.GetDateTimeProperty(rowIndex, columnIndex);
            }
            else
            {
                return GetMessage(rowIndex).PC.GetDateTimeProperty(propertyID);
            }
        }

        public Nullable<DateTime> GetMessageDateTimeProperty(int rowIndex, PropertyName propertyName)
        {
            NamedTableContext tc = GetContentsTable();
            int columnIndex = tc.FindColumnIndexByPropertyTag(propertyName, PropertyTypeName.PtypTime);
            if (columnIndex >= 0)
            {
                return tc.GetDateTimeProperty(rowIndex, columnIndex);
            }
            else
            {
                return GetMessage(rowIndex).PC.GetDateTimeProperty(propertyName);
            }
        }

        public string GetMessageStringProperty(int rowIndex, PropertyID propertyID)
        {
            NamedTableContext tc = GetContentsTable();
            int columnIndex = tc.FindColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypString);
            if (columnIndex >= 0)
            {
                return tc.GetStringProperty(rowIndex, columnIndex);
            }
            else
            {
                return GetMessage(rowIndex).PC.GetStringProperty(propertyID);
            }
        }

        public string GetMessageStringProperty(int rowIndex, PropertyName propertyName)
        {
            NamedTableContext tc = GetContentsTable();
            int columnIndex = tc.FindColumnIndexByPropertyTag(propertyName, PropertyTypeName.PtypString);
            if (columnIndex >= 0)
            {
                return tc.GetStringProperty(rowIndex, columnIndex);
            }
            else
            {
                return GetMessage(rowIndex).PC.GetStringProperty(propertyName);
            }
        }

        public byte[] GetMessageBytesProperty(int rowIndex, PropertyID propertyID)
        {
            NamedTableContext tc = GetContentsTable();
            int columnIndex = tc.FindColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypBinary);
            if (columnIndex >= 0)
            {
                return tc.GetBytesProperty(rowIndex, columnIndex);
            }
            else
            {
                return GetMessage(rowIndex).PC.GetBytesProperty(propertyID);
            }
        }

        public byte[] GetMessageBytesProperty(int rowIndex, PropertyName propertyName)
        {
            NamedTableContext tc = GetContentsTable();
            int columnIndex = tc.FindColumnIndexByPropertyTag(propertyName, PropertyTypeName.PtypBinary);
            if (columnIndex >= 0)
            {
                return tc.GetBytesProperty(rowIndex, columnIndex);
            }
            else
            {
                return GetMessage(rowIndex).PC.GetBytesProperty(propertyName);
            }
        }
        #endregion

        public MessageObject GetMessage(int index)
        {
            TableContext tc = GetContentsTable();
            if (tc != null)
            {
                if (index < tc.RowCount)
                {
                    // dwRowID is the MessageID
                    NodeID nodeID = new NodeID(tc.GetRowID(index));
                    MessageObject message = this.File.GetMessage(nodeID);
                    return message;
                }
            }
            return null;
        }

        /// <summary>
        /// Changes will be saved immediately
        /// </summary>
        public PSTFolder CreateChildFolder(string folderName, FolderItemTypeName folderItemType)
        {
            PSTFolder childFolder = CreateNewFolder(this.File, folderName, folderItemType, this.NodeID);
            PropertyContext pc = this.PC;
            if (!pc.GetBooleanProperty(PropertyID.PidTagSubfolders).Value)
            {
                pc.SetBooleanProperty(PropertyID.PidTagSubfolders, true);
                pc.SaveChanges(NodeID);
            }
            // update hierarchy table of parent (set PidTagSubfolders of current folder to true):
            TableContext parentHierarchyTable = ParentFolder.GetHierarchyTable();
            int rowIndexOfFolder = parentHierarchyTable.GetRowIndex(NodeID.Value);
            if (!parentHierarchyTable.GetBooleanProperty(rowIndexOfFolder, PropertyID.PidTagSubfolders).Value)
            {
                parentHierarchyTable.SetBooleanProperty(rowIndexOfFolder, PropertyID.PidTagSubfolders, true);
                parentHierarchyTable.SaveChanges(ParentFolder.GetHierarchyTableNodeID());
            }
            
            // update hierarchy table:
            TableContext hierarchyTable = GetHierarchyTable();
            
            hierarchyTable.AddRow(childFolder.NodeID.Value);
            int rowIndex = hierarchyTable.RowCount - 1;
            // Template properties (assured to be present)
            hierarchyTable.SetInt32Property(rowIndex, PropertyID.PidTagContentCount, 0);
            hierarchyTable.SetInt32Property(rowIndex, PropertyID.PidTagContentUnreadCount, 0);
            hierarchyTable.SetBooleanProperty(rowIndex, PropertyID.PidTagSubfolders, false);
            hierarchyTable.SetStringProperty(rowIndex, PropertyID.PidTagDisplayName, folderName);
            hierarchyTable.SetStringProperty(rowIndex, PropertyID.PidTagContainerClass, GetContainerClass(folderItemType));
            hierarchyTable.SetInt32Property(rowIndex, PropertyID.PidTagLtpRowId, (int)childFolder.NodeID.Value);
            // PidTagLtpRowVer uses dwUnique
            int rowVersion = (int)File.Header.AllocateNextUniqueID();
            hierarchyTable.SetInt32Property(rowIndex, PropertyID.PidTagLtpRowVer, rowVersion);

            hierarchyTable.SaveChanges(GetHierarchyTableNodeID());

            this.File.SearchManagementQueue.AddFolder(childFolder.NodeID, this.NodeID);

            return childFolder;
        }

        /// <summary>
        /// Dissociate child folder from this (parent) folder, changes will be saved immediately
        /// </summary>
        public void RemoveChildFolder(PSTFolder childFolder)
        {
            TableContext hierarchyTable = GetHierarchyTable();
            int rowIndex = hierarchyTable.GetRowIndex(childFolder.NodeID.Value);
            if (rowIndex >= 0)
            {
                if (this.ChildFolderCount == 1)
                {
                    PropertyContext pc = this.PC;
                    pc.SetBooleanProperty(PropertyID.PidTagSubfolders, false);
                    pc.SaveChanges(NodeID);

                    // update hierarchy table of parent (set PidTagSubfolders of current folder to false):
                    TableContext parentHierarchyTable = ParentFolder.GetHierarchyTable();
                    int rowIndexOfFolder = parentHierarchyTable.GetRowIndex(NodeID.Value);
                    parentHierarchyTable.SetBooleanProperty(rowIndexOfFolder, PropertyID.PidTagSubfolders, false);
                    parentHierarchyTable.SaveChanges(ParentFolder.GetHierarchyTableNodeID());
                }

                hierarchyTable.DeleteRow(rowIndex);

                hierarchyTable.SaveChanges(GetHierarchyTableNodeID());
            }
        }

        /// <summary>
        /// Changes will be saved immediately
        /// </summary>
        public void DeleteChildFolder(PSTFolder childFolder)
        {
            RemoveChildFolder(childFolder);
            childFolder.Delete();

            this.File.SearchManagementQueue.DeleteFolder(childFolder.NodeID, this.NodeID);
        }

        public void AddMessage(MessageObject message)
        {
            int contentCount = PC.GetInt32Property(PropertyID.PidTagContentCount).Value;
            contentCount++;

            PC.SetInt32Property(PropertyID.PidTagContentCount, contentCount);
            // Changes to the PC must be saved later

            // update hierarchy table of parent (will be saved during SaveChanges):
            m_contentCount = contentCount;

            // Update contents table (will be saved during SaveChanges)
            NamedTableContext contentsTable = GetContentsTable();
            AddContentTableColumns(contentsTable);

            int rowIndex = contentsTable.AddRow(message.NodeID.Value);

            UpdateMessage(message, contentsTable, rowIndex);

            this.File.SearchManagementQueue.AddMessage(message.NodeID, this.NodeID);
        }

        /// <summary>
        /// Dissociate a message from this folder
        /// </summary>
        public void RemoveMessage(MessageObject message)
        {
            NamedTableContext contentsTable = GetContentsTable();
            int rowIndex = contentsTable.GetRowIndex(message.NodeID.Value);
            if (rowIndex >= 0)
            {
                int contentCount = PC.GetInt32Property(PropertyID.PidTagContentCount).Value;
                contentCount--;

                PC.SetInt32Property(PropertyID.PidTagContentCount, contentCount);
                // Changes to the PC must be saved later

                // update hierarchy table of parent (will be saved during SaveChanges):
                m_contentCount = contentCount;

                // Update contents table (will be saved during SaveChanges)
                contentsTable.DeleteRow(rowIndex);
            }
        }

        public void DeleteMessage(MessageObject message)
        {
            RemoveMessage(message);
            message.Delete();

            this.File.SearchManagementQueue.DeleteMessage(message.NodeID, this.NodeID);
        }

        /// <summary>
        /// Add columns if not exist
        /// </summary>
        public virtual void AddContentTableColumns(NamedTableContext contentsTable)
        {
            contentsTable.AddPropertyColumnIfNotExist(PropertyID.PidTagIconIndex, PropertyTypeName.PtypInteger32);
            contentsTable.AddPropertyColumnIfNotExist(PropertyID.PidTagSearchKey, PropertyTypeName.PtypBinary);
            contentsTable.AddPropertyColumnIfNotExist(PropertyID.PidTagInternetMessageId, PropertyTypeName.PtypString);
            contentsTable.AddPropertyColumnIfNotExist(PropertyID.PidTagFlagStatus, PropertyTypeName.PtypInteger32);
            contentsTable.AddPropertyColumnIfNotExist(PropertyID.PidTagReplyTime, PropertyTypeName.PtypTime);
        }

        public void UpdateMessage(MessageObject message)
        {
            TableContext contentsTable = GetContentsTable();
            UpdateMessage(message, contentsTable);
        }

        private void UpdateMessage(MessageObject message, TableContext contentsTable)
        { 
            int rowIndex = contentsTable.GetRowIndex(message.NodeID.Value);
            if (rowIndex >= 0)
            {
                UpdateMessage(message, contentsTable, rowIndex);
            }
        }

        /// <summary>
        /// Properties that were removed from the message must be explicitly removed from the TC
        /// </summary>
        public void UpdateMessage(MessageObject message, TableContext contentsTable, int rowIndex)
        {
            TableContextHelper.CopyProperties(message.PC, contentsTable, rowIndex);
            // Note: Outlook 2003 simply iterates over all of the table columns:
            
            // Outlook 2003 Content Table Template fields:
            contentsTable.SetInt32Property(rowIndex, PropertyID.PidTagLtpRowId, (int)message.NodeID.Value);
            // UNDOCUMENTED - Outlook uses dwUnique for PidTagLtpRowVer
            contentsTable.SetInt32Property(rowIndex, PropertyID.PidTagLtpRowVer, (int)File.Header.AllocateNextUniqueID());
        }

        public string DisplayName
        {
            get
            {
                PropertyContext pc = this.PC;
                if (pc != null)
                {
                    return pc.GetStringProperty(PropertyID.PidTagDisplayName);
                }
                else
                {
                    return null;
                }
            }
            set
            { 
                PropertyContext pc = this.PC;
                if (pc != null)
                {
                    pc.SetStringProperty(PropertyID.PidTagDisplayName, value);
                }
            }
        }

        public string ContainerClass
        {
            get
            {
                PropertyContext pc = this.PC;
                if (pc != null)
                {
                    return pc.GetStringProperty(PropertyID.PidTagContainerClass);
                }
                else
                {
                    return null;
                }
            }
        }

        public int MessageCount
        {
            get
            {
                PropertyContext pc = this.PC;
                if (pc != null)
                {
                    return pc.GetInt32Property(PropertyID.PidTagContentCount).Value;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// a.k.a. Description
        /// </summary>
        public string Comment
        {
            get
            {
                PropertyContext pc = this.PC;
                if (pc != null)
                {
                    return pc.GetStringProperty(PropertyID.PidTagComment);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                PropertyContext pc = this.PC;
                if (pc != null)
                {
                    pc.SetStringProperty(PropertyID.PidTagComment, value);
                }
            }
        }

        public int ChildFolderCount
        {
            get
            {
                TableContext tc = GetHierarchyTable();

                if (tc != null)
                {
                    return tc.RowCount;
                }
                return 0;
            }
        }

        [Obsolete]
        public int MessageCountDirect
        {
            get
            { 
                TableContext tc = GetContentsTable();
                if (tc != null)
                {
                    return tc.RowCount;
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool HasSubfolders
        {
            get
            { 
                PropertyContext pc = this.PC;
                if (pc != null)
                {
                    return pc.GetBooleanProperty(PropertyID.PidTagSubfolders).Value;
                }
                else
                {
                    return false;
                }
            }
        }

        public FolderItemTypeName ItemType
        {
            get
            {
                return GetItemType(this.ContainerClass);
            }
        }

        public bool IsCalendarFolder
        {
            get
            {
                return (this.ItemType == FolderItemTypeName.Appointment);
            }
        }

        public PSTFolder ParentFolder
        {
            get
            {
                return PSTFolder.GetFolder(this.File, this.ParentNodeID);
            }
        }

        /// <summary>
        /// Caller must update its hierarchy table to include the new child
        /// </summary>
        public static PSTFolder CreateNewFolder(PSTFile file, string folderName, FolderItemTypeName folderItemType, NodeID parentNodeID)
        {
            // create the normal folder node
            PropertyContext pc = PropertyContext.CreateNewPropertyContext(file);
            pc.SetStringProperty(PropertyID.PidTagDisplayName, folderName);
            pc.SetInt32Property(PropertyID.PidTagContentCount, 0);
            pc.SetInt32Property(PropertyID.PidTagContentUnreadCount, 0);
            pc.SetBooleanProperty(PropertyID.PidTagSubfolders, false);
            pc.SetStringProperty(PropertyID.PidTagContainerClass, GetContainerClass(folderItemType));
            pc.SaveChanges();

            NodeID pcNodeID = file.Header.AllocateNextFolderNodeID();
            file.NodeBTree.InsertNodeEntry(pcNodeID, pc.DataTree, pc.SubnodeBTree, parentNodeID);
            PSTNode pcNode = new PSTNode(file, pcNodeID, pc.DataTree, pc.SubnodeBTree);

            // There is no need to create a new empty TC, we can simply point to the appropriate template
            // and only update the reference to another data tree during modification
            PSTNode hierarchyTableTemplateNode = file.GetNode(InternalNodeName.NID_HIERARCHY_TABLE_TEMPLATE);
            NodeID hierarchyTableNodeID = new NodeID(NodeTypeName.NID_TYPE_HIERARCHY_TABLE, pcNodeID.nidIndex);
            file.NodeBTree.InsertNodeEntry(hierarchyTableNodeID, hierarchyTableTemplateNode.DataTree, null, new NodeID(0));
            file.BlockBTree.IncrementBlockEntryReferenceCount(hierarchyTableTemplateNode.DataTree.RootBlock.BlockID);

            PSTNode contentsTableTemplateNode = file.GetNode(InternalNodeName.NID_CONTENTS_TABLE_TEMPLATE);
            NodeID contentsTableNodeID = new NodeID(NodeTypeName.NID_TYPE_CONTENTS_TABLE, pcNodeID.nidIndex);
            file.NodeBTree.InsertNodeEntry(contentsTableNodeID, contentsTableTemplateNode.DataTree, null, new NodeID(0));
            file.BlockBTree.IncrementBlockEntryReferenceCount(contentsTableTemplateNode.DataTree.RootBlock.BlockID);

            PSTNode associatedContentsTableTemplateNode = file.GetNode(InternalNodeName.NID_ASSOC_CONTENTS_TABLE_TEMPLATE);
            NodeID associatedContentsTableNodeID = new NodeID(NodeTypeName.NID_TYPE_ASSOC_CONTENTS_TABLE, pcNodeID.nidIndex);
            file.NodeBTree.InsertNodeEntry(associatedContentsTableNodeID, associatedContentsTableTemplateNode.DataTree, null, new NodeID(0));
            file.BlockBTree.IncrementBlockEntryReferenceCount(associatedContentsTableTemplateNode.DataTree.RootBlock.BlockID);

            PSTFolder folder = PSTFolder.GetFolder(pcNode);
            return folder;
        }

        public static string GetContainerClass(FolderItemTypeName folderItemType)
        {
            switch (folderItemType)
            {
                case FolderItemTypeName.Appointment:
                    return "IPF.Appointment";
                case FolderItemTypeName.Contact:
                    return "IPF.Contact";
                case FolderItemTypeName.Journal:
                    return "IPF.Journal";
                case FolderItemTypeName.Note:
                    return "IPF.Note";
                case FolderItemTypeName.StickyNote:
                    return "IPF.StickyNote";
                case FolderItemTypeName.Task:
                    return "IPF.Task";
                default:
                    return null;
            }
        }

        public static FolderItemTypeName GetItemType(string containerClass)
        {
            switch (containerClass)
            {
                case "IPF.Appointment":
                    return FolderItemTypeName.Appointment;
                case "IPF.Contact":
                    return FolderItemTypeName.Contact;
                case "IPF.Journal":
                    return FolderItemTypeName.Journal;
                case "IPF.Note":
                    return FolderItemTypeName.Note; //	Mail Messages and notes
                case "IPF.StickyNote":
                    return FolderItemTypeName.StickyNote;
                case "IPF.Task":
                    return FolderItemTypeName.Task;
                default:
                    return FolderItemTypeName.Unspecified;
            }
        }

        public static PSTFolder GetFolder(PSTFile file, NodeID nodeID)
        {
            if (nodeID.nidType == NodeTypeName.NID_TYPE_NORMAL_FOLDER)
            {
                PSTNode node = file.GetNode(nodeID);
                if (node != null)
                {
                    return GetFolder(node);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new Exception("Node ID does not belong to a folder");
            }
        }

        public static PSTFolder GetFolder(PSTNode node)
        {
            if (node.NodeID.nidType == NodeTypeName.NID_TYPE_NORMAL_FOLDER)
            {
                PropertyContext pc = node.PC;
                if (pc != null)
                {
                    string containerClass = pc.GetStringProperty(PropertyID.PidTagContainerClass);
                    FolderItemTypeName itemType = GetItemType(containerClass);
                    switch (itemType)
                    {
                        case FolderItemTypeName.Appointment:
                            return new CalendarFolder(node);
                        case FolderItemTypeName.Note:
                            return new MailFolder(node);
                        default:
                            return new PSTFolder(node);
                    }
                }
                else
                {
                    throw new Exception("PC is null");
                }
            }
            else
            {
                throw new Exception("Node ID does not belong to a folder");
            }
        }
    }
}
