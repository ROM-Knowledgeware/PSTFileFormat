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
    public class BufferedBlockStore
    {
        private PSTFile m_file;
        
        // We use the block buffer to store cached and modified blocks
        // besides caching, the main purpose of this buffer is to store the modifications to the blocks,
        // this way they could be written to the PST file later in a single transaction.
        private Dictionary<ulong, Block> m_blockBuffer = new Dictionary<ulong, Block>();

        // The NDB is immutable, we allocate blocks for both new blocks and modified blocks,
        // for modified blocks, we unallocate the original blocks (using m_blocksToFree).
        private List<ulong> m_blocksToWrite = new List<ulong>();
        private List<ulong> m_blocksToFree = new List<ulong>();

        protected BufferedBlockStore(PSTFile file)
        {
            m_file = file;
        }

        protected Block GetBlock(BlockID blockID)
        {
            return GetBlock(blockID.Value);
        }

        /// <summary>
        /// Will get a block from the buffer,
        /// A cloned copy of the block will be returned
        /// </summary>
        protected Block GetBlock(ulong blockID)
        {
            if (m_blockBuffer.ContainsKey(blockID))
            {
                return m_blockBuffer[blockID].Clone();
            }
            else
            {
                Block block = m_file.FindBlockByBlockID(blockID);
                m_blockBuffer.Add(blockID, block);
                return block.Clone();
            }
        }

        protected bool IsBlockPendingWrite(Block block)
        {
            return IsBlockPendingWrite(block.BlockID);
        }

        protected bool IsBlockPendingWrite(BlockID blockID)
        {
            return m_blocksToWrite.Contains(blockID.Value);
        }

        /// <param name="block">BlockID might be updated</param>
        public void UpdateBlock(Block block)
        {
            if (block.TotalLength > Block.MaximumLength)
            {
                throw new Exception("Invalid block length");
            }

            if (IsBlockPendingWrite(block))
            {
                // the block we wish to replace is already pending write
                // we just need to update the buffer:
                m_blockBuffer[block.BlockID.Value] = block;
            }
            else
            {
                // we need to mark the old block for freeing, and add the new block to the buffer
                DeleteBlock(block);
                bool isInternal = block.BlockID.Internal;
                block.BlockID = m_file.Header.AllocateNextBlockID();
                block.BlockID.Internal = isInternal;
                m_blockBuffer.Add(block.BlockID.Value, block);
                m_blocksToWrite.Add(block.BlockID.Value);
            }
        }

        /// <param name="block">will be assigned a new BlockID</param>
        public void AddBlock(Block block)
        {
            if (block.TotalLength > Block.MaximumLength)
            {
                throw new Exception("Invalid block length");
            }

            block.BlockID = m_file.Header.AllocateNextBlockID();
            block.BlockID.Internal = !(block is DataBlock);
            m_blockBuffer.Add(block.BlockID.Value, block);
            m_blocksToWrite.Add(block.BlockID.Value);
        }

        public void DeleteBlock(Block block)
        {
            ulong blockID = block.BlockID.Value;
            
            if (m_blockBuffer.ContainsKey(blockID))
            {
                // remove the old block from the cache
                m_blockBuffer.Remove(blockID);
            }

            // no need to free a block that has not been written yet
            if (IsBlockPendingWrite(block))
            {
                m_blocksToWrite.Remove(blockID);
            }
            else
            {
                m_blocksToFree.Add(blockID);
            }
        }

        /// <summary>
        /// The caller must update its reference to point to the new root
        /// </summary>
        public virtual void SaveChanges()
        {
            foreach (ulong blockID in m_blocksToWrite)
            {
                Block block = m_blockBuffer[blockID];
                long offset = AllocationHelper.AllocateSpaceForBlock(m_file, block.TotalLength);
                block.WriteToStream(m_file.BaseStream, offset);
                m_file.BlockBTree.InsertBlockEntry(block.BlockID, offset, block.DataLength);
            }

            foreach (ulong blockID in m_blocksToFree)
            {
                BlockBTreeEntry entry = m_file.FindBlockEntryByBlockID(blockID);
                entry.cRef--;
                // Any leaf BBT entry that points to a BID holds a reference count to it.
                if (entry.cRef == 1)
                {
                    // we can mark the allocation to be freed and delete the entry,
                    // We should not free the allocation until the BBT is committed.
                    m_file.MarkAllocationToBeFreed(entry.BREF.ib, Block.GetTotalBlockLength(entry.cb));
                    m_file.BlockBTree.DeleteBlockEntry(entry.BREF.bid);
                }
                else
                {
                    m_file.BlockBTree.UpdateBlockEntry(entry.BREF.bid, entry.cRef);
                }
            }
            
            m_blocksToWrite.Clear();
            m_blocksToFree.Clear();
        }

        public PSTFile File
        {
            get
            {
                return m_file;
            }
        }
    }
}
