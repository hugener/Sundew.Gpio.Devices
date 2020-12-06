// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TagDetectedEventArgs.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.RfidTransceivers
{
    using System;

    /// <summary>
    /// Event arguments for when a tag is detected.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class TagDetectedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagDetectedEventArgs"/> class.
        /// </summary>
        /// <param name="tag">The uid.</param>
        public TagDetectedEventArgs(ITag tag)
        {
            this.Tag = tag;
        }

        /// <summary>
        /// Gets the uid.
        /// </summary>
        /// <value>
        /// The uid.
        /// </value>
        public ITag Tag { get; }
    }
}