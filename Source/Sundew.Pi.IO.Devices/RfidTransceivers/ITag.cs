// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITag.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.RfidTransceivers
{
    using System;

    /// <summary>
    /// Represents a unique id.
    /// </summary>
    /// <seealso cref="ITag" />
    public interface ITag : IEquatable<ITag>
    {
        /// <summary>
        /// Gets the full uid.
        /// </summary>
        /// <value>
        /// The full uid.
        /// </value>
        byte[] RawData { get; }

        /// <summary>
        /// Gets a value indicating whether the Uid is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        bool IsValid { get; }
    }
}