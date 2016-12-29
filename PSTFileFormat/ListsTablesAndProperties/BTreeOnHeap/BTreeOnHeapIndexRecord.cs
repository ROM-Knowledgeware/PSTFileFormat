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
    public class BTreeOnHeapIndexRecord
    {
        public byte[] key;
        public HeapID hidNextLevel;

        // This field helps us during updates:
        public BTreeOnHeapIndex Index;

        public BTreeOnHeapIndexRecord()
        { 
        }

        public BTreeOnHeapIndexRecord(byte[] buffer, int offset, int keyLength)
        {
            key = ByteReader.ReadBytes(buffer, offset, keyLength);
            hidNextLevel = new HeapID(buffer, offset + keyLength);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            ByteWriter.WriteBytes(buffer, offset, key);
            LittleEndianWriter.WriteUInt32(buffer, offset + key.Length, hidNextLevel.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is BTreeOnHeapIndexRecord)
            {
                return (CompareTo((BTreeOnHeapIndexRecord)obj) == 0);
            }
            return false;
        }

        public int CompareTo(BTreeOnHeapIndexRecord record)
        {
            return CompareTo(record.key);
        }

        public int CompareTo(byte[] keyToCompare)
        {
            if (key.Length == 2 && keyToCompare.Length == 2)
            {
                return LittleEndianConverter.ToUInt16(key, 0).CompareTo(LittleEndianConverter.ToUInt16(keyToCompare, 0));
            }
            else if (key.Length == 4 && keyToCompare.Length == 4)
            {
                return LittleEndianConverter.ToUInt32(key, 0).CompareTo(LittleEndianConverter.ToUInt32(keyToCompare, 0));
            }
            else if (key.Length == 8 && keyToCompare.Length == 8)
            {
                return LittleEndianConverter.ToUInt64(key, 0).CompareTo(LittleEndianConverter.ToUInt64(keyToCompare, 0));
            }
            else
            {
                // key.Length could be 16
                throw new NotImplementedException("Unsupported key length");
            }
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }
    }
}
