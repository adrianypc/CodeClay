using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using CodistriCore;

namespace CodeClay
{
    [XmlType("CiMacro")]
    public class CiMacro: CiPlugin
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

        [XmlElement("Confirm")]
        public bool Confirm { get; set; } = false;

        [XmlElement("VisibleSQL")]
        public string VisibleSQL { get; set; } = "";

        [XmlElement("NavigateUrl")]
        public string NavigateUrl { get; set; } = "";

        [XmlElement("NavigatePos")]
        public string NavigatePos { get; set; } = "";

        [XmlElement("Caption")]
		public string MacroCaption
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

		// --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual void Run(DataRow drParams)
        {
            ErrorMessage = RunValidateSQL(drParams);

            if (MyUtils.IsEmpty(ErrorMessage))
            {
                ResultTable = RunActionSQL(drParams);

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

		public virtual ArrayList GetActionParameterNames()
		{
			ArrayList sqlParams = new ArrayList();

			foreach (string actionSQL in ActionSQL)
			{
				sqlParams = MyWebUtils.Merge(sqlParams, MyUtils.GetParameters(actionSQL));
			}

            if (CiTable != null)
            {
                foreach (CiField ciField in CiTable.CiSearchableFields)
                {
                    string fieldName = ciField.FieldName;
                    if (sqlParams.Contains(fieldName))
                    {
                        sqlParams.Remove(fieldName);
                    }
                }
            }

			return sqlParams;
		}

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private string RunValidateSQL(DataRow drParams)
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

        private DataTable RunActionSQL(DataRow drParams)
        {
            if (ActionSQL != null && ActionSQL.Length == 1 && ActionSQL[0] == "*")
            {
                return drParams.Table;
            }

            return UiApplication.Me.GetBySQL(ActionSQL, drParams);
        }

        protected virtual string GetResultScript(DataTable dt)
        {
            string navigateUrl = NavigateUrl;
            bool isPuxUrl = false;

            DataRow drParams = (MyWebUtils.GetNumberOfRows(dt) > 0)
                ? dt.Rows[0]
                : null;

            // Reformat URL if navigateUrl refers to a PUX file
            if (!MyUtils.IsEmpty(navigateUrl) && navigateUrl.EndsWith(".pux"))
            {
                string popupQueryParameter = (NavigatePos == "Popup") ? "IsPopup=Y&" : "";
                navigateUrl = string.Format("Default.aspx?Application={0}&{1}PluginSrc={2}", MyWebUtils.Application, popupQueryParameter, navigateUrl);
                isPuxUrl = true;
            }

            string parameterQueryString = "";
            if (drParams != null)
            {
                foreach (DataColumn dc in drParams.Table.Columns)
                {
                    string columnName = dc.ColumnName;
                    object columnValue = MyUtils.Coalesce(drParams[dc], "");

                    if (!isPuxUrl)
                    {
                        parameterQueryString += "?";
                    }
                    else
                    {
                        parameterQueryString += "&";
                    }

                    parameterQueryString += string.Format("{0}={1}", columnName, columnValue.ToString());
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
}