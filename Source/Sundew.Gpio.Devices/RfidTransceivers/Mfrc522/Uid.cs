// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Uid.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.RfidTransceivers.Mfrc522
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Sundew.Base.Equality;

    /// <summary>
    /// Represents the unique id of a NFC tag.
    /// </summary>
    public class Uid : ITag
    {
        private const string SingleByteHexFormat = "{0:x2}";

        /// <summary>
        /// Initializes a new instance of the <see cref="Uid" /> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        /// <param name="fourth">The fourth.</param>
        public Uid(byte first, byte second, byte third, byte fourth)
            : this(new[] { first, second, third, fourth, GetBcc(first, second, third, fourth) }, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Uid" /> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        /// <param name="fourth">The fourth.</param>
        /// <param name="fifth">The fifth.</param>
        /// <param name="sixth">The sixth.</param>
        /// <param name="seventh">The seventh.</param>
        public Uid(byte first, byte second, byte third, byte fourth, byte fifth, byte sixth, byte seventh)
            : this(new[] { first, second, third, fourth, fifth, sixth, seventh, GetBcc(first, second, third, fourth, fifth, sixth, seventh) }, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Uid" /> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        /// <param name="fourth">The fourth.</param>
        /// <param name="fifth">The fifth.</param>
        /// <param name="sixth">The sixth.</param>
        /// <param name="seventh">if set to <c>true</c> [seventh].</param>
        /// <param name="eighth">The eighth.</param>
        /// <param name="ninth">The ninth.</param>
        /// <param name="tenth">The tenth.</param>
        public Uid(byte first, byte second, byte third, byte fourth, byte fifth, byte sixth, byte seventh, byte eighth, byte ninth, byte tenth)
            : this(new[] { first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth, GetBcc(first, second, third, fourth, fifth, sixth, seventh, eighth, ninth, tenth) }, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Uid"/> class.
        /// </summary>
        /// <param name="uid">The uid.</param>
        public Uid(byte[] uid)
            : this(uid, false)
        {
        }

        private Uid(byte[] uid, bool isVerified)
        {
            if (uid.Length == 0)
            {
                this.RawData = new byte[] { 0, 0, 0, 0, 0 };
                this.IsValid = false;
            }
            else
            {
                this.RawData = uid;
                this.IsValid = isVerified || this.RawData.Aggregate(0, (accumulate, current) => accumulate ^ current) == 0;
            }

            this.Bcc = this.RawData[this.RawData.Length - 1];
            this.Bytes = this.RawData.AsMemory().Slice(0, this.Size);
        }

        /// <summary>
        /// Gets the size of the uid.
        /// </summary>
        /// <value>
        /// The size of the uid.
        /// </value>
        public int Size => this.RawData.Length - 1;

        /// <summary>
        /// Gets the BCC.
        /// </summary>
        /// <value>
        /// The BCC.
        /// </value>
        public byte Bcc { get; }

        /// <summary>
        /// Gets the bytes.
        /// </summary>
        /// <value>
        /// The bytes.
        /// </value>
        public Memory<byte> Bytes { get; }

        /// <summary>
        /// Gets the full uid.
        /// </summary>
        /// <value>
        /// The full uid.
        /// </value>
        public byte[] RawData { get; }

        /// <summary>
        /// Gets a value indicating whether the Uid is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid { get; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(ITag other)
        {
            return EqualityHelper.Equals(this, other, rhs => this.RawData.SequenceEqual(rhs.RawData));
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public sealed override bool Equals(object obj)
        {
            return EqualityHelper.Equals<ITag>(this, obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public sealed override int GetHashCode()
        {
            return this.Bytes.Span.GetItemsHashCode();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public sealed override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var @byte in this.Bytes.Span)
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, SingleByteHexFormat, @byte);
            }

            return stringBuilder.ToString();
        }

        private static byte GetBcc(params byte[] uid)
        {
            return (byte)uid.Aggregate(0, (accumulate, current) => accumulate ^ current);
        }
    }
}
