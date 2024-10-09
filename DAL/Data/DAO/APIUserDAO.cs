﻿using DAL.Data.Exceptions;
using DAL.Data.Interfaces;
using Emgu.CV.Features2D;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilbudsAvisLibrary.Entities;

namespace DAL.Data.DAO
{
    public class APIUserDAO : DAObject, IAPIUserDAO
    {
        private const string APIUserQuery = @"
            INSERT INTO APIUser (Token, Role, PermissionLevel) 
            VALUES (@Token, @Role, @PermissionLevel); 
            SELECT SCOPE_IDENTITY();";

        private const string APIUserPermissionQuery = @"
            SELECT PermissionLevel FROM APIUser WHERE Token = @Token";

        public async Task<int> Add(APIUser user, int permissionLevel)
        {
            if (permissionLevel >= 3)
            {
                throw new InsufficientTokenPermission("Access denied");
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand command = new SqlCommand(APIUserQuery, connection, transaction))
                        {
                            // Assigning parameters to the SQL command
                            command.Parameters.AddWithValue("@Token", user.Token);
                            command.Parameters.AddWithValue("@Role", user.Role);
                            command.Parameters.AddWithValue("@PermissionLevel", user.PermissionLevel);

                            // Execute the query and get the generated ID
                            int generatedId = Convert.ToInt32(await command.ExecuteScalarAsync());

                            user.SetId(generatedId);

                            transaction.Commit();

                            return generatedId;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
            throw new NotImplementedException();
        }

        public Task Delete(int id, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public Task<APIUser?> Get(int id, int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public Task<List<APIUser>> GetAll(int permissionLevel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(APIUser t, int permissionLevel)
        {
            throw new NotImplementedException();
        }


        // 1 permission to get, 2 permission to delete and add, 3 permission to create new apiusers/tokens
        public async Task<int> GetPermissionLevel(string token)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                try
                {
                    using (SqlCommand command = new SqlCommand(APIUserPermissionQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Token", token);

                        SqlDataReader reader = await command.ExecuteReaderAsync();

                        int permissionLevel = reader.GetInt32(reader.GetOrdinal("PermissionLevel"));

                        return permissionLevel;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}