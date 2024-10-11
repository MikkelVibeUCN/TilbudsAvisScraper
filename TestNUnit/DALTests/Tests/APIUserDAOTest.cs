using DAL.Data.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace TestNUnit.DALTests.Tests
{
    public class APIUserDAOTest
    {
        APIUserDAO apiUserDAO;
        APIUser apiUser = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            apiUserDAO = new APIUserDAO();
        }


        [Test]
        public async Task Test_AddAPIUser()
        {
            try
            {
                apiUser = new APIUser("TestUser", 3, null);
                int generatedId = await apiUserDAO.Add(apiUser);
                apiUser.SetId(generatedId);
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
           
            if(apiUser != null)
            {
                Assert.That(apiUser.Token != null);
            }
            else
            {
                Assert.Fail("APIUser not created");
            }
        }

        [Test]
        public async Task Test_GetAPIUserPermissionLevel()
        {
            try
            {
                apiUser = new APIUser("TestUser", 3, null);
                int generatedId = await apiUserDAO.Add(apiUser);
                int permissionLevel = await apiUserDAO.GetPermissionLevel(apiUser.Token);
                Assert.That(permissionLevel == 3);
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await apiUserDAO.DeleteAllWithSpecificRole("TestUser");
        }
    } 
}