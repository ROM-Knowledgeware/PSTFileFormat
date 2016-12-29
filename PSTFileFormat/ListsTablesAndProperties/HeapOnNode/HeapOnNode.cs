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
    public class HeapOnNode // HN
    {
        public const int MaximumAllocationLength = 3580;
        
        private DataTree m_dataTree;

        private Dictionary<int, HeapOnNodeBlockData> m_buffer = new Dictionary<int,HeapOnNodeBlockData>();
        private List<int> m_blocksToUpdate = new List<int>();

        public HeapOnNode(DataTree dataTree)
        {
            m_dataTree = dataTree;
        }

        private HeapOnNodeBlockData GetBlockData(int blockIndex)
        {
            if (!m_buffer.ContainsKey(blockIndex))
            {
                HeapOnNodeBlockData blockData = GetBlockDataUnbuffered(blockIndex);
                m_buffer.Add(blockIndex, blockData);
                return blockData;
            }
            else
            {
                return m_buffer[blockIndex];
            }
        }

        private void UpdateBuffer(int blockIndex, HeapOnNodeBlockData blockData)
        {
            m_buffer[blockIndex] = blockData;
            if (!m_blocksToUpdate.Contains(blockIndex))
            {
                m_blocksToUpdate.Add(blockIndex);
            }
        }

        private HeapOnNodeBlockData GetBlockDataUnbuffered(int blockIndex)
        {
            DataBlock block = DataTree.GetDataBlock(blockIndex);
            if (blockIndex == 0)
            {
                HeapOnNodeFirstBlockData heapFirstBlock = new HeapOnNodeFirstBlockData(block.Data);
                return heapFirstBlock;
            }
            else
            {
                // HNBITMAPHDR appears at data block 8 (zero-based) and thereafter every 128 blocks.
                // (that is, data block 8, data block 136, data block 264, and so on).
                if (blockIndex % 128 == 8)
                {
                    HeapOnNodeBitmapBlockData bitmapBlock = new HeapOnNodeBitmapBlockData(block.Data);
                    return bitmapBlock;
                }
                else
                {
                    HeapOnNodePageBlockData pageBlock = new HeapOnNodePageBlockData(block.Data);
                    return pageBlock;
                }
            }
        }

        public byte[] GetHeapItem(HeapID hid)
        {
            HeapOnNodeBlockData blockData = GetBlockData(hid.hidBlockIndex);
            // hidIndex is one-based
            if (hid.hidIndex == 0)
            {
                throw new ArgumentOutOfRangeException("hidIndex", "PST is corrupted, hidIndex cannot be 0");
            }
            else if (hid.hidIndex - 1 < blockData.HeapItems.Count)
            {
                return blockData.HeapItems[hid.hidIndex - 1];
            }
            else
            {
                throw new ArgumentOutOfRangeException("hidIndex", "PST is corrupted, hidIndex is out of range");
            }
        }

        public HeapID AddItemToHeap(byte[] itemBytes)
        {
            if (itemBytes.Length > MaximumAllocationLength)
            {
                throw new ArgumentException("Maximum size of a heap allocation is 3580 bytes");
            }

            if (itemBytes.Length == 0)
            {
                return HeapID.EmptyHeapID;
            }
            
            // We can use the Header / BitmapHeader to locate available space,
            // but the final authority is HNPAGEMAP located at the end of each block
            // Note: we are only required to maintain HNPAGEMAP
            for (int blockIndex = 0; blockIndex < m_dataTree.DataBlockCount; blockIndex ++)
            {
                HeapOnNodeBlockData blockData = GetBlockData(blockIndex);
                // We need space for the item itself and for a place for it in the page map (2 bytes)
                // We also have to make sure we do not allocate more items then can be represented by hidIndex
                if (blockData.AvailableSpace > itemBytes.Length + 2 && blockData.HeapItems.Count < HeapID.MaximumHidIndex)
                {
                    blockData.HeapItems.Add(itemBytes);
                    UpdateBuffer(blockIndex, blockData);

                    // hidIndex is one-based
                    ushort hidIndex = (ushort)(blockData.HeapItems.Count);
                    return new HeapID((ushort)blockIndex, hidIndex);
                }
            }

            // no space found in existing blocks, we need to allocate new data block
            int newBlockIndex = m_dataTree.DataBlockCount;
            HeapOnNodeBlockData newBlockData;
            if (newBlockIndex % 128 == 8)
            {
                newBlockData = new HeapOnNodeBitmapBlockData();
            }
            else
            {
                newBlockData = new HeapOnNodePageBlockData();
            }
            newBlockData.HeapItems.Add(itemBytes);
            this.DataTree.AddDataBlock(newBlockData.GetBytes());
            // hidIndex is one-based
            return new HeapID((ushort)newBlockIndex, 1);
        }

        public void RemoveItemFromHeap(HeapID heapID)
        {
            int blockIndex = heapID.hidBlockIndex;
            HeapOnNodeBlockData blockData = GetBlockData(blockIndex);
            // We can't remove the HeapItem, because then the HeapID of the subsequent items will be incorrect
            // So instead we put an empty item instead of the item we wish to remove.
            // (We must not forget to update the HNPAGEMAP's cFree, otherwise outlook will report that the pst is corrupt)

            // hidIndex is one-based
            blockData.HeapItems[heapID.hidIndex - 1] = new byte[0];
            CompactBlockData(blockData);
            UpdateBuffer(blockIndex, blockData);
        }

        /// <summary>
        /// Replace item in-place, to avoid issues, item should not be larger than the old item
        /// </summary>
        /// <returns>true if success</returns>
        private bool TryReplacingHeapItemInPlace(HeapID heapID, byte[] itemBytes)
        {
            int blockIndex = heapID.hidBlockIndex;
            HeapOnNodeBlockData blockData = GetBlockData(blockIndex);
            // hidIndex is one-based
            int oldSize = blockData.HeapItems[heapID.hidIndex - 1].Length;
            int newSize = itemBytes.Length;
            if (newSize - oldSize <=  blockData.AvailableSpace)
            {
                blockData.HeapItems[heapID.hidIndex - 1] = itemBytes;
                UpdateBuffer(blockIndex, blockData);
                return true;
            }
            else
            {
                return false;
            }
        }

        public HeapID ReplaceHeapItem(HeapID heapID, byte[] itemBytes)
        {
            bool success = TryReplacingHeapItemInPlace(heapID, itemBytes);
            if (!success)
            {
                // no room for the replacement item in the current block
                RemoveItemFromHeap(heapID);
                return AddItemToHeap(itemBytes);
            }
            else
            {
                return heapID;
            }
        }

        public virtual void SaveChanges()
        {
            FlushToDataTree();
            m_dataTree.SaveChanges();
        }

        public void FlushToDataTree()
        {
            UpdateFillLevelMap();

            foreach (int blockIndex in m_blocksToUpdate)
            {
                HeapOnNodeBlockData blockData = m_buffer[blockIndex];
                m_dataTree.UpdateDataBlock(blockIndex, blockData.GetBytes());
            }

            m_blocksToUpdate.Clear();
        }

        /// <summary>
        /// We trim freed items (items at the end of the block that can be removed without corrupting HeapIDs)
        /// (so that new items could be added in their place)
        /// </summary>
        /// <param name="blockData"></param>
        private void CompactBlockData(HeapOnNodeBlockData blockData)
        {
            for (int itemIndex = blockData.HeapItems.Count - 1; itemIndex >= 0; itemIndex--)
            {
                if (blockData.HeapItems[itemIndex].Length == 0)
                {
                    blockData.HeapItems.RemoveAt(itemIndex);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// http://social.msdn.microsoft.com/Forums/en-US/os_binaryfile/thread/a5f9c653-40f5-4638-85d3-00c54607d984/
        /// Outlook 2003 MUST have a valid rgbFillLevel for write operations
        /// </summary>
        private void UpdateFillLevelMap()
        {
            int blocksToProcess = Math.Min(m_dataTree.DataBlockCount, 8);
            HeapOnNodeHeader header = this.HeapHeader;
            for (int blockIndex = 0; blockIndex < blocksToProcess; blockIndex++)
            {
                if (m_blocksToUpdate.Contains(blockIndex))
                {
                    HeapOnNodeBlockData blockData = GetBlockData(blockIndex);
                    byte fillLevel = HeapOnNodeHelper.GetBlockFillLevel(blockData);
                    header.rgbFillLevel[blockIndex] = fillLevel;
                }
            }
            UpdateHeapHeader(header);

            for (int blockIndex = 8; blockIndex < m_dataTree.DataBlockCount; blockIndex += 128)
            {
                int blocksLeft = m_dataTree.DataBlockCount - blockIndex;
                blocksToProcess = Math.Min(blocksLeft, 128);
                HeapOnNodeBitmapHeader bitmapHeader = GetBitmapHeader(blockIndex);
                for (int blockOffset = 0; blockOffset < blocksToProcess; blockOffset++)
                {
                    if (m_blocksToUpdate.Contains(blockIndex + blockOffset))
                    {
                        HeapOnNodeBlockData blockData = GetBlockData(blockIndex + blockOffset);
                        byte fillLevel = HeapOnNodeHelper.GetBlockFillLevel(blockData);
                        bitmapHeader.rgbFillLevel[blockOffset] = fillLevel;
                    }
                }
                UpdateBitmapHeader(blockIndex, bitmapHeader);

                blocksLeft -= blocksToProcess;
            }
        }

        public HeapOnNodeBitmapHeader GetBitmapHeader(int blockIndex)
        {
            if (blockIndex % 128 == 8)
            {
                HeapOnNodeBitmapBlockData bitmapBlockData = (HeapOnNodeBitmapBlockData)GetBlockData(blockIndex);
                return bitmapBlockData.BitmapHeader;
            }
            else
            {
                throw new Exception("Invalid block index");
            }
        }

        public void UpdateHeapHeader(HeapOnNodeHeader header)
        {
            HeapOnNodeFirstBlockData blockData = (HeapOnNodeFirstBlockData)GetBlockData(0);
            blockData.HeapHeader = header;
            UpdateBuffer(0, blockData);
        }

        public void UpdateBitmapHeader(int blockIndex, HeapOnNodeBitmapHeader header)
        {
            HeapOnNodeBitmapBlockData blockData = (HeapOnNodeBitmapBlockData)GetBlockData(blockIndex);
            // header is fixed length, so we won't overwrite anything but the old header
            blockData.BitmapHeader = header;
            UpdateBuffer(blockIndex, blockData);
        }

        public int GetItemCount(int blockIndex)
        {
            HeapOnNodeBlockData blockData = GetBlockData(blockIndex);
            return blockData.HeapItems.Count;
        }

        /// <summary>
        /// For discovery purposes
        /// </summary>
        public int GetBlockibHnpm(int blockIndex)
        {
            HeapOnNodeBlockData blockData = GetBlockData(blockIndex);
            return blockData.ibHnpm;
        }

        public HeapOnNodeHeader HeapHeader
        {
            get
            {
                HeapOnNodeFirstBlockData firstBlockData = (HeapOnNodeFirstBlockData)GetBlockData(0);
                return firstBlockData.HeapHeader;
            }
        }

        public DataTree DataTree
        {
            get
            {
                return m_dataTree;
            }
        }

        public PSTFile File
        {
            get
            {
                return m_dataTree.File;
            }
        }

        public static HeapOnNode CreateNewHeap(PSTFile file)
        {
            DataTree dataTree = new DataTree(file);
            HeapOnNodeFirstBlockData blockData = new HeapOnNodeFirstBlockData();
            dataTree.UpdateDataBlock(0, blockData.GetBytes());
            // now the data tree contains a valid HN
            HeapOnNode heap = new HeapOnNode(dataTree);
            return heap;
        }
    }
}
