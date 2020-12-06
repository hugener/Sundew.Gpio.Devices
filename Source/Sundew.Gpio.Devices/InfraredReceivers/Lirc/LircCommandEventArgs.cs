// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LircCommandEventArgs.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sundew.Gpio.Devices.InfraredReceivers.Lirc
{
    using System;

    /// <summary>
    /// Event arguments for when a lirc command is received.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class LircCommandEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LircCommandEventArgs"/> class.
        /// </summary>
        /// <param name="keyCode">The key code.</param>
        /// <param name="keyCodeRaw">The key code raw.</param>
        /// <param name="repeatCount">The repeat count.</param>
        /// <param name="remoteName">Name of the remote.</param>
        public LircCommandEventArgs(LircKeyCodes keyCode, string keyCodeRaw, int repeatCount, string remoteName)
        {
            this.KeyCode = keyCode;
            this.KeyCodeRaw = keyCodeRaw;
            this.RepeatCount = repeatCount;
            this.RemoteName = remoteName;
        }

        /// <summary>
        /// Gets the repeat count.
        /// </summary>
        /// <value>
        /// The repeat count.
        /// </value>
        public int RepeatCount { get; }

        /// <summary>
        /// Gets the key code.
        /// </summary>
        /// <value>
        /// The key code.
        /// </value>
        public LircKeyCodes KeyCode { get; }

        /// <summary>
        /// Gets the key code raw.
        /// </summary>
        /// <value>
        /// The key code raw.
        /// </value>
        public string KeyCodeRaw { get; }

        /// <summary>
        /// Gets the name of the remote.
        /// </summary>
        /// <value>
        /// The name of the remote.
        /// </value>
        public string RemoteName { get; }
    }
}