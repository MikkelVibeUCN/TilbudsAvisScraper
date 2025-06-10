namespace TilbudsAvisLibrary
{
    public class ProductQueryParameters
    {
        public string? SortBy { get; set; }
        public string? Retailer { get; set; }
        public string? SearchTerm { get; set; } = ""; 
        public int? PageNumber { get; set; } = 0;
        public int? PageSize { get; set; } = 132;
        public int? Threshold { get; set; } = 200;
    }
}
