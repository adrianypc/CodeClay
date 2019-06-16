using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
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
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("FieldName")]
        public string FieldName { get; set; } = Guid.NewGuid().ToString();

        [XmlElement("TextFieldName")]
        public string TextFieldName { get; set; } = "";

        [XmlElement("Caption")]
        public string Caption { get; set; } = "";

        [XmlElement("ColumnFormat")]
        public string ColumnFormat { get; set; } = "";

        [XmlElement("Mask")]
		public eTextMask Mask { get; set; } = eTextMask.None;

        [XmlElement("ForeColor")]
		public string ForeColor { get; set; } = "";

        [XmlElement("Hidden")]
        public bool Hidden { get; set; } = false;

        [XmlSqlElement("Editable", typeof(bool))]
        public XmlElement Editable { get; set; } = MyWebUtils.CreateXmlElement("Editable", true);

        [XmlElement("Mandatory")]
        public bool Mandatory { get; set; } = false;

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
        public bool Searchable { get; set; } = false;

        [XmlElement("Browsable")]
        public bool Browsable { get; set; } = true;

        // --------------------------------------------------------------------------------------------------
        // Properties (Derived)
        // --------------------------------------------------------------------------------------------------

        [XmlIgnore]
        public override string ID
        {
            get { return FieldName; }
        }

        [XmlIgnore]
        public string SearchableFieldName
        {
            get { return PARAMETER_PREFIX + FieldName; }
        }

        [XmlIgnore]
        public bool IsLabel
        {
            get { return GetType() == typeof(CiField); }
        }

        [XmlIgnore]
        public bool IsSearching
        {
            get
            {
                return (CiTable != null) ? (CiTable.IsSearching) && Searchable : false;
            }
        }

        [XmlIgnore]
        public bool IsVisible
        {
            get
            {
                return IsSearching || (Browsable && !Hidden);
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
                            int colCount = IsVisible ? CiTable.ColCount : 1;
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

        public virtual Type GetNativeType(object fieldValue)
        {
            return (!MyUtils.IsEmpty(fieldValue)) ? fieldValue.GetType() : typeof(string);
        }

        public virtual object GetNativeValue(object fieldValue)
        {
            return fieldValue;
        }

        public virtual bool IsEditable(DataRow drParams)
        {
            return IsSearching || (Enabled && MyWebUtils.Eval<bool>(Editable, drParams));
        }

        public virtual ArrayList GetLeaderFieldNames()
        {
            return MyUtils.GetParameters(MyWebUtils.GetSQLFromXml(Editable));
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

				dxColumn.Visible = IsVisible;
				dxColumn.Caption = Caption;
				dxColumn.Width = ColumnWidth;

                CardViewFormLayoutProperties layoutProperties = dxColumn.CardView.CardLayoutProperties;
                CardViewColumnLayoutItem dxLayoutItem = layoutProperties.FindColumnItem(fieldName) as CardViewColumnLayoutItem;

                if (dxLayoutItem == null)
                {
                    dxLayoutItem = new CardViewColumnLayoutItem();
                    dxLayoutItem.Name = fieldName;
                    dxLayoutItem.ColumnName = fieldName;
                    layoutProperties.Items.Add(dxLayoutItem);
                }

                dxLayoutItem.Visible = IsVisible;
                dxLayoutItem.Caption = (Caption == "*") ? "" : Caption;
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

				dxColumn.Visible = IsVisible;
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

                    fieldValue = MyUtils.Coalesce(fieldValue, CiField.Value);
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
                string fieldName = CiField.IsSearching ? CiField.SearchableFieldName : CiField.FieldName;
                bool isEditable = CiField.IsEditable(drParams);
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
                mEditor.Visible = CiField.IsVisible;
                mEditor.BackColor = GetBackColor(isEditable);
				
				if (!MyUtils.IsEmpty(foreColor))
				{
					mEditor.ForeColor = Color.FromName(foreColor);
				}

                mEditor.JSProperties["cpHasFieldExitMacro"] = (CiField.CiFieldExitMacros.Length > 0);
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
                    }
                }
            }
        }

        public virtual Color GetBackColor(bool isEditable)
        {
            Color backColor = Color.Transparent;

            if (!CiField.IsLabel && isEditable)
            {
                if (CiField.Mandatory)
                {
                    backColor = Color.LightPink;
                }
                else
                {
                    backColor = Color.PaleGoldenrod;
                }
            }

            return backColor;
        }
    }
    
    public class XiField : XiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        public XiField()
        {
            PluginType = typeof(CiField);
        }

        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public bool IsRowKeyChecked { get; set; } = false;

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        protected override bool IsPluginTypeOk(Type pluginType)
        {
            return base.IsPluginTypeOk(pluginType) || (pluginType == typeof(CiTable));
        }

        protected override string GetPluginTypeName(DataRow drPluginDefinition)
        {
            string pluginTypeName = "CiField";

            if (drPluginDefinition.Table.Columns.Contains("Type"))
            {
                string fieldType = MyUtils.Coalesce(drPluginDefinition["Type"], "").ToString();

                if (fieldType == "Table")
                {
                    pluginTypeName = null;
                }
                else
                {
                    pluginTypeName = "Ci" + fieldType + "Field";
                }
            }

            return pluginTypeName;
        }

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return MyWebUtils.GetBySQL("?exec spField_sel @AppID, @TableID", drPluginKey, true);
        }

        protected override List<XElement> GetPluginDefinitions(List<XElement> xElements)
        {
            if (xElements != null)
            {
                return xElements.FindAll(el => el.Name.ToString().EndsWith("Field"));
            }

            return null;
        }

        protected override void DeletePluginDefinitions(DataRow drPluginKey)
        {
            MyWebUtils.GetBySQL("exec spField_del @AppID, @TableID", drPluginKey, true);
        }

        protected override void WriteToDB(DataRow drPluginDefinition)
        {
            string insertColumnNames = "@AppID, @TableID, @FieldName, @Caption, @Type, @Width, @InRowKey";
            string insertSQL = string.Format("?exec spField_ins {0}", insertColumnNames);

            drPluginDefinition["FieldID"] = MyWebUtils.EvalSQL(insertSQL, drPluginDefinition, true);

            string updateColumnNames = "@AppID, @TableID, @FieldID, @FieldName, @Editable, @Mandatory" +
                ", @Hidden, @Searchable, @Summary, @ForeColor, @RowSpan, @ColSpan, @Width, @HorizontalAlign" +
                ", @VerticalAlign, @Value, @DropdownSQL, @InsertSQL, @Code, @Description, @TextFieldName" +
                ", @DropdownWidth, @Folder, @MinValue, @MaxValue, @Columns, @ColumnFormat, @Mask";
            string updateSQL = string.Format("exec spField_updLong {0}", updateColumnNames);

            MyWebUtils.GetBySQL(updateSQL, drPluginDefinition, true);
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                DownloadRowKey(drPluginDefinition, xPluginDefinition);
                //DownloadCiTable(drPluginDefinition, xPluginDefinition);
            }
        }

        protected override void UploadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            DataColumnCollection dcPluginColumns = drPluginDefinition.Table.Columns;

            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                UploadType(drPluginDefinition, xPluginDefinition, dcPluginColumns);
                UploadRowKey(drPluginDefinition, xPluginDefinition, dcPluginColumns);
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private void DownloadRowKey(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (!IsRowKeyChecked)
            {
                IsRowKeyChecked = true;

                DataTable dtParent = drPluginDefinition.Table;
                XElement xParent = xPluginDefinition.Parent;

                if (dtParent != null && xParent != null)
                {
                    XElement xRowKey = xParent.Element("RowKey");
                    if (xRowKey != null)
                    {
                        string rowKey = xRowKey.Value;

                        foreach (DataRow drField in dtParent.Rows)
                        {
                            object objInRowKey = drField["InRowKey"];
                            bool inRowKey = false;

                            if (!MyUtils.IsEmpty(objInRowKey))
                            {
                                inRowKey = Convert.ToBoolean(objInRowKey);
                            }

                            if (inRowKey)
                            {
                                if (!MyUtils.IsEmpty(rowKey))
                                {
                                    rowKey += ",";
                                }

                                rowKey += drField["FieldName"];

                                xRowKey.Value = rowKey;
                            }
                        }
                    }
                }
            }
        }

        private void DownloadCiTable(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (xPluginDefinition.Name == "CiTable")
            {
                xPluginDefinition.SetElementValue("TableName", drPluginDefinition["FieldName"].ToString());
                xPluginDefinition.SetElementValue("Caption", drPluginDefinition["Caption"].ToString());
            }
        }

        private void UploadType(DataRow drPluginDefinition, XElement xPluginDefinition, DataColumnCollection dcPluginColumns)
        {
            if (dcPluginColumns.Contains("Type"))
            {
                string fieldType = xPluginDefinition.Name.ToString();
                drPluginDefinition["Type"] = fieldType.Substring(2, fieldType.Length - 7);
            }
        }

        private void UploadRowKey(DataRow drPluginDefinition, XElement xPluginDefinition, DataColumnCollection dcPluginColumns)
        {
            if (dcPluginColumns.Contains("InRowKey"))
            {
                XElement xFieldName = xPluginDefinition.Element("FieldName");

                XElement xParentPluginDefinition = xPluginDefinition.Parent;

                if (xParentPluginDefinition != null)
                {
                    XElement xRowKey = xParentPluginDefinition.Element("RowKey");
                    if (xRowKey != null)
                    {
                        string[] rowKeyNames = xRowKey.Value.Split(',');

                        drPluginDefinition["InRowKey"] = (xFieldName != null) &&
                            (rowKeyNames != null) &&
                            rowKeyNames.Contains(xFieldName.Value);
                    }
                }
            }
        }
    }
}