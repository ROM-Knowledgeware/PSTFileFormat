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
    // "External" (as in not internal) node, with a unique NID within the Node database
    public class PSTNode : Node
    {
        private NodeID m_nodeID;
        
        public PSTNode(PSTFile file, NodeID nodeID, DataTree dataTree, SubnodeBTree subnodeBTree) : base(file, dataTree, subnodeBTree)
        {
            m_nodeID = nodeID;
        }

        public PSTNode(PSTNode node) : base(node.File, node.DataTree, node.SubnodeBTree)
        {
            m_nodeID = node.NodeID;
        }

        /// <summary>
        /// The Node BTree will be updated to point to the new data tree and subnode BTree
        /// </summary>
        public override void SaveChanges()
        {
            base.SaveChanges();

            NodeBTreeEntry entry = File.FindNodeEntryByNodeID(m_nodeID.Value);
            ulong dataTreeRootBlockID = 0;
            if (DataTree != null && DataTree.RootBlock != null)
            {
                dataTreeRootBlockID = DataTree.RootBlock.BlockID.Value;
            }

            ulong subnodeBTreeRootBlockID = 0;
            if (SubnodeBTree != null && SubnodeBTree.RootBlock != null)
            { 
                subnodeBTreeRootBlockID = SubnodeBTree.RootBlock.BlockID.Value;
            }

            if (entry.bidData.Value != dataTreeRootBlockID ||
                entry.bidSub.Value != subnodeBTreeRootBlockID)
            {
                File.NodeBTree.UpdateNodeEntry(m_nodeID, DataTree, SubnodeBTree);
            }
        }

        public override void Delete()
        {
            base.Delete();
            File.NodeBTree.DeleteNodeEntry(this.NodeID);
        }

        public NodeID NodeID
        {
            get
            {
                return m_nodeID;
            }
        }

        public NodeID ParentNodeID
        { 
            get
            {
                NodeBTreeEntry entry = File.NodeBTree.FindNodeEntryByNodeID(m_nodeID.Value);
                if (entry != null)
                {
                    return entry.nidParent;
                }
                else
                {
                    return null;
                }
            }
        }

        public static PSTNode GetPSTNode(PSTFile file, NodeID nodeID)
        {
            NodeBTreeEntry entry = file.FindNodeEntryByNodeID(nodeID.Value);
            if (entry != null)
            {
                DataTree dataTree = null;
                if (entry.bidData.Value != 0)
                {
                    Block rootDataBlock = file.FindBlockByBlockID(entry.bidData);
                    dataTree = new DataTree(file, rootDataBlock);
                }

                SubnodeBTree subnodeBTree = null;
                if (entry.bidSub.Value != 0)
                {
                    Block rootSubnodeBlock = file.FindBlockByBlockID(entry.bidSub);
                    subnodeBTree = new SubnodeBTree(file, rootSubnodeBlock);
                }
                return new PSTNode(file, nodeID, dataTree, subnodeBTree);
            }
            else
            {
                return null;
            }
        }
    }
}
