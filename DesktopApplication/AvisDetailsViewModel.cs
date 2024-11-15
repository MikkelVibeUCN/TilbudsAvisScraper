using APIIntegrationLibrary.DTO;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TilbudsAvisLibrary.Entities;

namespace DesktopApplication
{
    public class AvisDetailsViewModel : INotifyPropertyChanged
    {
        private AvisDTO _selectedAvis;
        private ProductDTO _selectedProduct;
        private string _searchTerm;
        private ObservableCollection<ProductDTO> _filteredProducts;
        public int CompanyId;
        public AvisDTO SelectedAvis
        {
            get { return _selectedAvis; }
            set
            {
                _selectedAvis = value;
                OnPropertyChanged(nameof(SelectedAvis));
                FilteredProducts = new ObservableCollection<ProductDTO>(_selectedAvis.Products); // Initialize filtered products
            }
        }

        public ProductDTO SelectedProduct
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

        public ObservableCollection<ProductDTO> FilteredProducts
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

        public AvisDetailsViewModel(AvisDTO avisDTO, int companyId)
        {
            CompanyId = companyId;
            SelectedAvis = avisDTO;
            SelectedProduct = SelectedAvis.Products[0]; // Default selected product
            FilteredProducts = new ObservableCollection<ProductDTO>(SelectedAvis.Products); // Initialize filtered products
        }

        private void FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                // If the search term is empty, show all products
                FilteredProducts = new ObservableCollection<ProductDTO>(SelectedAvis.Products);
            }
            else
            {
                // Filter products based on the search term
                var filtered = SelectedAvis.Products
                    .Where(p => p.Name.IndexOf(SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                FilteredProducts = new ObservableCollection<ProductDTO>(filtered);
            }
        }
    }
}
