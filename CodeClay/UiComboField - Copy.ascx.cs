using System;
using System.Collections;
using System.Data;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiComboField")]
    public class CiComboField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Member Variables
        // --------------------------------------------------------------------------------------------------

        private string mDataSource = "";

        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("DropdownSQL")]
        public string DropdownSQL { get; set; } = "";

        [XmlElement("InsertSQL")]
        public string InsertSQL { get; set; } = "";

        [XmlElement("DropdownWidth")]
        public int DropdownWidth { get; set; } = 100;

        [XmlAnyElement("DataSource")]
        public XmlElement DataSource
        {
            get
            {
                var x = new XmlDocument();
                x.LoadXml(string.Format("<DataSource>{0}</DataSource>", mDataSource));
                return x.DocumentElement;
            }

            set { mDataSource = value.InnerXml; }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override ArrayList GetLeaderFieldNames()
        {
            ArrayList sqlParameters = base.GetLeaderFieldNames();

            sqlParameters.AddRange(MyUtils.GetParameters(DropdownSQL));
            sqlParameters.AddRange(MyUtils.GetParameters(InsertSQL));

            return sqlParameters; ;
        }
    }

    public partial class UiComboField : UiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public DataTable DataSource { get; set; } = null;

        public CiComboField CiComboField
        {
            get { return CiField as CiComboField; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            mEditor = dxComboBox;
            base.Page_Load(sender, e);

            mEditor.Attributes.Add("onkeypress", String.Format("dxComboBox_KeyPress({0}, event);",
                mEditor.ClientInstanceName));

            // Setup combo columns
            dxComboBox.Columns.Clear();

			if (CiComboField != null)
			{
				dxComboBox.DropDownWidth = CiComboField.DropdownWidth;

				dxComboBox.DropDownStyle = MyUtils.IsEmpty(CiComboField.InsertSQL)
					? DropDownStyle.DropDownList
					: DropDownStyle.DropDown;

                dxComboBox.JSProperties["cpTextFieldName"] = CiComboField.TextFieldName;
			}

            if (DataSource != null)
            {
                dxComboBox.Columns.Clear();

                string valueField = "";
                string textField = "";
                DataColumnCollection columns = DataSource.Columns;

                if (CiComboField != null && columns != null && columns.Count >= 2)
                {
                    valueField = columns[0].ColumnName;
                    textField = columns[1].ColumnName;
                }

                int textColumnIndex = 0;
                foreach (DataColumn dc in columns)
                {
                    string columnName = dc.ColumnName;
                    if (columnName != "RowKey")
                    {
                        dxComboBox.Columns.Add(columnName);
                        if (columnName == textField)
                        {
                            dxComboBox.TextFormatString = string.Format("{{{0}}}", textColumnIndex);
                        }
                        else
                        {
                            textColumnIndex++;
                        }
                    }
                }

                int columnCount = dxComboBox.Columns.Count;
                if (columnCount > 0)
                {
                    dxComboBox.ValueField = MyUtils.IsEmpty(valueField)
                        ? dxComboBox.Columns[0].FieldName
                        : valueField;

                    dxComboBox.TextField = MyUtils.IsEmpty(textField)
                        ? dxComboBox.Columns[0].FieldName
                        : textField;

                    for (int i = 0; i < columnCount - 1; i++)
                    {
                        dxComboBox.Columns[i].Width = Unit.Pixel(50);
                    }
                }
            }

            // Select item in combobox, and cast from NON-STRING to STRING
            object selectedValue = MyUtils.Coalesce(dxComboBox.Value, "");
            dxComboBox.SelectedIndex = -1;
            dxComboBox.Value = selectedValue.ToString();
        }

        protected void dxComboPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh(sender, e);            
        }

        protected void MyComboData_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["Dropdown"] = this;
        }

        protected void MyComboData_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            this.DataSource = e.ReturnValue as DataTable;
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (override)
        // --------------------------------------------------------------------------------------------------

        public override void Refresh(object sender, CallbackEventArgsBase e = null)
        {
            base.Refresh(sender, e);

            ASPxCallbackPanel dxComboPanel = sender as ASPxCallbackPanel;
            if (dxComboPanel != null)
            {
                DataRow drParams = GetState();

                string fieldName = CiField.FieldName;
                string fieldValue = MyWebUtils.GetStringField(drParams, fieldName);

                if (e != null)
                {
                    string[] nameValuePairs = e.Parameter.Split(LIST_SEPARATOR.ToCharArray());

                    if (nameValuePairs.Length > 1)
                    {
                        fieldName = nameValuePairs[0];
                        fieldValue = nameValuePairs[1];

                        drParams[fieldName] = fieldValue;
                    }
                }

                ASPxComboBox dxComboBox = dxComboPanel.FindControl(fieldName) as ASPxComboBox;
                if (dxComboBox != null)
                {
                    dxComboBox.Text = fieldValue;
                    if (dxComboBox.Items.FindByTextWithTrim(fieldValue) == null)
                    {
                        string insertSQL = CiComboField.InsertSQL;

                        if (!MyUtils.IsEmpty(insertSQL))
                        {
                            MyWebUtils.EvalSQL(insertSQL, drParams);
                        }
                    }
                }
            }
        }
    }
}