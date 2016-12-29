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
    // Represents BTree node with PC or TCRowID records
    public class BTreeOnHeapLeaf<T> where T : BTreeOnHeapDataRecord, new()
    {
        public List<T> Records = new List<T>();

        // These fields helps us during updates:
        public BTreeOnHeapIndex Index; // The parent of this leaf
        public HeapID HeapID;

        public BTreeOnHeapLeaf()
        { 
        }

        public BTreeOnHeapLeaf(byte[] heapItemBytes) 
        { 
            int offset = 0;

            while (offset < heapItemBytes.Length)
            {
                T record = BTreeOnHeapDataRecord.CreateInstance<T>(heapItemBytes, offset);
                Records.Add(record);
                offset += record.RecordLength;
            }
        }

        public byte[] GetBytes()
        {
            int recordLength = new T().RecordLength;
            int length = Records.Count * recordLength;
            byte[] buffer = new byte[length];
            int offset = 0;
            foreach (T record in Records)
            {
                record.WriteBytes(buffer, offset);
                offset += recordLength;
            }

            return buffer;
        }

        public int GetSortedInsertIndex(T recordToInsert)
        {
            int insertIndex = 0;
            while (insertIndex < Records.Count && recordToInsert.CompareTo(Records[insertIndex]) > 0)
            {
                insertIndex++;
            }
            return insertIndex;
        }

        public int InsertSorted(T recordToInsert)
        {
            int insertIndex = GetSortedInsertIndex(recordToInsert);
            Records.Insert(insertIndex, recordToInsert);
            return insertIndex;
        }

        public int GetIndexOfRecord(byte[] key)
        {
            for (int index = 0; index < Records.Count; index++)
            {
                if (Records[index].IsKeyEquals(key))
                {
                    return index;
                }
            }
            return -1;
        }

        /// <returns>Index of record that was removed</returns>
        public int RemoveRecord(byte[] key)
        {
            int index = GetIndexOfRecord(key);
            if (index >= 0)
            {
                Records.RemoveAt(index);
            }
            return index;
        }

        public BTreeOnHeapLeaf<T> Split()
        {
            int newNodeStartIndex = Records.Count / 2;
            BTreeOnHeapLeaf<T> newNode = new BTreeOnHeapLeaf<T>();
            newNode.Index = Index;
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
                int recordLength = new T().RecordLength;
                return HeapOnNode.MaximumAllocationLength / recordLength;
            }
        }

        public byte[] NodeKey
        {
            get
            {
                return Records[0].Key;
            }
        }
    }
}
