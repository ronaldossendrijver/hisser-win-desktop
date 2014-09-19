/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents a Hisser Server.
    /// </summary>
    public class Server
    {
        private string _hostname;
        private NetworkCredential _credentials;

        /// <summary>
        /// Defines a Hisser Server.
        /// </summary>
        /// <param name="hostname">The hostname of the server, for example: www.hisser.nl.</param>
        /// <param name="credentials">The credentials to log onto the Server with.</param>
        public Server(string hostname, NetworkCredential credentials)
        {
            _hostname = hostname;
            _credentials = credentials;
        }

        /// <summary>
        /// Returns all Aliases of the current user.
        /// </summary>
        /// <returns>A list of Aliases.</returns>
        public async Task<List<string>> GetAliases()
        {
            List<string> result = new List<string>();

            using (HttpWebResponse response = await PerformRequestAsync("GET", "/alias"))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK: //200
                        {
                            using (HisserStream stream = new HisserStream(response.GetResponseStream()))
                            {
                                int nrOfstringes = stream.ReadInt2();
                                for (int i = 0; i < nrOfstringes; i++)
                                {
                                    if (i==45) {
                                        System.Diagnostics.Debug.Print(i.ToString());
                                    }
                                    result.Add(stream.ReadString1());
                                }
                            }
                            break;
                        }
                    case HttpStatusCode.MethodNotAllowed: throw new MethodNotAllowedException(); //405
                    case HttpStatusCode.InternalServerError: throw new InternalServerErrorException(); //500
                    case HttpStatusCode.NoContent: break; //204
                    case HttpStatusCode.Unauthorized: throw new AuthenticationRequiredException(); //401
                    default: throw new UnexpectedResponseException(response.StatusCode.ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a new Alias for this user on the Server.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns>An awaitable task.</returns>
        public async Task CreateAlias(string alias)
        {
            using (HttpWebResponse response = await PerformRequestAsync("POST", "/alias/create", HisserStream.Encode("alias", alias)))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK: break; //200
                    case HttpStatusCode.MethodNotAllowed: throw new MethodNotAllowedException(); //405
                    case HttpStatusCode.InternalServerError: throw new InternalServerErrorException(); //500
                    case HttpStatusCode.Created: break; //201
                    case HttpStatusCode.BadRequest: throw new IncorrectAliasException(); //400
                    case HttpStatusCode.Unauthorized: throw new AuthenticationRequiredException(); //401
                    case HttpStatusCode.PaymentRequired: throw new AccountExpiredException(); //402
                    case HttpStatusCode.Conflict: throw new AliasAlreadyExistsException(); //409
                    default: throw new UnexpectedResponseException(response.StatusCode.ToString());
                }
            }
        }

        /// <summary>
        /// Deletes the given Alias for this user from the Hisser Server.
        /// </summary>
        /// <param name="alias">The Alias to delete.</param>
        /// <returns>An awaitable task.</returns>
        public async Task DeleteAlias(string alias)
        {
            using (HttpWebResponse response = await PerformRequestAsync("POST", "/alias/delete", HisserStream.Encode("alias", alias)))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK: break; //200
                    case HttpStatusCode.MethodNotAllowed: throw new MethodNotAllowedException(); //405
                    case HttpStatusCode.InternalServerError: throw new InternalServerErrorException(); //500
                    case HttpStatusCode.BadRequest: throw new IncorrectAliasException(); //400
                    case HttpStatusCode.Unauthorized: throw new AuthenticationRequiredException(); //401
                    case HttpStatusCode.NotFound: throw new AliasDoesNotExistException(); //404
                    default: throw new NotImplementedException(string.Format("Unexpected return code from server: {0}", response.StatusCode));
                }
            }
        }

        /// <summary>
        /// Sends the given Invitation.
        /// </summary>
        /// <param name="invitation">The Invitation to send.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SendInvitation(SentInvitation invitation)
        {
            using (HttpWebResponse response = await PerformRequestAsync("POST", "/contactlist", invitation))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Created: break; //201
                    case HttpStatusCode.MethodNotAllowed: throw new MethodNotAllowedException(); //405
                    case HttpStatusCode.InternalServerError: throw new InternalServerErrorException(); //500
                    case HttpStatusCode.BadRequest: throw new IncorrectAliasException(); //400
                    case HttpStatusCode.NotFound: throw new AliasDoesNotExistException(); //404
                    default: throw new NotImplementedException(string.Format("Unexpected return code from server: {0}", response.StatusCode));
                }
            }
        }

        /// <summary>
        /// Retrieves the headers of all messages waiting on the Server for the given Device.
        /// </summary>
        /// <param name="device">The device to retrieve messages for.</param>
        /// <returns>A list of Message Headers.</returns>
        public async Task<List<MessageHeader>> GetMessageHeaders(Device device)
        {
            List<MessageHeader> result = new List<MessageHeader>();

            using (HttpWebResponse response = await PerformRequestAsync("GET", string.Format("/message/index?device={0}", device.Identifier)))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK: //200
                        {
                            using (HisserStream stream = new HisserStream(response.GetResponseStream()))
                            {
                                int nrOfMessages = stream.ReadInt2();
                                for (long i = 0; i < nrOfMessages; i++)
                                {
                                    result.Add(MessageHeader.FromStream(stream));
                                }
                            }

                            break;
                        }
                    case HttpStatusCode.NoContent: break;
                    case HttpStatusCode.MethodNotAllowed: throw new MethodNotAllowedException(); //405
                    case HttpStatusCode.InternalServerError: throw new InternalServerErrorException(); //500
                    case HttpStatusCode.Unauthorized: throw new AuthenticationRequiredException(); //401
                    case HttpStatusCode.PaymentRequired: throw new AccountExpiredException(); //402
                    case HttpStatusCode.Forbidden: throw new InvalidDeviceIdentifierException(device); //403
                    default: throw new NotImplementedException(string.Format("Unexpected return code from server: {0}", response.StatusCode));
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves the message indicated by the given Message Header from the server.
        /// </summary>
        /// <param name="message_header">The message header that identifies the message to retrieve.</param>
        /// <param name="context">The ContactManager that holds information about known Contacts.</param>
        /// <returns>The Message retrieved from the server.</returns>
        public async Task<Message> GetMessage(MessageHeader message_header, ContactManager context)
        {
            using (HttpWebResponse response = await PerformRequestAsync("GET", string.Format("/message?id={0}", message_header.ID))) {

                switch (response.StatusCode)  //200
                {
                    case HttpStatusCode.OK:
                        {
                            using (HisserStream stream = new HisserStream(response.GetResponseStream()))
                            {
                                switch (message_header.Type)
                                {
                                    case MessageType.InvitationRequest:
                                        {
                                            return ReceivedInvitation.FromStream(message_header, stream);
                                        }
                                    case MessageType.ChatMessage:
                                        {
                                            return ReceivedMessage.FromStream(message_header, stream, context);
                                        }
                                    default:
                                        {
                                            throw new NotImplementedException(string.Format("Unsupported message type: {0}", message_header.Type));
                                        }
                                }
                            }
                        }
                    case HttpStatusCode.MethodNotAllowed: throw new MethodNotAllowedException(); //405
                    case HttpStatusCode.InternalServerError: throw new InternalServerErrorException(); //500
                    case HttpStatusCode.BadRequest: throw new InvalidMessageIdException(message_header.ID); //400
                    case HttpStatusCode.Unauthorized: throw new AuthenticationRequiredException(); //401
                    case HttpStatusCode.PaymentRequired: throw new AccountExpiredException(); //402
                    case HttpStatusCode.NotFound: throw new MessageNotFoundException(message_header.ID); //404
                    default: throw new NotImplementedException(string.Format("Unexpected return code from server: {0}", response.StatusCode));
                }
            }
        }

        /// <summary>
        /// Sends the given Mesage.
        /// </summary>
        /// <param name="message">The Message to send.</param>
        /// <returns>An awaitable task.</returns>
        public async Task SendMessage(SentMessage message)
        {
            using (HttpWebResponse response = await PerformRequestAsync("POST", "/message", message))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK: break; //200
                    case HttpStatusCode.MethodNotAllowed: throw new MethodNotAllowedException(); //405
                    case HttpStatusCode.InternalServerError: throw new InternalServerErrorException(); //500
                    case HttpStatusCode.Created: break; //201
                    case HttpStatusCode.BadRequest: throw new IncorrectOrIncompleteRequestException(); //400
                    default: throw new NotImplementedException(string.Format("Unexpected return code from server: {0}", response.StatusCode));
                }
            }
        }

        /// <summary>
        /// Deletes the message indicated by the Message Header from the server.
        /// </summary>
        /// <param name="message_header">The header indicating the message to be deleted.</param>
        /// <returns>An awaitable task.</returns>
        public async Task DeleteMessage(MessageHeader message_header)
        {
            using (HttpWebResponse response = await PerformRequestAsync("POST", "/message/delete", HisserStream.Encode("id", message_header.ID.ToString())))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK: break; //200
                    case HttpStatusCode.MethodNotAllowed: throw new MethodNotAllowedException(); //405
                    case HttpStatusCode.InternalServerError: throw new InternalServerErrorException(); //500
                    case HttpStatusCode.BadRequest: throw new InvalidMessageIdException(message_header.ID); //400
                    case HttpStatusCode.Unauthorized: throw new AuthenticationRequiredException(); //401
                    case HttpStatusCode.NotFound: throw new MessageNotFoundException(message_header.ID); //404
                    default: throw new NotImplementedException(string.Format("Unexpected return code from server: {0}", response.StatusCode));
                }
            }
        }

        /// <summary>
        /// Registers the given device on the server.
        /// </summary>
        /// <param name="device">The device to register.</param>
        /// <returns>An awaitable task.</returns>
        public async Task RegisterDevice(Device device)
        {
            using (HttpWebResponse response = await PerformRequestAsync("POST", "/register", device))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Created: break; //201
                    case HttpStatusCode.MethodNotAllowed: throw new MethodNotAllowedException(); //405
                    case HttpStatusCode.InternalServerError: throw new InternalServerErrorException(); //500
                    case HttpStatusCode.BadRequest: throw new IncorrectTypeOrTokenException(); //400
                    case HttpStatusCode.Unauthorized: throw new AuthenticationRequiredException(); //401
                    case HttpStatusCode.PaymentRequired: throw new AccountExpiredException(); //402
                    case HttpStatusCode.Forbidden: throw new MaxDeviceCountReachedException(); //403
                    default: throw new NotImplementedException(string.Format("Unexpected return code from server: {0}", response.StatusCode));
                }
            }
        }
        
        private Task<HttpWebResponse> PerformRequestAsync(string method, string operation)
        {
            return PerformRequestAsync(method, operation, new byte[0]);
        }

        private Task<HttpWebResponse> PerformRequestAsync(string method, string operation, IHisserStreamable toSend)
        {
            MemoryStream buffer = new MemoryStream();
            HisserStream stream = new HisserStream(buffer);
            toSend.ToStream(stream);
            return PerformRequestAsync(method, operation, buffer.ToArray());
        }

        private async Task<HttpWebResponse> PerformRequestAsync(string method, string operation, byte[] toSend)
        {
            Uri uri = new Uri("http://" + _hostname + operation);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);

            req.Method = method;
            req.ContentType = "application/x-www-form-urlencoded";        
            req.Credentials = _credentials;

            if (toSend.Length != 0)
            {
                req.ContentLength = toSend.Length;

                using (Stream stream = await req.GetRequestStreamAsync())
                {
                    stream.Write(toSend, 0, toSend.Length);
                    stream.Close();
                }
            }

            try
            {
                return (HttpWebResponse)await req.GetResponseAsync();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    return (HttpWebResponse)e.Response;
                }
                else if (e.Status == WebExceptionStatus.Timeout || e.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    throw new NoConnectionException();
                }
                else
                {
                    throw e;
                }
            }
        }
    }

}
