// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRotaryEncoderWithButtonDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.RotaryEncoders
{
    using System;

    /// <summary>
    /// Interface for implementing a rotary encoder with a button.
    /// </summary>
    /// <seealso cref="IRotaryEncoderDevice" />
    public interface IRotaryEncoderWithButtonDevice : IRotaryEncoderDevice
    {
        /// <summary>
        /// Occurs when the encoder is pressed.
        /// </summary>
        event EventHandler Pressed;
    }
}