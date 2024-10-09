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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            double leftMarginPercent = 0.1;   // 10% left margin
            double topMarginPercent = 0.1;    // 10% top margin
            double rightMarginPercent = 0.1;  // 10% right margin
            double bottomMarginPercent = 0.1; // 10% bottom margin

            // Calculate the actual margins based on the window size
            double leftMargin = this.ActualWidth * leftMarginPercent;
            double topMargin = this.ActualHeight * topMarginPercent;
            double rightMargin = this.ActualWidth * rightMarginPercent;
            double bottomMargin = this.ActualHeight * bottomMarginPercent;

            // Apply margins to the grid
            OptionsGrid.Margin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        }
    }
}