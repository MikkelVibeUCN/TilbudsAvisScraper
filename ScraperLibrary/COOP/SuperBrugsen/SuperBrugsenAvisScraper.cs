using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.COOP.SuperBrugsen
{
    public class SuperBrugsenAvisScraper : COOPAvisScraper, IAvisScraper
    {
        public SuperBrugsenAvisScraper() : base("https://superbrugsen.coop.dk/avis/", new SuperBrugsenProductScraper())
        {
            
        }
    }
}
