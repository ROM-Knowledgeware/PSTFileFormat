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
    public class DensityListPage : Page // DLISTPAGE
    {
        public const int FirstPageOffset = 0x4200;

        public const int MaxNumberOfEntries = 119; // 476 / 4
        
        public const byte DFL_BACKFILL_COMPLETE = 0x01;

        public byte bFlags;
        //public byte cEntDList;
        // 2 bytes padding
        // If DFL_BACKFILL_COMPLETE is set in bFlags, then ulCurrentPage indicates the AMap page index that is used
        // in the next allocation. If DFL_BACKFILL_COMPLETE is not set in bFlags, then ulCurrentPage indicates
        // the AMap page index that is attempted for backfilling in the next allocation.
        uint ulCurrentPage;
        List<DensityListPageEntry> rgDListPageEnt = new List<DensityListPageEntry>();

        public DensityListPage(byte[] buffer) : base(buffer)
        {
            bFlags = buffer[0];
            byte cEntDList = buffer[1];
            ulCurrentPage = LittleEndianConverter.ToUInt32(buffer, 4);
            int offset = 8;
            for (int index = 0; index < cEntDList; index++)
            {
                DensityListPageEntry entry = new DensityListPageEntry(buffer, offset);
                rgDListPageEnt.Add(entry);
                offset += DensityListPageEntry.Length;
            }
        }

        public override byte[] GetBytes(ulong fileOffset)
        {
            if (rgDListPageEnt.Count > MaxNumberOfEntries)
            {
                throw new Exception("Density list contains too many entries");
            }

            byte[] buffer = new byte[Length];
            buffer[0] = bFlags;
            buffer[1] = (byte)rgDListPageEnt.Count;
            LittleEndianWriter.WriteUInt32(buffer, 4, ulCurrentPage);

            int offset = 8;
            foreach (DensityListPageEntry entry in rgDListPageEnt)
            {
                entry.WriteBytes(buffer, offset);
                offset += DensityListPageEntry.Length;
            }
            pageTrailer.dwCRC = PSTCRCCalculation.ComputeCRC(buffer, Length - PageTrailer.Length);
            pageTrailer.WriteToPage(buffer, fileOffset);

            return buffer;
        }
    }
}
