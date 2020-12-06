// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AntennaGain.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.RfidTransceivers.Mfrc522
{
    /// <summary>
    /// Specifies the antenna gain.
    /// </summary>
    public enum AntennaGain : byte
    {
        /// <summary>
        /// The gain18.
        /// </summary>
        Gain18 = 0,

        /// <summary>
        /// The gain23.
        /// </summary>
        Gain23 = 1 << 4,

        /// <summary>
        /// The gain 18.
        /// </summary>
        Gain_18 = 2 << 4,

        /// <summary>
        /// The gain 23.
        /// </summary>
        Gain_23 = 3 << 4,

        /// <summary>
        /// The gain33.
        /// </summary>
        Gain33 = 4 << 4,

        /// <summary>
        /// The gain38.
        /// </summary>
        Gain38 = 5 << 4,

        /// <summary>
        /// The gain43.
        /// </summary>
        Gain43 = 6 << 4,

        /// <summary>
        /// The gain48.
        /// </summary>
        Gain48 = 7 << 4,
    }
}