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
    public class NodeBTree : BTree
    {
        public NodeBTree(PSTFile file, BTreePage bTreeRootPage) : base(file, bTreeRootPage)
        {
        }

        public NodeBTreeEntry FindNodeEntryByNodeID(uint nodeID)
        {
            NodeBTreeLeafPage page = (NodeBTreeLeafPage)FindLeafBTreePage(nodeID);

            if (page != null)
            {
                // page.cLevel is now 0
                List<NodeBTreeEntry> nodeEntryList = page.NodeEntryList;
                foreach (NodeBTreeEntry entry in nodeEntryList)
                {
                    if (entry.nid.Value == nodeID)
                    {
                        return entry;
                    }
                }
            }
            return null;
        }

        public void InsertNodeEntry(NodeID nodeID, DataTree dataTree, SubnodeBTree subnodeBTree, NodeID parentNodeID)
        {
            NodeBTreeEntry entry = new NodeBTreeEntry();
            entry.nid = nodeID;
            entry.bidData = new BlockID(0);
            if (dataTree != null && dataTree.RootBlock != null)
            {
                entry.bidData = dataTree.RootBlock.BlockID;
            }
            
            entry.bidSub = new BlockID(0);
            if (subnodeBTree != null && subnodeBTree.RootBlock != null)
            {
                entry.bidSub = subnodeBTree.RootBlock.BlockID;
            }

            entry.nidParent = parentNodeID;
            InsertNodeEntry(entry);
        }

        public void InsertNodeEntry(NodeBTreeEntry entryToInsert)
        {
            ulong key = entryToInsert.nid.Value;
            NodeBTreeLeafPage page = (NodeBTreeLeafPage)FindLeafBTreePage(key);
            if (page.NodeEntryList.Count < NodeBTreeLeafPage.MaximumNumberOfEntries)
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
                NodeBTreeLeafPage newPage = page.Split();
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

                InsertIndexEntry(page.ParentPage, newPage.NodeEntryList[0].nid.Value, newPage.BlockID);
            }
        }

        public void UpdateNodeEntry(NodeID nodeID, DataTree dataTree, SubnodeBTree subnodeBTree)
        {
            NodeBTreeLeafPage leaf = (NodeBTreeLeafPage)FindLeafBTreePage(nodeID.Value);
            int index = leaf.GetIndexOfEntry(nodeID.Value);
            if (index >= 0)
            {
                leaf.NodeEntryList[index].bidData = new BlockID(0);
                if (dataTree != null && dataTree.RootBlock != null)
                {
                    leaf.NodeEntryList[index].bidData = dataTree.RootBlock.BlockID;
                }
                
                leaf.NodeEntryList[index].bidSub = new BlockID(0);
                if (subnodeBTree != null && subnodeBTree.RootBlock != null)
                {
                    leaf.NodeEntryList[index].bidSub = subnodeBTree.RootBlock.BlockID;
                }

                // now we have to store the new leaf page, and cascade the changes up to the root page
                UpdatePageAndReferences(leaf);
            }
        }

        public void DeleteNodeEntry(NodeID nodeID)
        {
            NodeBTreeLeafPage leaf = (NodeBTreeLeafPage)FindLeafBTreePage(nodeID.Value);

            // Remove the entry
            int index = leaf.RemoveEntry(nodeID.Value);

            if (index == 0)
            {
                // scanpst.exe report an error if page key does not match the first entry,
                // so we want to update the parent
                if (leaf.ParentPage != null)
                {
                    if (leaf.NodeEntryList.Count > 0)
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
