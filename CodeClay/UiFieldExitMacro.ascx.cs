using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiFieldExitMacro")]
    public class CiFieldExitMacro: CiMacro
    {
        // --------------------------------------------------------------------------------------------------
        // Member Variables
        // --------------------------------------------------------------------------------------------------

        private ArrayList mFieldNames = new ArrayList();

        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("FieldName")]
        public string[] FieldNames
        {
            get
            {
                return (string[])mFieldNames.ToArray(typeof(string));
            }
            set
            {
                if (value != null)
                {
                    string[] FieldNames = value;
                    mFieldNames.Clear();
                    foreach (string sql in FieldNames)
                    {
                        mFieldNames.Add(sql);
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Properties (Derived)
        // --------------------------------------------------------------------------------------------------

        [XmlIgnore]
		public bool IsTest
		{
			get { return CiTable == null; }
		}

        [XmlIgnore]
        protected DataRow InputParams { get; set; } = null;

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override void Run(DataRow drParams)
        {
            InputParams = drParams;
            base.Run(drParams);
        }

        protected override string GetResultScript(DataTable dt)
        {
            string script = "";
            DataRow drParams = (MyWebUtils.GetNumberOfRows(dt) > 0)
                ? dt.Rows[0]
                : null;

            DataColumnCollection dcInputColumns = (InputParams != null)
                ? InputParams.Table.Columns
                : null;

            if (drParams != null)
            {
                foreach (DataColumn dc in drParams.Table.Columns)
                {
                    string columnName = dc.ColumnName;
                    object objColumnValue = drParams[dc];
                    string strColumnValue = MyUtils.Coalesce(objColumnValue, "").ToString();

                    if (columnName.StartsWith("_"))
                    {
                        columnName = columnName.Substring(1);
                        strColumnValue = FillTemplate(strColumnValue);
                    }
                    else if (dcInputColumns != null && dcInputColumns.Contains(columnName))
                    {
                        InputParams[columnName] = objColumnValue;
                    }

                    if (objColumnValue != null && objColumnValue.GetType() == typeof(bool))
                    {
                        strColumnValue = objColumnValue.ToString().ToLower();
                    }
                    else
                    {
                        strColumnValue = "'" + HttpUtility.JavaScriptStringEncode(strColumnValue) + "'";
                    }

                    if (IsTest)
                    {
                        script += string.Format("alert(\'{0} = {1}\'); ", columnName, strColumnValue);
                    }
                    else if (!FieldNames.Contains(columnName) && CiTable != null)
                    {
                        // Avoid recursive calls to change a field's value
                        script += string.Format("SetEditorValue(\'{0}\', \'{1}\', {2}); ",
                          CiTable.TableName,
                          columnName,
                          strColumnValue);
                    }
                }
            }

            return script;
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private string FillTemplate(string template)
        {
            if (InputParams != null)
            {
                foreach (DataColumn dc in InputParams.Table.Columns)
                {
                    string columnName = dc.ColumnName;
                    template = template.Replace("{" + columnName + "}", InputParams[columnName].ToString());
                }
            }

            return template;
        }
    }

    public partial class UiFieldExitMacro : UiMacro
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public virtual CiFieldExitMacro CiFieldExitMacro
        {
            get { return CiPlugin as CiFieldExitMacro; }
            set { CiPlugin = value; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            string[] fieldNames = (CiFieldExitMacro != null) ? CiFieldExitMacro.FieldNames : new string[]{};

            UiTable = this.Parent as UiTable;

            dxTestField.Visible = IsUnbound;
            if (IsUnbound && fieldNames.Length > 0)
            {
                string fieldName = fieldNames[0];

                dxTestField.ID = fieldName;
                dxTestField.Caption = fieldName;
                dxTestField.JSProperties["cpHasFieldExitMacro"] = true;
            }

            if (UiTable != null)
            {
                CiTable ciTable = UiTable.CiTable;
                if (ciTable != null)
                {
                    dxFieldExitPanel.JSProperties["cpTableName"] = ciTable.TableName;
                }
            }
        }

        protected void dxFieldExitPanel_Callback(object sender, CallbackEventArgsBase e)
		{
			dxFieldExitPanel.JSProperties["cpScript"] = RunFieldExitMacros(e.Parameter);
		}

		// --------------------------------------------------------------------------------------------------
		// Helpers
		// --------------------------------------------------------------------------------------------------

		private string RunFieldExitMacros(string fieldName)
		{
			string script = null;
			DataRow drParams = null;
			ArrayList ciMacros = new ArrayList();

			if (IsUnbound)
			{
				DataTable dt = new DataTable();
				drParams = dt.NewRow();
				dt.Rows.Add(drParams);

				dt.Columns.Add(fieldName);
				drParams[fieldName] = dxTestField.Value;
				ciMacros.Add(CiFieldExitMacro);
			}
			else if (UiTable != null && !MyUtils.IsEmpty(fieldName))
			{
				drParams = GetState();

				CiTable ciTable = UiTable.CiTable;
				if (ciTable != null)
				{
					ciMacros.AddRange(ciTable.GetFieldExitMacros(fieldName));
				}
			}

            if (UiTable != null)
            {
                foreach (CiMacro ciMacro in ciMacros)
                {
                    ciMacro.Run(drParams);
                    script += ciMacro.ResultScript;

                    DataTable dt = ciMacro.ResultTable;
                    if (dt != null && MyWebUtils.GetNumberOfRows(dt) > 0)
                    {
                        DataRow dr = dt.Rows[0];
                        foreach (DataColumn dc in dt.Columns)
                        {
                            string key = dc.ColumnName;
                            UiTable[key] = dr[key];
                        }
                    }
                }
            }

            script += GetFormatScript(fieldName);

			return script;
		}

        private string GetFormatScript(string fieldName)
        {
            string script = "";

            if (UiTable != null)
            {
                CiTable ciTable = UiTable.CiTable;
                if (ciTable != null)
                {
                    CiField ciField = ciTable.GetField(fieldName);
                    if (ciField != null)
                    {
                        foreach (CiField ciFollowerField in ciField.CiFollowerFields)
                        {
                            script += GetEditableScript(ciFollowerField);
                        }
                    }
                }
            }

            return script;
        }

        private string GetEditableScript(CiField ciField)
        {
            string script = "";

            if (ciField != null)
            {
                DataRow drParams = GetState();
                CiTable ciTable = ciField.CiTable;

                bool isEditable = MyWebUtils.Eval<bool>(ciField.Editable, drParams);
                script += string.Format("SetEditorEditable('{0}', '{1}', {2}, {3});",
                    ciTable.TableName,
                    ciField.FieldName,
                    ciField.Mandatory.ToString().ToLower(),
                    isEditable.ToString().ToLower());
            }

            return script;
        }
    }
}