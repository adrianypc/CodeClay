using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

// Extra references
using CodistriCore;

namespace CodeClay
{
    [XmlType("CiMacro")]
    public class CiMacro : CiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Member Variables
        // --------------------------------------------------------------------------------------------------

        private string mMacroCaption = "";
        private ArrayList mValidateSQL = new ArrayList();
        private ArrayList mActionSQL = new ArrayList();

        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("MacroName")]
        public string MacroName { get; set; } = "";

        [XmlElement("IconID")]
        public string IconID { get; set; } = "actions_task_16x16devav";

        [XmlElement("Confirm")]
        public bool Confirm { get; set; } = false;

        [XmlElement("VisibleSQL")]
        public string VisibleSQL { get; set; } = "";

        [XmlElement("NavigateUrl")]
        public string NavigateUrl { get; set; } = "";

        [XmlElement("NavigatePos")]
        public string NavigatePos { get; set; } = "";

        [XmlElement("Caption")]
        public string Caption
        {
            get
            {
                if (MyUtils.IsEmpty(mMacroCaption))
                {
                    return MacroName;
                }

                return mMacroCaption;
            }

            set
            {
                mMacroCaption = value;
            }
        }

        [XmlElement("Toolbar")]
        public bool Toolbar { get; set; } = false;

        [XmlElement("ValidateSQL")]
        public string[] ValidateSQL
        {
            get
            {
                return (string[])mValidateSQL.ToArray(typeof(string));
            }
            set
            {
                if (value != null)
                {
                    string[] ValidateSQL = value;
                    mValidateSQL.Clear();
                    foreach (string sql in ValidateSQL)
                    {
                        mValidateSQL.Add(sql);
                    }
                }
            }
        }

        [XmlElement("ActionSQL")]
        public string[] ActionSQL
        {
            get
            {
                return (string[])mActionSQL.ToArray(typeof(string));
            }
            set
            {
                if (value != null)
                {
                    string[] actionSQL = value;
                    mActionSQL.Clear();
                    foreach (string sql in actionSQL)
                    {
                        mActionSQL.Add(sql);
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Properties (Derived)
        // --------------------------------------------------------------------------------------------------

        [XmlIgnore]
        public override string ID
        {
            get { return MacroName; }
        }

        [XmlIgnore]
        public string ErrorMessage { get; set; } = null;

        [XmlIgnore]
        public DataTable ResultTable { get; set; } = null;

        [XmlIgnore]
        public string ResultScript { get; set; } = null;

        [XmlIgnore]
        public CiTable CiTable
        {
            get { return CiParentPlugin as CiTable; }
            set { CiParentPlugin = value; }
        }

        [XmlIgnore]
        public CiMacro[] CiMacros
        {
            get
            {
                return CiPlugins.Where(c => (c as CiMacro) != null).Select(c => c as CiMacro).ToArray();
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual void Run(DataRow drParams)
        {
            ErrorMessage = RunValidateSQL(drParams);

            if (MyUtils.IsEmpty(ErrorMessage))
            {
                ResultTable = RunActionSQL(drParams);

                ResultTable = RunChildMacros(ResultTable);

                ResultScript = GetResultScript(ResultTable);
            }
            else
            {
                ResultScript = string.Format("alert('{0}')", ErrorMessage);
            }
        }

        public virtual bool IsVisible(DataRow drParams)
        {
            return MyWebUtils.IsTrueSQL(VisibleSQL, drParams);
        }

        public virtual ArrayList GetMacroParameters()
        {
            ArrayList sqlParams = new ArrayList();

            foreach (string actionSQL in ActionSQL)
            {
                sqlParams = MyWebUtils.Merge(sqlParams, MyUtils.GetParameters(actionSQL));
            }

            sqlParams = MyWebUtils.Merge(sqlParams, MyUtils.GetParameters(VisibleSQL));

            return sqlParams;
        }

        public virtual ArrayList GetMissingMacroParameters(DataRow drParams)
        {
            ArrayList sqlParams = new ArrayList();

            if (drParams != null)
            {
                DataTable dt = drParams.Table;

                if (dt != null)
                {
                    DataColumnCollection columnCollection = dt.Columns;
                    foreach (string parameterName in GetMacroParameters())
                    {
                        if (!columnCollection.Contains(parameterName))
                        {
                            sqlParams.Add(parameterName);
                        }
                    }
                }
            }

            return sqlParams;
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        protected string RunValidateSQL(DataRow drParams)
        {
            foreach (string validateSQL in ValidateSQL)
            {
                DataTable dt = UiApplication.Me.GetBySQL(validateSQL, drParams);

                if (dt != null && MyWebUtils.GetNumberOfRows(dt) > 0 && MyWebUtils.GetNumberOfColumns(dt) > 0)
                {
                    return MyUtils.Coalesce(dt.Rows[0][0], "").ToString();
                }
            }

            return null;
        }

        protected DataTable RunActionSQL(DataRow drParams)
        {
            if (ActionSQL != null && ActionSQL.Length == 1 && ActionSQL[0] == "*")
            {
                return drParams.Table;
            }

            ArrayList sqlList = new ArrayList();
            if (CiTable != null && CiTable.CiParentTable == null && MacroName.ToUpper() == "SELECT")
            {
                foreach (string actionSQL in ActionSQL)
                {
                    string sql = actionSQL;
                    foreach (CiField ciField in CiTable.CiSearchableFields)
                    {
                        string fieldName = ciField.FieldName;
                        sql = sql.Replace("@" + fieldName, "@" + ciField.SearchableFieldName);
                    }

                    sqlList.Add(sql);
                }
            }

            if (sqlList.Count > 0)
            {
                return UiApplication.Me.GetBySQL((string[])sqlList.ToArray(typeof(string)), drParams);
            }

            return UiApplication.Me.GetBySQL(ActionSQL, drParams);
        }

        protected DataTable RunChildMacros(DataTable dtInput)
        {
            DataTable dtOutput = dtInput;

            foreach (CiMacro ciMacro in CiMacros)
            {
                if (dtInput != null)
                {
                    dtOutput = null;

                    foreach (DataRow drInput in dtInput.Rows)
                    {
                        ciMacro.Run(drInput);
                        DataTable dtResult = ciMacro.ResultTable;

                        if (dtOutput == null)
                        {
                            dtOutput = dtResult;
                        }
                        else if (dtOutput != null && dtResult != null)
                        {
                            foreach (DataRow drResult in dtResult.Rows)
                            {
                                dtOutput.Rows.Add(drResult);
                            }
                        }
                    }
                }
                else
                {
                    ciMacro.Run(null);

                    dtOutput = ciMacro.ResultTable;
                }

                dtInput = dtOutput;
            }

            return dtOutput;
        }

        protected virtual string GetResultScript(DataTable dt)
        {
            string navigateUrl = NavigateUrl;
            bool isPuxUrl = false;

            DataRow drParams = (MyWebUtils.GetNumberOfRows(dt) > 0)
                ? dt.Rows[0]
                : null;

            if (MyUtils.IsEmpty(navigateUrl) && dt != null && dt.Columns.Contains("NavigateUrl") && drParams != null)
            {
                navigateUrl = MyUtils.Coalesce(drParams["NavigateUrl"], "").ToString();
            }

            // Reformat URL if navigateUrl refers to a PUX file
            if (!MyUtils.IsEmpty(navigateUrl) && !navigateUrl.Contains(@"\") && navigateUrl.EndsWith(".pux"))
            {
                string popupQueryParameter = (NavigatePos == "Popup") ? "IsPopup=Y&" : "";
                navigateUrl = string.Format("Default.aspx?Application={0}&{1}PluginSrc={2}", MyWebUtils.Application, popupQueryParameter, navigateUrl);
                isPuxUrl = true;
            }

            string parameterQueryString = "";
            if (drParams != null)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    string columnName = dc.ColumnName;
                    if (columnName != "NavigateUrl")
                    {
                        object columnValue = MyUtils.Coalesce(drParams[dc], "");

                        if (!isPuxUrl)
                        {
                            parameterQueryString += "?";
                        }
                        else
                        {
                            parameterQueryString += "&";
                        }

                        parameterQueryString += string.Format("{0}={1}", columnName, columnValue.ToString().Trim());
                    }
                }
            }

            if (NavigatePos == "Previous")
            {
                navigateUrl = "..";
            }

            if (!MyUtils.IsEmpty(navigateUrl))
            {
                navigateUrl += parameterQueryString;

                if (!MyUtils.IsEmpty(NavigatePos))
                {
                    navigateUrl += LIST_SEPARATOR + NavigatePos;
                }
            }

            if (!MyUtils.IsEmpty(navigateUrl))
            {
                return string.Format("GotoUrl(\'{0}\')", HttpUtility.JavaScriptStringEncode(navigateUrl));
            }

            return null;
        }
    }

    public partial class UiMacro : UiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public bool IsUnbound
        {
            get { return UiTable == null && CiMacro != null; }
        }

        public CiMacro CiMacro
        {
            get { return CiPlugin as CiMacro; }
            set { CiPlugin = value; }
        }

        public UiTable UiTable
        {
            get { return UiParentPlugin as UiTable; }
            set { UiParentPlugin = value; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            string script = "";

            if (CiMacro != null)
            {
                CiMacro.Run(GetState());
                script = CiMacro.ResultScript;
            }

            dxMacroLabel.JSProperties["cpScript"] = script;
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual DataRow GetState()
        {
            if (UiTable != null)
            {
                return UiTable.GetState();
            }

            return base.GetState();
        }
    }

    public class XiMacro : XiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        public XiMacro()
        {
            PluginType = typeof(CiMacro);
        }

        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public List<string> ActionSQL = new List<string>();

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        protected override string GetPluginTypeName(DataRow drPluginDefinition)
        {
            string pluginTypeName = "CiMacro";

            if (drPluginDefinition.Table.Columns.Contains("Type"))
            {
                object objFieldType = MyUtils.Coalesce(drPluginDefinition["Type"], "");

                pluginTypeName = objFieldType.ToString();

                switch (pluginTypeName.ToUpper())
                {
                    case "SELECT":
                    case "INSERT":
                    case "UPDATE":
                    case "DELETE":
                        pluginTypeName = drPluginDefinition["MacroName"].ToString() + "Macro";
                        break;

                    default:
                        pluginTypeName = "CiMacro";
                        break;
                }
            }

            return pluginTypeName;
        }

        protected override Type GetPluginType(string pluginTypeName)
        {
            return typeof(CiMacro);
        }

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return MyWebUtils.GetBySQL("?exec spMacro_sel @AppID, @TableID", drPluginKey, true);
        }

        protected override List<XElement> GetPluginDefinitions(List<XElement> xElements)
        {
            if (xElements != null)
            {
                return xElements.FindAll(el => el.Name.ToString().EndsWith("Macro"));
            }

            return null;
        }

        protected override void DeletePluginDefinitions(DataRow drPluginKey)
        {
            MyWebUtils.GetBySQL("exec spMacro_del @AppID, @TableID", drPluginKey, true);
        }

        protected override void WriteToDB(DataRow drPluginDefinition)
        {
            string insertColumnNames = "@AppID, @TableID, @MacroName, @Caption, @Type";
            string insertSQL = string.Format("?exec spMacro_ins {0}", insertColumnNames);

            object objMacroID = MyWebUtils.EvalSQL(insertSQL, drPluginDefinition, true);
            if (!MyUtils.IsEmpty(objMacroID))
            {
                drPluginDefinition["MacroID"] = objMacroID;

                string updateColumnNames = "@AppID, @TableID, @MacroID, @NavigateUrl" +
                    ", @NavigatePos, @Confirm";
                string updateSQL = string.Format("exec spMacro_updLong {0}", updateColumnNames);

                MyWebUtils.GetBySQL(updateSQL, drPluginDefinition, true);

                StoreActionSQL(drPluginDefinition);
            }
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                if (drPluginDefinition.Table.Columns.Contains("Type"))
                {
                    if (MyUtils.Coalesce(drPluginDefinition["Type"], "").ToString() == "Toolbar")
                    {
                        XElement xToolbar = new XElement("Toolbar");
                        xToolbar.Value = "true";
                        xPluginDefinition.Add(xToolbar);
                    }
                }

                DataTable dtActionSQL = MyWebUtils.GetBySQL("?exec spSQL_sel 'CiMacro', 'ActionSQL', @AppID, @TableID, @MacroID", drPluginDefinition, true);
                if (dtActionSQL != null)
                {
                    foreach (DataRow drActionSQL in dtActionSQL.Rows)
                    {
                        XElement xActionSQL = new XElement("ActionSQL");
                        xActionSQL.Value = drActionSQL["SQL"].ToString();
                        xPluginDefinition.Add(xActionSQL);
                    }
                }
            }
        }

        protected override void UploadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                string pluginName = xPluginDefinition.Name.ToString();
                switch (pluginName)
                {
                    case "SelectMacro":
                    case "InsertMacro":
                    case "UpdateMacro":
                    case "DeleteMacro":
                        string macroName = pluginName.Substring(0, 6);
                        drPluginDefinition["MacroName"] = macroName;
                        drPluginDefinition["Caption"] = "-";
                        drPluginDefinition["Type"] = macroName;
                        break;
                }

                ActionSQL.Clear();
                foreach (XElement xActionSQL in xPluginDefinition.Elements("ActionSQL"))
                {
                    ActionSQL.Add(xActionSQL.Value);
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private void StoreActionSQL(DataRow drActionSQL)
        {
            if (drActionSQL != null)
            {
                DataTable dt = drActionSQL.Table;

                if (dt != null && !dt.Columns.Contains("SQL"))
                {
                    dt.Columns.Add("SQL");
                }

                MyWebUtils.GetBySQL("exec spSQL_del 'CiMacro', 'ActionSQL', @AppID, @TableID, @MacroID", drActionSQL, true);

                foreach (string actionSQL in ActionSQL)
                {
                    drActionSQL["SQL"] = actionSQL;
                    MyWebUtils.GetBySQL("exec spSQL_ins 'CiMacro', 'ActionSQL', @AppID, @TableID, @MacroID, @SQL", drActionSQL, true);
                }

            }
        }
    }
}