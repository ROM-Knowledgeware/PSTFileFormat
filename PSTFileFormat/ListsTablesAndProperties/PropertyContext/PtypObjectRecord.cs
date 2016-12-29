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
    // http://msdn.microsoft.com/en-us/library/gg491783%28v=office.12%29.aspx
    public class PtypObjectRecord
    {
        public NodeID Nid;
        public uint ulSize;

        public PtypObjectRecord(NodeID nodeID, uint size)
        {
            Nid = nodeID;
            ulSize = size;
        }

        public PtypObjectRecord(byte[] buffer)
        {
            Nid = new NodeID(buffer, 0x00);
            ulSize = LittleEndianConverter.ToUInt32(buffer, 0x04);
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[8];
            LittleEndianWriter.WriteUInt32(buffer, 0, Nid.Value);
            LittleEndianWriter.WriteUInt32(buffer, 4, ulSize);
            return buffer;
        }
    }
}
