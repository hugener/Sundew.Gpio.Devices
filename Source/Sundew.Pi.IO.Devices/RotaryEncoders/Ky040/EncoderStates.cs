// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncoderStates.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Sundew.Pi.IO.Devices.RotaryEncoders.Ky040
{
    /// <summary>
    /// Defines the various encoder states for detecting increments or decrements.
    /// </summary>
    public enum EncoderStates
    {
        /// <summary>
        /// The clock falling edge and data is high.
        /// This is the first quarter turn for clockwise rotation.
        /// </summary>
        ClockFallingEdgeAndDataIsHigh = 0b0111,

        /// <summary>
        /// The data falling edge and clock is low.
        /// This is the half turn for clockwise rotation.
        /// </summary>
        DataFallingEdgeAndClockIsLow = 0b0001,

        /// <summary>
        /// The clock rising edge and data is low.
        /// This is the third quarter turn for clockwise rotation.
        /// </summary>
        ClockRisingEdgeAndDataIsLow = 0b1000,

        /// <summary>
        /// The data rising edge and clock is high.
        /// This is the full turn for clockwise rotation.
        /// </summary>
        DataRisingEdgeAndClockIsHigh = 0b1110,

        /// <summary>
        /// The data falling edge and clock is high.
        /// This is the first quarter turn for counter clockwise rotation.
        /// </summary>
        DataFallingEdgeAndClockIsHigh = 0b1011,

        /// <summary>
        /// The clock falling edge and data is low.
        /// This is the half turn for counter clockwise rotation.
        /// </summary>
        ClockFallingEdgeAndDataIsLow = 0b0010,

        /// <summary>
        /// The data rising edge and clock is low.
        /// This is the third quarter turn for counter clockwise rotation.
        /// </summary>
        DataRisingEdgeAndClockIsLow = 0b0100,

        /// <summary>
        /// The clock rising edge and data is high.
        /// This is the full turn for counter clockwise rotation.
        /// </summary>
        ClockRisingEdgeAndDataIsHigh = 0b1101,
    }
}