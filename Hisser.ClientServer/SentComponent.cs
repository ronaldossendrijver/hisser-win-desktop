/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;
using System.Data.Linq.Mapping;
using BigMath;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents a Diffie-Hellman component created by you that can be used to create a Shared Secret with.
    /// </summary>
    [Table]
    public class SentComponent : SecretComponent
    {
        private const int PRIVATE_VALUE_LENGTH = 620;

        /// <summary>
        /// Database identifier of this Received Component.
        /// </summary>
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public override long ID { get; protected set; }

        /// <summary>
        /// Serial number of this Sent Component.
        /// </summary>
        [Column]
        public override long Serial { get; protected set; }

        /// <summary>
        /// ID of the Contact that you sent this Component to.
        /// </summary>
        [Column]
        public override long ContactID { get; internal set; }

        /// <summary>
        /// The Public Value of this Component, known to both you and the Receiver.
        /// </summary>
        [Column(DbType = "image")]
        public override byte[] PublicValue { get; protected set; }
        
        /// <summary>
        /// The Private Value of this Component, known to you only.
        /// </summary>
        [Column(DbType = "image")]
        public byte[] PrivateValue { get; private set; }

        /// <summary>
        /// Creates a Secret Component that can be sent to a Contact to create a Shared Secret with.
        /// </summary>
        /// <param name="serial">The Serial Number of this Component.</param>
        internal SentComponent(long serial)
            : base(serial)
        {
            PrivateValue = new BigInteger(RandomGenerator.GetString(PRIVATE_VALUE_LENGTH, "0123456789")).ToByteArray();
            PublicValue = Secret.ModPow(Secret.g, PrivateValue, Secret.p);
        }

        public SentComponent()
            : base()
        {
        }
    }
}
