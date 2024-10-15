using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DesktopApplication
{
    public partial class ScrapeAviser : Window
    {
        private string Token;
        public ObservableCollection<GrocerProgress> GrocerProgressList { get; set; }
        public Queue<GrocerProgress> GrocerQueue { get; set; }
        private bool QueueIsProcessing = false;
        // List of all possible grocers
        private readonly string[] allGrocers = { "Rema", "365 Discount"};

        private void PopulateComboBox()
        {
            foreach (var grocer in allGrocers)
            {
                GrocerComboBox.Items.Add(grocer);
            }
        }

        public ScrapeAviser(string token)
        {
            InitializeComponent();

            GrocerQueue = new Queue<GrocerProgress>();

            // Initialize the ObservableCollection for progress tracking
            GrocerProgressList = new ObservableCollection<GrocerProgress>();

            // Bind the DataGrid to the progress list
            ProgressGrid.ItemsSource = GrocerProgressList;

            PopulateComboBox();

            Token = token;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartGrocer();
        }

        private void StartAllButton_Click(object sender, RoutedEventArgs e)
        {
            StartAllGrocers();
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            ViewAvis(sender);
        }

        private void ViewAvis(object sender)
        {
            var button = sender as Button;

            var grocerProgress = button?.CommandParameter as GrocerProgress;
            if (grocerProgress != null)
            {
                this.IsEnabled = false;
                AvisDetailsWindow avisDetailsWindow = new(grocerProgress.avis, Token);
                avisDetailsWindow.Show();
            }
        }

        private void StartGrocer()
        {
            if (GrocerComboBox.SelectedItem != null)
            {
                var selectedGrocer = GrocerComboBox.SelectedItem.ToString();
                AddGrocerToQueue(selectedGrocer);
            }

        }

        private void AddGrocerToQueue(string selectedGrocer)
        {
            if (selectedGrocer != string.Empty)
            {
                var grocerInList = GrocerProgressList.FirstOrDefault(g => g.GrocerName == selectedGrocer);

                if (grocerInList == null)
                {
                    var newGrocerProgress = new GrocerProgress { GrocerName = selectedGrocer, Progress = "0%", IsProcessing = false, CancellationToken = new CancellationTokenSource()};

                    switch (newGrocerProgress.GrocerName)
                    {
                        case "Rema":
                            newGrocerProgress.ProcessMethod = (progressCallback) => new GrocerOperations().ScrapeRemaAvis(progressCallback, newGrocerProgress.CancellationToken.Token);
                            break;
                        case "365 Discount":
                            newGrocerProgress.ProcessMethod = (progressCallback) => new GrocerOperations().Scrape365Avis(progressCallback);
                            break;
                        default:
                            break;
                    }

                    GrocerProgressList.Add(newGrocerProgress);
                    GrocerQueue.Enqueue(newGrocerProgress);

                    if(!QueueIsProcessing)
                    {
                        ProcessQueue();
                    }
                }
            }
        }

        private async Task ProcessNextGrocer()
        {
            GrocerProgress grocerProgress = GrocerQueue.Dequeue();
            grocerProgress.avis = await grocerProgress.Process(); 
        }

        private async void ProcessQueue()
        {
            QueueIsProcessing = true;
            while (GrocerQueue.Count > 0)
            {
                await ProcessNextGrocer();
            }
            QueueIsProcessing = false;
        }
        private void StartAllGrocers()
        {
            foreach (var grocer in allGrocers)
            {
                AddGrocerToQueue(grocer);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            var grocerProgress = button?.CommandParameter as GrocerProgress;

            if (grocerProgress != null)
            {
                grocerProgress.CancellationToken.Cancel();
                GrocerProgressList.Remove(grocerProgress);
            }
        }
    }
}
