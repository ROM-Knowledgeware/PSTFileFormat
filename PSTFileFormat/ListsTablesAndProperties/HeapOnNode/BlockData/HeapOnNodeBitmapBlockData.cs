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
    public class HeapOnNodeBitmapBlockData : HeapOnNodeBlockData
    {
        public HeapOnNodeBitmapHeader BitmapHeader;

        public HeapOnNodeBitmapBlockData()
        {
            BitmapHeader = new HeapOnNodeBitmapHeader();
        }

        public HeapOnNodeBitmapBlockData(byte[] buffer)
        {
            BitmapHeader = new HeapOnNodeBitmapHeader(buffer);
            PopulateHeapItems(buffer, BitmapHeader.ibHnpm);
        }

        public override void WriteHeader(byte[] buffer, int offset)
        {
            BitmapHeader.WriteBytes(buffer, offset);
        }

        public override int HeaderLength
        {
            get 
            {
                return HeapOnNodeBitmapHeader.Length;
            }
        }

        public override ushort ibHnpm
        {
            get 
            {
                return BitmapHeader.ibHnpm; 
            }
            set
            {
                BitmapHeader.ibHnpm = value;
            }
        }
    }
}
