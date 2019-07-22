using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using WC = System.Web.UI.WebControls;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiButtonField")]
    public class CiButtonField: CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (Derived)
        // --------------------------------------------------------------------------------------------------

        public CiMacro CiFirstMacro
        {
            get
            {
                CiMacro[] ciMacros = CiMacros;

                if (ciMacros != null && ciMacros.Length > 0)
                {
                    return ciMacros[0];
                }

                return null;
            }
        }

        [XmlIgnore]
        public CiMacro[] CiMacros
        {
            get
            {
                return CiPlugins.Where(c => (c as CiMacro) != null).Select(c => c as CiMacro).ToArray();
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Overrides)
        // --------------------------------------------------------------------------------------------------

        public override CardViewColumn CreateCardColumn(UiTable uiTable)
		{
			CardViewColumn dxColumn = new CardViewColumn();

			if (uiTable != null)
			{
				dxColumn.DataItemTemplate = uiTable.CreateTemplate(this);
				dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
			}

			return dxColumn;
		}

		public override GridViewDataColumn CreateGridColumn(UiTable uiTable)
		{
			GridViewDataColumn dxColumn = new GridViewDataColumn();

			if (uiTable != null)
			{
				dxColumn.DataItemTemplate = uiTable.CreateTemplate(this);
				dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
			}

			return dxColumn;
		}

        public override void FormatGridColumn(GridViewDataColumn dxColumn)
		{
			if (dxColumn != null)
			{
				dxColumn.Caption = " ";
                dxColumn.CellStyle.HorizontalAlign = WC.HorizontalAlign.Center;
                dxColumn.CellStyle.VerticalAlign = WC.VerticalAlign.Top;
                dxColumn.EditCellStyle.HorizontalAlign = WC.HorizontalAlign.Center;
                dxColumn.EditCellStyle.VerticalAlign = WC.VerticalAlign.Top;
            }
        }

		public override bool HasBorder(UiTable uiTable)
		{
			return false;
		}
	}

	public partial class UiButtonField : UiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

		public virtual CiButtonField CiButtonField
        {
			get { return CiPlugin as CiButtonField; }
            set { CiPlugin = value; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            if (CiButtonField != null)
            {
                string buttonText = CiButtonField.Caption;

                CiMacro ciMacro = CiButtonField.CiFirstMacro;
                if (ciMacro != null)
                {
                    buttonText = ciMacro.Caption;
                }

                dxButton.Text = buttonText;

                string fieldName = CiButtonField.FieldName;
                dxButton.JSProperties["cpFieldName"] = fieldName;
                ASPxCallbackPanel editorPanel = dxButton.NamingContainer as ASPxCallbackPanel;
                if (editorPanel != null)
                {
                    editorPanel.JSProperties["cpFieldName"] = fieldName;
                }
            }
        }

        protected void dxButtonPanel_Callback(object source, CallbackEventArgsBase e)
        {
            ASPxCallbackPanel dxButtonPanel = source as ASPxCallbackPanel;

            if (dxButtonPanel != null)
            {
                dxButtonPanel.JSProperties["cpScript"] = RunMacro(e.Parameter);
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Overrides)
        // --------------------------------------------------------------------------------------------------

        public override void ConfigureIn(Control container)
        {
            base.ConfigureIn(container);

            if (UiTable != null)
            {
                CiTable ciTable = CiButtonField.CiTable;
                string tableName = (ciTable != null) ? ciTable.TableName : "";
                string fieldName = CiButtonField.FieldName;

                dxButton.Enabled = !UiTable.IsEditing;
                dxButton.JSProperties["cpItemIndex"] = ItemIndex;
                dxButton.JSProperties["cpTableName"] = tableName;
                dxButton.JSProperties["cpFieldName"] = fieldName;

                ASPxCallbackPanel editorPanel = dxButton.NamingContainer as ASPxCallbackPanel;
                if (editorPanel != null)
                {
                    editorPanel.JSProperties["cpTableName"] = tableName;
                    editorPanel.JSProperties["cpFieldName"] = fieldName;
                    editorPanel.JSProperties["cpItemIndex"] = ItemIndex;
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private string RunMacro(string macroString)
        {
            string script = "";
            CiButtonField ciButtonField = null;
            int itemIndex = -1;

            if (IsUnbound)
            {
                ciButtonField = CiButtonField;
            }
            else if (!MyUtils.IsEmpty(macroString))
            {
                string[] parameters = macroString.Split(LIST_SEPARATOR.ToCharArray());

                if (parameters.Length == 2)
                {
                    string buttonText = parameters[0];
                    string itemIndexString = parameters[1];

                    try
                    {
                        itemIndex = Convert.ToInt32(itemIndexString);
                    }
                    catch
                    {
                        // Do nothing
                    }

                    ciButtonField = CiTable.GetField(buttonText) as CiButtonField;
                }
            }

            if (ciButtonField != null)
            {
                CiMacro ciMacro = ciButtonField.CiFirstMacro;
                if (ciMacro != null)
                {
                    ciMacro.Run(GetState(itemIndex));
                    script = ciMacro.ResultScript;
                }
            }

            return script;
        }
    }
}