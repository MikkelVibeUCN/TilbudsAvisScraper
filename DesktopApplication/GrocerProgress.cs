using ScraperLibrary.Rema;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

public class GrocerProgress : INotifyPropertyChanged
{
    private string _progress;
    private bool _isProcessing;

    public string GrocerName { get; set; }

    public string Progress
    {
        get => _progress;
        set
        {
            if (_progress != value)
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            if (_isProcessing != value)
            {
                _isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }
    }

    public ProcessingMethod ProcessMethod { get; set; } // Delegate for the processing method

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task Process()
    {
        if (ProcessMethod != null)
        {
            IsProcessing = true;
            await ProcessMethod(progress =>
            {
                // Update the Progress property based on the current progress value
                Progress = $"{progress}%"; // Update progress as a string
            });
            IsProcessing = false;
        }
    }

    public delegate Task<Avis> ProcessingMethod(Action<int> progressCallback);

}