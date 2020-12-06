// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LircDevice.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
// Based on: https://github.com/shawty/raspberrypi-csharp-lirc

namespace Sundew.Pi.IO.Devices.InfraredReceivers.Lirc
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

        /// <summary>
        /// Initializes a new instance of the <see cref="LircDevice"/> class.
        /// </summary>
        public LircDevice()
        {
            this.receiveJob = new CancellableJob(this.Receive);
        }

        /// <summary>
        /// Occurs when a lirc command received.
        /// </summary>
        public event EventHandler<LircCommandEventArgs>? CommandReceived;

        /// <summary>
        /// Starts listening for commands.
        /// </summary>
        public void StartListening()
        {
            this.receiveJob.Start();
        }

        /// <summary>
        /// Stops listening for commands.
        /// </summary>
        public void Stop()
        {
            this.receiveJob.Stop();
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
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
            using (var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP))
            {
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
}