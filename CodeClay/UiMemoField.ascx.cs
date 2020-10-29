using System;
using System.Data;
using System.Drawing;
using System.Xml.Serialization;
using System.Web;
using System.Web.UI;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiMemoField")]
    public class CiMemoField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public bool AllowHtml { get; set; } = false;

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

        public bool AllowHtml
        {
            get
            {
                return (CiMemoField != null && CiMemoField.AllowHtml);
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            mEditor = dxMemo;
            base.Page_Load(sender, e);

            if (AllowHtml)
            {
                dxMemo.Visible = false;
                dxHtmlMemo.Visible = true;
            }
            else
            {
                dxMemo.Visible = true;
                dxHtmlMemo.Visible = false;
            }
        }

        protected void dxMemoPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh(sender, e);
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (override)
        // --------------------------------------------------------------------------------------------------

        public override void Refresh(object sender, CallbackEventArgsBase e = null)
        {
            if (AllowHtml)
            {
                if (CiMemoField != null)
                {
                    DataRow drParams = GetState();

                    CiTable ciTable = CiMemoField.CiTable;
                    string tableName = (ciTable != null) ? ciTable.TableName : "";
                    string fieldName = CiMemoField.IsSearching ? CiMemoField.SearchableID : CiMemoField.FieldName;
                    bool isEditable = CiMemoField.IsEditable(drParams);
                    object fieldValue = MyUtils.Coalesce(MyWebUtils.GetField(drParams, fieldName), CiMemoField.Value, "");

                    dxHtmlMemo.ID = fieldName;
                    dxHtmlMemo.DataBind();
                    dxHtmlMemo.Settings.AllowHtmlView = isEditable;
                    dxHtmlMemo.Settings.AllowDesignView = isEditable;
                    dxHtmlMemo.Settings.AllowPreview = !isEditable;
                    
                    dxHtmlMemo.ClientIDMode = ClientIDMode.Static;
                    dxHtmlMemo.Width = CiMemoField.EditorWidth;
                    dxHtmlMemo.Html = fieldValue.ToString();
                    dxHtmlMemo.Visible = CiMemoField.IsVisible(drParams);

                    dxHtmlMemo.JSProperties["cpHasFieldExitMacro"] = (CiMemoField.CiFieldExitMacros.Length > 0);
                    dxHtmlMemo.JSProperties["cpTableName"] = tableName;
                    dxHtmlMemo.JSProperties["cpFollowerFields"] = FollowerFieldNames;

                    ASPxCallbackPanel editorPanel = dxHtmlMemo.NamingContainer as ASPxCallbackPanel;
                    if (editorPanel != null)
                    {
                        editorPanel.ID = fieldName + "Panel";
                        editorPanel.JSProperties["cpTableName"] = tableName;
                        editorPanel.JSProperties["cpFieldName"] = fieldName;
                        editorPanel.JSProperties["cpItemIndex"] = ItemIndex;
                        editorPanel.ClientIDMode = ClientIDMode.Static;
                    }
                }
            }
            else
            {
                base.Refresh(sender, e);
            }
        }
    }
}