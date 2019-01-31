namespace Sundew.Pi.IO.Devices.Tester
{
    using System;
    using global::Pi.IO.Devices.Displays.Hd44780;
    using global::Pi.IO.GeneralPurpose;
    using global::Pi.Timers;
    using Sundew.Pi.IO.Devices.RfidTransceivers.Mfrc522;
    using Sundew.Pi.IO.Devices.RotaryEncoders.Ky040;
    using Sundew.TextView.ApplicationFramework;
    using Sundew.TextView.ApplicationFramework.DeviceInterface;
    using Sundew.TextView.ApplicationFramework.TextViewRendering;
    using Sundew.TextView.Pi.Drivers.Displays.Hd44780;

    class Program
    {
        static void Main(string[] args)
        {
            var application = new Application();
            using (var gpioConnectionDriverFactory = new GpioConnectionDriverFactory(true))
            {
                var textViewNavigator =
                    application.StartRendering(new TextViewRendererFactory(new ConsoleDisplayDevice(), new TimerFactory()));
                using (var rfidTransceiver =
                    new Mfrc522Connection("/dev/spidev0.0", ConnectorPin.P1Pin22, gpioConnectionDriverFactory, null, new RfidConnectionLogger()))
                using (var rotaryEncoder = new Ky040Device(
                    ConnectorPin.P1Pin36,
                    ConnectorPin.P1Pin38,
                    ConnectorPin.P1Pin40,
                    gpioConnectionDriverFactory,
                    new Ky040ConsoleReporter()))
                {
                    textViewNavigator.NavigateToAsync(new MainTextView(rfidTransceiver, rotaryEncoder));
                    application.Run();
                }
            }
        }

        private static (ITextDisplayDevice, IDisposable) Create(IGpioConnectionDriverFactory gpioConnectionDriverFactory)
        {
            var hd44780LcdDeviceSettings = new Hd44780LcdDeviceSettings
            {
                ScreenHeight = 2,
                ScreenWidth = 16,
                Encoding = new Hd44780A00Encoding()
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
