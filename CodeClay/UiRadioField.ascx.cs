﻿using System;
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
            Refresh(sender, e);
        }

        protected void MyRadioData_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["Dropdown"] = this;
        }

        protected void MyRadioData_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            DataTable dt = e.ReturnValue as DataTable;
            int columnCount = dt.Columns.Count;

            if (dt != null && columnCount > 0)
            {
                string valueField = dt.Columns[0].ColumnName;
                string textField = (columnCount > 1) ? dt.Columns[1].ColumnName : valueField;

                dxRadioBox.ValueField = valueField;
                dxRadioBox.TextField = textField;
            }
        }
    }
}