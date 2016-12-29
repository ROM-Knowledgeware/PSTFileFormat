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
using Microsoft.Win32;

namespace Utilities
{
    public partial class RegistryTimeZoneUtils
    {
        /// <summary>
        /// For dynamic DST use TimeZoneInfo
        /// </summary>
        /// /// <returns>null if the key does not contain timezone information</returns>
        public static RegistryTimeZoneInformation GetStaticTimeZoneInformation(string keyName)
        {
            RegistryKey timeZonesKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones");
            RegistryKey timeZoneKey = timeZonesKey.OpenSubKey(keyName);
            if (timeZoneKey == null)
            {
                return null;
            }

            byte[] tzi = (byte[])timeZoneKey.GetValue("TZI");
            if (tzi == null)
            {
                return null;
            }

            return new RegistryTimeZoneInformation(tzi);
        }

        public static string GetDisplayName(string keyName, out string standardDisplayName, out string daylightDisplayName)
        {
            RegistryKey timeZonesKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones");
            RegistryKey timeZoneKey = timeZonesKey.OpenSubKey(keyName);
            if (timeZoneKey == null)
            {
                standardDisplayName = null;
                daylightDisplayName = null;
                return null;
            }

            string displayName = (string)timeZoneKey.GetValue("Display");
            standardDisplayName = (string)timeZoneKey.GetValue("Std");
            daylightDisplayName = (string)timeZoneKey.GetValue("Dlt");

            return displayName;
        }

        public static bool IsDaylightSavingsEnabled()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\TimeZoneInformation");
            int value = (int)key.GetValue("DisableAutoDaylightTimeSet", 0);
            return (value == 0);
        }
    }
}
