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

namespace PSTFileFormat
{
    public class BlockBTreeLeafPage : BTreePage
    {
        public const int MaximumNumberOfEntries = 20; // 488 / 24

        public List<BlockBTreeEntry> BlockEntryList = new List<BlockBTreeEntry>();

        public BlockBTreeLeafPage() : base()
        { 
        }

        public BlockBTreeLeafPage(byte[] buffer) : base(buffer)
        {
        }

        public override void PopulateEntries(byte[] buffer, byte numberOfEntries)
        {
            for (int index = 0; index < numberOfEntries; index++)
            {
                int offset = index * cbEnt;
                BlockBTreeEntry entry = new BlockBTreeEntry(buffer, offset);
                BlockEntryList.Add(entry);
            }
        }

        public override int WriteEntries(byte[] buffer)
        {
            int offset = 0;
            foreach (BlockBTreeEntry entry in BlockEntryList)
            {
                entry.WriteBytes(buffer, offset);
                offset += cbEnt;
            }
            return BlockEntryList.Count;
        }

        private int GetSortedInsertIndex(BlockBTreeEntry entryToInsert)
        {
            ulong key = entryToInsert.BREF.bid.Value;

            int insertIndex = 0;
            while (insertIndex < BlockEntryList.Count && key > BlockEntryList[insertIndex].BREF.bid.Value)
            {
                insertIndex++;
            }
            return insertIndex;
        }

        /// <returns>Insert index</returns>
        public int InsertSorted(BlockBTreeEntry entryToInsert)
        {
            int insertIndex = GetSortedInsertIndex(entryToInsert);
            BlockEntryList.Insert(insertIndex, entryToInsert);
            return insertIndex;
        }

        public int GetIndexOfEntry(ulong blockID)
        {
            for (int index = 0; index < BlockEntryList.Count; index++)
            {
                if (BlockEntryList[index].BREF.bid.Value == blockID)
                {
                    return index;
                }
            }
            return -1;
        }

        public int RemoveEntry(ulong blockID)
        {
            int index = GetIndexOfEntry(blockID);
            if (index >= 0)
            {
                BlockEntryList.RemoveAt(index);
            }
            return index;
        }

        public BlockBTreeLeafPage Split()
        {
            int newNodeStartIndex = BlockEntryList.Count / 2;
            BlockBTreeLeafPage newPage = new BlockBTreeLeafPage();
            // BlockID will be given when the page will be added
            newPage.cEntMax = cEntMax;
            newPage.cbEnt = cbEnt;
            newPage.cLevel = cLevel;
            newPage.pageTrailer.ptype = pageTrailer.ptype;
            for (int index = newNodeStartIndex; index < BlockEntryList.Count; index++)
            {
                newPage.BlockEntryList.Add(BlockEntryList[index]);
            }

            BlockEntryList.RemoveRange(newNodeStartIndex, BlockEntryList.Count - newNodeStartIndex);
            return newPage;
        }

        public override ulong PageKey
        {
            get
            {
                return BlockEntryList[0].BREF.bid.Value;
            }
        }
    }
}
