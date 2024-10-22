using ScraperLibrary._365_Discount;
using ScraperLibrary.Rema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestNUnit.ScraperTests
{
    public class Scraper365Test
    {
        private _365AvisScraper _365AvisScraper;
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _365AvisScraper = new _365AvisScraper();
        }

        [Test]
        public async Task FetchAvisIdTest()
        {
            string externalAvisId = await _365AvisScraper.FindAvisUrl("https://365discount.coop.dk/365avis/");

            TestContext.WriteLine($"External Avis ID: {externalAvisId}");

            Assert.That(externalAvisId.Length == 8);
            Assert.That(externalAvisId != null || externalAvisId != string.Empty);
        }

        [Test]
        public async Task GetAvisProductsTest()
        {
            var response = await _365AvisScraper.GetAvis(null, CancellationToken.None);
        }
    }
}