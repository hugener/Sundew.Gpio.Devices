// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILircDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.InfraredReceivers.Lirc
{
    using System;

    /// <summary>
    /// Interface representing a lirc device.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ILircDevice : IDisposable
    {
        /// <summary>
        /// Occurs when a lirc command received.
        /// </summary>
        event EventHandler<LircCommandEventArgs> CommandReceived;

        /// <summary>
        /// Starts listening for commands.
        /// </summary>
        void StartListening();

        /// <summary>
        /// Stops listening for commands.
        /// </summary>
        void Stop();
    }
}