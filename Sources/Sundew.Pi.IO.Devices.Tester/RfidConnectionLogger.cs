// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RfidConnectionLogger.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.Tester
{
    using System;
    using RfidTransceivers;
    using RfidTransceivers.Mfrc522;

    public class RfidConnectionLogger : IRfidConnectionReporter
    {
        public void SetSource(object source)
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