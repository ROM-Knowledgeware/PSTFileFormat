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
    public class DataBlock : Block
    {
        public const int MaximumDataLength = 8176; // Block.MaximumLength - BlockTrailer.Length;

        private bCryptMethodName m_bCryptMethod;
        public byte[] Data = new byte[0];

        public DataBlock(bCryptMethodName bCryptMethod)
        {
            m_bCryptMethod = bCryptMethod;
        }

        public DataBlock(byte[] buffer, bCryptMethodName bCryptMethod) : base(buffer)
        {
            m_bCryptMethod = bCryptMethod;
            Data = new byte[BlockTrailer.cb];
            Array.Copy(buffer, Data, BlockTrailer.cb);

            // DataBlock's data may be decoded
            Data = GetDecodedData();
        }

        public override void WriteDataBytes(byte[] buffer, ref int offset)
        {
            byte[] data = GetEncodedData();
            ByteWriter.WriteBytes(buffer, offset, data);
            offset += data.Length;
        }

        public byte[] GetDecodedData()
        {
            byte[] result = new byte[Data.Length];
            Array.Copy(Data, result, Data.Length);
            if (m_bCryptMethod == bCryptMethodName.NDB_CRYPT_PERMUTE)
            {
                PSTEncryptionUtils.CryptPermute(result, result.Length, false);
            }
            else if (m_bCryptMethod == bCryptMethodName.NDB_CRYPT_CYCLIC)
            {
                // The block trailer was supposed to be read at this stage.
                // [MS-PST]: the value to use for dwKey is the lower DWORD of the BID
                // associated with this data block.
                uint key = (uint)(BlockID.Value & 0xFFFFFFFF);
                PSTEncryptionUtils.CryptCyclic(result, result.Length, key);
            }
            return result;
        }

        public byte[] GetEncodedData()
        {
            byte[] result = new byte[Data.Length];
            Array.Copy(Data, result, Data.Length);
            if (m_bCryptMethod == bCryptMethodName.NDB_CRYPT_PERMUTE)
            {
                PSTEncryptionUtils.CryptPermute(result, result.Length, true);
            }
            else if (m_bCryptMethod == bCryptMethodName.NDB_CRYPT_CYCLIC)
            {
                // [MS-PST]: the value to use for dwKey is the lower DWORD of the BID
                // associated with this data block.
                uint key = (uint)(BlockID.Value & 0xFFFFFFFF);
                PSTEncryptionUtils.CryptCyclic(result, result.Length, key);
            }
            return result;
        }

        public override Block Clone()
        {
            DataBlock result = (DataBlock)MemberwiseClone();
            result.Data = new byte[Data.Length];
            Array.Copy(Data, result.Data, Data.Length);
            return result;
        }

        // Raw data contained in the block (excluding trailer and alignment padding)
        public override int DataLength
        {
            get 
            {
                return Data.Length;
            }
        }
    }
}
