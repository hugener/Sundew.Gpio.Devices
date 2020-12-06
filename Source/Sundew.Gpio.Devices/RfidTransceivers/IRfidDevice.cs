// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRfidDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.RfidTransceivers
{
    using System;

    /// <summary>
    /// Interface for representing a rfid transceiver device.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IRfidDevice : IDevice
    {
        /// <summary>
        /// Occurs when a tag is detected.
        /// </summary>
        event EventHandler<TagDetectedEventArgs> TagDetected;
    }
}