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
    public class MailFolder : PSTFolder
    {
        public MailFolder(PSTNode node) : base(node)
        {
        }

        public Note GetNote(int index)
        {
            TableContext tc = GetContentsTable();
            if (tc != null)
            {
                if (index < tc.RowCount)
                {
                    // dwRowID is the MessageID
                    NodeID nodeID = new NodeID(tc.GetRowID(index));
                    Note note = Note.GetNote(this.File, nodeID);
                    return note;
                }
            }
            return null;
        }

        public override void AddContentTableColumns(NamedTableContext contentsTable)
        {
            base.AddContentTableColumns(contentsTable);

            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidSideEffects, PropertyTypeName.PtypInteger32);
            contentsTable.AddPropertyColumnIfNotExist(PropertyNames.PidLidHeaderItem, PropertyTypeName.PtypInteger32);
        }
    }
}
