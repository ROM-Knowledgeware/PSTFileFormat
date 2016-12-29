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
using System.ServiceProcess;
using System.Text;
using Utilities;

namespace PSTFileFormat
{
    public class SearchManagementQueue
    {
        private PSTFile m_file;
        private PSTNode m_node; // SMQ node
        private bool m_isWindowsDesktopSearchQueuing;

        public SearchManagementQueue(PSTFile file)
        {
            m_file = file;
            // http://social.msdn.microsoft.com/Forums/en-US/os_binaryfile/thread/a5f9c653-40f5-4638-85d3-00c54607d984/
            // Outlook 2007+ queue some SUDs if WDS is enabled.
            //m_isWindowsDesktopSearchQueuing = file.WriterCompatibilityMode >= WriterCompatibilityMode.Outlook2007RTM && IsWindowsDesktopSearchIndexingEnabled();
            m_isWindowsDesktopSearchQueuing = false; // I couldn't find a reason to queue these SUDs
        }

        // http://social.msdn.microsoft.com/Forums/en-US/os_binaryfile/thread/a5f9c653-40f5-4638-85d3-00c54607d984/
        // For the following types, search the SDO for the NIDs of the old and new parent folders.
        // If either of the NIDs is contained in the SDO, enqueue the SUD
        private void QueueSearchUpdateDescriptor(SearchUpdateDescriptor sud)
        {
            if (m_node == null)
            {
                m_node = m_file.GetNode((uint)InternalNodeName.NID_SEARCH_MANAGEMENT_QUEUE);
            }

            if (m_node.DataTree == null)
            {
                m_node.DataTree = new DataTree(m_file);
            }
            
            m_node.DataTree.AppendData(sud.GetBytes());
        }

        public void SaveChanges()
        {
            if (m_node != null)
            {
                m_node.SaveChanges();
            }
        }

        public void AddFolder(NodeID folderNodeID, NodeID parentNodeID)
        {
            if (m_isWindowsDesktopSearchQueuing ||
                m_file.SearchDomainObject.ContainsNode(parentNodeID))
            {
                SearchUpdateDescriptorFolderAdded folderAdded = new SearchUpdateDescriptorFolderAdded(parentNodeID, folderNodeID);
                SearchUpdateDescriptor sud = new SearchUpdateDescriptor((SearchUpdateDescriptorFlags)0, SearchUpdateDescriptorType.SUDT_FLD_ADD, folderAdded);
                QueueSearchUpdateDescriptor(sud);
            }
        }

        public void ModifyFolder(NodeID folderNodeID, NodeID parentNodeID)
        {
            if (m_isWindowsDesktopSearchQueuing ||
                m_file.SearchDomainObject.ContainsNode(parentNodeID))
            {
                SearchUpdateDescriptorFolderModified folderModified = new SearchUpdateDescriptorFolderModified(folderNodeID);
                SearchUpdateDescriptor sud = new SearchUpdateDescriptor((SearchUpdateDescriptorFlags)0, SearchUpdateDescriptorType.SUDT_FLD_MOD, folderModified);
                QueueSearchUpdateDescriptor(sud);
            }
        }

        public void DeleteFolder(NodeID folderNodeID, NodeID parentNodeID)
        {
            if (m_isWindowsDesktopSearchQueuing ||
                m_file.SearchDomainObject.ContainsNode(parentNodeID))
            {
                SearchUpdateDescriptorFolderModified folderDeleted = new SearchUpdateDescriptorFolderModified(folderNodeID);
                SearchUpdateDescriptor sud = new SearchUpdateDescriptor((SearchUpdateDescriptorFlags)0, SearchUpdateDescriptorType.SUDT_FLD_DEL, folderDeleted);
                QueueSearchUpdateDescriptor(sud);
            }
        }

        public void AddMessage(NodeID messageNodeID, NodeID folderNodeID)
        {
            if (m_isWindowsDesktopSearchQueuing ||
                m_file.SearchDomainObject.ContainsNode(folderNodeID))
            {
                SearchUpdateDescriptorMessageAdded messageAdded = new SearchUpdateDescriptorMessageAdded(folderNodeID, messageNodeID);
                SearchUpdateDescriptor sud = new SearchUpdateDescriptor((SearchUpdateDescriptorFlags)0, SearchUpdateDescriptorType.SUDT_MSG_ADD, messageAdded);
                QueueSearchUpdateDescriptor(sud);
            }
        }

        public void ModifyMessage(NodeID messageNodeID, NodeID folderNodeID, bool isContentsTableModified)
        {
            bool sdoContainsNode = m_file.SearchDomainObject.ContainsNode(folderNodeID);
            if (m_isWindowsDesktopSearchQueuing ||
                sdoContainsNode)
            {
                SearchUpdateDescriptorMessageAdded messageModified = new SearchUpdateDescriptorMessageAdded(folderNodeID, messageNodeID);
                SearchUpdateDescriptor sud = new SearchUpdateDescriptor((SearchUpdateDescriptorFlags)0, SearchUpdateDescriptorType.SUDT_MSG_MOD, messageModified);
                QueueSearchUpdateDescriptor(sud);

                if (isContentsTableModified && sdoContainsNode)
                {
                    SearchUpdateDescriptorMessageAdded messageRowModified = new SearchUpdateDescriptorMessageAdded(folderNodeID, messageNodeID);
                    SearchUpdateDescriptor sudRow = new SearchUpdateDescriptor((SearchUpdateDescriptorFlags)0, SearchUpdateDescriptorType.SUDT_MSG_ROW_MOD, messageRowModified);
                    QueueSearchUpdateDescriptor(sudRow);
                }
            }
        }

        public void DeleteMessage(NodeID messageNodeID, NodeID folderNodeID)
        {
            if (m_isWindowsDesktopSearchQueuing ||
                m_file.SearchDomainObject.ContainsNode(folderNodeID))
            {
                SearchUpdateDescriptorMessageAdded messageDeleted = new SearchUpdateDescriptorMessageAdded(folderNodeID, messageNodeID);
                SearchUpdateDescriptor sud = new SearchUpdateDescriptor((SearchUpdateDescriptorFlags)0, SearchUpdateDescriptorType.SUDT_MSG_DEL, messageDeleted);
                QueueSearchUpdateDescriptor(sud);
            }
        }

        public static bool IsWindowsDesktopSearchIndexingEnabled()
        {
            ServiceController service = FindService("WSearch");
            if (service != null)
            {
                if (service.Status == ServiceControllerStatus.Running)
                {
                    // FIXME - For now we assume Outlook is being indexed
                    return true;
                }
            }
            return false;
        }

        public static ServiceController FindService(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController service in services)
            {
                if (service.ServiceName.Equals(serviceName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return service;
                }
            }
            return null;
        }
    }
}
