// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderDirection.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Sundew.Gpio.Devices.RotaryEncoders
{
    /// <summary>
    /// Specifies the rotation direction of and encoder.
    /// </summary>
    public enum EncoderDirection
    {
        /// <summary>
        /// The clockwise rotation.
        /// </summary>
        Clockwise,

        /// <summary>
        /// The counter clockwise rotation.
        /// </summary>
        CounterClockwise,
    }
}