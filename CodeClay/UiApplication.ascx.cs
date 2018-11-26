using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Xml.Serialization;
using System.Threading;

// Extra references
using CodistriCore;

namespace CodeClay
{
    public class CiApplication : CiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("ConnectionString")]
        public string ConnectionString { get; set; } = "";

        [XmlElement("ProviderName")]
        public string ProviderName { get; set; } = "";

        [XmlElement("LoginMacro")]
        public CiMacro LoginMacro { get; set; } = null;

        [XmlElement("LogoutMacro")]
        public CiMacro LogoutMacro { get; set; } = null;

        [XmlElement("HomePluginSrc")]
        public string HomePluginSrc { get; set; } = "";

        [XmlElement("AppName")]
        public string AppName { get; set; } = "";
    }

    public partial class UiApplication : UiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Member Variables
        // --------------------------------------------------------------------------------------------------

        private DbProviderFactory mDbProvider = null;
        private DbConnection mDbConnection = null;
        private Object mLock = new Object();

        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        static UiApplication()
        {
            // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
            // Blocks other threads from simulaneously accessing any other static members
        }

        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public string ProviderName
        {
            get
            {
                if (CiApplication != null)
                {
                    return CiApplication.ProviderName;
                }

                return "System.Data.OleDb";
            }
        }

        public DbProviderFactory DbProvider
        {
            get
            {
                if (mDbProvider == null)
                {
                    mDbProvider = DbProviderFactories.GetFactory(ProviderName);
                }

                return mDbProvider;
            }
        }

        public DbConnection DbConnection
        {
            get
            {
                if (mDbConnection == null)
                {
                    switch (ProviderName)
                    {
                        case "System.Data.SqlClient":
                            mDbConnection = new SqlConnection();
                            break;
                        case "System.Data.OleDb":
                            mDbConnection = new OleDbConnection();
                            break;
                        case "System.Data.Odbc":
                            mDbConnection = new OdbcConnection();
                            break;
                        default:
                            mDbConnection = DbProvider.CreateConnection();
                            break;
                    }
                }

                if (mDbConnection != null && mDbConnection.State != ConnectionState.Open)
                {
                    string connectionString = CiApplication.ConnectionString;

                    mDbConnection.ConnectionString = connectionString;

                    string databaseFolder = MyWebUtils.MapPath(MyWebUtils.ApplicationFolder);

                    if (ProviderName == "System.Data.OleDb" &&
                        !System.IO.Path.IsPathRooted(mDbConnection.DataSource) &&
                        !MyUtils.IsEmpty(databaseFolder))
                    {
                        string newConnectionString = "";

                        foreach (string connectionParameter in connectionString.Split(';'))
                        {
                            if (connectionParameter.StartsWith("Data Source="))
                            {
                                newConnectionString += "Data Source=" +
                                    databaseFolder + @"\" + mDbConnection.DataSource + ";";
                            }
                            else
                            {
                                newConnectionString += connectionParameter + ";";
                            }
                        }

                        mDbConnection.ConnectionString = newConnectionString;
                    }
                }

                return mDbConnection;
            }
        }

        public override object this[string key, int rowIndex = -1]
        {
            get
            {
                if (key.StartsWith("CI_"))
                {
                    return GetSystemVariable(key.Substring(1));
                }
                else if (dxClientState.Contains(key))
                {
                    return dxClientState[key];
                }

                return null;
            }

            set
            {
                if (!key.StartsWith("CI_"))
                {
                    dxClientState[key] = value;
                }
            }
        }

        public static UiApplication Me { get; set; } = null;

        public virtual CiApplication CiApplication
        {
            get { return CiPlugin as CiApplication; }
            set { CiPlugin = value; }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------------------------------------

        public virtual void RunLoginMacro()
        {
            CiMacro loginMacro = (CiApplication != null) ? CiApplication.LoginMacro : null;

            if (loginMacro != null)
            {
                loginMacro.RunSQL(null);
            }
        }

        public virtual void RunLogoutMacro()
        {
            CiMacro logoutMacro = (CiApplication != null) ? CiApplication.LogoutMacro : null;

            if (logoutMacro != null)
            {
                logoutMacro.RunSQL(null);
            }
        }

        public virtual object GetSystemVariable(string variableName)
        {
            object variableValue = null;

            switch (variableName)
            {
                case "CI_UserEmail":
                    if (Context.User != null)
                    {
                        variableValue = Context.User.Identity.GetUserName();
                    }
                    break;

                case "CI_Now":
                    variableValue = DateTime.Now;
                    break;

                case "CI_AppName":
                    variableValue = CiApplication.AppName;
                    break;
            }

            return variableValue;
        }

        public virtual DataRow GetSystemVariables()
        {
            DataTable dt = new DataTable();

            string[] columnNames = { "CI_UserEmail", "CI_Now", "CI_AppName" };
            foreach (string columnName in columnNames)
            {
                dt.Columns.Add(columnName).DefaultValue = GetSystemVariable(columnName);
            }

            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            return dr;
        }

        public virtual void ClearState(string tableName)
        {
            ArrayList keys = new ArrayList();
            foreach (var item in dxClientState)
            {
                string key = item.Key;
                if (key.StartsWith(tableName))
                {
                    keys.Add(key);
                }
            }

            foreach (string key in keys)
            {
                dxClientState.Remove(key);
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            dxMenuPanel.Visible = !MyWebUtils.IsPopup(this.Page);
        }

        // --------------------------------------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------------------------------------

        public string GetCommandFired(string tableName)
        {
            object command = null;

            if (dxClientCommand.Contains(tableName))
            {
                command = dxClientCommand[tableName];
            }

            return MyUtils.Coalesce(command, "").ToString();
        }

        public void SetCommandFired(string tableName, string command)
        {
            dxClientCommand[tableName] = command;
        }

        public DataTable GetBySQL(string sql, DataRow drParams)
        {
            return GetBySQL(new string[] { sql }, drParams);
        }

        public DataTable GetBySQL(string[] sqlList, DataRow drParams)
        {
            lock (mLock)
            {
                DataTable dt = new DataTable();

                int lastSelectSQLIndex = sqlList.Length;
                while (--lastSelectSQLIndex > -1)
                {
                    string strActionSQL = sqlList[lastSelectSQLIndex].ToString().Trim();
                    if (strActionSQL.ToUpper().StartsWith("SELECT") ||
                        strActionSQL.StartsWith("?") ||
                        strActionSQL.StartsWith("$"))
                    {
                        break;
                    }
                }

                ArrayList drInputList = new ArrayList();
                DataRow drInitial;
                if (drParams == null)
                {
                    drInitial = (new DataTable()).NewRow();
                }
                else
                {
                    drInitial = MyUtils.CloneDataRow(drParams);
                }

                drInputList.Add(drInitial);

                for (int i = 0; i < sqlList.Length; i++)
                {
                    string sql = sqlList[i].ToString().Trim();
                    bool isSelectSQL = sql.ToUpper().StartsWith("SELECT");
                    if (sql.StartsWith("?"))
                    {
                        // Stored procedure which fetches data will begin with '?'
                        isSelectSQL = true;
                        sql = sql.Substring(1);
                    }
                    else if (sql.StartsWith("$"))
                    {
                        // Shorthand for singleton select
                        isSelectSQL = true;
                        sql = string.Format("select {0} from tblSingleton", sql.Substring(1));
                    }

                    // Stored procedure can be specified at runtime with '@'
                    // e.g. @CardSproc
                    string sprocVariableName = "";
                    if (sql.StartsWith("@"))
                    {
                        sprocVariableName = sql.Substring(1).Split(WHITESPACE)[0];
                    }

                    ArrayList actionColumns = new ArrayList();
                    if (MyUtils.IsEmpty(sprocVariableName))
                    {
                        actionColumns = MyUtils.GetParameters(sql);
                    }

                    ArrayList drResultList = new ArrayList();
                    int rowsFetched = 0;
                    int lastRowAdded = 0;

                    foreach (DataRow drInput in drInputList.ToArray(typeof(DataRow)))
                    {
                        // Replace stored procedure name if necessary
                        if (!MyUtils.IsEmpty(sprocVariableName))
                        {
                            sql = drInput[sprocVariableName] + sql.Substring(sprocVariableName.Length + 1);
                            actionColumns = MyUtils.GetParameters(sql);
                        }

                        ArrayList args = MyUtils.Project(MyWebUtils.AppendColumns(drInput, GetSystemVariables()), actionColumns);
                        ArrayList parameterNameList = MyUtils.GetParameters(sql);
                        DbCommand dbCommand = GetCommand(sql, parameterNameList, args);

                        if (isSelectSQL)
                        {
                            DataTable dtResultTable = null;

                            if (i == lastSelectSQLIndex)
                            {
                                if (dt != null)
                                {
                                    lastRowAdded = rowsFetched;
                                    dtResultTable = dt;
                                }
                                else
                                {
                                    lastRowAdded = 0;
                                    dtResultTable = new DataTable();
                                }

                                rowsFetched += Fill(dbCommand, dt);
                            }
                            else
                            {
                                lastRowAdded = 0;
                                dtResultTable = (new DataSet()).Tables.Add();
                                Fill(dbCommand, dtResultTable);
                            }

                            if (i < sqlList.Length - 1)
                            {
                                for (int rowIndex = lastRowAdded; rowIndex < MyWebUtils.GetNumberOfRows(dtResultTable); rowIndex++)
                                {
                                    drResultList.Add(dtResultTable.Rows[rowIndex]);
                                }
                            }
                        }
                        else
                        {
                            Fill(dbCommand, null);
                        }
                    }

                    if (isSelectSQL && i < sqlList.Length - 1)
                    {
                        drInputList.Clear();
                        foreach (DataRow drResult in drResultList.ToArray(typeof(DataRow)))
                        {
                            DataRow drInput = MyUtils.AppendColumns(drResult, drInitial);
                            drInputList.Add(drInput);
                        }
                    }
                }

                return dt;
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private void OpenConnection()
        {
            if (DbConnection != null)
            {
                // Try opening connection for up to 1 minute
                for (int i = 0; i < 60; i++ )
                {
                    if (DbConnection.State == ConnectionState.Closed)
                    {
                        DbConnection.Open();
                    }

                    if (DbConnection.State != ConnectionState.Open)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void CloseConnection()
        {
            if (DbConnection != null)
            {
                // Try closing connection for up to 1 minute
                for (int i = 0; i < 60; i++)
                {
                    if (DbConnection.State == ConnectionState.Open)
                    {
                        DbConnection.Close();
                    }

                    if (DbConnection.State != ConnectionState.Closed)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private DbCommand GetCommand(string SQL)
        {
            if (DbProvider != null && !MyUtils.IsEmpty(SQL))
            {
                DbCommand command = DbProvider.CreateCommand();
                if (command != null)
                {
                    command.Connection = DbConnection;

                    string[] SQLSplit = SQL.Trim().Split(new char[] { ' ', '\t', '\n', '\r' });

                    if (!SQLSplit[0].Trim().ToUpper().Equals("SELECT") &&
                       !SQLSplit[0].Trim().ToUpper().Equals("INSERT") &&
                       !SQLSplit[0].Trim().ToUpper().Equals("UPDATE") &&
                       !SQLSplit[0].Trim().ToUpper().Equals("DELETE") &&
                       !SQLSplit[0].Trim().ToUpper().Equals("EXEC"))
                    {
                        string commandText = SQL.Split(new char[] { '@' })[0].Trim();

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = commandText;
                    }
                    else
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = SQL;
                    }

                    return command;
                }
            }

            return null;
        }

        private DbCommand GetCommand(string SQL, ArrayList parameterNameList, ArrayList parameterValueList)
        {
            DbCommand command = GetCommand(SQL);
            if (command != null)
            {
                if (command.GetType() == typeof(SqlCommand) && command.CommandType == CommandType.StoredProcedure)
                {
                    this.AddParameters((SqlCommand)command, parameterValueList);
                }
                else
                {
                    this.AddParameters(command, parameterNameList, parameterValueList);
                }
            }

            return command;
        }

        private int Fill(DbCommand command, DataTable dt)
        {
            int rowsAdded = 0;

            if (dt != null)
            {
                if (DbProvider != null)
                {
                    DbDataAdapter dbAdapter = DbProvider.CreateDataAdapter();

                    if (dbAdapter != null)
                    {
                        dbAdapter.SelectCommand = command;

                        OpenConnection();
                        rowsAdded = dbAdapter.Fill(dt);
                        CloseConnection();
                    }
                }
            }
            else if (command != null)
            {
                OpenConnection();
                command.ExecuteNonQuery();
                CloseConnection();
            }

            return rowsAdded;
        }

        private void AddParameters(DbCommand command, ArrayList parameterNameList, ArrayList parameterValueList)
        {
            if (command != null && parameterNameList != null && parameterValueList != null)
            {
                for (int i = 0; i < parameterNameList.Count; i++)
                {
                    if (!command.Parameters.Contains("@" + parameterNameList[i].ToString()))
                    {
                        DbParameter parameter = command.CreateParameter();
                        parameter.ParameterName = "@" + parameterNameList[i].ToString();

                        if (MyUtils.IsEmpty(parameterValueList[i]))
                        {
                            parameter.Value = Convert.DBNull;
                        }
                        else
                        {
                            if (parameterValueList[i].GetType() == typeof(string) && string.IsNullOrEmpty((string)parameterValueList[i]))
                            {
                                parameter.Value = Convert.DBNull;
                            }
                            else
                            {
                                parameter.Value = parameterValueList[i];
                            }
                        }

                        command.Parameters.Add(parameter);
                    }
                }
            }
        }

        private void AddParameters(SqlCommand command, ArrayList parameterValueList)
        {
            if (command != null && parameterValueList != null)
            {
                OpenConnection();
                SqlCommandBuilder.DeriveParameters(command);

                int index = 0; ;
                foreach (SqlParameter parameter in command.Parameters)
                {
                    if (parameter.Direction == ParameterDirection.Input)
                    {
                        object parameterValue = parameterValueList[index++];
                        if (MyUtils.IsEmpty(parameterValue))
                        {
                            parameter.Value = Convert.DBNull;
                        }
                        else
                        {
                            if (parameterValue.GetType() == typeof(string) && string.IsNullOrEmpty((string)parameterValue))
                            {
                                parameter.Value = Convert.DBNull;
                            }
                            else
                            {
                                parameter.Value = parameterValue;
                            }
                        }
                    }
                }
            }
        }
    }
}