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
    public partial class ModifiedAppointmentInstance
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
        /// Maximum length: 255 characters
        /// </summary>
        public string Subject
        {
            get
            {
                return this.PC.GetStringProperty(PropertyID.PidTagSubject);
            }
            set
            {
                PC.SetStringProperty(PropertyID.PidTagSubject, value);
                PC.SetStringProperty(PropertyID.PidTagConversationTopic, value);
            }
        }

        public string Body
        {
            get
            {
                return this.PC.GetStringProperty(PropertyID.PidTagBody);
            }
            set
            {
                PC.SetStringProperty(PropertyID.PidTagBody, value);
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
