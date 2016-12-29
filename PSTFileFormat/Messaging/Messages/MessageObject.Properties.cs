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
    public partial class MessageObject
    {
        public string MessageClass
        {
            get
            {
                return PC.GetStringProperty(PropertyID.PidTagMessageClass);
            }
            set
            {
                PC.SetStringProperty(PropertyID.PidTagMessageClass, value);
            }
        }

        /// <summary>
        /// PidTagSearchKey is not guaranteed to be unique within the folder.
        /// 
        /// Whenever a copy of an object is created, the key is also copied from the original object.
        /// http://msdn.microsoft.com/en-us/library/ee178645%28v=exchg.80%29.aspx
        /// </summary>
        public Guid SearchKey
        {
            get
            {
                byte[] bytes = PC.GetBytesProperty(PropertyID.PidTagSearchKey);
                if (bytes != null)
                {
                    return LittleEndianConverter.ToGuid(bytes, 0);
                }
                return Guid.Empty;
            }
        }

        public string SenderName
        {
            set
            {
                PC.SetStringProperty(PropertyID.PidTagSenderName, value);
            }
        }

        public string SenderAddressType
        {
            set
            {
                PC.SetStringProperty(PropertyID.PidTagSenderAddressType, value);
            }
        }

        public string SenderEmailAddress
        {
            set
            {
                PC.SetStringProperty(PropertyID.PidTagSenderEmailAddress, value);
            }
        }

        public string SentRepresentingName
        {
            set
            {
                PC.SetStringProperty(PropertyID.PidTagSentRepresentingName, value);
            }
        }

        public string SentRepresentingAddressType
        {
            set
            {
                PC.SetStringProperty(PropertyID.PidTagSentRepresentingAddressType, value);
            }
        }

        public string SentRepresentingEmailAddress
        {
            set
            {
                PC.SetStringProperty(PropertyID.PidTagSentRepresentingEmailAddress, value);
            }
        }

        public string DisplayTo
        {
            get
            {
                return PC.GetStringProperty(PropertyID.PidTagDisplayTo);
            }
            set
            {
                PC.SetStringProperty(PropertyID.PidTagDisplayTo, value);
            }
        }

        /// <summary>
        /// Maximum length: 255 characters (including prefix)
        /// 
        /// The first character of PidTagSubject indicates whether metadata exists to tell the reader
        /// how to parse the prefix and normalized subject.
        /// If the first character contains the value of 0x01, the next character indicates the length of the
        /// Subject Prefix, including the separator between the prefix and the normalized subject
        /// (a space character in most cases). The Normalized Subject immediately follows the Subject Prefix.
        /// However, if the first character is not 0x01, then the string contains the entire message subject,
        /// with no additional metadata. In this case, the message subject MUST be parsed to extract the prefix
        /// and normalized subject.
        /// </summary>
        public string SubjectRaw
        {
            get
            {
                return this.PC.GetStringProperty(PropertyID.PidTagSubject);
            }
            set
            {
                PC.SetStringProperty(PropertyID.PidTagSubject, value);
            }
        }

        public string Subject
        {
            get
            {
                string subjectRaw = this.SubjectRaw;
                if (subjectRaw != null)
                {
                    if (subjectRaw.Length >= 2 && subjectRaw[0] == 0x01)
                    {
                        return subjectRaw.Substring(2);
                    }
                }
                return subjectRaw;
            }
            set
            {
                this.SubjectRaw = ((char)0x0001).ToString() + ((char)0x0001).ToString() + value;
                this.ConversationTopic = value;
            }
        }

        public string SubjectPrefix
        {
            get
            {
                string subjectRaw = this.SubjectRaw;
                if (subjectRaw != null)
                {
                    if (subjectRaw.Length >= 2 && subjectRaw[0] == 0x01)
                    {
                        int length = subjectRaw[1] - 1;
                        return subjectRaw.Substring(2, length);
                    }
                }
                return String.Empty;
            }
        }

        public string NormalizedSubject
        {
            get
            {
                string subjectRaw = this.SubjectRaw;
                if (subjectRaw != null)
                {
                    if (subjectRaw.Length >= 2 && subjectRaw[0] == 0x01)
                    {
                        int length = subjectRaw[1] - 1;
                        return subjectRaw.Substring(2 + length);
                    }
                }
                return subjectRaw;
            }
        }

        public string ConversationTopic
        {
            get
            {
                return PC.GetStringProperty(PropertyID.PidTagConversationTopic);
            }
            set
            {
                PC.SetStringProperty(PropertyID.PidTagConversationTopic, value);
            }
        }

        public byte[] ConversationId
        {
            get
            {
                return PC.GetBytesProperty(PropertyID.PidTagConversationId);
            }
            set
            {
                PC.SetBytesProperty(PropertyID.PidTagConversationId, value);
            }
        }

        public string Body
        {
            get
            {
                return PC.GetStringProperty(PropertyID.PidTagBody);
            }
            set
            {
                PC.SetStringProperty(PropertyID.PidTagBody, value);
            }
        }

        public bool ReadReceiptRequested
        {
            set
            {
                PC.SetBooleanProperty(PropertyID.PidTagReadReceiptRequested, value);
            }
        }

        public bool AlternateRecipientAllowed
        {
            set
            {
                PC.SetBooleanProperty(PropertyID.PidTagAlternateRecipientAllowed, value);
            }
        }

        public MessageFlags MessageFlags
        {
            get
            {
                return (MessageFlags)PC.GetInt32Property(PropertyID.PidTagMessageFlags);
            }
            set
            {
                PC.SetInt32Property(PropertyID.PidTagMessageFlags, (int)value);
            }
        }

        public DateTime CreationTime
        {
            set
            {
                PC.SetDateTimeProperty(PropertyID.PidTagCreationTime, value);
            }
        }

        public DateTime MessageDeliveryTime
        {
            set
            {
                PC.SetDateTimeProperty(PropertyID.PidTagMessageDeliveryTime, value);
            }
        }

        public DateTime LastModificationTime
        {
            get
            {
                Nullable<DateTime> result = PC.GetDateTimeProperty(PropertyID.PidTagLastModificationTime);
                if (result.HasValue)
                {
                    return result.Value;
                }
                return DateTime.MinValue;
            }
            set
            {
                PC.SetDateTimeProperty(PropertyID.PidTagLastModificationTime, value);
            }
        }

        public DateTime ClientSubmitTime
        {
            set
            {
                PC.SetDateTimeProperty(PropertyID.PidTagClientSubmitTime, value);
            }
        }

        public int InternetCodepage
        {
            set
            {
                PC.SetInt32Property(PropertyID.PidTagInternetCodepage, value);
            }
        }

        public SideEffectsFlags SideEffects
        {
            set
            {
                PC.SetInt32Property(PropertyNames.PidLidSideEffects, (int)value);
            }
        }

        public IconIndex IconIndex
        {
            set
            {
                PC.SetInt32Property(PropertyID.PidTagIconIndex, (int)value);
            }
        }

        public TaskMode TaskMode
        {
            set
            {
                PC.SetInt32Property(PropertyNames.PidLidTaskMode, (int)value);
            }
        }

        public MessageImportance Importance
        {
            set
            {
                PC.SetInt32Property(PropertyID.PidTagImportance, (int)value);
            }
        }

        public MessagePriority Priority
        {
            set
            {
                PC.SetInt32Property(PropertyID.PidTagPriority, (int)value);
            }
        }

        public MessageSensitivity Sensitivity
        {
            set
            {
                PC.SetInt32Property(PropertyID.PidTagSensitivity, (int)value);
            }
        }
    }
}
