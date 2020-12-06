// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRfidDeviceReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.RfidTransceivers
{
    using System;
    using Sundew.Base.Reporting;
    using Sundew.Gpio.Devices.RfidTransceivers.Mfrc522;

    /// <summary>
    /// Interface for implementing a rfid connection reporter.
    /// </summary>
    /// <seealso cref="Sundew.Base.Reporting.IReporter" />
    public interface IRfidDeviceReporter : IReporter
    {
        /// <summary>
        /// Tags the detected.
        /// </summary>
        /// <param name="uid">The uid.</param>
        void TagDetected(Uid uid);

        /// <summary>
        /// Called when [exception].
        /// </summary>
        /// <param name="exception">The exception.</param>
        void OnException(Exception exception);
    }
}