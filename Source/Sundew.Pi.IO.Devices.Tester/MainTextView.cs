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
using Sundew.Pi.IO.Devices.Buttons;

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
        private readonly PullDownButtonDevice menuButton;
        private readonly PullDownButtonDevice playButton;
        private readonly PullDownButtonDevice nextButton;
        private readonly PullDownButtonDevice prevButton;
        private readonly ICurrentThread thread;
        private IInvalidater? invalidater;
        private ITag? tag;
        private int detectionCount;
        private int jobCounter;
        private ContinuousJob? job;
        private char lastPressed = '-';
        private int pressed;
        private int rotation;

        public MainTextView(Mfrc522Connection rfidTransceiver, Ky040Device rotaryEncoder, PullDownButtonDevice menuButton, PullDownButtonDevice playButton, PullDownButtonDevice nextButton, PullDownButtonDevice prevButton)
        {
            this.rfidTransceiver = rfidTransceiver;
            this.rotaryEncoder = rotaryEncoder;
            this.menuButton = menuButton;
            this.playButton = playButton;
            this.nextButton = nextButton;
            this.prevButton = prevButton;
            this.menuButton.Pressed += (_, e) => this.Pressed('M');
            this.playButton.Pressed += (_, e) => this.Pressed('P');
            this.nextButton.Pressed += (_, e) => this.Pressed('N');
            this.prevButton.Pressed += (_, e) => this.Pressed('B');
            this.rotaryEncoder.Pressed += (_, e) => this.Pressed('R');
            this.rotaryEncoder.Rotated += this.OnRotaryEncoderRotated;
            this.rfidTransceiver.TagDetected += this.OnRfidTransceiverTagDetected;
            this.thread = new ThreadFactory(Board.Current, true).Create();
        }

        public IEnumerable<object>? InputTargets => null;

        public Task OnShowingAsync(IInvalidater invalidater, ICharacterContext? characterContext)
        {
            this.invalidater = invalidater;
            this.rfidTransceiver.StartScanning();
            this.rotaryEncoder.Start();
            this.job = new ContinuousJob(this.Action);
            this.job.Start();
            return Task.CompletedTask;
        }

        public void OnDraw(IRenderContext renderContext)
        {
            renderContext.SetPosition(0, 0);
            renderContext.Write($"T:{this.GetTag(tag)}".LimitAndPadRight(renderContext.Size.Width, ' '));
            renderContext.SetPosition(0, 1);
            renderContext.WriteLine($"U:{this.jobCounter} P{this.lastPressed}{this.pressed} R{this.rotation}".LimitAndPadRight(renderContext.Size.Width, ' '));
        }

        public Task OnClosingAsync()
        {
            this.job?.Dispose();
            this.job = null;
            return Task.CompletedTask;
        }

        private void OnRfidTransceiverTagDetected(object? sender, TagDetectedEventArgs e)
        {
            if (this.tag?.RawData.SequenceEqual(e.Tag.RawData) == true)
            {
                this.detectionCount++;
            }
            else
            {
                this.detectionCount = 0;
            }

            this.tag = e.Tag;
            this.invalidater?.Invalidate();
        }

        private void OnRotaryEncoderRotated(object? sender, RotaryEncoders.RotationEventArgs e)
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

            this.invalidater?.Invalidate();
        }

        private void Action(CancellationToken obj)
        {
            this.jobCounter++;
            this.invalidater?.Invalidate();
            this.thread.Sleep(1000, obj);
        }

        private string GetTag(ITag? tag)
        {
            if (tag == null)
            {
                return "<none>";
            }

            return tag.IsValid ? $"{tag} {this.detectionCount,4}" : "Invalid";
        }
        private void Pressed(char button)
        {
            if (this.lastPressed == button)
            {
                this.pressed++;
                this.invalidater?.Invalidate();
                return;
            }

            this.lastPressed = button;
            this.pressed = 1;
            this.invalidater?.Invalidate();
        }
    }
}