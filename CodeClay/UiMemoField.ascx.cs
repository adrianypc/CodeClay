using System;

// Extra references
using DevExpress.Web;
using System.Xml.Serialization;

namespace CodeClay
{
    [XmlType("CiMemoField")]
    public class CiMemoField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

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
            CardViewMemoColumn dxColumn = new CardViewMemoColumn();

            if (uiTable != null)
            {
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

        public override GridViewDataColumn CreateGridColumn(UiTable uiTable)
        {
            GridViewDataMemoColumn dxColumn = new GridViewDataMemoColumn();

            if (uiTable != null)
            {
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }
    }

    public partial class UiMemoField : UiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public CiMemoField CiMemoField
        {
            get { return CiField as CiMemoField; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            mEditor = dxMemo;
            base.Page_Load(sender, e);
        }

        protected void dxMemoPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh();
        }
    }
}