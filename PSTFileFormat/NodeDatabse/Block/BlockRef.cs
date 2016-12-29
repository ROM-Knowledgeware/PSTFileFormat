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
    public class BlockRef // BREF
    {
        public const int Length = 16;

        public BlockID bid;
        public ulong ib; // byte offset from beginning of the file

        public BlockRef()
        {
        }

        public BlockRef(byte[] buffer, int offset)
        {
            bid = new BlockID(buffer, offset + 0);
            ib = LittleEndianConverter.ToUInt64(buffer, offset + 8);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            bid.WriteBytes(buffer, offset + 0);
            LittleEndianWriter.WriteUInt64(buffer, offset + 8, ib);
        }
    }
}
