// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILircDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.InfraredReceivers.Lirc
{
    using System;

    /// <summary>
    /// Interface representing a lirc device.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ILircDevice : IDevice
    {
        /// <summary>
        /// Occurs when a lirc command received.
        /// </summary>
        event EventHandler<LircCommandEventArgs> CommandReceived;
    }
}