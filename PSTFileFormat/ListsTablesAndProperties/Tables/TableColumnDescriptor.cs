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
    public class TableColumnDescriptor // TCOLDESC
    {
        public const int Length = 8;

        public uint Tag;      // tag that is associated with the column
        public ushort ibData; // indicates the offset from the beginning of the row data (in the Row Matrix)
        public byte cbData;   // Data size
        public byte iBit;     // Cell Existence Bitmap Index

        public TableColumnDescriptor()
        { 
        }

        public TableColumnDescriptor(byte[] buffer, int offset)
        {
            Tag = LittleEndianConverter.ToUInt32(buffer, offset + 0);
            ibData = LittleEndianConverter.ToUInt16(buffer, offset + 4);
            cbData = ByteReader.ReadByte(buffer, offset + 6);
            iBit = ByteReader.ReadByte(buffer, offset + 7);
        }

        public void WriteBytes(byte[] buffer, int offset)
        {
            LittleEndianWriter.WriteUInt32(buffer, offset + 0, Tag);
            LittleEndianWriter.WriteUInt16(buffer, offset + 4, ibData);
            ByteWriter.WriteByte(buffer, offset + 6, cbData);
            ByteWriter.WriteByte(buffer, offset + 7, iBit);
        }

        public PropertyTypeName PropertyType
        {
            get
            {
                return (PropertyTypeName)(this.Tag & 0xFFFF);
            }
            set
            {
                this.Tag |= (uint)(0x0000FFFF & (ushort)value);
            }
        }

        public PropertyID PropertyID
        {
            get
            {
                return (PropertyID)(this.Tag >> 16);
            }
            set
            {
                this.Tag |= (uint)(0xFFFF0000 & ((ushort)value << 16));
            }
        }

        public bool IsStoredExternally
        {
            get
            { 
                return (IsPropertyStoredExternally(this.PropertyType));
            }
        }

        public int DataLengthGroup
        {
            get
            {
                if (cbData == 4 || cbData == 8)
                {
                    return TableContextInfo.TCI_4b;
                }
                else if (cbData == 2)
                {
                    return TableContextInfo.TCI_2b;
                }
                else if (cbData == 1)
                {
                    return TableContextInfo.TCI_1b;
                }
                else
                {
                    throw new Exception("Invalid column length");
                }
            }
        }

        public static bool IsPropertyStoredExternally(PropertyTypeName wPropType)
        {
            // if wPropType is fixed and <= 8 bytes then it's internal, otherwise dwValueHnid is either NID or HID
            if (wPropType == PropertyTypeName.PtypString ||
                wPropType == PropertyTypeName.PtypString8 ||
                wPropType == PropertyTypeName.PtypBinary ||
                wPropType == PropertyTypeName.PtypObject ||
                wPropType == PropertyTypeName.PtypGuid ||
                wPropType == PropertyTypeName.PtypMultiString)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int Compare(TableColumnDescriptor a, TableColumnDescriptor b)
        {
            return a.Tag.CompareTo(b.Tag);
        }
    }
}
