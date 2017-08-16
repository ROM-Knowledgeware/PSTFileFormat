/* Copyright (C) 2017 ROM Knowledgeware. All rights reserved.
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
    /// <summary>
    /// Mail Message
    /// </summary>
    public class Note : MessageObject
    {
        protected Note(PSTNode node) : base(node)
        {
        }

        public override void SaveChanges()
        {
            base.SaveChanges();
        }

        public static Note GetNote(PSTFile file, NodeID nodeID)
        {
            PSTNode node = file.GetNode(nodeID);
            NamedPropertyContext pc = node.PC;
            if (pc != null)
            {
                return new Note(node);
            }
            else
            {
                return null;
            }
        }

        public static Note CreateNewNote(PSTFile file, NodeID parentNodeID)
        {
            return CreateNewNote(file, parentNodeID, Guid.NewGuid());
        }

        public static Note CreateNewNote(PSTFile file, NodeID parentNodeID, Guid searchKey)
        {
            MessageObject message = CreateNewMessage(file, FolderItemTypeName.Note, parentNodeID, searchKey);
            Note note = new Note(message);
            note.MessageFlags = MessageFlags.MSGFLAG_READ;
            note.InternetCodepage = 1255;
            note.MessageDeliveryTime = DateTime.UtcNow;
            note.ClientSubmitTime = DateTime.UtcNow;
            note.SideEffects = SideEffectsFlags.seOpenForCtxMenu | SideEffectsFlags.seOpenToMove | SideEffectsFlags.seOpenToCopy | SideEffectsFlags.seCoerceToInbox | SideEffectsFlags.seOpenToDelete;
            note.Importance = MessageImportance.Normal;
            note.Priority = MessagePriority.Normal;

            note.IconIndex = IconIndex.NewMail;
            return note;
        }
    }
}
