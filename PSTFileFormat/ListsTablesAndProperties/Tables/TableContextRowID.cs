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
    public class TableContextRowID : BTreeOnHeapDataRecord // TCROWID (BTH leaf record)
    {
        public const int RecordKeyLength = 4;
        public const int RecordDataLength = 4;
        public const int Length = 8;

        public uint dwRowID;
        public uint dwRowIndex;

        public TableContextRowID()
        { 

        }

        public TableContextRowID(uint rowID, uint rowIndex)
        {
            dwRowID = rowID;
            dwRowIndex = rowIndex;
        }

        public TableContextRowID(byte[] buffer, int offset)
        {
            dwRowID = LittleEndianConverter.ToUInt32(buffer, offset + 0);
            dwRowIndex = LittleEndianConverter.ToUInt32(buffer, offset + 4);
        }

        public override void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt32(buffer, offset + 0, dwRowID);
            LittleEndianWriter.WriteUInt32(buffer, offset + 4, dwRowIndex);
        }

        public override byte[] Key
        {
            get
            {
                return LittleEndianConverter.GetBytes(dwRowID);
            }
        }

        public override int KeyLength
        {
            get 
            {
                return RecordKeyLength;
            }
        }

        public override int DataLength
        {
            get
            {
                return RecordDataLength;
            }
        }

        public override int CompareTo(byte[] key)
        {
            if (key.Length == KeyLength)
            {
                return dwRowID.CompareTo(LittleEndianConverter.ToUInt32(key, 0));
            }
            return -1;
        }

        public override int CompareTo(BTreeOnHeapDataRecord record)
        {
            if (record is TableContextRowID)
            {
                return dwRowID.CompareTo(((TableContextRowID)record).dwRowID);
            }
            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is TableContextRowID)
            {
                return ((TableContextRowID)obj).dwRowID == dwRowID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return dwRowID.GetHashCode();
        }

        public static int CompareByRowIndex(TableContextRowID a, TableContextRowID b)
        {
            return a.dwRowIndex.CompareTo(b.dwRowIndex);
        }
    }
}
