using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.COOP.Brugsen
{
    public class BrugsenProductScraper : COOPProductScraper
    {
        public BrugsenProductScraper() : base("https://brugsen.coop.dk/avis/")
        {
        }
    }
}
