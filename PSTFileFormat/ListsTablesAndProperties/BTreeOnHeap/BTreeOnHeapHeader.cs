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
    public class BTreeOnHeapHeader // BTHHEADER
    {
        public int Length = 8;

        public OnHeapTypeName bType;
        public byte cbKey;      // Size of the BTree Key value, in bytes
        public byte cbEnt;      // Size of the data value, in bytes
        public byte bIdxLevels; // Index depth, 0 means the BTH root is a leaf (with data records) or an empty BTH.
        public HeapID hidRoot;    // This is the HID that points to the BTH root for this BTHHEADER.

        public BTreeOnHeapHeader()
        {
            bType = OnHeapTypeName.bTypeBTH;
            hidRoot = HeapID.EmptyHeapID; // Set to 0 if the BTH is empty
        }

        public BTreeOnHeapHeader(byte[] buffer) : this(buffer, 0)
        { }

        public BTreeOnHeapHeader(byte[] buffer, int offset)
        {
            bType = (OnHeapTypeName)ByteReader.ReadByte(buffer, offset + 0);
            cbKey = ByteReader.ReadByte(buffer, offset + 1);
            cbEnt = ByteReader.ReadByte(buffer, offset + 2);
            bIdxLevels = ByteReader.ReadByte(buffer, offset + 3);
            hidRoot = new HeapID(buffer, offset + 4);
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[Length];
            ByteWriter.WriteByte(buffer, 0, (byte)bType);
            ByteWriter.WriteByte(buffer, 1, cbKey);
            ByteWriter.WriteByte(buffer, 2, cbEnt);
            ByteWriter.WriteByte(buffer, 3, bIdxLevels);
            LittleEndianWriter.WriteUInt32(buffer, 4, hidRoot.Value);
            return buffer;
        }
    }
}
