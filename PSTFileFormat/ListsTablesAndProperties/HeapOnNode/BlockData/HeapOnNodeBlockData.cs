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
    public abstract class HeapOnNodeBlockData // Refers to the decoded data within the data block
    {
        private HeapOnNodePageMap m_pageMap;
        public List<byte[]> HeapItems = new List<byte[]>();

        public abstract void WriteHeader(byte[] buffer, int offset);

        public void PopulateHeapItems(byte[] buffer, ushort ibHnpm)
        {
            m_pageMap = new HeapOnNodePageMap(buffer, ibHnpm);
            
            int itemCount = m_pageMap.ItemCount;
            for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                int itemOffset = m_pageMap.GetHeapItemStartOffset(itemIndex);
                int itemSize = m_pageMap.GetHeapItemSize(itemIndex);
                byte[] itemBytes = new byte[itemSize];
                Array.Copy(buffer, itemOffset, itemBytes, 0, itemSize);
                HeapItems.Add(itemBytes);
            }
        }

        public byte[] GetBytes()
        {
            int headerLength = this.HeaderLength;
            int dataLength = this.DataLength;
            // http://social.msdn.microsoft.com/Forums/en-US/os_binaryfile/thread/a5f9c653-40f5-4638-85d3-00c54607d984/
            // Padding to align to 2 byte boundary must be appended
            int paddingLength = (int)Math.Ceiling((double)(headerLength + dataLength) / 2) * 2 - (headerLength + dataLength);
            
            int firstItemOffset = headerLength;
            this.ibHnpm = (ushort)(headerLength + dataLength + paddingLength);
            int length = headerLength + dataLength + paddingLength + HeapOnNodePageMap.CalculateRecordLength(this.HeapItems.Count);
            
            byte[] buffer = new byte[length];
            WriteHeader(buffer, 0);
            WriteHeapItems(buffer, firstItemOffset);
            HeapOnNodePageMap.WriteBytes(buffer, ibHnpm, firstItemOffset, HeapItems);
            return buffer;
        }

        public void WriteHeapItems(byte[] buffer, int offset)
        {
            foreach(byte[] itemBytes in HeapItems)
            {
                ByteWriter.WriteBytes(buffer, offset, itemBytes);
                offset += itemBytes.Length;
            }
        }

        public abstract int HeaderLength
        {
            get;
        }

        public abstract ushort ibHnpm
        {
            get;
            set;
        }

        public int DataLength
        {
            get
            {
                int result = 0;
                for (int index = 0; index < HeapItems.Count; index++)
                {
                    result += HeapItems[index].Length;
                }
                return result;
            }
        }

        public int AvailableSpace
        {
            get
            {
                return DataBlock.MaximumDataLength - this.HeaderLength - HeapOnNodePageMap.CalculateRecordLength(HeapItems.Count) - this.DataLength;
            }
        }
    }
}
