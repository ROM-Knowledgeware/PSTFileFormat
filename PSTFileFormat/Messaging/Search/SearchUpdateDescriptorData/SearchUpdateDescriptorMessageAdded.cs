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
    // SUD_MSG_ADD / SUD_MSG_MOD / SUD_MSG_DEL Structure
    public class SearchUpdateDescriptorMessageAdded : SearchUpdateDescriptorData
    {
        public NodeID nidParent; // NID of the parent Folder object into which the Message object is added, modified, or deleted.
        public NodeID nidMsg; // NID of the Message object that was added, modified, or deleted.

        public SearchUpdateDescriptorMessageAdded(NodeID folderNodeID, NodeID messageNodeID)
        {
            nidParent = folderNodeID;
            nidMsg = messageNodeID;
        }

        public SearchUpdateDescriptorMessageAdded(byte[] buffer, int offset)
        {
            nidParent = new NodeID(buffer, offset + 0);
            nidMsg = new NodeID(buffer, offset + 4);
        }

        public override void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt32(buffer, offset + 0, nidParent.Value);
            LittleEndianWriter.WriteUInt32(buffer, offset + 4, nidMsg.Value);
        }
    }
}
