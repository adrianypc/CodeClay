using System;
using System.Data;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiTextField")]
    public class CiTextField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override Type GetNativeType(object fieldValue)
        {
            if (Mask == eTextMask.Currency)
            {
                return typeof(decimal);
            }

            return base.GetNativeType(fieldValue);
        }

        public override object GetNativeValue(object fieldValue)
        {
            fieldValue = base.GetNativeValue(fieldValue);

            if (Mask == eTextMask.Currency)
            {
                fieldValue = !MyUtils.IsEmpty(fieldValue)
                    ? Convert.ToDecimal(fieldValue)
                    : Convert.DBNull;
            }

            return fieldValue;
        }

        public override CardViewColumn CreateCardColumn(UiTable uiTable)
        {
            CardViewColumn dxColumn = base.CreateCardColumn(uiTable);
            
            return dxColumn;
        }

		public override void FormatCardColumn(CardViewColumn dxColumn, DataRow dr)
		{
			base.FormatCardColumn(dxColumn, dr);

			if (dxColumn != null)
			{
				switch (Mask)
				{
					case eTextMask.Currency:
						dxColumn.PropertiesEdit.DisplayFormatString = "f2";
						break;
				}
			}
		}

		public override void FormatGridColumn(GridViewDataColumn dxColumn, DataRow dr)
		{
			base.FormatGridColumn(dxColumn, dr);

			if (dxColumn != null)
			{
				switch (Mask)
				{
					case eTextMask.Currency:
						HorizontalAlign right = System.Web.UI.WebControls.HorizontalAlign.Right;

						dxColumn.PropertiesEdit.DisplayFormatString = "f2";
						dxColumn.CellStyle.HorizontalAlign = right;
						dxColumn.EditCellStyle.HorizontalAlign = right;
						dxColumn.HeaderStyle.HorizontalAlign = right;
						break;
				}
			}
		}
    }

    public partial class UiTextField : UiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public CiTextField CiTextField
        {
            get { return CiField as CiTextField; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            mEditor = dxTextBox;
            base.Page_Load(sender, e);

            if (CiField != null && !CiField.Enabled)
            {
                dxTextBox.Border.BorderColor = Color.LightGray;
            }

            if (CiTextField != null)
            {
                switch (CiTextField.Mask)
                {
                    case eTextMask.Currency:
                        dxTextBox.MaskSettings.Mask = "<0..99999g>.<00..99>";
                        dxTextBox.MaskSettings.IncludeLiterals = MaskIncludeLiteralsMode.DecimalSymbol;
                        dxTextBox.ValidationSettings.Display = Display.Dynamic;

                        if (CiTable.DefaultView == "Grid")
                        {
                            dxTextBox.HorizontalAlign = HorizontalAlign.Right;
                        }
                        break;
                }
            }
        }

        protected void dxTextPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh();
        }
    }
}