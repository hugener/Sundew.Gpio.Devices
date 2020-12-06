// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PullDownSwitchDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.Buttons
{
    using System;
    using global::Pi.IO.GeneralPurpose;

    /// <summary>
    /// Pin connection to a pull down switch.
    /// </summary>
    public class PullDownSwitchDevice : ISwitchDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullDownSwitchDevice" /> class.
        /// </summary>
        /// <param name="switchConnectorPin">The switch connector pin.</param>
        /// <param name="gpioConnectionDriver">The gpio connection driver.</param>
        public PullDownSwitchDevice(ConnectorPin switchConnectorPin, IGpioConnectionDriver gpioConnectionDriver)
        {
            this.PinConfiguration = switchConnectorPin.Input().PullDown();
            this.PinConfiguration.OnStatusChanged(this.OnSwitchChanged);
            using (var switchPin = gpioConnectionDriver.In(switchConnectorPin))
            {
                this.State = switchPin.Read();
            }
        }

        /// <summary>
        /// Occurs when the button is pressed.
        /// </summary>
        public event EventHandler<SwitchEventArgs>? StateChanged;

        /// <summary>
        /// Gets a value indicating whether this <see cref="PullDownSwitchDevice"/> is state.
        /// </summary>
        /// <value>
        ///   <c>true</c> if state; otherwise, <c>false</c>.
        /// </value>
        public bool State { get; private set; }

        /// <summary>
        /// Gets the pin configuration.
        /// </summary>
        /// <value>
        /// The pin configuration.
        /// </value>
        public PinConfiguration PinConfiguration { get; }

        private void OnSwitchChanged(bool state)
        {
            this.State = state;
            this.StateChanged?.Invoke(this, new SwitchEventArgs(state));
        }
    }
}