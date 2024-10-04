using DAL.Data.DAO;
using DAL.Data.Interfaces;
using ScraperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace TestNUnit.DALTests.Tests
{
    class AvisDAOTest
    {
        IAvisDAO _avisDAO = new AvisDAO(new ProductDAO());

        [Test]
        public async Task AddingAvisToDBTest()
        {
            int permissionLevel = 2;

            Avis avis = await new RemaAvisScraper().GetAvis();


            int insertedId = await _avisDAO.Add(avis, 1, 2);

            Assert.That(insertedId != 0);
            //Assert.That(await _avisDAO.get)

        }
    }
}
