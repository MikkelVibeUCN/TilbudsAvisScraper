using TilbudsAvisLibrary.DTO;
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
    private bool _isViewEnabled; // New property for the View button
    public CancellationTokenSource CancellationToken { get; set; }

    public string GrocerName { get; set; }
    public int CompanyId { get; set; }

    public GrocerProgress(int companyId)
    {
        CompanyId = companyId;
    }

    public AvisDTO avis { get; set; }
    public string Progress
    {
        get => _progress;
        set
        {
            if (_progress != value)
            {
                _progress = value;
                OnPropertyChanged(nameof(Progress));
                // Update IsViewEnabled based on the Progress value
                IsViewEnabled = _progress == "100%"; // Only enable if progress is 100%
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

    public bool IsViewEnabled // New property to control the View button state
    {
        get => _isViewEnabled;
        set
        {
            if (_isViewEnabled != value)
            {
                _isViewEnabled = value;
                OnPropertyChanged(nameof(IsViewEnabled));
            }
        }
    }

    public ProcessingMethod ProcessMethod { get; set; } // Delegate for the processing method

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public async Task<AvisDTO> Process()
    {
        if (ProcessMethod != null)
        {
            IsProcessing = true;
            AvisDTO avis = await ProcessMethod(progress =>
            {
                // Update the Progress property based on the current progress value
                Progress = $"{progress}%"; // Update progress as a string
            });
            IsProcessing = false;
            return avis;
            // No need to manually set IsViewEnabled here since it's already handled in the Progress property setter
        }
        return null;
    }

    public delegate Task<AvisDTO> ProcessingMethod(Action<int> progressCallback);
}
