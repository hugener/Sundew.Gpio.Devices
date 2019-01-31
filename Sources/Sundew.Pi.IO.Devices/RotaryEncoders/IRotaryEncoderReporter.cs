// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRotaryEncoderReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.RotaryEncoders
{
    using System;
    using Sundew.Base.Reporting;

    /// <summary>
    /// Interface for implementing a Ky040 reporter.
    /// </summary>
    /// <seealso cref="Sundew.Base.Reporting.IReporter" />
    public interface IRotaryEncoderReporter : IReporter
    {
        /// <summary>
        /// Called when [encoder exception].
        /// </summary>
        /// <param name="exception">The exception.</param>
        void OnEncoderException(Exception exception);
    }
}