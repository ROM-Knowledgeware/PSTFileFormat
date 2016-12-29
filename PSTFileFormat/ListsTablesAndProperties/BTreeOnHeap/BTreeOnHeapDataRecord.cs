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

namespace PSTFileFormat
{
    public abstract class BTreeOnHeapDataRecord
    {
        public abstract int CompareTo(byte[] key);

        public abstract int CompareTo(BTreeOnHeapDataRecord record);

        public abstract void WriteBytes(byte[] buffer, int offset);

        public bool IsKeyEquals(byte[] key)
        {
            return (CompareTo(key) == 0);
        }

        public abstract byte[] Key
        {
            get;
        }

        public abstract int KeyLength
        {
            get;
        }

        public abstract int DataLength
        {
            get;
        }

        public int RecordLength
        {
            get
            {
                return KeyLength + DataLength;
            }
        }

        public static T CreateInstance<T>(byte[] leafBytes, int offset) where T : BTreeOnHeapDataRecord
        {
            // this will call one of the following:
            // PropertyContextRecord(byte[] buffer, int offset)
            // TableContextRowID(byte[] buffer, int offset)
            T record = (T)Activator.CreateInstance(typeof(T), leafBytes, offset);
            return record;
        }
    }
}
