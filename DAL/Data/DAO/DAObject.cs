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
        protected string ConnectionString = "Server=localhost;Database=TilbudsAvisScraperDB;Trusted_Connection=True;";
    }
}
