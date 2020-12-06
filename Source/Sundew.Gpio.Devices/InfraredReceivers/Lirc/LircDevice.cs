// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LircDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// Based on: https://github.com/shawty/raspberrypi-csharp-lirc

namespace Sundew.Gpio.Devices.InfraredReceivers.Lirc
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Sundew.Base.Collections;
    using Sundew.Base.Threading.Jobs;

    /// <summary>
    /// Connection to a unix lirc socket.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public partial class LircDevice
    {
        private const string LircSocketName = "/var/run/lirc/lircd";
        private readonly CancellableJob receiveJob;
        private readonly Activation activation;

        /// <summary>
        /// Initializes a new instance of the <see cref="LircDevice" /> class.
        /// </summary>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        public LircDevice(bool activate)
        {
            this.receiveJob = new CancellableJob(this.Receive);
            this.activation = new Activation(
                () => this.receiveJob.IsRunning,
                () => this.receiveJob.Start(),
                () => this.receiveJob.Stop(),
                activate);
        }

        /// <summary>
        /// Occurs when a lirc command received.
        /// </summary>
        public event EventHandler<LircCommandEventArgs>? CommandReceived;

        /// <summary>
        /// Gets a value indicating whether this instance is activated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is activated; otherwise, <c>false</c>.
        /// </value>
        public bool IsActivated => this.activation.IsActivated;

        /// <summary>
        /// Sets the activation.
        /// </summary>
        /// <param name="activate">if set to <c>true</c> [activate].</param>
        /// <returns>
        /// A value indicating whether the operation was successful.
        /// </returns>
        public bool SetActivation(bool activate)
        {
            return this.activation.SetActivation(activate);
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            this.activation.SetActivation(false);
            this.receiveJob.Dispose();
        }

        private static LircKeyCodes GetKeyCode(string keyCodeRaw)
        {
            if (KeyMap.TryGetValue(keyCodeRaw, out var keyCode))
            {
                return keyCode;
            }

            return LircKeyCodes.KeyUnknown;
        }

        private async Task Receive(CancellationToken cancellationToken)
        {
            var buffer = new byte[500];
            using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            await socket.ConnectAsync(UnixEndPoint.Create(LircSocketName)).ConfigureAwait(false);

            while (!cancellationToken.IsCancellationRequested)
            {
                if (socket.Poll(1000, SelectMode.SelectRead))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var size = await socket.ReceiveAsync(buffer.GetSegment(), SocketFlags.None).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();

                    var command = Encoding.ASCII.GetString(buffer, 0, size);
                    var commandParts = command.Split(' ');
                    var keyCodeRaw = commandParts[2];
                    var keyCode = GetKeyCode(keyCodeRaw);
                    this.CommandReceived?.Invoke(this, new LircCommandEventArgs(keyCode, keyCodeRaw, int.Parse(commandParts[1]), commandParts[3]));
                }
            }
        }
    }
}