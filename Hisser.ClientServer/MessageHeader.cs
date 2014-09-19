/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;
using System.Threading.Tasks;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents the Header of an Invitation or Chat Message.
    /// </summary>
    public class MessageHeader
    {
        /// <summary>
        /// Identifier of the message.
        /// </summary>
        public long ID {get; private set;}

        /// <summary>
        /// Size of the message.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// The type of message.
        /// </summary>
        public MessageType Type { get; private set; }

        private MessageHeader(long identifier, long size, MessageType type)
        {
            ID = identifier;
            Size = size;
            Type = type;
        }

        /// <summary>
        /// Reads a Message Header from a Hisser stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<MessageHeader> FromStream(HisserStream stream)
        {
            long messageID = stream.ReadInt4();
            long messageSize = stream.ReadInt4();
            MessageType messageType = MessageHeader.ParseMessageType(await stream.ReadString1());
            return new MessageHeader(messageID, messageSize, messageType);
        }

        public static MessageType ParseMessageType(string message_type_string)
        {
            switch (message_type_string)
            {
                case "message": return MessageType.ChatMessage;
                case "invitation": return MessageType.InvitationRequest;
                default:
                    {
                        throw new UnknownMessageTypeException(message_type_string);
                    }
            }
        }

        public static string MessageType_ToString(MessageType message_type)
        {
            switch (message_type)
            {
                case MessageType.InvitationRequest: return "invite";
                case MessageType.ChatMessage: return "message";
                default:
                    {
                        throw new UnknownMessageTypeException(message_type);
                    }
            }
        }
    }
}
