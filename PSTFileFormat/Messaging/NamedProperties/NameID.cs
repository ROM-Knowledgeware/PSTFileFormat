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
    public class NameID
    {
        public const int Length = 8;

        public uint dwPropertyID;
        public bool IdentifierType; // a.k.a. N
        public ushort wGuid;    // Index hint
        public ushort wPropIdx; // The Property ID of this named property is calculated by adding 0x8000 to wPropIndex

        public NameID(PropertyLongID propertyLongID, ushort propertySetGuidIndexHint, ushort propertyIndex)
        {
            dwPropertyID = (uint)propertyLongID;
            IdentifierType = false;
            wGuid = propertySetGuidIndexHint;
            wPropIdx = propertyIndex;
        }

        public NameID(byte[] buffer, int offset)
        {
            dwPropertyID = LittleEndianConverter.ToUInt32(buffer, offset + 0);
            ushort temp = LittleEndianConverter.ToUInt16(buffer, offset + 4);
            IdentifierType = (temp & 0x01) > 0;
            wGuid = (ushort)(temp >> 1);
            wPropIdx = LittleEndianConverter.ToUInt16(buffer, offset + 6);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt32(buffer, offset + 0, dwPropertyID);
            ushort temp = (ushort)(wGuid << 1);
            if (IdentifierType)
            {
                temp |= 0x01;
            }
            LittleEndianWriter.WriteUInt16(buffer, offset + 4, temp);
            LittleEndianWriter.WriteUInt16(buffer, offset + 6, wPropIdx);
        }

        public override string ToString()
        {
            return String.Format("dwPropertyID: {0}, IdentifierType: {1}, wGuid: {2}, wPropIdx: {3}", dwPropertyID, IdentifierType, wGuid, wPropIdx);
        }

        public ushort PropertyShortID
        {
            get
            {
                return (ushort)(0x8000 + wPropIdx);
            }
        }

        public bool IsStringIdentifier
        {
            get
            {
                // false - the named property identifier is a 16-bit numerical value
                // true  - the named property identifier is a string
                return (IdentifierType == true);
            }
        }
    }
}
