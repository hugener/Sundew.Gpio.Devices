// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ky040DeviceWithDebounce.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.RotaryEncoders.Ky040
{
    using System;
    using System.Diagnostics;
    using global::Pi.IO.GeneralPurpose;

    /// <inheritdoc />
    /// <summary>
    /// Pin connection to a KY-040 rotary encoder.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public class Ky040DeviceWithDebounce : IRotaryEncoderWithButtonDevice
    {
        private const int FullTurnSwitchesState = 0b0011;
        private readonly IGpioConnectionDriverFactory gpioConnectionDriverFactory;
        private readonly IGpioConnectionDriver gpioConnectionDriver;
        private readonly IRotaryEncoderReporter? rotaryEncoderReporter;
        private readonly ProcessorPin clockProcessorPin;
        private readonly ProcessorPin dataProcessorPin;
        private readonly InputPinConfiguration clkPinConfiguration;
        private readonly InputPinConfiguration dtPinConfiguration;
        private readonly InputPinConfiguration buttonPinConfiguration;
        private readonly Stopwatch debouncer = new Stopwatch();
        private GpioConnection? gpioConnection;
        private TimeSpan debounce = TimeSpan.FromMilliseconds(1);
        private int lastSwitchesState = FullTurnSwitchesState;
        private int encoderValue;
        private int errorStateCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ky040DeviceWithDebounce" /> class.
        /// </summary>
        /// <param name="clockConnectorPin">The clock connector pin.</param>
        /// <param name="dataConnectorPin">The data connector pin.</param>
        /// <param name="buttonConnectorPin">The button connector pin.</param>
        /// <param name="gpioConnectionDriverFactory">The gpio connection driver factory.</param>
        /// <param name="rotaryEncoderReporter">The ky040 reporter.</param>
        public Ky040DeviceWithDebounce(
            ConnectorPin clockConnectorPin,
            ConnectorPin dataConnectorPin,
            ConnectorPin buttonConnectorPin,
            IGpioConnectionDriverFactory? gpioConnectionDriverFactory = null,
            IRotaryEncoderReporter? rotaryEncoderReporter = null)
        {
            this.gpioConnectionDriverFactory = GpioConnectionDriverFactory.EnsureGpioConnectionDriverFactory(gpioConnectionDriverFactory);
            this.gpioConnectionDriver = this.gpioConnectionDriverFactory.Get();
            this.rotaryEncoderReporter = rotaryEncoderReporter;
            this.rotaryEncoderReporter?.SetSource(typeof(IRotaryEncoderReporter), this);
            this.clkPinConfiguration = clockConnectorPin.Input().PullUp();
            this.clkPinConfiguration.OnStatusChanged(this.OnEncoderChanged);
            this.dtPinConfiguration = dataConnectorPin.Input().PullUp();
            this.dtPinConfiguration.OnStatusChanged(this.OnEncoderChanged);
            this.buttonPinConfiguration = buttonConnectorPin.Input().PullUp();
            this.buttonPinConfiguration.OnStatusChanged(this.OnButtonPressed);
            this.clockProcessorPin = clockConnectorPin.ToProcessor();
            this.dataProcessorPin = dataConnectorPin.ToProcessor();
        }

        /// <summary>
        /// Occurs when the encoder is rotated.
        /// </summary>
        public event EventHandler<RotationEventArgs>? Rotated;

        /// <summary>
        /// Occurs when the encoder is pressed.
        /// </summary>
        public event EventHandler? Pressed;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (this.gpioConnection == null)
            {
                this.gpioConnection = new GpioConnection(
                    new GpioConnectionSettings { PollInterval = global::Pi.Core.Timers.TimeSpanUtility.FromMicroseconds(100) },
                    new GpioConnectionDriverFactory(this.gpioConnectionDriver),
                    this.dtPinConfiguration,
                    this.buttonPinConfiguration,
                    this.clkPinConfiguration);
                this.debouncer.Restart();

                if ((this.gpioConnectionDriver.GetCapabilities() &
                     GpioConnectionDriverCapabilities.CanSetPinDetectedEdges) > 0)
                {
                    this.gpioConnectionDriver.SetPinDetectedEdges(this.clockProcessorPin, PinDetectedEdges.Both);
                    this.gpioConnectionDriver.SetPinResistor(this.clockProcessorPin, PinResistor.PullUp);

                    this.gpioConnectionDriver.SetPinDetectedEdges(this.dataProcessorPin, PinDetectedEdges.Both);
                    this.gpioConnectionDriver.SetPinResistor(this.dataProcessorPin, PinResistor.PullUp);
                }
            }
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            this.gpioConnection?.Dispose();
            this.gpioConnection = null;
        }

        /// <inheritdoc />
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            this.Stop();
            this.gpioConnectionDriverFactory.Dispose(this.gpioConnectionDriver);
        }

        private void OnEncoderChanged(bool obj)
        {
            if (this.debounce == TimeSpan.Zero || this.debouncer.Elapsed > this.debounce)
            {
                var lsb = this.gpioConnectionDriver.Read(this.clockProcessorPin) ? 1 : 0;
                var msb = this.gpioConnectionDriver.Read(this.dataProcessorPin) ? 1 : 0;

                var switchesState = msb << 1 | lsb;
                var encoderState = (EncoderStates)(switchesState << 2 | this.lastSwitchesState);
                switch (encoderState)
                {
                    case EncoderStates.ClockFallingEdgeAndDataIsHigh:
                    case EncoderStates.DataFallingEdgeAndClockIsLow:
                    case EncoderStates.ClockRisingEdgeAndDataIsLow:
                    case EncoderStates.DataRisingEdgeAndClockIsHigh:
                        this.encoderValue++;
                        break;
                    case EncoderStates.DataFallingEdgeAndClockIsHigh:
                    case EncoderStates.ClockFallingEdgeAndDataIsLow:
                    case EncoderStates.DataRisingEdgeAndClockIsLow:
                    case EncoderStates.ClockRisingEdgeAndDataIsHigh:
                        this.encoderValue--;
                        break;
                    default:
                        this.errorStateCount++;
                        if (this.errorStateCount > 4)
                        {
                            this.errorStateCount = 0;
                            this.lastSwitchesState = 0;
                            this.encoderValue = 0;
                        }

                        return;
                }

                if (switchesState == FullTurnSwitchesState)
                {
                    switch (this.encoderValue >> 2)
                    {
                        case 1:
                            this.RaiseRotatedClockwise();
                            this.encoderValue = 0;
                            break;
                        case -1:
                            this.RaiseRotatedCounterClockwise();
                            this.encoderValue = 0;
                            break;
                    }
                }

                this.lastSwitchesState = switchesState;
                this.debouncer.Restart();
            }
        }

        private void OnButtonPressed(bool state)
        {
            try
            {
                if (!state)
                {
                    this.Pressed?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception e)
            {
                this.rotaryEncoderReporter?.OnEncoderException(e);
            }
        }

        private void RaiseRotatedClockwise()
        {
            try
            {
                this.Rotated?.Invoke(this, new RotationEventArgs(EncoderDirection.Clockwise));
            }
            catch (Exception e)
            {
                this.rotaryEncoderReporter?.OnEncoderException(e);
            }
        }

        private void RaiseRotatedCounterClockwise()
        {
            try
            {
                this.Rotated?.Invoke(this, new RotationEventArgs(EncoderDirection.CounterClockwise));
            }
            catch (Exception e)
            {
                this.rotaryEncoderReporter?.OnEncoderException(e);
            }
        }
    }
}
