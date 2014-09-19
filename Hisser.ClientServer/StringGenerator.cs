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

        /// <summary>
        /// Generates an array of the given length filled with random bytes.
        /// </summary>
        /// <param name="size">The length of the array.</param>
        /// <returns>An array of the given length filled with random bytes.</returns>
        public static byte[] GetBytes(int size)
        {
            byte[] buffer = new byte[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = (byte)_rng.Next(256);
            }

            return buffer;
        }

        /// <summary>
        /// Generates a string of the given length filled with random characters taken from the given string.
        /// </summary>
        /// <param name="size">The length of the array.</param>
        /// <param name="chars">A string containing possible characters to constuct a random string from.</param>
        /// <returns>A string of the given length filled with random characters.</returns>
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
