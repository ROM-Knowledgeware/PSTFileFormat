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
    public class PropertyContextRecord : BTreeOnHeapDataRecord // PC BTH leaf record
    {
        public const int RecordKeyLength = 2;
        public const int RecordDataLength = 6;
        public const int Length = 8;

        public PropertyID wPropId;
        public PropertyTypeName wPropType;
        public uint dwValueHnid; // the value itself or hid or nid

        public HeapOrNodeID HeapOrNodeID; // in case the dwValueHnid is HeapOrNodeID

        public PropertyContextRecord()
        { 
        }

        public PropertyContextRecord(byte[] buffer, int offset)
        {
            wPropId = (PropertyID)LittleEndianConverter.ToUInt16(buffer, offset + 0);
            wPropType = (PropertyTypeName)LittleEndianConverter.ToUInt16(buffer, offset + 2);
            dwValueHnid = LittleEndianConverter.ToUInt32(buffer, offset + 4);
            if (!IsPropertyStoredInternally(wPropType))
            {
                HeapOrNodeID = new HeapOrNodeID(buffer, offset + 4);
            }
        }

        public override void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt16(buffer, offset + 0, (ushort)wPropId);
            LittleEndianWriter.WriteUInt16(buffer, offset + 2, (ushort)wPropType);
            if (!IsPropertyStoredInternally(wPropType))
            {
                dwValueHnid = HeapOrNodeID.Value;
            }
            LittleEndianWriter.WriteUInt32(buffer, offset + 4, dwValueHnid);
        }

        public bool IsExternal
        {
            get
            {
                return (HeapOrNodeID != null);
            }
        }
        
        /// <summary>
        /// Caller should use IsExternal to verify HeapOrNodeID is not null before using IsHeapID
        /// </summary>
        public bool IsHeapID
        {
            get
            {
                return HeapOrNodeID.IsHeapID;
            }
        }

        public HeapID HeapID
        {
            get
            {
                return HeapOrNodeID.HeapID;
            }
            set
            {
                HeapOrNodeID = new HeapOrNodeID(value);
            }
        }

        public NodeID NodeID
        {
            get
            {
                return HeapOrNodeID.NodeID;
            }
            set
            {
                HeapOrNodeID = new HeapOrNodeID(value);
            }
        }

        public override byte[] Key
        {
            get
            {
                return LittleEndianConverter.GetBytes((ushort)wPropId);
            }
        }

        public override int KeyLength
        {
            get 
            {
                return RecordKeyLength;
            }
        }

        public override int DataLength
        {
            get 
            {
                return RecordDataLength;
            }
        }

        public override int CompareTo(byte[] keyToCompare)
        {
            if (keyToCompare.Length == KeyLength)
            {
                ushort key = (ushort)wPropId;
                return key.CompareTo(LittleEndianConverter.ToUInt16(keyToCompare, 0));
            }
            return -1;
        }

        public override int CompareTo(BTreeOnHeapDataRecord record)
        {
            if (record is PropertyContextRecord)
            {
                ushort key = (ushort)wPropId;
                ushort keyToCompare = (ushort)((PropertyContextRecord)record).wPropId;
                return key.CompareTo(keyToCompare);
            }
            return -1;
        }

        public override bool Equals(object obj)
        {
            if (obj is PropertyContextRecord)
            {
                return ((PropertyContextRecord)obj).wPropId == wPropId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return wPropId.GetHashCode();
        }


        public static bool IsPropertyStoredInternally(PropertyTypeName wPropType)
        {
            // if wPropType is fixed and <= 4 bytes, then dwValueHnid is either NID or HID
            if (wPropType == PropertyTypeName.PtypBoolean ||
                wPropType == PropertyTypeName.PtypInteger16 ||
                wPropType == PropertyTypeName.PtypInteger32 ||
                wPropType == PropertyTypeName.PtypFloating32 ||
                wPropType == PropertyTypeName.PtypErrorCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
