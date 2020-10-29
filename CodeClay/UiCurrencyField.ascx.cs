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
    [XmlType("CiCurrencyField")]
    public class CiCurrencyField : CiTextField
    {
        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override Type GetNativeType(object fieldValue)
        {
            return typeof(decimal);
        }

        public override object GetNativeValue(object fieldValue)
        {
            return !MyUtils.IsEmpty(fieldValue)
                    ? Convert.ToDecimal(fieldValue)
                    : Convert.DBNull; ;
        }

        public override void FormatCardColumn(CardViewColumn dxColumn, DataRow dr)
		{
			base.FormatCardColumn(dxColumn, dr);

			if (dxColumn != null)
			{
                dxColumn.PropertiesEdit.DisplayFormatString = "f2";
            }
        }

		public override void FormatGridColumn(GridViewDataColumn dxColumn, DataRow dr)
		{
			base.FormatGridColumn(dxColumn, dr);

			if (dxColumn != null)
			{
                HorizontalAlign right = System.Web.UI.WebControls.HorizontalAlign.Right;

                dxColumn.PropertiesEdit.DisplayFormatString = "f2";
                dxColumn.CellStyle.HorizontalAlign = right;
                dxColumn.EditCellStyle.HorizontalAlign = right;
                dxColumn.HeaderStyle.HorizontalAlign = right;
            }
        }
    }

    public partial class UiCurrencyField : UiTextField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public CiCurrencyField CiCurrencyField
        {
            get { return CiField as CiCurrencyField; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            if (mEditor == null)
            {
                mEditor = dxCurrencyBox;
            }

            base.Page_Load(sender, e);

            if (CiCurrencyField != null)
            {
                dxCurrencyBox.MaskSettings.Mask = "<0..99999g>.<00..99>";
                dxCurrencyBox.MaskSettings.IncludeLiterals = MaskIncludeLiteralsMode.DecimalSymbol;
                dxCurrencyBox.ValidationSettings.Display = Display.Dynamic;

                if (CiTable.DefaultView == "Grid")
                {
                    dxCurrencyBox.HorizontalAlign = HorizontalAlign.Right;
                }
            }
        }

        protected void dxCurrencyPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh(sender, e);
        }
    }
}