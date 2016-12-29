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
    public class SubnodeIntermediateEntry // SIENTRY
    {
        public const int Length = 16;

        public NodeID nid;  // The key NID value to the next-level child block
        public BlockID bid; // The BID of the SLBLOCK

        public SubnodeIntermediateEntry()
        { 
        }

        public SubnodeIntermediateEntry(NodeID nodeID, BlockID blockID)
        {
            nid = nodeID;
            bid = blockID;
        }

        public SubnodeIntermediateEntry(byte[] buffer, int offset)
        {
            // the 4-byte NID is extended to its 8-byte equivalent
            nid = new NodeID(buffer, offset);
            bid = new BlockID(buffer, offset + 8);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            // the 4-byte NID is extended to its 8-byte equivalent
            LittleEndianWriter.WriteUInt32(buffer, offset + 0, nid.Value);
            LittleEndianWriter.WriteUInt64(buffer, offset + 8, bid.Value);
        }

        public SubnodeIntermediateEntry Clone()
        {
            return new SubnodeIntermediateEntry(nid.Clone(), bid.Clone());
        }
    }
}
