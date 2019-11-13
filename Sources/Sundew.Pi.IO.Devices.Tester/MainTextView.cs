// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using Pi;
using Pi.Core;
using Pi.Core.Threading;
using Sundew.Base.Threading;
using Sundew.Base.Threading.Jobs;

namespace Sundew.Pi.IO.Devices.Tester
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Sundew.Base.Text;
    using Sundew.Pi.IO.Devices.RfidTransceivers;
    using Sundew.Pi.IO.Devices.RfidTransceivers.Mfrc522;
    using Sundew.Pi.IO.Devices.RotaryEncoders.Ky040;
    using Sundew.TextView.ApplicationFramework.TextViewRendering;

    public class MainTextView : ITextView
    {
        private readonly Mfrc522Connection rfidTransceiver;
        private readonly Ky040Device rotaryEncoder;
        private IInvalidater invalidater;
        private int rotation;
        private ITag tag;
        private int detectionCount;
        private int jobCounter;
        private ContinuousJob job;
        private ICurrentThread thread;

        public MainTextView(Mfrc522Connection rfidTransceiver, Ky040Device rotaryEncoder)
        {
            this.rfidTransceiver = rfidTransceiver;
            this.rotaryEncoder = rotaryEncoder;
            this.rfidTransceiver.TagDetected += this.OnRfidTransceiverTagDetected;
            this.rotaryEncoder.Pressed += this.OnRotaryEncoderPressed;
            this.rotaryEncoder.Rotated += this.OnRotaryEncoderRotated;
            this.thread = new ThreadFactory(Board.Current, true).Create();
        }

        public IEnumerable<object> InputTargets => null;

        public Task OnShowingAsync(IInvalidater invalidater, ICharacterContext characterContext)
        {
            this.invalidater = invalidater;
            //this.rfidTransceiver.StartScanning();
            this.rotaryEncoder.Start();
            this.job = new ContinuousJob(this.Action);
            this.job.Start();
            return Task.CompletedTask;
        }

        public void OnDraw(IRenderContext renderContext)
        {
            renderContext.Home();
            if (this.tag != null)
            {
                renderContext.Write($"{this.tag} {(this.tag.IsValid ? "OK" : "NOK")} {this.detectionCount,4}".LimitAndPadLeft(renderContext.Size.Width, ' '));
            }

            renderContext.SetPosition(0, 1);
            renderContext.WriteLine($"{this.jobCounter} {this.rotation}".LimitAndPadLeft(renderContext.Size.Width, ' '));
        }

        public Task OnClosingAsync()
        {
            this.job.Stop();
            this.job.Dispose();
            return Task.CompletedTask;
        }

        private void OnRfidTransceiverTagDetected(object sender, TagDetectedEventArgs e)
        {
            if (this.tag != null && this.tag.RawData.SequenceEqual(e.Tag.RawData))
            {
                this.detectionCount++;
            }
            else
            {
                this.detectionCount = 0;
            }

            this.tag = e.Tag;
            this.invalidater.Invalidate();
        }

        private void OnRotaryEncoderPressed(object sender, EventArgs e)
        {
        }

        private void OnRotaryEncoderRotated(object sender, RotaryEncoders.RotationEventArgs e)
        {
            switch (e.EncoderDirection)
            {
                case RotaryEncoders.EncoderDirection.Clockwise:
                    unchecked
                    {
                        this.rotation++;
                    }

                    break;
                case RotaryEncoders.EncoderDirection.CounterClockwise:
                    unchecked
                    {
                        this.rotation--;
                    }

                    break;
            }

            this.invalidater.Invalidate();
        }

        private void Action(CancellationToken obj)
        {
            this.jobCounter++;
            this.invalidater.Invalidate();
            this.thread.Sleep(1000, obj);
        }
    }
}