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
using System.IO;
using System.Text;
using Utilities;

namespace PSTFileFormat
{
    public class FreeMapPage : Page // FMAPPAGE
    {
        public const int FirstPageOffset = 0x1F04800; // offset of the first FMAPPAGE within the PST file (0x4800 + 253952 * 128)
        public const int MapppedLength = 125960192; // the number of bytes mapped by an FMap (496 * 253952)

        public byte[] rgbFMapBits = new byte[496];

        public FreeMapPage()
        {
            pageTrailer.ptype = PageTypeName.ptypeFMap;
            pageTrailer.wSig = 0x00; // zero for FMap
        }

        public FreeMapPage(byte[] buffer) : base(buffer)
        {
            Array.Copy(buffer, 0, rgbFMapBits, 0, rgbFMapBits.Length);
        }

        /// <param name="fileOffset">Irrelevant for AMap</param>
        public override byte[] GetBytes(ulong fileOffset)
        {
            byte[] buffer = new byte[Length];
            Array.Copy(rgbFMapBits, 0, buffer, 0, rgbFMapBits.Length);
            pageTrailer.WriteToPage(buffer, fileOffset);

            return buffer;
        }

        /// <returns>-1 for header</returns>
        public static int GetFreeMapPageIndex(int aMapPageIndex)
        {
            if (aMapPageIndex < 128)
            {
                return -1;
            }
            else
            {
                return (aMapPageIndex - 128) / 496;
            }
        }

        public static int GetFreeMapEntryIndex(int aMapPageIndex)
        {
            if (aMapPageIndex < 128)
            {
                return aMapPageIndex;
            }
            else
            {
                return (aMapPageIndex - 128) % 496;
            }
        }

        public void WriteFreeMapPage(PSTFile file, int fMapPageIndex)
        {
            long offset = FirstPageOffset + (long)MapppedLength * fMapPageIndex;
            WriteToStream(file.BaseStream, offset);
        }

        public static FreeMapPage ReadFreeMapPage(PSTFile file, int fMapPageIndex)
        {
            long offset = FirstPageOffset + (long)MapppedLength * fMapPageIndex;
            file.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte[] buffer = new byte[Length];
            file.BaseStream.Read(buffer, 0, Length);

            return new FreeMapPage(buffer);
        }
    }
}
