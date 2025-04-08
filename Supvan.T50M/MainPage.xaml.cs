using System.Text;

namespace Supvan.T50M
{
    public partial class MainPage : ContentPage
    {
        private LabelPrinterService _printerService;
        public MainPage()
        {
            InitializeComponent();
            _printerService = new LabelPrinterService();
        }
        private async void OnPrintButtonClicked(object sender, EventArgs e)
        {
            bool initialized = await _printerService.InitializePrinter();
            if (initialized)
            {
                _printerService.PrintLabel("Hello from MAUI!");
            }
            else
            {
                await DisplayAlert("Error", "Could not connect to printer", "OK");
            }
        }

    }

}
