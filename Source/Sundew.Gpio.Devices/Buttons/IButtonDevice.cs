// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IButtonDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.Buttons
{
    using System;

    /// <summary>
    /// Interface representing a button.
    /// </summary>
    public interface IButtonDevice : IDevice
    {
        /// <summary>
        /// Occurs when the button is pressed.
        /// </summary>
        event EventHandler Pressed;

        /// <summary>
        /// Gets the pin.
        /// </summary>
        /// <value>
        /// The pin.
        /// </value>
        public int Pin { get; }
    }
}