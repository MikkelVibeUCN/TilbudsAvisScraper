using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DesktopApplication
{
    public partial class ScrapeAviser : Window
    {
        public ObservableCollection<GrocerProgress> GrocerProgressList { get; set; }
        public Queue<GrocerProgress> GrocerQueue { get; set; }
        private bool QueueIsProcessing = false;
        // List of all possible grocers
        private readonly string[] allGrocers = { "Rema", "365 Discount", "Kvickly", "Netto" };

        public ScrapeAviser()
        {
            InitializeComponent();

            GrocerQueue = new Queue<GrocerProgress>();

            // Initialize the ObservableCollection for progress tracking
            GrocerProgressList = new ObservableCollection<GrocerProgress>();

            // Bind the DataGrid to the progress list
            ProgressGrid.ItemsSource = GrocerProgressList;

            PopulateComboBox();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartGrocer();
        }

        private void StartAllButton_Click(object sender, RoutedEventArgs e)
        {
            StartAllGrocers();
        }

        private void PopulateComboBox()
        {
            foreach (var grocer in allGrocers)
            {
                GrocerComboBox.Items.Add(grocer);
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
                    var newGrocerProgress = new GrocerProgress { GrocerName = selectedGrocer, Progress = "0%", IsProcessing = false};

                    switch (newGrocerProgress.GrocerName)
                    {
                        case "Rema":
                            newGrocerProgress.ProcessMethod = (progressCallback) => new GrocerOperations().ScrapeRemaAvis(progressCallback);
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
            await grocerProgress.Process(); 
            GrocerProgressList.Remove(grocerProgress);
        }

        private async Task ProcessQueue()
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

        // Event handler for Remove button
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the clicked button
            var button = sender as Button;

            // Get the GrocerProgress object from the button's DataContext
            var grocerProgress = button?.CommandParameter as GrocerProgress;

            if (grocerProgress != null)
            {
                // Remove the selected grocer progress from the collection
                GrocerProgressList.Remove(grocerProgress);
            }
        }
    }
}
