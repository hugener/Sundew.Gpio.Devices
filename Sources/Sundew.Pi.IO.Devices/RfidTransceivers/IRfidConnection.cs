// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRfidConnection.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.RfidTransceivers
{
    using System;

    /// <summary>
    /// Interface for representing a connection to an rfid transceiver.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IRfidConnection : IDisposable
    {
        /// <summary>
        /// Occurs when a tag is detected.
        /// </summary>
        event EventHandler<TagDetectedEventArgs> TagDetected;

        /// <summary>
        /// Starts scanning for tags.
        /// </summary>
        void StartScanning();
    }
}