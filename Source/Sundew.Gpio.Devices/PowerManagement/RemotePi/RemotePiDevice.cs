// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemotePiDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.PowerManagement.RemotePi
{
    using System;
    using System.Device.Gpio;
    using System.Threading;
    using System.Threading.Tasks;
    using Sundew.Base.Threading;
    using Sundew.Base.Time;
    using Sundew.Gpio.Devices.Buttons;

    /// <summary>
    /// Represents a connection to the RemotePI for shutting down PI.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class RemotePiDevice : IShutdownDevice
    {
        internal static readonly TimeSpan PowerOffTimeSpan = TimeSpan.FromMinutes(4);
        internal static readonly TimeSpan MaxShutdownTimeSpan = TimeSpan.FromSeconds(4);
        private readonly GpioController gpioController;
        private readonly IButtonDevice shutdownButtonDevice;
        private readonly int shutdownOutPin;
        private readonly IOperatingSystemShutdown operatingSystemShutdown;
        private readonly TimeSpan shutdownTimeSpan;
        private readonly IDateTime dateTime;
        private readonly ICurrentThread thread;
        private readonly Activation activation;
        private Task? shutdownTask;
        private CancellationTokenSource? cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemotePiDevice" /> class.
        /// </summary>
        /// <param name="gpioController">The gpio controller.</param>
        /// <param name="shutdownButtonDevice">The shutdown button device.</param>
        /// <param name="shutdownOutPin">The shutdown out pin.</param>
        /// <param name="operatingSystemShutdown">The operation system shutdown.</param>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        /// <param name="shutdownTimeSpan">The shutdown time span.</param>
        public RemotePiDevice(
            GpioController gpioController,
            IButtonDevice shutdownButtonDevice,
            int shutdownOutPin,
            IOperatingSystemShutdown operatingSystemShutdown,
            bool activate,
            TimeSpan shutdownTimeSpan = default)
            : this(
                gpioController,
                shutdownButtonDevice,
                shutdownOutPin,
                operatingSystemShutdown,
                activate,
                shutdownTimeSpan,
                null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemotePiDevice" /> class.
        /// </summary>
        /// <param name="gpioController">The gpio controller.</param>
        /// <param name="shutdownButtonDevice">The shutdown button device.</param>
        /// <param name="shutdownOutPin">The shutdown out pin.</param>
        /// <param name="operatingSystemShutdown">The operation system shutdown.</param>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        /// <param name="shutdownTimeSpan">The shutdown time span.</param>
        /// <param name="dateTime">The date time.</param>
        public RemotePiDevice(
            GpioController gpioController,
            IButtonDevice shutdownButtonDevice,
            int shutdownOutPin,
            IOperatingSystemShutdown operatingSystemShutdown,
            bool activate,
            TimeSpan shutdownTimeSpan,
            IDateTime? dateTime = null)
        {
            this.gpioController = gpioController;
            this.shutdownButtonDevice = shutdownButtonDevice;
            this.shutdownOutPin = shutdownOutPin;
            this.operatingSystemShutdown = operatingSystemShutdown;
            this.shutdownTimeSpan = shutdownTimeSpan > MaxShutdownTimeSpan ? MaxShutdownTimeSpan : shutdownTimeSpan;
            this.dateTime = dateTime ?? new DateTimeProvider();
            this.thread = new CurrentThread();
            this.shutdownButtonDevice.Pressed += this.OnShutdown;
            this.activation = new Activation(
                () => this.shutdownButtonDevice.IsActivated,
                () =>
                {
                    this.shutdownButtonDevice.SetActivation(true);
                    this.gpioController.OpenPin(this.shutdownOutPin, PinMode.Output);
                },
                () =>
                {
                    this.shutdownButtonDevice.SetActivation(false);
                    this.gpioController.ClosePin(this.shutdownOutPin);
                },
                activate);
        }

        /// <summary>
        /// Occurs when remote pi requests system shutdown.
        /// </summary>
        public event EventHandler<ShutdownEventArgs>? ShuttingDown;

        /// <summary>
        /// Gets a value indicating whether this instance is activated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is activated; otherwise, <c>false</c>.
        /// </value>
        public bool IsActivated => this.activation.IsActivated;

        /// <summary>
        /// Sets the activation.
        /// </summary>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        /// <returns>
        /// A value indicating whether the operation was successful.
        /// </returns>
        public bool SetActivation(bool activate)
        {
            return this.activation.SetActivation(activate);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.shutdownTask?.Wait(this.shutdownTimeSpan + TimeSpan.FromSeconds(2));
            this.activation.SetActivation(false);
        }

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        public void Shutdown()
        {
            this.gpioController.Write(this.shutdownOutPin, true);
            this.thread.Sleep(TimeSpan.FromMilliseconds(125));
            this.gpioController.Write(this.shutdownOutPin, false);
            this.thread.Sleep(TimeSpan.FromMilliseconds(200));
            this.gpioController.Write(this.shutdownOutPin, true);
            this.thread.Sleep(TimeSpan.FromMilliseconds(400));
            this.gpioController.Write(this.shutdownOutPin, false);
        }

        private void OnShutdown(object sender, EventArgs eventArgs)
        {
            this.cancellationTokenSource?.Cancel();
            this.cancellationTokenSource = new CancellationTokenSource();
            var shutdownEventArgs = new ShutdownEventArgs(this.cancellationTokenSource, this.dateTime.LocalTime, this.shutdownTimeSpan, PowerOffTimeSpan);
            this.shutdownTask = Task.Run(() => this.ShutdownAsync(this.cancellationTokenSource.Token), this.cancellationTokenSource.Token)
                .ContinueWith((_, taskCancellationTokenSource) => ((CancellationTokenSource)taskCancellationTokenSource).Dispose(), this.cancellationTokenSource);
            this.ShuttingDown?.Invoke(this, shutdownEventArgs);
        }

        private async Task ShutdownAsync(CancellationToken token)
        {
            try
            {
                this.shutdownButtonDevice.SetActivation(false);
                this.gpioController.OpenPin(this.shutdownButtonDevice.Pin, PinMode.Output);
                this.gpioController.Write(this.shutdownButtonDevice.Pin, true);
                await Task.Delay(this.shutdownTimeSpan, token).ConfigureAwait(false);
                this.operatingSystemShutdown.Shutdown();
            }
            catch (OperationCanceledException)
            {
                this.gpioController.Write(this.shutdownButtonDevice.Pin, false);
                this.gpioController.ClosePin(this.shutdownButtonDevice.Pin);
                this.shutdownButtonDevice.SetActivation(true);
            }
        }
    }
}