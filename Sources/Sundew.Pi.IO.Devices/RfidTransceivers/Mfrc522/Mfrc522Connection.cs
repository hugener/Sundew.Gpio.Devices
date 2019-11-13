// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Mfrc522Connection.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.RfidTransceivers.Mfrc522
{
    using System;
    using System.Threading;
    using global::Pi.Core.Threading;
    using global::Pi.IO.GeneralPurpose;
    using Sundew.Base.Threading;
    using Sundew.Base.Threading.Jobs;

    /// <summary>
    /// A connection to a <see cref="Mfrc522Device"/>.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class Mfrc522Connection : IRfidConnection
    {
        private readonly string spiDevicePath;
        private readonly ConnectorPin? resetConnectorPin;
        private readonly IGpioConnectionDriverFactory gpioConnectionDriverFactory;
        private readonly IRfidConnectionReporter rfidConnectionReporter;
        private readonly ICurrentThread thread;
        private readonly Mfrc522Device mfrc522Device;
        private readonly IGpioConnectionDriver gpioConnectionDriver;
        private readonly ContinuousJob scanningJob;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mfrc522Connection" /> class.
        /// </summary>
        /// <param name="spiDevicePath">The spi device path.</param>
        /// <param name="resetConnectorPin">The reset connector pin.</param>
        /// <param name="gpioConnectionDriverFactory">The gpio connection driver factory.</param>
        /// <param name="threadFactory">The thread factory.</param>
        /// <param name="rfidConnectionReporter">The rfid connection reporter.</param>
        public Mfrc522Connection(
            string spiDevicePath,
            ConnectorPin? resetConnectorPin,
            IGpioConnectionDriverFactory gpioConnectionDriverFactory = null,
            IThreadFactory threadFactory = null,
            IRfidConnectionReporter rfidConnectionReporter = null)
        {
            this.spiDevicePath = spiDevicePath;
            this.resetConnectorPin = resetConnectorPin;
            this.gpioConnectionDriverFactory = gpioConnectionDriverFactory;
            this.rfidConnectionReporter = rfidConnectionReporter;
            this.rfidConnectionReporter?.SetSource(this);
            this.thread = ThreadFactory.EnsureThreadFactory(threadFactory).Create();
            this.gpioConnectionDriverFactory =
                GpioConnectionDriverFactory.EnsureGpioConnectionDriverFactory(gpioConnectionDriverFactory);
            this.gpioConnectionDriver = this.gpioConnectionDriverFactory.Get();
            this.mfrc522Device = new Mfrc522Device(this.thread, this.gpioConnectionDriver);
            this.scanningJob = new ContinuousJob(this.CheckForTags, e => this.rfidConnectionReporter?.OnException(e));
        }

        /// <summary>
        /// Occurs when a tag is detected.
        /// </summary>
        public event EventHandler<TagDetectedEventArgs> TagDetected;

        /// <summary>
        /// Starts scanning for tags.
        /// </summary>
        public void StartScanning()
        {
            if (this.scanningJob.IsRunning)
            {
                return;
            }

            this.mfrc522Device.Initialize(this.spiDevicePath, this.resetConnectorPin, AntennaGain.Gain48);
            this.scanningJob.Start();
        }

        /// <summary>
        /// Stops scanning.
        /// </summary>
        public void StopScanning()
        {
            this.scanningJob.Stop();
        }

        void IDisposable.Dispose()
        {
            this.scanningJob.Dispose();
            this.mfrc522Device.Dispose();
            this.gpioConnectionDriverFactory.Dispose();
        }

        private void CheckForTags(CancellationToken cancellationToken)
        {
            //// Console.WriteLine("is tag");
            var result = this.mfrc522Device.IsTagPresent();
            if (result)
            {
                //// Console.WriteLine("read");
                var uid = this.mfrc522Device.ReadUid();
                //// Console.WriteLine("halt");
                this.mfrc522Device.HaltTag();
                if (uid.IsValid)
                {
                    this.rfidConnectionReporter?.TagDetected(uid);
                    this.TagDetected?.Invoke(this, new TagDetectedEventArgs(uid));
                }
            }

            this.thread.Sleep(TimeSpan.FromMilliseconds(400), cancellationToken);
        }
    }
}