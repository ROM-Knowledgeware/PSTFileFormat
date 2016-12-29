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
using System.IO;
using Utilities;

namespace PSTFileFormat
{
    public abstract class BTreePage : Page // BTPAGE
    {
        // private byte cEnt;    // The number of BTree entries stored in the page data.
        public byte cEntMax;
        public byte cbEnt;   // The size of each BTree entry, in bytes. Implementations MUST use the size specified in cbEnt to advance to the next entry
        public byte cLevel;  // The depth level of this page. Leaf pages have a level of zero, whereas intermediate pages have a level greater than 0
        // public uint dwPadding
        
        public BTreeIndexPage ParentPage; // We use this to keep reference to the parent page
        public ulong Offset; // offset in PST file

        public BTreePage() : base()
        { 

        }

        public BTreePage(byte[] buffer) : base(buffer)
        {
            byte cEnt = buffer[488];
            cEntMax = buffer[489];
            cbEnt = buffer[490];
            cLevel = buffer[491];

            PopulateEntries(buffer, cEnt);
        }

        public abstract void PopulateEntries(byte[] buffer, byte numberOfEntries);

        /// <returns>Number of entries</returns>
        public abstract int WriteEntries(byte[] buffer);

        public override byte[] GetBytes(ulong fileOffset)
        {
            byte[] buffer = new byte[Length];
            byte cEnt;
            
            cEnt = (byte)WriteEntries(buffer);
            buffer[488] = cEnt;
            buffer[489] = cEntMax;
            buffer[490] = cbEnt;
            buffer[491] = cLevel;
            pageTrailer.WriteToPage(buffer, fileOffset);

            return buffer;
        }

        public abstract ulong PageKey
        {
            get;
        }

        public static BTreePage ReadFromStream(Stream stream, BlockRef blockRef)
        {
            long offset = (long)blockRef.ib;
            stream.Seek(offset, SeekOrigin.Begin);
            byte[] buffer = new byte[Length];
            stream.Read(buffer, 0, Length);
            PageTypeName ptype = (PageTypeName)buffer[PageTrailer.OffsetFromPageStart + 0];
            BTreePage page;
            byte cLevel = buffer[491];
            if (cLevel > 0)
            {
                // If cLevel is greater than 0, then each entry in the array is of type BTENTRY.
                page = new BTreeIndexPage(buffer);
            }
            else
            {
                // If cLevel is 0, then each entry is either of type BBTENTRY or NBTENTRY, depending on the ptype of the page.
                if (ptype == PageTypeName.ptypeBBT)
                {
                    page = new BlockBTreeLeafPage(buffer);
                }
                else if (ptype == PageTypeName.ptypeNBT)
                {
                    page = new NodeBTreeLeafPage(buffer);
                }
                else
                {
                    throw new ArgumentException("BTreePage has incorrect ptype");
                }
            }
            page.Offset = (ulong)blockRef.ib;
            if (blockRef.bid.Value != page.BlockID.Value)
            {
                throw new InvalidBlockIDException();
            }
            uint crc = PSTCRCCalculation.ComputeCRC(buffer, PageTrailer.OffsetFromPageStart);
            if (page.pageTrailer.dwCRC != crc)
            {
                throw new InvalidChecksumException();
            }

            uint signature = BlockTrailer.ComputeSignature(blockRef.ib, blockRef.bid.Value);
            if (page.pageTrailer.wSig != signature)
            {
                throw new InvalidChecksumException();
            }
            return page;
        }
    }
}
