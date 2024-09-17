namespace TilbudsAvisLibrary
{
    public class Avis
    {
        public List<Page> Pages { get; set; } = new List<Page>();
        public string CompanyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}