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
    public class FPMapPage : Page // FPMAPPAGE
    {
        public const int FirstPageOffset = 0x7C004A00; // offset of the first FMAPPAGE within the PST file (0x4A00 + 253952 * 64 * 128)
        public const long MapppedLength = 8061452288; // the number of bytes mapped by an FMap (496 * 253952 * 64)

        public byte[] rgbFPMapBits = new byte[496];
        
        public FPMapPage()
        {
            pageTrailer.ptype = PageTypeName.ptypeFPMap;
            pageTrailer.wSig = 0x00; // zero for FPMap
        }

        public FPMapPage(byte[] buffer) : base(buffer)
        {
            Array.Copy(buffer, 0, rgbFPMapBits, 0, rgbFPMapBits.Length);
        }

        /// <param name="fileOffset">Irrelevant for AMap</param>
        public override byte[] GetBytes(ulong fileOffset)
        {
            byte[] buffer = new byte[Length];
            Array.Copy(rgbFPMapBits, 0, buffer, 0, rgbFPMapBits.Length);
            pageTrailer.WriteToPage(buffer, fileOffset);

            return buffer;
        }

        /// <returns>-1 for header</returns>
        public static int GetFPMapPageIndex(int aMapPageIndex)
        {
            if (aMapPageIndex < 128 * 64)
            {
                return -1;
            }
            else
            {
                return (aMapPageIndex - 128) / 496;
            }
        }

        public static int GetFPMapEntryIndex(int aMapPageIndex)
        {
            if (aMapPageIndex < 128 * 64)
            {
                return aMapPageIndex;
            }
            else
            {
                return (aMapPageIndex - 128 * 64) % (496 * 64);
            }
        }
    }
}
