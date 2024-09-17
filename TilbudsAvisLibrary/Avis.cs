namespace TilbudsAvisLibrary
{
    public class Avis
    {
        public List<Page> Pages { get; set; } = new List<Page>();
        public string CompanyName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Avis(string companyName, DateTime? startDate, DateTime? endDate)
        {
            CompanyName = companyName;
            StartDate = startDate;
            EndDate = endDate;
        }

        public void AddPage(Page page)
        {
            Pages.Add(page);
        }
        public void RemovePage(Page page)
        {
            Pages.Remove(page);
        }
    }
}