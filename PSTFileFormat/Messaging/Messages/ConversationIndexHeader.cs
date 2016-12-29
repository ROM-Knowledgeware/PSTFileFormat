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
    // http://msdn.microsoft.com/en-us/library/ee202481%28v=exchg.80%29.aspx
    public class ConversationIndexHeader
    {
        public DateTime FileTime;
        public Guid Guid;

        public ConversationIndexHeader(Guid guid)
        {
            FileTime = DateTime.Now;
            Guid = guid;
        }

        public ConversationIndexHeader(byte[] buffer)
        {
            if (buffer[0] != 0x01)
            {
                throw new InvalidPropertyException("Invalid Conversation Index Header");
            }

            // we want the most significant byte from current DateTime
            byte[] temp = BigEndianConverter.GetBytes(DateTime.Now.ToFileTimeUtc());
            temp[6] = 0;
            temp[7] = 0;
            Array.Copy(buffer, 1, temp, 1, 5);
            FileTime = DateTime.FromFileTimeUtc(BigEndianConverter.ToInt64(temp, 0));
            Guid = LittleEndianConverter.ToGuid(buffer, 6);
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[22];
            // the first byte of filetime should be 1
            byte[] temp = BigEndianConverter.GetBytes(FileTime.ToFileTimeUtc());
            Array.Copy(temp, 0, buffer, 0, 6);
            LittleEndianWriter.WriteGuidBytes(buffer, 6, Guid);
            return buffer;
        }

        public static ConversationIndexHeader GenerateNewConversationIndex()
        {
            return new ConversationIndexHeader(Guid.NewGuid());
        }
    }
}
