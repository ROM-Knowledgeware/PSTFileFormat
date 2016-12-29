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
    public class DataTree : BufferedBlockStore
    {
        private bCryptMethodName m_bCryptMethod;
        private Block m_rootBlock; // can be DataBlock, XBlock or XXBlock (or null [if we deleted the data tree])
        
        /// <summary>
        /// Create new data tree
        /// </summary>
        public DataTree(PSTFile file) : base(file)
        {
            m_bCryptMethod = file.Header.bCryptMethod;
            m_rootBlock = new DataBlock(m_bCryptMethod);
            AddBlock(m_rootBlock);
        }

        // Data tree root is either a single data block, or an XBlock / XXBlock
        public DataTree(PSTFile file, Block rootBlock) : base(file)
        {
            m_bCryptMethod = file.Header.bCryptMethod;
            m_rootBlock = rootBlock;
        }

        public bool IsDataBlockPendingWrite(int dataBlockIndex)
        {
            if (m_rootBlock is DataBlock)
            {
                return IsBlockPendingWrite(m_rootBlock);
            }
            else if (m_rootBlock is XBlock)
            {
                BlockID blockID = ((XBlock)m_rootBlock).rgbid[dataBlockIndex];
                return IsBlockPendingWrite(blockID);
            }
            else // XXBlock
            {
                int xBlockIndex = dataBlockIndex / XBlock.MaximumNumberOfDataBlocks;
                int dataBlockIndexInXBlock = dataBlockIndex % XBlock.MaximumNumberOfDataBlocks;

                XXBlock rootBlock = (XXBlock)m_rootBlock;
                XBlock xBlock = (XBlock)GetBlock(rootBlock.rgbid[xBlockIndex]);
                BlockID blockID = xBlock.rgbid[dataBlockIndexInXBlock];
                return IsBlockPendingWrite(blockID);
            }
        }

        public DataBlock GetDataBlock(int dataBlockIndex)
        {
            if (m_rootBlock == null)
            {
                throw new Exception("Data tree root block is null");
            }
            else if (m_rootBlock is DataBlock)
            {
                if (dataBlockIndex == 0)
                {
                    // We do not want to return the reference
                    return (DataBlock)m_rootBlock.Clone();
                }
                else
                {
                    throw new ArgumentException("Data tree root block is a data block, data block index must be 0");
                }
            }
            else if (m_rootBlock is XBlock)
            {
                XBlock rootBlock = (XBlock)m_rootBlock;
                if (dataBlockIndex < rootBlock.rgbid.Count)
                {
                    BlockID blockID = rootBlock.rgbid[dataBlockIndex];
                    DataBlock block = (DataBlock)GetBlock(blockID.Value);
                    return block;
                }
                else
                {
                    throw new ArgumentException("Invalid data block index");
                }
            }
            else // XXBlock
            {
                // We assume that all XBlocks are completely filled except the last one.
                int xBlockIndex = dataBlockIndex / XBlock.MaximumNumberOfDataBlocks;
                int dataBlockIndexInXBlock = dataBlockIndex % XBlock.MaximumNumberOfDataBlocks;

                XXBlock rootBlock = (XXBlock)m_rootBlock;
                if (xBlockIndex < rootBlock.NumberOfXBlocks)
                {
                    XBlock xBlock = (XBlock)GetBlock(rootBlock.rgbid[xBlockIndex]);
                    if (dataBlockIndexInXBlock < xBlock.NumberOfDataBlocks)
                    {
                        BlockID blockID = xBlock.rgbid[dataBlockIndexInXBlock];
                        DataBlock block = (DataBlock)GetBlock(blockID.Value);
                        return block;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid data block index");
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid XBlock index");
                }
            }
        }

        public void UpdateDataBlock(int dataBlockIndex, byte[] blockData)
        {
            if (m_rootBlock == null)
            {
                throw new Exception("Data tree root block is null");
            }
            else if (m_rootBlock is DataBlock)
            {
                if (dataBlockIndex == 0)
                {
                    ((DataBlock)m_rootBlock).Data = blockData;
                    UpdateBlock(m_rootBlock);
                }
                else
                {
                    throw new ArgumentException("Data tree root block is a data block, data block index must be 0");
                }
            }
            else if (m_rootBlock is XBlock)
            {
                XBlock rootBlock = (XBlock)m_rootBlock;
                if (dataBlockIndex < rootBlock.rgbid.Count)
                {
                    ulong currentBlockID = rootBlock.rgbid[dataBlockIndex].Value;
                    int currentDataLength = ((DataBlock)GetBlock(currentBlockID)).Data.Length;

                    DataBlock block = new DataBlock(m_bCryptMethod);
                    block.Data = blockData;
                    block.BlockID = new BlockID(currentBlockID);

                    UpdateBlock(block);

                    if (block.BlockID.Value != currentBlockID)
                    {
                        // A new block has been written instead of the old one,
                        // update the root block
                        rootBlock.rgbid[dataBlockIndex] = block.BlockID;
                    }
                    // Update the total length
                    uint totalLength = (uint)(rootBlock.lcbTotal + blockData.Length - currentDataLength);
                    rootBlock.lcbTotal = totalLength;
                    UpdateBlock(rootBlock);
                }
                else
                {
                    throw new ArgumentException("Invalid data block index");
                }
            }
            else // XXBlock
            {
                int xBlockIndex = dataBlockIndex / XBlock.MaximumNumberOfDataBlocks;
                int dataBlockIndexInXBlock = dataBlockIndex % XBlock.MaximumNumberOfDataBlocks;

                XXBlock rootBlock = (XXBlock)m_rootBlock;
                
                if (xBlockIndex < rootBlock.NumberOfXBlocks)
                {
                    XBlock xBlock = (XBlock)GetBlock(rootBlock.rgbid[xBlockIndex]);
                    if (dataBlockIndexInXBlock < xBlock.NumberOfDataBlocks)
                    {
                        ulong currentBlockID = xBlock.rgbid[dataBlockIndexInXBlock].Value;
                        int currentDataLength = ((DataBlock)GetBlock(currentBlockID)).Data.Length;

                        DataBlock block = new DataBlock(m_bCryptMethod);
                        block.Data = blockData;
                        block.BlockID = new BlockID(currentBlockID);

                        UpdateBlock(block);

                        if (block.BlockID.Value != currentBlockID)
                        {
                            // A new block has been written instead of the old one,
                            // update the root block
                            xBlock.rgbid[dataBlockIndexInXBlock] = block.BlockID;
                        }
                        // Update the total length
                        uint xBlockTotalLength = (uint)(xBlock.lcbTotal + blockData.Length - currentDataLength);
                        xBlock.lcbTotal = xBlockTotalLength;
                        
                        ulong currentXBlockID = rootBlock.rgbid[xBlockIndex].Value;
                        UpdateBlock(xBlock);
                        if (xBlock.BlockID.Value != currentXBlockID)
                        {
                            rootBlock.rgbid[xBlockIndex] = xBlock.BlockID;
                        }

                        uint totalLength = (uint)(rootBlock.lcbTotal + blockData.Length - currentDataLength);
                        rootBlock.lcbTotal = totalLength;
                        UpdateBlock(rootBlock);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid data block index");
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid XBlock index");
                }
            }
        }

        public void AddDataBlock(byte[] blockData)
        {
            // http://social.msdn.microsoft.com/Forums/en-US/os_binaryfile/thread/a5f9c653-40f5-4638-85d3-00c54607d984/
            // We must completely fill the previous block in order for Outlook 2003 to read the next block
            // This is true for both the Subnode containing the rows of a Table Context
            // and for the DataTree containing the heap items of a Table Context.

            // Emphasis: The block that must be filled is the one preceding the block that was added.
            if (DataBlockCount > 0)
            {
                ZeroFillBlock(DataBlockCount - 1);
            }

            DataBlock block = new DataBlock(m_bCryptMethod);
            block.Data = blockData;
            AddBlock(block);

            // update the root
            if (m_rootBlock == null)
            {
                m_rootBlock = block;
            }
            else if (m_rootBlock is DataBlock)
            {
                DataBlock firstDataBlock = (DataBlock)m_rootBlock;
                
                // Create an XBlock:
                XBlock rootBlock = new XBlock();
                rootBlock.rgbid.Add(firstDataBlock.BlockID);
                rootBlock.rgbid.Add(block.BlockID);
                rootBlock.lcbTotal = (uint)(firstDataBlock.Data.Length + blockData.Length);
                AddBlock(rootBlock);
                m_rootBlock = rootBlock;

                // Note: the reference from the node to the rootblock will be updated later
            }
            else if (m_rootBlock is XBlock)
            {
                XBlock rootBlock = (XBlock)m_rootBlock;
                if (rootBlock.NumberOfDataBlocks < XBlock.MaximumNumberOfDataBlocks)
                {
                    rootBlock.rgbid.Add(block.BlockID);
                    rootBlock.lcbTotal += (uint)blockData.Length;
                    UpdateBlock(rootBlock);
                }
                else // We need to create a new XXBlock
                {
                    // Create an XBlock:
                    XBlock xBlock = new XBlock();
                    xBlock.rgbid.Add(block.BlockID);
                    xBlock.lcbTotal = (uint)blockData.Length;
                    AddBlock(xBlock);

                    // Create an XXBlock
                    XXBlock newRootBlock = new XXBlock();
                    newRootBlock.rgbid.Add(rootBlock.BlockID);
                    newRootBlock.rgbid.Add(xBlock.BlockID);
                    newRootBlock.lcbTotal = rootBlock.lcbTotal + xBlock.lcbTotal;
                    AddBlock(newRootBlock);
                    m_rootBlock = newRootBlock;

                    // Note: the reference from the node to the rootblock will be updated later
                }
            }
            else // XXBlock
            {
                XXBlock rootBlock = (XXBlock)m_rootBlock;

                BlockID lastXBlockID = rootBlock.rgbid[rootBlock.NumberOfXBlocks - 1];
                XBlock lastXBlock = (XBlock)GetBlock(lastXBlockID);

                if (lastXBlock.NumberOfDataBlocks < XBlock.MaximumNumberOfDataBlocks)
                {
                    lastXBlock.rgbid.Add(block.BlockID);
                    lastXBlock.lcbTotal += (uint)blockData.Length;
                    UpdateBlock(lastXBlock);

                    rootBlock.lcbTotal += (uint)blockData.Length;
                    UpdateBlock(rootBlock);
                }
                else if (rootBlock.NumberOfXBlocks < XXBlock.MaximumNumberOfXBlocks)
                {
                    // Create an XBlock:
                    XBlock xBlock = new XBlock();
                    xBlock.rgbid.Add(block.BlockID);
                    xBlock.lcbTotal = (uint)blockData.Length;
                    AddBlock(xBlock);

                    rootBlock.rgbid.Add(xBlock.BlockID);
                    rootBlock.lcbTotal += (uint)blockData.Length;
                    UpdateBlock(rootBlock);
                }
                else
                {
                    throw new Exception("Data Tree is full");
                }
            }
        }

        public void DeleteLastDataBlock()
        {
            if (m_rootBlock == null)
            {
                return;
            }
            else if (m_rootBlock is DataBlock)
            {
                DeleteBlock(m_rootBlock);
                m_rootBlock = null;
            }
            else if (m_rootBlock is XBlock)
            {
                XBlock rootBlock = (XBlock)m_rootBlock;
                int dataBlockIndex = rootBlock.rgbid.Count - 1;
                ulong currentBlockID = rootBlock.rgbid[dataBlockIndex].Value;
                DataBlock dataBlock = (DataBlock)GetBlock(currentBlockID);
                int currentDataLength = dataBlock.Data.Length;

                DeleteBlock(dataBlock);

                rootBlock.rgbid.RemoveAt(dataBlockIndex);
                // Update the total length
                uint totalLength = (uint)(rootBlock.lcbTotal - currentDataLength);
                rootBlock.lcbTotal = totalLength;
                UpdateBlock(rootBlock);
            }
            else // XXBlock
            {
                XXBlock rootBlock = (XXBlock)m_rootBlock;

                BlockID lastXBlockID = rootBlock.rgbid[rootBlock.NumberOfXBlocks - 1];
                XBlock lastXBlock = (XBlock)GetBlock(lastXBlockID);
                if (lastXBlock.NumberOfDataBlocks > 1)
                {
                    int dataBlockIndexInXBlock = lastXBlock.NumberOfDataBlocks - 1;
                    ulong currentBlockID = lastXBlock.rgbid[dataBlockIndexInXBlock].Value;
                    DataBlock dataBlock = (DataBlock)GetBlock(currentBlockID);
                    int currentDataLength = dataBlock.Data.Length;
                    
                    DeleteBlock(dataBlock);

                    lastXBlock.rgbid.RemoveAt(dataBlockIndexInXBlock);
                    // Update the total length
                    uint xBlockTotalLength = (uint)(lastXBlock.lcbTotal - currentDataLength);
                    lastXBlock.lcbTotal = xBlockTotalLength;
                    UpdateBlock(lastXBlock);

                    uint totalLength = (uint)(rootBlock.lcbTotal - currentDataLength);
                    rootBlock.lcbTotal = totalLength;
                    UpdateBlock(rootBlock);
                }
                else if (lastXBlock.NumberOfDataBlocks == 1)
                {
                    ulong currentBlockID = lastXBlock.rgbid[0].Value;
                    DataBlock dataBlock = (DataBlock)GetBlock(currentBlockID);
                    int currentDataLength = dataBlock.Data.Length;

                    DeleteBlock(dataBlock);
                    DeleteBlock(lastXBlock);

                    int lastXBlockIndex = rootBlock.rgbid.Count - 1;
                    rootBlock.rgbid.RemoveAt(lastXBlockIndex);
                    rootBlock.lcbTotal = (uint)(rootBlock.lcbTotal - currentDataLength);
                    UpdateBlock(rootBlock);
                }
            }
        }

        public void ZeroFillBlock(int blockIndex)
        {
            DataBlock block = GetDataBlock(blockIndex);
            if (block.DataLength < DataBlock.MaximumDataLength)
            {
                byte[] temp = new byte[DataBlock.MaximumDataLength];
                Array.Copy(block.Data, temp, block.Data.Length);
                UpdateDataBlock(blockIndex, temp);
            }
        }

        public override void SaveChanges()
        {
            // A data block could have been modified (heap item was removed) and a block that was 
            // supposed to be zero-filled may have become non-zero filled.
            // We must make sure that all blocks that are pending write (except the last block) are zero filled
            for(int blockIndex = 0; blockIndex < DataBlockCount - 1; blockIndex++)
            {
                if (IsDataBlockPendingWrite(blockIndex))
                {
                    ZeroFillBlock(blockIndex);
                }
            }
            
            base.SaveChanges();
        }

        /// <summary>
        /// Return the bytes of the entire data tree
        /// </summary>
        public byte[] GetData()
        {
            byte[] result = new byte[TotalDataLength];
            int bytesCopied = 0;
            for (int blockIndex = 0; blockIndex < DataBlockCount; blockIndex++)
            {
                byte[] blockData = GetDataBlock(blockIndex).Data;
                Array.Copy(blockData, 0, result, bytesCopied, blockData.Length);
                bytesCopied += blockData.Length;
            }
            return result;
        }

        public void Delete()
        {
            int dataBlockCount = DataBlockCount;
            while (dataBlockCount > 0)
            {
                DeleteLastDataBlock();
                dataBlockCount--;
            }

            // m_rootBlock could be null at this point
            if (m_rootBlock is XBlock || m_rootBlock is XXBlock)
            {
                DeleteBlock(m_rootBlock);
                m_rootBlock = null;
            }
            base.SaveChanges();
        }

        public void AppendDataToBlock(int blockIndex, byte[] data)
        { 
            DataBlock block = GetDataBlock(blockIndex);
            byte[] temp = new byte[block.DataLength + data.Length];
            Array.Copy(block.Data, temp, block.DataLength);
            Array.Copy(data, 0, temp, block.DataLength, data.Length);
            block.Data = temp;
            UpdateDataBlock(blockIndex, block.Data);
        }

        /// <summary>
        /// Write data across multiple blocks (if necessary)
        /// </summary>
        public void AppendData(byte[] data)
        {
            int offset = 0;

            if (this.DataBlockCount > 0)
            {
                // handle first block
                int blockIndex = this.DataBlockCount - 1;
                DataBlock block = GetDataBlock(blockIndex);
                int availableLength = DataBlock.MaximumDataLength - block.DataLength;
                if (availableLength > 0)
                {
                    int currentWriteLength = Math.Min(availableLength, data.Length);
                    byte[] buffer = new byte[currentWriteLength];
                    Array.Copy(data, buffer, currentWriteLength);
                    AppendDataToBlock(blockIndex, buffer);
                    offset += currentWriteLength;
                }
            }

            while (offset < data.Length)
            {
                int leftToWrite = data.Length - offset;
                int currentBlockLength = Math.Min(leftToWrite, DataBlock.MaximumDataLength);
                byte[] buffer = new byte[currentBlockLength];
                Array.Copy(data, offset, buffer, 0, buffer.Length);
                AddDataBlock(buffer);
                offset += currentBlockLength;
            }
        }

        public void Clear()
        {
            // delete extra blocks
            for (int index = 0; index < DataBlockCount; index++)
            {
                DeleteLastDataBlock();
            }
        }

        public Block RootBlock
        {
            get
            {
                return m_rootBlock;
            }
        }

        public int DataBlockCount
        {
            get
            {
                if (m_rootBlock == null)
                {
                    return 0;
                }
                else if (m_rootBlock is DataBlock)
                {
                    return 1;
                }
                else if (m_rootBlock is XBlock)
                {
                    return ((XBlock)m_rootBlock).NumberOfDataBlocks;
                }
                else // XXBlock
                {
                    XXBlock rootBlock = (XXBlock)m_rootBlock;
                    BlockID lastXBlockID = rootBlock.rgbid[rootBlock.NumberOfXBlocks - 1];
                    XBlock lastXBlock = (XBlock)GetBlock(lastXBlockID);

                    return (rootBlock.NumberOfXBlocks - 1) * XBlock.MaximumNumberOfDataBlocks + lastXBlock.NumberOfDataBlocks;
                }
            }
        }

        public int TotalDataLength
        {
            get
            {
                if (m_rootBlock == null)
                {
                    return 0;
                }
                else if (m_rootBlock is DataBlock)
                {
                    return ((DataBlock)m_rootBlock).Data.Length;
                }
                else if (m_rootBlock is XBlock)
                {
                    return (int)((XBlock)m_rootBlock).lcbTotal;
                }
                else // XXBlock
                {
                    return (int)((XXBlock)m_rootBlock).lcbTotal;
                }
            }
        }
    }
}
