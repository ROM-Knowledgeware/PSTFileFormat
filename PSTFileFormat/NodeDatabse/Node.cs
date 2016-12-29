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
    public abstract class Node
    {
        private PSTFile m_file;
        private DataTree m_dataTree;
        private SubnodeBTree m_subnodeBTree;

        private HeapOnNode m_heap; // The heap is buffered, so we use a single reference
        private NamedPropertyContext m_propertyContext; // The BTH leaves are cached, so we use a single reference

        public Node(PSTFile file, DataTree dataTree, SubnodeBTree subnodeBTree)
        {
            m_file = file;
            m_dataTree = dataTree;
            m_subnodeBTree = subnodeBTree;
        }

        /// <summary>
        /// The caller must update its reference to point to the new data tree and subnode B-tree
        /// </summary>
        public virtual void SaveChanges()
        {
            if (!m_file.IsSavingChanges)
            {
                throw new Exception("Implementer must call PSTFile.BeginSavingChanges() before saving changes");
            }

            if (m_propertyContext != null)
            {
                m_propertyContext.FlushToDataTree();
            }

            if (m_dataTree != null)
            {
                m_dataTree.SaveChanges();
            }

            if (m_subnodeBTree != null)
            {
                m_subnodeBTree.SaveChanges();
            }
        }

        public virtual void Delete()
        {
            if (!m_file.IsSavingChanges)
            {
                throw new Exception("Implementer must call PSTFile.BeginSavingChanges() before saving changes");
            }

            if (m_dataTree != null)
            {
                m_dataTree.Delete();
                m_dataTree = null;
            }

            if (m_subnodeBTree != null)
            {
                m_subnodeBTree.Delete();
                m_subnodeBTree = null;
            }
        }

        public void CreateSubnodeBTreeIfNotExist()
        {
            if (m_subnodeBTree == null)
            {
                m_subnodeBTree = new SubnodeBTree(m_file);
            }
        }

        public PSTFile File
        {
            get
            {
                return m_file;
            }
        }

        [Obsolete]
        public BlockID RootDataBlockID
        {
            get
            {
                if (m_dataTree != null)
                {
                    return m_dataTree.RootBlock.BlockID;
                }
                else
                {
                    return null;
                } 
            }
        }

        [Obsolete]
        public BlockID RootSubnodeBlockID
        {
            get
            {
                if (m_subnodeBTree != null)
                {
                    return m_subnodeBTree.RootBlock.BlockID;
                }
                else
                {
                    return null;
                }
            }
        }

        [Obsolete]
        public Block RootDataBlock
        {
            get
            {
                if (m_dataTree != null)
                {
                    return m_dataTree.RootBlock;
                }
                else
                {
                    return null;
                }
            }
        }

        [Obsolete]
        public Block RootSubnodeBlock
        {
            get
            {
                if (m_subnodeBTree != null)
                {
                    return m_subnodeBTree.RootBlock;
                }
                else
                {
                    return null;
                }
            }
        }

        public DataTree DataTree
        {
            get
            {
                return m_dataTree;
            }
            set
            {
                m_dataTree = value;
            }
        }

        public SubnodeBTree SubnodeBTree
        {
            get
            {
                return m_subnodeBTree;
            }
            set
            {
                m_subnodeBTree = value;
            }
        }

        public HeapOnNode Heap
        {
            get
            {
                if (m_heap == null)
                {
                    if (m_dataTree != null)
                    {
                        m_heap = new HeapOnNode(m_dataTree);
                    }
                }
                return m_heap;
            }
        }

        public NamedPropertyContext PC
        {
            get
            {
                if (m_propertyContext == null)
                {
                    HeapOnNode heap = this.Heap;
                    if (heap != null)
                    {
                        if (heap.HeapHeader.bClientSig == OnHeapTypeName.bTypePC)
                        {
                            // We want to share m_subnodeBTree with the PC, so we must not leave it as null,
                            // (if both this Node and the PC will have it set to null, then when a new subnode BTree will be created,
                            //  only one of the references will point to the new subnode BTree)
                            if (m_subnodeBTree == null)
                            {
                                // To prevent the creation of a new leaf block when not necessary, we set the root block to null.
                                m_subnodeBTree = new SubnodeBTree(m_file, null);
                            }
                            m_propertyContext = new NamedPropertyContext(heap, m_subnodeBTree, m_file.NameToIDMap);
                        }
                    }
                }
                return m_propertyContext;
            }
        }

        public TableContext TableContext
        {
            get
            {
                HeapOnNode heap = this.Heap;
                if (heap != null)
                {
                    if (heap.HeapHeader.bClientSig == OnHeapTypeName.bTypeTC)
                    {
                        // We want to share m_subnodeBTree with the TC, so we must not leave it as null,
                        if (m_subnodeBTree == null)
                        {
                            // To prevent the creation of a new leaf block when not necessary, we set the root block to null.
                            m_subnodeBTree = new SubnodeBTree(m_file, null);
                        }
                        TableContext tc = new TableContext(heap, m_subnodeBTree);
                        return tc;
                    }
                }
                return null;
            }
        }

        public NamedTableContext NamedTableContext
        {
            get
            {
                TableContext tc = this.TableContext;
                if (tc != null)
                {
                    return new NamedTableContext(tc, m_file.NameToIDMap);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
