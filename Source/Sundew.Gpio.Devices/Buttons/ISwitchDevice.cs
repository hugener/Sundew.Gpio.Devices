// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISwitchDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.Buttons
{
    using System;

    /// <summary>
    /// Interface representing a switch.
    /// </summary>
    /// <seealso cref="IDevice" />
    public interface ISwitchDevice : IDevice
    {
        /// <summary>
        /// Occurs when the button is pressed.
        /// </summary>
        event EventHandler<SwitchEventArgs> StateChanged;

        /// <summary>
        /// Gets a value indicating whether this <see cref="SwitchDevice"/> is state.
        /// </summary>
        /// <value>
        ///   <c>true</c> if state; otherwise, <c>false</c>.
        /// </value>
        bool State { get; }
    }
}