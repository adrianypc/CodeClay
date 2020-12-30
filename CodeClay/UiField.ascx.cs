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

        [XmlSqlElement("Caption", typeof(string))]
        public XmlElement Caption { get; set; } = MyWebUtils.CreateXmlElement("Caption", "");

        [XmlElement("Mask")]
        public eTextMask Mask { get; set; } = eTextMask.None;

        [XmlElement("ForeColor")]
        public string ForeColor { get; set; } = "";

        [XmlSqlElement("Editable", typeof(bool))]
        public XmlElement Editable { get; set; } = MyWebUtils.CreateXmlElement("Editable", true);

        [XmlSqlElement("Mandatory", typeof(bool))]
        public XmlElement Mandatory { get; set; } = MyWebUtils.CreateXmlElement("Mandatory", false);

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

        [XmlElement("Browsable")]
        public bool Browsable { get; set; } = true;

        [XmlElement("AutoBlank")]
        public bool AutoBlank { get; set; } = false;

        // --------------------------------------------------------------------------------------------------
        // Properties (Derived)
        // --------------------------------------------------------------------------------------------------

        [XmlIgnore]
        public override string ID
        {
            get { return FieldName; }
        }

        [XmlIgnore]
        public bool IsLabel
        {
            get { return GetType() == typeof(CiField); }
        }

        [XmlIgnore]
        public override bool IsSearching
        {
            get
            {
                return (CiTable != null && CiTable.IsSearching && Searchable);
            }
        }

        [XmlIgnore]
        public Unit EditorWidth
        {
            get
            {
                Unit editorWidth = Unit.Percentage(100);
                bool hasLayout = (CiTable != null && CiTable.LayoutXml != null);
                bool isCardView = (CiTable != null && CiTable.DefaultView == "Card");

                if (IsSearching)
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
        public CiMacro[] CiMacros
        {
            get
            {
                return CiPlugins.Where(c => (c as CiMacro) != null).Select(c => c as CiMacro).ToArray();
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

        [XmlIgnore]
        public bool Transparent
        {
            get
            {
                return GetType() == typeof(CiField) || GetType() == typeof(CiCheckField);
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override CiPlugin GetContainerPlugin()
        {
            return CiTable;
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual bool IsEditable(DataRow drParams)
        {
            return IsSearching || (Enabled && MyWebUtils.Eval<bool>(Editable, drParams));
        }

        public virtual bool IsMandatory(DataRow drParams)
        {
            return IsSearching || (Enabled && MyWebUtils.Eval<bool>(Mandatory, drParams));
        }

        public virtual bool IsVisible(DataRow drParams)
        {
            return IsSearching || (Browsable && !IsHidden(drParams));
        }

        public virtual Unit GetColumnWidth(DataRow drParams)
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
                        int colCount = IsVisible(drParams) ? CiTable.ColCount : 1;
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

        public virtual ArrayList GetLeaderFieldNames()
        {
            ArrayList fieldNames = MyUtils.GetParameters(MyWebUtils.GetSQLFromXml(Editable));
            fieldNames.AddRange(MyUtils.GetParameters(MyWebUtils.GetSQLFromXml(Mandatory)));
            fieldNames.AddRange(MyUtils.GetParameters(MyWebUtils.GetSQLFromXml(Hidden)));

            return fieldNames;
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

        public virtual void FormatCardColumn(CardViewColumn dxColumn, DataRow dr)
        {
            if (dxColumn != null)
            {
                string fieldName = dxColumn.FieldName;
                string caption = EvalCaptionSQL(dr);

                dxColumn.Visible = IsVisible(dr);
                dxColumn.Caption = caption;
                dxColumn.Width = GetColumnWidth(dr);

                CardViewFormLayoutProperties layoutProperties = dxColumn.CardView.CardLayoutProperties;
                CardViewColumnLayoutItem dxLayoutItem = layoutProperties.FindColumnItem(fieldName) as CardViewColumnLayoutItem;

                if (dxLayoutItem == null)
                {
                    dxLayoutItem = new CardViewColumnLayoutItem();
                    dxLayoutItem.Name = fieldName;
                    dxLayoutItem.ColumnName = fieldName;
                    layoutProperties.Items.Add(dxLayoutItem);
                }

                dxLayoutItem.Visible = IsVisible(dr);
                dxLayoutItem.Caption = caption; //.Replace(' ', Convert.ToChar(160));
                dxLayoutItem.RowSpan = RowSpan;
                dxLayoutItem.ColSpan = ColSpan;
                dxLayoutItem.Width = GetColumnWidth(dr);
                dxLayoutItem.HorizontalAlign = HorizontalAlign;
                dxLayoutItem.VerticalAlign = VerticalAlign;
                dxLayoutItem.ShowCaption = !MyUtils.IsEmpty(caption)
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

        public virtual void FormatGridColumn(GridViewDataColumn dxColumn, DataRow dr)
        {
            if (dxColumn != null)
            {
                HorizontalAlign left = System.Web.UI.WebControls.HorizontalAlign.Left;
                VerticalAlign top = System.Web.UI.WebControls.VerticalAlign.Top;

                dxColumn.Visible = IsVisible(dr);
                dxColumn.Caption = EvalCaptionSQL(dr);
                dxColumn.CellStyle.HorizontalAlign = left;
                dxColumn.CellStyle.VerticalAlign = top;
                dxColumn.EditCellStyle.VerticalAlign = top;
                dxColumn.EditCellStyle.HorizontalAlign = left;
                dxColumn.HeaderStyle.HorizontalAlign = left;
                dxColumn.CellRowSpan = RowSpan;
                dxColumn.Width = GetColumnWidth(dr);

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
                return (GetType() != typeof(CiField)) && !uiTable.IsEditing && !IsSearching;
            }

            return false;
        }

        public virtual string ToString(object value)
        {
            return MyUtils.Coalesce(value, "").ToString();
        }


        public virtual string EvalCaptionSQL(DataRow dr)
        {
            string caption = MyWebUtils.Eval<string>(Caption, dr);

            return (caption == "*") ? "" : caption;
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
                dxLabel.Style.Add(HtmlTextWriterStyle.Display, "block");

                if (UiTable != null && UiTable.IsGridView)
                {
                    dxLabel.Style.Add(HtmlTextWriterStyle.PaddingLeft, "4px");
                    dxLabel.Style.Add(HtmlTextWriterStyle.PaddingTop, "3px");
                }
                else
                {
                    dxLabel.Style.Add(HtmlTextWriterStyle.PaddingTop, "4px");
                }
            }

            Refresh(sender);
        }

        protected void dxLabelPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh(sender, e);
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

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

        public virtual void Refresh(object sender, CallbackEventArgsBase e = null)
        {
            if (mEditor != null && CiField != null)
            {
                DataRow drParams = GetState();
                string fieldName = "";
                object fieldValue = "";

                if (e != null)
                {
                    string[] nameValuePairs = e.Parameter.Split(LIST_SEPARATOR.ToCharArray());

                    if (nameValuePairs.Length > 1)
                    {
                        fieldName = nameValuePairs[0];
                        fieldValue = nameValuePairs[1];

                        CiField ciField = CiField.CiTable.GetField(fieldName);
                        if (ciField != null)
                        {
                            drParams[fieldName] = ciField.GetNativeValue(fieldValue);
                        }
                    }

                    if (nameValuePairs.Length > 3)
                    {
                        fieldName = nameValuePairs[2];
                        fieldValue = nameValuePairs[3];

                        CiField ciField = CiField.CiTable.GetField(fieldName);
                        if (ciField != null)
                        {
                            drParams[fieldName] = ciField.GetNativeValue(fieldValue);
                        }
                    }
                }

                CiTable ciTable = CiField.CiTable;
                string tableName = (ciTable != null) ? ciTable.TableName : "";
                fieldName = CiField.IsSearching ? CiField.SearchableID : CiField.FieldName;
                fieldValue = MyUtils.Coalesce(MyWebUtils.GetField(drParams, fieldName), CiField.Value);
                string foreColor = CiField.ForeColor;

                mEditor.ID = fieldName;
                mEditor.DataBind();
                mEditor.Caption = InContainer() ? "" : CiField.EvalCaptionSQL(drParams);
                mEditor.ClientIDMode = ClientIDMode.Static;
                mEditor.CssClass = "css" + fieldName;
                mEditor.Width = CiField.EditorWidth;
                mEditor.Value = fieldValue;
                mEditor.Visible = CiField.IsVisible(drParams);

                if (!MyUtils.IsEmpty(foreColor))
                {
                    mEditor.ForeColor = Color.FromName(foreColor);
                }

                mEditor.JSProperties["cpHasFieldExitMacro"] = (CiField.CiFieldExitMacros.Length > 0);
                mEditor.JSProperties["cpTableName"] = tableName;
                mEditor.JSProperties["cpFollowerFields"] = FollowerFieldNames;
                mEditor.JSProperties["cpMandatory"] = CiField.IsMandatory(drParams);
                mEditor.JSProperties["cpEditable"] = CiField.IsEditable(drParams);
                mEditor.JSProperties["cpAutoBlank"] = CiField.AutoBlank;
                mEditor.JSProperties["cpTransparent"] = CiField.Transparent;
                mEditor.JSProperties["cpVisible"] = CiField.IsVisible(drParams);

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

                        eventHandlerName = mEditor.ClientInstanceName + "_KeyPress";
                        mEditor.SetClientSideEventHandler("KeyPress", FormatEvent(eventHandlerName));
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        protected override object GetEditorValue(string key, int rowIndex = -1)
        {
            object editorValue = null;

            if (CiField != null)
            {
                UiTable uiParentTable = UiParentPlugin as UiTable;
                if (uiParentTable != null)
                {
                    editorValue = uiParentTable[key, rowIndex];
                }
                else
                {
                    GridBaseTemplateContainer container = this.NamingContainer as GridBaseTemplateContainer;
                    if (container != null && !CiField.Computed)
                    {
                        try
                        {
                            editorValue = DataBinder.Eval(container.DataItem, key);
                        }
                        catch
                        {
                            // Do nothing
                        }
                    }
                }

                if (!CiField.Enabled && (CiField.GetType().Name.Substring(2)) != CiField.GetUiPluginName().Substring(2))
                {
                    // For disabled fields which have been cast to TextField
                    editorValue = CiField.ToString(editorValue);
                }

                CiPlugin ciContainerPlugin = CiField.GetContainerPlugin();
                if (ciContainerPlugin != null)
                {
                    CiPlugin ciEditorField = ciContainerPlugin.GetById(key);
                    if (ciEditorField != null)
                    {
                        editorValue = ciEditorField.GetNativeValue(editorValue);
                    }
                }
            }

            return editorValue;
        }

        protected virtual void SetupPopupMacro(UiButtonField uiPopupMacro)
        {
            uiPopupMacro.Visible = false;

            foreach (CiMacro ciMacro in CiField.CiMacros)
            {
                uiPopupMacro.Visible = !MyUtils.IsEmpty(ciMacro.NavigateUrl.InnerText);
                CiButtonField ciButtonField = new CiButtonField();
                ciButtonField.FieldName = CiField.FieldName + "Popup";
                uiPopupMacro.CiButtonField = ciButtonField;
                ciButtonField.Add(ciMacro);
                break;
            }
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

        private bool IsRowKeyChecked { get; set; } = false;

        private Hashtable mPropertySQL = new Hashtable();

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

                switch (fieldType)
                {
                    case "Table":
                    case "Form":
                        pluginTypeName = null;
                        break;

                    case "Identity":
                    case "FKey":
                        pluginTypeName = "CiTextField";
                        break;

                    default:
                        pluginTypeName = "Ci" + fieldType + "Field";
                        break;

                }
            }

            return pluginTypeName;
        }

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            DataTable dt = MyWebUtils.GetBySQL("?exec spField_sel @AppID, @TableID, null, '!Button,Stub'", drPluginKey, true);

            return dt;
        }

        protected override List<XElement> GetPluginDefinitions(List<XElement> xElements)
        {
            if (xElements != null)
            {
                return xElements.FindAll(el => IsDataField(el.Name.ToString()));
            }

            return null;
        }

        protected override void WriteToDB(DataRow drPluginDefinition)
        {
            object objFieldID = MyWebUtils.EvalSQL("select FieldID from CiField " +
                "where AppID = @AppID and TableID = @TableID and FieldName = @FieldName", drPluginDefinition);

            if (objFieldID != null)
            {
                drPluginDefinition["FieldID"] = objFieldID;
            }
            else
            {
                string insertColumnNames = "@AppID, @TableID, @FieldName, @Caption, @Type, @Width, @InRowKey";
                string insertSQL = string.Format("?exec spField_ins {0}", insertColumnNames);

                drPluginDefinition["FieldID"] = MyWebUtils.EvalSQL(insertSQL, drPluginDefinition, true);
            }

            string updateColumnNames = "@AppID, @TableID, @FieldID, @FieldName, @Caption, @Type, @Editable, @AutoBlank, @Mandatory" +
                ", @Hidden, @Searchable, @Summary, @ForeColor, @RowSpan, @ColSpan, @Width, @HorizontalAlign" +
                ", @VerticalAlign, @Value, @DropdownSQL, @InsertSQL, @Code, @Description, @TextFieldName" +
                ", @DropdownWidth, @Folder, @MinValue, @MaxValue, @Columns, @Mask, @NestedMacroID";
            string updateSQL = string.Format("exec spField_updLong {0}", updateColumnNames);

            MyWebUtils.GetBySQL(updateSQL, drPluginDefinition, true);

            DataColumnCollection dcPluginDefinition = drPluginDefinition.Table.Columns;

            if (!dcPluginDefinition.Contains("SQLType"))
            {
                dcPluginDefinition.Add("SQLType");
            }

            if (!dcPluginDefinition.Contains("SQL"))
            {
                dcPluginDefinition.Add("SQL");
            }

            MyWebUtils.GetBySQL("exec spFieldSQL_del @AppID, @TableID, @FieldID", drPluginDefinition, true);
            foreach (string sqlType in mPropertySQL.Keys)
            {
                drPluginDefinition["SQLType"] = sqlType;
                drPluginDefinition["SQL"] = mPropertySQL[sqlType];
                MyWebUtils.GetBySQL("exec spFieldSQL_upd @AppID, @TableID, @FieldID, @SQLType, @SQL", drPluginDefinition, true);
            }
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                string fieldName = MyWebUtils.GetStringField(drPluginDefinition, "FieldName");
                if (fieldName == "ParentID")
                {
                    XElement xHiddenSQL = new XElement("Hidden");
                    xHiddenSQL.Add(new XAttribute("lang", "sql"));
                    xHiddenSQL.Value = "select 1 from Singleton where @ParentID is not null";
                    xPluginDefinition.Add(xHiddenSQL);

                    XElement xFieldName = xPluginDefinition.Element("FieldName");
                    xFieldName.Value = "EditParentID";
                }

                DataTable dtFieldSQL = MyWebUtils.GetBySQL("?exec spFieldSQL_sel @AppID, @TableID, @FieldID", drPluginDefinition, true);
                foreach (DataRow drFieldSQL in dtFieldSQL.Rows)
                {
                    string sqlType = MyWebUtils.GetStringField(drFieldSQL, "SQLType");
                    string SQL = MyWebUtils.GetStringField(drFieldSQL, "SQL");
                    XElement xSQLType = xPluginDefinition.Element(sqlType);
                    if (xSQLType == null)
                    {
                        xSQLType = new XElement(sqlType);
                        xPluginDefinition.Add(xSQLType);
                    }

                    XAttribute xLang = xSQLType.Attribute("lang");
                    if (xLang == null)
                    {
                        xLang = new XAttribute("lang", "");
                        xSQLType.Add(xLang);
                    }

                    xLang.Value = "sql";
                    xSQLType.Value = SQL;
                }
            }
        }

        protected override void UploadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            DataColumnCollection dcPluginColumns = drPluginDefinition.Table.Columns;

            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                UploadType(drPluginDefinition, xPluginDefinition, dcPluginColumns);
                UploadRowKey(drPluginDefinition, xPluginDefinition, dcPluginColumns);

                mPropertySQL.Clear();
                foreach (XElement xProperty in xPluginDefinition.Elements())
                {
                    if (xProperty.Attributes("lang").Count() > 0)
                    {
                        mPropertySQL[xProperty.Name.ToString()] = xProperty.Value;
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private bool IsDataField(string fieldName)
        {
            if (!MyUtils.IsEmpty(fieldName) && fieldName.EndsWith("Field") && !fieldName.EndsWith("ButtonField"))
            {
                return true;
            }

            return false;
        }

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
                            }
                        }

                        xRowKey.Value = rowKey;

                        if (MyUtils.IsEmpty(rowKey))
                        {
                            xRowKey.Remove();
                        }
                    }
                }
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

    public class XiButtonField : XiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        private XElement mXButtonMacro = null;

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        protected override void DeletePluginDefinitions(DataRow drPluginKey)
        {
            // Do nothing
        }

        protected override List<XElement> GetPluginDefinitions(List<XElement> xElements)
        {
            if (xElements != null)
            {
                return xElements.FindAll(el => IsButtonField(el.Name.ToString()));
            }

            return null;
        }

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return MyWebUtils.GetBySQL("?exec spField_sel @AppID, @TableID, null, 'Button'", drPluginKey, true);
        }

        protected override void WriteToDB(DataRow drPluginDefinition)
        {
            if (mXButtonMacro != null)
            {
                XiButtonMacro xiButtonMacro = new XiButtonMacro();
                xiButtonMacro.UploadFromXml(drPluginDefinition, new List<XElement>() { mXButtonMacro });

                drPluginDefinition["NestedMacroID"] = xiButtonMacro.NewMacroID;
                base.WriteToDB(drPluginDefinition);
            }
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (drPluginDefinition != null && xPluginDefinition != null && xPluginDefinition.Name == "CiButtonField")
            {
                XiButtonMacro xiButtonMacro = new XiButtonMacro();
                xiButtonMacro.DownloadToXml(drPluginDefinition, xPluginDefinition);
            }
        }

        protected override void UploadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            base.UploadDerivedValues(drPluginDefinition, xPluginDefinition);

            mXButtonMacro = xPluginDefinition.Element("CiMacro");
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private bool IsButtonField(string fieldName)
        {
            if (!MyUtils.IsEmpty(fieldName) && fieldName.EndsWith("ButtonField"))
            {
                return true;
            }

            return false;
        }
    }
}