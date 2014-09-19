/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;

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
        public async Task<string> ReadString1()
        {
            int stringLength = ReadInt1();
            return await ReadString(stringLength);
        }

        /// <summary>
        /// Reads a Hisser [int(1) length, byte[]], for example bytes [3][65][66][67] are returned as byte[65,66,67].
        /// </summary>
        /// <returns>The byte[] read.</returns>
        public async Task<byte[]> ReadBytes1()
        {
            int stringLength = ReadInt1();
            return await ReadBytes(stringLength);
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
        public async Task WriteString1(string toWrite)
        {
            await WriteBytes1(Encoding.UTF8.GetBytes(toWrite));
        }

        /// <summary>
        /// Writes a Hisser [int(1) length, byte[]] to the stream.
        /// </summary>
        /// <param name="toWrite"></param>
        public async Task WriteBytes1(byte[] toWrite)
        {
            WriteInt1(toWrite.Length);
            await _stream.WriteAsync(toWrite, 0, toWrite.Length);
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
        public async Task<string> ReadString2()
        {
            int stringLength = ReadInt2();
            return await ReadString(stringLength);
        }

        /// <summary>
        /// Reads a Hisser [int(2) length, byte[]], for example bytes [0][3][65][66][67] are returned as byte[65,66,67].
        /// </summary>
        /// <returns>The byte[] read.</returns>
        public async Task<byte[]> ReadBytes2()
        {
            int stringLength = ReadInt2();
            return await ReadBytes(stringLength);
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
        public async Task WriteString2(string toWrite)
        {
            byte[] bytesToWrite = Encoding.UTF8.GetBytes(toWrite);
            WriteInt2(bytesToWrite.Length);
            await _stream.WriteAsync(bytesToWrite, 0, bytesToWrite.Length);
        }

        /// <summary>
        /// Writes a Hisser [int(2) length, byte[]] to the stream.
        /// </summary>
        /// <param name="toWrite">The byte[] to be written.</param>
        public async Task WriteBytes2(byte[] toWrite)
        {
            WriteInt2(toWrite.Length);
            await _stream.WriteAsync(toWrite, 0, toWrite.Length);
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
        public async Task<string> ReadString4()
        {
            int stringLength = Convert.ToInt32(ReadInt4());
            return await ReadString(stringLength);
        }

        /// <summary>
        /// Reads a Hisser [int(4) length, byte[]], for example bytes [0][0][0][0][3][65][66][67] are returned as byte[65,66,67].
        /// </summary>
        /// <returns>The byte[] read.</returns>
        public async Task<byte[]> ReadBytes4()
        {
            int stringLength = Convert.ToInt32(ReadInt4());
            return await ReadBytes(stringLength);
        }

        /// <summary>
        /// Writes a Hisser int(4) to the stream.
        /// </summary>
        /// <param name="towrite"></param>
        public void WriteInt4(long towrite)
        {
            _stream.WriteByte((byte)(towrite >> 24));
            _stream.WriteByte((byte)((towrite & bytemask3) >> 16));
            _stream.WriteByte((byte)((towrite & bytemask2) >> 8));
            _stream.WriteByte((byte)(towrite & bytemask1));
        }

        /// <summary>
        /// Writes a Hisser [int(4) length, string] to the stream.
        /// </summary>
        /// <param name="toWrite">The string to be written.</param>
        public async Task WriteString4(string toWrite)
        {
            byte[] bytesToWrite = Encoding.UTF8.GetBytes(toWrite);
            WriteInt4(bytesToWrite.Length);
            await _stream.WriteAsync(bytesToWrite, 0, bytesToWrite.Length);
        }

        /// <summary>
        /// Writes a Hisser [int(4) length, byte[]] to the stream.
        /// </summary>
        /// <param name="toWrite">The byte[] to be written.</param>
        public async Task WriteBytes4(byte[] toWrite)
        {
            WriteInt4(toWrite.Length);
            await _stream.WriteAsync(toWrite, 0, toWrite.Length);
        }

        /// <summary>
        /// Reads a string of the given length.
        /// </summary>
        /// <param name="length">The length of the string to read.</param>
        /// <returns>The string read.</returns>
        private async Task<string> ReadString(int length)
        {
            return Encoding.UTF8.GetString(await ReadBytes(length), 0, length);
        }

        /// <summary>
        /// Reads an array of bytes of the given length.
        /// </summary>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The read byte array.</returns>
        private async Task<byte[]> ReadBytes(int length)
        {
            byte[] read = new byte[length];
            int offset = 0;
            int nrOfBytesRead = 0;
            while (offset < length)
            {
                nrOfBytesRead = await _stream.ReadAsync(read, offset, length - offset);
                offset += nrOfBytesRead;

                if (nrOfBytesRead == 0)
                {
                    break;
                }
            }
            return read;
        }
        
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
