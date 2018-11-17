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
		public ArrayList Parameters
        {
            get
            {
                ArrayList allParameters = new ArrayList();
                foreach (string actionSQL in ActionSQL)
                {
                    ArrayList sqlParameters = MyUtils.GetParameters(actionSQL);
                    foreach (string sqlParameter in sqlParameters.ToArray(typeof(string)))
                    {
                        if (!allParameters.Contains(sqlParameter))
                        {
                            allParameters.Add(sqlParameter);
                        }
                    }
                }

                if (allParameters.Count > 0)
                {
                    return allParameters;
                }

                return null;
            }
        }

        [XmlIgnore]
        public CiTable CiTable
        {
            get { return CiParentPlugin as CiTable; }
            set { CiParentPlugin = value; }
        }

		// --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual DataTable RunSQL(DataRow drParams)
        {
            if (ActionSQL != null && ActionSQL.Length == 1 && ActionSQL[0] == "*")
            {
                return drParams.Table;
            }

            return UiApplication.Me.GetBySQL(ActionSQL, drParams);
        }

        public virtual string Run(DataRow drParams)
        {
            DataTable dtResult = RunSQL(drParams);

            DataRow drResult = (MyWebUtils.GetNumberOfRows(dtResult) > 0)
                ? dtResult.Rows[0]
                : null;

            return GetScript(drResult);
        }

        public virtual bool IsVisible(DataRow drParams)
        {
            return MyWebUtils.IsTrueSQL(VisibleSQL, drParams);
        }

        public virtual string GetScript(DataRow drParams)
        {
            string navigateUrl = null;
            int parameterCount = 0;

            // Reformat URL if navigateUrl refers to a PUX file
            navigateUrl = NavigateUrl;
            if (!MyUtils.IsEmpty(navigateUrl) && navigateUrl.EndsWith(".pux"))
            {
                string popupQueryParameter = (NavigatePos == "Popup") ? "IsPopup=Y&" : "";
                navigateUrl = string.Format("Default.aspx?Application={0}&{1}PluginSrc={2}", MyWebUtils.Application, popupQueryParameter, navigateUrl);
                parameterCount++;
            }

            string queryString = "";
            if (drParams != null)
            {
                foreach (DataColumn dc in drParams.Table.Columns)
                {
                    string columnName = dc.ColumnName;
                    object columnValue = MyUtils.Coalesce(drParams[dc], "");

                    if (parameterCount++ == 0)
                    {
                        queryString += "?";
                    }
                    else
                    {
                        queryString += "&";
                    }

                    queryString += string.Format("{0}={1}", columnName, columnValue.ToString());
                }
            }

            if (NavigatePos == "Previous")
            {
                navigateUrl = "..";
            }

            if (!MyUtils.IsEmpty(navigateUrl))
            {
                navigateUrl += queryString;

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
                script = CiMacro.Run(GetState());
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