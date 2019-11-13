// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemotePiDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.PowerManagement.RemotePi
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Pi.Core.Threading;
    using global::Pi.IO.GeneralPurpose;
    using Sundew.Base.Threading;
    using Sundew.Base.Time;

    /// <summary>
    /// Represents a connection to the RemotePI for shutting down PI.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class RemotePiDevice : IShutdownDevice
    {
        internal static readonly TimeSpan PowerOffTimeSpan = TimeSpan.FromMinutes(4);
        private readonly IGpioConnectionDriverFactory gpioConnectionDriverFactory;
        private readonly IGpioConnectionDriver gpioConnectionDriver;
        private readonly ConnectorPin shutdownInConnectorPin;
        private readonly ConnectorPin shutdownOutConnectorPin;
        private readonly IOperatingSystemShutdown operatingSystemShutdown;
        private readonly TimeSpan shutdownTimeSpan;
        private readonly IDateTime dateTime;
        private readonly ICurrentThread thread;
        private readonly GpioConnection gpioConnection;
        private Task shutdownTask;
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemotePiDevice" /> class.
        /// </summary>
        /// <param name="shutdownInConnectorPin">The shutdown in connector pin.</param>
        /// <param name="shutdownOutConnectorPin">The shutdown out connector pin.</param>
        /// <param name="operatingSystemShutdown">The operation system shutdown.</param>
        /// <param name="shutdownTimeSpan">The shutdown time span.</param>
        public RemotePiDevice(
            ConnectorPin shutdownInConnectorPin,
            ConnectorPin shutdownOutConnectorPin,
            IOperatingSystemShutdown operatingSystemShutdown,
            TimeSpan shutdownTimeSpan = default)
            : this(
                  shutdownInConnectorPin,
                  shutdownOutConnectorPin,
                  operatingSystemShutdown,
                  shutdownTimeSpan,
                  null,
                  null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemotePiDevice" /> class.
        /// </summary>
        /// <param name="shutdownInConnectorPin">The shutdown in connector pin.</param>
        /// <param name="shutdownOutConnectorPin">The shutdown out connector pin.</param>
        /// <param name="operatingSystemShutdown">The operation system shutdown.</param>
        /// <param name="shutdownTimeSpan">The shutdown time span.</param>
        /// <param name="gpioConnectionDriverFactory">The gpio connection driver factory.</param>
        public RemotePiDevice(
            ConnectorPin shutdownInConnectorPin,
            ConnectorPin shutdownOutConnectorPin,
            IOperatingSystemShutdown operatingSystemShutdown,
            TimeSpan shutdownTimeSpan = default,
            IGpioConnectionDriverFactory gpioConnectionDriverFactory = null)
            : this(
                shutdownInConnectorPin,
                shutdownOutConnectorPin,
                operatingSystemShutdown,
                shutdownTimeSpan,
                gpioConnectionDriverFactory,
                null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemotePiDevice" /> class.
        /// </summary>
        /// <param name="shutdownInConnectorPin">The shutdown in connector pin.</param>
        /// <param name="shutdownOutConnectorPin">The shutdown out connector pin.</param>
        /// <param name="operatingSystemShutdown">The operation system shutdown.</param>
        /// <param name="shutdownTimeSpan">The shutdown time span.</param>
        /// <param name="threadFactory">The thread factory.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="gpioConnectionDriverFactory">The gpio connection driver factory.</param>
        public RemotePiDevice(
                    ConnectorPin shutdownInConnectorPin,
                    ConnectorPin shutdownOutConnectorPin,
                    IOperatingSystemShutdown operatingSystemShutdown,
                    TimeSpan shutdownTimeSpan,
                    IGpioConnectionDriverFactory gpioConnectionDriverFactory = null,
                    IDateTime dateTime = null,
                    IThreadFactory threadFactory = null)
        {
            this.gpioConnectionDriverFactory = GpioConnectionDriverFactory.EnsureGpioConnectionDriverFactory(gpioConnectionDriverFactory);
            this.gpioConnectionDriver = this.gpioConnectionDriverFactory.Get();
            this.shutdownInConnectorPin = shutdownInConnectorPin;
            this.shutdownOutConnectorPin = shutdownOutConnectorPin;
            this.operatingSystemShutdown = operatingSystemShutdown;
            this.shutdownTimeSpan = shutdownTimeSpan < TimeSpan.FromSeconds(4) ? TimeSpan.FromSeconds(4) : shutdownTimeSpan;
            this.dateTime = dateTime ?? new DateTimeProvider();
            this.thread = ThreadFactory.EnsureThreadFactory(threadFactory).Create();
            var pinConfiguration = shutdownInConnectorPin.Input().PullDown();
            pinConfiguration.OnStatusChanged(this.OnShutdown);
            this.gpioConnection = new GpioConnection(new GpioConnectionDriverFactory(this.gpioConnectionDriver), pinConfiguration);
        }

        /// <summary>
        /// Occurs when remote pi requests system shutdown.
        /// </summary>
        public event EventHandler<ShutdownEventArgs> ShuttingDown;

        /// <inheritdoc />
        public void Dispose()
        {
            this.shutdownTask?.Wait(this.shutdownTimeSpan + TimeSpan.FromSeconds(2));
            this.gpioConnection.Dispose();
            this.gpioConnectionDriverFactory.Dispose(this.gpioConnectionDriver);
        }

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        public void Shutdown()
        {
            var shutdownOutputPin = this.gpioConnectionDriver.Out(this.shutdownOutConnectorPin);
            shutdownOutputPin.Write(true);
            this.thread.Sleep(TimeSpan.FromMilliseconds(125));
            shutdownOutputPin.Write(false);
            this.thread.Sleep(TimeSpan.FromMilliseconds(200));
            shutdownOutputPin.Write(true);
            this.thread.Sleep(TimeSpan.FromMilliseconds(400));
            shutdownOutputPin.Write(false);
        }

        private void OnShutdown(bool state)
        {
            if (state)
            {
                this.cancellationTokenSource?.Cancel();
                this.cancellationTokenSource = new CancellationTokenSource();
                var shutdownEventArgs = new ShutdownEventArgs(this.cancellationTokenSource, this.dateTime.LocalTime, this.shutdownTimeSpan, PowerOffTimeSpan);
                this.shutdownTask = Task.Run(() => this.ShutdownAsync(this.cancellationTokenSource.Token), this.cancellationTokenSource.Token)
                    .ContinueWith((task, _) => this.cancellationTokenSource.Dispose(), null);
                this.ShuttingDown?.Invoke(this, shutdownEventArgs);
            }
        }

        private async Task ShutdownAsync(CancellationToken token)
        {
            try
            {
                this.gpioConnectionDriver.Out(this.shutdownInConnectorPin).Write(true);
                await Task.Delay(this.shutdownTimeSpan, token);
                this.operatingSystemShutdown.Shutdown();
            }
            catch (OperationCanceledException)
            {
                this.gpioConnectionDriver.Out(this.shutdownInConnectorPin).Write(false);
            }
        }
    }
}