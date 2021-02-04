using System;

// Extra references
using CodistriCore;
using DevExpress.Web;
using System.Xml.Serialization;

namespace CodeClay
{
    [XmlType("CiTimeField")]
    public class CiTimeField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override Type GetNativeType(object fieldValue)
        {
            return typeof(DateTime);
        }

        public override object GetNativeValue(object fieldValue)
        {
            return !MyUtils.IsEmpty(fieldValue)
                ? Convert.ToDateTime(fieldValue)
                : Convert.DBNull;
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
            CardViewDateColumn dxColumn = new CardViewDateColumn();

            if (uiTable != null)
            {
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

        public override GridViewDataColumn CreateGridColumn(UiTable uiTable)
        {
            GridViewDataDateColumn dxColumn = new GridViewDataDateColumn();

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
                formattedValue = ((DateTime)value).ToShortDateString();
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

            // Setup date value
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