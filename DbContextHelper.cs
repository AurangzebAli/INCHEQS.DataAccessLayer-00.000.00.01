using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Web.Configuration;
using INCHEQS.Common;
//using INCHEQS.Areas.ICS.Models.SystemProfile;

namespace INCHEQS.DataAccessLayer {
    public abstract class DbContextHelper : DbContext {
        //private readonly ISystemProfileDao systemProfileDao;
        public DbContextHelper(string connectionString) : base(connectionString) {
        }


        public object ExecuteScalar( CommandType cmdType, string cmdText, SqlParameter[] cmdParms) {
            object val = null;
            string errorMsg = "";
            try
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = Database.Connection.ConnectionString;//ConfigureSetting.GetConnectionString();//  Configuration.ConnectionStrings["default"].ConnectionString;
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    PrepareCommand(cmd, null, CommandType.Text, cmdText, cmdParms, ref errorMsg);
                    if (errorMsg != "")
                    {
                        //systemProfileDao.Log("Error Exception DbContextHelper ExecuteNonQuery: " + errorMsg);
                    }
                    else
                    {
                        val = cmd.ExecuteScalar();
                    }
                    cmd.Parameters.Clear();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
               // systemProfileDao.Log("Error Exception DbContextHelper ExecuteReader: " + ex.Message);
            }

            return val;
           
        }
        

        public DataTable GetRecordsAsDataTable(string sql, SqlParameter[] cmdParms = null) {
            DataTable dtResult = new DataTable();
           
            try
            {
            string errorMsg = "";

                
                   using (SqlConnection conn = new SqlConnection())
                   {
                        conn.ConnectionString = Database.Connection.ConnectionString; ;
                        //ConfigureSetting.GetConnectionString(); // ConfigurationManager.ConnectionStrings["default"].ConnectionString;
                        conn.Open();
                    
                         SqlCommand cmd = conn.CreateCommand();
                         PrepareCommand(cmd, null, CommandType.Text, sql, cmdParms, ref errorMsg);
                        if (errorMsg != "")
                        {
                           // systemProfileDao.Log("Error Exception DbContextHelper ExecuteReader: " + errorMsg);
                        }

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.SelectCommand.CommandTimeout = 300; //manually set sql execution timeout to 5 minutes
                            adapter.Fill(dtResult);
                        }

                   
                }
               
            }
           catch (Exception ex)
            {
                throw ex;
           }
           return dtResult;
        }

        public DataTable GetRecordsAsDataTableSP(string storeProcedureName, SqlParameter[] cmdParms = null)
        {
            DataTable dtResult = new DataTable();

            try
            {
                string errorMsg = "";


                using (SqlConnection conn = new SqlConnection())
                {
                    SqlDataReader rdr = null;

                    conn.ConnectionString = Database.Connection.ConnectionString; 
                    //ConfigureSetting.GetConnectionString(); 
                    //ConfigurationManager.ConnectionStrings["default"].ConnectionString;
                    conn.Open();
                    //SqlCommand cmd = conn.CreateCommand();
                    // PrepareCommand(cmd, null, CommandType.Text, sql, cmdParms, ref errorMsg);
                    SqlCommand cmd = new SqlCommand(storeProcedureName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd .CommandTimeout = 300;
                    AttachParameters(cmd, cmdParms);
                    rdr = cmd.ExecuteReader();

                    if (errorMsg != "")
                    {
                     //   systemProfileDao.Log("Error Exception DbContextHelper ExecuteReader (SP): " + errorMsg);
                    }
                        

                    dtResult.Load(rdr);


                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dtResult;
        }


        
        public int ConstructAndExecuteUpdateCommandWithStringCondition(
            string tableName, Dictionary<string, dynamic> keyValueMap, string conditionParams) {
            List<SqlParameter> parameters = new List<SqlParameter>();
            string sql = string.Format("UPDATE {0} SET ", tableName);

            foreach (KeyValuePair<string, dynamic> pair in keyValueMap) {
                sql += string.Format(" {0} = @{0} ,", pair.Key);
                parameters.Add(new SqlParameter("@" + pair.Key, pair.Value));
            }

            sql = sql.TrimEnd(',');
            sql += " WHERE " + conditionParams;

            return ExecuteNonQuery(sql, parameters.ToArray());
        }

        public int ConstructAndExecuteUpdateCommand(
            string tableName, Dictionary<string, dynamic> keyValueMap, Dictionary<string, dynamic> conditionParams) {
            List<SqlParameter> parameters = new List<SqlParameter>();
            string sql = string.Format("UPDATE {0} SET ", tableName);

            foreach (KeyValuePair<string, dynamic> pair in keyValueMap) {
                sql += string.Format(" {0} = @{0} ,", pair.Key);
                parameters.Add(new SqlParameter("@" + pair.Key, pair.Value));
            }

            sql = sql.TrimEnd(',');

            string conditionSql = " WHERE ";

            foreach (KeyValuePair<string, dynamic> pair in conditionParams) {
                conditionSql += string.Format(" {0} = @{0} ,", pair.Key);
                parameters.Add(new SqlParameter("@" + pair.Key, pair.Value));
            }
            conditionSql = conditionSql.TrimEnd(',');

            if (conditionParams != null & conditionParams.Count > 0) {
                sql += conditionSql;
            }

            return ExecuteNonQuery(sql, parameters.ToArray());
        }


        public int ConstructAndExecuteInsertCommand(string tableName, Dictionary<string, dynamic> map) {
            List<SqlParameter> parameters = new List<SqlParameter>();
            string sql = "INSERT INTO " + tableName + " (";
            string columns = "";
            string values = "";
            foreach (KeyValuePair<string, dynamic> pair in map) {
                columns += pair.Key + ",";
                values += "@" + pair.Key + ",";
                parameters.Add(new SqlParameter("@" + pair.Key, pair.Value));
            }
            sql += columns.TrimEnd(',') + ") Values (" + values.TrimEnd(',') + ")";

            return ExecuteNonQuery(sql, parameters.ToArray());
        }


        //insert or update
        public int ExecuteNonQuery(string cmdText, SqlParameter[] cmdParms = null) {
            int val = 0;
            string errorMsg = "";
            try
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = Database.Connection.ConnectionString; 
                    //ConfigureSetting.GetConnectionString();  //ConfigurationManager.ConnectionStrings["default"].ConnectionString;
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    PrepareCommand(cmd, null, CommandType.Text, cmdText, cmdParms, ref errorMsg);
                    if (errorMsg != "")
                    {
                        //systemProfileDao.Log("Error Exception DbContextHelper ExecuteNonQuery: " + errorMsg);
                    }
                    else
                    {
                        val = cmd.ExecuteNonQuery();
                    }
                    cmd.Parameters.Clear();
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //systemProfileDao.Log("Error Exception DbContextHelper ExecuteReader: " + ex.Message);
            }
                
            return val;

        }

        //insert or update
        //Command Type for stored Procs
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, SqlParameter[] cmdParms) {
            int val = 0;
            string errorMsg = "";
            try
            {
                using (SqlConnection conn =new SqlConnection())
                {
                    conn.ConnectionString = Database.Connection.ConnectionString; 
                    // ConfigureSetting.GetConnectionString();// ConfigurationManager.ConnectionStrings["default"].ConnectionString;
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    PrepareCommand(cmd, null, cmdType, cmdText, cmdParms, ref errorMsg);
                    if (errorMsg != "")
                    {
                        //systemProfileDao.Log("Error Exception DbContextHelper ExecuteNonQuery: " + errorMsg);
                    }else
                    {
                        val = cmd.ExecuteNonQuery();
                    }
                    
                    cmd.Parameters.Clear();

               
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //systemProfileDao.Log("Error Exception DbContextHelper ExecuteReader: " + ex.Message); //write into SystemLog log files
            }
            return val;
        }

        //select
        //public DbDataReader ExecuteReader(CommandType cmdType, string cmdText, SqlParameter[] cmdParms) {

        //     DbDataReader rdr = null;
        //    //SqlDataReader rdr = null;
        //    string errorMsg = "";
           
        //    try 
        //    {
        //        using (SqlConnection conn = new SqlConnection())
        //        {
        //            conn.ConnectionString = ConfigurationManager.ConnectionStrings["default"].ConnectionString ;
        //            conn.Open();
        //            SqlCommand cmd = conn.CreateCommand();
        //            PrepareCommand(cmd, null, cmdType, cmdText, cmdParms, ref errorMsg);
        //            if(errorMsg != "")
        //            {
        //                systemProfileDao.Log("Error Exception DbContextHelper ExecuteReader: " + errorMsg);
        //            }
        //            rdr = cmd.ExecuteReader();
        //        }

                   
               
        //    }
        //    catch(Exception ex) {
        //        systemProfileDao.Log("Error Exception DbContextHelper ExecuteReader: " + ex.Message); //write into SystemLog log files
        //    }
        //    finally
        //    {
               
               
        //    }
        //        return rdr;
        //}



        private void PrepareCommand(SqlCommand cmd,  SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] commandParameters, ref string _ErrorMsg) {
         try
            {
                cmd.CommandText = cmdText;
                if (trans != null)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandType = cmdType;
                //attach the command parameters if they are provided
                if (commandParameters != null)
                {
                    AttachParameters(cmd, commandParameters);
                }

            }
            catch(Exception ex)
            {
                throw ex;
                _ErrorMsg = "[PrepareCommand] (" + cmdText + ")" +  ex.Message;
            }
           
              
            
           
            
        }

        private void AttachParameters(SqlCommand command, SqlParameter[] commandParameters) {
            //command.Parameters.Clear();
            foreach (SqlParameter p in commandParameters) {
                //check for derived output value with no value assigned
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null)) {
                    p.Value = DBNull.Value;
                }
                
                command.Parameters.Add(p);
            }
        }


        public bool CheckExist(string sql, SqlParameter[] cmdParms = null) {
           
            DataTable dtResult = new DataTable();
            bool blnResult;
            blnResult = false;
            try
            {
                string errorMsg = "";


                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = Database.Connection.ConnectionString; 
                    //ConfigureSetting.GetConnectionString(); //ConfigurationManager.ConnectionStrings["default"].ConnectionString;
                    conn.Open();
                    SqlCommand cmd = conn.CreateCommand();
                    PrepareCommand(cmd, null, CommandType.Text, sql, cmdParms, ref errorMsg);
                    if (errorMsg != "")
                    {
                       // systemProfileDao.Log("Error Exception DbContextHelper ExecuteReader: " + errorMsg);
                    }

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dtResult);
                    }


                }

                if(dtResult.Rows.Count > 0)
                {
                    blnResult = true;
                }
                

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return blnResult;
        }


        public DataTable GetDataTableFromSqlWithParameter(string sql, Dictionary<string, string> parameters = null) {
            DataTable resultTable = new DataTable();

            if (parameters == null) {
                parameters = new Dictionary<string, string>();
            }
            List<SqlParameter> sqlParams = new List<SqlParameter>();
            try {
                foreach (KeyValuePair<string, string> pair in parameters) {
                    if ((sql.IndexOf("@" + pair.Key) >= 0)) {
                        sqlParams.Add(new SqlParameter("@" + pair.Key, pair.Value));
                    }

                }
                resultTable = GetRecordsAsDataTable(sql, sqlParams.ToArray());

            } catch (Exception ex) {
                throw ex;
            }

            return resultTable;
        }

        public bool IsServerConnected() {
            using (SqlConnection conn = new SqlConnection()) {
                try {
                    conn.ConnectionString = Database.Connection.ConnectionString; 
                    if (conn.State != ConnectionState.Open) {
                        conn.Close();
                        conn.Open();
                    }
                    return true;
                } catch (SqlException) {
                    return false;
                }
            }
        }
    }
}