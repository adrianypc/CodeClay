using System;
using System.Web.UI;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiTimeField")]
    public class CiTimeField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        public CiTimeField()
        {
            if (!MyUtils.IsEmpty(Mask))
            {
                Mask = "h:mm tt";
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override Type GetNativeType(object fieldValue)
        {
            return typeof(DateTime);
        }

        public override object GetNativeValue(object fieldValue)
        {
            if (!MyUtils.IsEmpty(fieldValue))
            {
                DateTime dateValue = Convert.ToDateTime(fieldValue);
                if (dateValue.Year < 1000)
                {
                    dateValue = dateValue.AddYears(1800);
                }

                return dateValue;
            }

            return Convert.DBNull;
        }

        public override string GetUiPluginName()
        {
            if (!Enabled)
            {
                return "UiTextField";
            }

            return base.GetUiPluginName();
        }

        public override CardViewColumn CreateCardColumn(UiTable uiTable)
        {
            CardViewTimeEditColumn dxColumn = new CardViewTimeEditColumn();

            if (!MyUtils.IsEmpty(Mask))
            {
                dxColumn.PropertiesTimeEdit.DisplayFormatString = Mask;
            }

            if (uiTable != null)
            {
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

        public override GridViewDataColumn CreateGridColumn(UiTable uiTable)
        {
            GridViewDataTimeEditColumn dxColumn = new GridViewDataTimeEditColumn();

            if (!MyUtils.IsEmpty(Mask))
            {
                dxColumn.PropertiesTimeEdit.DisplayFormatString = Mask;
            }

            if (uiTable != null)
            {
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

        public override string ToString(object value)
        {
            string formattedValue = base.ToString(value);

            try
            {
                formattedValue = ((DateTime)value).ToShortTimeString();
            }
            catch
            {
                // Do nothing
            }

            return formattedValue;
        }
    }

    public partial class UiTimeField : UiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public virtual CiTimeField CiTimeField
        {
            get { return CiField as CiTimeField; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            mEditor = dxTimeBox;
            base.Page_Load(sender, e);

            mEditor.Attributes.Add("onkeypress", String.Format("dxTimeBox_KeyPress({0}, event);",
                mEditor.ClientInstanceName));

            if (CiTimeField != null)
            {
                dxTimeBox.DisplayFormatString = CiTimeField.Mask;
                dxTimeBox.EditFormatString = CiTimeField.Mask;
            }

            // Setup time value
            try
            {
                dxTimeBox.Value = null;
                if (CiField != null)
                {
                    dxTimeBox.Value = Convert.ToDateTime(this[CiField.FieldName]);
                }
            }
            catch
            {
                // Do nothing
            }
        }

        protected void dxTimePanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh(sender, e);
        }
    }
}