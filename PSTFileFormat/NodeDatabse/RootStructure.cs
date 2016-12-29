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
    public class RootStructure
    {
        public const int Length = 72;

        public const byte INVALID_AMAP = 0x00;
        public const byte VALID_AMAP1 = 0x01; // Outlook 2003
        public const byte VALID_AMAP2 = 0x02; // Outlook 2007+

        public ulong ibFileEOF;
        public ulong ibAMapLast;
        public ulong cbAMapFree;
        public ulong cbPMapFree;
        public BlockRef BREFNBT; // Node BTree
        public BlockRef BREFBBT; // Block BTree
        public byte fAMapValid;
        // Outlook 2003-2010 use these value for implementation-specific data.
        // Modification of these values can result in failure to read the PST file by Outlook
        public byte bReserved;
        public ushort wReserved;

        public RootStructure(byte[] buffer, int offset)
        {
            ibFileEOF = LittleEndianConverter.ToUInt64(buffer, offset + 4);
            ibAMapLast = LittleEndianConverter.ToUInt64(buffer, offset + 12);
            cbAMapFree = LittleEndianConverter.ToUInt64(buffer, offset + 20);
            cbPMapFree = LittleEndianConverter.ToUInt64(buffer, offset + 28);
            BREFNBT = new BlockRef(buffer, offset + 36);
            BREFBBT = new BlockRef(buffer, offset + 52);
            fAMapValid = ByteReader.ReadByte(buffer, offset + 68);
            bReserved = ByteReader.ReadByte(buffer, offset + 69);
            wReserved = LittleEndianConverter.ToUInt16(buffer, offset + 70);
        }

        public void WriteBytes(byte[] buffer, int offset, WriterCompatibilityMode writerCompatibilityMode)
        {
            if (fAMapValid == VALID_AMAP1 && writerCompatibilityMode >= WriterCompatibilityMode.Outlook2007RTM)
            {
                fAMapValid = VALID_AMAP2;
            }

            LittleEndianWriter.WriteUInt64(buffer, offset + 4, ibFileEOF);
            LittleEndianWriter.WriteUInt64(buffer, offset + 12, ibAMapLast);
            LittleEndianWriter.WriteUInt64(buffer, offset + 20, cbAMapFree);
            LittleEndianWriter.WriteUInt64(buffer, offset + 28, cbPMapFree);
            BREFNBT.WriteBytes(buffer, offset + 36);
            BREFBBT.WriteBytes(buffer, offset + 52);
            ByteWriter.WriteByte(buffer, offset + 68, fAMapValid);
            ByteWriter.WriteByte(buffer, offset + 69, bReserved);
            LittleEndianWriter.WriteUInt16(buffer, offset + 70, wReserved);
        }

        public int NumberOfAllocationMapPages
        {
            get
            {
                return (int)((ibAMapLast - AllocationMapPage.FirstPageOffset) / AllocationMapPage.MapppedLength) + 1;
            }
        }

        public bool IsAllocationMapValid
        {
            get
            {
                return (fAMapValid == VALID_AMAP1 || fAMapValid == VALID_AMAP2);
            }
            set
            {
                if (value)
                {
                    // We set it to VALID_AMAP2 during WriteBytes() if necessary
                    fAMapValid = VALID_AMAP1;
                }
                else
                {
                    fAMapValid = INVALID_AMAP;
                }
            }
        }
    }
}
