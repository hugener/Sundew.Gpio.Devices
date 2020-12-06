// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ky040ConsoleReporter.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Sundew.Gpio.Devices.RotaryEncoders;

namespace Sundew.Gpio.Devices.Tester
{
    public class Ky040ConsoleReporter : IRotaryEncoderReporter
    {
        public void SetSource(Type target, object source)
        {
        }

        public void OnEncoderException(Exception exception)
        {
        }
    }
}