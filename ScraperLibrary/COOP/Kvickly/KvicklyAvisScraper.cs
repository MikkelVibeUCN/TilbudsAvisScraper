using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.COOP.Kvickly
{
    public class KvicklyAvisScraper : COOPAvisScraper, IAvisScraper
    {
        public KvicklyAvisScraper() : base("https://kvickly.coop.dk/avis/", new KvicklyProductScraper())
        {
            
        }
    }
}
