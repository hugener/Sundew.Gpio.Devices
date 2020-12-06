// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwitchDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.Buttons
{
    using System;
    using System.Device.Gpio;

    /// <summary>
    /// Pin connection to a pull down switch.
    /// </summary>
    public class SwitchDevice : ISwitchDevice
    {
        private readonly GpioController gpioController;
        private readonly PullMode pullMode;
        private readonly Activation activation;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchDevice" /> class.
        /// </summary>
        /// <param name="gpioController">The gpio controller.</param>
        /// <param name="pin">The pin.</param>
        /// <param name="pullMode">The pull mode.</param>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        public SwitchDevice(GpioController gpioController, int pin, PullMode pullMode, bool activate)
        {
            this.gpioController = gpioController;
            this.Pin = pin;
            this.pullMode = pullMode;
            this.activation = new Activation(
                () => this.gpioController.IsPinOpen(this.Pin),
                () =>
                {
                    this.gpioController.OpenPin(this.Pin, this.pullMode == PullMode.Up ? PinMode.InputPullUp : PinMode.InputPullDown);
                    this.gpioController.RegisterCallbackForPinValueChangedEvent(this.Pin, PinEventTypes.Rising | PinEventTypes.Falling, this.OnSwitchChanged);
                    this.State = this.gpioController.Read(this.Pin) == PinValue.High;
                },
                () =>
                {
                    this.gpioController.UnregisterCallbackForPinValueChangedEvent(this.Pin, this.OnSwitchChanged);
                    this.gpioController.ClosePin(this.Pin);
                },
                activate);
        }

        /// <summary>
        /// Occurs when the button is pressed.
        /// </summary>
        public event EventHandler<SwitchEventArgs>? StateChanged;

        /// <summary>
        /// Gets the pin.
        /// </summary>
        /// <value>
        /// The pin.
        /// </value>
        public int Pin { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SwitchDevice"/> is state.
        /// </summary>
        /// <value>
        ///   <c>true</c> if state; otherwise, <c>false</c>.
        /// </value>
        public bool State { get; private set; }

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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.SetActivation(false);
        }

        private void OnSwitchChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            this.State = this.gpioController.Read(this.Pin) == PinValue.High;
            this.StateChanged?.Invoke(this, new SwitchEventArgs(this.State));
        }
    }
}