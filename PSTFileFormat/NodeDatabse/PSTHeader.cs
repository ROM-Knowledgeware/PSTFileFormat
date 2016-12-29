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
    public class PSTHeader
    {
        public const int HeaderLength = 564; 

        public enum PSTVersion
        {
            //Ansi = 14,
            //Ansi = 15,
            Unicode = 23, // Unicode PST file
        }

        public enum ClientVersion
        { 
            OfflineFolders = 12,  // OST file
            PersonalFolders = 19, // PST file
        }

        // Note regarding dwReserved1, dwReserved2, rgbReserved2, bReserved, rgbReserved3:
        // Outlook 2003-2010 use these value for implementation-specific data.
        // Modification of these values can result in failure to read the PST file by Outlook

        public string dwMagic = "!BDN"; // !BDN
        public uint CRCPartial;
        public string wMagicClient;
        public ushort wVer;
        public ushort wVerClient;
        public byte bPlatformCreate;
        public byte bPlatformAccess;
        public uint dwReserved1; // offset 16, 
        public uint dwReserved2;
        public BlockID bidUnused; // offset 24
        private BlockID bidNextP; // offset 32
        /* Note: no bidNextB here, documentation mistake, see http://social.msdn.microsoft.com/Forums/pl/os_binaryfile/thread/b50106b9-d1a0-4877-aaa5-8d23be1084fd */
        private uint dwUnique; // offset 40
        private uint[] rgnid = new uint[32];
        // private ulong Unused - offset 172
        public RootStructure root; // offset 180
        public uint dwAlign; // offset 252
        public byte[] rgbFM = new byte[128]; // offset 256
        public byte[] rgbFP = new byte[128]; // offset 384
        public byte bSentinel = 0x80; // offset 512, must be set to 0x80
        public bCryptMethodName bCryptMethod;
        public ushort rgbReserved;
        private BlockID bidNextB; // offset 516, Indicates the next available BID value
        public uint dwCRCFull; // offset 524
        public byte[] rgbReserved2 = new byte[3];
        public byte bReserved;
        public byte[] rgbReserved3 = new byte[32];

        public PSTHeader(byte[] buffer)
        {
            dwMagic = ByteReader.ReadAnsiString(buffer, 0, 4);
            CRCPartial = LittleEndianConverter.ToUInt32(buffer, 4);
            wMagicClient = ByteReader.ReadAnsiString(buffer, 8, 2);
            wVer = LittleEndianConverter.ToUInt16(buffer, 10);
            wVerClient = LittleEndianConverter.ToUInt16(buffer, 12);
            bPlatformCreate = buffer[14];
            bPlatformAccess = buffer[15];

            dwReserved1 = LittleEndianConverter.ToUInt32(buffer, 16);
            dwReserved2 = LittleEndianConverter.ToUInt32(buffer, 20);
            // bidUnused - which is not necessarily zeroed out
            bidUnused = new BlockID(buffer, 24);
            bidNextP = new BlockID(buffer, 32);

            dwUnique = LittleEndianConverter.ToUInt32(buffer, 40);
            int position = 44;
            for (int index = 0; index < 32; index++)
            {
                rgnid[index] = LittleEndianConverter.ToUInt32(buffer, position);
                position += 4;
            }

            root = new RootStructure(buffer, 180);

            dwAlign = LittleEndianConverter.ToUInt32(buffer, 252);
            Array.Copy(buffer, 256, rgbFM, 0, 128);
            Array.Copy(buffer, 384, rgbFP, 0, 128);
            bSentinel = buffer[512];
            bCryptMethod = (bCryptMethodName)buffer[513];

            bidNextB = new BlockID(buffer, 516);

            dwCRCFull = LittleEndianConverter.ToUInt32(buffer, 524);
            Array.Copy(buffer, 528, rgbReserved2, 0, 3);
            bReserved = buffer[531];
            Array.Copy(buffer, 532, rgbReserved3, 0, 32);

            uint partial = PSTCRCCalculation.ComputeCRC(buffer, 8, 471);
            if (partial != CRCPartial)
            {
                throw new InvalidChecksumException();
            }
            uint full = PSTCRCCalculation.ComputeCRC(buffer, 8, 516);
            if (full != dwCRCFull)
            {
                throw new InvalidChecksumException();
            }
        }

        public byte[] GetBytes(WriterCompatibilityMode writerCompatibilityMode)
        { 
            byte[] buffer = new byte[HeaderLength];
            ByteWriter.WriteAnsiString(buffer, 0, dwMagic, 4);
            // update CRCPartial later
            ByteWriter.WriteAnsiString(buffer, 8, wMagicClient, 2);
            LittleEndianWriter.WriteUInt16(buffer, 10, wVer);
            LittleEndianWriter.WriteUInt16(buffer, 12, wVerClient);
            buffer[14] = bPlatformCreate;
            buffer[15] = bPlatformAccess;

            LittleEndianWriter.WriteUInt32(buffer, 16, dwReserved1);
            LittleEndianWriter.WriteUInt32(buffer, 20, dwReserved2);

            bidUnused.WriteBytes(buffer, 24);
            bidNextP.WriteBytes(buffer, 32);

            LittleEndianWriter.WriteUInt32(buffer, 40, dwUnique);

            int position = 44;
            for (int index = 0; index < 32; index++)
            {
                LittleEndianWriter.WriteUInt32(buffer, position, rgnid[index]);
                position += 4;
            }

            root.WriteBytes(buffer, 180, writerCompatibilityMode);

            LittleEndianWriter.WriteUInt32(buffer, 252, dwAlign);
            Array.Copy(rgbFM, 0, buffer, 256, 128);
            Array.Copy(rgbFP, 0, buffer, 384, 128);
            buffer[512] = bSentinel;
            buffer[513] = (byte)bCryptMethod;

            bidNextB.WriteBytes(buffer, 516);

            // update dwCRCFull later
            // ullReserved
            // dwReserved
            Array.Copy(rgbReserved2, 0, buffer, 528, 3);
            buffer[531] = bReserved;
            Array.Copy(rgbReserved3, 0, buffer, 532, 32);

            CRCPartial = PSTCRCCalculation.ComputeCRC(buffer, 8, 471);
            dwCRCFull = PSTCRCCalculation.ComputeCRC(buffer, 8, 516);
            LittleEndianWriter.WriteUInt32(buffer, 4, CRCPartial);
            LittleEndianWriter.WriteUInt32(buffer, 524, dwCRCFull);

            return buffer;
          
        }
        
        public void WriteToStream(Stream stream, WriterCompatibilityMode writerCompatibilityMode)
        {
            dwUnique++;
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(GetBytes(writerCompatibilityMode), 0, HeaderLength);
        }

        public BlockID AllocateNextBlockID()
        {
            BlockID result = bidNextB.Clone();
            if (bidNextB.bidIndex < BlockID.MaximumBidIndex)
            {
                bidNextB.bidIndex++; // this will increment the value itself by 4, as required
            }
            else
            {
                throw new Exception("Could not allocate bidIndex");
            }
            return result;
        }

        /// <summary>
        /// UNDOCUMENTED: Outlook increments bidNextP by 1
        /// Note: bidNextP have no special purpose for the two least insignificant bits
        /// </summary>
        /// <returns></returns>
        public BlockID AllocateNextPageBlockID()
        {
            BlockID result = bidNextP.Clone();
            // MS-PST (2.6.1.1.4): bidNextB must be incremented as well
            AllocateNextBlockID();
            bidNextP = new BlockID(bidNextP.Value + 1);
            return result;
        }

        public uint AllocateNextUniqueID()
        {
            uint result = dwUnique;
            dwUnique++;
            return result;
        }

        private uint AllocateNextNodeIndex(NodeTypeName nodeType)
        {
            int typeIndex = (byte)nodeType;
            if (rgnid[typeIndex] < NodeID.MaximumNidIndex)
            {
                rgnid[typeIndex]++;
            }
            else
            {
                throw new Exception("Could not allocate nidIndex");
            }
            return rgnid[typeIndex];
        }

        public NodeID AllocateNextFolderNodeID()
        {
            uint nodeIndex = AllocateNextNodeIndex(NodeTypeName.NID_TYPE_NORMAL_FOLDER);
            AllocateNextNodeIndex(NodeTypeName.NID_TYPE_HIERARCHY_TABLE);
            AllocateNextNodeIndex(NodeTypeName.NID_TYPE_CONTENTS_TABLE);
            AllocateNextNodeIndex(NodeTypeName.NID_TYPE_ASSOC_CONTENTS_TABLE);
            return new NodeID(NodeTypeName.NID_TYPE_NORMAL_FOLDER, nodeIndex);
        }

        public NodeID AllocateNextNodeID(NodeTypeName nodeType)
        {
            uint nodeIndex = AllocateNextNodeIndex(nodeType);
            return new NodeID(nodeType, nodeIndex);
        }

        public static PSTHeader ReadFromStream(Stream stream, WriterCompatibilityMode writerCompatibilityMode)
        { 
            byte[] buffer = new byte[HeaderLength];
            stream.Read(buffer, 0, HeaderLength);

            string dwMagic = ByteReader.ReadAnsiString(buffer, 0, 4);
            string wMagicClient = ByteReader.ReadAnsiString(buffer, 8, 2);
            ushort wVer = LittleEndianConverter.ToUInt16(buffer, 10);
            ushort wVerClient = LittleEndianConverter.ToUInt16(buffer, 12);

            if (dwMagic == "!BDN" && wVer == (int)PSTVersion.Unicode &&
                ((wMagicClient == "SO" && wVerClient == (int)ClientVersion.OfflineFolders) ||
                 (wMagicClient == "SM" && wVerClient == (int)ClientVersion.PersonalFolders)))
            {
                return new PSTHeader(buffer);
            }
            else
            {
                return null;
            }
        }
    }
}
