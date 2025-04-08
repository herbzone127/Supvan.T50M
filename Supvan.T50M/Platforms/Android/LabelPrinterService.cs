using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Supvan.T50M
{
    public partial class LabelPrinterService
    {
        private UsbManager _usbManager;
        private UsbDevice _printerDevice;
        private UsbDeviceConnection _connection;
        private UsbInterface _usbInterface;
        private UsbEndpoint _endpoint;

        public LabelPrinterService()
        {
#if ANDROID
            _usbManager = (UsbManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.UsbService);
#endif
        }

        // Check and request USB permission
        public async Task<bool> InitializePrinter()
        {
#if ANDROID
            var devices = _usbManager.DeviceList;
            if (devices.Count == 0)
            {
                Console.WriteLine("No USB devices found");
                return false;
            }

            // Find the printer (you might need to adjust VendorId/ProductId for your printer)
            _printerDevice = devices.Values.FirstOrDefault(d =>
                d.VendorId == 0x0A5F && // Example Vendor ID (Zebra)
                d.ProductId == 0x0100); // Example Product ID

            if (_printerDevice == null)
            {
                Console.WriteLine("Printer not found");
                return false;
            }

            // Request permission if needed
            if (!_usbManager.HasPermission(_printerDevice))
            {
                var permissionIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0,
                    new Intent("com.android.example.USB_PERMISSION"), PendingIntentFlags.UpdateCurrent);
                _usbManager.RequestPermission(_printerDevice, permissionIntent);
                // Note: You'll need to handle the permission response in your Activity
            }

            // Setup connection
            _connection = _usbManager.OpenDevice(_printerDevice);
            _usbInterface = _printerDevice.GetInterface(0);
            _endpoint = _usbInterface.GetEndpoint(0);

            return true;
#else
        return false;
#endif
        }

        // Print a simple label
        public void PrintLabel(string text)
        {
#if ANDROID
            if (_connection == null || _endpoint == null)
            {
                Console.WriteLine("Printer not initialized");
                return;
            }

            // Example ZPL command for a simple label
            string zplCommand = "^XA" + // Start ZPL command
                              "^FO50,50" + // Position
                              "^A0N,30,40" + // Font selection
                              $"^FD{text}^FS" + // Text to print
                              "^XZ"; // End ZPL command

            byte[] data = System.Text.Encoding.ASCII.GetBytes(zplCommand);

            int result = _connection.BulkTransfer(_endpoint, data, data.Length, 1000);
            if (result < 0)
            {
                Console.WriteLine("Failed to send print command");
            }
#endif
        }

        // Cleanup
        public void Dispose()
        {
#if ANDROID
            _connection?.Close();
#endif
        }
    }
}
