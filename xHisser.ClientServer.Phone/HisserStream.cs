/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;
using System.IO;
using System.Net;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents a stream of data communicated with a Hisser Server.
    /// </summary>
    public class HisserStream : IDisposable
    {
        private Stream _stream;

        private const int bytemask1 = 255;          //byte ---X
        private const int bytemask2 = 65280;        //byte --X-
        private const int bytemask3 = 16711680;     //byte -X--
        
        /// <summary>
        /// Creates a wrapper around the given stream that enables you to easily send numbers, strings and byte arrays
        /// over that stream in a way a Hisser server understands.
        /// </summary>
        /// <param name="stream"></param>
        public HisserStream(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Reads a Hisser [int(1)], for example byte [3] is returned as 3.
        /// </summary>
        /// <returns>The int(1) read.</returns>
        public int ReadInt1()
        {
            return _stream.ReadByte();
        }

        /// <summary>
        /// Reads a Hisser [int(1) length, string], for example bytes [3][65][66][67] are returned as "ABC".
        /// </summary>
        /// <returns>The string read.</returns>
        public string ReadString1()
        {
            int stringLength = ReadInt1();
            return ReadString(stringLength);
        }

        /// <summary>
        /// Reads a Hisser [int(1) length, byte[]], for example bytes [3][65][66][67] are returned as byte[65,66,67].
        /// </summary>
        /// <returns>The byte[] read.</returns>
        public byte[] ReadBytes1()
        {
            int stringLength = ReadInt1();
            return ReadBytes(stringLength);
        }

        /// <summary>
        /// Writes a Hisser int(1) to the stream.
        /// </summary>
        /// <param name="towrite">The int to be written.</param>
        public void WriteInt1(int towrite)
        {
            _stream.WriteByte((byte)towrite);
        }

        /// <summary>
        /// Writes a Hisser [int(1) length, string] to the stream.
        /// </summary>
        /// <param name="toWrite">The string to be written.</param>
        public void WriteString1(string toWrite)
        {
            WriteBytes1(Encoding.UTF8.GetBytes(toWrite));
        }

        /// <summary>
        /// Writes a Hisser [int(1) length, byte[]] to the stream.
        /// </summary>
        /// <param name="toWrite"></param>
        public void WriteBytes1(byte[] toWrite)
        {
            WriteInt1(toWrite.Length);
            _stream.Write(toWrite, 0, toWrite.Length);
        }

        /// <summary>
        /// Reads a Hisser [int(2)], for example byte [4][3] is returned as (4)*2^8+(3) = 1027.
        /// </summary>
        /// <returns>The int(2) read.</returns>
        public int ReadInt2()
        {
            return _stream.ReadByte() << 8 | _stream.ReadByte();
        }

        /// <summary>
        /// Reads a Hisser [int(2) length, string], for example bytes [0][3][65][66][67] are returned as "ABC".
        /// </summary>
        /// <returns>The string read.</returns>
        public string ReadString2()
        {
            int stringLength = ReadInt2();
            return ReadString(stringLength);
        }

        /// <summary>
        /// Reads a Hisser [int(2) length, byte[]], for example bytes [0][3][65][66][67] are returned as byte[65,66,67].
        /// </summary>
        /// <returns>The byte[] read.</returns>
        public byte[] ReadBytes2()
        {
            int stringLength = ReadInt2();
            return ReadBytes(stringLength);
        }

        /// <summary>
        /// Writes a Hisser int(2) to the stream.
        /// </summary>
        /// <param name="towrite">The int to be written.</param>
        public void WriteInt2(int towrite)
        {
            _stream.WriteByte((byte)(towrite >> 8));
            _stream.WriteByte((byte)(towrite & bytemask1));
        }

        /// <summary>
        /// Writes a Hisser [int(2) length, string] to the stream.
        /// </summary>
        /// <param name="toWrite">The string to be written.</param>
        public void WriteString2(string toWrite)
        {
            byte[] bytesToWrite = Encoding.UTF8.GetBytes(toWrite);
            WriteInt2(bytesToWrite.Length);
            _stream.Write(bytesToWrite, 0, bytesToWrite.Length);
        }

        /// <summary>
        /// Writes a Hisser [int(2) length, byte[]] to the stream.
        /// </summary>
        /// <param name="toWrite">The byte[] to be written.</param>
        public void WriteBytes2(byte[] toWrite)
        {
            WriteInt2(toWrite.Length);
            _stream.Write(toWrite, 0, toWrite.Length);
        }

        /// <summary>
        /// Reads a Hisser [int(4)], for example byte [6][5][4][3] is returned as (6)*2^24+(5)*2^16+(4)*2^8+(3) = 201655299.
        /// </summary>
        /// <returns>The int(4) read.</returns>
        public long ReadInt4()
        {
            return _stream.ReadByte() << 24 | _stream.ReadByte() << 16 | _stream.ReadByte() << 8 | _stream.ReadByte();
        }

        /// <summary>
        /// Reads a Hisser [int(1) length, string], for example bytes [0][0][0][3][65][66][67] are returned as "ABC".
        /// </summary>
        /// <returns>The string read.</returns>
        public string ReadString4()
        {
            int stringLength = Convert.ToInt32(ReadInt4());
            return ReadString(stringLength);
        }

        /// <summary>
        /// Reads a Hisser [int(4) length, byte[]], for example bytes [0][0][0][0][3][65][66][67] are returned as byte[65,66,67].
        /// </summary>
        /// <returns>The byte[] read.</returns>
        public byte[] ReadBytes4()
        {
            int stringLength = Convert.ToInt32(ReadInt4());
            return ReadBytes(stringLength);
        }

        /// <summary>
        /// Writes a Hisser int(4) to the stream.
        /// </summary>
        /// <param name="towrite"></param>
        public void WriteInt4(long towrite)
        {
            _stream.WriteByte((byte)(towrite >> 24));
            _stream.WriteByte((byte)(towrite & bytemask3 >> 16));
            _stream.WriteByte((byte)(towrite & bytemask2 >> 8));
            _stream.WriteByte((byte)(towrite & bytemask1));
        }

        /// <summary>
        /// Writes a Hisser [int(4) length, string] to the stream.
        /// </summary>
        /// <param name="toWrite">The string to be written.</param>
        public void WriteString4(string toWrite)
        {
            byte[] bytesToWrite = Encoding.UTF8.GetBytes(toWrite);
            WriteInt4(bytesToWrite.Length);
            _stream.Write(bytesToWrite, 0, bytesToWrite.Length);
        }

        /// <summary>
        /// Writes a Hisser [int(4) length, byte[]] to the stream.
        /// </summary>
        /// <param name="toWrite">The byte[] to be written.</param>
        public void WriteBytes4(byte[] toWrite)
        {
            WriteInt4(toWrite.Length);
            _stream.Write(toWrite, 0, toWrite.Length);
        }

        /// <summary>
        /// Writes a [key,value] pair to the Hisser stream in format key=value&key=value..., where key and value are urlencoded.
        /// </summary>
        /// <param name="key">The key (variable name).</param>
        /// <param name="value">The value (variable value).</param>
        public void WriteKeyValue(string key, long value)
        {
            WriteKeyValue(key, new byte[] {
                (byte)(value >> 24),
                (byte)(value & bytemask3),
                (byte)(value & bytemask2),
                (byte)(value & bytemask1),
            });
        }

        /// <summary>
        /// Writes a [key,value] pair to the Hisser stream in format key=value&key=value..., where key and value are urlencoded.
        /// </summary>
        /// <param name="key">The key (variable name).</param>
        /// <param name="value">The value (variable value).</param>
        public void WriteKeyValue(string key, string value)
        {
            WriteKeyValue(key, Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// Writes a [key,value] pair to the Hisser stream in format key=value&key=value..., where key and value are urlencoded.
        /// </summary>
        /// <param name="key">The key (variable name).</param>
        /// <param name="value">The value (variable value).</param>
        public void WriteKeyValue(string key, byte[] value)
        {
            WriteKey(key);
            byte[] toWriteEncoded = WebUtility.UrlEncodeToBytes(value, 0, value.Length);
            _stream.Write(toWriteEncoded, 0, toWriteEncoded.Length);
        }

        /// <summary>
        /// Encodes one Key-Value pair.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <returns>The encoded version of key and value that can be sent to a Hisser Server in a GET request.</returns>
        public static byte[] Encode(string key, string value)
        {
            return Encoding.UTF8.GetBytes(string.Format("{0}={1}", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value)));
        }
        
        private void WriteKey(string key)
        {
            byte[] toWrite;
            if (_stream.Position > 0) {
                toWrite = Encoding.UTF8.GetBytes("&" + WebUtility.UrlEncode(key) + "=");
            }
            else {
                toWrite = Encoding.UTF8.GetBytes(WebUtility.UrlEncode(key) + "=");
            }
            _stream.Write(toWrite, 0, toWrite.Length);
        }

        /// <summary>
        /// Reads a string of the given length.
        /// </summary>
        /// <param name="length">The length of the string to read.</param>
        /// <returns>The string read.</returns>
        private string ReadString(int length)
        {
            byte[] read = new byte[length];
            _stream.Read(read, 0, length);
            return Encoding.UTF8.GetString(read);
        }

        /// <summary>
        /// Reads an array of bytes of the given length.
        /// </summary>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The read byte array.</returns>
        private byte[] ReadBytes(int length)
        {
            byte[] read = new byte[length];
            _stream.Read(read, 0, length);
            return read;
        }
        
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
