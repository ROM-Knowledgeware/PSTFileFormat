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

namespace PSTFileFormat // TCINFO
{
    public class TableContextInfo
    {
        public const int TCI_4b = 0; // Ending offset of 8- and 4-byte data value group. (first)
        public const int TCI_2b = 1; // Ending offset of 2-byte data value group. (second)
        public const int TCI_1b = 2; // Ending offset of 1-byte data value group. (third)
        public const int TCI_bm = 3; // Ending offset of the Cell Existence Block. (last)

        public OnHeapTypeName bType;
        //private byte cCols;
        public ushort[] rgib = new ushort[4];
        public HeapID hidRowIndex;
        public HeapOrNodeID hnidRows; // NDID: HeapID or NodeID
        //public ushort hidIndex; // deprecated
        public List<TableColumnDescriptor> rgTCOLDESC = new List<TableColumnDescriptor>();

        public TableContextInfo()
        {
            bType = OnHeapTypeName.bTypeTC;
            hidRowIndex = HeapID.EmptyHeapID;
            hnidRows = new HeapOrNodeID(HeapID.EmptyHeapID);
        }

        public TableContextInfo(byte[] buffer)
        {
            bType = (OnHeapTypeName)ByteReader.ReadByte(buffer, 0);
            byte cCols = ByteReader.ReadByte(buffer, 1);
            int position = 2;
            for(int index = 0; index < 4; index++)
            {
                rgib[index] = LittleEndianConverter.ToUInt16(buffer, position);
                position += 2;
            }
            hidRowIndex = new HeapID(buffer, 10);
            hnidRows = new HeapOrNodeID(buffer, 14);
            // hidIndex - deprecated
            position = 22;
            for (int index = 0; index < cCols; index++)
            {
                TableColumnDescriptor descriptor = new TableColumnDescriptor(buffer, position);
                rgTCOLDESC.Add(descriptor);
                position += TableColumnDescriptor.Length;
            }
        }

        public byte[] GetBytes()
        {
            int length = 22 + rgTCOLDESC.Count * TableColumnDescriptor.Length;
            byte[] buffer = new byte[length];
            ByteWriter.WriteByte(buffer, 0, (byte)bType);
            ByteWriter.WriteByte(buffer, 1, (byte)rgTCOLDESC.Count);
            int position = 2;
            for (int index = 0; index < 4; index++)
            {
                LittleEndianWriter.WriteUInt16(buffer, position, rgib[index]);
                position += 2;
            }
            LittleEndianWriter.WriteUInt32(buffer, 10, hidRowIndex.Value);
            LittleEndianWriter.WriteUInt32(buffer, 14, hnidRows.Value);

            position = 22;
            // http://social.msdn.microsoft.com/Forums/en-US/os_binaryfile/thread/a5f9c653-40f5-4638-85d3-00c54607d984/
            // rgTCOLDESC must be sorted by Tag
            rgTCOLDESC.Sort(TableColumnDescriptor.Compare);

            foreach(TableColumnDescriptor descriptor in rgTCOLDESC)
            {
                descriptor.WriteBytes(buffer, position);
                position += TableColumnDescriptor.Length;
            }

            return buffer;
        }

        /// <summary>
        /// This will update rgid
        /// </summary>
        public void UpdateDataLayout()
        { 
            rgib[TCI_4b] = 0;
            rgib[TCI_2b] = 0;
            rgib[TCI_1b] = 0;
            rgib[TCI_bm] = (ushort)Math.Ceiling((double)rgTCOLDESC.Count / 8);
            foreach (TableColumnDescriptor descriptor in rgTCOLDESC)
            {
                rgib[descriptor.DataLengthGroup] += descriptor.cbData;
            }

            rgib[TCI_2b] += rgib[TCI_4b];
            rgib[TCI_1b] += rgib[TCI_2b];
            rgib[TCI_bm] += rgib[TCI_1b];
        }

        public int ColumnCount
        {
            get
            {
                return rgTCOLDESC.Count;
            }
        }

        public ushort CellExistenceBlockStartOffset
        {
            get
            {
                return rgib[TCI_1b];
            }
        }

        public ushort RowLength
        {
            get
            {
                return rgib[TCI_bm];
            }
        }
    }
}
