using System.Windows;
using TilbudsAvisLibrary.Entities;

namespace DesktopApplication
{
    public partial class AvisDetailsWindow : Window
    {
        public AvisDetailsWindow(Avis avis)
        {
            InitializeComponent();

            // Set the DataContext for data binding to the ViewModel
            this.DataContext = new AvisDetailsViewModel(avis);
        }
    }
}