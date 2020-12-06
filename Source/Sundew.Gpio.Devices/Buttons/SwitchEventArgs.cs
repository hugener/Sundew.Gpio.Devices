// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwitchEventArgs.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.Buttons
{
    /// <summary>
    /// Event arguments with the state of a switch.
    /// </summary>
    public class SwitchEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchEventArgs"/> class.
        /// </summary>
        /// <param name="state">if set to <c>true</c> [state].</param>
        public SwitchEventArgs(bool state)
        {
            this.State = state;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SwitchEventArgs"/> is state.
        /// </summary>
        /// <value>
        ///   <c>true</c> if state; otherwise, <c>false</c>.
        /// </value>
        public bool State { get; }
    }
}