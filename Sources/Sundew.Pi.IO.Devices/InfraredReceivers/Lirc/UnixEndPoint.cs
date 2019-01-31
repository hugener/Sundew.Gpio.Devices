// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnixEndPoint.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.InfraredReceivers.Lirc
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Sundew.Base.Equality;

    /// <summary>
    /// Represents a Unix end point.
    /// </summary>
    /// <seealso cref="EndPoint" />
    /// <seealso cref="IEquatable{UnixEndPoint}" />
    public class UnixEndPoint : EndPoint, IEquatable<UnixEndPoint>
    {
        private UnixEndPoint(string fileName)
        {
            this.Filename = fileName;
        }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public string Filename { get; }

        /// <summary>
        /// Gets the address family to which the endpoint belongs.
        /// </summary>
        public override AddressFamily AddressFamily => AddressFamily.Unix;

        /// <summary>
        /// Creates the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The <see cref="UnixEndPoint"/>.</returns>
        /// <exception cref="ArgumentNullException">filename.</exception>
        /// <exception cref="ArgumentException">Cannot be empty. - filename.</exception>
        public static UnixEndPoint Create(string filename)
        {
            if (filename == null)
            {
                throw new ArgumentNullException(nameof(filename));
            }

            if (filename == string.Empty)
            {
                throw new ArgumentException("Cannot be empty.", nameof(filename));
            }

            return new UnixEndPoint(filename);
        }

        /// <summary>
        /// Creates an <see cref="T:System.Net.EndPoint"></see> instance from a <see cref="T:System.Net.SocketAddress"></see> instance.
        /// </summary>
        /// <param name="socketAddress">The socket address that serves as the endpoint for a connection.</param>
        /// <returns>
        /// A new <see cref="T:System.Net.EndPoint"></see> instance that is initialized from the specified <see cref="T:System.Net.SocketAddress"></see> instance.
        /// </returns>
        public override EndPoint Create(SocketAddress socketAddress)
        {
            /*
             * Should also check this
             *
            int addr = (int) AddressFamily.Unix;
            if (socketAddress [0] != (addr & 0xFF))
                throw new ArgumentException ("socketAddress is not a unix socket address.");
            if (socketAddress [1] != ((addr & 0xFF00) >> 8))
                throw new ArgumentException ("socketAddress is not a unix socket address.");
             */

            if (socketAddress.Size == 2)
            {
                // Empty filename.
                // Probably from RemoteEndPoint which on linux does not return the file name.
                return new UnixEndPoint(string.Empty);
            }

            int size = socketAddress.Size - 2;
            byte[] bytes = new byte[size];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = socketAddress[i + 2];

                // There may be junk after the null terminator, so ignore it all.
                if (bytes[i] == 0)
                {
                    size = i;
                    break;
                }
            }

            return new UnixEndPoint(Encoding.UTF8.GetString(bytes, 0, size));
        }

        /// <summary>
        /// Serializes endpoint information into a <see cref="T:System.Net.SocketAddress"></see> instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Net.SocketAddress"></see> instance that contains the endpoint information.
        /// </returns>
        public override SocketAddress Serialize()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(this.Filename);
            var socketAddress = new SocketAddress(this.AddressFamily, 2 + bytes.Length + 1);

            // sa [0] -> family low byte, sa [1] -> family high byte
            for (int i = 0; i < bytes.Length; i++)
            {
                socketAddress[2 + i] = bytes[i];
            }

            // NULL suffix for non-abstract path
            socketAddress[2 + bytes.Length] = 0;
            return socketAddress;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Filename ?? "<None>";
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Filename.GetHashCodeOrDefault();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(UnixEndPoint other)
        {
            return EqualityHelper.Equals(this, other, rhs => this.Filename == rhs.Filename);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object other)
        {
            return EqualityHelper.Equals(this, other);
        }
    }
}
