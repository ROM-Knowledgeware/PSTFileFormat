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
    public class AllocationMapPage : Page // AMAPPAGE
    {
        public const int FirstPageOffset = 0x4400; // offset of the first AMAPPAGE within the PST file
        public const int MapppedLength = 253952; // the number of bytes mapped by an AMap (496 * 8 * 64)

        public byte[] rgbAMapBits = new byte[496];
        
        public AllocationMapPage() : base()
        {
            rgbAMapBits[0] = 0xFF; // An AMap is allocated out of the data section
            pageTrailer.ptype = PageTypeName.ptypeAMap;
            pageTrailer.wSig = 0x00; // zero for AMap
        }

        public AllocationMapPage(byte[] buffer) : base(buffer)
        {
            Array.Copy(buffer, 0, rgbAMapBits, 0, rgbAMapBits.Length);
        }

        /// <param name="fileOffset">Irrelevant for AMap</param>
        public override byte[] GetBytes(ulong fileOffset)
        {
            byte[] buffer = new byte[Length];
            Array.Copy(rgbAMapBits, 0, buffer, 0, rgbAMapBits.Length);
            pageTrailer.WriteToPage(buffer, fileOffset);

            return buffer;
        }

        /// <returns>offset within the mapped space, or -1 if no such allocation possible</returns>
        public int FindContiguousSpace(int allocationLength, bool pageAligned)
        {
            int firstFreeOffset = -1;
            int freeLength = 0;
            for (int byteIndex = 0; byteIndex < rgbAMapBits.Length; byteIndex++)
            {
                for (int bitNumber = 0; bitNumber < 8; bitNumber++)
                {
                    // from MSB to LSB (as suggested by MS-PST, page 68)
                    // The MSB represents the first bit in the map
                    int bitOffset = 7 - bitNumber;
                    if (bitNumber > 0 && pageAligned)
                    {
                        if (freeLength == 0) // means that we are not in a middle of a free space sequence
                        {
                            break; // we're looking for page aligned space, scan next page
                        }
                    }

                    bool isFree = (((byte)(rgbAMapBits[byteIndex] >> bitOffset)) & 0x01) == 0;
                    if (isFree)
                    {
                        if (freeLength == 0)
                        { 
                            // first free bit
                            firstFreeOffset = byteIndex * 8 * 64 + bitNumber * 64;
                        }

                        freeLength += 64; // each bit represents 64 bytes
                        if (freeLength >= allocationLength)
                        {
                            return firstFreeOffset;
                        }
                    }
                    else
                    {
                        freeLength = 0;
                    }
                }
            }

            return -1;
        }

        
        /// <summary>
        /// We assume allocated space is free
        /// </summary>
        /// <param name="startOffset">offset within the mapped space</param>
        /// <param name="allocationLength">length (in bytes) within the mapped space</param>
        public void AllocateSpace(int startOffset, int allocationLength)
        {
            for (int offset = startOffset; offset < startOffset + allocationLength; offset += 64)
            {
                AllocateUnit(offset);
            }
        }

        /// <summary>
        /// Unit is 64 bytes
        /// </summary>
        /// <param name="startOffset">Start offset of the unit to allocate</param>
        public void AllocateUnit(int startOffset)
        {
            int byteOffset = startOffset / (8 * 64);
            // from MSB to LSB (as suggested by MS-PST, page 68)
            int bitNumber = (startOffset % (8 * 64)) / 64;
            int bitOffset = 7 - bitNumber;
            rgbAMapBits[byteOffset] |= (byte)(0x01 << bitOffset);
        }

        public void FreeAllocatedSpace(int startOffset, int allocationLength)
        {
            for (int offset = startOffset; offset < startOffset + allocationLength; offset += 64)
            {
                FreeAllocatedUnit(offset);
            }
        }

        /// <summary>
        /// Unit is 64 bytes
        /// </summary>
        /// <param name="startOffset">Start offset of the unit to free</param>
        public void FreeAllocatedUnit(int startOffset)
        {
            int byteOffset = startOffset / (8 * 64);
            // from MSB to LSB (as suggested by MS-PST, page 68)
            int bitNumber = (startOffset % (8 * 64)) / 64;
            int bitOffset = 7 - bitNumber;
            rgbAMapBits[byteOffset] &= (byte)~(0x01 << bitOffset);
        }

        /// <returns>Max contiguous space in bytes</returns>
        public int GetMaxContiguousSpace()
        {
            int freeLength = 0;
            int max = 0;

            for (int byteIndex = 0; byteIndex < rgbAMapBits.Length; byteIndex++)
            {
                for (int bitNumber = 0; bitNumber < 8; bitNumber++)
                {
                    // from MSB to LSB (as suggested by MS-PST, page 68)
                    // The MSB represents the first bit in the map
                    int bitOffset = 7 - bitNumber;

                    bool isFree = (((byte)(rgbAMapBits[byteIndex] >> bitOffset)) & 0x01) == 0;
                    if (isFree)
                    {
                        freeLength += 64; // each bit represents 64 bytes
                        if (freeLength > max)
                        {
                            max = freeLength;
                        }
                    }
                    else
                    {
                        freeLength = 0;
                    }
                }
            }

            return max;
        }

        public void WriteAllocationMapPage(PSTFile file, int allocationMapPageIndex)
        {
            long offset = FirstPageOffset + (long)MapppedLength * allocationMapPageIndex;
            WriteToStream(file.BaseStream, offset);
        }

        public static AllocationMapPage ReadAllocationMapPage(PSTFile file, int allocationMapPageIndex)
        { 
            // The first AMap of a PST file is always located at absolute file offset 0x4400, and subsequent AMaps appear at intervals of 253,952 bytes thereafter
            long offset = FirstPageOffset + (long)MapppedLength * allocationMapPageIndex;
            file.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte[] buffer = new byte[Length];
            file.BaseStream.Read(buffer, 0, Length);

            return new AllocationMapPage(buffer);
        }
    }
}
