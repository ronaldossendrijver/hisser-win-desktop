/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using BigMath;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents a Secret Component used to construct a Shared Secret with.
    /// </summary>
    public abstract class SecretComponent
    {
        public virtual long ID { get; protected set; }

        public virtual long Serial { get; protected set; }

        public virtual long ContactID { get; internal set; }

        public virtual byte[] PublicValue { get; protected set; }

        internal SecretComponent(long serial)
        {
            this.Serial = serial;
        }

        public SecretComponent()
        {
        }
    }
}
