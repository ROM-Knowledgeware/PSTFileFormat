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
    public class HeapOnNodePageBlockData : HeapOnNodeBlockData
    {
        public HeapOnNodePageHeader PageHeader;

        public HeapOnNodePageBlockData()
        {
            PageHeader = new HeapOnNodePageHeader();
        }

        public HeapOnNodePageBlockData(byte[] buffer)
        {
            PageHeader = new HeapOnNodePageHeader(buffer);
            PopulateHeapItems(buffer, PageHeader.ibHnpm);
        }

        public override void WriteHeader(byte[] buffer, int offset)
        {
            PageHeader.WriteBytes(buffer, offset);
        }

        public override int HeaderLength
        {
            get 
            {
                return HeapOnNodePageHeader.Length;
            }
        }

        public override ushort ibHnpm
        {
            get 
            {
                return PageHeader.ibHnpm;
            }
            set
            {
                PageHeader.ibHnpm = value;
            }
        }
    }
}
