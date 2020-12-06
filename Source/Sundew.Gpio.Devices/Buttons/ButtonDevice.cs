// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ButtonDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.Buttons
{
    using System;
    using System.Device.Gpio;
    using Sundew.Gpio.Devices.Timing;

    /// <summary>
    /// Represents a button device.
    /// </summary>
    public class ButtonDevice : IButtonDevice
    {
        private readonly GpioController gpioController;
        private readonly PinEventTypes pinEventTypes;
        private readonly PullMode pullMode;
        private readonly TimeSpan debounce;
        private readonly Debouncer debouncer = new Debouncer();
        private readonly Activation activation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonDevice" /> class.
        /// </summary>
        /// <param name="gpioController">The gpio controller.</param>
        /// <param name="pin">The pin.</param>
        /// <param name="pinEventTypes">The pin event types.</param>
        /// <param name="pullMode">The pull mode.</param>
        /// <param name="debounce">The debounce.</param>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        public ButtonDevice(GpioController gpioController, int pin, PinEventTypes pinEventTypes, PullMode pullMode, TimeSpan debounce, bool activate)
        {
            this.gpioController = gpioController;
            this.Pin = pin;
            this.pinEventTypes = pinEventTypes;
            this.pullMode = pullMode;
            this.debounce = debounce;
            this.activation = new Activation(
                () => this.gpioController.IsPinOpen(this.Pin),
                () =>
                {
                    this.gpioController.OpenPin(this.Pin, this.pullMode == PullMode.Up ? PinMode.InputPullUp : PinMode.InputPullDown);
                    this.gpioController.RegisterCallbackForPinValueChangedEvent(this.Pin, this.pinEventTypes, this.OnButtonPressed);
                    this.debouncer.Start();
                },
                () =>
                {
                    this.debouncer.Stop();
                    this.gpioController.UnregisterCallbackForPinValueChangedEvent(this.Pin, this.OnButtonPressed);
                    this.gpioController.ClosePin(this.Pin);
                },
                activate);
        }

        /// <summary>
        /// Occurs when the button is pressed.
        /// </summary>
        public event EventHandler? Pressed;

        /// <summary>
        /// Gets the pin.
        /// </summary>
        /// <value>
        /// The pin.
        /// </value>
        public int Pin { get; }

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
            this.debouncer.Dispose();
            this.SetActivation(false);
        }

        private void OnButtonPressed(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            using var isDebounced = this.debouncer.Debounce(this.debounce);
            Console.WriteLine(pinValueChangedEventArgs.ChangeType + " - " + isDebounced.IsDebounced + " - " + this.debounce);
            if (isDebounced && this.pinEventTypes.HasFlag(pinValueChangedEventArgs.ChangeType))
            {
                this.Pressed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}