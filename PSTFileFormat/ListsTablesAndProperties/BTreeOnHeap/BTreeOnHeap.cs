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
    // BTH
    public class BTreeOnHeap<T> where T : BTreeOnHeapDataRecord, new()
    {
        private HeapOnNode m_heap;
        private HeapID m_bTreeHeaderHeapID;
        public BTreeOnHeapHeader BTreeHeader;

        private Dictionary<uint, BTreeOnHeapLeaf<T>> m_leavesCache = new Dictionary<uint, BTreeOnHeapLeaf<T>>();

        public BTreeOnHeap(HeapOnNode heap) : this(heap, heap.HeapHeader.hidUserRoot)
        {
        }

        public BTreeOnHeap(HeapOnNode heap, HeapID bTreeHeaderHeapID)
        {
            m_heap = heap;
            m_bTreeHeaderHeapID = bTreeHeaderHeapID;
            byte[] headerBytes = m_heap.GetHeapItem(bTreeHeaderHeapID);
            BTreeHeader = new BTreeOnHeapHeader(headerBytes);
        }

        public byte[] GetHeapItem(HeapID heapID)
        {
            return m_heap.GetHeapItem(heapID);
        }

        public HeapID ReplaceHeapItem(HeapID heapID, byte[] itemBytes)
        {
            return m_heap.ReplaceHeapItem(heapID, itemBytes);
        }

        public HeapID AddItemToHeap(byte[] itemBytes)
        {
            return m_heap.AddItemToHeap(itemBytes);
        }

        public void RemoveItemFromHeap(HeapID heapID)
        {
            m_heap.RemoveItemFromHeap(heapID);
        }

        public virtual void FlushToDataTree()
        {
            m_heap.FlushToDataTree();
        }

        public virtual void SaveChanges()
        { 
            m_heap.SaveChanges();
        }

        /// <summary>
        /// Find the leaf BTreePage where the entry with the given key should be located
        /// We use this method for search and insertion
        /// Note: you can use record.Index to find the BTreeOnHeapIndex of the record
        /// </summary>
        private BTreeOnHeapIndexRecord FindLeafIndexRecord(byte[] key)
        {
            int keyLength = BTreeHeader.cbKey;
            if (keyLength != key.Length)
            {
                throw new ArgumentException("Key length is not valid");
            }

            if (BTreeHeader.bIdxLevels > 0)
            {
                if (BTreeHeader.hidRoot.IsEmpty)
                {
                    throw new Exception("PST is corrupted, bIdxLevels > 0 and BTH root is empty");
                }
                byte level = BTreeHeader.bIdxLevels;
                byte[] indexBytes = GetHeapItem(BTreeHeader.hidRoot);
                BTreeOnHeapIndex index = new BTreeOnHeapIndex(indexBytes, keyLength);
                index.HeapID = BTreeHeader.hidRoot;

                while (level > 0)
                {
                    BTreeOnHeapIndexRecord lastToMatch = index.Records[0];
                    
                    for (int recordIndex = 1; recordIndex < index.Records.Count; recordIndex++)
                    {
                        BTreeOnHeapIndexRecord record = index.Records[recordIndex];
                        // All the entries in the child have key values greater than or equal to this key value.
                        if (record.CompareTo(key) <= 0)
                        {
                            lastToMatch = record;
                        }
                        else
                        {
                            break;
                        }
                    }

                    level--;

                    if (level == 0)
                    {
                        lastToMatch.Index = index;
                        return lastToMatch;
                    }
                    else
                    {
                        byte[] bytes = GetHeapItem(lastToMatch.hidNextLevel);
                        BTreeOnHeapIndex childIndex = new BTreeOnHeapIndex(bytes, keyLength);
                        childIndex.ParentIndex = index;
                        childIndex.HeapID = lastToMatch.hidNextLevel;
                        index = childIndex;
                    }
                }
            }

            return null;
        }

        public BTreeOnHeapLeaf<T> FindLeaf(byte[] key)
        {
            BTreeOnHeapIndexRecord leafIndexRecord = FindLeafIndexRecord(key);

            // note: the leaf we refer to here is one level below the leaf index record
            HeapID leafHeapID;
            if (leafIndexRecord == null)
            {
                leafHeapID = BTreeHeader.hidRoot;
            }
            else
            {
                leafHeapID = leafIndexRecord.hidNextLevel;
            }

            return GetLeaf(leafHeapID, leafIndexRecord);
        }

        public BTreeOnHeapLeaf<T> GetLeaf(HeapID leafHeapID, BTreeOnHeapIndexRecord leafIndexRecord)
        {
            BTreeOnHeapLeaf<T> result;
            if (!m_leavesCache.ContainsKey(leafHeapID.Value))
            {
                BTreeOnHeapLeaf<T> leaf = GetLeafFromHeap(leafHeapID);
                if (leaf != null)
                {
                    m_leavesCache.Add(leafHeapID.Value, leaf);
                }
                result = leaf;
            }
            else
            {
                result = m_leavesCache[leafHeapID.Value];
            }

            if (result != null && leafIndexRecord != null)
            {
                // The BTH index node might have changed since the leaf was stored,
                // We must not use the index node stored in the cache.
                result.Index = leafIndexRecord.Index;
            }

            return result;
        }

        public BTreeOnHeapLeaf<T> GetLeafFromHeap(HeapID leafHeapID)
        {
            // The hid is set to zero if the BTH is empty
            if (leafHeapID.Value == 0)
            {
                return null;
            }
            else
            {
                byte[] leafBytes = GetHeapItem(leafHeapID);
                BTreeOnHeapLeaf<T> result = new BTreeOnHeapLeaf<T>(leafBytes);
                result.HeapID = leafHeapID;
                return result;
            }
        }

        // the PC is simply a BTH with cbKey set to 2 and cbEnt set to 6
        public T FindRecord(byte[] key)
        {
            BTreeOnHeapLeaf<T> leaf = FindLeaf(key);

            if (leaf != null)
            {
                foreach (T record in leaf.Records)
                {
                    // All the entries in the child have key values greater than or equal to this key value.
                    if (record.IsKeyEquals(key))
                    {
                        return record;
                    }
                }
            }

            return default(T);
        }

        // Note: The PC is simply a BTH with cbKey set to 2 and cbEnt set to 6
        public List<T> GetAll()
        {
            List<T> result = new List<T>();

            List<byte[]> leaves = new List<byte[]>();

            if (BTreeHeader.bIdxLevels > 0)
            {
                KeyValuePairList<byte[], byte> parents = new KeyValuePairList<byte[], byte>();
                parents.Add(GetHeapItem(BTreeHeader.hidRoot), BTreeHeader.bIdxLevels);
                while (parents.Count > 0)
                {
                    byte[] parentBytes = parents[0].Key;
                    byte level = parents[0].Value;

                    int offset = 0;
                    while (offset < parentBytes.Length)
                    {
                        HeapID hid = new HeapID(parentBytes, offset + BTreeHeader.cbKey);
                        byte[] bytes = GetHeapItem(hid);
                        if (level == 1)
                        {
                            leaves.Add(bytes);
                        }
                        else
                        {
                            parents.Add(bytes, (byte)(level - 1));
                        }
                        offset += BTreeHeader.cbKey + HeapID.Length;
                    }
                    parents.RemoveAt(0);
                }
            }
            else
            { 
                leaves.Add(GetHeapItem(BTreeHeader.hidRoot));
            }

            foreach(byte[] leafBytes in leaves)
            {
                int offset = 0;

                while (offset < leafBytes.Length)
                {
                    T record = BTreeOnHeapDataRecord.CreateInstance<T>(leafBytes, offset);
                    result.Add(record);
                    offset += BTreeHeader.cbKey + BTreeHeader.cbEnt;
                }
            }

            return result;
        }

        public BTreeOnHeapIndex CreateNewRoot(byte[] nextLevelKey)
        {
            BTreeOnHeapIndex newRoot = new BTreeOnHeapIndex(nextLevelKey.Length);
            BTreeOnHeapIndexRecord rootIndexRecord = new BTreeOnHeapIndexRecord();

            rootIndexRecord.key = nextLevelKey;
            rootIndexRecord.hidNextLevel = BTreeHeader.hidRoot;
            newRoot.Records.Add(rootIndexRecord);
            newRoot.HeapID = AddItemToHeap(newRoot.GetBytes());
            
            BTreeHeader.hidRoot = newRoot.HeapID;
            BTreeHeader.bIdxLevels++;
            UpdateBTreeHeader();

            return newRoot;
        }

        public void AddRecord(T record)
        {
            BTreeOnHeapLeaf<T> leaf = FindLeaf(record.Key);

            if (leaf == null) // BTH is empty
            {
                leaf = new BTreeOnHeapLeaf<T>();
                leaf.InsertSorted(record);
                HeapID rootHeapID = AddItemToHeap(leaf.GetBytes());
                UpdateRootHeapID(rootHeapID);
            }
            else if (leaf.Records.Count < leaf.MaximumNumberOfRecords)
            {
                int insertIndex = leaf.InsertSorted(record);

                HeapID existingHeapID = leaf.HeapID;
                leaf.HeapID = ReplaceHeapItem(leaf.HeapID, leaf.GetBytes());
                m_leavesCache[leaf.HeapID.Value] = leaf;
                if (leaf.HeapID.Value != existingHeapID.Value)
                {
                    if (leaf.Index == null)
                    {
                        // this is the root node
                        UpdateRootHeapID(leaf.HeapID);
                    }
                    else
                    {
                        // update the parent
                        UpdateIndexRecord(leaf.Index, existingHeapID, leaf.HeapID, leaf.NodeKey);
                    }
                    m_leavesCache.Remove(existingHeapID.Value);
                }
                else if (insertIndex == 0 && leaf.Index != null)
                {
                    // Node key has been modified, we must update the parent
                    UpdateIndexRecord(leaf.Index, leaf.HeapID, leaf.HeapID, leaf.NodeKey);
                }
            }
            else
            {
                // The node is full, we have to split it
                BTreeOnHeapLeaf<T> newNode = leaf.Split();
                if (record.CompareTo(newNode.NodeKey) > 0)
                {
                    newNode.InsertSorted(record);
                }
                else
                {
                    int insertIndex = leaf.InsertSorted(record);
                    if (insertIndex == 0 && leaf.Index != null)
                    {
                        // Node key has been modified, we must update the parent
                        UpdateIndexRecord(leaf.Index, leaf.HeapID, leaf.HeapID, leaf.NodeKey);
                    }
                }

                if (leaf.Index == null)
                {
                    // this is a root page and it's full, we have to create a new root
                    leaf.Index = CreateNewRoot(leaf.NodeKey);
                }

                // Item will be replaced in place, because it has less items than before
                ReplaceHeapItem(leaf.HeapID, leaf.GetBytes());
                m_leavesCache[leaf.HeapID.Value] = leaf;

                HeapID newNodeHeapID = AddItemToHeap(newNode.GetBytes());

                // We made sure we have a parent to add our new page to
                InsertIndexRecord(leaf.Index, newNode.NodeKey, newNodeHeapID);
            }
        }

        public void InsertIndexRecord(BTreeOnHeapIndex index, byte[] key, HeapID hidNextLevel)
        {
            BTreeOnHeapIndexRecord indexRecord = new BTreeOnHeapIndexRecord();
            indexRecord.key = key;
            indexRecord.hidNextLevel = hidNextLevel;
            InsertIndexRecord(index, indexRecord);
        }

        public void InsertIndexRecord(BTreeOnHeapIndex index, BTreeOnHeapIndexRecord record)
        {
            HeapID existingHeapID = index.HeapID;
            if (index.Records.Count < index.MaximumNumberOfRecords)
            {
                int insertIndex = index.InsertSorted(record);
                index.HeapID = ReplaceHeapItem(existingHeapID, index.GetBytes());
                if (index.HeapID.Value != existingHeapID.Value)
                {
                    if (index.ParentIndex == null)
                    {
                        // this is the root node
                        UpdateRootHeapID(index.HeapID);
                    }
                    else
                    {
                        UpdateIndexRecord(index.ParentIndex, existingHeapID, index.HeapID, index.NodeKey);
                    }
                }
                else if (insertIndex == 0 && index.ParentIndex != null)
                {
                    // Node key has been modified, we must update the parent
                    UpdateIndexRecord(index.ParentIndex, existingHeapID, index.HeapID, index.NodeKey);
                }
            }
            else
            {
                // The node is full, we have to split it
                BTreeOnHeapIndex newNode = index.Split();
                if (record.CompareTo(newNode.NodeKey) > 0)
                {
                    newNode.InsertSorted(record);
                }
                else
                {
                    int insertIndex = index.InsertSorted(record);
                    if (insertIndex == 0 && index.ParentIndex != null)
                    {
                        // Node key has been modified, we must update the parent
                        UpdateIndexRecord(index.ParentIndex, index.HeapID, index.HeapID, index.NodeKey);
                    }
                }

                if (index.ParentIndex == null)
                {
                    // this is a root page and it's full, we have to create a new root
                    index.ParentIndex = CreateNewRoot(index.NodeKey);
                }

                // Item will be replaced in place, because it has less items than before
                ReplaceHeapItem(index.HeapID, index.GetBytes());

                HeapID newNodeHeapID = AddItemToHeap(newNode.GetBytes());

                // We made sure we have a parent to add our new page to
                InsertIndexRecord(index.ParentIndex, newNode.NodeKey, newNodeHeapID);
            }
        }

        public void UpdateIndexRecord(BTreeOnHeapIndex index, HeapID oldHeapID, HeapID newHeapID, byte[] newKey)
        {
            int recordIndex = index.GetIndexOfRecord(oldHeapID);
            index.Records[recordIndex].hidNextLevel = newHeapID;
            index.Records[recordIndex].key = newKey;
            // will always replace in place (same size)
            ReplaceHeapItem(index.HeapID, index.GetBytes());

            if (recordIndex == 0 && index.ParentIndex != null)
            {
                UpdateIndexRecord(index.ParentIndex, index.HeapID, index.HeapID, index.NodeKey);
            }
        }

        public void DeleteFromIndexRecord(BTreeOnHeapIndex index, HeapID heapID)
        {
            int recordIndex = index.GetIndexOfRecord(heapID);
            index.Records.RemoveAt(recordIndex);
            if (index.Records.Count > 0)
            {
                // will always replace in place (smaller size)
                ReplaceHeapItem(index.HeapID, index.GetBytes());
            }
            else
            {
                if (index.ParentIndex == null)
                {
                    // this is the root node
                    BTreeHeader.hidRoot = HeapID.EmptyHeapID;
                    BTreeHeader.bIdxLevels = 0;
                    UpdateBTreeHeader();
                }
                else
                {
                    DeleteFromIndexRecord(index.ParentIndex, index.HeapID);
                }
            }
        }

        public void RemoveRecord(byte[] key)
        {
            BTreeOnHeapLeaf<T> leaf = FindLeaf(key);

            if (leaf != null)
            {
                int index = leaf.RemoveRecord(key);
                if (leaf.Records.Count == 0)
                {
                    RemoveItemFromHeap(leaf.HeapID);

                    // leaf with 0 entries is invalid and must be deleted
                    if (leaf.Index == null)
                    {
                        // this is the root node
                        BTreeHeader.hidRoot = HeapID.EmptyHeapID;
                        BTreeHeader.bIdxLevels = 0;
                        UpdateBTreeHeader();
                    }
                    else
                    {
                        DeleteFromIndexRecord(leaf.Index, leaf.HeapID);
                    }
                }
                else
                {
                    // will always replace in place (the new leaf is smaller than the old one)
                    ReplaceHeapItem(leaf.HeapID, leaf.GetBytes());
                    m_leavesCache[leaf.HeapID.Value] = leaf;

                    // scanpst.exe report an error if page key does not match the first entry,
                    // so we want to update the parent
                    if (index == 0 && leaf.Index != null)
                    {
                        UpdateIndexRecord(leaf.Index, leaf.HeapID, leaf.HeapID, leaf.NodeKey);
                    }
                }
            }
        }

        public void UpdateRecord(T record)
        {
            RemoveRecord(record.Key);
            AddRecord(record);
        }

        public void UpdateRootHeapID(HeapID newRootHeapID)
        {
            BTreeHeader.hidRoot = newRootHeapID;
            UpdateBTreeHeader();
        }

        public void UpdateBTreeHeader()
        {
            // we will always replace in place (same size)
            ReplaceHeapItem(m_bTreeHeaderHeapID, BTreeHeader.GetBytes());
        }

        public HeapOnNode Heap
        {
            get
            {
                return m_heap;
            }
        }

        public PSTFile File
        {
            get
            {
                return this.Heap.File;
            }
        }

        public DataTree DataTree
        {
            get
            {
                return m_heap.DataTree;
            }
        }
    }
}
