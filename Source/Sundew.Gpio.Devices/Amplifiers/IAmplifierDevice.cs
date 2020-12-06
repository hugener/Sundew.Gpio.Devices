// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAmplifierDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.Amplifiers
{
    /// <summary>
    /// Interface for implementing an amplifier.
    /// </summary>
    /// <seealso cref="IDevice" />
    public interface IAmplifierDevice : IDevice
    {
        /// <summary>
        /// Gets a value indicating whether this instance is muted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is muted; otherwise, <c>false</c>.
        /// </value>
        bool IsMuted { get; }

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <returns>The current volume.</returns>
        byte GetVolume();

        /// <summary>
        /// Sets the volume.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The actual volume.</returns>
        byte SetVolume(byte value);

        /// <summary>
        /// Sets the state of the mute.
        /// </summary>
        /// <param name="mute">if set to <c>true</c> [mute].</param>
        /// <returns>The current mute state.</returns>
        bool SetMuteState(bool mute);

        /// <summary>
        /// Toggles the mute.
        /// </summary>
        void ToggleMute();

        /// <summary>
        /// Sets the state of the shutdown pin.
        /// </summary>
        /// <param name="isShutdown">if set to <c>true</c> the amplifier is shutdown.</param>
        void SetShutdownState(bool isShutdown);
    }
}