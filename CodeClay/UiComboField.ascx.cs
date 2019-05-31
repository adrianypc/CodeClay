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

        [XmlElement("Code")]
        public string Code { get; set; } = "";

        [XmlElement("Description")]
        public string Description { get; set; } = "";

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

            // Setup combo columns
            dxComboBox.Columns.Clear();

			if (CiComboField != null)
			{
				dxComboBox.DropDownWidth = CiComboField.DropdownWidth;

				dxComboBox.DropDownStyle = MyUtils.IsEmpty(CiComboField.InsertSQL)
					? DropDownStyle.DropDownList
					: DropDownStyle.DropDown;

                dxComboBox.JSProperties["cpAlternateName"] = CiComboField.TextFieldName;
			}

            if (DataSource != null)
            {
                dxComboBox.Columns.Clear();

                string valueField = "";
                string textField = "";

                if (CiComboField != null)
                {
                    valueField = CiComboField.Code;
                    textField = CiComboField.Description;
                }

                int textColumnIndex = 0;
                foreach (DataColumn dc in DataSource.Columns)
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

                if (dxComboBox.Columns.Count > 0)
                {
                    dxComboBox.ValueField = MyUtils.IsEmpty(valueField)
                        ? dxComboBox.Columns[0].FieldName
                        : valueField;

                    dxComboBox.TextField = MyUtils.IsEmpty(textField)
                        ? dxComboBox.Columns[0].FieldName
                        : textField;
                }
            }

            // Select item in combobox, and cast from NON-STRING to STRING
            object selectedValue = MyUtils.Coalesce(dxComboBox.Value, "");
            dxComboBox.SelectedIndex = -1;
            dxComboBox.Value = selectedValue.ToString();
        }

        protected void dxComboBox_TextChanged(object sender, EventArgs e)
		{
			if (CiComboField != null)
			{
				ASPxComboBox dxComboBox = sender as ASPxComboBox;
				if (dxComboBox != null)
				{
					string value = dxComboBox.Text;
					if (dxComboBox.Items.FindByTextWithTrim(value) == null)
					{
						string insertSQL = CiComboField.InsertSQL;

						if (!MyUtils.IsEmpty(insertSQL))
						{
							DataRow drParams = GetState();
							MyWebUtils.EvalSQL(insertSQL, drParams);
						}
					}
				}
			}
		}

        protected void dxComboPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh();
        }

        protected void MyComboData_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["Dropdown"] = this;
        }

        protected void MyComboData_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            this.DataSource = e.ReturnValue as DataTable;
        }
    }
}