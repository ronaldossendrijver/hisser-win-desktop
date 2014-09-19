/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents the Status of a Contact.
    /// </summary>
    public enum ContactStatus
    {
        Unknown = 0,

        /// <summary>
        /// You know of this contact, but you haven't Hissed with it (yet)
        /// </summary>
        Known = 1,

        /// <summary>
        /// You have invited this contact, but there is no respons to your invitation (yet)
        /// </summary>
        Invited = 2,

        /// <summary>
        /// This contact has sent you an invitation request, asking for acceptance
        /// </summary>
        Wannabe = 3,

        /// <summary>
        /// You are on speaking terms with this contact
        /// </summary>
        Friend = 4,

        /// <summary>
        /// You have rejected an invitation request of this contact
        /// </summary>
        Rejected = 5,

        /// <summary>
        /// This contact has rejected your invitation request
        /// </summary>
        Unfriendly = 6
    }

    /// <summary>
    /// Types of Hisser Messages.
    /// </summary>
    public enum MessageType
    {
        Unknown = 0,
        InvitationRequest = 1,
        ChatMessage = 2
    }

    /// <summary>
    /// Notification Types supported by Hisser.
    /// </summary>
    public enum NotifyType
    {
        Unknown = 0,
        APNS = 1,
        EMAIL = 2,
        NMA = 3,
        NONE = 4,
        PROWL = 5
    }

    public enum ContentType
    {
        UNDEFINED = 0,
        TEXT_PLAIN = 1,
        IMAGE_JPEG = 2,
        IMAGE_GIF = 3,
        IMAGE_PNG = 4
    }
}
