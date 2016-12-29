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
    public class PropertyName
    {
        public PropertyLongID PropertyLongID;
        public Guid PropertySetGuid;

        public PropertyName(PropertyLongID propertyLongID, Guid propertySetGuid)
        {
            PropertyLongID = propertyLongID;
            PropertySetGuid = propertySetGuid;
        }

        public override bool Equals(object obj)
        {
            if (obj is PropertyName)
            {
                return (PropertyLongID == ((PropertyName)obj).PropertyLongID &&
                        PropertySetGuid == ((PropertyName)obj).PropertySetGuid);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return PropertyLongID.GetHashCode();
        }
    }
}
