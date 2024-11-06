
namespace TilbudsAvisLibrary.Entities
{
    public class Company
    {
        public string Name { get; set; }
        public List<Avis> Aviser { get; set; }

        public Company(string name, List<Avis> aviser)
        {
            Name = name;
            Aviser = aviser;
        }
        public Company(string name)
        {
            Name = name;
            Aviser = new List<Avis>();
        }
        public void AddAvis(Avis avis)
        {
            Aviser.Add(avis);
        }
    }
}
