using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System.Text;

namespace Supvan.T50M
{
    public partial class MainPage : ContentPage
    {
        private IBluetoothLE bluetoothLE;
        private IAdapter adapter;
        private List<IDevice> discoveredDevices = new();

        public MainPage()
        {
            InitializeComponent();
            bluetoothLE = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            adapter.DeviceDiscovered += (s, a) => DeviceDiscovered(a.Device);
        }

        private void DeviceDiscovered(IDevice device)
        {
            if (device.Name != null && !discoveredDevices.Any(d => d.Id == device.Id))
            {
                discoveredDevices.Add(device);
                DevicePicker.Items.Add(device.Name);
            }
        }

        private async void OnConnectClicked(object sender, EventArgs e)
        {
            if (DevicePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Please select a device.", "OK");
                return;
            }

            var selectedDevice = discoveredDevices[DevicePicker.SelectedIndex];
            StatusLabel.Text = $"Status: Connecting to {selectedDevice.Name}...";

            try
            {
                await adapter.ConnectToDeviceAsync(selectedDevice);

                // Assuming the T50M Pro uses a generic characteristic for printing
                var service = await selectedDevice.GetServiceAsync(Guid.Parse("000018f0-0000-1000-8000-00805f9b34fb")); // Example service UUID
                var characteristic = await service.GetCharacteristicAsync(Guid.Parse("00002af1-0000-1000-8000-00805f9b34fb")); // Example characteristic UUID

                if (characteristic != null)
                {
                    // Example print command (replace with actual T50M Pro command)
                   

                    StatusLabel.Text = "Status: Print command sent";
                }
                else
                {
                    StatusLabel.Text = "Status: No suitable characteristic found";
                }

                // Disconnect after printing
               // await adapter.DisconnectDeviceAsync(selectedDevice);
            }
            catch (DeviceConnectionException ex)
            {
                await DisplayAlert("Error", $"Connection failed: {ex.Message}", "OK");
                StatusLabel.Text = "Status: Connection failed";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Print failed: {ex.Message}", "OK");
                StatusLabel.Text = "Status: Print failed";
            }
        }
    }

}
