using System.Runtime.CompilerServices;
using System.Windows;
using APIIntegrationLibrary;
using APIIntegrationLibrary.Client;
using static System.Net.WebRequestMethods;
namespace DesktopApplication
{
    public partial class MainWindow : Window
    {
        private string Token;
        private const string DebugAPIUrl = "https://localhost:7133/api/v1";
        private const string ServerAPIUrl = "http://94.72.103.138:8801/api/v1/";

        private static string APIUrl = DebugAPIUrl;

        private int permissionLevel = 0;
        private readonly TokenValidator _tokenValidation;
        public MainWindow()
        {
            _tokenValidation = new TokenValidator(APIUrl);

            InitializeComponent();
        }


        private async void SubmitToken_Click(object sender, RoutedEventArgs e)
        {
            await SubmitToken();
        }

        private async Task SubmitToken()
        {
            // Hardcoded for testing replace
            //EnableButtons();
            //tokenInput.Visibility = Visibility.Collapsed;
            //buttonGrid.Visibility = Visibility.Visible;
            //return;


            string token = tokenInput.Text;
            if (token == string.Empty)
            {
                MessageBox.Show("Please enter a token.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (token.Length != 66)
            {
                MessageBox.Show("Please enter a token with valid format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                if (await _tokenValidation.IsTokenValidForAction(token, 3))
                {
                    EnableButtons();

                    tokenInput.Visibility = Visibility.Collapsed;
                    buttonGrid.Visibility = Visibility.Visible;

                    Token = token;
                }
                else
                {
                    MessageBox.Show("Invalid token. Please try again.");
                }
            }
            catch (Exception e) 
            {
                MessageBox.Show("Internal error: " + e.ToString());
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
            this.IsEnabled = false;
            ScrapeAviser scrapeWindow = new ScrapeAviser(Token, APIUrl);
            scrapeWindow.Show();
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