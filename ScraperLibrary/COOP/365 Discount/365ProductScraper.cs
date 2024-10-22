using ScraperLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace ScraperLibrary.COOP._365_Discount
{
    public class _365ProductScraper : COOPProductScraper
    {
        public _365ProductScraper() : base("https://365discount.coop.dk/365avis/")
        {
            
        }

    }
}
