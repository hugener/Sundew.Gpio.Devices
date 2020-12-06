// <copyright file="ShutdownEventArgs.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Sundew.Gpio.Devices.PowerManagement
{
    using System;
    using System.Threading;

    /// <summary>
    /// Event args for the shutting down event.
    /// </summary>
    public sealed class ShutdownEventArgs : EventArgs
    {
        private readonly CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShutdownEventArgs" /> class.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token.</param>
        /// <param name="shutdownStartTime">The shutdown start time.</param>
        /// <param name="shutdownTimeSpan">The shutdown time span.</param>
        /// <param name="powerOffTimeSpan">The power off time span.</param>
        public ShutdownEventArgs(
            CancellationTokenSource cancellationTokenSource,
            DateTime shutdownStartTime,
            TimeSpan shutdownTimeSpan,
            TimeSpan powerOffTimeSpan)
        {
            this.cancellationTokenSource = cancellationTokenSource;
            this.ShutdownStartTime = shutdownStartTime;
            this.ShutdownTime = this.ShutdownStartTime + shutdownTimeSpan;
            this.PowerOffTime = this.ShutdownStartTime + powerOffTimeSpan;
        }

        /// <summary>
        /// Gets the shutdown start time.
        /// </summary>
        /// <value>
        /// The shutdown start time.
        /// </value>
        public DateTime ShutdownStartTime { get; }

        /// <summary>
        /// Gets the time until shutdown.
        /// </summary>
        /// <value>
        /// The time until shutdown.
        /// </value>
        public DateTime ShutdownTime { get; }

        /// <summary>
        /// Gets the time until power off.
        /// </summary>
        /// <value>
        /// The time until power off.
        /// </value>
        public DateTime PowerOffTime { get; }

        /// <summary>
        /// Cancels the shutdown.
        /// </summary>
        public void CancelShutdown()
        {
            this.cancellationTokenSource.Cancel();
        }
    }
}