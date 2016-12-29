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
    public class SubnodeLeafBlock : Block // SLBLOCK
    {
        public const int MaximumNumberOfEntries = 340; // (8192 - 16 - 8) / 24

        public BlockType btype;
        public byte cLevel;
        // ushort cEnt;
        // dwPadding
        public List<SubnodeLeafEntry> rgentries = new List<SubnodeLeafEntry>();

        public SubnodeLeafBlock() : base()
        {
            btype = BlockType.SLBLOCK;
            cLevel = 0x00; // 0x00 for SLBLOCK
        }

        public SubnodeLeafBlock(byte[] buffer) : base(buffer)
        {
            btype = (BlockType)buffer[0];
            cLevel = buffer[1];
            ushort cEnt = LittleEndianConverter.ToUInt16(buffer, 2);

            int offset = 8;
            for (int index = 0; index < cEnt; index++)
            {
                SubnodeLeafEntry entry = new SubnodeLeafEntry(buffer, offset);
                rgentries.Add(entry);
                offset += SubnodeLeafEntry.Length;
            }
        }

        public override void WriteDataBytes(byte[] buffer, ref int offset)
        {
            ByteWriter.WriteByte(buffer, offset + 0, (byte)btype);
            ByteWriter.WriteByte(buffer, offset + 1, cLevel);
            LittleEndianWriter.WriteInt32(buffer, offset + 2, rgentries.Count);

            offset = 8;
            for (int index = 0; index < rgentries.Count; index++)
            {
                rgentries[index].WriteBytes(buffer, offset);
                offset += SubnodeLeafEntry.Length;
            }
        }

        public int IndexOfLeafEntry(uint nodeID)
        {
            for (int index = 0; index < rgentries.Count; index++)
            {
                if (rgentries[index].nid.Value == nodeID)
                {
                    return index;
                }
            }
            return -1;
        }

        public int GetSortedInsertIndex(SubnodeLeafEntry entryToInsert)
        {
            uint key = entryToInsert.nid.Value;

            int insertIndex = 0;
            while (insertIndex < rgentries.Count && key > rgentries[insertIndex].nid.Value)
            {
                insertIndex++;
            }
            return insertIndex;
        }

        /// <returns>Insert index</returns>
        public int InsertSorted(SubnodeLeafEntry entryToInsert)
        {
            int insertIndex = GetSortedInsertIndex(entryToInsert);
            rgentries.Insert(insertIndex, entryToInsert);
            return insertIndex;
        }

        public SubnodeLeafBlock Split()
        {
            int newBlockStartIndex = rgentries.Count / 2;
            SubnodeLeafBlock newBlock = new SubnodeLeafBlock();
            // BlockID will be given when the block will be added
            for (int index = newBlockStartIndex; index < rgentries.Count; index++)
            {
                newBlock.rgentries.Add(rgentries[index]);
            }

            rgentries.RemoveRange(newBlockStartIndex, rgentries.Count - newBlockStartIndex);
            return newBlock;
        }

        public override Block Clone()
        {
            SubnodeLeafBlock result = (SubnodeLeafBlock)MemberwiseClone();
            result.rgentries = new List<SubnodeLeafEntry>();
            foreach (SubnodeLeafEntry entry in rgentries)
            {
                result.rgentries.Add(entry.Clone());
            }
            return result;
        }
        
        // Raw data contained in the block (excluding trailer and alignment padding)
        public override int DataLength
        {
            get 
            {
                return 8 + rgentries.Count * 24;
            }
        }

        public uint BlockKey
        {
            get
            {
                return this.rgentries[0].nid.Value;
            }
        }
    }
}
