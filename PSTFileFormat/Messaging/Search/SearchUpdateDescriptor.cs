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
    public class SearchUpdateDescriptor
    {
        public const int Length = 20;

        public SearchUpdateDescriptorFlags wFlags;
        public SearchUpdateDescriptorType wSUDType;
        public SearchUpdateDescriptorData SUDData;

        public SearchUpdateDescriptor(SearchUpdateDescriptorFlags flags, SearchUpdateDescriptorType type, SearchUpdateDescriptorData data)
        {
            wFlags = flags;
            wSUDType = type;
            SUDData = data;
        }

        public SearchUpdateDescriptor(byte[] buffer, int offset)
        {
            wFlags = (SearchUpdateDescriptorFlags)LittleEndianConverter.ToUInt16(buffer, offset + 0);
            wSUDType = (SearchUpdateDescriptorType)LittleEndianConverter.ToUInt16(buffer, offset + 2);
            switch (wSUDType)
            {
                case SearchUpdateDescriptorType.SUDT_FLD_ADD:
                case SearchUpdateDescriptorType.SUDT_FLD_MOV:
                    SUDData = new SearchUpdateDescriptorFolderAdded(buffer, offset + 4);
                    break;
                case SearchUpdateDescriptorType.SUDT_FLD_MOD:
                case SearchUpdateDescriptorType.SUDT_FLD_DEL:
                    SUDData = new SearchUpdateDescriptorFolderModified(buffer, offset + 4);
                    break;
                case SearchUpdateDescriptorType.SUDT_MSG_ADD:
                case SearchUpdateDescriptorType.SUDT_MSG_MOD:
                case SearchUpdateDescriptorType.SUDT_MSG_DEL:
                    SUDData = new SearchUpdateDescriptorMessageAdded(buffer, offset + 4);
                    break;
                default:
                    throw new NotImplementedException("Unsupported SUD type");
            }
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[Length];
            LittleEndianWriter.WriteUInt16(buffer, 0, (ushort)wFlags);
            LittleEndianWriter.WriteUInt16(buffer, 2, (ushort)wSUDType);
            SUDData.WriteBytes(buffer, 4);

            return buffer;
        }
    }
}
