using Newtonsoft.Json.Linq;
using TilbudsAvisLibrary.Entities;
namespace TestNUnit.TilbudsAvisLibraryTests
{
    public class APIUserTests
    {
        HashSet<string> uniqueTokens = new HashSet<string>();
        private const int amount = 100000;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            for (int i = 0; i < amount; i++)
            {
                string token = APIUser.GenerateUniqueToken();
                uniqueTokens.Add(token);
            }
        }

        [Test]
        public void OnlyUniqueTokensAreMadeTest()
        {
            Assert.That(uniqueTokens.Count == amount);

        }

        [Test]
        public void TokensCorrectLengthTest()
        {
            foreach (string token in uniqueTokens)
            {
                Assert.That(token.Length == 66);
            }
        }
    }
}