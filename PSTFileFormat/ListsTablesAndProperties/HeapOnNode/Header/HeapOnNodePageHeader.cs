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
    public class HeapOnNodePageHeader // HNPAGEHDR
    {
        public const int Length = 2;

        public ushort ibHnpm;

        public HeapOnNodePageHeader()
        { 
        }

        public HeapOnNodePageHeader(byte[] buffer) : this(buffer, 0)
        {
        }

        public HeapOnNodePageHeader(byte[] buffer, int offset)
        {
            ibHnpm = LittleEndianConverter.ToUInt16(buffer, offset + 0);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt16(buffer, offset + 0, ibHnpm);
        }
    }
}
