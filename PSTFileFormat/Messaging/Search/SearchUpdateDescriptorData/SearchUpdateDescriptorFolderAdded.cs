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
    // SUD_FLD_ADD / SUD_FLD_MOV
    public class SearchUpdateDescriptorFolderAdded : SearchUpdateDescriptorData
    {
        public NodeID nidParent;
        public NodeID nidFld; // NID of the Folder object that was added or moved.

        public SearchUpdateDescriptorFolderAdded(NodeID parentNodeID, NodeID folderNodeID)
        {
            nidParent = parentNodeID;
            nidFld = folderNodeID;
        }

        public SearchUpdateDescriptorFolderAdded(byte[] buffer, int offset)
        {
            nidParent = new NodeID(buffer, offset + 0);
            nidFld = new NodeID(buffer, offset + 4);
        }

        public override void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt32(buffer, offset + 0, nidParent.Value);
            LittleEndianWriter.WriteUInt32(buffer, offset + 4, nidFld.Value);
        }
    }
}
