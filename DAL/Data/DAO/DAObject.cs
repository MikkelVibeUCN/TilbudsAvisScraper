using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Data.DAO
{
    public abstract class DAObject
    {
        protected string ConnectionString;

        public DAObject()
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string filePath = Path.Combine(userProfile, "source", "repos", "TilbudsAvisScraper", "databaselogin.txt");

            ConnectionString = File.ReadAllText(filePath);
        }
        protected string GetAllFromTableQuery(string tableName, string identifer)
        {
            return $"SELECT * FROM {tableName} where {identifer} = @{identifer};";
        }
    }
}
