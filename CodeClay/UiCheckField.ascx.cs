using System;
using System.Drawing;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiCheckField")]
    public class CiCheckField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override Type GetNativeType(object fieldValue)
        {
            return typeof(bool);
        }

        public override object GetNativeValue(object fieldValue)
        {
            if (MyUtils.IsEmpty(fieldValue))
            {
                fieldValue = Convert.DBNull;
            }
            else if (fieldValue.Equals("0"))
            {
                fieldValue = false;
            }
            else if (fieldValue.Equals("1"))
            {
                fieldValue = true;
            }
            else
            {
                fieldValue = Convert.ToBoolean(fieldValue);
            }

            return fieldValue;
        }

        public override CardViewColumn CreateCardColumn(UiTable uiTable)
        {
            CardViewCheckColumn dxColumn = new CardViewCheckColumn();
            
            if (uiTable != null)
            {
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

        public override GridViewDataColumn CreateGridColumn(UiTable uiTable)
        {
            GridViewDataCheckColumn dxColumn = new GridViewDataCheckColumn();

            if (uiTable != null)
            {
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

		public override bool HasBorder(UiTable uiTable)
		{
			return false;
		}
	}

    public partial class UiCheckField : UiField
    {
        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            mEditor = dxCheckBox;
            base.Page_Load(sender, e);

            // Setup checkbox value
            if (!MyUtils.IsEmpty(FieldValue))
            {
                try
                {
                    dxCheckBox.Checked = Convert.ToBoolean(FieldValue);
                }
                catch
                {
                    // Do nothing
                }
            }
        }

        protected void dxCheckPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh();
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