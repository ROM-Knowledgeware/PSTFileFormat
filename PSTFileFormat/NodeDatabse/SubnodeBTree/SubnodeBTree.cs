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
    public class SubnodeBTree : BufferedBlockStore
    {
        Block m_rootBlock; // can be Intermediate block or leaf block (or null)

        /// <summary>
        /// Create a new Subnode BTree
        /// </summary>
        public SubnodeBTree(PSTFile file) : base(file)
        {
            m_rootBlock = new SubnodeLeafBlock();
            AddBlock(m_rootBlock);
        }

        public SubnodeBTree(PSTFile file, Block rootBlock) : base(file)
        {
            m_rootBlock = rootBlock;
        }

        public void Delete()
        {
            List<SubnodeLeafBlock> leaves = GetLeafBlocks();
            foreach (SubnodeLeafBlock leaf in leaves)
            {
                foreach (SubnodeLeafEntry entry in leaf.rgentries)
                {
                    Subnode subnode = Subnode.GetSubnode(this.File, entry);
                    subnode.Delete();
                }
                DeleteBlock(leaf);
            }

            // we could have already deleted our root block (if it's a leaf)
            if (m_rootBlock is SubnodeIntermediateBlock)
            {
                DeleteBlock(m_rootBlock);
            }
            m_rootBlock = null;
            
            SaveChanges();
        }

        public override void SaveChanges()
        {
            if (m_rootBlock is SubnodeIntermediateBlock)
            {
                if (((SubnodeIntermediateBlock)m_rootBlock).rgentries.Count == 0)
                {
                    // It's logically derived that we can't have an SIBLOCK with 0 entries
                    DeleteBlock(m_rootBlock);
                    m_rootBlock = null;
                }
            }
            else if (m_rootBlock is SubnodeLeafBlock)
            {
                if (((SubnodeLeafBlock)m_rootBlock).rgentries.Count == 0)
                {
                    // http://social.msdn.microsoft.com/Forums/en-US/os_binaryfile/thread/a5f9c653-40f5-4638-85d3-00c54607d984/
                    // We are not allowed to have an SLBLOCK with 0 entries
                    DeleteBlock(m_rootBlock);
                    m_rootBlock = null;
                }
            }
            base.SaveChanges();
        }

        /// <summary>
        /// Find the SubnodeLeafBlock where the entry with the given key should be located
        /// We use this method for search and insertion
        /// </summary>
        public SubnodeLeafBlock FindLeafBlock(uint subnodeID)
        {
            if (m_rootBlock == null)
            {
                return null;
            }
            else if (m_rootBlock is SubnodeLeafBlock)
            {
                // We do not want to return the reference
                return (SubnodeLeafBlock)m_rootBlock.Clone();
            }
            else if (m_rootBlock is SubnodeIntermediateBlock)
            {
                SubnodeIntermediateBlock intermediateBlock = (SubnodeIntermediateBlock)m_rootBlock;

                int index = intermediateBlock.IndexOfIntermediateEntryWithMatchingRange(subnodeID);

                BlockID blockID = intermediateBlock.rgentries[index].bid;
                return (SubnodeLeafBlock)GetBlock(blockID.LookupValue);
            }
            else
            {
                throw new Exception("Invalid Subnode BTree root block");
            }
        }

        public SubnodeLeafEntry GetLeafEntry(uint subnodeID)
        {
            Block block = FindLeafBlock(subnodeID);

            if (block is SubnodeLeafBlock)
            {
                SubnodeLeafBlock leafBlock = (SubnodeLeafBlock)block;
                int index = leafBlock.IndexOfLeafEntry(subnodeID);
                if (index >= 0)
                {
                    return leafBlock.rgentries[index];
                }
            }

            return null;
        }

        private void UpdateBlockAndReferences(SubnodeLeafBlock leafBlock)
        {
            ulong existingBlockID = leafBlock.BlockID.Value;
            UpdateBlock(leafBlock);
            if (m_rootBlock is SubnodeIntermediateBlock)
            {
                if (leafBlock.BlockID.Value != existingBlockID)
                {
                    // A new block has been written instead of the old one, update the root block
                    SubnodeIntermediateBlock rootBlock = (SubnodeIntermediateBlock)m_rootBlock;
                    int index = rootBlock.GetIndexOfBlockID(existingBlockID);
                    rootBlock.rgentries[index].bid = leafBlock.BlockID;
                    UpdateBlock(m_rootBlock);
                }
            }
            else if (m_rootBlock is SubnodeLeafBlock)
            {
                m_rootBlock = leafBlock;
            }
        }

        public void CreateNewRoot()
        {
            SubnodeIntermediateBlock newRoot = new SubnodeIntermediateBlock();

            SubnodeIntermediateEntry rootIntermediateEntry = new SubnodeIntermediateEntry();

            // We make sure the old root page has been updated (to prevent the BlockID from changing during update)
            UpdateBlock(m_rootBlock);

            rootIntermediateEntry.nid = new NodeID(((SubnodeLeafBlock)m_rootBlock).BlockKey);
            rootIntermediateEntry.bid = m_rootBlock.BlockID;
            newRoot.rgentries.Add(rootIntermediateEntry);
            AddBlock(newRoot);
            m_rootBlock = newRoot;
        }

        protected void InsertIntermediateEntry(SubnodeIntermediateBlock intermediateBlock, uint key, BlockID blockID)
        {
            SubnodeIntermediateEntry intermediateEntry = new SubnodeIntermediateEntry();
            intermediateEntry.nid = new NodeID(key);
            intermediateEntry.bid = blockID;
            InsertIntermediateEntry(intermediateBlock, intermediateEntry);
        }

        protected void InsertIntermediateEntry(SubnodeIntermediateBlock intermediateBlock, SubnodeIntermediateEntry entryToInsert)
        {
            if (intermediateBlock.rgentries.Count < SubnodeIntermediateBlock.MaximumNumberOfEntries)
            {
                intermediateBlock.InsertSorted(entryToInsert);
            }
            else
            {
                throw new Exception("Maximum number of entries has been reached for subnode intermediate block");
            }
        }

        public void UpdateIntermediateEntry(SubnodeIntermediateBlock intermediateBlock, BlockID blockIDOfChild, uint newKeyOfChild)
        {
            for (int index = 0; index < intermediateBlock.rgentries.Count; index++)
            {
                SubnodeIntermediateEntry entry = intermediateBlock.rgentries[index];
                if (entry.bid.Value == blockIDOfChild.Value)
                {
                    entry.nid = new NodeID(newKeyOfChild);
                }
            }
        }

        public void InsertSubnodeEntry(NodeID subnodeID, DataTree dataTree, SubnodeBTree subnodeBTree)
        {
            SubnodeLeafEntry entry = new SubnodeLeafEntry();
            entry.nid = subnodeID;
            entry.bidData = new BlockID(0);
            if (dataTree != null && dataTree.RootBlock != null)
            {
                entry.bidData = dataTree.RootBlock.BlockID;
            }

            entry.bidSub = new BlockID(0);
            if (subnodeBTree != null && subnodeBTree.RootBlock != null)
            {
                entry.bidSub = subnodeBTree.RootBlock.BlockID;
            }
            InsertSubnodeEntry(entry);
        }

        public void InsertSubnodeEntry(SubnodeLeafEntry entry)
        {
            if (m_rootBlock == null)
            {
                m_rootBlock = new SubnodeLeafBlock();
                AddBlock(m_rootBlock);
            }

            SubnodeLeafBlock leafBlock = FindLeafBlock(entry.nid.Value);
            if (leafBlock.rgentries.Count < SubnodeLeafBlock.MaximumNumberOfEntries)
            {
                leafBlock.InsertSorted(entry);
                UpdateBlockAndReferences(leafBlock);
            }
            else
            {
                // Instead of splitting, it might be better to move entries around so blocks will remain packed
                SubnodeLeafBlock newLeafBlock = leafBlock.Split();
                if (newLeafBlock.BlockKey < entry.nid.Value)
                {
                    newLeafBlock.InsertSorted(entry);
                }
                else
                {
                    int insertIndex = leafBlock.InsertSorted(entry);
                    if (insertIndex == 0 && m_rootBlock is SubnodeIntermediateBlock)
                    {
                        // block key has been modified, we must update the parent
                        UpdateIntermediateEntry((SubnodeIntermediateBlock)m_rootBlock, leafBlock.BlockID, leafBlock.BlockKey);
                    }
                }

                UpdateBlockAndReferences(leafBlock);
                AddBlock(newLeafBlock);

                if (m_rootBlock is SubnodeLeafBlock)
                {
                    // this is a root page and it's full, we have to create a new root
                    CreateNewRoot();
                }

                InsertIntermediateEntry((SubnodeIntermediateBlock)m_rootBlock, newLeafBlock.BlockKey, newLeafBlock.BlockID);
            }
        }

        public void UpdateSubnodeEntry(NodeID subnodeID, DataTree dataTree, SubnodeBTree subnodeBTree)
        {
            SubnodeLeafEntry entry = GetLeafEntry(subnodeID.Value);
            if (entry != null)
            {
                entry.bidData = new BlockID(0);
                if (dataTree != null && dataTree.RootBlock != null)
                {
                    entry.bidData = dataTree.RootBlock.BlockID;
                }

                entry.bidSub = new BlockID(0);
                if (subnodeBTree != null && subnodeBTree.RootBlock != null)
                {
                    entry.bidSub = subnodeBTree.RootBlock.BlockID;
                }

                // We have to store the new leaf block, and cascade the changes up to the root block
                SubnodeLeafBlock leafBlock = FindLeafBlock(entry.nid.Value);
                // leafBlock cannot be null
                int index = leafBlock.IndexOfLeafEntry(subnodeID.Value);
                leafBlock.rgentries[index] = entry;

                UpdateBlockAndReferences(leafBlock);
            }
        }

        public void DeleteSubnodeEntry(NodeID subnodeID)
        {
            SubnodeLeafBlock leafBlock = FindLeafBlock(subnodeID.Value);
            if (leafBlock != null)
            {
                int index = leafBlock.IndexOfLeafEntry(subnodeID.Value);
                if (index >= 0)
                {
                    leafBlock.rgentries.RemoveAt(index);
                    if (leafBlock.rgentries.Count == 0)
                    {
                        // We will only delete the root during SaveChanges()
                        // [We want to avoid setting the root to null, because we may still use it before changes are saved]
                        if (m_rootBlock is SubnodeIntermediateBlock)
                        {
                            int indexOfBlock = ((SubnodeIntermediateBlock)m_rootBlock).GetIndexOfBlockID(leafBlock.BlockID.Value);
                            ((SubnodeIntermediateBlock)m_rootBlock).rgentries.RemoveAt(indexOfBlock);
                            UpdateBlock(m_rootBlock);
                            DeleteBlock(leafBlock);
                        }
                        else
                        {
                            UpdateBlockAndReferences(leafBlock);
                        }
                    }
                    else
                    {
                        UpdateBlockAndReferences(leafBlock);
                    }
                }
            }
        }

        public Subnode GetSubnode(NodeID subnodeID)
        {
            return GetSubnode(subnodeID.Value);
        }

        public Subnode GetSubnode(uint subnodeID)
        {
            SubnodeLeafEntry entry = GetLeafEntry(subnodeID);
            if (entry != null)
            {
                Subnode result = Subnode.GetSubnode(this.File, entry);
                return result;
            }
            return null;
        }

        public SubnodeLeafBlock GetLeafBlock(int blockIndex)
        {
            if (m_rootBlock is SubnodeLeafBlock)
            {
                if (blockIndex == 0)
                {
                    return (SubnodeLeafBlock)m_rootBlock;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("blockIndex", "Invalid leaf block index");
                }
            }
            else
            {
                SubnodeIntermediateBlock intermediateBlock = (SubnodeIntermediateBlock)m_rootBlock;
                BlockID blockID = intermediateBlock.rgentries[blockIndex].bid;
                return (SubnodeLeafBlock)GetBlock(blockID.LookupValue);
            }
        }

        public List<SubnodeLeafBlock> GetLeafBlocks()
        {
            List<SubnodeLeafBlock> result = new List<SubnodeLeafBlock>();
            for (int blockIndex = 0; blockIndex < NumberOfLeafBlocks; blockIndex++)
            {
                SubnodeLeafBlock block = GetLeafBlock(blockIndex);
                result.Add(block);
            }

            return result;
        }

        public int GetDataLengthOfAllSubnodes()
        {
            List<SubnodeLeafBlock> leaves = GetLeafBlocks();
            int result = 0;
            foreach (SubnodeLeafBlock leaf in leaves)
            {
                foreach (SubnodeLeafEntry entry in leaf.rgentries)
                {
                    Subnode subnode = Subnode.GetSubnode(this.File, entry);
                    result += subnode.DataTree.TotalDataLength;
                    if (subnode.SubnodeBTree != null)
                    {
                        result += subnode.SubnodeBTree.GetDataLengthOfAllSubnodes();
                    }
                }
            }
            return result;
        }

        // We simply use a unique node ID
        [Obsolete]
        public uint GetNextNodeIndex()
        {
            uint nextIndex = 0;
            for (int blockIndex = 0; blockIndex < this.NumberOfLeafBlocks; blockIndex++)
            {
                SubnodeLeafBlock leafBlock = GetLeafBlock(blockIndex);
                foreach (SubnodeLeafEntry entry in leafBlock.rgentries)
                {
                    if (entry.nid.nidIndex >= nextIndex)
                    {
                        nextIndex = entry.nid.nidIndex + 1;
                    }
                }
            }
            return nextIndex;
        }

        public Block RootBlock
        {
            get
            {
                return m_rootBlock;
            }
        }

        public int NumberOfLeafBlocks
        {
            get
            {
                if (m_rootBlock == null)
                {
                    return 0;
                }
                else if (m_rootBlock is SubnodeLeafBlock)
                {
                    return 1;
                }
                else
                {
                    return ((SubnodeIntermediateBlock)m_rootBlock).rgentries.Count;
                }
            }
        }
    }
}
