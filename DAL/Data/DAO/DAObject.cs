using DAL.Data.Batch;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;
using static DAL.Data.DAO.ProductDAO;

namespace DAL.Data.DAO
{
    public abstract class DAObject : BatchHandler
    {
        protected string ConnectionString;

        public DAObject()
        {
            try
            {
                string filePath = "/src/databaselogin.txt";
                ConnectionString = File.ReadAllText(filePath);
            }
            catch
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                string filePath = Path.Combine(currentDirectory, "..", "databaselogin.txt");
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string filePathWindows = Path.Combine(userProfile, "source", "repos", "TilbudsAvisScraper", "databaselogin.txt");
                ConnectionString = File.ReadAllText(filePathWindows);
            }
        }

        protected string GetAllFromTableQueryOnCondition(string tableName, string identifer)
        {
            return $"SELECT * FROM {tableName} where {identifer} = @{identifer};";
        }

        protected string DeleteFromTableQueryWhereCondition(string tableName, string identifier)
        {
            return $"DELETE FROM {tableName} WHERE {identifier} = @{identifier};";
        }
    }
}