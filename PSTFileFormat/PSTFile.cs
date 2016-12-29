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
using System.IO;
using System.Text;
using Utilities;

namespace PSTFileFormat
{
    public partial class PSTFile
    {
        private PSTHeader m_header;
        private BlockBTree m_blockBTree;
        private NodeBTree m_nodeBTree;
        private KeyValuePairList<ulong, int> m_offsetsToFree = new KeyValuePairList<ulong, int>();
        private SearchManagementQueue m_searchManagementQueue;
        private SearchDomainObject m_searchDomainObject;
        private PropertyNameToIDMap m_map;
        private bool m_isSavingChanges = false;
        private WriterCompatibilityMode m_writerCompatibilityMode;
        
        FileStream m_stream;

        public PSTFile(string path) : this(path, FileAccess.ReadWrite, WriterCompatibilityMode.Outlook2003RTM)
        { 
        }

        public PSTFile(string path, FileAccess fileAccess) : this(path, fileAccess, WriterCompatibilityMode.Outlook2003RTM)
        { 
        }

        public PSTFile(string path, FileAccess fileAccess, WriterCompatibilityMode writerCompatibilityMode)
        {
            m_stream = new FileStream(path, FileMode.Open, fileAccess);
            m_header = PSTHeader.ReadFromStream(m_stream, writerCompatibilityMode);
            m_writerCompatibilityMode = writerCompatibilityMode;
        }

        public void CloseFile()
        {
            if (m_stream != null)
            {
                m_stream.Close();
            }
        }

        ~PSTFile()
        {
            CloseFile();
        }

        public BlockBTreeEntry FindBlockEntryByBlockID(ulong blockID)
        {
            return this.BlockBTree.FindBlockEntryByBlockID(blockID);
        }

        public NodeBTreeEntry FindNodeEntryByNodeID(uint nodeID)
        {
            return this.NodeBTree.FindNodeEntryByNodeID(nodeID);
        }

        public Nullable<ulong> FindBlockIDByNodeID(uint nodeID)
        {
            NodeBTreeEntry entry = FindNodeEntryByNodeID(nodeID);
            if (entry == null)
            {
                return null;
            }
            else
            {
                return entry.bidData.Value;
            }
        }

        public Block FindBlockByBlockID(BlockID blockID)
        {
            return FindBlockByBlockID(blockID.LookupValue);
        }

        public Block FindBlockByBlockID(ulong blockID)
        {
            BlockBTreeEntry entry = FindBlockEntryByBlockID(blockID);
            if (entry != null)
            {
                return Block.ReadFromStream(m_stream, entry.BREF, entry.cb, Header.bCryptMethod);
            }
            else
            {
                return null;
            }
        }

        public void BeginSavingChanges()
        {
            if (!Header.root.IsAllocationMapValid)
            {
                throw new InvalidAllocationMapException();
            }

            // We want to make sure outlook will rebuild the allocation map if the operation does not complete successfully
            AllocationHelper.InvalidateAllocationMap(this);
            m_isSavingChanges = true;
        }

        public void CommitChanges()
        {
            // We must save changes to the SMQ before committing the BBT / NBT
            if (m_searchManagementQueue != null)
            {
                m_searchManagementQueue.SaveChanges();
            }

            this.BlockBTree.SaveChanges();
            this.NodeBTree.SaveChanges();

            // Only after we saved changes to the BBT, we may free blocks,
            // this is to prevent blocks that are referenced by the BBT to be freed prematurely (before changes are committed)
            foreach (KeyValuePair<ulong, int> offsetToFree in m_offsetsToFree)
            {
                ulong offset = offsetToFree.Key;
                int length = offsetToFree.Value;
                AllocationHelper.FreeAllocation(this, (long)offset, length);
            }
            m_offsetsToFree.Clear();

            UpdateBlockBTreeRootReference();
            UpdateNodeBTreeRootReference();
        }

        public void EndSavingChanges()
        {
            CommitChanges();

            AllocationHelper.ValidateAllocationMap(this);
            m_isSavingChanges = false;
        }

        public void UpdateBlockBTreeRootReference()
        {
            // update header
            if (BlockBTree.RootPage.BlockID.Value != Header.root.BREFBBT.bid.Value)
            {
                Header.root.BREFBBT.bid = BlockBTree.RootPage.BlockID;
                Header.root.BREFBBT.ib = BlockBTree.RootPage.Offset;
                Header.AllocateNextUniqueID();
                Header.WriteToStream(BaseStream, m_writerCompatibilityMode);
            }
        }

        public void UpdateNodeBTreeRootReference()
        {
            // update header
            if (NodeBTree.RootPage.BlockID.Value != Header.root.BREFNBT.bid.Value)
            {
                Header.root.BREFNBT.bid = NodeBTree.RootPage.BlockID;
                Header.root.BREFNBT.ib = NodeBTree.RootPage.Offset;
                Header.WriteToStream(BaseStream, m_writerCompatibilityMode);
            }
        }

        public void MarkAllocationToBeFreed(ulong offset, int length)
        {
            m_offsetsToFree.Add(offset, length);
        }

        public PSTNode GetNode(InternalNodeName nodeName)
        {
            return GetNode((uint)nodeName);
        }

        public PSTNode GetNode(uint nodeID)
        {
            return GetNode(new NodeID(nodeID));
        }

        public PSTNode GetNode(NodeID nodeID)
        {
            PSTNode node = PSTNode.GetPSTNode(this, nodeID);
            return node;
        }

        public PSTFolder GetFolder(uint nodeID)
        {
            return GetFolder(new NodeID(nodeID));
        }

        public PSTFolder GetFolder(NodeID nodeID)
        {
            PSTFolder folder = PSTFolder.GetFolder(this, nodeID);
            return folder;
        }

        public MessageObject GetMessage(NodeID nodeID)
        {
            MessageObject message = MessageObject.GetMessage(this, nodeID);
            return message;
        }

        public PropertyNameToIDMap NameToIDMap
        {
            get
            {
                if (m_map == null)
                {
                    m_map = new PropertyNameToIDMap(this);
                }
                return m_map;
            }
        }

        public SearchManagementQueue SearchManagementQueue
        {
            get
            {
                if (m_searchManagementQueue == null)
                {
                    m_searchManagementQueue = new SearchManagementQueue(this);
                }
                return m_searchManagementQueue;
            }
        }

        public SearchDomainObject SearchDomainObject
        {
            get
            {
                if (m_searchDomainObject == null)
                {
                    m_searchDomainObject = new SearchDomainObject(this);
                }
                return m_searchDomainObject;
            }
        }

        public PSTFolder RootFolder
        {
            get
            {
                NodeID rootFolderNodeID = new NodeID((uint)InternalNodeName.NID_ROOT_FOLDER);
                PSTFolder rootFolder = PSTFolder.GetFolder(this, rootFolderNodeID);
                return rootFolder;
            }
        }

        public PSTFolder TopOfPersonalFolders
        {
            get
            {
                NodeID topFolderNodeID;
                if (m_header.wVerClient == (int)PSTHeader.ClientVersion.PersonalFolders)
                {
                    topFolderNodeID = new NodeID((uint)InternalNodeName.NID_TOP_OF_PERSONAL_FOLDERS);
                }
                else // OST file
                {
                    topFolderNodeID = new NodeID((uint)InternalNodeName.OST_NID_TOP_OF_PERSONAL_FOLDERS);
                }
                PSTFolder topFolder = PSTFolder.GetFolder(this, topFolderNodeID);
                return topFolder;
            }
        }

        public PSTHeader Header
        {
            get
            {
                return m_header;
            }
        }

        public Stream BaseStream
        {
            get
            {
                return m_stream;
            }
        }
        
        public BlockBTree BlockBTree
        {
            get
            {
                if (m_blockBTree == null)
                {
                    BTreePage blockBTreeRootPage = BTreePage.ReadFromStream(m_stream, m_header.root.BREFBBT);
                    m_blockBTree = new BlockBTree(this, blockBTreeRootPage);
                }
                return m_blockBTree;
            }
        }
        
        public NodeBTree NodeBTree
        {
            get
            {
                if (m_nodeBTree == null)
                {
                    BTreePage nodeBTreeRootPage = BTreePage.ReadFromStream(m_stream, m_header.root.BREFNBT);
                    m_nodeBTree = new NodeBTree(this, nodeBTreeRootPage);
                }
                return m_nodeBTree;
            }
        }

        public bool IsSavingChanges
        {
            get
            {
                return m_isSavingChanges;
            }
        }

        public WriterCompatibilityMode WriterCompatibilityMode
        {
            get
            {
                return m_writerCompatibilityMode;
            }
        }

        public static WriterCompatibilityMode AutoDetectPSTWriter(PSTFile file)
        {
            if (file.Header.root.fAMapValid > 0)
            {
                if (file.Header.root.fAMapValid == RootStructure.VALID_AMAP1)
                {
                    return WriterCompatibilityMode.Outlook2003RTM;
                }
                else if (file.Header.root.fAMapValid == RootStructure.VALID_AMAP2)
                {
                    return WriterCompatibilityMode.Outlook2007RTM;
                    // check if DList exist
                }
            }

            // default to Outlook2003
            return WriterCompatibilityMode.Outlook2003RTM;
        }
    }
}
