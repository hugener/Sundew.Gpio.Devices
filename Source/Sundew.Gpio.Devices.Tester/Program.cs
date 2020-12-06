using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Iot.Device.CharacterLcd;
using Sundew.Base.Threading;
using Sundew.Gpio.Devices.Buttons;
using Sundew.Gpio.Devices.RfidTransceivers.Mfrc522;
using Sundew.Gpio.Devices.RotaryEncoders.Ky040;
using Sundew.TextView.ApplicationFramework;
using Sundew.TextView.ApplicationFramework.DeviceInterface;
using Sundew.TextView.ApplicationFramework.TextViewRendering;

namespace Sundew.Gpio.Devices.Tester
{
    public static class Program
    {
        public static void Main()
        {
            var application = new Application();
            application.RenderException += (_, args) => Console.WriteLine(args.Exception);
            using var gpioController = new GpioController(PinNumberingScheme.Logical);
            var (textDisplayDevice, lcd) = Create(gpioController);
            using (lcd)
            {
                textDisplayDevice.Clear();
                var i = 0;
                var r = true;
                Console.CancelKeyPress += (sender, args) =>
                {
                    args.Cancel = true;
                    Console.WriteLine("did it");
                    r = false;
                };
                Thread.Sleep(200);
                textDisplayDevice.SetPosition(0, 0);
                Thread.Sleep(200);
                textDisplayDevice.Write("                ");
                Thread.Sleep(200);
                textDisplayDevice.SetPosition(0, 1);
                Thread.Sleep(200);
                textDisplayDevice.Write("                ");
                Thread.Sleep(200);
                textDisplayDevice.SetPosition(0, 0);
                Thread.Sleep(200);
                textDisplayDevice.Write("Hello");
                while (r)
                {
                    textDisplayDevice.SetPosition(0, 1);
                    textDisplayDevice.Write(i++);
                    // Thread.Sleep(100);
                }

                /* var textViewNavigator = application.StartRendering(new TextViewRendererFactory(textDisplayDevice, new TimerFactory()));
                 using var rfidTransceiver = new Mfrc522Device(gpioController, 0, 0, ToChipPin(22), false, new RfidDeviceLogger());
                 using var menuButton = new ButtonDevice(gpioController, ToChipPin(13), PinEventTypes.Falling, PullMode.Down, TimeSpan.Zero, false);
                 using var playButton = new ButtonDevice(gpioController, ToChipPin(15), PinEventTypes.Falling, PullMode.Down, TimeSpan.Zero, false);
                 using var nextButton = new ButtonDevice(gpioController, ToChipPin(18), PinEventTypes.Falling, PullMode.Down, TimeSpan.Zero, false);
                 using var prevButton = new ButtonDevice(gpioController, ToChipPin(16), PinEventTypes.Falling, PullMode.Down, TimeSpan.Zero, false);
                 using var rotaryEncoder = new Ky040Device(gpioController, ToChipPin(36), ToChipPin(38), ToChipPin(40), false, TimeSpan.Zero, new Ky040ConsoleReporter());
                 menuButton.Pressed += (sender, args) => Console.WriteLine("Menu");
                 playButton.Pressed += (sender, args) => Console.WriteLine("Play");
                 nextButton.Pressed += (sender, args) => Console.WriteLine("Next");
                 prevButton.Pressed += (sender, args) => Console.WriteLine("Next");
                 rotaryEncoder.Pressed += (sender, args) => Console.WriteLine("Rotary");
                 rotaryEncoder.Rotated += RotaryEncoder_Rotated;
                 rfidTransceiver.TagDetected += RfidTransceiver_TagDetected;
                 textViewNavigator.NavigateToAsync(new MainTextView(rfidTransceiver, rotaryEncoder, menuButton, playButton, nextButton, prevButton));
                 application.Run();*/
                textDisplayDevice.Clear();
            }

            Console.WriteLine("Ending...");
        }

        private static void RfidTransceiver_TagDetected(object? sender, RfidTransceivers.TagDetectedEventArgs e)
        {
            Console.WriteLine(e.Tag);
        }

        private static void RotaryEncoder_Rotated(object? sender, RotaryEncoders.RotationEventArgs e)
        {
            Console.WriteLine(e.EncoderDirection);
        }

        private static (ITextDisplayDevice, IDisposable) Create(GpioController gpioController)
        {
            var lcdInterface = LcdInterface.CreateGpio(ToChipPin(29), ToChipPin(32),
                new[] { ToChipPin(31), ToChipPin(33), ToChipPin(35), ToChipPin(37) },
                controller: gpioController,
                shouldDispose: false);
            var hd44780 = new Lcd1602(lcdInterface);

            return (new Sundew.TextView.Iot.Devices.Drivers.LcdTextDisplayDevice(hd44780, "A00"), hd44780);
        }

        private static int ToChipPin(int pinNumber)
        {
            return pinNumber switch
            {
                3 => 2,
                5 => 3,
                7 => 4,
                8 => 14,
                10 => 15,
                11 => 17,
                12 => 18,
                13 => 27,
                15 => 22,
                16 => 23,
                18 => 24,
                19 => 10,
                21 => 9,
                22 => 25,
                23 => 11,
                24 => 8,
                26 => 7,
                27 => 0,
                28 => 1,
                29 => 5,
                31 => 6,
                32 => 12,
                33 => 13,
                35 => 19,
                36 => 16,
                37 => 26,
                38 => 20,
                40 => 21,
                _ => throw new ArgumentException(
                    $"Board (header) pin {pinNumber} is not a GPIO pin on the {typeof(Program).GetType().Name} device.",
                    nameof(pinNumber))
            };
        }
    }
}
