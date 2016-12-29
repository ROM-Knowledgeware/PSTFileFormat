using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace PSTFileFormat
{
    public class HeapOrNodeID // HNID
    {
        private HeapID m_heapID;
        private NodeID m_nodeID;

        public HeapOrNodeID(HeapID heapID)
        {
            m_heapID = heapID;
        }

        public HeapOrNodeID(NodeID nodeID)
        {
            m_nodeID = nodeID;
        }

        public HeapOrNodeID(byte[] buffer) : this(buffer, 0)
        {

        }

        public HeapOrNodeID(byte[] buffer, int offset)
        {
            HeapID tempHID = new HeapID(buffer, offset);
            if (tempHID.hidType == NodeTypeName.NID_TYPE_HID)
            {
                m_heapID = tempHID;
            }
            else
            {
                m_nodeID = new NodeID(buffer, offset);
            }
        }

        public bool IsHeapID
        {
            get
            {
                return (m_heapID != null);
            }
        }

        public bool IsEmpty
        {
            get
            {
                // Note if the uint value in the buffer is 0, then IsHeapID == true
                return (IsHeapID && m_heapID.Value == 0);
            }
        }

        /*public bool IsNodeID
        {
            get
            {
                return (m_nodeID != null);
            }
        }*/

        public HeapID HeapID
        {
            get
            {
                return m_heapID;
            }
        }

        public NodeID NodeID
        {
            get
            {
                return m_nodeID;
            }
        }

        public uint Value
        {
            get
            {
                if (IsHeapID)
                {
                    return m_heapID.Value;
                }
                else
                {
                    return m_nodeID.Value;
                }
            }
        }
    }
}
