﻿using DAL.Data.Batch;
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

        public DAObject(string connectionString)
        {
            ConnectionString = connectionString;
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