/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;
using System.Threading.Tasks;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents a physical Device like a Phone used to run a Hisser Client.
    /// </summary>
    public class Notification : IHisserStreamable
    {
        public NotifyType NotifyType { get; private set; }

        public string NotifyToken { get; private set; }

        public Notification(NotifyType notify_type, string notify_token)
        {
            NotifyType = notify_type;
            NotifyToken = notify_token;
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
                        throw new IncorrectTypeOrTokenException(notify_type_string);
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
                    throw new IncorrectTypeOrTokenException(notify_type);
                }
            }
        }

        public async Task ToStream(HisserStream stream)
        {
            await stream.WriteString1(NotifyType_ToString(NotifyType));
            await stream.WriteString1(NotifyToken);
        }
    }
}
