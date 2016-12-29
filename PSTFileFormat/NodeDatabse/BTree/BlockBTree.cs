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
    public class BlockBTree : BTree
    {
        public BlockBTree(PSTFile file, BTreePage bTreeRootPage) : base(file, bTreeRootPage)
        {
        }

        public BlockBTreeEntry FindBlockEntryByBlockID(ulong blockID)
        {
            // Readers MUST ignore the reserved bit and treat it as zero before looking up the BID from the BBT
            //
            // Documented cases where the Reserved bit was set to true:
            // 1. PST was created by Outlook 2010 RTM, the hirerarchy table of the 'Top of Personal Folders'
            //    had a rows subnode, and the entry in the subnode-BTree had the Reserved bit set to true,
            //    However, both the BBT entry (BREF) and the PageTrailer had the corresponding entry with the bit set to false.
            // 2. PST was created by Outlook 2007 RTM, this library created a calendar folder that was modified by Outlook (columns were added)
            //    The XBlock entry had the reserved bit set to true,
            //    However, both the BBT entry (BREF) and the PageTrailer had the corresponding entry with the bit set to false.
            ulong lookupValue = blockID & 0xFFFFFFFFFFFFFFFEU;

            BlockBTreeLeafPage page = (BlockBTreeLeafPage)FindLeafBTreePage(lookupValue);

            if (page != null)
            {
                // page.cLevel is now 0
                List<BlockBTreeEntry> blockBTreeEntryList = page.BlockEntryList;
                foreach (BlockBTreeEntry entry in blockBTreeEntryList)
                {
                    if (entry.BREF.bid.Value == lookupValue)
                    {
                        return entry;
                    }
                }
            }
            return null;
        }

        public void InsertBlockEntry(BlockID blockID, long offset, int dataLength)
        {
            BlockBTreeEntry entryToInsert = new BlockBTreeEntry();
            entryToInsert.BREF.bid = blockID;
            entryToInsert.BREF.ib = (ulong)offset;
            entryToInsert.cb = (ushort)dataLength;
            entryToInsert.cRef = 2; // Any leaf BBT entry that points to a BID holds a reference count to it
            InsertBlockEntry(entryToInsert);
        }

        public void InsertBlockEntry(BlockBTreeEntry entryToInsert)
        {
            ulong key = entryToInsert.BREF.bid.Value;
            BlockBTreeLeafPage page = (BlockBTreeLeafPage)FindLeafBTreePage(key);
            if (page.BlockEntryList.Count < BlockBTreeLeafPage.MaximumNumberOfEntries)
            {
                int insertIndex = page.InsertSorted(entryToInsert);
                UpdatePageAndReferences(page);

                if (insertIndex == 0 && page.ParentPage != null)
                {
                    // page key has been modified, we must update the parent
                    UpdateIndexEntry(page.ParentPage, page.BlockID, page.PageKey);
                }
            }
            else
            {
                // We have to split the tree node
                BlockBTreeLeafPage newPage = page.Split();
                if (newPage.PageKey < key)
                {
                    newPage.InsertSorted(entryToInsert);
                }
                else
                {
                    int insertIndex = page.InsertSorted(entryToInsert);
                    if (insertIndex == 0 && page.ParentPage != null)
                    {
                        // page key has been modified, we must update the parent
                        UpdateIndexEntry(page.ParentPage, page.BlockID, page.PageKey);
                    }
                }
                UpdatePageAndReferences(page);
                AddPage(newPage);

                if (page.ParentPage == null)
                {
                    // this is a root page and it's full, we have to create a new root
                    CreateNewRoot();
                }
                
                InsertIndexEntry(page.ParentPage, newPage.PageKey, newPage.BlockID);
            }
        }

        public void IncrementBlockEntryReferenceCount(BlockID blockID)
        {
            BlockBTreeEntry entry = FindBlockEntryByBlockID(blockID.LookupValue);
            UpdateBlockEntry(blockID, (ushort)(entry.cRef + 1));
        }

        public void UpdateBlockEntry(BlockID blockID, ushort cRef)
        {
            BlockBTreeLeafPage leaf = (BlockBTreeLeafPage)FindLeafBTreePage(blockID.LookupValue);
            int index = leaf.GetIndexOfEntry(blockID.LookupValue);
            if (index >= 0)
            {
                leaf.BlockEntryList[index].cRef = cRef;

                // now we have to store the new leaf page, and cascade the changes up to the root page
                UpdatePageAndReferences(leaf);
            }
        }

        /// <summary>
        /// Since the PST is immutable, any change will result in a new BBT root page
        /// Note: we may leave an empty leaf page
        /// </summary>
        /// <returns>New BlockBTree root page</returns>
        public void DeleteBlockEntry(BlockID blockID)
        {
            BlockBTreeLeafPage leaf = (BlockBTreeLeafPage)FindLeafBTreePage(blockID.LookupValue);

            // Remove the entry
            int index = leaf.RemoveEntry(blockID.LookupValue);

            if (index == 0)
            {
                // scanpst.exe report an error if page key does not match the first entry,
                // so we want to update the parent
                if (leaf.ParentPage != null)
                {
                    if (leaf.BlockEntryList.Count > 0)
                    {
                        UpdateIndexEntry(leaf.ParentPage, leaf.BlockID, leaf.PageKey);
                    }
                    else
                    {
                        DeleteIndexEntry(leaf.ParentPage, leaf.BlockID);
                        DeletePage(leaf);
                        return;
                    }
                }
            }

            // now we have to store the new leaf page, and cascade the changes up to the root page
            UpdatePageAndReferences(leaf);
        }
    }
}
