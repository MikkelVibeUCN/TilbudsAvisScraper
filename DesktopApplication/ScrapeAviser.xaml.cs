using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TilbudsAvisLibrary.Exceptions;

namespace DesktopApplication
{
    public partial class ScrapeAviser : Window
    {
        private string Token;
        public ObservableCollection<GrocerProgress> GrocerProgressList { get; set; }
        public Queue<GrocerProgress> GrocerQueue { get; set; }
        private bool QueueIsProcessing = false;
        // List of all possible grocers
        private readonly Dictionary<string, int> allGrocers = new Dictionary<string, int>
        {
            { "Rema", 1 },
            { "365 Discount", 2 },
            { "Kvickly", 3 },
            {"SuperBrugsen", 4 },
            {"Brugsen", 5 },
        };

        private void PopulateComboBox()
        {
            foreach (var grocer in allGrocers)
            {
                GrocerComboBox.Items.Add(grocer.Key);
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
                AvisDetailsWindow avisDetailsWindow = new(grocerProgress.avis, Token, grocerProgress.CompanyId);
                avisDetailsWindow.Show();
            }
        }

        private void StartGrocer()
        {
            if (GrocerComboBox.SelectedItem != null)
            {
                string selectedGrocer = GrocerComboBox.SelectedItem.ToString();

                var grocer = new KeyValuePair<string, int>(selectedGrocer, allGrocers[selectedGrocer]);

                AddGrocerToQueue(grocer);
            }
        }

        private void AddGrocerToQueue(KeyValuePair<string, int> selectedGrocer)
        {
            if (selectedGrocer.Key != string.Empty)
            {
                var grocerInList = GrocerProgressList.FirstOrDefault(g => g.GrocerName == selectedGrocer.Key);

                if (grocerInList == null)
                {
                    var newGrocerProgress = new GrocerProgress(selectedGrocer.Value) { GrocerName = selectedGrocer.Key, Progress = "0%", IsProcessing = false, CancellationToken = new CancellationTokenSource()};

                    switch (newGrocerProgress.GrocerName)
                    {
                        case "Rema":
                            newGrocerProgress.ProcessMethod = (progressCallback) => new GrocerOperations().ScrapeRemaAvis(progressCallback, newGrocerProgress.CancellationToken.Token, selectedGrocer.Value);
                            break;
                        case "365 Discount":
                            newGrocerProgress.ProcessMethod = (progressCallback) => new GrocerOperations().Scrape365Avis(progressCallback, newGrocerProgress.CancellationToken.Token, selectedGrocer.Value);
                            break;
                        case "Kvickly":
                            newGrocerProgress.ProcessMethod = (progressCallback) => new GrocerOperations().ScrapeKvicklyAvis(progressCallback, newGrocerProgress.CancellationToken.Token, selectedGrocer.Value);
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
            try
            {
                grocerProgress.avis = await grocerProgress.Process();
            }
            catch (CannotReachWebsiteException e)
            {
                CancelProgress(grocerProgress);
                MessageBox.Show(e.Message);
            }
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
                CancelProgress(grocerProgress);
            }
        }

        private void CancelProgress(GrocerProgress grocerProgress)
        {
            grocerProgress.CancellationToken.Cancel();
            GrocerProgressList.Remove(grocerProgress);
        }

    }
}
