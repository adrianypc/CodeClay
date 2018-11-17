using System;
using System.Web.UI.WebControls;

// Extra references
using DevExpress.Web;
using System.Xml;
using System.Xml.Serialization;

namespace CodeClay
{
    [XmlType("CiNumericField")]
    public class CiNumericField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("MinValue")]
        public string MinValue { get; set; } = "";

        [XmlElement("MaxValue")]
        public string MaxValue { get; set; } = "";

		// --------------------------------------------------------------------------------------------------
		// Methods (Override)
		// --------------------------------------------------------------------------------------------------

		public override void FormatGridColumn(GridViewDataColumn dxColumn)
		{
			base.FormatGridColumn(dxColumn);

			if (dxColumn != null)
			{
				HorizontalAlign right = System.Web.UI.WebControls.HorizontalAlign.Right;

				dxColumn.CellStyle.HorizontalAlign = right;
				dxColumn.EditCellStyle.HorizontalAlign = right;
				dxColumn.HeaderStyle.HorizontalAlign = right;
			}
		}
    }

    public partial class UiNumericField : UiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public CiNumericField CiNumericField
        {
            get { return CiField as CiNumericField; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            mEditor = dxSpinEdit;
            base.Page_Load(sender, e);

            dxSpinEdit.HorizontalAlign = HorizontalAlign.Left;

            if (CiNumericField != null)
            {
                decimal minValue;
                decimal maxValue;

                if (Decimal.TryParse(CiNumericField.MinValue, out minValue))
                {
                    dxSpinEdit.MinValue = minValue;
                }

                if (Decimal.TryParse(CiNumericField.MaxValue, out maxValue))
                {
                    dxSpinEdit.MaxValue = maxValue;
                }
            }
        }

        protected void dxSpinEditPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh();
        }
    }
}