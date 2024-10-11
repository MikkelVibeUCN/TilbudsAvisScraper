using System.Windows;
using APIIntegrationLibrary;
namespace DesktopApplication
{
    public partial class MainWindow : Window
    {
        private InfoWrapper _infoWrapper = InfoWrapper.GetInstance();
        private TokenValidation _tokenValidation;
        private int permissionLevel = 0;
        public MainWindow()
        {
            InitializeComponent();
            _tokenValidation = new TokenValidation();
        }


        private async void SubmitToken_Click(object sender, RoutedEventArgs e)
        {
            await SubmitToken();
        }

        private async Task SubmitToken()
        {
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
                if (await _tokenValidation.VerifyToken(token, 3))
                {
                    EnableButtons();

                    tokenInput.Visibility = Visibility.Collapsed;
                    buttonGrid.Visibility = Visibility.Visible;

                    _infoWrapper.Token = token;
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