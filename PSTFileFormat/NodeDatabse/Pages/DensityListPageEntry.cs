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
    public class DensityListPageEntry // DLISTPAGEENT
    {
        public const int Length = 4;

        public uint dwPageNum; // 20 bits
        public ushort dwFreeSlots; // 12 bits

        public DensityListPageEntry(byte[] buffer, int offset)
        {
            uint temp = LittleEndianConverter.ToUInt32(buffer, offset);
            dwPageNum = temp & 0xFFFFF;
            dwFreeSlots = (ushort)(temp >> 20);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            uint temp = dwPageNum & 0xFFFFF;
            temp |= ((dwFreeSlots & 0x3FFU) << 20);
            LittleEndianWriter.WriteUInt32(buffer, offset, temp);
        }
    }
}
