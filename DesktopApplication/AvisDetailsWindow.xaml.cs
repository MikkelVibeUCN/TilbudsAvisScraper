using System.Windows;
using APIIntegrationLibrary;
using APIIntegrationLibrary.DTO;
using APIIntegrationLibrary.Interfaces;

namespace DesktopApplication
{
    public partial class AvisDetailsWindow : Window
    {
        private readonly IAvisAPIRestClient _avisAPIRestClient;
        private string Token;
        public AvisDetailsWindow(AvisDTO avisDTO, string token, int companyId, IAvisAPIRestClient avisAPIRestClient)
        {
            InitializeComponent();

            Token = token;

            _avisAPIRestClient = avisAPIRestClient;
            // Set the DataContext for data binding to the ViewModel
            this.DataContext = new AvisDetailsViewModel(avisDTO, companyId);
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (AvisDetailsViewModel)DataContext;

            // Access the SelectedAvis property and submit it
            if (viewModel.SelectedAvis != null)
            {
                try
                {
                    await _avisAPIRestClient.CreateAsync(viewModel.SelectedAvis, viewModel.CompanyId, Token);
                    MessageBox.Show("Avis saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) 
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No Avis selected to save.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}