using PuppeteerSharp;
using ScraperLibrary.COOP;
using ScraperLibrary.COOP._365_Discount;
using ScraperLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;
using TilbudsAvisLibrary.Exceptions;

namespace ScraperLibrary._365_Discount
{
    public class _365AvisScraper : COOPAvisScraper
    {
        public _365AvisScraper() : base("https://365discount.coop.dk/365avis/", new _365ProductScraper())
        {
        }
    }
}
