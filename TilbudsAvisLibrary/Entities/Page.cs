namespace TilbudsAvisLibrary.Entities
{
    public class Page
    {
        public int PageNumber { get; set; }
        public string ImageUrl { get; set; }

        public int Id { get; private set; }

        public Page(string imageUrl, int pageNumber)
        {
            ImageUrl = imageUrl;
            PageNumber = pageNumber;
        }

        public void SetId(int id) => Id = id;

    }
}
