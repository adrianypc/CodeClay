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

		// --------------------------------------------------------------------------------------------------
		// Methods (Override)
		// --------------------------------------------------------------------------------------------------
		
		public override string GetScript(DataRow drParams)
		{
			string script = "";

			if (drParams != null)
			{
				foreach (DataColumn dc in drParams.Table.Columns)
				{
					string columnName = dc.ColumnName;
					string columnValue = MyUtils.Coalesce(drParams[dc], "").ToString();
					
					columnValue = HttpUtility.JavaScriptStringEncode(columnValue);

					if (IsTest)
					{
						script += string.Format("alert(\'{0} = {1}\'); ", columnName, columnValue);
					}
					else if (!FieldNames.Contains(columnName) && CiTable != null)
					{
						// Avoid recursive calls to change a field's value
						script += string.Format("SetEditorValue(\'{0}\', \'{1}\', \'{2}\'); ",
							CiTable.TableName,
							columnName,
							columnValue);
					}
				}
			}

			return script;
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

			foreach (CiMacro ciMacro in ciMacros)
			{
				script += ciMacro.Run(drParams);
			}

			return script;
		}
    }
}