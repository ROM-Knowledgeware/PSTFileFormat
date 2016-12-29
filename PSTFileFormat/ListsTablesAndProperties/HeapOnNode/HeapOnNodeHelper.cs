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
    public class HeapOnNodeHelper
    {
        public static byte[] ReadFillLevelMap(byte[] buffer, int offset, int numberOfEntries)
        {
            byte[] fillLevelMap = new byte[numberOfEntries];
            int bytesToRead = numberOfEntries / 2;
            for (int index = 0; index < bytesToRead; index++)
            {
                byte entry1 = (byte)(buffer[offset] & 0xF);
                byte entry2 = (byte)(buffer[offset] >> 4);
                fillLevelMap[index * 2] = entry1;
                fillLevelMap[index * 2 + 1] = entry2;
                offset += 1;
            }
            return fillLevelMap;
        }

        public static void WriteFillLevelMap(byte[] buffer, int offset, byte[] fillLevelMap)
        {
            int bytesToWrite = fillLevelMap.Length / 2;
            for (int index = 0; index < bytesToWrite; index++)
            {
                byte fillLevelByte = (byte)(fillLevelMap[index * 2] & 0xF);
                fillLevelByte |= (byte)(fillLevelMap[index * 2 + 1] << 4);
                ByteWriter.WriteByte(buffer, offset, fillLevelByte);
                offset += 1;
            }
        }

        public static byte GetBlockFillLevel(HeapOnNodeBlockData blockData)
        {
            int availableSpace = blockData.AvailableSpace;
            if (availableSpace >= 3584)
            {
                return 0x00;
            }
            else if (availableSpace >= 2560)
            {
                return 0x01;
            }
            else if (availableSpace >= 2048)
            {
                return 0x02;
            }
            else if (availableSpace >= 1792)
            {
                return 0x03;
            }
            else if (availableSpace >= 1536)
            {
                return 0x04;
            }
            else if (availableSpace >= 1280)
            {
                return 0x05;
            }
            else if (availableSpace >= 1024)
            {
                return 0x06;
            }
            else if (availableSpace >= 768)
            {
                return 0x07;
            }
            else if (availableSpace >= 512)
            {
                return 0x08;
            }
            else if (availableSpace >= 256)
            {
                return 0x09;
            }
            else if (availableSpace >= 128)
            {
                return 0x0A;
            }
            else if (availableSpace >= 64)
            {
                return 0x0B;
            }
            else if (availableSpace >= 32)
            {
                return 0x0C;
            }
            else if (availableSpace >= 16)
            {
                return 0x0D;
            }
            else if (availableSpace >= 8)
            {
                return 0x0E;
            }
            else
            {
                return 0x0F;
            }
        }
    }
}
