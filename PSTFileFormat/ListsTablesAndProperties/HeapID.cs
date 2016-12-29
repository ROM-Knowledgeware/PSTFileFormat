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
    public class HeapID // HID - 4 bytes
    {
        public const int MaximumHidIndex = 2047; // hidIndex is represented by 11 bytes
        public static readonly HeapID EmptyHeapID = new HeapID(0, 0); // The hid is set to zero if the item is empty

        public const int Length = 4;
        // An HID is a 4-byte value that identifies an item allocated from the heap.
        // hidType & hidIndex & hidBlockIndex together comprise the unique HID
        
        private uint m_heapID;

        public HeapID(uint heapID)
        {
            m_heapID = heapID;
        }

        public HeapID(byte[] buffer) : this(buffer, 0)
        {

        }

        public HeapID(byte[] buffer, int offset)
        {
            m_heapID = LittleEndianConverter.ToUInt32(buffer, offset);
        }

        public HeapID(ushort hidBlockIndex, ushort hidIndex)
        {
            if (hidIndex > MaximumHidIndex)
            {
                throw new ArgumentException("hidIndex is out of range, cannot allocate additional items");
            }
            m_heapID = ((byte)NodeTypeName.NID_TYPE_HID & 0x1F);
            m_heapID |= (uint)(hidIndex & 0x7FF) << 5;
            m_heapID |= (uint)(hidBlockIndex << 16);
        }

        public NodeTypeName hidType
        {
            get
            {
                return (NodeTypeName)(m_heapID & 0x1F);
            }
        }

        /// <summary>
        /// 1-based index value that identifies an item allocated from the heap node
        /// </summary>
        public ushort hidIndex
        {
            get
            {
                return (ushort)((m_heapID >> 5) & 0x7FF);
            }
        }

        /// <summary>
        /// indicates the zero-based index of the data block in which this heap item resides.
        /// </summary>
        public ushort hidBlockIndex
        {
            get
            {
                return (ushort)(m_heapID >> 16);
            }
        }

        public uint Value
        {
            get
            {
                return m_heapID;
            }
        }

        // The hid is set to zero if the item is empty
        public bool IsEmpty
        {
            get
            {
                return (m_heapID == 0);
            }
        }
    }
}
