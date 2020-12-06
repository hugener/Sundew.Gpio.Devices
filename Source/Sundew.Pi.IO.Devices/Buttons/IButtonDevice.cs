// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IButtonDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.Buttons
{
    using System;

    /// <summary>
    /// Interface representing a button.
    /// </summary>
    /// <seealso cref="Sundew.Pi.IO.Devices.IGpioConfiguration" />
    public interface IButtonDevice : IGpioConfiguration
    {
        /// <summary>
        /// Occurs when the button is pressed.
        /// </summary>
        event EventHandler Pressed;
    }
}