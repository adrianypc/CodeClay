using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Data;
using DevExpress.Web;

namespace CodeClay
{
	[XmlType("CiField")]
    public class CiField : CiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Member Variables
        // --------------------------------------------------------------------------------------------------

		private bool mSearchable = false;

        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("FieldName")]
        public string FieldName { get; set; } = Guid.NewGuid().ToString();

        [XmlElement("TextFieldName")]
        public string TextFieldName { get; set; } = "";

        [XmlElement("Caption")]
        public string Caption { get; set; } = "";

        [XmlElement("Mask")]
		public eTextMask Mask { get; set; } = eTextMask.None;

        [XmlElement("ForeColor")]
		public string ForeColor { get; set; } = "";

        [XmlElement("Hidden")]
        public bool Hidden { get; set; } = false;

        [XmlAnyElement("Editable")]
        public XmlElement Editable { get; set; } = MyWebUtils.CreateXmlElement("Editable", true);

        [XmlElement("EditableSQL")]
        public string EditableSQL { get; set; } = "";

        [XmlElement("Computed")]
        public bool Computed { get; set; } = false;

        [XmlElement("RowSpan")]
        public int RowSpan { get; set; } = 1;

        [XmlElement("ColSpan")]
        public int ColSpan { get; set; } = 1;

        [XmlElement("Width")]
        public int Width { get; set; } = 0;

        [XmlElement("HorizontalAlign")]
        public FormLayoutHorizontalAlign HorizontalAlign { get; set; } = FormLayoutHorizontalAlign.Left;

        [XmlElement("VerticalAlign")]
        public FormLayoutVerticalAlign VerticalAlign { get; set; } = FormLayoutVerticalAlign.Top;

        [XmlElement("Summary")]
        public SummaryItemType Summary { get; set; } = SummaryItemType.None;

        [XmlElement("Searchable")]
        public bool Searchable
        {
            get { return mSearchable; }
            set
            {
                mSearchable = value;
                if (mSearchable)
                {
                    Hidden = true;
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Properties (Derived)
        // --------------------------------------------------------------------------------------------------

        [XmlIgnore]
        public bool IsLabel
        {
            get { return GetType() == typeof(CiField); }
        }

        [XmlIgnore]
        public bool Visible
        {
            get
            {
                bool isSearching = (CiTable != null)
                    ? (CiTable.IsSearching)
                    : false;

                return !isSearching && !Hidden || isSearching && Searchable;
            }
        }

        [XmlIgnore]
        public Unit ColumnWidth
        {
            get
            {
                Unit columnWidth = Unit.Percentage(Width);

                if (CiTable != null)
                {
                    if (CiTable.IsSearching)
                    {
                        columnWidth = Unit.Percentage(100);
                    }
                    else if (CiTable.DefaultView == "Card")
                    {
                        if (Width == 0 && ColSpan == 1)
                        {
                            int colCount = Visible ? CiTable.ColCount : 1;
                            columnWidth = Unit.Percentage(100 / colCount);
                        }
                    }
                    else if (CiTable.DefaultView == "Grid")
                    {
                        if (Width == 0)
                        {
                            columnWidth = Unit.Empty;
                        }
                    }
                }

                return columnWidth;
            }
        }

        [XmlIgnore]
        public Unit EditorWidth
        {
            get
            {
                Unit editorWidth = Unit.Percentage(100);
                bool isSearching = (CiTable != null && CiTable.IsSearching);
                bool hasLayout = (CiTable != null && CiTable.LayoutXml != null);
                bool isCardView = (CiTable != null && CiTable.DefaultView == "Card");

                if (isSearching)
                {
                    editorWidth = Unit.Percentage(50);
                }
                else if (Width > 0 && !hasLayout && isCardView)
                {
                    editorWidth = Unit.Percentage(Width);
                }

                return editorWidth;
            }
        }

        [XmlIgnore]
        public CiFieldExitMacro[] CiFieldExitMacros
        {
            get
            {
                if (CiTable != null)
                {
                    return CiTable.GetFieldExitMacros(FieldName);
                }

                return new CiFieldExitMacro[] { };
            }
        }

        [XmlIgnore]
        public CiTable CiTable
        {
            get { return CiParentPlugin as CiTable; }
            set { CiParentPlugin = value; }
        }

        [XmlIgnore]
        public CiField[] CiFollowerFields
        {
            get
            {
                ArrayList ciFollowerFields = new ArrayList();
                string leaderFieldName = FieldName;
                CiTable ciTable = CiTable;

                if (ciTable != null)
                {
                    foreach (CiField ciField in ciTable.CiFields)
                    {
                        if (ciField.GetLeaderFieldNames().Contains(leaderFieldName))
                        {
                            // These fields are affected by changes in the leader's value
                            ciFollowerFields.Add(ciField);
                        }
                    }
                }

                return (CiField[])ciFollowerFields.ToArray(typeof(CiField));
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual ArrayList GetLeaderFieldNames()
        {
            return MyUtils.GetParameters(EditableSQL);
        }

        public virtual CardViewColumn CreateCardColumn(UiTable uiTable)
        {
            CardViewColumn dxColumn = new CardViewTextColumn();
            dxColumn.PropertiesEdit.EncodeHtml = !IsLabel;

            if (uiTable != null)
            {
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

        public virtual GridViewDataColumn CreateGridColumn(UiTable uiTable)
        {
            GridViewDataColumn dxColumn = new GridViewDataTextColumn();
            dxColumn.PropertiesEdit.EncodeHtml = !IsLabel;

            if (uiTable != null)
            {
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

        public virtual void FormatCardColumn(CardViewColumn dxColumn)
		{
			if (dxColumn != null)
			{
				string fieldName = dxColumn.FieldName;

				dxColumn.Visible = Visible;
				dxColumn.Caption = Caption;
				dxColumn.Width = ColumnWidth;

				if (Visible)
				{
					CardViewColumnLayoutItem dxLayoutItem = new CardViewColumnLayoutItem();
					dxLayoutItem.Name = fieldName;
					dxLayoutItem.Visible = Visible;
					dxLayoutItem.Caption = (Caption == "*") ? "" : Caption;
					dxLayoutItem.ColumnName = fieldName;
					dxLayoutItem.RowSpan = RowSpan;
					dxLayoutItem.ColSpan = ColSpan;
					dxLayoutItem.Width = ColumnWidth;
					dxLayoutItem.HorizontalAlign = HorizontalAlign;
					dxLayoutItem.VerticalAlign = VerticalAlign;
					dxLayoutItem.ShowCaption = !MyUtils.IsEmpty(Caption)
						? DevExpress.Utils.DefaultBoolean.True
						: DevExpress.Utils.DefaultBoolean.False;
					dxLayoutItem.NestedControlCellStyle.CssClass =
						HasBorder(dxColumn.CardView.NamingContainer as UiTable)
						? "cssFieldBorderStyle"
						: "";
					dxColumn.CardView.CardLayoutProperties.Items.Add(dxLayoutItem);
				}

				DevExpress.Data.SummaryItemType summaryItemType = Summary;
				if (summaryItemType != DevExpress.Data.SummaryItemType.None)
				{
					ASPxCardViewSummaryItem item = new ASPxCardViewSummaryItem(FieldName, summaryItemType);

					item.FieldName = FieldName;
					dxColumn.CardView.TotalSummary.Add(item);
				}
			}
		}

		public virtual void FormatGridColumn(GridViewDataColumn dxColumn)
		{
			if (dxColumn != null)
			{
				HorizontalAlign left = System.Web.UI.WebControls.HorizontalAlign.Left;
				VerticalAlign top = System.Web.UI.WebControls.VerticalAlign.Top;

				dxColumn.Visible = Visible;
				dxColumn.Caption = Caption;
				dxColumn.CellStyle.HorizontalAlign = left;
				dxColumn.CellStyle.VerticalAlign = top;
				dxColumn.EditCellStyle.VerticalAlign = top;
				dxColumn.EditCellStyle.HorizontalAlign = left;
				dxColumn.HeaderStyle.HorizontalAlign = left;
				dxColumn.CellRowSpan = RowSpan;
				dxColumn.Width = ColumnWidth;

				DevExpress.Data.SummaryItemType summaryItemType = Summary;
				if (summaryItemType != DevExpress.Data.SummaryItemType.None)
				{
					ASPxSummaryItem item = new ASPxSummaryItem(FieldName, summaryItemType);

					item.ShowInColumn = FieldName;
					dxColumn.Grid.TotalSummary.Add(item);
				}
			}
		}

		public virtual bool HasBorder(UiTable uiTable)
		{
            if (uiTable != null)
            {
                CiTable ciTable = uiTable.CiTable;
                if (ciTable != null)
                {
                    return (GetType() != typeof(CiField)) && !uiTable.IsEditing && !ciTable.IsSearching;
                }
            }

            return false;
        }

        public virtual string ToString(object value)
        {
            return MyUtils.Coalesce(value, "").ToString();
        }
    }

    public partial class UiField : UiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Member Variables
        // --------------------------------------------------------------------------------------------------

        protected ASPxEditBase mEditor = null;

        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public bool IsItemEditing { get; set; } = false;

        public int ItemIndex { get; set; } = -1;

        public object FieldValue
        {
            get
            {
                object fieldValue = null;

                if (CiField != null)
                {
                    string fieldName = CiField.FieldName;

                    GridBaseTemplateContainer container = this.NamingContainer as GridBaseTemplateContainer;
                    if (container != null && !CiField.Computed)
                    {
                        try
                        {
                            fieldValue = DataBinder.Eval(container.DataItem, fieldName);
                        }
                        catch
                        {
                            // Do nothing
                        }
                    }

                    if (!CiField.Enabled && (CiField.GetType().Name.Substring(2)) != CiField.GetUiPluginName().Substring(2))
                    {
                        // For disabled fields which have been cast to TextField
                        fieldValue = CiField.ToString(fieldValue);
                    }

                    fieldValue = MyUtils.Coalesce(fieldValue, CiField.DefaultValue);
                }

                return fieldValue;
            }
        }

        public virtual bool IsUnbound
        {
            get { return UiTable == null && CiField != null; }
        }

        public string FollowerFieldNames
        {
            get
            {
                string followerFieldNames = "";

                if (CiField != null)
                {
                    int i = 0;
                    foreach (CiField ciFollowerField in CiField.CiFollowerFields)
                    {
                        if (i++ > 0)
                        {
                            followerFieldNames += LIST_SEPARATOR;
                        }

                        CiTable ciTable = ciFollowerField.CiTable;
                        string tableName = (ciTable != null) ? ciTable.TableName : "";
                        followerFieldNames += tableName + "." + ciFollowerField.FieldName;
                        if (ItemIndex >= 0)
                        {
                            followerFieldNames += "." + ItemIndex.ToString();
                        }
                    }
                }

                return followerFieldNames;
            }
        }

        public CiField CiField
        {
            get { return CiPlugin as CiField; }
            set { CiPlugin = value; }
        }

        public CiTable CiTable
        {
            get
            {
                if (CiField != null)
                {
                    return CiField.CiTable;
                }

                return null;
            }
        }

        public UiTable UiTable
        {
            get { return UiParentPlugin as UiTable; }
			set { UiParentPlugin = value; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            if (mEditor == null)
            {
                mEditor = dxLabel;
            }

            Refresh();
        }

        protected void dxLabelPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh();
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override DataRow GetState(int rowIndex = -1)
        {
            if (UiTable != null)
            {
                return UiTable.GetState(rowIndex);
            }

            return base.GetState(rowIndex);
        }

        public override void ConfigureIn(Control container)
        {
            IsItemEditing = (CiField != null && CiField.Enabled);

            base.ConfigureIn(container);

            if (UiTable != null)
            {
                if (UiTable.IsCardView)
                {
                    CardViewBaseTemplateContainer cardContainer = container as CardViewBaseTemplateContainer;
                    if (cardContainer != null)
                    {
                        ASPxCardView dxCard = cardContainer.CardView;
                        if (dxCard != null)
                        {
                            if (dxCard.IsNewCardEditing)
                            {
                                IsItemEditing = cardContainer.ItemIndex < 0;
                            }
                            else
                            {
                                IsItemEditing = UiTable.IsEditing && cardContainer.ItemIndex == dxCard.FocusedCardIndex;
                            }
                        }

                        ItemIndex = cardContainer.ItemIndex;
                    }
                }
                else if (UiTable.IsGridView)
                {
                    GridViewBaseTemplateContainer gridContainer = container as GridViewBaseTemplateContainer;
                    if (gridContainer != null)
                    {
                        ASPxGridView dxGrid = gridContainer.Grid;
                        if (dxGrid != null)
                        {
                            if (dxGrid.IsNewRowEditing)
                            {
                                IsItemEditing = gridContainer.ItemIndex < 0;
                            }
                            else
                            {
                                IsItemEditing = UiTable.IsEditing && gridContainer.ItemIndex == dxGrid.FocusedRowIndex;
                            }

                            ItemIndex = gridContainer.ItemIndex;
                        }
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual void Refresh()
        {
            if (mEditor != null && CiField != null)
            {
                DataRow drParams = (UiTable != null) ? UiTable.GetState() : null;

                CiTable ciTable = CiField.CiTable;
                string tableName = (ciTable != null) ? ciTable.TableName : "";
                string fieldName = CiField.FieldName;
                bool isEditable = CiField.Enabled && MyWebUtils.Eval<bool>(CiField.Editable, drParams);
                object fieldValue = MyUtils.Coalesce(mEditor.Value, FieldValue);
				string foreColor = CiField.ForeColor;

                mEditor.ID = fieldName;
                mEditor.DataBind();
                mEditor.ReadOnly = !isEditable;
                mEditor.Caption = InContainer() ? "" : CiField.Caption;
                mEditor.ClientIDMode = ClientIDMode.Static;
                mEditor.CssClass = "css" + fieldName;
                mEditor.Width = CiField.EditorWidth;
                mEditor.Value = fieldValue;
                mEditor.Visible = CiField.Visible;
                mEditor.BackColor = (!CiField.IsLabel && isEditable)
                    ? Color.PaleGoldenrod
                    : Color.Transparent;
				mEditor.JSProperties["cpHasFieldExitMacro"] = (CiField.CiFieldExitMacros.Length > 0);
				
				if (!MyUtils.IsEmpty(foreColor))
				{
					mEditor.ForeColor = Color.FromName(foreColor);
				}

                mEditor.JSProperties["cpTableName"] = tableName;
                mEditor.JSProperties["cpFollowerFields"] = FollowerFieldNames;

                ASPxCallbackPanel editorPanel = mEditor.NamingContainer as ASPxCallbackPanel;
                if (editorPanel != null)
                {
                    editorPanel.ID = fieldName + "Panel";
                    editorPanel.JSProperties["cpTableName"] = tableName;
                    editorPanel.JSProperties["cpFieldName"] = fieldName;
                    editorPanel.JSProperties["cpItemIndex"] = ItemIndex;
                    editorPanel.ClientIDMode = ClientIDMode.Static;
                }

                // Setup event handlers
                if (mEditor != null)
                {
                    string eventHandlerName = mEditor.ClientInstanceName + "_Init";
                    mEditor.SetClientSideEventHandler("Init", FormatEvent(eventHandlerName));

                    if (mEditor.GetType() != typeof(ASPxLabel))
                    {
                        eventHandlerName = mEditor.ClientInstanceName + "_ValueChanged";
                        mEditor.SetClientSideEventHandler("ValueChanged", FormatEvent(eventHandlerName));

						eventHandlerName = "dxField_KeyPress";
						mEditor.SetClientSideEventHandler("KeyPress", FormatEvent(eventHandlerName));
                    }
                }
            }
        }
    }
}