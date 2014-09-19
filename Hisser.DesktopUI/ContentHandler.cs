/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Hisser.ClientServer;

namespace Hisser.DesktopUI
{
    /// <summary>
    /// Handles content sent or received as MessageData.
    /// </summary>
    public class ContentHandler
    {
        /// <summary>
        /// Gets the content of this Message Data as an Image.
        /// </summary>
        /// <returns></returns>
        public static Image GetImage(MessageData message)
        {
            return Image.FromStream(new MemoryStream(message.Content));
        }

        /// <summary>
        /// Gets the content of this Message Data as a String.
        /// </summary>
        /// <returns></returns>
        public static string GetString(MessageData message)
        {
            return Encoding.UTF8.GetString(message.Content);
        }

        /// <summary>
        /// Converts the content represented by a given object to a byte array that can be sent as MessageData.
        /// </summary>
        /// <param name="type">The type of object.</param>
        /// <param name="content">The object to be converted.</param>
        /// <returns>The object represented as a byte array.</returns>
        public static byte[] GetBytes(ContentType type, object content)
        {
            switch (type)
            {
                case ContentType.TEXT_PLAIN:
                    {
                        return Encoding.ASCII.GetBytes((string)content);
                    }
                case ContentType.IMAGE_JPEG:
                    {
                        MemoryStream ms = new MemoryStream();
                        ((Image)content).Save(ms, ImageFormat.Jpeg);
                        return ms.ToArray();
                    }
                case ContentType.IMAGE_GIF:
                    {
                        MemoryStream ms = new MemoryStream();
                        ((Image)content).Save(ms, ImageFormat.Gif);
                        return ms.ToArray();
                    }
                case ContentType.IMAGE_PNG:
                    {
                        MemoryStream ms = new MemoryStream();
                        ((Image)content).Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                default:
                    {
                        throw new UnknownContentTypeException(type);
                    }
            }
        }
    }
}
