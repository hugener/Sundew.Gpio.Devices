namespace Sundew.Pi.IO.Devices.Tester
{
    using System;
    using System.Threading;
    using global::Pi.IO.Devices.Displays.Hd44780;
    using global::Pi.IO.GeneralPurpose;
    using global::Pi.Core.Timers;
    using Sundew.Base.Text;
    using Sundew.Pi.IO.Devices.Buttons;
    using Sundew.Pi.IO.Devices.RfidTransceivers.Mfrc522;
    using Sundew.Pi.IO.Devices.RotaryEncoders.Ky040;
    using Sundew.TextView.ApplicationFramework;
    using Sundew.TextView.ApplicationFramework.DeviceInterface;
    using Sundew.TextView.ApplicationFramework.TextViewRendering;
    using Sundew.TextView.Pi.Drivers.Displays.Hd44780;

    public static class Program
    {
        public static void Main()
        {
            var application = new Application();
            application.RenderException += (_, args) => Console.WriteLine(args.Exception);
            using (var timerFactory = new TimerFactory())
            using (var gpioConnectionDriverFactory = new GpioConnectionDriverFactory(true))
            {
                var (textDisplayDevice, lcd) = Create(gpioConnectionDriverFactory);
                using (lcd)
                {
                    // var textViewNavigator = application.StartRendering(new TextViewRendererFactory(textDisplayDevice, timerFactory));
                    var menuButton = new PullDownButtonDevice(ConnectorPin.P1Pin13);
                    var playButton = new PullDownButtonDevice(ConnectorPin.P1Pin15);
                    var nextButton = new PullDownButtonDevice(ConnectorPin.P1Pin16);
                    var prevButton = new PullDownButtonDevice(ConnectorPin.P1Pin18);
                    using (var gpioConnection = new GpioConnection(gpioConnectionDriverFactory, menuButton.PinConfiguration, playButton.PinConfiguration, nextButton.PinConfiguration, prevButton.PinConfiguration))
                    {
                        using (var rfidTransceiver = new Mfrc522Connection("/dev/spidev0.0", ConnectorPin.P1Pin22, gpioConnectionDriverFactory, null, new RfidConnectionLogger()))
                        using (var rotaryEncoder = new Ky040Device(ConnectorPin.P1Pin36, ConnectorPin.P1Pin38, ConnectorPin.P1Pin40, gpioConnectionDriverFactory, new Ky040ConsoleReporter()))
                        {
                            menuButton.Pressed += (sender, eventArgs) => Console.WriteLine("Menu");
                            playButton.Pressed += (sender, eventArgs) => Console.WriteLine("Play");
                            nextButton.Pressed += (sender, eventArgs) => Console.WriteLine("Next");
                            prevButton.Pressed += (sender, eventArgs) => Console.WriteLine("Prev");
                            rotaryEncoder.Pressed += (sender, args) => Console.WriteLine("Rotary");
                            rotaryEncoder.Rotated += RotaryEncoder_Rotated;
                            rfidTransceiver.TagDetected += RfidTransceiver_TagDetected;
                            // textViewNavigator.NavigateToAsync(new MainTextView(rfidTransceiver, rotaryEncoder, menuButton,
                            //    playButton, nextButton, prevButton));
                            // application.Run();
                            var i = 0;
                            var token = new CancellationTokenSource();
                            Console.CancelKeyPress += (sender, args) => token.Cancel();
                            while (!token.IsCancellationRequested)
                            {
                                textDisplayDevice.WriteLine(AlignedString.Format("Hello: {0:9, <>}", i++));
                                Thread.Sleep(100);
                                textDisplayDevice.WriteLine("                ");
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Ending...");
        }

        private static void RfidTransceiver_TagDetected(object? sender, RfidTransceivers.TagDetectedEventArgs e)
        {
            Console.WriteLine($"{e.Tag}");
        }

        private static void RotaryEncoder_Rotated(object? sender, RotaryEncoders.RotationEventArgs e)
        {
            Console.WriteLine($"{e.EncoderDirection}");
        }

        private static (ITextDisplayDevice, IDisposable) Create(IGpioConnectionDriverFactory gpioConnectionDriverFactory)
        {
            var hd44780LcdDeviceSettings = new Hd44780LcdDeviceSettings
            {
                ScreenHeight = 2,
                ScreenWidth = 16,
                Encoding = new Hd44780A00Encoding(),
                SyncDelay = TimeSpan.FromMilliseconds(10),
            };
            var hd44780LcdDevice = new Hd44780LcdDevice(
                hd44780LcdDeviceSettings,
                gpioConnectionDriverFactory,
                ConnectorPin.P1Pin29,
                ConnectorPin.P1Pin32,
                new Hd44780DataPins(
                    ConnectorPin.P1Pin31,
                    ConnectorPin.P1Pin33,
                    ConnectorPin.P1Pin35,
                    ConnectorPin.P1Pin37));
            return (new Hd44780TextDisplayDevice(hd44780LcdDevice, hd44780LcdDeviceSettings), hd44780LcdDevice);
        }
    }
}
