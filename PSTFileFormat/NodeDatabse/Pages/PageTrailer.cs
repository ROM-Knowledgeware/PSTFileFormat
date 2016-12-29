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
    public class PageTrailer // PAGETRAILER
    {
        public const int Length = 16;
        public const int OffsetFromPageStart = 496;

        public PageTypeName ptype;
        //public PageTypeName ptypeRepeat;
        public ushort wSig;
        public uint dwCRC;
        public BlockID bid;

        public PageTrailer()
        { 
        }

        public PageTrailer(byte[] buffer, int offset)
        {
            ptype = (PageTypeName)buffer[offset + 0];
            PageTypeName ptypeRepeat = (PageTypeName)buffer[offset + 1];
            wSig = LittleEndianConverter.ToUInt16(buffer, offset + 2);
            dwCRC = LittleEndianConverter.ToUInt32(buffer, offset + 4);
            bid = new BlockID(buffer, offset + 8);
        }

        public void WriteToPage(byte[] buffer, ulong fileOffset)
        {
            if (ptype == PageTypeName.ptypeBBT ||
                ptype == PageTypeName.ptypeNBT ||
                ptype == PageTypeName.ptypeDL)
            {
                wSig = BlockTrailer.ComputeSignature(fileOffset, bid.Value);
            }
            else
            {
                wSig = 0;
                // AMap, PMap, FMap, and FPMap pages have a special convention where their BID is assigned the same value as their IB
                bid = new BlockID(fileOffset);
            }

            
            dwCRC = PSTCRCCalculation.ComputeCRC(buffer, OffsetFromPageStart);

            int offset = OffsetFromPageStart;
            buffer[offset + 0] = (byte)ptype;
            buffer[offset + 1] = (byte)ptype;
            LittleEndianWriter.WriteUInt16(buffer, offset + 2, wSig);
            LittleEndianWriter.WriteUInt32(buffer, offset + 4, dwCRC);
            LittleEndianWriter.WriteUInt64(buffer, offset + 8, bid.Value);
        }

        public static PageTrailer ReadFromPage(byte[] buffer)
        {
            return new PageTrailer(buffer, OffsetFromPageStart);
        }
    }
}
