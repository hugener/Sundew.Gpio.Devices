// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RotationEventArgs.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.RotaryEncoders
{
    using System;

    /// <summary>
    /// Event arguments for when a encoder rotation occurs.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class RotationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RotationEventArgs"/> class.
        /// </summary>
        /// <param name="encoderDirection">The encoder direction.</param>
        public RotationEventArgs(EncoderDirection encoderDirection)
        {
            this.EncoderDirection = encoderDirection;
        }

        /// <summary>
        /// Gets the encoder direction.
        /// </summary>
        /// <value>
        /// The encoder direction.
        /// </value>
        public EncoderDirection EncoderDirection { get; }
    }
}
