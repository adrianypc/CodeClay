using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Xml;
using System.Xml.Linq;
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

        [XmlElement("CPanelConnectionString")]
        public string CPanelConnectionString { get; set; } = "";

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
        private readonly Object mDbLock = new Object();

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

        public string[] ClientKeys
        {
            get
            {
                ArrayList keys = new ArrayList();
                foreach (var item in dxClientState)
                {
                    keys.Add(item);
                }

                return keys.ToArray(typeof(string)) as string[];
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
                loginMacro.Run(null);
            }
        }

        public virtual void RunLogoutMacro()
        {
            CiMacro logoutMacro = (CiApplication != null) ? CiApplication.LogoutMacro : null;

            if (logoutMacro != null)
            {
                logoutMacro.Run(null);
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

                case "CI_SiteFolder":
                    variableValue = Server.MapPath("Sites");
                    break;
            }

            return variableValue;
        }

        public virtual DataRow GetSystemVariables()
        {
            DataTable dt = new DataTable();

            string[] columnNames = { "CI_UserEmail", "CI_Now", "CI_AppName", "CI_SiteFolder" };
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

        public DataTable GetBySQL(string sql, DataRow drParams, bool isCPanel = false)
        {
            return GetBySQL(new string[] { sql }, drParams, isCPanel);
        }

        public DataTable GetBySQL(string[] sqlList, DataRow drParams, bool isCPanel = false)
        {
            DataTable dt = (new DataSet()).Tables.Add();

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
                                dtResultTable = (new DataSet()).Tables.Add();
                            }

                            rowsFetched += Fill(sql, drInput, dt, isCPanel);
                        }
                        else
                        {
                            lastRowAdded = 0;
                            dtResultTable = (new DataSet()).Tables.Add();
                            Fill(sql, drInput, dtResultTable, isCPanel);
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
                        dt = (new DataSet()).Tables.Add();
                        Fill(sql, drInput, null, isCPanel);
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

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private int Fill(string SQL, DataRow drParams, DataTable dt, bool isCPanel = false)
        {
            lock (mDbLock)
            {
                int rowsAdded = 0;
                DbCommand command = CreateCommand(SQL, drParams, isCPanel);

                if (command != null)
                {
                    if (dt != null)
                    {
                        if (DbProvider != null)
                        {
                            DbDataAdapter dbAdapter = DbProvider.CreateDataAdapter();

                            if (dbAdapter != null)
                            {
                                dbAdapter.SelectCommand = command;

                                OpenConnection(command);
                                rowsAdded = dbAdapter.Fill(dt);
                                CloseConnection(command);
                            }
                        }
                    }
                    else
                    {
                        OpenConnection(command);
                        command.ExecuteNonQuery();
                        CloseConnection(command);
                    }
                }

                return rowsAdded;
            }
        }

        private DbCommand CreateCommand(string SQL, DataRow drParams, bool isCPanel = false)
        {
            if (DbProvider != null && !MyUtils.IsEmpty(SQL))
            {
                DbCommand command = DbProvider.CreateCommand();
                if (command != null)
                {
                    command.Connection = CreateConnection(isCPanel);

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

                    AddParameterNames(command);
                    AddParameterValues(command, drParams);
                }

                return command;
            }

            return null;
        }

        private DbConnection CreateConnection(bool isCPanel = false)
        {
            DbConnection connection = null;

            switch (ProviderName)
            {
                case "System.Data.SqlClient":
                    connection = new SqlConnection();
                    break;
                case "System.Data.OleDb":
                    connection = new OleDbConnection();
                    break;
                case "System.Data.Odbc":
                    connection = new OdbcConnection();
                    break;
                default:
                    connection = DbProvider.CreateConnection();
                    break;
            }

            if (connection != null)
            {
                string connectionString = isCPanel
                    ? Properties.Settings.Default.CPanelConnectionString
                    : CiApplication.ConnectionString;

                connection.ConnectionString = connectionString;

                string databaseFolder = MyWebUtils.MapPath(MyWebUtils.ApplicationFolder);

                if (ProviderName == "System.Data.OleDb" &&
                    !System.IO.Path.IsPathRooted(connection.DataSource) &&
                    !MyUtils.IsEmpty(databaseFolder))
                {
                    string newConnectionString = "";

                    foreach (string connectionParameter in connectionString.Split(';'))
                    {
                        if (connectionParameter.StartsWith("Data Source="))
                        {
                            newConnectionString += "Data Source=" +
                                databaseFolder + @"\" + connection.DataSource + ";";
                        }
                        else
                        {
                            newConnectionString += connectionParameter + ";";
                        }
                    }

                    connection.ConnectionString = newConnectionString;
                }
            }

            return connection;
        }

        private void AddParameterNames(DbCommand command)
        {
            if (command != null)
            {
                if (command.GetType() == typeof(SqlCommand) && command.CommandType == CommandType.StoredProcedure)
                {
                    OpenConnection(command);
                    SqlCommandBuilder.DeriveParameters((SqlCommand)command);
                    CloseConnection(command);
                }
                else
                {
                    ArrayList argNames = MyUtils.GetParameters(command.CommandText);
                    if (argNames != null)
                    {
                        foreach (string argName in argNames)
                        {
                            string parameterName = "@" + argName;
                            if (!command.Parameters.Contains(parameterName))
                            {
                                DbParameter parameter = command.CreateParameter();
                                parameter.ParameterName = parameterName;

                                command.Parameters.Add(parameter);
                            }
                        }
                    }
                }
            }
        }

        private void AddParameterValues(DbCommand command, DataRow drParams)
        {
            if (command != null && drParams != null)
            {
                drParams = MyWebUtils.AppendColumns(drParams, GetSystemVariables());

                DataTable dt = drParams.Table;
                if (dt != null)
                {
                    DataColumnCollection dc = dt.Columns;
                    foreach (DbParameter parameter in command.Parameters)
                    {
                        if (parameter.Direction == ParameterDirection.Input)
                        {
                            string argName = parameter.ParameterName.Substring(1); // ignore leading @
                            object argValue = dc.Contains(argName)
                                ? drParams[argName]
                                : null;

                            if (MyUtils.IsEmpty(argValue))
                            {
                                parameter.Value = Convert.DBNull;
                            }
                            else
                            {
                                parameter.Value = argValue;
                            }
                        }
                    }
                }
            }
        }

        private void OpenConnection(DbCommand command)
        {
            if (command != null)
            {
                DbConnection connection = command.Connection;
                if (connection != null)
                {
                    // Try opening connection for up to 1 minute
                    for (int i = 0; i < 60; i++)
                    {
                        if (connection.State == ConnectionState.Closed)
                        {
                            connection.Open();
                        }

                        if (connection.State != ConnectionState.Open)
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
        }

        private void CloseConnection(DbCommand command)
        {
            if (command != null)
            {
                DbConnection connection = command.Connection;
                if (connection != null)
                {
                    // Try closing connection for up to 1 minute
                    for (int i = 0; i < 60; i++)
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            connection.Close();
                        }

                        if (connection.State != ConnectionState.Closed)
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
        }
    }

    public class XiApplication: XiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        public XiApplication()
        {
            PluginType = typeof(CiApplication);
        }

        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        private Hashtable mPropertySQL = new Hashtable();

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override string GetPuxUrl(DataRow drPluginKey)
        {
            string appName = GetApplicationName(drPluginKey);

            return MyWebUtils.MapPath(string.Format("Sites/{0}/UiApplication.pux", appName));
        }

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return MyWebUtils.GetBySQL("select * from Application where AppID = @AppID",
                drPluginKey,
                true);
        }

        protected override List<XElement> GetPluginDefinitions(List<XElement> xElements)
        {
            if (xElements != null)
            {
                return xElements.FindAll(el => el.Name == "CiApplication");
            }

            return null;
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            string databaseName = MyWebUtils.GetField(drPluginDefinition, "SQLDatabaseName");
            string connectionString = UiApplication.Me.CiApplication.ConnectionString;
            connectionString = connectionString.Replace("CPanel", databaseName);
            xPluginDefinition.Add(new XElement("ConnectionString", connectionString));
        }
    }
}