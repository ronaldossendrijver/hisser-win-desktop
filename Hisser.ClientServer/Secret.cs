/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigMath;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents a Diffie-Hellman secret used as key to encrypt and decrypt messages.
    /// </summary>
    public class Secret
    {
        public static readonly byte[] p = new BigInteger("416064700201658306196320137931").ToByteArray();
        public static readonly byte[] g = new BigInteger(2).ToByteArray();

        /// <summary>
        /// The byte-representation of this value that can be used as a key.
        /// </summary>
        public byte[] Value
        {
            get
            {
                return ModPow(OtherComponent.PublicValue, MyComponent.PrivateValue, p);
            }
        }

        /// <summary>
        /// The component created by you to determine the secret shared with a Contact.
        /// </summary>
        public SentComponent MyComponent { get; private set; }

        /// <summary>
        /// The component created by your Contact to determine the secret shared with that Contact.
        /// </summary>
        public ReceivedComponent OtherComponent { get; private set; }
        
        /// <summary>
        /// Creates a Secret Value constructed from a Secret Component created by you and a Secret Component created by the Contact you will share this Secret with.
        /// </summary>
        /// <param name="myComponent">A Secret Component created by you.</param>
        /// <param name="otherComponent">A Secret Component created by the Contact you will share this Secret with.</param>
        public Secret(
            SentComponent myComponent,
            ReceivedComponent otherComponent)
        {
            MyComponent = myComponent;
            OtherComponent = otherComponent;
        }

        public static byte[] ModPow(byte[] g, byte[] e, byte[] m)
        {
            BigInteger ground = new BigInteger(g);
            BigInteger exponent = new BigInteger(e);
            BigInteger modulus = new BigInteger(m);
            return ground.ModPow(exponent, modulus).ToByteArray();
        }
    }
}
