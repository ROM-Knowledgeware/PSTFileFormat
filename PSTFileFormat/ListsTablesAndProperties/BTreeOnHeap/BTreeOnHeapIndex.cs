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
    public class BTreeOnHeapIndex // Level 1+ index
    {
        public int KeyLength;
        public List<BTreeOnHeapIndexRecord> Records = new List<BTreeOnHeapIndexRecord>();

        // These fields help us during updates:
        public BTreeOnHeapIndex ParentIndex;
        public HeapID HeapID;

        public BTreeOnHeapIndex(int keyLength)
        {
            KeyLength = keyLength;
        }

        public BTreeOnHeapIndex(byte[] heapItemBytes, int keyLength)
        {
            KeyLength = keyLength;
            int offset = 0;
            while (offset < heapItemBytes.Length)
            {
                BTreeOnHeapIndexRecord record = new BTreeOnHeapIndexRecord(heapItemBytes, offset, keyLength);
                Records.Add(record);
                offset += keyLength + HeapID.Length;
            }
        }

        public byte[] GetBytes()
        {
            int length = Records.Count * (KeyLength + HeapID.Length);
            byte[] buffer = new byte[length];
            int offset = 0;
            foreach (BTreeOnHeapIndexRecord record in Records)
            {
                record.WriteBytes(buffer, offset);
                offset += KeyLength + HeapID.Length;
            }

            return buffer;
        }

        public int GetIndexOfRecord(byte[] key)
        {
            for (int index = 0; index < Records.Count; index++)
            {
                if (ByteUtils.AreByteArraysEqual(Records[index].key, key))
                {
                    return index;
                }
            }
            return -1;
        }

        public int GetIndexOfRecord(HeapID hidNextLevel)
        {
            for (int index = 0; index < Records.Count; index++)
            {
                if (Records[index].hidNextLevel.Value == hidNextLevel.Value)
                {
                    return index;
                }
            }
            return -1;
        }

        public int GetSortedInsertIndex(BTreeOnHeapIndexRecord recordToInsert)
        {
            int insertIndex = 0;
            while (insertIndex < Records.Count && recordToInsert.CompareTo(Records[insertIndex]) > 0)
            {
                insertIndex++;
            }
            return insertIndex;
        }

        public int InsertSorted(BTreeOnHeapIndexRecord recordToInsert)
        {
            int insertIndex = GetSortedInsertIndex(recordToInsert);
            Records.Insert(insertIndex, recordToInsert);
            return insertIndex;
        }

        public BTreeOnHeapIndex Split()
        {
            int newNodeStartIndex = Records.Count / 2;
            BTreeOnHeapIndex newNode = new BTreeOnHeapIndex(this.KeyLength);
            newNode.ParentIndex = ParentIndex;
            // Heap ID will be given when the item will be added
            for (int index = newNodeStartIndex; index < Records.Count; index++)
            {
                newNode.Records.Add(Records[index]);
            }

            Records.RemoveRange(newNodeStartIndex, Records.Count - newNodeStartIndex);
            return newNode;
        }

        public int MaximumNumberOfRecords
        {
            get
            {
                return HeapOnNode.MaximumAllocationLength / (KeyLength + HeapID.Length);
            }
        }

        public byte[] NodeKey
        {
            get
            {
                return Records[0].key;
            }
        }
    }
}
