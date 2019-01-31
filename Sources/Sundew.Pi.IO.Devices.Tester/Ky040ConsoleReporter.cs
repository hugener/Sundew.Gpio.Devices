// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ky040ConsoleReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.Tester
{
    using System;
    using RotaryEncoders;

    public class Ky040ConsoleReporter : IRotaryEncoderReporter
    {
        public void SetSource(object source)
        {
        }

        public void OnEncoderException(Exception exception)
        {
        }
    }
}