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
    public abstract class Block
    {
        public const int MaximumLength = 8192;

        public BlockTrailer BlockTrailer;
        
        public Block()
        {
            BlockTrailer = new BlockTrailer();
        }

        public Block(byte[] buffer)
        {
            BlockTrailer = BlockTrailer.ReadFromEndOfBuffer(buffer);
        }

        public abstract void WriteDataBytes(byte[] buffer, ref int offset);

        public byte[] GetBytes(ulong fileOffset)
        {
            byte[] buffer = new byte[this.TotalLength];
            int paddingLength = this.TotalLength - this.DataLength - BlockTrailer.Length;
            BlockTrailer.cb = (ushort)this.DataLength;
            int offset = 0x00;
            WriteDataBytes(buffer, ref offset);
            offset += paddingLength;
            BlockTrailer.WriteBytes(buffer, this.DataLength, offset, fileOffset);
            return buffer;
        }

        public static int GetTotalBlockLength(int dataLength)
        {
            // block is padded to 64-byte alignment
            int lengthWithTrailer = dataLength + BlockTrailer.Length;
            int length = (int)Math.Ceiling((double)lengthWithTrailer / 64) * 64;
            return length;
        }

        public static Block ReadFromStream(Stream stream, BlockRef blockRef, int dataLength, bCryptMethodName bCryptMethod)
        {
            long offset = (long)blockRef.ib;
            int totalLength = GetTotalBlockLength(dataLength);
            stream.Seek(offset, SeekOrigin.Begin);
            byte[] buffer = new byte[totalLength];
            stream.Read(buffer, 0, totalLength);

            BlockTrailer trailer = BlockTrailer.ReadFromEndOfBuffer(buffer);
            Block block;
            if (trailer.bid.Internal)
            {
                // XBlock or XXBlock
                byte btype = buffer[0];
                byte cLevel = buffer[1];
                if (btype == (byte)BlockType.XBlock && cLevel == 0x01)
                {
                    // XBLOCK
                    block = new XBlock(buffer);
                }
                else if (btype == (byte)BlockType.XXBlock && cLevel == 0x02)
                {
                    // XXBLOCK
                    block = new XXBlock(buffer);
                }
                else if (btype == (byte)BlockType.SLBLOCK && cLevel == 0x00)
                { 
                    // SLBLock
                    block = new SubnodeLeafBlock(buffer);
                }
                else if (btype == (byte)BlockType.SIBLOCK && cLevel == 0x01)
                {
                    // SIBLock
                    block = new SubnodeIntermediateBlock(buffer);
                }
                else
                {
                    throw new Exception("Internal block, but not XBLOCK, XXBlock, SLBLOCK or SIBLOCK");
                }
            }
            else
            {
                block = new DataBlock(buffer, bCryptMethod);
            }

            // See question 3 at:
            // http://social.msdn.microsoft.com/Forums/en-CA/os_binaryfile/thread/923f5964-4a89-4811-86c2-06a553c34510
            // However, so far all tests suggest that there should be no problem to use BlockID.Value for both arguments
            if (blockRef.bid.LookupValue != block.BlockID.LookupValue)
            {
                throw new InvalidBlockIDException();
            }

            if (dataLength != trailer.cb)
            {
                throw new Exception("Invalid block length");
            }

            uint crc = PSTCRCCalculation.ComputeCRC(buffer, dataLength);
            if (block.BlockTrailer.dwCRC != crc)
            {
                throw new InvalidChecksumException();
            }

            uint signature = BlockTrailer.ComputeSignature(blockRef.ib, blockRef.bid.Value);
            if (block.BlockTrailer.wSig != signature)
            {
                throw new InvalidChecksumException();
            }

            return block;
        }

        /// <summary>
        /// Block signature (wSig) will be set before writing
        /// </summary>
        public void WriteToStream(Stream stream, long offset)
        {
            byte[] blockBytes = GetBytes((ulong)offset);
            stream.Seek(offset, SeekOrigin.Begin);
            stream.Write(blockBytes, 0, blockBytes.Length);
        }

        public abstract Block Clone();

        public BlockID BlockID
        {
            get
            {
                return this.BlockTrailer.bid;
            }
            set
            {
                this.BlockTrailer.bid = value;
            }
        }

        // Raw data contained in the block (excluding trailer and alignment padding)
        public abstract int DataLength
        {
            get;
        }

        public int TotalLength
        {
            get
            {
                return GetTotalBlockLength(this.DataLength);
            }
        }
    }
}
