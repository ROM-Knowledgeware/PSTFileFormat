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
using System.IO;
using System.Text;
using Utilities;

namespace PSTFileFormat
{
    public class BTree : BufferedBTreePageStore
    {
        BTreePage m_bTreeRootPage;

        public BTree(PSTFile file, BTreePage bTreeRootPage) : base(file)
        {
            m_bTreeRootPage = bTreeRootPage;
        }

        private void CascadeUpReferenceUpdate(BTreeIndexPage indexPage, ulong oldBlockID, ulong newBlockID)
        {
            int index = indexPage.GetIndexOfBlockID(oldBlockID);
            if (index >= 0)
            {
                BTreeIndexEntry entry = indexPage.IndexEntryList[index];
                entry.BREF.bid = new BlockID(newBlockID);
                entry.BREF.ib = 0; // this will tell BufferedBTreePageStore that the block hasn't been written yet
                ulong currentBlockID = indexPage.BlockID.Value;
                UpdatePage(indexPage);
                if (currentBlockID != indexPage.BlockID.Value &&
                    indexPage.ParentPage != null)
                {
                    CascadeUpReferenceUpdate(indexPage.ParentPage, currentBlockID, indexPage.BlockID.Value);
                }
            }
        }

        /// <summary>
        /// After modifying a page, we must update its parent's BlockID to refer to the new page
        /// </summary>
        protected void UpdatePageAndReferences(BTreePage page)
        {
            ulong currentBlockID = page.BlockID.Value;
            UpdatePage(page);

            if (currentBlockID != page.BlockID.Value &&
                page.ParentPage != null)
            {
                // we must update the parent to refer to the new page,
                // and cascade the changes up to the root page
                CascadeUpReferenceUpdate(page.ParentPage, currentBlockID, page.BlockID.Value);
            }
        }

        public void CreateNewRoot()
        {
            BTreeIndexPage newRoot = BTreeIndexPage.GetEmptyIndexPage(m_bTreeRootPage.pageTrailer.ptype, m_bTreeRootPage.cLevel + 1);

            BTreeIndexEntry rootIndexEntry = new BTreeIndexEntry();

            // We make sure the old root page has been updated (to prevent the BlockID from changing during update)
            UpdatePage(m_bTreeRootPage);

            rootIndexEntry.btkey = m_bTreeRootPage.PageKey;
            rootIndexEntry.BREF.bid = m_bTreeRootPage.BlockID;
            rootIndexEntry.BREF.ib = 0; // this will tell BufferedBTreePageStore that the block hasn't been written yet
            newRoot.IndexEntryList.Add(rootIndexEntry);
            AddPage(newRoot);
            m_bTreeRootPage.ParentPage = newRoot;
            m_bTreeRootPage = newRoot;
        }

        /// <summary>
        /// Find the leaf BTreePage where the entry with the given key should be located
        /// We use this method for search and insertion
        /// Note: the returned BTreePage will have its ParentPage set (can be traversed up to the bTreeRootPage)
        /// </summary>
        protected BTreePage FindLeafBTreePage(ulong key)
        {
            BTreePage currentPage = m_bTreeRootPage;
            int cLevel = m_bTreeRootPage.cLevel;
            while (cLevel > 0)
            {
                int matchingIndex = ((BTreeIndexPage)currentPage).GetIndexOfEntryWithMatchingRange(key);
                BTreeIndexEntry matchingEntry = ((BTreeIndexPage)currentPage).IndexEntryList[matchingIndex];

                BTreePage childPage = GetPage(matchingEntry.BREF);

                childPage.ParentPage = (BTreeIndexPage)currentPage;
                currentPage = childPage;
                cLevel--;
                if (cLevel != currentPage.cLevel)
                {
                    throw new Exception("Invalid Page cLevel");
                }
            }

            return currentPage;
        }

        /// <param name="blockID">BlockID of the child page with the new key</param>
        protected void UpdateIndexEntry(BTreeIndexPage indexPage, BlockID blockIDOfChild, ulong newKeyOfChild)
        {
            for(int index = 0; index < indexPage.IndexEntryList.Count; index++)
            {
                BTreeIndexEntry entry = indexPage.IndexEntryList[index];
                if (entry.BREF.bid.Value == blockIDOfChild.Value)
                {
                    entry.btkey = newKeyOfChild;
                    UpdatePageAndReferences(indexPage); // parents will now refer to the new blockID
                    if (index == 0 && indexPage.ParentPage != null)
                    { 
                        // page key has been modified, we must update the parent as well
                        UpdateIndexEntry(indexPage.ParentPage, indexPage.BlockID, indexPage.PageKey);
                    }
                }
            }
        }

        protected void DeleteIndexEntry(BTreeIndexPage indexPage, BlockID blockIDOfChild)
        {
            for (int index = 0; index < indexPage.IndexEntryList.Count; index++)
            {
                BTreeIndexEntry entry = indexPage.IndexEntryList[index];
                if (entry.BREF.bid.Value == blockIDOfChild.Value)
                {
                    indexPage.IndexEntryList.RemoveAt(index);
                    UpdatePageAndReferences(indexPage); // parents will now refer to the new blockID
                    if (index == 0 && indexPage.ParentPage != null)
                    {
                        if (indexPage.IndexEntryList.Count > 0)
                        {
                            // page key has been modified, we must update the parent as well
                            UpdateIndexEntry(indexPage.ParentPage, indexPage.BlockID, indexPage.PageKey);
                        }
                        else
                        {
                            DeleteIndexEntry(indexPage.ParentPage, indexPage.BlockID);
                            DeletePage(indexPage);
                        }
                    }
                }
            }
        }

        protected void InsertIndexEntry(BTreeIndexPage indexPage, ulong btkey, BlockID blockID)
        {
            BTreeIndexEntry indexEntry = new BTreeIndexEntry();
            indexEntry.btkey = btkey;
            indexEntry.BREF.bid = blockID;
            InsertIndexEntry(indexPage, indexEntry);
        }

        protected void InsertIndexEntry(BTreeIndexPage indexPage, BTreeIndexEntry entryToInsert)
        {
            if (indexPage.IndexEntryList.Count < BTreeIndexPage.MaximumNumberOfEntries)
            {
                int insertIndex = indexPage.InsertSorted(entryToInsert);
                UpdatePageAndReferences(indexPage);
                if (insertIndex == 0 && indexPage.ParentPage != null)
                {
                    // page key has been modified, we must update the parent
                    UpdateIndexEntry(indexPage.ParentPage, indexPage.BlockID, indexPage.PageKey);
                }
            }
            else
            {
                // The tree node is full, we have to split it
                BTreeIndexPage newPage = indexPage.Split();
                if (newPage.PageKey < entryToInsert.btkey)
                {
                    newPage.InsertSorted(entryToInsert);
                }
                else
                {
                    int insertIndex = indexPage.InsertSorted(entryToInsert);
                    if (insertIndex == 0 && indexPage.ParentPage != null)
                    {
                        // page key has been modified, we must update the parent
                        UpdateIndexEntry(indexPage.ParentPage, indexPage.BlockID, indexPage.PageKey);
                    }
                }
                UpdatePageAndReferences(indexPage);
                AddPage(newPage);

                if (indexPage.ParentPage == null)
                {
                    // this is a root page and it's full, we have to create a new root
                    CreateNewRoot();
                }

                // We made sure we have a parent to add our new page to
                InsertIndexEntry(indexPage.ParentPage, newPage.PageKey, newPage.BlockID);
            }
        }

        public BTreePage RootPage
        {
            get
            {
                return m_bTreeRootPage;
            }
        }
    }
}
