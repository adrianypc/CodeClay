using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
        public string IconID { get; set; } = "functionlibrary_datetime_16x16";

        [XmlElement("Confirm")]
        public bool Confirm { get; set; } = false;

        [XmlElement("VisibleSQL")]
        public string VisibleSQL { get; set; } = "";

        [XmlSqlElement("NavigateUrl", typeof(string))]
        public XmlElement NavigateUrl { get; set; } = MyWebUtils.CreateXmlElement("NavigateUrl", "");

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

        public virtual void Run(DataRow drParams, bool ignoreErrorMessage = false)
        {
            if (IsVisible(drParams))
            {
                ErrorMessage = RunValidateSQL(drParams);

                if (MyUtils.IsEmpty(ErrorMessage))
                {
                    try
                    {
                        ResultTable = RunActionSQL(drParams);

                        ResultTable = RunChildMacros(ResultTable);

                        ResultScript = GetResultScript(ResultTable);
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = string.Format("{0} macro failed: {1}", MacroName, HttpUtility.JavaScriptStringEncode(ex.Message));
                    }
                }
            }
            else
            {
                ErrorMessage = string.Format("Cannot run {0} macro on this record", MacroName);
            }

            if (ignoreErrorMessage)
            {
                ErrorMessage = "";
            }

            if (!MyUtils.IsEmpty(ErrorMessage))
            {
                ResultScript = string.Format("alert('{0}');", ErrorMessage);
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

        protected virtual string RunValidateSQL(DataRow drParams)
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

        protected virtual DataTable RunActionSQL(DataRow drParams)
        {
            if (ActionSQL != null && ActionSQL.Length == 1 && ActionSQL[0] == "*")
            {
                return drParams.Table;
            }

            return UiApplication.Me.GetBySQL(ActionSQL, drParams);
        }

        protected virtual DataTable RunChildMacros(DataTable dtInput)
        {
            DataTable dtOutput = dtInput.Copy();

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
                            dtOutput = dtResult.Copy();
                        }
                        else if (dtOutput != null && dtResult != null)
                        {
                            foreach (DataRow drResult in dtResult.Rows)
                            {
                                try
                                {
                                    dtOutput.Rows.Add(drResult);
                                }
                                catch
                                {
                                    // Do nothing as row may already exist
                                }
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
            DataRow drParams = (MyWebUtils.GetNumberOfRows(dt) > 0)
                ? dt.Rows[0]
                : null;

            string navigateUrl = MyWebUtils.Eval<string>(NavigateUrl, drParams);
            bool isPuxUrl = false;

            // Reformat URL if navigateUrl refers to a PUX file
            string[] navigateUrlTokens = navigateUrl.Split('&');
            if (!MyUtils.IsEmpty(navigateUrl) && !navigateUrl.Contains(@"\") && navigateUrlTokens[0].EndsWith(".pux"))
            {
                bool isPopup = !MyUtils.IsEmpty(NavigatePos) && NavigatePos.StartsWith("Popup");

                string popupQueryParameter = isPopup ? "IsPopup=Y&" : "";
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
            else if (NavigatePos == "Download")
            {
                navigateUrl = "";
                return "Download('Adrian.txt', 'Something goes here')";
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

        private List<string> mActionSQL = new List<string>();
        private List<string> mVisibleSQL = new List<string>();
        private List<string> mTriggerFieldNames = new List<string>();

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
                    case "DEFAULT":
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
            return MyWebUtils.GetBySQL("?exec spMacro_sel @AppID, @TableID, null, '!Stub'", drPluginKey, 0);
        }

        protected override List<XElement> GetPluginDefinitions(List<XElement> xElements)
        {
            if (xElements != null)
            {
                return xElements.FindAll(el => el.Name.ToString().EndsWith("Macro"));
            }

            return null;
        }

        protected override void WriteToDB(DataRow drPluginDefinition)
        {
            object objMacroID = MyWebUtils.EvalSQL("select MacroID from CiMacro " +
                "where AppID = @AppID and TableID = @TableID and MacroName = @MacroName", drPluginDefinition);

            if (!MyUtils.IsEmpty(objMacroID))
            {
                drPluginDefinition["MacroID"] = objMacroID;
            }
            else
            {
                string insertColumnNames = "@AppID, @TableID, @MacroName, @Caption, @Type";
                string insertSQL = string.Format("?exec spMacro_ins {0}", insertColumnNames);

                drPluginDefinition["MacroID"] = MyWebUtils.EvalSQL(insertSQL, drPluginDefinition, 0);
            }

            string updateColumnNames = "@AppID, @TableID, @MacroID, @MacroName, @NavigateUrl" +
                ", @NavigatePos, @Confirm, @Type";
            string updateSQL = string.Format("exec spMacro_upd {0}", updateColumnNames);

            MyWebUtils.GetBySQL(updateSQL, drPluginDefinition, 0);

            if (drPluginDefinition["Type"].ToString() == "Field Exit")
            {
                StoreTriggerFields(drPluginDefinition);
            }

            StoreSQL("ActionSQL", mActionSQL, drPluginDefinition);
            StoreSQL("VisibleSQL", mVisibleSQL, drPluginDefinition);
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                if (drPluginDefinition.Table.Columns.Contains("Type"))
                {
                    string macroType = MyUtils.Coalesce(drPluginDefinition["Type"], "").ToString();
                    if (macroType == "Toolbar")
                    {
                        XElement xToolbar = new XElement("Toolbar");
                        xToolbar.Value = "true";
                        xPluginDefinition.Add(xToolbar);
                    }
                    else if ("Select,Insert,Update,Delete,Default".Contains(macroType))
                    {
                        if (xPluginDefinition.Name == macroType + "Macro")
                        {
                            XElement xMacroName = xPluginDefinition.Element("MacroName");
                            if (xMacroName != null)
                            {
                                xMacroName.Remove();
                            }

                            XElement xCaption = xPluginDefinition.Element("Caption");
                            if (xCaption != null)
                            {
                                xCaption.Remove();
                            }
                        }
                    }
                    else if (macroType == "Field Exit")
                    {
                        xPluginDefinition.Name = "CiFieldExitMacro";

                        XElement xCaption = xPluginDefinition.Element("Caption");
                        if (xCaption != null)
                        {
                            xCaption.Remove();
                        }
                    }
                }

                DataTable dtTriggerField = MyWebUtils.GetBySQL("?exec spTriggerField_sel @AppID, @TableID, @MacroID", drPluginDefinition, 0);
                if (dtTriggerField != null)
                {
                    foreach (DataRow drTriggerField in dtTriggerField.Rows)
                    {
                        XElement xFieldName = new XElement("FieldName");
                        xFieldName.Value = drTriggerField["DisplayFieldName"].ToString();
                        xPluginDefinition.Add(xFieldName);
                    }
                }

                DataTable dtActionSQL = MyWebUtils.GetBySQL("?exec spMacroSQL_sel @AppID, @TableID, @MacroID, 'ActionSQL'", drPluginDefinition, 0);
                if (dtActionSQL != null)
                {
                    foreach (DataRow drActionSQL in dtActionSQL.Rows)
                    {
                        XElement xActionSQL = new XElement("ActionSQL");
                        xActionSQL.Value = drActionSQL["SQL"].ToString();
                        xPluginDefinition.Add(xActionSQL);
                    }
                }

                DataTable dtVisibleSQL = MyWebUtils.GetBySQL("?exec spMacroSQL_sel @AppID, @TableID, @MacroID, 'VisibleSQL'", drPluginDefinition, 0);
                if (dtVisibleSQL != null)
                {
                    foreach (DataRow drVisibleSQL in dtVisibleSQL.Rows)
                    {
                        XElement xVisibleSQL = new XElement("VisibleSQL");
                        xVisibleSQL.Value = drVisibleSQL["SQL"].ToString();
                        xPluginDefinition.Add(xVisibleSQL);
                    }
                }

                DataTable dtValidateSQL = MyWebUtils.GetBySQL("?exec spMacroSQL_sel @AppID, @TableID, @MacroID, 'ValidateSQL'", drPluginDefinition, 0);
                if (dtValidateSQL != null)
                {
                    foreach (DataRow drValidateSQL in dtValidateSQL.Rows)
                    {
                        XElement xValidateSQL = new XElement("ValidateSQL");
                        xValidateSQL.Value = drValidateSQL["SQL"].ToString();
                        xPluginDefinition.Add(xValidateSQL);
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
                    case "DefaultMacro":
                        string macroName = pluginName.Substring(0, pluginName.Length - 5);
                        drPluginDefinition["MacroName"] = macroName;
                        drPluginDefinition["Caption"] = "-";
                        drPluginDefinition["Type"] = macroName;
                        break;

                    case "CiFieldExitMacro":
                        drPluginDefinition["Caption"] = "-";
                        drPluginDefinition["Type"] = "Field Exit";
                        break;

                    case "CiMacro":
                        drPluginDefinition["MacroName"] = MyWebUtils.GetXmlChildValue(xPluginDefinition, "MacroName");
                        drPluginDefinition["Caption"] = MyWebUtils.GetXmlChildValue(xPluginDefinition, "Caption");

                        if (MyWebUtils.GetXmlChildValue(xPluginDefinition, "Toolbar").ToUpper() == "TRUE")
                        {
                            drPluginDefinition["Type"] = "Toolbar";
                        }
                        else
                        {
                            drPluginDefinition["Type"] = "Menu";
                        }
                        break;
                }

                mActionSQL.Clear();
                foreach (XElement xActionSQL in xPluginDefinition.Elements("ActionSQL"))
                {
                    mActionSQL.Add(xActionSQL.Value);
                }

                mVisibleSQL.Clear();
                foreach (XElement xVisibleSQL in xPluginDefinition.Elements("VisibleSQL"))
                {
                    mVisibleSQL.Add(xVisibleSQL.Value);
                }

                if (xPluginDefinition.Name == "CiFieldExitMacro")
                {
                    mTriggerFieldNames.Clear();
                    foreach (XElement xTriggerField in xPluginDefinition.Elements("FieldName"))
                    {
                        mTriggerFieldNames.Add(xTriggerField.Value);
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private void StoreTriggerFields(DataRow drPluginDefinition)
        {
            if (drPluginDefinition != null)
            {
                DataTable dt = drPluginDefinition.Table;

                if (dt != null && !dt.Columns.Contains("FieldID"))
                {
                    dt.Columns.Add("FieldID");
                }

                if (dt != null && !dt.Columns.Contains("FieldName"))
                {
                    dt.Columns.Add("FieldName");
                }

                string deleteSQL = string.Format("exec spTriggerField_del @AppID, @TableID, @MacroID");
                MyWebUtils.GetBySQL(deleteSQL, drPluginDefinition, 0);

                foreach (string fieldName in mTriggerFieldNames)
                {
                    drPluginDefinition["FieldName"] = fieldName;
                    drPluginDefinition["FieldID"] = MyWebUtils.EvalSQL("select FieldID from CiField " +
                        "where AppID = @AppID and TableID = @TableID and FieldName = @FieldName", drPluginDefinition);

                    string insertSQL = string.Format("exec spTriggerField_ins @AppID, @TableID, @MacroID, @FieldID");
                    MyWebUtils.GetBySQL(insertSQL, drPluginDefinition, 0);
                }

            }
        }

        private void StoreSQL(string sqlType, List<string> sqlList, DataRow drPluginDefinition)
        {
            if (drPluginDefinition != null)
            {
                DataTable dt = drPluginDefinition.Table;

                if (dt != null && !dt.Columns.Contains("SQL"))
                {
                    dt.Columns.Add("SQL");
                }

                string deleteSQL = string.Format("exec spMacroSQL_del @AppID, @TableID, @MacroID, '{0}'", sqlType);
                MyWebUtils.GetBySQL(deleteSQL, drPluginDefinition, 0);

                foreach (string sql in sqlList)
                {
                    drPluginDefinition["SQL"] = sql;

                    string insertSQL = string.Format("exec spMacroSQL_ins @AppID, @TableID, @MacroID, '{0}', @SQL", sqlType);
                    MyWebUtils.GetBySQL(insertSQL, drPluginDefinition, 0);
                }
            }
        }
    }

    public class XiButtonMacro: XiMacro
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public object NewMacroID { get; set; }  = null;

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return MyWebUtils.GetBySQL("?exec spButtonMacro_sel @AppID, @TableID, @FieldID", drPluginKey, 0);
        }

        protected override void DeletePluginDefinitions(DataRow drPluginKey)
        {
            // Do nothing
        }

        protected override void WriteToDB(DataRow drPluginDefinition)
        {
            drPluginDefinition["Type"] = "Button";
            base.WriteToDB(drPluginDefinition);
            NewMacroID = drPluginDefinition["MacroID"];
        }
    }
}