using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiRadioField")]
    public class CiRadioField : CiField
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

        [XmlElement("Code")]
        public string Code { get; set; } = "";

        [XmlElement("Description")]
        public string Description { get; set; } = "";

        [XmlElement("Columns")]
        public int Columns { get; set; } = 1;

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

    public partial class UiRadioField : UiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public virtual CiRadioField CiRadioField
        {
            get { return CiField as CiRadioField; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            mEditor = dxRadioBox;
            base.Page_Load(sender, e);

            if (CiRadioField != null)
            {
                dxRadioBox.RepeatColumns = CiRadioField.Columns;
            }
        }

        protected void dxRadioPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh();
        }

        protected void MyRadioData_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["Dropdown"] = this;
        }

        protected void MyRadioData_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            DataTable dt = e.ReturnValue as DataTable;

            if (dt != null && dt.Columns.Count > 0)
            {
                string valueField = "";
                string textField = "";
                string firstColumnName = dt.Columns[0].ColumnName;

                if (CiRadioField != null)
                {
                    valueField = CiRadioField.Code;
                    textField = CiRadioField.Description;
                }

                dxRadioBox.ValueField = MyUtils.IsEmpty(valueField)
                    ? firstColumnName
                    : valueField;

                dxRadioBox.TextField = MyUtils.IsEmpty(textField)
                    ? firstColumnName
                    : textField;
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override Color GetBackColor(bool isEditable)
        {
            return Color.Transparent;
        }
    }
}