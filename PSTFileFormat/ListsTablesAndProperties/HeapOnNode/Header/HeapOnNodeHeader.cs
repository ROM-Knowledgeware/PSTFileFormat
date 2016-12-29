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
    public class HeapOnNodeHeader // HNHDR, appear at the first data block
    {
        public const byte HeapOnNodeBlockSignature = 0xEC;

        public const int Length = 12;

        public ushort ibHnpm; // The byte offset to the HN page Map record
        public byte bSig;
        public OnHeapTypeName bClientSig;
        public HeapID hidUserRoot;
        public byte[] rgbFillLevel = new byte[8]; // 4 bytes, 8 entries

        public HeapOnNodeHeader()
        {
            bSig = HeapOnNodeBlockSignature; // heap signature
            hidUserRoot = HeapID.EmptyHeapID;
        }

        public HeapOnNodeHeader(byte[] buffer) : this(buffer, 0)
        { }

        public HeapOnNodeHeader(byte[] buffer, int offset)
        {
            ibHnpm = LittleEndianConverter.ToUInt16(buffer, offset + 0);
            bSig = ByteReader.ReadByte(buffer, offset + 2);
            bClientSig = (OnHeapTypeName)ByteReader.ReadByte(buffer, offset + 3);
            hidUserRoot = new HeapID(buffer, offset + 4);
            rgbFillLevel = HeapOnNodeHelper.ReadFillLevelMap(buffer, offset + 8, 8);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt16(buffer, offset + 0, ibHnpm);
            ByteWriter.WriteByte(buffer, offset + 2, bSig);
            ByteWriter.WriteByte(buffer, offset + 3, (byte)bClientSig);
            LittleEndianWriter.WriteUInt32(buffer, offset + 4, hidUserRoot.Value);
            HeapOnNodeHelper.WriteFillLevelMap(buffer, offset + 8, rgbFillLevel);
        }
    }
}
