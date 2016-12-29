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
    public class BufferedBTreePageStore
    {
        private PSTFile m_file;

        // We use the page buffer to store cached and modified BTree pages
        // besides caching, the main purpose of this buffer is to store the modifications to the BTree,
        // this way they could be written to the PST file later in a single transaction.
        private Dictionary<ulong, BTreePage> m_pageBuffer = new Dictionary<ulong, BTreePage>();

        // The NDB is immutable, we allocate pages for both new pages and modified pages,
        // for modified pages, we unallocate the original pages (using m_pagesToFree).
        private List<ulong> m_pagesToWrite = new List<ulong>();
        private List<ulong> m_offsetsToFree = new List<ulong>();

        public BufferedBTreePageStore(PSTFile file)
        {
            m_file = file;
        }

        public BTreePage GetPage(BlockRef blockRef)
        {
            ulong blockID = blockRef.bid.Value;
            
            if (m_pageBuffer.ContainsKey(blockID))
            {
                return m_pageBuffer[blockID];
            }
            else
            {
                BTreePage page = BTreePage.ReadFromStream(m_file.BaseStream, blockRef);
                m_pageBuffer.Add(blockRef.bid.Value, page);
                return page;
            }
        }

        public bool IsPagePendingWrite(BTreePage page)
        {
            return IsPagePendingWrite(page.BlockID.Value);
        }

        public bool IsPagePendingWrite(ulong blockID)
        {
            return m_pagesToWrite.Contains(blockID);
        }

        public void UpdatePage(BTreePage page)
        {
            if (IsPagePendingWrite(page))
            {
                m_pageBuffer[page.BlockID.Value] = page;
            }
            else
            {
                ulong oldBlockID = page.BlockID.Value;
                if (m_pageBuffer.ContainsKey(oldBlockID))
                {
                    // remove the old block from the buffer
                    m_pageBuffer.Remove(oldBlockID);
                }

                m_offsetsToFree.Add(page.Offset);

                page.BlockID = m_file.Header.AllocateNextPageBlockID();
                
                m_pageBuffer.Add(page.BlockID.Value, page);
                m_pagesToWrite.Add(page.BlockID.Value);
            }
        }

        public void AddPage(BTreePage page)
        {
            // It is not clear whether new BTree pages should be marked as internal or not
            // Outlook sometimes chooses to mark them as internal, and sometimes not (for both BBT and NBT)
            page.BlockID = m_file.Header.AllocateNextPageBlockID();
            
            m_pageBuffer.Add(page.BlockID.Value, page);
            m_pagesToWrite.Add(page.BlockID.Value);
        }

        public void DeletePage(BTreePage page)
        {
            ulong blockID = page.BlockID.Value;

            if (m_pageBuffer.ContainsKey(blockID))
            {
                // remove the old block from the cache
                m_pageBuffer.Remove(blockID);
            }

            // no need to free a block that has not been written yet
            if (IsPagePendingWrite(page))
            {
                m_pagesToWrite.Remove(blockID);
            }
            else
            {
                m_offsetsToFree.Add(page.Offset);
            }
        }

        public void SaveChanges()
        {
            List<BTreePage> pages = new List<BTreePage>();
            foreach (BTreePage page in m_pageBuffer.Values)
            {
                if (m_pagesToWrite.Contains(page.BlockID.Value))
                {
                    pages.Add(page);
                }
            }
            SortByCLevel(pages, false);

            // we now have the pages in ascending cLevel order (i.e. leaves first)
            // we need them this way because we need to update the offset from old pages to new pages
            // so we must write children first

            // for new blocks:
            Dictionary<ulong, long> blockToOffset = new Dictionary<ulong, long>();
            foreach (BTreePage page in pages)
            {
                if (page is BTreeIndexPage) // page.cLevel > 0
                {
                    // parent page, we may need to update references
                    BTreeIndexPage indexPage = (BTreeIndexPage)page;
                    for (int index = 0; index < indexPage.IndexEntryList.Count; index++)
                    {
                        ulong childBlockID = indexPage.IndexEntryList[index].BREF.bid.Value;

                        if (indexPage.IndexEntryList[index].BREF.ib == 0) // reference to a new / updated block
                        {
                            long blockOffset = blockToOffset[childBlockID];
                            indexPage.IndexEntryList[index].BREF.ib = (ulong)blockOffset;
                        }
                    }
                }

                long newPageAllocationOffset = AllocationHelper.AllocateSpaceForPage(m_file);
                blockToOffset.Add(page.BlockID.Value, newPageAllocationOffset);
                page.Offset = (ulong)newPageAllocationOffset; // we may need this later
                byte[] pageBytes = page.GetBytes((ulong)newPageAllocationOffset);
                m_file.BaseStream.Seek(newPageAllocationOffset, SeekOrigin.Begin);
                m_file.BaseStream.Write(pageBytes, 0, pageBytes.Length);
            }

            // free all the marked pages
            foreach (ulong offset in m_offsetsToFree)
            {
                AllocationHelper.FreePageAllocation(m_file, (long)offset);
            }

            m_pagesToWrite.Clear();
            m_offsetsToFree.Clear();
        }

        public void SortByCLevel(List<BTreePage> pages, bool descendingOrder)
        {
            pages.Sort(CompareByCLevel);
            if (descendingOrder)
            {
                pages.Reverse();
            }
        }

        public PSTFile File
        {
            get
            {
                return m_file;
            }
        }

        private static int CompareByCLevel(BTreePage x, BTreePage y)
        {
            if (x.cLevel > y.cLevel)
            {
                return 1;
            }
            else if (x.cLevel < y.cLevel)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
