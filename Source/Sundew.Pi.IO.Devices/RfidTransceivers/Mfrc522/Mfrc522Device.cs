// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Mfrc522Device.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.RfidTransceivers.Mfrc522
{
    using System;
    using System.Threading.Tasks;
    using global::Pi.IO.GeneralPurpose;
    using global::Pi.IO.SerialPeripheralInterface;
    using Sundew.Base.Computation;
    using Sundew.Base.Threading;

    /// <summary>
    /// Represents a Mfrc522 device.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class Mfrc522Device : IDisposable
    {
        private readonly ICurrentThread thread;
        private readonly IGpioConnectionDriver gpioConnectionDriver;
        private NativeSpiConnection? nativeSpiConnection;
        private GpioOutputBinaryPin? resetPowerDown;

        internal Mfrc522Device(ICurrentThread thread, IGpioConnectionDriver gpioConnectionDriver)
        {
            this.thread = thread;
            this.gpioConnectionDriver = gpioConnectionDriver;
        }

        /// <summary>
        /// Initializes the device with the specified spi path.
        /// </summary>
        /// <param name="spiPath">The device file path.</param>
        /// <param name="resetConnectorPin">The reset connector pin.</param>
        /// <param name="antennaGain">The antenna gain.</param>
        /// <exception cref="Exception">GPIO initialization failed.</exception>
        /// <exception cref="Exception"> SPI initialization failed.</exception>
        public void Initialize(string spiPath, ConnectorPin? resetConnectorPin, AntennaGain? antennaGain = null)
        {
            try
            {
                if (resetConnectorPin.HasValue)
                {
                    this.resetPowerDown = this.gpioConnectionDriver.Out(resetConnectorPin.Value);
                    this.resetPowerDown.Write(true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GPIO initialization failed", ex);
            }

            try
            {
                var settings = new SpiConnectionSettings
                {
                    MaxSpeed = 1000000,
                    Mode = SpiMode.Mode0,
                };

                this.nativeSpiConnection = new NativeSpiConnection(spiPath, settings);
            } /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }

            if (antennaGain != null)
            {
                this.SetRegisterBits(Registers.RFCgReg, (byte)((int)antennaGain & 0x70));
            }

            this.Reset();
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            if (this.resetPowerDown != null)
            {
                this.resetPowerDown.Write(false);
                this.thread.Sleep(TimeSpan.FromMilliseconds(50));
                this.resetPowerDown.Write(true);
                this.thread.Sleep(TimeSpan.FromMilliseconds(50));
            }

            // Force 100% ASK modulation
            this.WriteRegister(Registers.TxAsk, 0x40);

            // Set CRC to 0x6363
            this.WriteRegister(Registers.Mode, 0x3D);

            // Enable antenna
            this.SetRegisterBits(Registers.TxControl, 0x03);
        }

        /// <summary>
        /// Determines whether a tag is present.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if tag is present; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTagPresent()
        {
            // Enable short frames
            this.WriteRegister(Registers.BitFraming, 0x07);
            //// Console.WriteLine("BitFraming7");

            // Transceive the Request command to the tag
            this.Transceive(false, PiccCommands.Request);
            //// Console.WriteLine("Transceive");

            // Disable short frames
            this.WriteRegister(Registers.BitFraming, 0x00);
            //// Console.WriteLine("BitFraming0");

            if (this.GetFifoLevel() == 2)
            {
                var (_, atqaLow) = this.ReadFromFifo2Bytes();
                var bitFrame = atqaLow & 0b0001_1111;
                //// Console.WriteLine(atqaLow);
                if (bitFrame == PiccResponses.AnswerToRequest)
                {
                    return true;
                }
            }

            //// Console.WriteLine("Fail");

            return false;
        }

        /// <summary>
        /// Halts the tag.
        /// </summary>
        public void HaltTag()
        {
            // Transceive the Halt command to the tag
            this.Transceive(false, PiccCommands.Halt1, PiccCommands.Halt2);
        }

        /// <summary>
        /// Selects the tag.
        /// </summary>
        /// <param name="uid">The uid.</param>
        /// <returns><c>true</c>, if the tag was selected, otherwise <c>false</c>.</returns>
        public bool SelectTag(Uid uid)
        {
            // Send Select command to tag
            var data = new byte[2 + uid.Size];
            data[0] = PiccCommands.Select1;
            data[1] = PiccCommands.Select2;
            uid.RawData.CopyTo(data, 2);

            this.Transceive(true, data);

            return this.GetFifoLevel() == 1 && this.ReadFromFifo() == PiccResponses.SelectAcknowledge;
        }

        /// <summary>
        /// Reads the uid.
        /// </summary>
        /// <returns>The read <see cref="Uid"/>.</returns>
        public Uid ReadUid()
        {
            // Run the anti-collision loop on the card
            this.Transceive(false, PiccCommands.Anticollision1, PiccCommands.Anticollision2);

            // Return tag UID from FIFO
            var uid = new Uid(this.ReadFromFifoAll());
            return uid;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.nativeSpiConnection?.Dispose();
            this.resetPowerDown?.Dispose();
        }

        private void Transceive(bool enableCrc, params byte[] data)
        {
            if (enableCrc)
            {
                // Enable CRC
                this.SetRegisterBits(Registers.TxMode, 0x80);
                this.SetRegisterBits(Registers.RxMode, 0x80);
            }

            // Put reader in Idle mode
            this.WriteRegister(Registers.Command, PcdCommands.Idle);

            // Clear the FIFO
            this.SetRegisterBits(Registers.FifoLevel, 0x80);

            // Write the data to the FIFO
            this.WriteToFifo(data);

            // Put reader in Transceive mode and start sending
            this.WriteRegister(Registers.Command, PcdCommands.Transceive);
            this.SetRegisterBits(Registers.BitFraming, 0x80);

            // Wait for (a generous) 25 ms
            Task.Delay(25).Wait();

            // Stop sending
            this.ClearRegisterBits(Registers.BitFraming, 0x80);

            if (enableCrc)
            {
                // Disable CRC
                this.ClearRegisterBits(Registers.TxMode, 0x80);
                this.ClearRegisterBits(Registers.RxMode, 0x80);
            }
        }

        private byte[] ReadFromFifo(int length)
        {
            var buffer = new byte[length];

            for (int i = 0; i < length; i++)
            {
                buffer[i] = this.ReadRegister(Registers.FifoData);
            }

            return buffer;
        }

        private byte[] ReadFromFifoAll()
        {
            return this.ReadFromFifo(this.GetFifoLevel());
        }

        private byte ReadFromFifo()
        {
            return this.ReadFromFifo(1)[0];
        }

        private void WriteToFifo(params byte[] values)
        {
            foreach (var b in values)
            {
                this.WriteRegister(Registers.FifoData, b);
            }
        }

        private int GetFifoLevel()
        {
            return this.ReadRegister(Registers.FifoLevel);
        }

        private byte ReadRegister(byte register)
        {
            register <<= 1;
            register |= 0x80;

            var writeBuffer = new byte[] { register, 0x00 };

            return this.TransferSpi(writeBuffer)[1];
        }

        private (byte High, byte Low) ReadFromFifo2Bytes()
        {
            var low = this.ReadRegister(Registers.FifoData);
            var high = this.ReadRegister(Registers.FifoData);
            return (high, low);
        }

        private void WriteRegister(byte register, byte value)
        {
            register <<= 1;

            var writeBuffer = new[] { register, value };

            this.TransferSpi(writeBuffer);
        }

        private byte[] TransferSpi(byte[] writeBuffer)
        {
            if (this.nativeSpiConnection == null)
            {
                throw new InvalidOperationException("Not initialized.");
            }

            var readBuffer = new byte[writeBuffer.Length];
            using (var transferBuffer = this.nativeSpiConnection.CreateTransferBuffer(writeBuffer.Length, SpiTransferMode.ReadWrite))
            {
                transferBuffer.Tx.Copy(writeBuffer, 0, 0, writeBuffer.Length);
                this.nativeSpiConnection.Transfer(transferBuffer);
                transferBuffer.Rx.Copy(0, readBuffer, 0, readBuffer.Length);
            }

            return readBuffer;
        }

        private void SetRegisterBits(byte register, byte bits)
        {
            var currentValue = this.ReadRegister(register);
            this.WriteRegister(register, (byte)(currentValue | bits));
        }

        private void ClearRegisterBits(byte register, byte bits)
        {
            var currentValue = this.ReadRegister(register);
            this.WriteRegister(register, (byte)(currentValue & ~bits));
        }
    }
}