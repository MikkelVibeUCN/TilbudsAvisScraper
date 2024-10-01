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
            string currentDirectory = Directory.GetCurrentDirectory();
            string filePath = Path.Combine(currentDirectory, "..", "databaselogin.txt");
            ConnectionString = File.ReadAllText(filePath);
        }
    }
}
