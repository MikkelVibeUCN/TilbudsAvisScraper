using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TilbudsAvisLibrary.Entities;

namespace DesktopApplication
{
    public class AvisDetailsViewModel : INotifyPropertyChanged
    {
        private Avis _selectedAvis;
        private Product _selectedProduct;
        private string _searchTerm;
        private ObservableCollection<Product> _filteredProducts;

        public Avis SelectedAvis
        {
            get { return _selectedAvis; }
            set
            {
                _selectedAvis = value;
                OnPropertyChanged(nameof(SelectedAvis));
                FilteredProducts = new ObservableCollection<Product>(_selectedAvis.Products); // Initialize filtered products
            }
        }

        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm));
                FilterProducts(); // Filter products whenever the search term changes
            }
        }

        public ObservableCollection<Product> FilteredProducts
        {
            get { return _filteredProducts; }
            set
            {
                _filteredProducts = value;
                OnPropertyChanged(nameof(FilteredProducts));
            }
        }

        // INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AvisDetailsViewModel(Avis avis)
        {
            SelectedAvis = avis;
            SelectedProduct = SelectedAvis.Products[0]; // Default selected product
            FilteredProducts = new ObservableCollection<Product>(SelectedAvis.Products); // Initialize filtered products
        }

        private void FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                // If the search term is empty, show all products
                FilteredProducts = new ObservableCollection<Product>(SelectedAvis.Products);
            }
            else
            {
                // Filter products based on the search term
                var filtered = SelectedAvis.Products
                    .Where(p => p.Name.IndexOf(SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 p.Description.IndexOf(SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                FilteredProducts = new ObservableCollection<Product>(filtered);
            }
        }
    }
}
