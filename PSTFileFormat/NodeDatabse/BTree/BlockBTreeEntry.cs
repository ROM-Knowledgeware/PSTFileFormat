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
    public class BlockBTreeEntry // BBTENTRY (Leaf BBT Entry)
    {
        public BlockRef BREF;
        public ushort cb;   // The count of bytes of the raw data contained in the block (excluding the block trailer and alignment padding)
        public ushort cRef; // Reference count: indicating the count of references to this block.
        //public uint dwPadding

        public BlockBTreeEntry()
        {
            BREF = new BlockRef();
        }

        public BlockBTreeEntry(byte[] buffer, int offset)
        {
            BREF = new BlockRef(buffer, offset + 0);
            cb = LittleEndianConverter.ToUInt16(buffer, offset + 16);
            cRef = LittleEndianConverter.ToUInt16(buffer, offset + 18);
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[24];
            WriteBytes(buffer, 0);
            return buffer;
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            BREF.WriteBytes(buffer, offset + 0);
            LittleEndianWriter.WriteUInt16(buffer, offset + 16, cb);
            LittleEndianWriter.WriteUInt16(buffer, offset + 18, cRef);
        }
    }
}
