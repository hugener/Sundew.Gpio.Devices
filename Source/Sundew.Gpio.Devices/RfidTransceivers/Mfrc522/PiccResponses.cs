// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PiccResponses.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.RfidTransceivers.Mfrc522
{
    internal static class PiccResponses
    {
        public const ushort AnswerToRequest = 0x0004;

        public const byte SelectAcknowledge = 0x08;

        public const byte Acknowledge = 0x0A;
    }
}
