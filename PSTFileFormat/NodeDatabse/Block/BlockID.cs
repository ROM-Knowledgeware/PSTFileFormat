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
    public class BlockID // BID
    {
        public const long MaximumBidIndex = 0xFFFFFFFFFFFFFFF;
        public const int Length = 8;

        // Reserved & Internel & bidIndex together comprise the unique BlockID
        private ulong m_blockID;

        public BlockID(ulong blockID)
        {
            m_blockID = blockID;
        }

        public BlockID(bool isInternal, ulong bidIndex)
        {
            this.Internal = isInternal;
            this.bidIndex = bidIndex;
        }

        public BlockID(byte[] buffer, int offset)
        {
            m_blockID = LittleEndianConverter.ToUInt64(buffer, offset + 0);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt64(buffer, offset + 0, m_blockID);
        }

        public ulong Value
        {
            get
            {
                return m_blockID;
            }
        }

        public ulong LookupValue
        {
            get
            {
                // Readers MUST ignore the reserved bit and treat it as zero before looking up the BID from the BBT
                return m_blockID & 0xFFFFFFFFFFFFFFFEU;
            }
        }

        
        // first bit is 'reserved'
        // Office Outlook 2003, Office Outlook 2007, and Outlook 2010 use the reserved bit for implementation-specific data
        public bool Reserved
        {
            get
            {
                return (m_blockID & 0x01) != 0;
            }
        }

        // second bit is 'internal'
        public bool Internal
        {
            get
            {
                return (m_blockID & 0x02) != 0;
            }
            set
            {
                if (value)
                {
                    m_blockID |= 0x02;
                }
                else
                {
                    m_blockID &= 0xFFFFFFFD;
                }
            
            }
        }

        public ulong bidIndex
        {
            get
            {
                return m_blockID >> 2;
            }
            set
            {
                m_blockID &= 0x03;
                m_blockID |= (value << 2);
            }
        }

        public BlockID Clone()
        {
            return new BlockID(m_blockID);
        }
    }
}
