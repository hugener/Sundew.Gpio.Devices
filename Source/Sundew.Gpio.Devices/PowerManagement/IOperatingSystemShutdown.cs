// <copyright file="IOperatingSystemShutdown.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Sundew.Gpio.Devices.PowerManagement
{
    /// <summary>
    /// Interface for implementing a shutdown of an operating system.
    /// </summary>
    public interface IOperatingSystemShutdown
    {
        /// <summary>
        /// Shutdowns the operating system.
        /// </summary>
        void Shutdown();
    }
}