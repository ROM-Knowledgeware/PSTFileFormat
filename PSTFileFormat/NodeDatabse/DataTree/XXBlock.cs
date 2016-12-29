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
    public class XXBlock : Block
    {
        public const int MaximumNumberOfXBlocks = 1021; // (8192 - 16 - 8) / 8
        public const int MaximumNumberOfDataBlocks = 1042441; // 1021 * 1021

        public BlockType btype;
        public byte cLevel;
        //private ushort cEnt;
        public uint lcbTotal; // Total bytes of all the external data
        public List<BlockID> rgbid = new List<BlockID>();

        public XXBlock()
        {
            btype = BlockType.XXBlock;
            cLevel = 0x02; // 0x01 for XBlock
        }

        public XXBlock(byte[] buffer) : base(buffer)
        {
            btype = (BlockType)buffer[0];
            cLevel = buffer[1];
            ushort cEnt = LittleEndianConverter.ToUInt16(buffer, 2);
            lcbTotal = LittleEndianConverter.ToUInt32(buffer, 4);
            int position = 8;
            for (int index = 0; index < cEnt; index++)
            {
                BlockID bid = new BlockID(buffer, position);
                rgbid.Add(bid);
                position += 8;
            }
        }

        public override void WriteDataBytes(byte[] buffer, ref int offset)
        {
            ByteWriter.WriteByte(buffer, offset + 0, (byte)btype);
            ByteWriter.WriteByte(buffer, offset + 1, (byte)cLevel);
            LittleEndianWriter.WriteInt32(buffer, offset + 2, rgbid.Count);
            LittleEndianWriter.WriteUInt32(buffer, offset + 4, lcbTotal);
            offset = 8;
            for (int index = 0; index < rgbid.Count; index++)
            {
                LittleEndianWriter.WriteUInt64(buffer, offset, rgbid[index].Value);
                offset += 8;
            }
        }

        public override Block Clone()
        {
            XXBlock result = (XXBlock)MemberwiseClone();
            result.rgbid = new List<BlockID>();
            foreach (BlockID blockID in rgbid)
            {
                result.rgbid.Add(blockID.Clone());
            }
            return result;
        }

        // Raw data contained in the block (excluding trailer and alignment padding)
        public override int DataLength
        {
            get 
            {
                return 8 + rgbid.Count * 8;
            }
        }

        public int NumberOfXBlocks
        {
            get
            {
                return rgbid.Count;
            }
        }
    }
}
