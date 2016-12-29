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
    public class HeapOnNodeFirstBlockData : HeapOnNodeBlockData
    {
        public HeapOnNodeHeader HeapHeader;

        /// <summary>
        /// Create new first block
        /// </summary>
        public HeapOnNodeFirstBlockData()
        {
            HeapHeader = new HeapOnNodeHeader();
        }

        public HeapOnNodeFirstBlockData(byte[] buffer)
        {
            HeapHeader = new HeapOnNodeHeader(buffer, 0);
            PopulateHeapItems(buffer, HeapHeader.ibHnpm);
        }

        public override void WriteHeader(byte[] buffer, int offset)
        {
            HeapHeader.WriteBytes(buffer, offset);
        }

        public override int HeaderLength
        {
            get
            {
                return HeapOnNodeHeader.Length;   
            }
        }

        public override ushort ibHnpm
        {
            get 
            {
                return HeapHeader.ibHnpm;
            }
            set
            {
                HeapHeader.ibHnpm = value;
            }
        }
    }
}
