using System.Text;
#if ANDROID
using Android.Bluetooth;
using Java.Util;
using Plugin.BLE.Abstractions.Contracts;
#endif
namespace Supvan.T50M
{
    public partial class MainPage : ContentPage
    {
        private List<IDevice> discoveredDevices = new(); // If still using Plugin.BLE elsewhere
#if ANDROID
        private BluetoothSocket socket; // Store the socket for printing
#endif

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnScanClicked(object sender, EventArgs e)
        {
            StatusLabel.Text = "Status: Checking permissions...";

            // Assuming permissions are already handled as per previous steps
            if (!await RequestBluetoothPermissions())
            {
                StatusLabel.Text = "Status: Permissions denied";
                return;
            }

            StatusLabel.Text = "Status: Scanning paired devices...";
            DevicePicker.Items.Clear();

#if ANDROID
            try
            {
                var bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
                if (bluetoothAdapter != null && bluetoothAdapter.IsEnabled)
                {
                    var devices = bluetoothAdapter.BondedDevices;
                    if (devices == null || devices.Count == 0)
                    {
                        StatusLabel.Text = "Status: No paired devices found";
                        return;
                    }

                    foreach (var device in devices)
                    {
                        DevicePicker.Items.Add(device.Name);
                        if (device.Name == "T0147B2412137156")
                        {
                            StatusLabel.Text = $"Status: Found device {device.Name}, connecting...";
                            try
                            {
                                // Connect to the device
                                socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805F9B34FB"));
                                await Task.Run(() => socket.Connect()); // Run on a background thread
                                StatusLabel.Text = "Status: Connected to device";
                                ConnectButton.IsEnabled = true;
                            }
                            catch (Exception ex)
                            {
                                StatusLabel.Text = $"Status: Connection failed: {ex.Message}";
                                socket?.Close();
                                socket = null;
                            }
                        }
                    }

                    if (DevicePicker.Items.Count == 0)
                    {
                        StatusLabel.Text = "Status: No devices found";
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Bluetooth is not enabled.", "OK");
                    StatusLabel.Text = "Status: Bluetooth not enabled";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to scan devices: {ex.Message}", "OK");
                StatusLabel.Text = "Status: Scan failed";
            }
#else
        StatusLabel.Text = "Status: This feature is only supported on Android";
#endif
        }

        private async void OnConnectClicked(object sender, EventArgs e)
        {
            if (DevicePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Please select a device.", "OK");
                return;
            }

            var selectedDeviceName = DevicePicker.Items[DevicePicker.SelectedIndex];
            if (selectedDeviceName != "T0147B2412137156")
            {
                StatusLabel.Text = "Status: Please select the correct device (T0147B2412137156)";
                return;
            }

            if (socket == null || !socket.IsConnected)
            {
                StatusLabel.Text = "Status: Not connected to the device";
                return;
            }

            StatusLabel.Text = "Status: Sending print command...";

            try
            {
                // Send a simple print command
                await SendPrintCommand(socket);
                StatusLabel.Text = "Status: Print command sent successfully";
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Status: Print failed: {ex.Message}";
            }
            finally
            {
                // Close the socket after printing
                try
                {
                    socket?.Close();
                    socket = null;
                }
                catch (Exception ex)
                {
                    StatusLabel.Text = $"Status: Failed to close socket: {ex.Message}";
                }
            }
        }

        private async Task SendPrintCommand(BluetoothSocket socket)
        {
            if (socket == null || !socket.IsConnected)
            {
                throw new Exception("Socket is not connected");
            }

            try
            {
                var outputStream = socket.OutputStream;
                if (outputStream == null)
                {
                    throw new Exception("Failed to get output stream");
                }

                // Basic print command (assuming ESC/POS-like protocol)
                byte[] initPrinter = new byte[] { 0x1B, 0x40 }; // ESC @ (Initialize printer)
                byte[] printText = Encoding.ASCII.GetBytes("Test Label from SUPVAN T50M Pro\n");
                byte[] lineFeed = new byte[] { 0x0A }; // Line feed
                byte[] printAndFeed = new byte[] { 0x1B, 0x64, 0x02 }; // ESC d 2 (Print and feed 2 lines)

                // Combine all commands
                var command = new List<byte>();
                command.AddRange(initPrinter);
                command.AddRange(printText);
                command.AddRange(lineFeed);
                command.AddRange(printAndFeed);

                // Send the command
                await Task.Run(() =>
                {
                    outputStream.Write(command.ToArray(), 0, command.Count);
                    outputStream.Flush();
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send print command: {ex.Message}");
            }
        }

        // Permission handling (as previously provided)
        private async Task<bool> RequestBluetoothPermissions()
        {
#if ANDROID
            var status = await Permissions.CheckStatusAsync<BluetoothPermissions>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<BluetoothPermissions>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Denied", "Bluetooth permissions are required to scan and connect to devices.", "OK");
                    return false;
                }
            }
#endif
            return true;
        }
    }

}
