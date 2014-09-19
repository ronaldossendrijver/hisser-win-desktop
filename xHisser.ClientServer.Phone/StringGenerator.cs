/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Generates random Strings or Byte Arrays.
    /// </summary>
    public class RandomGenerator
    {
        private static Random _rng = new Random();

        public static byte[] GetBytes(int size)
        {
            byte[] buffer = new byte[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = (byte)_rng.Next(256);
            }

            return buffer;
        }

        public static string GetString(int size, string chars)
        {
            char[] buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = chars[_rng.Next(chars.Length)];
            }

            return new string(buffer);
        }
    }
}
