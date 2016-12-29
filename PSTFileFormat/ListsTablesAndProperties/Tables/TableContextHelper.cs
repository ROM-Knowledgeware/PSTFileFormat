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
    public class TableContextHelper
    {
        public static void CopyProperties(PropertyContext pc, TableContext tc, int rowIndex)
        { 
            // Note: Outlook 2003 simply iterates over all of the table columns:
            foreach (TableColumnDescriptor descriptor in tc.Columns)
            {
                switch (descriptor.PropertyType)
                {
                    case PropertyTypeName.PtypBoolean:
                        CopyBooleanProperty(pc, tc, rowIndex, descriptor.PropertyID);
                        break;
                    case PropertyTypeName.PtypInteger16:
                        CopyInt16Property(pc, tc, rowIndex, descriptor.PropertyID);
                        break;
                    case PropertyTypeName.PtypInteger32:
                        CopyInt32Property(pc, tc, rowIndex, descriptor.PropertyID);
                        break;
                    case PropertyTypeName.PtypInteger64:
                        CopyInt64Property(pc, tc, rowIndex, descriptor.PropertyID);
                        break;
                    case PropertyTypeName.PtypTime:
                        CopyDateTimeProperty(pc, tc, rowIndex, descriptor.PropertyID);
                        break;
                    case PropertyTypeName.PtypGuid:
                        CopyGuidProperty(pc, tc, rowIndex, descriptor.PropertyID);
                        break;
                    case PropertyTypeName.PtypBinary:
                        CopyBytesProperty(pc, tc, rowIndex, descriptor.PropertyID);
                        break;
                    case PropertyTypeName.PtypString:
                        CopyStringProperty(pc, tc, rowIndex, descriptor.PropertyID);
                        break;
                    default:
                        break;
                }
            }
        }

        public static void CopyBooleanProperty(PropertyContext pc, TableContext tc, int rowIndex, PropertyID propertyID)
        {
            Nullable<bool> value = pc.GetBooleanProperty(propertyID);
            if (value.HasValue && tc.ContainsPropertyColumn(propertyID, PropertyTypeName.PtypBoolean))
            {
                tc.SetBooleanProperty(rowIndex, propertyID, value.Value);
            }
        }

        public static void CopyInt16Property(PropertyContext pc, TableContext tc, int rowIndex, PropertyID propertyID)
        {
            Nullable<short> value = pc.GetInt16Property(propertyID);
            if (value.HasValue && tc.ContainsPropertyColumn(propertyID, PropertyTypeName.PtypInteger16))
            {
                tc.SetInt16Property(rowIndex, propertyID, value.Value);
            }
        }

        public static void CopyInt32Property(PropertyContext pc, TableContext tc, int rowIndex, PropertyID propertyID)
        {
            Nullable<int> value = pc.GetInt32Property(propertyID);
            if (value.HasValue && tc.ContainsPropertyColumn(propertyID, PropertyTypeName.PtypInteger32))
            {
                tc.SetInt32Property(rowIndex, propertyID, value.Value);
            }
        }

        public static void CopyInt64Property(PropertyContext pc, TableContext tc, int rowIndex, PropertyID propertyID)
        {
            Nullable<long> value = pc.GetInt64Property(propertyID);
            if (value.HasValue && tc.ContainsPropertyColumn(propertyID, PropertyTypeName.PtypInteger64))
            {
                tc.SetInt64Property(rowIndex, propertyID, value.Value);
            }
        }

        public static void CopyDateTimeProperty(PropertyContext pc, TableContext tc, int rowIndex, PropertyID propertyID)
        {
            Nullable<DateTime> value = pc.GetDateTimeProperty(propertyID);
            if (value.HasValue && tc.ContainsPropertyColumn(propertyID, PropertyTypeName.PtypTime))
            {
                tc.SetDateTimeProperty(rowIndex, propertyID, value.Value);
            }
        }

        public static void CopyGuidProperty(PropertyContext pc, TableContext tc, int rowIndex, PropertyID propertyID)
        {
            byte[] value = pc.GetBytesProperty(propertyID);
            if (value != null && tc.ContainsPropertyColumn(propertyID, PropertyTypeName.PtypGuid))
            {
                tc.SetGuidProperty(rowIndex, propertyID, value);
            }
        }

        public static void CopyStringProperty(PropertyContext pc, TableContext tc, int rowIndex, PropertyID propertyID)
        {
            string value = pc.GetStringProperty(propertyID);
            if (value != null && tc.ContainsPropertyColumn(propertyID, PropertyTypeName.PtypString))
            {
                tc.SetStringProperty(rowIndex, propertyID, value);
            }
        }

        public static void CopyBytesProperty(PropertyContext pc, TableContext tc, int rowIndex, PropertyID propertyID)
        {
            byte[] value = pc.GetBytesProperty(propertyID);
            if (value != null && tc.ContainsPropertyColumn(propertyID, PropertyTypeName.PtypBinary))
            {
                tc.SetBytesProperty(rowIndex, propertyID, value);
            }
        }

        public static void CopyBooleanProperty(NamedPropertyContext pc, TableContext tc, int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = pc.Map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                CopyBooleanProperty(pc, tc, rowIndex, propertyID.Value);
            }
        }

        public static void CopyInt16Property(NamedPropertyContext pc, TableContext tc, int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = pc.Map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                CopyInt16Property(pc, tc, rowIndex, propertyID.Value);
            }
        }

        public static void CopyInt32Property(NamedPropertyContext pc, TableContext tc, int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = pc.Map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                CopyInt32Property(pc, tc, rowIndex, propertyID.Value);
            }
        }

        public static void CopyDateTimeProperty(NamedPropertyContext pc, TableContext tc, int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = pc.Map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                CopyDateTimeProperty(pc, tc, rowIndex, propertyID.Value);
            }
        }

        public static void CopyBytesProperty(NamedPropertyContext pc, TableContext tc, int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = pc.Map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                CopyBytesProperty(pc, tc, rowIndex, propertyID.Value);
            }
        }

        public static void CopyStringProperty(NamedPropertyContext pc, TableContext tc, int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = pc.Map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                CopyStringProperty(pc, tc, rowIndex, propertyID.Value);
            }
        }
    }
}
