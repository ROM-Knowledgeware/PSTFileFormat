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
    public class SearchDomainObject
    {
        PSTFile m_file;
        byte[] m_data;

        public SearchDomainObject(PSTFile file)
        {
            m_file = file;
        }

        public bool ContainsNode(NodeID nodeID)
        {
            if (m_data == null)
            {
                PSTNode node = m_file.GetNode((uint)InternalNodeName.NID_SEARCH_DOMAIN_OBJECT);
                if (node.DataTree == null)
                {
                    m_data = new byte[0];
                }
                else
                {
                    m_data = node.DataTree.GetData();
                }
            }

            int nodeCount = m_data.Length / 4;
            for (int index = 0; index < nodeCount; index++)
            {
                uint currentNodeID = LittleEndianConverter.ToUInt32(m_data, index * 4);
                if (currentNodeID == nodeID.Value)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
