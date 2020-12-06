// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRotaryEncoderDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.RotaryEncoders
{
    using System;

    /// <summary>
    /// Interface for implementing a rotary encoder.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IRotaryEncoderDevice : IDisposable
    {
        /// <summary>
        /// Occurs when the encoder is rotated.
        /// </summary>
        event EventHandler<RotationEventArgs> Rotated;

        /// <summary>
        /// Starts handling rotation and pressed event.
        /// </summary>
        void Start();

        /// <summary>
        /// Starts handling rotation and pressed event.
        /// </summary>
        void Stop();
    }
}