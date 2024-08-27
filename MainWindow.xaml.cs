using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ColaApps.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using System.Globalization;

namespace ColaApps
{

    public sealed partial class MainWindow : Window
    {
        private readonly FirebaseService _firebaseService;

        public MainWindow()
        {
            this.InitializeComponent();
            _firebaseService = new FirebaseService();
            InitializeWebView();
            LoadStores();
        }


        private async void InitializeWebView()
        {
            await webView2.EnsureCoreWebView2Async();
        }
        private async void LoadStores()
        {
            try
            {
                var storesData = await _firebaseService.ReadStoresAsync();
                storeComboBox.Items.Clear();

                foreach (var store in storesData)
                {
                    string storeName = store.Name;
                    storeComboBox.Items.Add(new ComboBoxItem { Content = storeName });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading stores: " + ex.Message);
            }
        }
        private async void UpdateData_Click(object sender, RoutedEventArgs e)
        {
            var selectedStore = (storeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var bottleCount = bottleCountTextBox.Text;
            var literCount = literCountTextBox.Text;

            if (string.IsNullOrEmpty(selectedStore) || string.IsNullOrEmpty(bottleCount) || string.IsNullOrEmpty(literCount))
            {
                statusTextBlock.Text = "Please fill all fields.";
                return;
            }

            var path = $"stores/{selectedStore}";
            var existingData = await _firebaseService.ReadDataAsync(path);

            var latitude = existingData.Latitude ?? 0;
            var longitude = existingData.Longitude ?? 0;

            var data = new
            {
                BottleCount = bottleCount,
                LiterCount = literCount,
                Latitude = latitude,
                Longitude = longitude,
            };

            await _firebaseService.WriteDataAsync(path, data);

            statusTextBlock.Text = "Baþarýyla Güncellendi.";
        }

        private async void StoreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedStore = (storeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(selectedStore))
                return;

            var path = $"stores/{selectedStore}";
            var data = await _firebaseService.ReadDataAsync(path);

            if (data != null)
            {
                bottleCountTextBox.Text = data.BottleCount ?? "0";
                literCountTextBox.Text = data.LiterCount ?? "0";

                string latitude = data.Latitude.ToString("F3", CultureInfo.InvariantCulture);
                string longitude = data.Longitude.ToString("F3", CultureInfo.InvariantCulture);

                string googleMapsHtml = $@"
                    <html>
                    <body style='margin:0;padding:0;'>
                        <iframe 
                            width='100%' 
                            height='100%' 
                            frameborder='0' 
                            style='border:0' 
                            src='https://www.google.com/maps/embed/v1/place?q={latitude}%2C{longitude}&key={your_api_key}' 
                            allowfullscreen>
                        </iframe>
                    </body>
                    </html>";

                Debug.WriteLine($"Google Maps HTML: {googleMapsHtml}");
                webView2.NavigateToString(googleMapsHtml);
            }
        }
        private async void AddStore_Click(object sender, RoutedEventArgs e)
        {
            string storeName = newStoreNameTextBox.Text.Trim();
            string bottleCount = newBottleCountTextBox.Text.Trim();
            string literCount = newLiterCountTextBox.Text.Trim();
            string latitudeStr = newLatitudeTextBox.Text.Trim();
            string longitudeStr = newLongitudeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(storeName) || string.IsNullOrEmpty(bottleCount) ||
                string.IsNullOrEmpty(literCount) || string.IsNullOrEmpty(latitudeStr) ||
                string.IsNullOrEmpty(longitudeStr))
            {
                statusTextBlock.Text = "Lütfen tüm alanlarý doldurun.";
                return;
            }

            if (!double.TryParse(latitudeStr, out double latitude) ||
                !double.TryParse(longitudeStr, out double longitude))
            {
                statusTextBlock.Text = "Geçerli bir enlem ve boylam girin.";
                return;
            }

            var newStoreData = new
            {
                BottleCount = bottleCount,
                LiterCount = literCount,
                Latitude = latitude,
                Longitude = longitude,
            };


            await _firebaseService.WriteDataAsync($"stores/{storeName}", newStoreData);
            statusTextBlock.Text = "Bayi baþarýyla eklendi.";
            newStoreNameTextBox.Text = "";
            newBottleCountTextBox.Text = "";
            newLiterCountTextBox.Text = "";
            newLatitudeTextBox.Text = "";
            newLongitudeTextBox.Text = "";
            LoadStores();
        }

    }

}

