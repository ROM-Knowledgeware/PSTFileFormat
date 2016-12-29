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
    // http://msdn.microsoft.com/en-us/library/ee202811%28v=exchg.80%29.aspx
    public class RecipientEntryID
    {
        // uint Flags
        public Guid ProviderUID = new Guid("{a41f2b81-a3be-1910-9d6e-00dd010f5402}");
        public ushort Version;
        
        public bool Pad1; // 1 bit
        public byte MAE; // 2 bits
        public byte Format; // 4 bits
        public bool PureMime; // 1 bit

        public bool UTF16; // 1 bit
        public byte Reserved; // 2 bits
        public bool AvoidAddressBookLookup; // 1 bit
        public byte Pad2; // 4 bits

        public string DisplayName = String.Empty;
        public string AddressType = String.Empty;
        public string EmailAddress = String.Empty;

        public RecipientEntryID()
        { 

        }

        public RecipientEntryID(byte[] buffer)
        {
            int offset = 4;
            ProviderUID = LittleEndianReader.ReadGuid(buffer, ref offset);
            Version = LittleEndianReader.ReadUInt16(buffer, ref offset);

            byte temp = ByteReader.ReadByte(buffer, ref offset);
            MAE = (byte)((temp >> 5) & 0x03);
            Format = (byte)((temp >> 1) & 0x0F);
            PureMime = ((temp & 0x01) > 0);

            temp = ByteReader.ReadByte(buffer, ref offset);
            UTF16 = ((temp & 0x80) > 0);
            AvoidAddressBookLookup = ((temp & 0x10) > 0);

            if (UTF16)
            {
                DisplayName = ReadUnicodeString(buffer, ref offset);
                AddressType = ReadUnicodeString(buffer, ref offset);
                EmailAddress = ReadUnicodeString(buffer, ref offset);
            }
            else
            {
                DisplayName = ReadMultibyteString(buffer, ref offset);
                AddressType = ReadMultibyteString(buffer, ref offset);
                EmailAddress = ReadMultibyteString(buffer, ref offset);
            }
        }

        public byte[] GetBytes()
        {
            int length = 24;
            length += (DisplayName.Length + 1) * 2;
            length += (AddressType.Length + 1) * 2;
            length += (EmailAddress.Length + 1) * 2;
            byte[] buffer = new byte[length];
            int offset = 4;
            LittleEndianWriter.WriteGuidBytes(buffer, ref offset, ProviderUID);
            LittleEndianWriter.WriteUInt16(buffer, ref offset, Version);
            byte temp = 0;
            temp |= (byte)((MAE & 0x03) << 5);
            temp |= (byte)((Format & 0x0F) << 1);
            if (PureMime)
            {
                temp |= 0x01;
            }

            ByteWriter.WriteByte(buffer, ref offset, temp);
            temp = 0;
            if (UTF16)
            {
                temp |= 0x80;
            }

            if (AvoidAddressBookLookup)
            {
                temp |= 0x10;
            }
            ByteWriter.WriteByte(buffer, ref offset, temp);

            if (UTF16)
            {
                WriteUnicodeString(buffer, ref offset, DisplayName);
                WriteUnicodeString(buffer, ref offset, AddressType);
                WriteUnicodeString(buffer, ref offset, EmailAddress);
            }
            else
            {
                throw new NotImplementedException("Encoding MBCS is not supported");
            }
            return buffer;
        }

        public static string ReadUnicodeString(byte[] buffer, ref int offset)
        {
            char temp = (char)LittleEndianReader.ReadUInt16(buffer, ref offset);
            string result = String.Empty;
            while (temp != 0)
            {
                result += temp.ToString();
                temp = (char)LittleEndianReader.ReadUInt16(buffer, ref offset);
            }
            return result;
        }

        public static string ReadMultibyteString(byte[] buffer, ref int offset)
        {
            byte temp = ByteReader.ReadByte(buffer, ref offset);
            List<byte> bytes = new List<byte>();
            while (temp != 0)
            {
                bytes.Add(temp);
                temp = ByteReader.ReadByte(buffer, ref offset);
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public static void WriteUnicodeString(byte[] buffer, ref int offset, string value)
        {
            value += '\0';
            byte[] data = UnicodeEncoding.Unicode.GetBytes(value);
            ByteWriter.WriteBytes(buffer, ref offset, data);
        }

        public static RecipientEntryID GetEntryID(string displayName, string emailAddress)
        {
            RecipientEntryID entryID = new RecipientEntryID();
            entryID.DisplayName = displayName;
            entryID.AddressType = "SMTP";
            entryID.EmailAddress = emailAddress;
            entryID.AvoidAddressBookLookup = true;
            entryID.PureMime = true;
            entryID.UTF16 = true;

            return entryID;
        }
    }
}
