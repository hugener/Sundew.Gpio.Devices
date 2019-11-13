// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Max9744Device.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.Amplifiers.Max9744
{
    using System;
    using global::Pi.IO.GeneralPurpose;
    using global::Pi.IO.InterIntegratedCircuit;
    using Sundew.Base.Numeric;

    /// <inheritdoc />
    /// <summary>
    /// I2C connection to the MAX 9744 amplifier.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public class Max9744Device : IDisposable
    {
        private readonly IGpioConnectionDriver gpioConnectionDriver;
        private readonly I2cDeviceConnection connection;
        private readonly I2cDriver i2CDriver;
        private readonly OutputPinConfiguration shutdownPin;
        private readonly GpioConnection gpioConnection;
        private readonly OutputPinConfiguration mutePin;
        private readonly IGpioConnectionDriverFactory gpioConnectionDriverFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Max9744Device" /> class.
        /// </summary>
        /// <param name="i2cAddress">The i2c address.</param>
        /// <param name="mutePin">The mute pin.</param>
        /// <param name="shutdownPin">The shutdown pin.</param>
        /// <param name="sdaPin">The sda pin.</param>
        /// <param name="sclPin">The SCL pin.</param>
        public Max9744Device(
            byte i2cAddress,
            ConnectorPin mutePin,
            ConnectorPin shutdownPin,
            ProcessorPin sdaPin,
            ProcessorPin sclPin)
            : this(i2cAddress, mutePin, shutdownPin, sdaPin, sclPin, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Max9744Device" /> class.
        /// </summary>
        /// <param name="i2cAddress">The i2c address.</param>
        /// <param name="mutePin">The mute pin.</param>
        /// <param name="shutdownPin">The shutdown pin.</param>
        /// <param name="sdaPin">The sda pin.</param>
        /// <param name="sclPin">The SCL pin.</param>
        /// <param name="gpioConnectionDriverFactory">The gpio connection driver factory.</param>
        public Max9744Device(
            byte i2cAddress,
            ConnectorPin mutePin,
            ConnectorPin shutdownPin,
            ProcessorPin sdaPin,
            ProcessorPin sclPin,
            IGpioConnectionDriverFactory gpioConnectionDriverFactory)
        {
            this.gpioConnectionDriverFactory = GpioConnectionDriverFactory.EnsureGpioConnectionDriverFactory(gpioConnectionDriverFactory);
            this.gpioConnectionDriver = this.gpioConnectionDriverFactory.Get();

            // connect volume via i2c
            this.i2CDriver = new I2cDriver(sdaPin, sclPin);
            this.connection = this.i2CDriver.Connect(i2cAddress);

            // connect shutdown via gpio
            this.mutePin = mutePin.Output().Revert();
            this.shutdownPin = shutdownPin.Output().Revert();
            this.gpioConnection = new GpioConnection(this.gpioConnectionDriverFactory, this.shutdownPin, this.mutePin);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is muted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is muted; otherwise, <c>false</c>.
        /// </value>
        public bool IsMuted => this.gpioConnection[this.mutePin];

        /// <summary>
        /// Gets the volume range.
        /// </summary>
        /// <value>
        /// The volume range.
        /// </value>
        public Interval<byte> VolumeRange { get; } = new Interval<byte>(0, 63);

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The actual volume.</returns>
        public byte SetVolume(byte value)
        {
            var volume = this.VolumeRange.Limit(value);
            this.connection.WriteByte(volume);
            return volume;
        }

        /// <summary>
        /// Sets the state of the mute.
        /// </summary>
        /// <param name="mute">if set to <c>true</c> [mute].</param>
        /// <returns>The current mute state.</returns>
        public bool SetMuteState(bool mute)
        {
            this.gpioConnection[this.mutePin] = mute;
            return mute;
        }

        /// <summary>
        /// Toggles the mute.
        /// </summary>
        public void ToggleMute()
        {
            this.gpioConnection[this.mutePin] = !this.IsMuted;
        }

        /// <summary>
        /// Sets the state of the shutdown.
        /// </summary>
        /// <param name="isShutdown">if set to <c>true</c> [is shutdown].</param>
        public void SetShutdownState(bool isShutdown)
        {
            this.gpioConnection[this.shutdownPin] = isShutdown;
        }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <returns>The current volume.</returns>
        public byte GetVolume()
        {
            return this.connection.ReadByte();
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.SetShutdownState(true);
            this.i2CDriver.Dispose();
            this.gpioConnection.Dispose();
            this.gpioConnectionDriverFactory.Dispose(this.gpioConnectionDriver);
        }
    }
}