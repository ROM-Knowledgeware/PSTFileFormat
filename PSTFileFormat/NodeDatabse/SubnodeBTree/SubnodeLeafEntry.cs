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
    public class SubnodeLeafEntry // SLENTRY
    {
        public const int Length = 24;

        public NodeID nid;
        public BlockID bidData;
        public BlockID bidSub;

        public SubnodeLeafEntry()
        { 
        }

        public SubnodeLeafEntry(byte[] buffer, int offset)
        {
            // the 4-byte NID is extended to its 8-byte equivalent
            nid = new NodeID(buffer, offset + 0);
            bidData = new BlockID(buffer, offset + 8);
            bidSub = new BlockID(buffer, offset + 16);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            // the 4-byte NID is extended to its 8-byte equivalent
            LittleEndianWriter.WriteUInt32(buffer, offset + 0, nid.Value);
            LittleEndianWriter.WriteUInt64(buffer, offset + 8, bidData.Value);
            LittleEndianWriter.WriteUInt64(buffer, offset + 16, bidSub.Value);
        }

        public SubnodeLeafEntry Clone()
        {
            SubnodeLeafEntry result = new SubnodeLeafEntry();
            result.nid = nid.Clone();
            result.bidData = bidData.Clone();
            result.bidSub = bidSub.Clone();
            return result;
        }
    }
}
