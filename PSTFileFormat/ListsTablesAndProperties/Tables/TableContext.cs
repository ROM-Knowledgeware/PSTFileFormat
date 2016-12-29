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
    public class TableContext
    {
        private HeapOnNode m_heap;
        private SubnodeBTree m_subnodeBTree;
        private TableContextInfo m_tcInfo;
        private List<TableContextRowID> m_rowIndex = new List<TableContextRowID>();

        private int m_rowsPerBlock;

        private Subnode m_subnodeRows; // for buffering purposes

        public TableContext(HeapOnNode heap, SubnodeBTree subnodeBTree)
        {
            m_heap = heap;
            m_subnodeBTree = subnodeBTree;
            m_tcInfo = new TableContextInfo(m_heap.GetHeapItem(m_heap.HeapHeader.hidUserRoot));

            BTreeOnHeap<TableContextRowID> bTreeOnHeap = new BTreeOnHeap<TableContextRowID>(m_heap, m_tcInfo.hidRowIndex);
            if (bTreeOnHeap.BTreeHeader.hidRoot.hidIndex > 0) // hidRoot is set to zero if the BTH is empty.
            {
                m_rowIndex = bTreeOnHeap.GetAll();
                m_rowIndex.Sort(TableContextRowID.CompareByRowIndex);
            }

            m_rowsPerBlock = (int)Math.Floor((double)DataBlock.MaximumDataLength / RowLength);
        }

        public void SaveChanges(NodeID pstNodeID)
        {
            SaveChanges();
            // We can optimize and only update when root block is changed.
            // Note however that multiple SaveChanges() may be called before this method.
            File.NodeBTree.UpdateNodeEntry(pstNodeID, this.DataTree, m_subnodeBTree);
        }

        public void SaveChanges(SubnodeBTree parentSubnodeBTree, NodeID subnodeID)
        {
            SaveChanges();
            // We can optimize and only update when root block is changed.
            // Note however that multiple SaveChanges() may be called before this method.
            parentSubnodeBTree.UpdateSubnodeEntry(subnodeID, this.DataTree, this.SubnodeBTree);
        }

        /// <summary>
        /// The caller must update its reference to point to the new data tree and subnode B-tree
        /// </summary>
        public void SaveChanges()
        {
            if (m_subnodeRows != null)
            {
                m_subnodeRows.SaveChanges();
                m_subnodeBTree.UpdateSubnodeEntry(m_tcInfo.hnidRows.NodeID, m_subnodeRows.DataTree, m_subnodeRows.SubnodeBTree);
            }

            m_heap.SaveChanges();
            if (m_subnodeBTree != null)
            {
                m_subnodeBTree.SaveChanges();
            }
        }

        public int GetRowIndex(uint rowID)
        {
            foreach (TableContextRowID entry in m_rowIndex)
            {
                if (entry.dwRowID == rowID)
                {
                    return (int)entry.dwRowIndex;
                }
            }
            return -1;
        }

        public uint GetRowID(int rowIndex)
        {
            return m_rowIndex[rowIndex].dwRowID;
        }

        #region Column Related
        public int GetColumnIndexByPropertyTag(PropertyID propertyID, PropertyTypeName propertyType)
        {
            int columnIndex = FindColumnIndexByPropertyTag(propertyID, propertyType);
            if (columnIndex == -1)
            { 
                throw new Exception(String.Format("Column {0} does not belong to table", GetPropertyIDString((ushort)propertyID)));
            }
            return columnIndex;
        }

        public int FindColumnIndexByPropertyTag(PropertyID propertyID, PropertyTypeName propertyType)
        {
            for (int index = 0; index < m_tcInfo.rgTCOLDESC.Count; index++)
            {
                TableColumnDescriptor descriptor = m_tcInfo.rgTCOLDESC[index];
                if (descriptor.PropertyID == propertyID &&
                    descriptor.PropertyType == propertyType)
                {
                    return index;
                }
            }
            return -1;
        }

        public bool ContainsPropertyColumn(PropertyID propertyID, PropertyTypeName propertyTypeName)
        {
            int index = FindColumnIndexByPropertyTag(propertyID, propertyTypeName);
            return (index >= 0);
        }

        /// <returns>True if property column was added</returns>
        public bool AddPropertyColumnIfNotExist(PropertyID propertyID, PropertyTypeName propertyTypeName)
        {
            if (!ContainsPropertyColumn(propertyID, propertyTypeName))
            {
                AddPropertyColumn(propertyID, propertyTypeName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// This method must be called before adding the new row
        /// </summary>
        private List<byte[]> GetRedistributedRows(ushort newColumnOffset, int newColumnLength)
        {
            List<byte[]> rows = new List<byte[]>();
            for (int index = 0; index < RowCount; index++)
            {
                rows.Add(GetRowBytes(index));
            }

            int newLength = RowLength + newColumnLength;
            if (m_tcInfo.ColumnCount % 8 == 0)
            {
                // we need another byte for the iBit
                newLength += 1;
            }

            for (int index = 0; index < rows.Count; index++)
            {
                byte[] rowBytes = rows[index];
                byte[] data = new byte[newLength];
                Array.Copy(rowBytes, 0, data, 0, newColumnOffset);
                Array.Copy(rowBytes, newColumnOffset, data, newColumnOffset + newColumnLength, rowBytes.Length - newColumnOffset);
                rows[index] = data;
            }

            return rows;
        }

        /// <summary>
        /// Add column to an empty table context.
        /// If this is a Contents Table, the caller should call UpdateMessage() afterwards.
        /// Similarly, for attachment table, the caller should call UpdateAttachment().
        /// </summary>
        public void AddPropertyColumn(PropertyID propertyID, PropertyTypeName propertyType)
        {
            TableColumnDescriptor newColumnDescriptor = new TableColumnDescriptor();
            newColumnDescriptor.PropertyID = propertyID;
            newColumnDescriptor.PropertyType = propertyType;
            newColumnDescriptor.iBit = (byte)m_tcInfo.ColumnCount;
            newColumnDescriptor.cbData = (byte)GetPropertyDataLength(propertyType);

            // Set the ibData:
            // http://social.msdn.microsoft.com/Forums/en-US/os_binaryfile/thread/a5f9c653-40f5-4638-85d3-00c54607d984/
            // PidTagLtpRowId and PidTagLtpRowVer must not be relocated
            if (newColumnDescriptor.DataLengthGroup == TableContextInfo.TCI_4b)
            {
                newColumnDescriptor.ibData = m_tcInfo.rgib[TableContextInfo.TCI_4b];
            }
            else if (newColumnDescriptor.DataLengthGroup == TableContextInfo.TCI_2b)
            {
                newColumnDescriptor.ibData = m_tcInfo.rgib[TableContextInfo.TCI_2b];
            }
            else
            {
                newColumnDescriptor.ibData = m_tcInfo.rgib[TableContextInfo.TCI_1b];
            }

            // We call GetRedistributedRows() before adding the new column:
            List<byte[]> rows = GetRedistributedRows(newColumnDescriptor.ibData, newColumnDescriptor.cbData);

            // add the new column
            m_tcInfo.rgTCOLDESC.Add(newColumnDescriptor);

            // redistribute column descriptions
            ushort offset = (ushort)(newColumnDescriptor.ibData + newColumnDescriptor.cbData);
            for (int groupIndex = newColumnDescriptor.DataLengthGroup + 1; groupIndex < 3; groupIndex++)
            {
                for (int index = 0; index < m_tcInfo.rgTCOLDESC.Count; index++)
                {
                    TableColumnDescriptor descriptor = m_tcInfo.rgTCOLDESC[index];

                    if (groupIndex == descriptor.DataLengthGroup)
                    {
                        // changes to descriptor will be saved when calling UpdateTableContextInfo() 
                        descriptor.ibData = offset;
                        offset += descriptor.cbData;
                    }
                }
            }

            // update the group ending offset
            m_tcInfo.UpdateDataLayout();
            m_rowsPerBlock = (int)Math.Floor((double)DataBlock.MaximumDataLength / RowLength);
            
            // Update the rows data
            if (!m_tcInfo.hnidRows.IsEmpty)
            {
                if (m_tcInfo.hnidRows.IsHeapID)
                {
                    m_heap.RemoveItemFromHeap(m_tcInfo.hnidRows.HeapID);
                    CreateSubnodeForRows();
                }
                else
                {
                    if (m_subnodeRows == null)
                    {
                        m_subnodeRows = m_subnodeBTree.GetSubnode(m_tcInfo.hnidRows.NodeID);
                    }
                    m_subnodeRows.Delete(); // this will set the subnode data-tree to null
                    // New data tree will be created when the first row will be added
                    m_subnodeBTree.UpdateSubnodeEntry(m_tcInfo.hnidRows.NodeID, null, null);
                }

                for(int index = 0; index < rows.Count; index++)
                {
                    AddRowToSubnode(index, rows[index]);
                }
            }

            UpdateTableContextInfo();
        }
        #endregion

        #region Row Related
        public byte[] GetRowBytes(int rowIndex)
        {
            int rowLength = RowLength;
            byte[] result = new byte[rowLength];

            // hnidRows is set to zero if the TC contains no rows
            if (m_tcInfo.hnidRows.IsEmpty)
            {
                throw new ArgumentException("Invalid row index, the table context is empty");
            }
            else if (m_tcInfo.hnidRows.IsHeapID)
            {
                // the row matrix is stored in the data tree
                byte[] rows = m_heap.GetHeapItem(m_tcInfo.hnidRows.HeapID);
                int offset = (int)rowIndex * rowLength;
                Array.Copy(rows, offset, result, 0, rowLength);
                return result;
            }
            else
            {
                // indicates that the row matrix is stored in the subnode block, and the NID is the local NID under the subnode BTree
                if (m_subnodeRows == null)
                {
                    m_subnodeRows = m_subnodeBTree.GetSubnode(m_tcInfo.hnidRows.NodeID);
                }
                int blockIndex = (int)(rowIndex / m_rowsPerBlock);
                int inBlockRowIndex = (int)(rowIndex % m_rowsPerBlock);
                DataBlock block = m_subnodeRows.DataTree.GetDataBlock(blockIndex);
                int offset = inBlockRowIndex * rowLength;

                Array.Copy(block.Data, offset, result, 0, rowLength);
                return result;
            }
        }

        public void SetRowBytes(int rowIndex, byte[] rowBytes)
        {
            if (rowIndex >= RowCount)
            {
                throw new ArgumentException("Invalid rowIndex");
            }

            int rowLength = this.RowLength;

            if (m_tcInfo.hnidRows.IsHeapID)
            {
                // the RowMatrix is stored in the data tree
                byte[] rows = m_heap.GetHeapItem(m_tcInfo.hnidRows.HeapID);
                int rowOffset = (int)rowIndex * rowLength;
                Array.Copy(rowBytes, 0, rows, rowOffset, rowLength);

                HeapID oldHeapID = m_tcInfo.hnidRows.HeapID;
                // this will replace the item in place (as they have the same size)
                m_heap.ReplaceHeapItem(oldHeapID, rows);
            }
            else
            {
                // indicates that the item is stored in the subnode block, and the NID is the local NID under the subnode BTree
                NodeID rowsNodeID = m_tcInfo.hnidRows.NodeID;
                if (m_subnodeRows == null)
                {
                    m_subnodeRows = m_subnodeBTree.GetSubnode(rowsNodeID);
                }
                int blockIndex = (int)(rowIndex / m_rowsPerBlock);
                int inBlockRowIndex = (int)(rowIndex % m_rowsPerBlock);
                DataBlock block = m_subnodeRows.DataTree.GetDataBlock(blockIndex);
                int offset = inBlockRowIndex * rowLength;

                Array.Copy(rowBytes, 0, block.Data, offset, rowLength);
                m_subnodeRows.DataTree.UpdateDataBlock(blockIndex, block.Data);
            }
        }

        public void DeleteRow(int rowIndex)
        {
            if (rowIndex < RowCount)
            {
                // free heap items that belong to this row
                foreach (TableColumnDescriptor column in m_tcInfo.rgTCOLDESC)
                {
                    if (column.IsStoredExternally)
                    { 
                        int columnIndex = GetColumnIndexByPropertyTag(column.PropertyID, column.PropertyType);
                        RemoveExternalProperty(rowIndex, columnIndex);
                    }
                }

                if (rowIndex == RowCount - 1)
                {
                    // last row is being deleted
                    TrimLastRow();
                    DeleteLastRowFromRowIndex();
                }
                else
                { 
                    // we will put the last row instead of the row we wish to delete, and trim the last row
                    byte[] rowBytes = GetRowBytes(RowCount - 1);
                    SetRowBytes(rowIndex, rowBytes);

                    SwitchRowsInRowIndex(rowIndex, RowCount - 1);
                    TrimLastRow();
                    DeleteLastRowFromRowIndex();
                }
            }
        }

        private void TrimLastRow()
        {
            if (m_tcInfo.hnidRows.IsHeapID)
            {
                TrimLastRowFromHeap();
            }
            else
            {
                TrimLastRowFromNode();
            }
        }

        private void TrimLastRowFromHeap()
        {
            if (RowCount == 1)
            {
                m_heap.RemoveItemFromHeap(m_tcInfo.hnidRows.HeapID);
                m_tcInfo.hnidRows = new HeapOrNodeID(HeapID.EmptyHeapID);
                UpdateTableContextInfo();
            }
            else
            {
                byte[] oldRows = m_heap.GetHeapItem(m_tcInfo.hnidRows.HeapID);
                byte[] newRows = new byte[oldRows.Length - RowLength];
                Array.Copy(oldRows, newRows, newRows.Length);
                // we will always replace in place (new item is smaller than the old item)
                m_heap.ReplaceHeapItem(m_tcInfo.hnidRows.HeapID, newRows);
            }
        }

        private void TrimLastRowFromNode()
        {
            int rowIndex = RowCount - 1;
            if (m_subnodeRows == null)
            {
                NodeID rowsNodeID = m_tcInfo.hnidRows.NodeID;
                m_subnodeRows = m_subnodeBTree.GetSubnode(rowsNodeID);
            }
            int blockIndex = (int)(rowIndex / m_rowsPerBlock);
            int rowIndexInBlock = (int)(rowIndex % m_rowsPerBlock);
            if (rowIndexInBlock == 0)
            {
                if (blockIndex > 0)
                {
                    m_subnodeRows.DataTree.DeleteLastDataBlock();
                }
                else
                {
                    m_subnodeRows.Delete(m_subnodeBTree);
                    m_subnodeRows = null;
                    m_tcInfo.hnidRows = new HeapOrNodeID(HeapID.EmptyHeapID);
                    UpdateTableContextInfo();
                }
            }
            else
            {
                DataBlock dataBlock = m_subnodeRows.DataTree.GetDataBlock(blockIndex);
                byte[] newRows = new byte[rowIndexInBlock * RowLength];
                Array.Copy(dataBlock.Data, newRows, newRows.Length);
                m_subnodeRows.DataTree.UpdateDataBlock(blockIndex, newRows);
            }
        }
        #endregion

        #region Cell Related
        public byte[] GetInternalCellBytes(int rowIndex, int columnIndex)
        {
            TableColumnDescriptor columnDescriptor = m_tcInfo.rgTCOLDESC[columnIndex];
            byte[] rowBytes = GetRowBytes(rowIndex);
            bool inUse = IsCellInUse(columnDescriptor, rowBytes);
            if (!inUse)
            {
                return null;
            }
            else
            {
                int cellLength = columnDescriptor.cbData;
                byte[] cellBytes = new byte[cellLength];
                int cellOffset = columnDescriptor.ibData;
                Array.Copy(rowBytes, cellOffset, cellBytes, 0, cellBytes.Length);
                return cellBytes;
            }
        }

        private void SetInternalCellBytes(int rowIndex, int columnIndex, byte[] cellBytes)
        {
            TableColumnDescriptor columnDescriptor = m_tcInfo.rgTCOLDESC[columnIndex];
            int cellLength = columnDescriptor.cbData;
            if (cellBytes.Length == cellLength)
            {
                byte[] rowBytes = GetRowBytes(rowIndex);
                int cellOffset = columnDescriptor.ibData;
                Array.Copy(cellBytes, 0, rowBytes, cellOffset, cellBytes.Length);

                // update CEB:
                UpdateCellExistenceBlock(columnDescriptor, rowBytes, true);

                SetRowBytes(rowIndex, rowBytes);
            }
            else
            {
                throw new InvalidPropertyException("Invalid cell length");
            }
        }

        public bool IsCellInUse(TableColumnDescriptor columnDescriptor, byte[] rowBytes)
        {
            int cebByteOffset = m_tcInfo.CellExistenceBlockStartOffset + columnDescriptor.iBit / 8;
            // from MSB to LSB (as suggested by MS-PST, page 68)
            int cebBitOffset = 7 - columnDescriptor.iBit % 8;
            return (rowBytes[cebByteOffset] & (0x01 << cebBitOffset)) > 0;
        }

        private void UpdateCellExistenceBlock(TableColumnDescriptor columnDescriptor, byte[] rowBytes, bool isInUse)
        {
            int cebByteOffset = m_tcInfo.CellExistenceBlockStartOffset + columnDescriptor.iBit / 8;
            // from MSB to LSB (as suggested by MS-PST, page 68)
            int cebBitOffset = 7 - columnDescriptor.iBit % 8;

            if (isInUse)
            {
                rowBytes[cebByteOffset] |= (byte)(0x01 << cebBitOffset);
            }
            else
            {
                rowBytes[cebByteOffset] &= (byte)~(0x01 << cebBitOffset);
            }
        }

        /// <returns>Row index</returns>
        public int AddRow(uint rowID)
        {
            byte[] rowBytes = new byte[RowLength];
            return AddRow(rowID, rowBytes);
        }

        private void CreateSubnodeForRows()
        {
            if (m_subnodeBTree == null)
            {
                m_subnodeBTree = new SubnodeBTree(this.File);
            }

            // We don't have to use a unique node ID for a subnode, but we can.
            NodeID nodeID = File.Header.AllocateNextNodeID(NodeTypeName.NID_TYPE_LTP);
            // Data tree will be created when first row will be added
            m_subnodeRows = new Subnode(File, nodeID, null, null);
            m_subnodeBTree.InsertSubnodeEntry(nodeID, null, null);

            // update the Table Context Info structure to point to the new rows
            m_tcInfo.hnidRows = new HeapOrNodeID(m_subnodeRows.SubnodeID);
            UpdateTableContextInfo();
        }

        /// <summary>
        /// New rows are always added at the end
        /// </summary>
        /// <returns>Row index</returns>
        public int AddRow(uint rowID, byte[] newRowBytes)
        {
            int rowIndex = m_rowIndex.Count;

            if (m_tcInfo.hnidRows.IsEmpty)
            {
                m_tcInfo.hnidRows = new HeapOrNodeID(m_heap.AddItemToHeap(newRowBytes));
                UpdateTableContextInfo();
            }
            else if (m_tcInfo.hnidRows.IsHeapID)
            {
                byte[] oldRows = m_heap.GetHeapItem(m_tcInfo.hnidRows.HeapID);
                if (oldRows.Length + RowLength <= HeapOnNode.MaximumAllocationLength)
                {
                    byte[] newRows = new byte[oldRows.Length + RowLength];
                    Array.Copy(oldRows, newRows, oldRows.Length);
                    Array.Copy(newRowBytes, 0, newRows, oldRows.Length, RowLength);

                    HeapID oldHeapID = m_tcInfo.hnidRows.HeapID;
                    HeapID newHeapID = m_heap.ReplaceHeapItem(oldHeapID, newRows);
                    if (oldHeapID.Value != newHeapID.Value)
                    {
                        // update the Table Context Info structure to point to the new rows
                        m_tcInfo.hnidRows = new HeapOrNodeID(newHeapID);
                        UpdateTableContextInfo();
                    }
                }
                else
                { 
                    // we must move the rows from the heap to a subnode
                    byte[] rows = Heap.GetHeapItem(m_tcInfo.hnidRows.HeapID);

                    // remove the old rows from the heap
                    m_heap.RemoveItemFromHeap(m_tcInfo.hnidRows.HeapID);

                    CreateSubnodeForRows();

                    for (int index = 0; index < RowCount; index++)
                    {
                        byte[] rowBytes = new byte[RowLength];
                        Array.Copy(rows, index * RowLength, rowBytes, 0, RowLength);
                        AddRowToSubnode(index, rowBytes);
                    }
                    // add the new row
                    AddRowToSubnode(rowIndex, newRowBytes);
                }
            }
            else
            {
                // indicates that the item is stored in the subnode block, and the NID is the local NID under the subnode BTree
                AddRowToSubnode(rowIndex);
            }

            AddRowToRowIndex(rowID, rowIndex);

            return rowIndex;
        }

        private void AddRowToRowIndex(uint rowID, int rowIndex)
        {
            TableContextRowID newRowID = new TableContextRowID(rowID, (uint)rowIndex);
            AddRowToRowIndex(newRowID);
        }

        private void AddRowToRowIndex(TableContextRowID rowID)
        {
            // Add the row to the list
            m_rowIndex.Add(rowID);
            // add the row to the BTH
            BTreeOnHeap<TableContextRowID> bTreeOnHeap = new BTreeOnHeap<TableContextRowID>(m_heap, m_tcInfo.hidRowIndex);
            bTreeOnHeap.AddRecord(rowID);
        }

        private void SwitchRowsInRowIndex(int rowIndex1, int rowIndex2)
        {
            TableContextRowID record1 = m_rowIndex[rowIndex1];
            TableContextRowID record2 = m_rowIndex[rowIndex2];
            uint temp = record1.dwRowIndex;
            record1.dwRowIndex = record2.dwRowIndex;
            record2.dwRowIndex = temp;

            m_rowIndex[rowIndex1] = record2;
            m_rowIndex[rowIndex2] = record1;
            
            BTreeOnHeap<TableContextRowID> bTreeOnHeap = new BTreeOnHeap<TableContextRowID>(m_heap, m_tcInfo.hidRowIndex);
            bTreeOnHeap.UpdateRecord(record1);
            bTreeOnHeap.UpdateRecord(record2);
        }

        private void DeleteLastRowFromRowIndex()
        {
            uint rowID = m_rowIndex[m_rowIndex.Count - 1].dwRowID;
            m_rowIndex.RemoveAt(m_rowIndex.Count - 1);

            BTreeOnHeap<TableContextRowID> bTreeOnHeap = new BTreeOnHeap<TableContextRowID>(m_heap, m_tcInfo.hidRowIndex);
            bTreeOnHeap.RemoveRecord(LittleEndianConverter.GetBytes(rowID));
        }

        private void AddRowToSubnode(int rowIndex)
        {
            byte[] rowBytes = new byte[RowLength];
            AddRowToSubnode(rowIndex, rowBytes);
        }

        private void AddRowToSubnode(int rowIndex, byte[] rowBytes)
        {
            if (rowBytes.Length != RowLength)
            {
                throw new ArgumentException("Invalid row bytes length");
            }

            if (m_subnodeRows == null)
            {
                NodeID rowsNodeID = m_tcInfo.hnidRows.NodeID;
                m_subnodeRows = m_subnodeBTree.GetSubnode(rowsNodeID);
            }
            int blockIndex = (int)(rowIndex / m_rowsPerBlock);
            int rowIndexInBlock = (int)(rowIndex % m_rowsPerBlock);
            if (blockIndex == 0 && rowIndexInBlock == 0)
            {
                if (m_subnodeRows.DataTree == null)
                {
                    // create new data block
                    m_subnodeRows.DataTree = new DataTree(this.File);
                    m_subnodeRows.DataTree.UpdateDataBlock(0, rowBytes);
                }
                else
                {
                    throw new Exception("Invalid TC subnode");
                }
            }
            else if (rowIndexInBlock == 0) // add data block
            {
                if (m_subnodeRows.DataTree != null)
                {
                    m_subnodeRows.DataTree.AddDataBlock(rowBytes);
                }
                else
                {
                    throw new Exception("Invalid TC subnode");
                }
            }
            else
            {
                DataBlock block = m_subnodeRows.DataTree.GetDataBlock(blockIndex);
                int offset = rowIndexInBlock * RowLength;

                byte[] oldRows = block.Data;
                byte[] newRows;
                newRows = new byte[oldRows.Length + rowBytes.Length];
                Array.Copy(oldRows, newRows, oldRows.Length);
                Array.Copy(rowBytes, 0, newRows, oldRows.Length, rowBytes.Length);

                block.Data = newRows;
                m_subnodeRows.DataTree.UpdateDataBlock(blockIndex, block.Data);
            }
        }
        #endregion

        #region Get Property
        public Nullable<bool> GetBooleanProperty(int rowIndex, PropertyID propertyID)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypBoolean);
            return GetBooleanProperty(rowIndex, columnIndex);
        }

        public Nullable<bool> GetBooleanProperty(int rowIndex, int columnIndex)
        {
            byte[] cellValue = GetPropertyValue(rowIndex, columnIndex);
            if (cellValue == null)
            {
                return null;
            }
            else
            {
                return cellValue[0] == 1;
            }
        }

        public Nullable<short> GetInt16Property(int rowIndex, PropertyID propertyID)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypInteger16);
            return GetInt16Property(rowIndex, columnIndex);
        }

        public Nullable<short> GetInt16Property(int rowIndex, int columnIndex)
        {
            byte[] cellValue = GetPropertyValue(rowIndex, columnIndex);
            if (cellValue == null)
            {
                return null;
            }
            else
            {
                return LittleEndianConverter.ToInt16(cellValue, 0);
            }
        }

        public Nullable<int> GetInt32Property(int rowIndex, PropertyID propertyID)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypInteger32);
            return GetInt32Property(rowIndex, columnIndex);
        }

        public Nullable<int> GetInt32Property(int rowIndex, int columnIndex)
        {
            byte[] cellValue = GetPropertyValue(rowIndex, columnIndex);
            if (cellValue == null)
            {
                return null;
            }
            else
            {
                return LittleEndianConverter.ToInt32(cellValue, 0);
            }
        }

        public Nullable<long> GetInt64Property(int rowIndex, PropertyID propertyID)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypInteger64);
            return GetInt64Property(rowIndex, columnIndex);
        }

        public Nullable<long> GetInt64Property(int rowIndex, int columnIndex)
        {
            byte[] cellValue = GetPropertyValue(rowIndex, columnIndex);
            if (cellValue == null)
            {
                return null;
            }
            else
            {
                return LittleEndianConverter.ToInt64(cellValue, 0);
            }
        }

        public Nullable<DateTime> GetDateTimeProperty(int rowIndex, PropertyID propertyID)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypTime);
            return GetDateTimeProperty(rowIndex, columnIndex);
        }

        public Nullable<DateTime> GetDateTimeProperty(int rowIndex, int columnIndex)
        {
            byte[] cellValue = GetPropertyValue(rowIndex, columnIndex);
            if (cellValue == null)
            {
                return null;
            }
            else
            {
                long temp = LittleEndianConverter.ToInt64(cellValue, 0);
                return DateTime.FromFileTimeUtc(temp);
            }
        }

        public string GetStringProperty(int rowIndex, PropertyID propertyID)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypString);
            return GetStringProperty(rowIndex, columnIndex);
        }

        public string GetStringProperty(int rowIndex, int columnIndex)
        {
            byte[] cellValue = GetPropertyValue(rowIndex, columnIndex);
            if (cellValue == null)
            {
                return null;
            }
            else
            {
                string result = UnicodeEncoding.Unicode.GetString(cellValue);
                return result;
            }
        }

        public byte[] GetBytesProperty(int rowIndex, PropertyID propertyID)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypBinary);
            return GetBytesProperty(rowIndex, columnIndex);
        }

        public byte[] GetBytesProperty(int rowIndex, int columnIndex)
        {
            return GetPropertyValue(rowIndex, columnIndex);
        }

        public byte[] GetPropertyValue(int rowIndex, int columnIndex)
        {
            TableColumnDescriptor columnDescriptor = m_tcInfo.rgTCOLDESC[columnIndex];
            byte[] cellBytes = GetInternalCellBytes(rowIndex, columnIndex);
            if (cellBytes == null)
            {
                return null;
            }
            else
            {
                if (columnDescriptor.IsStoredExternally)
                {
                    HeapOrNodeID heapOrNodeID = new HeapOrNodeID(cellBytes);
                    return NodeStorageHelper.GetExternalPropertyBytes(m_heap, m_subnodeBTree, heapOrNodeID);
                }
                else
                {
                    return cellBytes;
                }
            }
        }
        #endregion

        #region Set Property
        public void SetBooleanProperty(int rowIndex, PropertyID propertyID, bool value)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypBoolean);
            byte[] cellBytes = new byte[1];
            cellBytes[0] = Convert.ToByte(value);
            SetPropertyValue(rowIndex, columnIndex, cellBytes);
        }

        public void SetInt16Property(int rowIndex, PropertyID propertyID, short value)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypInteger16);
            byte[] cellBytes = LittleEndianConverter.GetBytes(value);
            SetPropertyValue(rowIndex, columnIndex, cellBytes);
        }

        public void SetInt32Property(int rowIndex, PropertyID propertyID, int value)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypInteger32);
            byte[] cellBytes = LittleEndianConverter.GetBytes(value);
            SetPropertyValue(rowIndex, columnIndex, cellBytes);
        }

        public void SetInt64Property(int rowIndex, PropertyID propertyID, long value)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypInteger64);
            byte[] cellBytes = LittleEndianConverter.GetBytes(value);
            SetPropertyValue(rowIndex, columnIndex, cellBytes);
        }

        public void SetDateTimeProperty(int rowIndex, PropertyID propertyID, DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new InvalidPropertyException("DateTime must be in UTC");
            }
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypTime);
            byte[] cellBytes = LittleEndianConverter.GetBytes(value.ToFileTimeUtc());
            SetPropertyValue(rowIndex, columnIndex, cellBytes);
        }

        /// <param name="value">set value to null to mark the row as in use</param>
        public void SetGuidProperty(int rowIndex, PropertyID propertyID, byte[] value)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypGuid);
            SetPropertyValue(rowIndex, columnIndex, value);
        }

        public void SetStringProperty(int rowIndex, PropertyID propertyID, string value)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypString);
            byte[] cellBytes = UnicodeEncoding.Unicode.GetBytes(value);
            SetPropertyValue(rowIndex, columnIndex, cellBytes);
        }

        /// <param name="value">set value to null to mark the row as in use</param>
        public void SetBytesProperty(int rowIndex, PropertyID propertyID, byte[] value)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, PropertyTypeName.PtypBinary);
            SetPropertyValue(rowIndex, columnIndex, value);
        }

        /// <param name="cellBytes">If property is external, byte[0] means empty data item</param>
        public void SetPropertyValue(int rowIndex, int columnIndex, byte[] propertyBytes)
        {
            TableColumnDescriptor columnDescriptor = m_tcInfo.rgTCOLDESC[columnIndex];
            if (columnDescriptor.IsStoredExternally)
            {
                byte[] cellBytes = GetInternalCellBytes(rowIndex, columnIndex);
                HeapOrNodeID heapOrNodeID;
                if (cellBytes != null)
                {
                    heapOrNodeID = new HeapOrNodeID(cellBytes);
                }
                else
                {
                    heapOrNodeID = new HeapOrNodeID(HeapID.EmptyHeapID);
                }

                HeapOrNodeID newHeapOrNodeID = NodeStorageHelper.StoreExternalProperty(this.File, m_heap, ref m_subnodeBTree, heapOrNodeID, propertyBytes);
                // we call SetInternalCellBytes even when oldHeapID.Value == newHeapID.Value,
                // this will make sure the CEB will be updated
                SetInternalCellBytes(rowIndex, columnIndex, LittleEndianConverter.GetBytes(newHeapOrNodeID.Value));
            }
            else
            {
                SetInternalCellBytes(rowIndex, columnIndex, propertyBytes);
            }
        }
        #endregion

        private void RemoveExternalProperty(int rowIndex, int columnIndex)
        {
            byte[] cellBytes = GetInternalCellBytes(rowIndex, columnIndex);
            if (cellBytes != null)
            {
                HeapOrNodeID heapOrNodeID = new HeapOrNodeID(cellBytes);
                NodeStorageHelper.RemoveExternalProperty(m_heap, m_subnodeBTree, heapOrNodeID);
            }
        }

        public void RemoveProperty(int rowIndex, PropertyID propertyID, PropertyTypeName propertyType)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, propertyType);
            TableColumnDescriptor columnDescriptor = m_tcInfo.rgTCOLDESC[columnIndex];
            if (columnDescriptor.IsStoredExternally)
            {
                RemoveExternalProperty(rowIndex, columnIndex);
            }
            byte[] rowBytes = GetRowBytes(rowIndex);
            UpdateCellExistenceBlock(columnDescriptor, rowBytes, false);
            SetRowBytes(rowIndex, rowBytes);
        }

        public bool IsCellInUse(int rowIndex, PropertyID propertyID, PropertyTypeName propertyType)
        {
            int columnIndex = GetColumnIndexByPropertyTag(propertyID, propertyType);
            TableColumnDescriptor columnDescriptor = m_tcInfo.rgTCOLDESC[columnIndex];
            
            byte[] rowBytes = GetRowBytes(rowIndex);
            return IsCellInUse(columnDescriptor, rowBytes);
        }

        /// <summary>
        /// For discovery purposes
        /// </summary>
        public List<string> ListTable()
        {
            List<string> result = new List<string>();
            result.Add("Number of Columns: " + this.ColumnCount);
            for (int index = 0; index < m_tcInfo.rgTCOLDESC.Count; index++)
            {
                TableColumnDescriptor descriptor = m_tcInfo.rgTCOLDESC[index];
                result.Add(String.Format("Column {0}, Property Type: {1}, PropertyName: {2}, Data Length: {3}, Offset: {4}, iBit: {5}", index, descriptor.PropertyType, GetPropertyIDString((ushort)descriptor.PropertyID), descriptor.cbData, descriptor.ibData, descriptor.iBit));
            }

            result.Add("Number of Rows: " + m_rowIndex.Count);
            result.Add("4-byte entries length: " + (m_tcInfo.rgib[TableContextInfo.TCI_4b]));
            result.Add("2-byte entries length: " + (m_tcInfo.rgib[TableContextInfo.TCI_2b] - m_tcInfo.rgib[TableContextInfo.TCI_4b]));
            result.Add("1-byte entries length: " + (m_tcInfo.rgib[TableContextInfo.TCI_1b] - m_tcInfo.rgib[TableContextInfo.TCI_2b]));
            result.Add("Row length (net): " + m_tcInfo.rgib[TableContextInfo.TCI_1b]);
            result.Add("Row length: " + this.RowLength);
            for (int rowIndex = 0; rowIndex < m_rowIndex.Count; rowIndex++)
            {
                result.Add("--------------------------------------------------------------------------------");
                result.Add("Row ID: " + m_rowIndex[rowIndex].dwRowID);
                result.Add("Data Length: " + m_rowIndex[rowIndex].DataLength);
                result.Add("Row Index: " + m_rowIndex[rowIndex].dwRowIndex);
                for (int columnIndex = 0; columnIndex < m_tcInfo.rgTCOLDESC.Count; columnIndex++)
                {
                    TableColumnDescriptor descriptor = m_tcInfo.rgTCOLDESC[columnIndex];
                    PropertyTypeName propertyType = descriptor.PropertyType;
                    PropertyID propertyID = descriptor.PropertyID;
                    string value;
                    if (IsCellInUse(rowIndex, propertyID, propertyType))
                    {
                        if (propertyType == PropertyTypeName.PtypBoolean)
                        {
                            bool boolValue = GetBooleanProperty(rowIndex, propertyID).Value;
                            value = boolValue.ToString();
                        }
                        else if (propertyType == PropertyTypeName.PtypInteger16)
                        {
                            value = GetInt16Property(rowIndex, propertyID).ToString() + " (Int16)";
                        }
                        else if (propertyType == PropertyTypeName.PtypInteger32)
                        {
                            value = GetInt32Property(rowIndex, propertyID).ToString();
                        }
                        else if (propertyType == PropertyTypeName.PtypInteger64)
                        {
                            value = GetInt64Property(rowIndex, propertyID).ToString() + " (Int64)";
                        }
                        else if (propertyType == PropertyTypeName.PtypTime)
                        {
                            value = GetDateTimeProperty(rowIndex, propertyID).ToString();
                        }
                        else if (propertyType == PropertyTypeName.PtypString)
                        {
                            value = GetStringProperty(rowIndex, propertyID);
                        }
                        else if (propertyType == PropertyTypeName.PtypBinary)
                        {
                            value = StringHelper.GetByteArrayString(GetBytesProperty(rowIndex, propertyID));
                        }
                        else
                        {
                            value = "-" + propertyType.ToString();
                        }
                    }
                    else
                    {
                        value = "(Unused)";
                    }
                    
                    result.Add(GetPropertyIDString((ushort)propertyID) + ": " + value);
                }
            }

            return result;
        }

        /// <summary>
        /// For discovery purposes
        /// </summary>
        public List<string> ListRowIndex()
        {
            List<string> result = new List<string>();
            BTreeOnHeap<TableContextRowID> bTreeOnHeap = new BTreeOnHeap<TableContextRowID>(m_heap, m_tcInfo.hidRowIndex);
            if (bTreeOnHeap.BTreeHeader.hidRoot.hidIndex > 0) // hidRoot is set to zero if the BTH is empty.
            {
                result.Add("Index levels: " + bTreeOnHeap.BTreeHeader.bIdxLevels);
            }

            for (int rowIndex = 0; rowIndex < m_rowIndex.Count; rowIndex++)
            {
                result.Add(String.Format("RowIndex: {0}, RowID: {1}", m_rowIndex[rowIndex].dwRowIndex, m_rowIndex[rowIndex].dwRowID));
            }
            return result;
        }

        public void UpdateTableContextInfo()
        {
            HeapID newUserRootHeapID = m_heap.ReplaceHeapItem(m_heap.HeapHeader.hidUserRoot, m_tcInfo.GetBytes());
            if (m_heap.HeapHeader.hidUserRoot != newUserRootHeapID)
            {
                HeapOnNodeHeader heapHeader = m_heap.HeapHeader;
                heapHeader.hidUserRoot = newUserRootHeapID;
                m_heap.UpdateHeapHeader(heapHeader);
            }
        }

        public virtual string GetPropertyIDString(ushort propertyID)
        {
            if (Enum.IsDefined(typeof(PropertyID), propertyID))
            {
                return ((PropertyID)propertyID).ToString();
            }
            else
            {
                return "0x" + propertyID.ToString("x");
            }
        }

        public int RowCount
        {
            get
            {
                return m_rowIndex.Count;
            }
        }

        public int ColumnCount
        {
            get
            {
                return m_tcInfo.ColumnCount;
            }
        }

        /// <summary>
        /// All rows have the same length
        /// </summary>
        public int RowLength
        {
            get
            {
                return m_tcInfo.RowLength;
            }
        }

        public HeapOnNode Heap
        {
            get
            {
                return m_heap;
            }
        }

        public DataTree DataTree
        {
            get
            {
                return m_heap.DataTree;
            }
        }

        public SubnodeBTree SubnodeBTree
        {
            get
            {
                return m_subnodeBTree;
            }
        }

        public List<TableColumnDescriptor> Columns
        {
            get
            {
                return m_tcInfo.rgTCOLDESC;
            }
        }

        public PSTFile File
        {
            get
            {
                return m_heap.DataTree.File;
            }
        }

        public static int GetPropertyDataLength(PropertyTypeName propertyType)
        {
            switch (propertyType)
            {
                case PropertyTypeName.PtypBoolean:
                    return 1;
                case PropertyTypeName.PtypInteger16:
                    return 2;
                case PropertyTypeName.PtypTime:
                    return 8;
                default:
                    return 4;
            }
        }

        [Obsolete]
        // We no longer create new TCs, we simply use the template and modify to another data tree
        public static TableContext CreateNewTableContext(PSTFile file, List<TableColumnDescriptor> columns)
        {
            HeapOnNode heap = HeapOnNode.CreateNewHeap(file);
            
            TableContextInfo tcInfo = new TableContextInfo();
            tcInfo.rgTCOLDESC = columns;
            tcInfo.UpdateDataLayout();

            HeapID newUserRoot = heap.AddItemToHeap(tcInfo.GetBytes());
            // The heap header may have just been updated
            HeapOnNodeHeader header = heap.HeapHeader;
            header.bClientSig = OnHeapTypeName.bTypeTC;
            header.hidUserRoot = newUserRoot;
            heap.UpdateHeapHeader(header);

            BTreeOnHeapHeader bTreeHeader = new BTreeOnHeapHeader();
            bTreeHeader.cbKey = TableContextRowID.RecordKeyLength;
            bTreeHeader.cbEnt = TableContextRowID.RecordDataLength;

            tcInfo.hidRowIndex = heap.AddItemToHeap(bTreeHeader.GetBytes());
            // this will replace the item in place (as they have the same size since number of columns was not modified)
            heap.ReplaceHeapItem(header.hidUserRoot, tcInfo.GetBytes());

            return new TableContext(heap, null);
        }

        public static TableContext GetHierarchyTableTemplate(PSTFile file)
        {
            PSTNode node = file.GetNode(InternalNodeName.NID_HIERARCHY_TABLE_TEMPLATE);
            return node.TableContext;
        }

        public static TableContext GetContentsTableTemplate(PSTFile file)
        {
            PSTNode node = file.GetNode(InternalNodeName.NID_CONTENTS_TABLE_TEMPLATE);
            return node.TableContext;
        }

        public static TableContext GetAssociatedContentsTableTemplate(PSTFile file)
        {
            PSTNode node = file.GetNode(InternalNodeName.NID_ASSOC_CONTENTS_TABLE_TEMPLATE);
            return node.TableContext;
        }
    }
}
