// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ky040Device.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.RotaryEncoders.Ky040
{
    using System;
    using System.Device.Gpio;
    using Sundew.Gpio.Devices.Buttons;
    using Sundew.Gpio.Devices.Timing;

    /// <inheritdoc />
    /// <summary>
    /// Pin connection to a KY-040 rotary encoder.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public class Ky040Device : IRotaryEncoderWithButtonDevice
    {
        private const int FullTurnSwitchesState = 0b0011;
        private readonly GpioController gpioController;
        private readonly int clockPin;
        private readonly int dataPin;
        private readonly ButtonDevice buttonDevice;
        private readonly IRotaryEncoderReporter? rotaryEncoderReporter;
        private readonly Debouncer debouncer = new Debouncer();
        private readonly Debouncer debouncer2 = new Debouncer();
        private readonly Activation activation;
        private readonly TimeSpan debounce;
        private int lastSwitchesState = FullTurnSwitchesState;
        private int encoderValue;
        private int errorStateCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ky040Device" /> class.
        /// </summary>
        /// <param name="gpioController">The gpio controller.</param>
        /// <param name="clockPin">The clock connector pin.</param>
        /// <param name="dataPin">The data connector pin.</param>
        /// <param name="buttonPin">The button connector pin.</param>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        /// <param name="debounce">The debounce.</param>
        /// <param name="rotaryEncoderReporter">The rotary encoder reporter.</param>
        public Ky040Device(
            GpioController gpioController,
            int clockPin,
            int dataPin,
            int buttonPin,
            bool activate,
            TimeSpan? debounce = null,
            IRotaryEncoderReporter? rotaryEncoderReporter = null)
        {
            this.gpioController = gpioController;
            this.clockPin = clockPin;
            this.dataPin = dataPin;
            this.debounce = debounce ?? TimeSpan.FromMilliseconds(1);
            this.buttonDevice = new ButtonDevice(this.gpioController, buttonPin, PinEventTypes.Rising, PullMode.Up, this.debounce, false);
            this.buttonDevice.Pressed += this.OnButtonPressed;
            this.rotaryEncoderReporter = rotaryEncoderReporter;
            this.activation = new Activation(
                () => this.buttonDevice.IsActivated,
                () =>
                {
                    this.buttonDevice.SetActivation(true);
                    this.gpioController.OpenPin(this.clockPin, PinMode.InputPullUp);
                    this.gpioController.RegisterCallbackForPinValueChangedEvent(this.clockPin, PinEventTypes.Falling | PinEventTypes.Rising, this.OnEncoderChanged);
                    this.gpioController.OpenPin(this.dataPin, PinMode.InputPullUp);
                    this.gpioController.RegisterCallbackForPinValueChangedEvent(this.dataPin, PinEventTypes.Falling | PinEventTypes.Rising, this.OnEncoderChanged2);
                    this.debouncer.Start();
                    this.debouncer2.Start();
                    this.lastSwitchesState = this.GetSwitchesState();
                    Console.WriteLine(Convert.ToString(this.lastSwitchesState, 2).PadLeft(4, '0'));
                },
                () =>
                {
                    this.debouncer.Stop();
                    this.debouncer2.Stop();
                    this.gpioController.UnregisterCallbackForPinValueChangedEvent(this.clockPin, this.OnEncoderChanged);
                    this.gpioController.ClosePin(this.clockPin);
                    this.gpioController.UnregisterCallbackForPinValueChangedEvent(this.dataPin, this.OnEncoderChanged);
                    this.gpioController.ClosePin(this.dataPin);
                    this.buttonDevice.SetActivation(false);
                },
                activate);
            this.rotaryEncoderReporter?.SetSource(typeof(IRotaryEncoderReporter), this);
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
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            this.debouncer.Dispose();
            this.debouncer2.Dispose();
            this.SetActivation(false);
        }

        private void OnEncoderChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            using var isDebounced = this.debouncer.Debounce(TimeSpan.FromMilliseconds(5));
            if (isDebounced)
            {
                var switchesState = this.GetSwitchesState();
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
            }
        }

        private void OnEncoderChanged2(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            using var isDebounced = this.debouncer2.Debounce(TimeSpan.FromMilliseconds(5));
            if (isDebounced)
            {
                var switchesState = this.GetSwitchesState();
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
            }
        }

        private int GetSwitchesState()
        {
            var lsb = (int)this.gpioController.Read(this.clockPin);
            var msb = (int)this.gpioController.Read(this.dataPin);
            return msb << 1 | lsb;
        }

        private void OnButtonPressed(object sender, EventArgs eventArgs)
        {
            try
            {
                this.Pressed?.Invoke(this, EventArgs.Empty);
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
