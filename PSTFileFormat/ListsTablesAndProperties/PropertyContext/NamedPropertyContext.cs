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
    public class NamedPropertyContext : PropertyContext
    {
        PropertyNameToIDMap m_map;

        public NamedPropertyContext(HeapOnNode heap, SubnodeBTree subnodeBTree, PropertyNameToIDMap map) : base(heap, subnodeBTree)
        {
            m_map = map;
        }

        public NamedPropertyContext(PropertyContext pc, PropertyNameToIDMap map) : base(pc.Heap, pc.SubnodeBTree)
        {
            m_map = map;
        }

        public void RemoveProperty(PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                RemoveProperty(propertyID.Value);
            }
        }

        public bool GetBooleanProperty(PropertyName propertyName, bool defaultValue)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetBooleanProperty(propertyID.Value, defaultValue);
            }
            return defaultValue;
        }

        public Nullable<bool> GetBooleanProperty(PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetBooleanProperty(propertyID.Value);
            }
            return null;
        }

        public short GetInt16Property(PropertyName propertyName, short defaultValue)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetInt16Property(propertyID.Value, defaultValue);
            }
            return defaultValue;
        }

        public Nullable<short> GetInt16Property(PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetInt16Property(propertyID.Value);
            }
            return null;
        }

        public int GetInt32Property(PropertyName propertyName, int defaultValue)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetInt32Property(propertyID.Value, defaultValue);
            }
            return defaultValue;
        }

        public Nullable<int> GetInt32Property(PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetInt32Property(propertyID.Value);
            }
            return null;
        }

        public DateTime GetDateTimeProperty(PropertyName propertyName, DateTime defaultValue)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetDateTimeProperty(propertyID.Value, defaultValue);
            }
            return defaultValue;
        }

        public Nullable<DateTime> GetDateTimeProperty(PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetDateTimeProperty(propertyID.Value);
            }
            return null;
        }
        
        public string GetStringProperty(PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetStringProperty(propertyID.Value);
            }
            return null;
        }

        public byte[] GetBytesProperty(PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = m_map.GetIDFromName(propertyName);
            if (propertyID.HasValue)
            {
                return GetBytesProperty(propertyID.Value);
            }
            return null;
        }

        public void SetBooleanProperty(PropertyName propertyName, bool value)
        {
            PropertyID propertyID = m_map.ObtainIDFromName(propertyName);
            SetBooleanProperty(propertyID, value);
        }

        public void SetInt32Property(PropertyName propertyName, int value)
        {
            PropertyID propertyID = m_map.ObtainIDFromName(propertyName);
            SetInt32Property(propertyID, value);
        }

        public void SetDateTimeProperty(PropertyName propertyName, DateTime value)
        {
            PropertyID propertyID = m_map.ObtainIDFromName(propertyName);
            SetDateTimeProperty(propertyID, value);
        }

        public void SetStringProperty(PropertyName propertyName, string value)
        {
            PropertyID propertyID = m_map.ObtainIDFromName(propertyName);
            SetStringProperty(propertyID, value);
        }

        public void SetBytesProperty(PropertyName propertyName, byte[] value)
        {
            PropertyID propertyID = m_map.ObtainIDFromName(propertyName);
            SetBytesProperty(propertyID, value);
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
