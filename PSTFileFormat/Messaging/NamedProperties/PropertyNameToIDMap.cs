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
    public class PropertyNameToIDMap
    {
        PSTFile m_file;
        public Dictionary<PropertyName, ushort> m_map;
        public byte[] PropertySetGuidStreamCache;

        public PropertyNameToIDMap(PSTFile file)
        {
            m_file = file;
        }

        /// <summary>
        /// Create short PropertyID if not exist
        /// </summary>
        public PropertyID ObtainIDFromName(PropertyName propertyName)
        {
            Nullable<PropertyID> propertyID = GetIDFromName(propertyName);
            if (!propertyID.HasValue)
            {
                return AddToMap(propertyName.PropertyLongID, propertyName.PropertySetGuid);
            }
            else
            {
                return propertyID.Value;
            }
        }

        public Nullable<PropertyID> GetIDFromName(PropertyName propertyName)
        {
            if (m_map == null)
            {
                FillMap();
            }
            ushort result;
            bool success = m_map.TryGetValue(propertyName, out result);
            if (success)
            {
                return (PropertyID)result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reverse search, useful only for discovery purposes
        /// </summary>
        public Nullable<PropertyLongID> GetLongIDFromID(ushort propertyID)
        {
            if (m_map == null)
            {
                FillMap();
            }

            foreach (PropertyName propertyName in m_map.Keys)
            {
                if (m_map[propertyName] == propertyID)
                {
                    return propertyName.PropertyLongID;
                }
            }
            return null;
        }


        public void FillMap()
        {
            m_map = new Dictionary<PropertyName, ushort>();
            PSTNode node = m_file.GetNode((uint)InternalNodeName.NID_NAME_TO_ID_MAP);
            byte[] buffer = node.PC.GetBytesProperty(PropertyID.PidTagNameidStreamEntry);
            if (buffer.Length % 8 > 0)
            {
                throw new InvalidPropertyException("Invalid NameidStreamEntry");
            }

            for (int index = 0; index < buffer.Length; index += 8)
            {
                NameID nameID = new NameID(buffer, index);
                if (!nameID.IsStringIdentifier) 
                {
                    ushort propertyShortID = nameID.PropertyShortID;
                    PropertyLongID propertyLongID = (PropertyLongID)nameID.dwPropertyID;
                    Guid propertySetGuid = GetPropertySetGuid(nameID.wGuid);

                    m_map.Add(new PropertyName(propertyLongID, propertySetGuid), propertyShortID);
                }
            }
        }

        /// <returns>short PropertyID used to store the property</returns>
        public PropertyID AddToMap(PropertyLongID propertyLongID, Guid propertySetGuid)
        {
            PSTNode node = m_file.GetNode((uint)InternalNodeName.NID_NAME_TO_ID_MAP);

            int wGuid = GetPropertySetGuidIndexHint(propertySetGuid);
            if (wGuid == -1)
            {
                wGuid = 3 + AddPropertySetGuid(node, propertySetGuid);
            }

            byte[] oldBuffer = node.PC.GetBytesProperty(PropertyID.PidTagNameidStreamEntry);
            int propertyIndex = oldBuffer.Length / 8;

            NameID nameID = new NameID(propertyLongID, (ushort)wGuid, (ushort)propertyIndex);
            byte[] newBuffer = new byte[oldBuffer.Length + 8];
            Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
            nameID.WriteBytes(newBuffer, oldBuffer.Length);
            node.PC.SetBytesProperty(PropertyID.PidTagNameidStreamEntry, newBuffer);
            
            AddPropertyToHashBucket(node, nameID);
            node.SaveChanges();

            PropertyID propertyID = (PropertyID)(nameID.PropertyShortID);
            m_map.Add(new PropertyName(propertyLongID, propertySetGuid), (ushort)propertyID);
            return propertyID;
        }
        
        // Note: Changes must be saved by the caller method
        private void AddPropertyToHashBucket(Node node, NameID nameID)
        { 
            int bucketCount = node.PC.GetInt32Property(PropertyID.PidTagNameidBucketCount).Value;
            uint firstBucketPropertyID = (uint)PropertyID.PidTagNameidBucketBase;
            uint arg0 = nameID.dwPropertyID;
            uint arg1 = Convert.ToUInt32(nameID.IdentifierType) + nameID.wGuid << 1;
            ushort bucketIndex = (ushort)((arg0 ^ arg1) % bucketCount);

            if (nameID.IsStringIdentifier)
            {
                throw new NotImplementedException("Named property with string identifier is not supported");
            }
            PropertyID bucketPropertyID = (PropertyID)(firstBucketPropertyID + bucketIndex);
            byte[] oldBuffer = node.PC.GetBytesProperty(bucketPropertyID);
            if (oldBuffer == null)
            {
                oldBuffer = new byte[0];
            }
            byte[] newBuffer = new byte[oldBuffer.Length + NameID.Length];
            Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
            nameID.WriteBytes(newBuffer, oldBuffer.Length);
            node.PC.SetBytesProperty(bucketPropertyID, newBuffer);
        }

        private Guid GetPropertySetGuid(int indexHint)
        {
            if (indexHint == 0)
            {
                return Guid.Empty;
            }
            else if (indexHint == 1)
            {
                return PropertySetGuid.PS_MAPI;
            }
            else if (indexHint == 2)
            {
                return PropertySetGuid.PS_PUBLIC_STRINGS;
            }
            else
            {
                int propertySetGuidIndex = indexHint - 3;
                if (PropertySetGuidStreamCache == null)
                {
                    PSTNode node = m_file.GetNode((uint)InternalNodeName.NID_NAME_TO_ID_MAP);
                    PropertySetGuidStreamCache = node.PC.GetBytesProperty(PropertyID.PidTagNameidStreamGuid);
                }

                int offset = propertySetGuidIndex * 16;
                Guid guid = LittleEndianConverter.ToGuid(PropertySetGuidStreamCache, offset);
                return guid;
            }
        }

        private int GetPropertySetGuidIndexHint(Guid propertySetGuid)
        {
            if (propertySetGuid == Guid.Empty)
            {
                return 0;
            }
            else if (propertySetGuid == PropertySetGuid.PS_MAPI)
            {
                return 1;
            }
            else if (propertySetGuid == PropertySetGuid.PS_PUBLIC_STRINGS)
            {
                return 2;
            }
            else
            {
                int guidIndex = GetPropertySetGuidIndex(propertySetGuid);
                if (guidIndex == -1)
                {
                    return -1;
                }
                else
                {
                    return 3 + guidIndex;
                }
            }
        }

        private int GetPropertySetGuidIndex(Guid propertySetGuid)
        {
            PSTNode node = m_file.GetNode((uint)InternalNodeName.NID_NAME_TO_ID_MAP);
            byte[] buffer = node.PC.GetBytesProperty(PropertyID.PidTagNameidStreamGuid);
            if (buffer.Length % 16 > 0)
            {
                throw new InvalidPropertyException("Invalid NameidStreamGuid");
            }

            for (int index = 0; index < buffer.Length; index += 16)
            {
                Guid guid = LittleEndianConverter.ToGuid(buffer, index);
                if (guid == propertySetGuid)
                {
                    return index / 16;
                }
            }

            return -1;
        }

        // Note: Changes must be saved by the caller method
        /// <returns>Index of property set GUID</returns>
        private int AddPropertySetGuid(Node node, Guid propertySetGuid)
        {
            byte[] oldBuffer = node.PC.GetBytesProperty(PropertyID.PidTagNameidStreamGuid);
            byte[] newBuffer = new byte[oldBuffer.Length + 16];
            Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
            LittleEndianWriter.WriteGuidBytes(newBuffer, oldBuffer.Length, propertySetGuid);
            node.PC.SetBytesProperty(PropertyID.PidTagNameidStreamGuid, newBuffer);

            PropertySetGuidStreamCache = newBuffer; // update cache

            return oldBuffer.Length / 16;
        }
    }
}
