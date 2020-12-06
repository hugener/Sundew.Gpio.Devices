// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShutdownDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.PowerManagement
{
    using System;

    /// <summary>
    /// Interface representing a shutdown device.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IShutdownDevice : IDisposable
    {
        /// <summary>
        /// Occurs when remote pi requests system shutdown.
        /// </summary>
        event EventHandler<ShutdownEventArgs> ShuttingDown;

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        void Shutdown();
    }
}