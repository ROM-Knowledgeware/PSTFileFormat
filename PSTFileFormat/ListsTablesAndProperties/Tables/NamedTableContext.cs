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
    public class NamedTableContext : TableContext
    {
        private PropertyNameToIDMap m_map;

        public NamedTableContext(TableContext tc, PropertyNameToIDMap map) : base(tc.Heap, tc.SubnodeBTree)
        {
            m_map = map;
        }

        public NamedTableContext(HeapOnNode heap, SubnodeBTree subnodeBTree, PropertyNameToIDMap map)
            : base(heap, subnodeBTree)
        {
            m_map = map;
        }

        #region Get Property
        public Nullable<bool> GetBooleanProperty(int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetBooleanProperty(rowIndex, propertyID.Value);
            }
            return null;
        }

        public Nullable<int> GetInt16Property(int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetInt16Property(rowIndex, propertyID.Value);
            }
            return null;
        }

        public Nullable<int> GetInt32Property(int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetInt32Property(rowIndex, propertyID.Value);
            }
            return null;
        }

        public Nullable<DateTime> GetDateTimeProperty(int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetDateTimeProperty(rowIndex, propertyID.Value);
            }
            return null;
        }

        public string GetStringProperty(int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetStringProperty(rowIndex, propertyID.Value);
            }
            return null;
        }

        public byte[] GetBytesProperty(int rowIndex, PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetBytesProperty(rowIndex, propertyID.Value);
            }
            return null;
        }
        #endregion

        #region Set Property
        public void SetInt16Property(int rowIndex, PropertyName propertyName, short value)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                SetInt16Property(rowIndex, propertyID.Value, value);
            }
        }

        public void SetInt32Property(int rowIndex, PropertyName propertyName, int value)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                SetInt32Property(rowIndex, propertyID.Value, value);
            }
        }

        public void SetStringProperty(int rowIndex, PropertyName propertyName, string value)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                SetStringProperty(rowIndex, propertyID.Value, value);
            }
        }
        #endregion

        public void RemoveProperty(int rowIndex, PropertyName propertyName, PropertyTypeName propertyType)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                RemoveProperty(rowIndex, propertyID.Value, propertyType);
            }
        }

        public bool ContainsPropertyColumn(PropertyName propertyName, PropertyTypeName propertyType)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return ContainsPropertyColumn(propertyID.Value, propertyType);
            }
            return false;
        }

        public int FindColumnIndexByPropertyTag(PropertyName propertyName, PropertyTypeName propertyType)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return FindColumnIndexByPropertyTag(propertyID.Value, propertyType);
            }
            return -1;
        }

        /// <returns>True if property column was added</returns>
        public bool AddPropertyColumnIfNotExist(PropertyName propertyName, PropertyTypeName propertyType)
        {
            PropertyID propertyID = m_map.ObtainIDFromName(propertyName);
            return AddPropertyColumnIfNotExist(propertyID, propertyType);
        }

        public override string GetPropertyIDString(ushort propertyID)
        {
            if (propertyID >= 0x8000)
            {
                // named property
                Nullable<PropertyLongID> propertyLongID = m_map.GetLongIDFromID(propertyID);
                if (propertyLongID.HasValue)
                {
                    if (Enum.IsDefined(typeof(PropertyLongID), propertyLongID))
                    {
                        return propertyLongID.ToString();
                    }
                    else
                    {
                        return "[Named] 0x" + ((uint)propertyLongID).ToString("x");
                    }
                }
                else
                {
                    return "[Named Error] 0x" + ((uint)propertyID).ToString("x");
                }
            }

            return base.GetPropertyIDString(propertyID);
        }

        public PropertyNameToIDMap Map
        {
            get
            {
                return m_map;
            }
        }
    }
}
