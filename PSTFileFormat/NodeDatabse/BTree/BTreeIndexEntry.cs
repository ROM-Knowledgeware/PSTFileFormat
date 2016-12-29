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
    public class BTreeIndexEntry // BTENTRY (Intermediate Entries)
    {
        public const int Length = 24;

        public ulong btkey;   // All the entries in the child BTPAGE referenced by BREF have key values greater than or equal to this key value.
        public BlockRef BREF;

        public BTreeIndexEntry()
        {
            BREF = new BlockRef();
        }

        public BTreeIndexEntry(byte[] buffer, int offset)
        {
            btkey = LittleEndianConverter.ToUInt64(buffer, offset + 0);
            BREF = new BlockRef(buffer, offset + 8);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt64(buffer, offset + 0, btkey);
            BREF.WriteBytes(buffer, offset + 8);
        }
    }
}
