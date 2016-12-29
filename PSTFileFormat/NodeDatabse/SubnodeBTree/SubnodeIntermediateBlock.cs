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
    public class SubnodeIntermediateBlock : Block // SIBLOCK
    {
        public const int MaximumNumberOfEntries = 510; // (8192 - 16 - 8) / 16

        public BlockType btype;
        public byte cLevel;
        //private ushort cEnt;
        // dwPadding
        public List<SubnodeIntermediateEntry> rgentries = new List<SubnodeIntermediateEntry>();

        public SubnodeIntermediateBlock()
        {
            btype = BlockType.SIBLOCK;
            cLevel = 0x01; // 0x01 for SIBLOCK
        }

        public SubnodeIntermediateBlock(byte[] buffer) : base(buffer)
        {
            btype = (BlockType)buffer[0];
            cLevel = buffer[1];
            ushort cEnt = LittleEndianConverter.ToUInt16(buffer, 2);

            int offset = 8;
            for (int index = 0; index < cEnt; index++)
            {
                SubnodeIntermediateEntry entry = new SubnodeIntermediateEntry(buffer, offset);
                rgentries.Add(entry);
                offset += SubnodeIntermediateEntry.Length;
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
                offset += SubnodeIntermediateEntry.Length;
            }
        }

        /// <summary>
        /// Find the index of the SubnodeIntermediateEntry pointing to the SubnodeLeafBlock where the
        /// entry with the given key should be located
        /// </summary>
        public int IndexOfIntermediateEntryWithMatchingRange(uint nodeID)
        {
            int lastIndexToMatch = 0;
            for (int index = 1; index < rgentries.Count; index++)
            {
                // All the entries in the child have key values greater than or equal to this key value.
                if (rgentries[index].nid.Value <= nodeID)
                {
                    lastIndexToMatch = index;
                }
            }
            return lastIndexToMatch;
        }

        public int GetIndexOfBlockID(ulong blockID)
        {
            for (int index = 0; index < rgentries.Count; index++)
            {
                if (rgentries[index].bid.Value == blockID)
                {
                    return index;
                }
            }
            return -1;
        }

        public int GetSortedInsertIndex(SubnodeIntermediateEntry entryToInsert)
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
        public int InsertSorted(SubnodeIntermediateEntry entryToInsert)
        {
            int insertIndex = GetSortedInsertIndex(entryToInsert);
            rgentries.Insert(insertIndex, entryToInsert);
            return insertIndex;
        }

        public override Block Clone()
        {
            SubnodeIntermediateBlock result = (SubnodeIntermediateBlock)MemberwiseClone();
            result.rgentries = new List<SubnodeIntermediateEntry>();
            foreach (SubnodeIntermediateEntry entry in rgentries)
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
                return 8 + rgentries.Count * 16;
            }
        }
    }
}
