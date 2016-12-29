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
    public class NodeID // NID - 4 bytes
    {
        public const int MaximumNidIndex = 0x7FFFFFF;
        // nidType & nidIndex together comprise the unique NodeID
        
        // Note:
        // The documentation is not clear about the order of nidType and nidIndex,
        // it's bit reversed in [MS-PST]

        private uint m_nodeID;

        public NodeID(byte[] buffer, int offset)
        {
            m_nodeID = LittleEndianConverter.ToUInt32(buffer, offset + 0);
        }

        public NodeID(uint nid)
        {
            m_nodeID = nid;
        }

        public NodeID(NodeTypeName nidType, uint nidIndex)
        {
            m_nodeID = (byte)((byte)nidType & 0x1F);
            m_nodeID |= (nidIndex << 5);
        }

        public NodeTypeName nidType
        {
            get
            {
                return (NodeTypeName)(m_nodeID & 0x1F);
            }
        }

        public NodeID Clone()
        {
            return new NodeID(m_nodeID);
        }

        public uint nidIndex
        {
            get
            {
                return (m_nodeID >> 5);
            }
        }

        public uint Value
        {
            get
            {
                return m_nodeID;
            }
        }
    }
}
