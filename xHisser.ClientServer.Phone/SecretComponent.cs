/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using BigMath;

namespace Hisser.ClientServer
{
    public abstract class SecretComponent
    {
        public virtual long ID { get; protected set; }

        public virtual long Serial { get; protected set; }

        public virtual long ContactID { get; internal set; }

        public virtual BigInteger PublicValue { get; protected set; }

        internal SecretComponent(long serial)
        {
            this.Serial = serial;
        }

        internal SecretComponent()
        {
        }
    }
}
