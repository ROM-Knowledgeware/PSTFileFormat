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
    public abstract class Page
    {
        public const int Length = 512;
        public PageTrailer pageTrailer;

        public Page()
        {
            pageTrailer = new PageTrailer();
        }

        public Page(byte[] buffer)
        {
            pageTrailer = PageTrailer.ReadFromPage(buffer);
        }

        public abstract byte[] GetBytes(ulong fileOffset);

        public BlockID BlockID
        {
            get
            {
                return pageTrailer.bid;
            }
            set
            {
                pageTrailer.bid = value;
            }
        }

        public void WriteToStream(Stream stream, long offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            stream.Write(GetBytes((ulong)offset), 0, Length);
        }
    }
}
