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
    public class BlockTrailer
    {
        public const int Length = 16;

        public ushort cb;
        public ushort wSig;
        public uint dwCRC;
        public BlockID bid;

        public BlockTrailer()
        { 
        }

        public BlockTrailer(byte[] buffer, int offset)
        {
            cb = LittleEndianConverter.ToUInt16(buffer, offset + 0);
            wSig = LittleEndianConverter.ToUInt16(buffer, offset + 2);
            dwCRC = LittleEndianConverter.ToUInt32(buffer, offset + 4);
            bid = new BlockID(buffer, offset + 8);
        }

        public void WriteBytes(byte[] buffer, int dataLength, int offset, ulong fileOffset)
        {
            wSig = ComputeSignature(fileOffset, bid.Value);
            // CRC is only calculated on the raw data (i.e. excluding BlockTrailer and padding)
            dwCRC = PSTCRCCalculation.ComputeCRC(buffer, dataLength);

            LittleEndianWriter.WriteUInt16(buffer, offset + 0, cb);
            LittleEndianWriter.WriteUInt16(buffer, offset + 2, wSig);
            LittleEndianWriter.WriteUInt32(buffer, offset + 4, dwCRC);
            LittleEndianWriter.WriteUInt64(buffer, offset + 8, bid.Value);
        }

        public static BlockTrailer ReadFromEndOfBuffer(byte[] buffer)
        {
            return new BlockTrailer(buffer, buffer.Length - 16);
        }

        public static ushort ComputeSignature(ulong ib, ulong bid)
        {
            ib ^= bid;
            return ((ushort)((ushort)(ib >> 16) ^ (ushort)(ib)));
        }
    }
}
