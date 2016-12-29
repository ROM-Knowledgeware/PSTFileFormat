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
    public class NodeBTreeLeafPage : BTreePage
    {
        public const int MaximumNumberOfEntries = 15; // 488 / 32

        public List<NodeBTreeEntry> NodeEntryList = new List<NodeBTreeEntry>();

        public NodeBTreeLeafPage() : base()
        { 
        }

        public NodeBTreeLeafPage(byte[] buffer) : base(buffer)
        {
        }

        public override void PopulateEntries(byte[] buffer, byte numberOfEntries)
        {
            int offset = 0;
            for (int index = 0; index < numberOfEntries; index++)
            {
                NodeBTreeEntry entry = new NodeBTreeEntry(buffer, offset);
                NodeEntryList.Add(entry);
                offset += cbEnt;
            }
        }

        public override int WriteEntries(byte[] buffer)
        {
            int offset = 0;
            foreach (NodeBTreeEntry entry in NodeEntryList)
            {
                entry.WriteBytes(buffer, offset);
                offset += cbEnt;
            }
            return NodeEntryList.Count;
        }

        private int GetSortedInsertIndex(NodeBTreeEntry entryToInsert)
        {
            ulong key = entryToInsert.nid.Value;

            int insertIndex = 0;
            while (insertIndex < NodeEntryList.Count && key > NodeEntryList[insertIndex].nid.Value)
            {
                insertIndex++;
            }
            return insertIndex;
        }

        /// <returns>Insert index</returns>
        public int InsertSorted(NodeBTreeEntry entryToInsert)
        {
            int insertIndex = GetSortedInsertIndex(entryToInsert);
            NodeEntryList.Insert(insertIndex, entryToInsert);
            return insertIndex;
        }

        public int GetIndexOfEntry(uint nodeID)
        {
            for (int index = 0; index < NodeEntryList.Count; index++)
            {
                if (NodeEntryList[index].nid.Value == nodeID)
                {
                    return index;
                }
            }
            return -1;
        }

        public int RemoveEntry(uint nodeID)
        {
            int index = GetIndexOfEntry(nodeID);
            if (index >= 0)
            {
                NodeEntryList.RemoveAt(index);
            }
            return index;
        }

        public NodeBTreeLeafPage Split()
        {
            int newNodeStartIndex = NodeEntryList.Count / 2;
            NodeBTreeLeafPage newPage = new NodeBTreeLeafPage();
            // BlockID will be given when the page will be added
            newPage.cEntMax = cEntMax;
            newPage.cbEnt = cbEnt;
            newPage.cLevel = cLevel;
            newPage.pageTrailer.ptype = pageTrailer.ptype;
            for (int index = newNodeStartIndex; index < NodeEntryList.Count; index++)
            {
                newPage.NodeEntryList.Add(NodeEntryList[index]);
            }

            NodeEntryList.RemoveRange(newNodeStartIndex, NodeEntryList.Count - newNodeStartIndex);
            return newPage;
        }

        public override ulong PageKey
        {
            get 
            {
                return NodeEntryList[0].nid.Value;
            }
        }
    }
}
