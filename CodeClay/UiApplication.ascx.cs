using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Threading;

// Extra references
using CodistriCore;
using DevExpress.Web;

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

        [XmlElement("Theme")]
        public string Theme { get; set; } = ASPxWebControl.GlobalTheme;

        [XmlElement("ThemeColor")]
        public string ThemeColor { get; set; } = ASPxWebControl.GlobalThemeBaseColor;

        [XmlElement("IsAzureFolder")]
        public bool IsAzureFolder { get; set; } = Properties.Settings.Default.IsAzureFolder;
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
                if (mDbProvider == null && !MyUtils.IsEmpty(ProviderName))
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

        public DataTable GetBySQL(string sql, DataRow drParams, int? appID = null)
        {
            return GetBySQL(new string[] { sql }, drParams, appID);
        }

        public DataTable GetBySQL(string[] sqlList, DataRow drParams, int? appID = null)
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
                string databaseConnectionKeyword = "use ";

                if (sql.StartsWith("?"))
                {
                    // SQL which fetches data will begin with '?'
                    isSelectSQL = true;
                    sql = sql.Substring(1);
                }
                else if (sql.StartsWith("$"))
                {
                    // Shorthand for singleton select
                    isSelectSQL = true;
                    sql = string.Format("select {0} from tblSingleton", sql.Substring(1));
                }
                else if (sql.ToLower().StartsWith(databaseConnectionKeyword))
                {
                    string sqlFragment = sql.Substring(databaseConnectionKeyword.Length);
                    string databaseName = sqlFragment.Substring(0, sqlFragment.IndexOf(";"));
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

                            rowsFetched += Fill(sql, drInput, dt, appID);
                        }
                        else
                        {
                            lastRowAdded = 0;
                            dtResultTable = (new DataSet()).Tables.Add();
                            Fill(sql, drInput, dtResultTable, appID);
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
                        Fill(sql, drInput, null, appID);
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

        private int Fill(string SQL, DataRow drParams, DataTable dt, int? appID = null)
        {
            lock (mDbLock)
            {
                int rowsAdded = 0;
                DbCommand command = CreateCommand(SQL, drParams, appID);

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

        private DbCommand CreateCommand(string SQL, DataRow drParams, int? appID = null)
        {
            if (DbProvider != null && !MyUtils.IsEmpty(SQL))
            {
                DbCommand command = DbProvider.CreateCommand();
                if (command != null)
                {
                    command.Connection = CreateConnection(appID);
                    command.CommandTimeout = 300;

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

        private string GetConnectionString(int? appID = null)
        {
            string connectionString = "";

            switch (appID)
            {
                case null:
                    connectionString = CiApplication.ConnectionString;
                    break;

                case 0:
                    connectionString = Properties.Settings.Default.CPanelConnectionString;
                    break;

                default:
                    DataRow drKey = MyWebUtils.CreateDataRow();
                    drKey.Table.Columns.Add("AppID");
                    drKey["AppID"] = appID;
                    string databaseName = MyWebUtils.GetDatabaseName(drKey);
                    if (!MyUtils.IsEmpty(databaseName))
                    {
                        connectionString = Properties.Settings.Default.CPanelConnectionString;
                        connectionString = connectionString.Replace("=CPanel;", string.Format("={0};", databaseName));
                    }
                    break;
            }

            return connectionString;
        }

        private DbConnection CreateConnection(int? appID = null)
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
                string connectionString = GetConnectionString(appID);

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

        public override void DownloadFile(DataRow drKey, string puxUrl)
        {
            object oldAppName = MyWebUtils.GetField(drKey, "OldAppName");
            if (!MyUtils.IsEmpty(oldAppName))
            {
                string oldAppDir = MyWebUtils.MapPath(string.Format("Sites/{0}", oldAppName));
                if (!MyUtils.IsEmpty(puxUrl))
                {
                    string newAppName = MyWebUtils.GetApplicationName(drKey);

                    if (newAppName != oldAppName.ToString())
                    {
                        string newAppDir = MyWebUtils.MapPath(string.Format("Sites/{0}", newAppName));
                        if (Directory.Exists(oldAppDir))
                        {
                            Directory.Move(oldAppDir, newAppDir);
                        }
                    }
                }
                else
                {
                    Directory.Delete(oldAppDir, true);
                }
            }
            else if (IsUserDefinedApplication(drKey))
            {
                FileInfo fi = new FileInfo(puxUrl);
                string destinationFolder = MyWebUtils.MapPath(string.Format("Sites/{0}", fi.Directory.Name));
                string sourceDropdownPUX = MyWebUtils.MapPath(string.Format("Sites/{0}/Dropdown.pux", "Template"));
                string destinationDropdownPUX = string.Format("{0}/Dropdown.pux", destinationFolder);

                if (!File.Exists(destinationDropdownPUX))
                {
                    if (!Directory.Exists(destinationFolder))
                    {
                        Directory.CreateDirectory(destinationFolder);
                    }

                    File.Copy(sourceDropdownPUX, destinationDropdownPUX);
                }
            }

            base.DownloadFile(drKey, puxUrl);
        }

        public override string GetPuxUrl(DataRow drPluginKey)
        {
            string appName = MyWebUtils.GetApplicationName(drPluginKey);

            if (MyUtils.IsEmpty(appName))
            {
                return null;
            }

            return MyWebUtils.MapPath(string.Format("Sites/{0}/UiApplication.pux", appName));
        }

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return MyWebUtils.GetBySQL("select * from Application where AppID = @AppID",
                drPluginKey,
                0);
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
            xPluginDefinition.Add(new XElement("IsDesigner", true));
            xPluginDefinition.Add(new XElement("ProviderName", "System.Data.SqlClient"));

            XElement xHomePluginSrc = xPluginDefinition.Element("HomePluginSrc");
            if (xHomePluginSrc != null)
            {
                string homePluginSrc = xHomePluginSrc.Value;
                if (!MyUtils.IsEmpty(homePluginSrc) && !homePluginSrc.EndsWith(".pux"))
                {
                    xHomePluginSrc.Value = homePluginSrc + ".pux";
                }
            }

            string databaseName = MyWebUtils.GetField(drPluginDefinition, "SQLDatabaseName") as string;
            string connectionString = UiApplication.Me.CiApplication.ConnectionString;
            connectionString = connectionString.Replace("CPanel", databaseName);
            xPluginDefinition.Add(new XElement("ConnectionString", connectionString));

            xPluginDefinition.Add(new XElement("CiMenu",
                new XElement("MenuName", "Shortcut")));

            XElement xMenu = new XElement("CiMenu", new XElement("MenuName", "Application"));
            xPluginDefinition.Add(xMenu);

            DataRow drMenuParams = CreateMenuParams(drPluginDefinition);
            DownloadMenu(xMenu, drMenuParams);
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private bool IsUserDefinedApplication(DataRow drAppKey)
        {
            bool? userDefined = false;
            DataTable dt = MyWebUtils.GetBySQL("?exec spApplication_sel @AppID", drAppKey, 0);

            if (dt != null && MyWebUtils.GetNumberOfRows(dt) > 0)
            {
                userDefined = MyWebUtils.GetField<bool>(dt.Rows[0], "UserDefined");
            }

            return userDefined.GetValueOrDefault(false);
        }

        private void DownloadMenu(XElement xMenu, DataRow drParams)
        {
            DataTable dtMenus = MyWebUtils.GetBySQL("?exec spMenu_sel @AppID, @ParentMenuID, null", drParams);
            if (MyWebUtils.GetNumberOfRows(dtMenus) > 0)
            {
                foreach (DataRow drMenu in dtMenus.Rows)
                {
                    XElement xMenuItem = new XElement("CiMenu");
                    xMenuItem.Add(new XElement("MenuName", MyWebUtils.GetStringField(drMenu, "MenuName")));
                    xMenuItem.Add(new XElement("PluginSrc", MyWebUtils.GetStringField(drMenu, "MenuUrl")));

                    xMenu.Add(xMenuItem);

                    DataRow drMenuParams = CreateMenuParams(drMenu);
                    DownloadMenu(xMenuItem, drMenuParams);
                }
            }
        }

        private DataRow CreateMenuParams(DataRow drParams)
        {
            DataRow drMenuParams = MyWebUtils.CreateDataRow();
            DataColumnCollection dcMenuParams = drMenuParams.Table.Columns;
            MyWebUtils.AddColumnIfRequired(dcMenuParams, "AppID");
            MyWebUtils.AddColumnIfRequired(dcMenuParams, "ParentMenuID");

            drMenuParams["AppID"] = MyWebUtils.GetField<int>(drParams, "AppID");
            drMenuParams["ParentMenuID"] = MyUtils.Coalesce(MyWebUtils.GetField<int>(drParams, "MenuID"), 0);

            return drMenuParams;
        }
    }
}