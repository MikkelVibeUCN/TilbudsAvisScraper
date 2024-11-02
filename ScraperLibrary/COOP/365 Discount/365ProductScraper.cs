using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperLibrary.COOP._365_Discount
{
    public class _365ProductScraper : COOPProductScraper
    {

        public _365ProductScraper() : base("https://365discount.coop.dk/365avis/")
        {
        }
    }
}
