// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices
{
    using System;

    /// <summary>
    /// Interface for implementing a device.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IDevice : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is activated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is activated; otherwise, <c>false</c>.
        /// </value>
        bool IsActivated { get; }

        /// <summary>
        /// Sets the activation.
        /// </summary>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        /// <returns>A value indicating whether the operation was successful.</returns>
        bool SetActivation(bool activate);
    }
}