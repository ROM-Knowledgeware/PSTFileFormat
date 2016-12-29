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
    public class PMapPage : Page //PMAPPAGE
    {
        public const int FirstPageOffset = 0x4600; // offset of the first PMAPPAGE within the PST file
        public const int MapppedLength = 2031616; // the number of bytes mapped by a PMap (496 * 8 * 512)

        public byte[] rgbPMapBits = new byte[496];

        public PMapPage()
        {
            pageTrailer.ptype = PageTypeName.ptypePMap;
            pageTrailer.wSig = 0x00; // zero for PMap
        }

        public PMapPage(byte[] buffer) : base(buffer)
        {
            Array.Copy(buffer, 0, rgbPMapBits, 0, rgbPMapBits.Length);
        }

        /// <param name="fileOffset">Irrelevant for AMap</param>
        public override byte[] GetBytes(ulong fileOffset)
        {
            byte[] buffer = new byte[Length];
            Array.Copy(rgbPMapBits, 0, buffer, 0, rgbPMapBits.Length);
            pageTrailer.WriteToPage(buffer, fileOffset);

            return buffer;
        }

        public static PMapPage GetFilledPMapPage()
        {
            PMapPage page = new PMapPage();
            for (int index = 0; index < page.rgbPMapBits.Length; index++)
            {
                page.rgbPMapBits[index] = 0xFF;
            }
            return page;
        }

        public static PMapPage ReadPMapPage(PSTFile file, int pMapPageIndex)
        {
            // The first AMap of a PST file is always located at absolute file offset 0x4600, and subsequent PMaps appear at intervals of 2,031,616 bytes thereafter
            long offset = FirstPageOffset + (long)MapppedLength * pMapPageIndex;
            file.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte[] buffer = new byte[Length];
            file.BaseStream.Read(buffer, 0, Length);

            return new PMapPage(buffer);
        }
    }
}
