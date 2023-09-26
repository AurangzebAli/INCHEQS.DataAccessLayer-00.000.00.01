
using INCHEQS.DataAccessLayer;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

//Refer: http://www.abstractmethod.co.uk/blog/2016/3/aspnet-identity-unity-and-database-first-development/
//Refer As well : http://www.jerriepelser.com/blog/resolve-dbcontext-as-interface-in-aspnet5-ioc-container
namespace INCHEQS.DataAccessLayer {

    public class ApplicationDbContext : DbContextHelper {
        public ApplicationDbContext() : base("default") { }
        public static ApplicationDbContext Create() {
            return new ApplicationDbContext();
        }
    }

    public interface IApplicationDbContext {
        DataTable GetRecordsAsDataTable(string sql, DbParameter[] cmdParms = null);
        int ConstructAndExecuteUpdateCommand( string tableName, Dictionary<string, dynamic> keyValueMap, Dictionary<string, dynamic> conditionParams);
        int ConstructAndExecuteInsertCommand(string tableName, Dictionary<string, dynamic> map);
        int ExecuteNonQuery(string cmdText, SqlParameter[] cmdParms);
        int ExecuteNonQuery(CommandType cmdType, string cmdText, SqlParameter[] cmdParms);
        DataTable GetDataTableFromSqlWithParameter(string sql, Dictionary<string, string> parameters = null);
        DbDataReader ExecuteReader(CommandType cmdType, string cmdText, DbParameter[] cmdParms);
        bool CheckExist(string sql, SqlParameter[] cmdParms = null);
    }

}