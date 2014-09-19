﻿/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Text;
using System.Data.Linq.Mapping;
using BigMath;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Represents a Diffie-Hellman component created by someone else that can be used to create a Shared Secret with.
    /// </summary>
    [Table]
    public class ReceivedComponent : SecretComponent
    {
        /// <summary>
        /// Database identifier of this Received Component.
        /// </summary>
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public override long ID { get; protected set; }

        /// <summary>
        /// Serial number of this Received Component.
        /// </summary>
        [Column]
        public override long Serial { get; protected set; }

        /// <summary>
        /// ID of the Contact that you received this Component from.
        /// </summary>
        [Column]
        public override long ContactID { get; internal set; }

        /// <summary>
        /// The Public Value of this Component, known to both you and the Sender.
        /// </summary>
        [Column(DbType = "ntext")]
        public override BigInteger PublicValue { get; protected set; }

        /// <summary>
        /// Represents a Secret Component you received from a Contact to create a Shared Secret with.
        /// </summary>
        /// <param name="serial">The Serial Number of this Component.</param>
        /// <param name="publicValue">The Public Value contained within this Component.</param>
        internal ReceivedComponent(long serial, string publicValue)
            : this(serial, BigInteger.Parse(publicValue))
        {
        }

        /// <summary>
        /// Represents a Secret Component you received from a Contact to create a Shared Secret with.
        /// </summary>
        /// <param name="serial">The Serial Number of this Component.</param>
        /// <param name="publicValue">The Public Value contained within this Component.</param>
        internal ReceivedComponent(long serial, byte[] publicValue)
            : this(serial, new BigInteger(publicValue))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serial">The Serial Number of this Component.</param>
        /// <param name="publicValue">The Public Value contained within this Component.</param>
        internal ReceivedComponent(long serial, BigInteger publicValue)
            : base(serial)
        {
            PublicValue = publicValue;
        }

        internal ReceivedComponent()
            : base()
        {
        }
    }
}