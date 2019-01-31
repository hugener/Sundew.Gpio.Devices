// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.cs" company="Hukano">
// Copyright (c) Hukano. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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

        public MainTextView(Mfrc522Connection rfidTransceiver, Ky040Device rotaryEncoder)
        {
            this.rfidTransceiver = rfidTransceiver;
            this.rotaryEncoder = rotaryEncoder;
            this.rfidTransceiver.TagDetected += this.OnRfidTransceiverTagDetected;
            this.rotaryEncoder.Pressed += this.OnRotaryEncoderPressed;
            this.rotaryEncoder.Rotated += this.OnRotaryEncoderRotated;
        }

        public IEnumerable<object> InputTargets => null;

        public Task OnShowingAsync(IInvalidater invalidater, ICharacterContext characterContext)
        {
            this.invalidater = invalidater;
            this.rfidTransceiver.StartScanning();
            this.rotaryEncoder.Start();
            return Task.CompletedTask;
        }

        public void Render(IRenderContext renderContext)
        {
            renderContext.Home();
            if (this.tag != null)
            {
                renderContext.Write($"{this.tag} {(this.tag.IsValid ? "OK" : "NOK")}".LimitAndPadLeft(renderContext.Size.Width, ' '));
            }

            renderContext.SetPosition(0, 1);
            renderContext.WriteLine(this.rotation.ToString().LimitAndPadLeft(renderContext.Size.Width, ' '));
        }

        public Task OnClosingAsync()
        {
            return Task.CompletedTask;
        }

        private void OnRfidTransceiverTagDetected(object sender, TagDetectedEventArgs e)
        {
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
            Thread.Sleep(100);
        }
    }
}