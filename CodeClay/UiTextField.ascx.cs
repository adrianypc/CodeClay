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

        public override CardViewColumn CreateCardColumn(UiTable uiTable)
        {
            CardViewColumn dxColumn = base.CreateCardColumn(uiTable);
            
            return dxColumn;
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
            if (mEditor == null)
            {
                mEditor = dxTextBox;
            }

            base.Page_Load(sender, e);

            if (CiField != null && !CiField.Enabled)
            {
                dxTextBox.Border.BorderColor = Color.LightGray;
            }
        }

        protected void dxTextPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh(sender, e);
        }
    }
}