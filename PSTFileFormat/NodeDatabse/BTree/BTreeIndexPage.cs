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
    public class BTreeIndexPage : BTreePage // Intermediate page
    {
        public const int MaximumNumberOfEntries = 20; // 488 / 24

        public List<BTreeIndexEntry> IndexEntryList = new List<BTreeIndexEntry>();

        public BTreeIndexPage() : base()
        { 
        }

        public BTreeIndexPage(byte[] buffer) : base(buffer)
        {
        }

        public override void PopulateEntries(byte[] buffer, byte numberOfEntries)
        {
            for (int index = 0; index < numberOfEntries; index++)
            {
                int offset = index * cbEnt;
                BTreeIndexEntry entry = new BTreeIndexEntry(buffer, offset);
                IndexEntryList.Add(entry);
            }
        }

        public override int WriteEntries(byte[] buffer)
        {
            int offset = 0;
            foreach (BTreeIndexEntry entry in IndexEntryList)
            {
                entry.WriteBytes(buffer, offset);
                offset += cbEnt;
            }

            return IndexEntryList.Count;
        }

        public int GetIndexOfEntryWithMatchingRange(ulong key)
        {
            int lastToMatchIndex = 0;
            for (int index = 1; index < IndexEntryList.Count; index++)
            {
                BTreeIndexEntry entry = IndexEntryList[index];
                // All the entries in the child BTPAGE .. have key values greater than or equal to this key value.
                if (key >= entry.btkey)
                {
                    lastToMatchIndex = index;
                }
            }
            return lastToMatchIndex;
        }

        public int GetIndexOfBlockID(ulong blockID)
        {
            for (int index = 0; index < IndexEntryList.Count; index++)
            {
                if (IndexEntryList[index].BREF.bid.Value == blockID)
                {
                    return index;
                }
            }
            return -1;
        }

        public int GetIndexOfEntry(ulong key)
        {
            for (int index = 0; index < IndexEntryList.Count; index++)
            {
                if (IndexEntryList[index].btkey == key)
                {
                    return index;
                }
            }
            return -1;
        }

        public int GetSortedInsertIndex(BTreeIndexEntry entryToInsert)
        {
            ulong key = entryToInsert.btkey;

            int insertIndex = 0;
            while (insertIndex < IndexEntryList.Count && key > IndexEntryList[insertIndex].btkey)
            {
                insertIndex++;
            }
            return insertIndex;
        }

        private void InsertSorted(ulong btkey, BlockID blockID)
        {
            BTreeIndexEntry entry = new BTreeIndexEntry();
            entry.BREF.bid = blockID;
            InsertSorted(entry);
        }

        /// <returns>Insert index</returns>
        public int InsertSorted(BTreeIndexEntry entryToInsert)
        {
            int insertIndex = GetSortedInsertIndex(entryToInsert);
            IndexEntryList.Insert(insertIndex, entryToInsert);
            return insertIndex;
        }

        public BTreeIndexPage Split()
        {
            int newNodeStartIndex = IndexEntryList.Count / 2;
            BTreeIndexPage newPage = new BTreeIndexPage();
            // blockID will be given when the page will be added
            newPage.cEntMax = cEntMax;
            newPage.cbEnt = cbEnt;
            newPage.cLevel = cLevel;
            newPage.pageTrailer.ptype = pageTrailer.ptype;
            for (int index = newNodeStartIndex; index < IndexEntryList.Count; index++)
            {
                newPage.IndexEntryList.Add(IndexEntryList[index]);
            }

            IndexEntryList.RemoveRange(newNodeStartIndex, IndexEntryList.Count - newNodeStartIndex);
            return newPage;
        }

        public static BTreeIndexPage GetEmptyIndexPage(PageTypeName pageType, int cLevel)
        {
            BTreeIndexPage page = new BTreeIndexPage();
            page.pageTrailer.ptype = pageType;
            page.cbEnt = BTreeIndexEntry.Length;
            page.cEntMax = MaximumNumberOfEntries;
            page.cLevel = (byte)cLevel;
            return page;
        }

        // the key of the first entry in the page
        public override ulong PageKey
        {
            get 
            {
                return IndexEntryList[0].btkey;
            }
        }
    }
}
