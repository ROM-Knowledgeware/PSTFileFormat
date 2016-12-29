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
    public class NodeBTreeEntry // NBTreeEntry (Leaf NBT Entry)
    {
        // to stay consistent with the size of the btkey member in BTENTRY, 
        // the 4-byte NID is extended to its 8-byte equivalent
        public NodeID nid;
        // padding of 4 bytes (TODO: Verify)
        public BlockID bidData;  // The BID of the data block for this node
        public BlockID bidSub;   // The BID of the subnode block for this node. If this value is zero, a subnode block does not exist
        public NodeID nidParent;
        // public uint dwPadding;

        public NodeBTreeEntry()
        { 
        }

        public NodeBTreeEntry(byte[] buffer, int offset)
        {
            nid = new NodeID(buffer, offset + 0);
            bidData = new BlockID(buffer, offset + 8);
            bidSub = new BlockID(buffer, offset + 16);
            nidParent = new NodeID(buffer, offset + 24);
            // 4 bytes of padding (Outlook does not always set these bytes to 0)
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[32];
            return buffer;
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt32(buffer, offset + 0, nid.Value);
            LittleEndianWriter.WriteUInt64(buffer, offset + 8, bidData.Value);
            LittleEndianWriter.WriteUInt64(buffer, offset + 16, bidSub.Value);
            LittleEndianWriter.WriteUInt32(buffer, offset + 24, nidParent.Value);
        }
    }
}
