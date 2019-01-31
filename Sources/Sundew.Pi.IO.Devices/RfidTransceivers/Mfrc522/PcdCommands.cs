// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PcdCommands.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Pi.IO.Devices.RfidTransceivers.Mfrc522
{
    internal static class PcdCommands
    {
        public const byte Idle = 0x00;

        public const byte MifareAuthenticate = 0x0E;

        public const byte Transceive = 0x0C;
    }
}
