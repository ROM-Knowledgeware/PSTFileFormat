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
    public class Subnode : Node
    {
        private NodeID m_subnodeID;

        public Subnode(PSTFile file, NodeID subnodeID, DataTree dataTree, SubnodeBTree subnodeBTree)
            : base(file, dataTree, subnodeBTree)
        {
            m_subnodeID = subnodeID;
        }

        public NodeID SubnodeID
        {
            get
            {
                return m_subnodeID;
            }
        }

        public void SaveChanges(SubnodeBTree parentSubnodeBTree)
        {
            SaveChanges();
            parentSubnodeBTree.UpdateSubnodeEntry(m_subnodeID, this.DataTree, this.SubnodeBTree);
        }

        /// <summary>
        /// The entry will be removed from the parent subnode-BTree
        /// </summary>
        public void Delete(SubnodeBTree parentSubnodeBTree)
        {
            Delete();
            parentSubnodeBTree.DeleteSubnodeEntry(m_subnodeID);
        }

        public static Subnode GetSubnode(PSTFile file, SubnodeLeafEntry entry)
        { 
            DataTree dataTree = null;
            if (entry.bidData.Value != 0)
            {
                Block rootDataBlock = file.FindBlockByBlockID(entry.bidData);
                if (rootDataBlock == null)
                {
                    throw new Exception("Cannot get subnode: missing data tree root block");
                }
                dataTree = new DataTree(file, rootDataBlock);
            }

            SubnodeBTree subnodeBTree = null;
            if (entry.bidSub.Value != 0)
            {
                Block rootSubnodeBlock = file.FindBlockByBlockID(entry.bidSub);
                if (rootSubnodeBlock == null)
                {
                    throw new Exception("Missing Subnode BTree Root Block");
                }
                subnodeBTree = new SubnodeBTree(file, rootSubnodeBlock);
            }
            return new Subnode(file, entry.nid, dataTree, subnodeBTree);
        }
    }
}
