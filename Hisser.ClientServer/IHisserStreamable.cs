/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Threading.Tasks;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents an object that can be serialized to a HisserStream.
    /// </summary>
    public interface IHisserStreamable
    {
        /// <summary>
        /// Serializes the object to the given Hisser Stream.
        /// </summary>
        /// <param name="stream">The stream this object should be serialized to.</param>
        Task ToStream(HisserStream stream);
    }
}
