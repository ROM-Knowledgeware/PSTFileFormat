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
    public class HeapOnNodePageMap // HNPAGEMAP
    {
        // ushort cAlloc;
        // ushort cFree; // cFree denotes the number of freed items (items with the length of 0)
        private List<ushort> rgibAlloc = new List<ushort>(); // Start offset of each item ending with the end offset of last item

        public HeapOnNodePageMap()
        { }

        public HeapOnNodePageMap(byte[] buffer, int offset)
        {
            ushort cAlloc = LittleEndianConverter.ToUInt16(buffer, offset + 0);
            ushort cFree = LittleEndianConverter.ToUInt16(buffer, offset + 2);

            // cAlloc + 1 entries
            // last entry marks the offset of the next available slot
            int entryOffset = offset + 4;
            for(int index = 0; index <= cAlloc; index++)
            {
                ushort entry = LittleEndianConverter.ToUInt16(buffer, entryOffset);
                rgibAlloc.Add(entry);
                entryOffset += 2;
            }

#if (DEBUG)
            // make sure the number of cFree is correct
            int cFreeVerification = 0;
            for (int index = 1; index < rgibAlloc.Count; index++)
            {
                if (rgibAlloc[index] == rgibAlloc[index - 1])
                {
                    cFreeVerification++;
                }
            }

            if (cFreeVerification != cFree)
            {
                throw new Exception("Invalid cFree value");
            }
#endif
        }

        public int GetHeapItemStartOffset(int itemIndex)
        {
            return rgibAlloc[itemIndex];
        }

        public int GetHeapItemSize(int itemIndex)
        {
            return rgibAlloc[itemIndex + 1] - rgibAlloc[itemIndex + 0];
        }

        public static List<ushort> MapHeapItems(int startOffset, List<byte[]> heapItems)
        {
            List<ushort> result = new List<ushort>();
            ushort nextOffset = (ushort)startOffset;
            result.Add(nextOffset);
            // cAlloc + 1 entries
            // last entry marks the offset of the next available slot
            foreach (byte[] itemBytes in heapItems)
            {
                nextOffset += (ushort)itemBytes.Length;
                result.Add(nextOffset);
            }

            return result;
        }

        public static int CountFreedItems(List<byte[]> heapItems)
        {
            int cFree = 0;
            for (int index = 0; index < heapItems.Count; index++)
            {
                if (heapItems[index].Length == 0)
                {
                    cFree++;
                }
            }
            return cFree;
        }

        public static byte[] GetBytes(int firstItemOffset, List<byte[]> heapItems)
        {
            List<ushort> rgibAlloc = MapHeapItems(firstItemOffset, heapItems);
            ushort cFree = (ushort)CountFreedItems(heapItems);
            byte[] buffer = new byte[CalculateRecordLength(heapItems.Count)];
            LittleEndianWriter.WriteUInt16(buffer, 0, (ushort)heapItems.Count);
            LittleEndianWriter.WriteUInt16(buffer, 2, cFree);
            int entryOffset = 4;
            for (int index = 0; index < rgibAlloc.Count; index++)
            {
                LittleEndianWriter.WriteUInt16(buffer, entryOffset, rgibAlloc[index]);
                entryOffset += 2;
            }
                        
            return buffer;
        }

        public static void WriteBytes(byte[] buffer, int offset, int firstItemOffset, List<byte[]> heapItems)
        {
            byte[] bytes = GetBytes(firstItemOffset, heapItems);
            Array.Copy(bytes, 0, buffer, offset, bytes.Length);
        }

        /// <summary>
        /// Number of heap items
        /// </summary>
        public int ItemCount
        {
            get
            {
                return rgibAlloc.Count - 1;
            }
        }

        public static int CalculateRecordLength(int heapItemCount)
        {
            return 4 + (heapItemCount + 1) * 2;
        }
    }
}
