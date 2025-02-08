using TilbudsAvisLibrary.DTO;
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
            SelectedProduct = SelectedAvis.Products[0];
            FilteredProducts = new ObservableCollection<ProductDTO>(SelectedAvis.Products);
        }

        private void FilterProducts()
        {
            if (FilteredProducts == null)
                FilteredProducts = new ObservableCollection<ProductDTO>();

            FilteredProducts.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchTerm)
                ? SelectedAvis.Products
                : SelectedAvis.Products
                    .Where(p => p.Name.IndexOf(SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var product in filtered)
                FilteredProducts.Add(product);
        }
    }
}