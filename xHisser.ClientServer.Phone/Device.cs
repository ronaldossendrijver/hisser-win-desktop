/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents a physical Device like a Phone used to run a Hisser Client.
    /// </summary>
    public class Device : IHisserStreamable
    {
        /// <summary>
        /// 
        /// </summary>
        public string Identifier { get; private set; }

        public NotifyType NotifyType { get; private set; }

        public string NotifyToken { get; private set; }

        public Device(string identifier, NotifyType notify_type, string notify_token)
        {
            Identifier = identifier;
            NotifyType = notify_type;
            NotifyToken = notify_token;
        }

        public Device(string identifier) : this(identifier, NotifyType.NONE, "")
        {
        }
        
        public static NotifyType ParseNotifyType(string notify_type_string)
        {
            switch (notify_type_string)
            {
                case "apns": return NotifyType.APNS;
                case "email": return NotifyType.EMAIL;
                case "nma": return NotifyType.NMA;
                case "none": return NotifyType.NONE;
                case "prowl": return NotifyType.PROWL;
                default:
                    {
                        throw new NotImplementedException(string.Format("Unsupported notification type: {0}", notify_type_string));
                    }
            }
        }

        public static string NotifyType_ToString(NotifyType notify_type)
        {
            switch (notify_type)
            {
                case NotifyType.APNS: return "apns"; 
                case NotifyType.EMAIL: return "email"; 
                case NotifyType.NMA: return "nma"; 
                case NotifyType.NONE: return "none"; 
                case NotifyType.PROWL: return "prowl"; 
                default: {
                    throw new NotImplementedException(string.Format("Unsupported notification type: {0}", notify_type));
                }
            }
        }

        public void ToStream(HisserStream stream)
        {
            stream.WriteKeyValue("device", Identifier);
            stream.WriteKeyValue("notify_type", NotifyType_ToString(NotifyType));
            stream.WriteKeyValue("notify_token", NotifyToken);
        }
    }
}
