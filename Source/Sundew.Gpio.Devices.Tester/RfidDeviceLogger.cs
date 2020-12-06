// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RfidDeviceLogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Sundew.Gpio.Devices.RfidTransceivers;
using Sundew.Gpio.Devices.RfidTransceivers.Mfrc522;

namespace Sundew.Gpio.Devices.Tester
{
    public class RfidDeviceLogger : IRfidDeviceReporter
    {
        public void SetSource(Type target, object source)
        {
        }

        public void TagDetected(Uid uid)
        {
        }

        public void OnException(Exception exception)
        {
            Console.WriteLine(exception.ToString());
        }
    }
}