// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Mfrc522Device.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.RfidTransceivers.Mfrc522
{
    using System;
    using System.Device.Gpio;
    using System.Threading;
    using Sundew.Base.Threading;
    using Sundew.Base.Threading.Jobs;

    /// <summary>
    /// A connection to a <see cref="Mfrc522"/>.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class Mfrc522Device : IRfidDevice
    {
        private readonly GpioController gpioController;
        private readonly IRfidDeviceReporter? rfidDeviceReporter;
        private readonly ICurrentThread thread;
        private readonly Mfrc522 mfrc522;
        private readonly ContinuousJob scanningJob;
        private readonly Activation activation;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mfrc522Device" /> class.
        /// </summary>
        /// <param name="gpioController">The gpio controller.</param>
        /// <param name="busId">The bus identifier.</param>
        /// <param name="chipLineSelect">The chip line select.</param>
        /// <param name="resetPin">The reset pin.</param>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        /// <param name="rfidDeviceReporter">The rfid connection reporter.</param>
        public Mfrc522Device(
            GpioController gpioController,
            int busId,
            int chipLineSelect,
            int? resetPin,
            bool activate,
            IRfidDeviceReporter? rfidDeviceReporter = null)
        {
            this.gpioController = gpioController;
            this.rfidDeviceReporter = rfidDeviceReporter;
            this.rfidDeviceReporter?.SetSource(typeof(IRfidDeviceReporter), this);
            this.thread = new CurrentThread();
            this.mfrc522 = new Mfrc522(this.thread, this.gpioController);
            this.scanningJob = new ContinuousJob(this.CheckForTags, (Exception e, ref bool _) => this.rfidDeviceReporter?.OnException(e));
            this.activation = new Activation(
                () => this.scanningJob.IsRunning,
                () =>
                {
                    this.mfrc522.Initialize(busId, chipLineSelect, resetPin, AntennaGain.Gain48);
                    this.scanningJob.Start();
                },
                () => this.scanningJob.Stop(),
                activate);
        }

        /// <summary>
        /// Occurs when a tag is detected.
        /// </summary>
        public event EventHandler<TagDetectedEventArgs>? TagDetected;

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

        void IDisposable.Dispose()
        {
            this.SetActivation(false);
            this.scanningJob.Dispose();
            this.mfrc522.Dispose();
        }

        private void CheckForTags(CancellationToken cancellationToken)
        {
            //// Console.WriteLine("is tag");
            var result = this.mfrc522.IsTagPresent();
            if (result)
            {
                //// Console.WriteLine("read");
                var uid = this.mfrc522.ReadUid();
                //// Console.WriteLine("halt");
                this.mfrc522.HaltTag();
                if (uid.IsValid)
                {
                    this.rfidDeviceReporter?.TagDetected(uid);
                    this.TagDetected?.Invoke(this, new TagDetectedEventArgs(uid));
                }
            }

            this.thread.Sleep(TimeSpan.FromMilliseconds(400), cancellationToken);
        }
    }
}