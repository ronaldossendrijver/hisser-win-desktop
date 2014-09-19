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
            : base("Incorrect string")
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

    public class InvalidDeviceIdentifierException : TechnicalException
    {
        public InvalidDeviceIdentifierException(Device d)
            : base(string.Format("Device {0} is unknown to the server.", d.Identifier))
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

    public class InvalidDeviceTokenException : FunctionalException
    {
        public InvalidDeviceTokenException()
            : base("The Hisser Server does not support your device. Please check if the Server has the same Hisser version as you")
        {
        }
    }

    public class InvalidAPIKeyException : FunctionalException
    {
        public InvalidAPIKeyException()
            : base("The Hisser Server does not support your device. Please check if the Server has the same Hisser version as you")
        {
        }
    }

    public class IncorrectTypeOrTokenException : FunctionalException
    {
        public IncorrectTypeOrTokenException()
            : base("The Hisser Server does not support your device. Please check if the Server has the same Hisser version as you")
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

    public class MaxDeviceCountReachedException : FunctionalException
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
}
 