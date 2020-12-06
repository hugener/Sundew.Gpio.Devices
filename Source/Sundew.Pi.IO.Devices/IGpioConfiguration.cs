// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGpioConfiguration.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices
{
    using global::Pi.IO.GeneralPurpose;

    /// <summary>
    /// Interface for a gpio pin configuration.
    /// </summary>
    public interface IGpioConfiguration
    {
        /// <summary>
        /// Gets the pin configuration.
        /// </summary>
        /// <value>
        /// The pin configuration.
        /// </value>
        PinConfiguration PinConfiguration { get; }
    }
}