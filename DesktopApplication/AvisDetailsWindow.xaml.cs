using System.Windows;
using TilbudsAvisLibrary.Entities;
using APIIntegrationLibrary;

namespace DesktopApplication
{
    public partial class AvisDetailsWindow : Window
    {
        private AvisHandling avisHandling = new();
        private string Token;
        public AvisDetailsWindow(Avis avis, string token, int companyId)
        {
            InitializeComponent();

            Token = token;

            // Set the DataContext for data binding to the ViewModel
            this.DataContext = new AvisDetailsViewModel(avis, companyId);
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (AvisDetailsViewModel)DataContext;

            // Access the SelectedAvis property and submit it
            if (viewModel.SelectedAvis != null)
            {
                try
                {
                    await avisHandling.SubmitAvis(viewModel.SelectedAvis, viewModel.CompanyId, Token);
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