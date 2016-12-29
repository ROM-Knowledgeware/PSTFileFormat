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
    public class NodeStorageHelper
    {
        public static byte[] GetExternalPropertyBytes(HeapOnNode heap, SubnodeBTree subnodeBTree, HeapOrNodeID heapOrNodeID)
        {
            if (heapOrNodeID.IsEmpty)
            {
                return new byte[0];
            }
            else if (heapOrNodeID.IsHeapID)
            {
                byte[] result = heap.GetHeapItem(heapOrNodeID.HeapID);
                return result;
            }
            else
            {
                // indicates that the item is stored in the subnode block, and the NID is the local NID under the subnode
                Subnode subnode = subnodeBTree.GetSubnode(heapOrNodeID.NodeID);
                if (subnode != null)
                {
                    if (subnode.DataTree == null)
                    {
                        return new byte[0];
                    }
                    else
                    {
                        return subnode.DataTree.GetData();
                    }
                }
                else
                {
                    throw new MissingSubnodeException();
                }
            }
        }

        public static void RemoveExternalProperty(HeapOnNode heap, SubnodeBTree subnodeBTree, HeapOrNodeID heapOrNodeID)
        {
            if (!heapOrNodeID.IsEmpty)
            {
                if (heapOrNodeID.IsHeapID)
                {
                    heap.RemoveItemFromHeap(heapOrNodeID.HeapID);
                }
                else
                {
                    DataTree dataTree = subnodeBTree.GetSubnode(heapOrNodeID.NodeID).DataTree;
                    dataTree.Delete();
                    subnodeBTree.DeleteSubnodeEntry(heapOrNodeID.NodeID);
                }
            }
        }

        public static HeapOrNodeID StoreExternalProperty(PSTFile file, HeapOnNode heap, ref SubnodeBTree subnodeBTree, byte[] propertyBytes)
        {
            return StoreExternalProperty(file, heap, ref subnodeBTree, new HeapOrNodeID(HeapID.EmptyHeapID), propertyBytes);
        }

        /// <param name="subnodeBTree">Note: We use ref, this way we are able to create a new subnode BTree and update the subnodeBTree the caller provided</param>
        /// <param name="heapOrNodeID">Existing HeapOrNodeID</param>
        public static HeapOrNodeID StoreExternalProperty(PSTFile file, HeapOnNode heap, ref SubnodeBTree subnodeBTree, HeapOrNodeID heapOrNodeID, byte[] propertyBytes)
        {
            // We should avoid storing items with length of 0, because those are consideref freed, and could be repurposed
            if (propertyBytes.Length == 0)
            {
                RemoveExternalProperty(heap, subnodeBTree, heapOrNodeID);
                return new HeapOrNodeID(HeapID.EmptyHeapID);
            }

            if (heapOrNodeID.IsHeapID) // if HeapOrNodeID is empty then IsHeapID == true
            {
                if (propertyBytes.Length <= HeapOnNode.MaximumAllocationLength)
                {
                    if (heapOrNodeID.IsEmpty)
                    {
                        return new HeapOrNodeID(heap.AddItemToHeap(propertyBytes));
                    }
                    else
                    {
                        return new HeapOrNodeID(heap.ReplaceHeapItem(heapOrNodeID.HeapID, propertyBytes));
                    }
                }
                else // old data (if exist) is stored on heap, but new data needs a subnode
                {
                    if (!heapOrNodeID.IsEmpty)
                    {
                        heap.RemoveItemFromHeap(heapOrNodeID.HeapID);
                    }

                    if (subnodeBTree == null)
                    {
                        subnodeBTree = new SubnodeBTree(file);
                    }
                    DataTree dataTree = new DataTree(file);
                    dataTree.AppendData(propertyBytes);
                    dataTree.SaveChanges();

                    NodeID subnodeID = file.Header.AllocateNextNodeID(NodeTypeName.NID_TYPE_LTP);
                    subnodeBTree.InsertSubnodeEntry(subnodeID, dataTree, null);

                    return new HeapOrNodeID(subnodeID);
                }
            }
            else // old data is stored in a subnode
            {
                Subnode subnode = subnodeBTree.GetSubnode(heapOrNodeID.NodeID);
                if (subnode.DataTree != null)
                {
                    subnode.DataTree.Delete();
                }
                subnode.DataTree = new DataTree(subnodeBTree.File);
                subnode.DataTree.AppendData(propertyBytes);
                subnode.SaveChanges(subnodeBTree);
                return new HeapOrNodeID(heapOrNodeID.NodeID);
            }
        }
    }
}
