/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hisser.ClientServer;

namespace Hisser.ClientServer
{
    public class TechnicalException : Exception
    {
        public TechnicalException(string message)
            : base(message)
        {
        }
    }

    public class FunctionalException : Exception
    {
        public FunctionalException(string message)
            : base(message)
        {
        }
    }

    public class MethodNotAllowedException : FunctionalException
    {
        public MethodNotAllowedException()
            : base("The Hisser Server does not support your request. Please check if the Server has the same Hisser version as you")
        {
        }
    }

    public class InternalServerErrorException : FunctionalException
    {
        public InternalServerErrorException()
            : base("Error on the Hisser Server. Please try again later")
        {
        }
    }

    public class IncorrectAliasException : TechnicalException
    {
        public IncorrectAliasException()
            : base("Incorrect Alias")
        {

        }
    }
     
    public class AuthenticationRequiredException : FunctionalException
    {
        public AuthenticationRequiredException()
            : base("Unable to log in. Please verify your username, password and certificates")
        {
        }
    }

    public class AccountExpiredException : FunctionalException
    {
        public AccountExpiredException()
            : base("Your Hisser account has expired")
        {
        }
    }

    public class AliasAlreadyExistsException : TechnicalException
    {
        public AliasAlreadyExistsException()
            : base("Alias already exists")
        {
        }
    }

    public class AliasDoesNotExistException : TechnicalException
    {
        public AliasDoesNotExistException()
            : base("Alias does not exist")
        {
        }
    }

    public class IncorrectOrIncompleteRequestException : TechnicalException
    {
        public IncorrectOrIncompleteRequestException()
            : base("The Hisser Server does not support your request. Please check if the Server has the same Hisser version as you")
        {
        }
    }

    public class UnknownRecieverException : FunctionalException
    {
        public UnknownRecieverException(Contact c)
            : base(string.Format("Contact {0} is unknown by the server. Perhaps the account has been removed.", c.Address))
        {
        }
    }

    public class InvalidMessageIdException : TechnicalException
    {
        public InvalidMessageIdException(long message_identifier)
            : base(string.Format("Invalid Message ID: {0}", message_identifier))
        {
        }
    }

    public class MessageNotFoundException : TechnicalException
    {
        public MessageNotFoundException(long message_identifier)
            : base(string.Format("Unknown Message ID: {0}", message_identifier))
        {
        }
    }

    public class InvalidDeviceTokenException : TechnicalException
    {
        public InvalidDeviceTokenException()
            : base("The Hisser Server does not support your device. Please check if the Server has the same Hisser version as you")
        {
        }
    }

    public class InvalidAPIKeyException : TechnicalException
    {
        public InvalidAPIKeyException()
            : base("The Hisser Server does not support your device. Please check if the Server has the same Hisser version as you")
        {
        }
    }

    public class IncorrectTypeOrTokenException : TechnicalException
    {
        public IncorrectTypeOrTokenException()
            : base("The Hisser Server does not support your notification type or token. Please check if the Server has the same Hisser version as you")
        {
        }

        public IncorrectTypeOrTokenException(string notification_type)
            : base(string.Format("The Hisser Server does not support your notification type or token ({0}). Please check if the Server has the same Hisser version as you", notification_type))
        {
        }

        public IncorrectTypeOrTokenException(NotifyType notification_type)
            : base(string.Format("The Hisser Server does not support your notification type or token ({0}). Please check if the Server has the same Hisser version as you", notification_type.ToString()))
        {
        }
    }

    public class UnknownContentTypeException : FunctionalException
    {
        public UnknownContentTypeException(string contentType)
            : base(string.Format("You received a message with unknown content type {0}. Please check if the Server has the same Hisser version as you", contentType))
        {
        }

        public UnknownContentTypeException(ContentType contentType)
            : base(string.Format("You are sending a message with unknown content type {0}. Please check if the Server has the same Hisser version as you", contentType.ToString()))
        {
        }

    }

    public class UnknownMessageTypeException : FunctionalException
    {
        public UnknownMessageTypeException(string contentType)
            : base(string.Format("You received a message with unknown message type {0}. Please check if the Server has the same Hisser version as you", contentType))
        {
        }

        public UnknownMessageTypeException(MessageType contentType)
            : base(string.Format("You are sending a message with unknown message type {0}. Please check if the Server has the same Hisser version as you", contentType.ToString()))
        {
        }
    }

    public class MaxDeviceCountReachedException : TechnicalException
    {
        public MaxDeviceCountReachedException()
            : base("You have reached the maximum number of devices associated with your user account")
        {
        }
    }

    public class NoAliasesFoundException : TechnicalException
    {
        public NoAliasesFoundException()
            : base("No Aliases found.")
        {
        }
    }

    public class UnexpectedResponseException : TechnicalException
    {
        public UnexpectedResponseException(string response)
            : base(string.Format("Unexpected return code from server: {0}", response))
        {
        }
    }

    public class NoConnectionException : FunctionalException
    {
        public NoConnectionException()
            : base("Cannot connect to the server. Please check your server address and Internet connection")
        {
        }
    }

    public class UnknownAliasException : TechnicalException
    {
        public UnknownAliasException()
            : base("You received a message from a Contact that you never sent an Alias")
        {
        }
    }

    public class DatabaseCreationException : FunctionalException
    {
        public DatabaseCreationException()
            : base("Unable to create a database to store your messages and contacts. Please make sure that SQL Server Compact Edition has been installed on your device.")
        {
        }
    }
}
 