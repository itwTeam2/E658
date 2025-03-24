using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ReportData.DAL
{
    public class DALCommanQuery:DALBase
    {


        public DataTable CallE65SP(int E658CreatorID)
        {
            ///Created BY   : Sqn ldr Wickramasinghe
            ///Created Date : 2024/03/26
            /// Description : Call CivilFlowStatus Sp , when seraching the details

            DataTable dt = new DataTable();
            try
            {
                SqlConnection Connection = DALConnectionManager.open();
                SqlCommand command = Connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "GetE658Detils";
                command.Parameters.AddWithValue("@E658CreatorID", E658CreatorID);
                command.CommandTimeout = 1000;
                SqlDataAdapter adp = new SqlDataAdapter(command);

                adp.Fill(dt);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable CallE65MoreDetailsSP(int RoleID)
        {
            ///Created BY   : Sqn ldr Wickramasinghe
            ///Created Date : 2024/03/29
            /// Description : Call CivilFlowStatus Sp , when seraching the details

            DataTable dt = new DataTable();
            try
            {
                SqlConnection Connection = DALConnectionManager.open();
                SqlCommand command = Connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "E658MoreDetails";
                command.Parameters.AddWithValue("@RoleID", RoleID);
                command.CommandTimeout = 1000;
                SqlDataAdapter adp = new SqlDataAdapter(command);

                adp.Fill(dt);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable CalleTranReqDetailsSP(int E658CreatorID)
        {
            ///Created BY   : Sqn ldr Wickramasinghe
            ///Created Date : 2025/02/27
            /// Description : Call the GetTransPortAuthDetails SP

            DataTable dt = new DataTable();
            try
            {
                SqlConnection Connection = DALConnectionManager.open();
                SqlCommand command = Connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "GetTransPortAuthDetails";
                command.Parameters.AddWithValue("@E658CreatorID", E658CreatorID);
                command.CommandTimeout = 1000;
                SqlDataAdapter adp = new SqlDataAdapter(command);

                adp.Fill(dt);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }

        }


        public DataTable CallTrnsAuthIndexSP(int RoleID)
        {
            ///Created BY   : Sqn ldr Wickramasinghe
            ///Created Date : 2024/03/29
            /// Description : Call CivilFlowStatus Sp , when seraching the details

            DataTable dt = new DataTable();
            try
            {
                SqlConnection Connection = DALConnectionManager.open();
                SqlCommand command = Connection.CreateCommand();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "ETransIndexList";
                command.Parameters.AddWithValue("@RoleID", RoleID);
                command.CommandTimeout = 1000;
                SqlDataAdapter adp = new SqlDataAdapter(command);

                adp.Fill(dt);

                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}