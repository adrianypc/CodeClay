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
        public int MinValue { get; set; } = 0;

        [XmlElement("MaxValue")]
        public int MaxValue { get; set; } = int.MaxValue;

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
                dxSpinEdit.MinValue = Convert.ToDecimal(CiNumericField.MinValue);
                dxSpinEdit.MaxValue = Convert.ToDecimal(CiNumericField.MaxValue);
            }
        }

        protected void dxSpinEditPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh();
        }
    }
}