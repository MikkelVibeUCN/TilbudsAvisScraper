using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;

        private int permissionLevel = 0;
        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }


        private async void SubmitToken_Click(object sender, RoutedEventArgs e)
        {
            string token = tokenInput.Text;

            // Simulate token verification (replace with actual verification logic)
            if (await VerifyToken())
            {
                EnableButtons();

                // Hide the token input area
                tokenInput.Visibility = Visibility.Collapsed;
                buttonGrid.Visibility = Visibility.Visible; // Show buttons
            }
            else
            {
                MessageBox.Show("Invalid token. Please try again.");
            }
        }

        private async Task<bool> VerifyToken()
        {
            string token = tokenInput.Text;
            int permissionLevel = 3; // Adjust based on your logic

            try
            {
                // Call the API to validate the token
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:7133/api/v1/APIUser?token={token}&permissionLevel={permissionLevel}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return false;
                }
                else
                {
                    MessageBox.Show($"An error occurred: {response.ReasonPhrase}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        private void EnableButtons()
        {
            btnScrape.IsEnabled = true; // Admin can scrape
            btnUpdateProducts.IsEnabled = true; // Admin can update products
            btnGenerateAPI.IsEnabled = true; // Admin can generate API token
            btnBackupDatabase.IsEnabled = true; // Admin can backup database
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnClick_Scrape(object sender, RoutedEventArgs e)
        {

        }
        private void btnClick_UpdateProducts(object sender, RoutedEventArgs e)
        {

        }
        private void btnClick_GenerateAPIUser(object sender, RoutedEventArgs e)
        {

        }
        private void btnClick_BackupDatabase(object sender, RoutedEventArgs e)
        {

        }
    }
}