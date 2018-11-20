using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;
using DevExpress.XtraPrinting;

namespace CodeClay
{
    public interface IUiColumn : ITemplate
    {
        void BuildCardColumn(Control container);
        void BuildGridColumn(Control container);
    }

    [XmlType("CiTable")]
    public class CiTable : CiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Member variables
        // --------------------------------------------------------------------------------------------------

        private string mDataSource = "";
        private CiMacro mSelectMacro = null;
        private CiMacro mUpdateMacro = null;
        private CiMacro mInsertMacro = null;
        private CiMacro mDeleteMacro = null;
        private CiMacro mDefaultMacro = null;

        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("TableName")]
        public string TableName { get; set; } = "myTable";

        [XmlElement("TableCaption")]
        public string TableCaption { get; set; } = "My Caption";

        [XmlElement("QuickInsert")]
        public bool QuickInsert { get; set; } = false;

        [XmlElement("PageSize")]
        public int PageSize { get; set; } = 0;

        [XmlElement("DefaultView")]
        public string DefaultView { get; set; } = "Grid";

        [XmlElement("Mode")]
        public eTableMode Mode { get; set; } = eTableMode.Browse;

        [XmlElement("ColCount")]
        public int ColCount { get;set;} = 1;

		[XmlElement("DoubleClickMacroName")]
		public string DoubleClickMacroName { get; set; }

        [XmlElement("BubbleUpdate")]
        public bool BubbleUpdate { get; set; } = false;

        [XmlElement("SelectMacro")]
        public CiMacro SelectMacro
        {
            get { return mSelectMacro; }

            set
            {
                mSelectMacro = value;
                if (mSelectMacro != null)
                {
                    mSelectMacro.CiTable = this;
                    if (MyUtils.IsEmpty(mSelectMacro.MacroName))
                    {
                        mSelectMacro.MacroName = "Select";
						mSelectMacro.MacroCaption = value.MacroCaption;
                    }
                }
            }
        }

        [XmlElement("UpdateMacro")]
        public CiMacro UpdateMacro
        {
            get { return mUpdateMacro; }

            set
            {
                mUpdateMacro = value;
                if (mUpdateMacro != null)
                {
                    mUpdateMacro.CiTable = this;
                    if (MyUtils.IsEmpty(mUpdateMacro.MacroName))
                    {
                        mUpdateMacro.MacroName = "Update";
						mUpdateMacro.MacroCaption = value.MacroCaption;
					}
                }
            }
        }

        [XmlElement("InsertMacro")]
        public CiMacro InsertMacro
        {
            get { return mInsertMacro; }

            set
            {
                mInsertMacro = value;
                if (mInsertMacro != null)
                {
                    mInsertMacro.CiTable = this;
                    if (MyUtils.IsEmpty(mInsertMacro.MacroName))
                    {
                        mInsertMacro.MacroName = "Insert";
						mInsertMacro.MacroCaption = value.MacroCaption;
					}
                }
            }
        }

        [XmlElement("DeleteMacro")]
        public CiMacro DeleteMacro
        {
            get { return mDeleteMacro; }

            set
            {
                mDeleteMacro = value;
                if (mDeleteMacro != null)
                {
                    mDeleteMacro.CiTable = this;
                    if (MyUtils.IsEmpty(mDeleteMacro.MacroName))
                    {
                        mDeleteMacro.MacroName = "Delete";
						mDeleteMacro.MacroCaption = value.MacroCaption;
					}
                }
            }
        }

        [XmlElement("DefaultMacro")]
        public CiMacro DefaultMacro
        {
            get { return mDefaultMacro; }

            set
            {
                mDefaultMacro = value;
                if (mDefaultMacro != null)
                {
                    mDefaultMacro.CiTable = this;
                    if (MyUtils.IsEmpty(mDefaultMacro.MacroName))
                    {
                        mDefaultMacro.MacroName = "Default";
                    }
                }
            }
        }

        [XmlAnyElement("DataSource")]
        public XmlElement DataSource
        {
            get
            {
                var x = new XmlDocument();
                x.LoadXml(string.Format("<Root>{0}</Root>", mDataSource));
                return x.DocumentElement;
            }

            set { mDataSource = value.InnerXml; }
        }

        // --------------------------------------------------------------------------------------------------
        // Properties (Derived)
        // --------------------------------------------------------------------------------------------------

        [XmlIgnore]
        public bool IsSearching
        {
            get
            {
                return UiApplication.Me.GetCommandFired(TableName) == "Search";
            }
        }

        [XmlIgnore]
        public CiField[] CiFields
        {
            get
            {
                return CiPlugins.Where(c => (c as CiField) != null).Select(c => c as CiField).ToArray();
            }
        }

        [XmlIgnore]
        public CiField[] CiBrowsableFields
        {
            get
            {
                CiField[] ciFields = CiFields;
                CiField[] sortedFields = new CiField[ciFields.Length];

                if (DefaultView == "Card")
                {
                    // Invisible fields go at the end of the list
                    int totalVisibleFields = ciFields.Length;
                    foreach (CiField ciField in ciFields)
                    {
                        if (!ciField.Visible)
                        {
                            sortedFields[--totalVisibleFields] = ciField;
                        }
                    }

                    int totalColumns = ColCount;
                    int rowNumber = 0;
                    int columnNumber = 0;

                    foreach (CiField ciField in ciFields)
                    {
                        if (ciField.Visible)
                        {
                            int fieldIndex = rowNumber * totalColumns + columnNumber;

                            if (fieldIndex >= totalVisibleFields)
                            {
                                rowNumber = 0;
                                columnNumber++;
                                fieldIndex = rowNumber * totalColumns + columnNumber;
                            }

                            sortedFields[fieldIndex] = ciField;
                            rowNumber++;
                        }
                    }
                }
                else
                {
                    sortedFields = ciFields.Where(c => !c.Searchable).ToArray();
                }

                return sortedFields;
            }
        }

        [XmlIgnore]
        public CiField[] CiSearchableFields
        {
            get
            {
                return CiFields.Where(c => c.Searchable).ToArray();
            }
        }

        [XmlIgnore]
        public CiMacro[] CiMacros
        {
            get
            {
                return Get<CiMacro>().Where(c => c.GetType() == typeof(CiMacro)).ToArray();
            }
        }

		[XmlIgnore]
		public CiFieldExitMacro[] CiFieldExitMacros
		{
			get
			{
				return Get<CiFieldExitMacro>();
			}
		}

        [XmlIgnore]
        public CiTable CiParentTable
        {
            get { return CiParentPlugin as CiTable; }
        }

        [XmlIgnore]
        public CiTable[] CiTables
        {
            get
            {
                return CiPlugins.Where(c => (c as CiTable) != null).Select(c => c as CiTable).ToArray();
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override CiPlugin GetById(string id)
        {
            return MyUtils.Coalesce(GetField(id), GetTable(id)) as CiPlugin;
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual CiTable GetTable(string tableName)
        {
            if (CiTables != null)
            {
                CiTable[] TablesFound = CiTables.Where(c => c.TableName == tableName).ToArray();
                if (TablesFound.Length > 0)
                {
                    return TablesFound[0];
                }
            }

            return null;
        }

        public virtual CiField GetField(string fieldName)
        {
            if (CiFields != null)
            {
                CiField[] FieldsFound = CiFields.Where(c => c.FieldName == fieldName).ToArray();
                if (FieldsFound.Length > 0)
                {
                    return FieldsFound[0];
                }
            }

            return null;
        }

        public virtual CiMacro GetMacro(string macroName)
        {
            if (CiMacros != null)
            {
                CiMacro[] macrosFound = CiMacros.Where(c => c.MacroName == macroName).ToArray();
                if (macrosFound.Length > 0)
                {
                    return macrosFound[0];
                }
            }

            return null;
        }

		public virtual CiFieldExitMacro[] GetFieldExitMacros(string fieldName)
		{
			if (CiFieldExitMacros != null)
			{
				return CiFieldExitMacros.Where(c => c.FieldNames.Contains(fieldName)).ToArray();
			}

			return new CiFieldExitMacro[] { };
		}
    }

    public partial class UiTable : UiPlugin, IUiColumn
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public eTableMode Mode
        {
            get
            {
                string mode = MyWebUtils.QueryString["Mode"];

                if (!MyUtils.IsEmpty(mode))
                {
                    try
                    {
                        return (eTableMode)Enum.Parse(typeof(eTableMode), mode);
                    }
                    catch
                    {
                        // Do nothing
                    }
                }

                return eTableMode.Browse;
            }
        }

        public bool RecordsExist
        {
            get
            {
                int numberOfRows = 0;

                if (dxCard != null && dxCard.Visible)
                {
                    numberOfRows = dxCard.VisibleCardCount;
                }
                else if (dxGrid != null && dxGrid.Visible)
                {
                    numberOfRows = dxGrid.VisibleRowCount;
                }

                return (numberOfRows > 0);
            }
        }

        public bool IsInserting
        {
            get
            {
                string command = UiApplication.Me.GetCommandFired(CiTable.TableName);

                bool isStartEditing = (command == "New") ||
                    dxCard.IsNewCardEditing ||
                    dxGrid.IsNewRowEditing ||
                    (MyUtils.IsEmpty(command) && (Mode == eTableMode.New));

                bool isFinishEditing = (command == "Update") || (command == "Cancel");

                return isStartEditing && !isFinishEditing;
            }
        }

        public bool IsEditing
        {
            get
            {
                string command = UiApplication.Me.GetCommandFired(CiTable.TableName);

                bool isStartEditing = (command == "Edit") || (command == "New") ||
                    dxCard.IsEditing || dxCard.IsNewCardEditing ||
                    dxGrid.IsEditing || dxGrid.IsNewRowEditing ||
                    (MyUtils.IsEmpty(command) &&
                    (Mode == eTableMode.Edit || Mode == eTableMode.New));

                bool isFinishEditing = (command == "Update") || (command == "Cancel");

                return isStartEditing && !isFinishEditing;
            }
        }

        public bool IsBrowsing
        {
            get
            {
                bool isSearching = (CiTable != null) && CiTable.IsSearching;

                return !isSearching;
            }
        }

        public bool IsNonEditable
        {
            get
            {
                if (CiTable != null)
                {
                    return (CiTable.UpdateMacro == null);
                }

                return false;
            }
        }

        public bool IsCardView
        {
            get
            {
                if (CiTable != null)
                {
                    return CiTable.DefaultView == "Card" || CiTable.IsSearching;
                }

                return false;
            }
        }

        public bool IsGridView
        {
            get
            {
                if (CiTable != null)
                {
                    return CiTable.DefaultView == "Grid" && !CiTable.IsSearching;
                }

                return false;
            }
        }

        public CiTable CiTable
        {
            get { return CiPlugin as CiTable; }
            set { CiPlugin = value; }
        }

        public UiTable UiParentTable
        {
            get { return UiParentPlugin as UiTable; }
        }

        public UiTable UiRootTable
        {
            get
            {
                if (UiParentTable != null)
                {
                    return UiParentTable.UiRootTable;
                }

                return this;
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
			if (CiTable != null && UiParentTable == null)
			{
				Page.Title = CiTable.TableCaption;
			}
		}

        // --------------------------------------------------------------------------------------------------
        // Event Handlers (Search)
        // --------------------------------------------------------------------------------------------------

        protected void dxSearch_Init(object sender, EventArgs e)
        {
            bool isVisible = CiTable != null && CiTable.IsSearching;

            dxSearch.Visible = isVisible;

            if (isVisible)
            {
                BuildCardView(dxSearch, true);
                dxSearch.AddNewCard();
            }
        }

        protected void dxSearch_CustomJSProperties(object sender, ASPxCardViewClientJSPropertiesEventArgs e)
        {
            if (CiTable != null)
            {
                e.Properties["cpPuxFile"] = CiTable.PuxFile;
            }
        }

        protected void dxSearch_InitNewCard(object sender, DevExpress.Web.Data.ASPxDataInitNewRowEventArgs e)
        {
            dxSearch.SettingsPager.SettingsTableLayout.RowsPerPage = 0;

            DataRow drParams = GetState();

            foreach (DataColumn dcColumn in drParams.Table.Columns)
            {
                string columnName = dcColumn.ColumnName;
                e.NewValues[columnName] = drParams[columnName];
            }
        }

        protected void dxSearch_CardInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e)
        {
            dxSearch.SettingsPager.SettingsTableLayout.RowsPerPage = 1;
            e.Cancel = true;
        }

        protected void dxSearch_CancelCardEditing(object sender, ASPxStartCardEditingEventArgs e)
        {
            dxSearch.SettingsPager.SettingsTableLayout.RowsPerPage = 1;
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers (Card)
        // --------------------------------------------------------------------------------------------------

        protected void dxCard_Init(object sender, EventArgs e)
        {
            bool isVisible = IsCardView && CiTable != null && !CiTable.IsSearching;

            dxCard.Visible = isVisible;

            if (isVisible)
            {
                BuildCardView(dxCard, false);
                BuildCardToolbar();

				if (!IsPostBack)
				{
					dxCard.JSProperties["cpMode"] = Mode;
				}
			}

            if (UiParentTable == null)
            {
                dxCard.JSProperties["cpIsRootTable"] = true;
            }

            if (CiTable != null)
            {
                dxCard.JSProperties["cpBubbleUpdate"] = CiTable.BubbleUpdate;
            }
        }

        protected void dxCard_CustomJSProperties(object sender, ASPxCardViewClientJSPropertiesEventArgs e)
        {
            e.Properties["cpDisabledMacros"] = GetDisabledMacros();
            if (CiTable != null)
            {
                e.Properties["cpPuxFile"] = CiTable.PuxFile;
            }
        }

        protected void dxCard_InitNewCard(object sender, DevExpress.Web.Data.ASPxDataInitNewRowEventArgs e)
        {
            dxCard.SettingsPager.SettingsTableLayout.RowsPerPage = 0;

            dxCard.JSProperties["cpFollowerFields"] = RunDefaultMacro(e);
        }

        protected void dxCard_CardInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e)
        {
            dxCard.SettingsPager.SettingsTableLayout.RowsPerPage = 1;
        }

        protected void dxCard_CancelCardEditing(object sender, ASPxStartCardEditingEventArgs e)
        {
            dxCard.SettingsPager.SettingsTableLayout.RowsPerPage = 1;
            dxCard.DataBind(); // Allows label text to be displayed

            if (dxCard.IsNewCardEditing && CiTable != null)
            {
                foreach (string key in CiTable.RowKeyNames)
                {
                    SetClientValue(key, GetServerValue(key));
                }
            }
        }

        protected void dxCard_ToolbarItemClick(object source, ASPxCardViewToolbarItemClickEventArgs e)
        {
            if (CiTable != null && !MyWebUtils.IsTimeOutReached(Page))
            {
                string macroName = UiApplication.Me.GetCommandFired(CiTable.TableName);

                switch (macroName)
                {
                    case "New":
                    case "Edit":
                        dxCard.SettingsPager.Visible = false;
                        break;

                    case "Delete":
                    case "Update":
                    case "Cancel":
                        dxCard.SettingsPager.Visible = true;
                        break;

                    default:
						dxCard.JSProperties["cpScript"] = RunMacro(macroName);
                        break;
                }
            }
        }

        protected void dxCard_CustomCallback(object sender, ASPxCardViewCustomCallbackEventArgs e)
        {
            if (!MyWebUtils.IsTimeOutReached(Page))
            {
                string tableName = e.Parameters;
                if (!MyUtils.IsEmpty(tableName))
                {
                    UiApplication.Me.ClearState(tableName);
                }

                dxCard.DataBind();
            }
        }

		protected void dxCard_CustomColumnDisplayText(object sender, ASPxCardViewColumnDisplayTextEventArgs e)
		{
			ApplyFieldStyles(e.Column.FieldName, e);
		}

        protected void pgCardTabs_Init(object sender, EventArgs e)
        {
            if (CiTable != null && MyUtils.IsEmpty(CiTable.LayoutUrl))
            {
                ASPxPageControl pgCardTabs = sender as ASPxPageControl;

                if (pgCardTabs != null && pgCardTabs.TabPages.Count == 0 && CiTable != null)
                {
                    foreach (CiTable ciChildTable in CiTable.Get<CiTable>())
                    {
                        IUiColumn uiColumn = this.CreateUiPlugin(ciChildTable) as IUiColumn;
                        if (uiColumn != null)
                        {
                            uiColumn.BuildCardColumn(pgCardTabs);
                        }
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers (Grid)
        // --------------------------------------------------------------------------------------------------

        protected void dxGrid_Init(object sender, EventArgs e)
        {
            bool isVisible = IsGridView && CiTable != null && !CiTable.IsSearching;

            dxGrid.Visible = isVisible;

            if (isVisible)
            {
                BuildGridView(dxGrid);
                BuildGridToolbar();

				if (!IsPostBack)
				{
					dxGrid.JSProperties["cpMode"] = Mode;
				}
			}

            if (UiParentTable == null)
            {
                dxGrid.JSProperties["cpIsRootTable"] = true;
            }

            if (CiTable != null)
            {
                string tableName = CiTable.TableName;
                dxPopupMenu.JSProperties["cpTableName"] = tableName;
                dxOpenMenuPanel.JSProperties["cpTableName"] = tableName;
                dxClickMenuPanel.JSProperties["cpTableName"] = tableName;
                dxLoadingPanel.JSProperties["cpTableName"] = tableName;

                dxGrid.JSProperties["cpBubbleUpdate"] = CiTable.BubbleUpdate;
            }
        }

        protected void dxGrid_CustomJSProperties(object sender, ASPxGridViewClientJSPropertiesEventArgs e)
        {
            e.Properties["cpDisabledMacros"] = GetDisabledMacros();
            if (CiTable != null)
            {
                e.Properties["cpPuxFile"] = CiTable.PuxFile;
            }
        }

        protected void dxGrid_BeforeColumnSortingGrouping(object sender, ASPxGridViewBeforeColumnGroupingSortingEventArgs e)
        {
            if (dxGrid.Visible)
            {
                // Expand all rows
                dxGrid.ExpandAll();
            }
        }

        protected void dxGrid_InitNewRow(object sender, DevExpress.Web.Data.ASPxDataInitNewRowEventArgs e)
        {
			dxGrid.JSProperties["cpFollowerFields"] = RunDefaultMacro(e);
        }

        protected void dxGrid_ToolbarItemClick(object source, DevExpress.Web.Data.ASPxGridViewToolbarItemClickEventArgs e)
        {
            if (!MyWebUtils.IsTimeOutReached(Page))
            {
                BuildGridToolbar();

                string macroName = UiApplication.Me.GetCommandFired(CiTable.TableName);

                switch (macroName)
                {
                    case "New":
                    case "Edit":
                    case "Delete":
                    case "Update":
                    case "Cancel":
                        // Do nothing
                        break;

                    default:
                        dxGrid.JSProperties["cpScript"] = RunMacro(macroName);
                        break;
                }
            }
        }

		protected void dxGrid_CustomCallback(object sender, ASPxGridViewCustomCallbackEventArgs e)
		{
            if (!MyWebUtils.IsTimeOutReached(Page))
            {
                string tableName = e.Parameters;
                if (!MyUtils.IsEmpty(tableName))
                {
                    UiApplication.Me.ClearState(tableName);
                }

                dxGrid.DataBind();
            }
		}

		protected void dxGrid_SummaryDisplayText(object sender, ASPxGridViewSummaryDisplayTextEventArgs e)
		{
			if (e.IsTotalSummary)
			{
				if (e.Item.SummaryType == DevExpress.Data.SummaryItemType.Sum)
				{
					e.EncodeHtml = false;
					object value = e.Value;

					if (CiTable != null)
					{
						CiField ciField = CiTable.GetField(e.Item.ShowInColumn);
						if (ciField != null && ciField.Mask == eTextMask.Currency)
						{
							value = string.Format("{0:f2}", value);
						}
					}

					e.Text = string.Format("<span style='font-weight:bold'>{0}</span>", value);
				}
			}
		}

		protected void dxGrid_CustomColumnDisplayText(object sender, ASPxGridViewColumnDisplayTextEventArgs e)
		{
			ApplyFieldStyles(e.Column.FieldName, e);
		}

        protected void pgGridTabs_Init(object sender, EventArgs e)
        {
            ASPxPageControl pgGridTabs = sender as ASPxPageControl;

            if (pgGridTabs != null && pgGridTabs.TabPages.Count == 0 && CiTable != null)
            {
				foreach (CiTable ciChildTable in CiTable.Get<CiTable>())
                {
                    IUiColumn uiColumn = this.CreateUiPlugin(ciChildTable) as IUiColumn;
                    if (uiColumn != null)
                    {
                        uiColumn.BuildGridColumn(pgGridTabs);
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers (Context menu for Grid)
        // --------------------------------------------------------------------------------------------------

        protected void dxOpenMenuPanel_Callback(object sender, CallbackEventArgsBase e)
        {
			string parameter = e.Parameter;
            string script = "";
            string doubleClickMacroName = CiTable.DoubleClickMacroName;

            if (!MyUtils.IsEmpty(parameter))
			{
				string[] parameters = parameter.Split(LIST_SEPARATOR.ToCharArray());

				if (parameters.Length == 2)
				{
					string xPos = parameters[0];
					string yPos = parameters[1];

					dxOpenMenuPanel.JSProperties["cpMouseCoordinates"] = OpenMenu(xPos, yPos);
				}
			}
			else if (IsBrowsing && !MyUtils.IsEmpty(doubleClickMacroName))
            {
                script = RunMacro(doubleClickMacroName);
            }

            dxOpenMenuPanel.JSProperties["cpScript"] = script;
        }

        protected void dxClickMenuPanel_Callback(object source, CallbackEventArgs e)
        {
			dxClickMenuPanel.JSProperties["cpScript"] = RunMacro(e.Parameter);
		}

        // --------------------------------------------------------------------------------------------------
        // Event Handlers (ObjectDataSource)
        // --------------------------------------------------------------------------------------------------

        protected void MyTableData_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            e.InputParameters["table"] = CiTable;
            e.InputParameters["parameters"] = GetState();
        }

        protected void MyTableData_Updating(object sender, ObjectDataSourceMethodEventArgs e)
        {
            e.InputParameters["table"] = CiTable;
            e.InputParameters["parameters"] = GetState();
        }

        protected void MyTableData_Inserting(object sender, ObjectDataSourceMethodEventArgs e)
        {
            e.InputParameters["table"] = CiTable;
            e.InputParameters["parameters"] = GetState();
        }

        protected void MyTableData_Inserted(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if (e.OutputParameters.Count > 0 && CiTable != null)
            {
                object rowKey = MyUtils.Coalesce(e.OutputParameters["RowKey"], "");

                if (IsCardView)
                {
                    dxCard.JSProperties["cpInsertedRowIndex"] = dxCard.FindVisibleIndexByKeyValue(rowKey);
                }
                else if (IsGridView)
                {
                    dxGrid.JSProperties["cpInsertedRowIndex"] = dxGrid.FindVisibleIndexByKeyValue(rowKey);
                }

                if (!MyUtils.IsEmpty(rowKey))
                {
                    string[] keyNames = CiTable.RowKeyNames;
                    string[] keyValues = (rowKey as string).Trim().Split(',');

                    for (int i = 0; i < keyNames.Length && i < keyValues.Length; i++)
                    {
                        string keyName = keyNames[i];
                        object keyValue = keyValues[i];

                        this[keyName] = keyValue;
                    }
                }
            }
        }

        protected void MyTableData_Deleting(object sender, ObjectDataSourceMethodEventArgs e)
        {
            e.InputParameters["table"] = CiTable;
            e.InputParameters["parameters"] = GetState();
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Overridden)
        // --------------------------------------------------------------------------------------------------

        public override int GetFocusedIndex()
        {
            string defaultView = (CiTable != null) ? CiTable.DefaultView : null;
            int focusedIndex = -1;

            if (IsCardView)
            {
                focusedIndex = dxCard.IsNewCardEditing ? dxCard.VisibleCardCount : dxCard.FocusedCardIndex;
            }
            else
            {
                focusedIndex = dxGrid.IsNewRowEditing ? dxGrid.VisibleRowCount : dxGrid.FocusedRowIndex;
            }

            return focusedIndex;
        }

        public override string GetFormattedKey(string key)
        {
            if (CiTable != null)
            {
                return string.Format("{0}.{1}", CiTable.TableName, key);
            }

            return null;
        }

        public override object GetServerValue(string key, int rowIndex = -1)
        {
            string defaultView = (CiTable != null) ? CiTable.DefaultView : null;

            return IsCardView ? GetCardValue(key, rowIndex) : GetGridValue(key, rowIndex);
        }

        public override DataRow GetState(int rowIndex = -1)
        {
            if (rowIndex == -1)
            {
                rowIndex = GetFocusedIndex();
            }

            if (CiTable != null)
            {
                DataTable dt = new DataTable();
                DataColumnCollection dcColumns = dt.Columns;
                DataRow dr = dt.NewRow();
                dt.Rows.Add(dr);

                foreach (CiField ciField in CiTable.CiFields)
                {
                    string fieldName = ciField.FieldName;

                    object fieldValue = this[fieldName, rowIndex];

                    if (!dcColumns.Contains(fieldName))
                    {
                        Type type = (!MyUtils.IsEmpty(fieldValue))
                           ? fieldValue.GetType()
                           : typeof(string);

                        if (ciField.IsInstanceOf(typeof(CiDateField)))
                        {
                            type = typeof(DateTime);
                            fieldValue = !MyUtils.IsEmpty(fieldValue)
                                ? Convert.ToDateTime(fieldValue)
                                : Convert.DBNull;
                        }
                        else if (ciField.IsInstanceOf(typeof(CiCheckField)))
                        {
                            type = typeof(bool);
                            fieldValue = !MyUtils.IsEmpty(fieldValue)
                                ? Convert.ToBoolean(fieldValue)
                                : Convert.DBNull;
                        }
                        else if (ciField.IsInstanceOf(typeof(CiTextField)) && ciField.Mask == eTextMask.Currency)
                        {
                            type = typeof(decimal);
                            fieldValue = !MyUtils.IsEmpty(fieldValue)
                                ? Convert.ToDecimal(fieldValue)
                                : Convert.DBNull;
                        }

                        dcColumns.Add(fieldName, type);
                    }

                    dr[fieldName] = fieldValue;
                }

                if (UiParentTable != null)
                {
                    dr = MyWebUtils.AppendColumns(dr, UiParentTable.GetState(), false);
                }

                return dr;
            }

            return null;
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual void InstantiateIn(Control container)
        {
            // Required when implementing ITemplate interface
            if (container != null)
            {
                container.Controls.Add(this);
            }
        }

        public virtual void BuildCardColumn(Control container)
        {
            ASPxPageControl pgCardTabs = container as ASPxPageControl;
            if (pgCardTabs != null && CiTable != null)
            {
                TabPage tabPage = pgCardTabs.TabPages.Add(CiTable.TableName);
                tabPage.Text = CiTable.TableCaption;
                tabPage.ToolTip = CiTable.TableCaption;
                tabPage.Controls.Add(this);

                ID += "_" + tabPage.Index.ToString();
            }
        }

        public virtual void BuildGridColumn(Control container)
        {
            ASPxPageControl pgGridTabs = container as ASPxPageControl;
            if (pgGridTabs != null && CiTable != null)
            {
                TabPage tabPage = pgGridTabs.TabPages.Add(CiTable.TableName);
                tabPage.Text = CiTable.TableCaption;
                tabPage.ToolTip = CiTable.TableCaption;
                tabPage.Controls.Add(this);

                ID += "_" + tabPage.Index.ToString();
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private void BuildCardView(ASPxCardView dxTable, bool isSearchMode)
        {
            if (CiTable != null)
            {
                int totalColumns = isSearchMode ? 1 : CiTable.ColCount;
                string titlePrefix = isSearchMode ? "Search for: " : "";

                dxTable.Styles.TitlePanel.BackColor = isSearchMode ? Color.SpringGreen : Color.Transparent;
                dxTable.SettingsText.Title = titlePrefix + CiTable.TableCaption;

                dxTable.CardLayoutProperties.ColCount = totalColumns;

                // Set JSProperties
                dxTable.JSProperties["cpTableName"] = CiTable.TableName;

                // Build field columns
                CiField[] ciFields = isSearchMode ? CiTable.CiSearchableFields : CiTable.CiBrowsableFields;
                foreach (CiField ciField in ciFields)
                {
                    BuildCardColumn(dxTable, ciField);
                }

                if (!isSearchMode && CiTable.LayoutXml != null)
                {
                    dxTable.Templates.Card = CreateLayout(false);
                    dxTable.Templates.EditForm = CreateLayout(true);
                }
            }
        }

        private void BuildGridView(ASPxGridView dxTable)
        {
            if (CiTable != null)
            {
                dxTable.SettingsText.Title = CiTable.TableCaption;
                dxTable.SettingsDetail.ShowDetailRow = (CiTable.Get<CiTable>().Length > 0);
                dxTable.SettingsEditing.Mode = GridViewEditingMode.Inline;

                if (CiTable.PageSize > 0)
                {
                    dxTable.SettingsPager.Mode = GridViewPagerMode.ShowPager;
                    dxTable.SettingsPager.PageSize = CiTable.PageSize;
                }

                // Set JSProperties
                dxTable.JSProperties["cpTableName"] = CiTable.TableName;
                dxTable.JSProperties["cpQuickInsert"] = CiTable.QuickInsert;

                // Build field columns
                CiField[] ciFields = CiTable.CiBrowsableFields;
                foreach (CiField ciField in ciFields)
                {
                    BuildGridColumn(dxTable, ciField);
                }
            }
        }

        private void BuildCardColumn(ASPxCardView dxTable, CiField ciField)
        {
            if (ciField != null)
            {
                string textFieldName = ciField.TextFieldName;
                string fieldName = !MyUtils.IsEmpty(textFieldName) ? textFieldName : ciField.FieldName;

                CardViewColumn dxColumn = dxTable.Columns.Cast<CardViewColumn>()
                    .SingleOrDefault(c => c.FieldName == fieldName);

                if (dxColumn == null)
                {
                    dxColumn = ciField.CreateCardColumn(this);
					dxColumn.FieldName = fieldName;
					dxTable.Columns.Add(dxColumn);
                }

				ciField.FormatCardColumn(dxColumn);
            }
        }

        private void BuildGridColumn(ASPxGridView dxTable, CiField ciField)
        {
            if (ciField != null)
            {
                string textFieldName = ciField.TextFieldName;
                string fieldName = !MyUtils.IsEmpty(textFieldName) ? textFieldName : ciField.FieldName;

                GridViewDataColumn dxColumn = dxTable.DataColumns.Cast<GridViewDataColumn>()
                    .SingleOrDefault(c => c.FieldName == fieldName);

                if (dxColumn == null && !ciField.Searchable)
                {
                    dxColumn = ciField.CreateGridColumn(this);
					dxColumn.FieldName = fieldName;
					dxTable.Columns.Add(dxColumn);
                }

				ciField.FormatGridColumn(dxColumn);
            }
        }

        private void BuildCardToolbar()
        {
            CardViewToolbar toolbar = dxCard.Toolbars[0];
            if (toolbar != null)
            {
                ArrayList macroNames = GetVisibleMacros();

                // Setup standard toolbar buttons
                foreach (CardViewToolbarItem button in toolbar.Items)
                {
                    button.Visible = macroNames.IndexOf(button.Name) >= 0;
                }

                // Setup custom toolbar buttons
                foreach (string macroName in macroNames)
                {
                    if (toolbar.Items.FindByName(macroName) == null && CiTable != null)
                    {
						CiMacro ciMacro = CiTable.GetMacro(macroName);
						if (ciMacro != null)
						{
							CardViewToolbarItem button = new CardViewToolbarItem();
							button.Name = macroName;
							button.Text = ciMacro.MacroCaption;
							button.Command = CardViewToolbarCommand.Custom;
							button.BeginGroup = true;
							toolbar.Items.Add(button);
						}
                    }
                }
            }
        }

        private void BuildGridToolbar()
        {
            GridViewToolbar toolbar = dxGrid.Toolbars[0];
            if (toolbar != null)
            {
                ArrayList macroNames = GetVisibleMacros();

                // Setup standard toolbar buttons
                foreach (GridViewToolbarItem button in toolbar.Items)
                {
                    button.Visible = macroNames.IndexOf(button.Name) >= 0;
                }

				// Setup custom toolbar buttons
				foreach (string macroName in macroNames)
				{
					if (toolbar.Items.FindByName(macroName) == null && CiTable != null)
					{
						CiMacro ciMacro = CiTable.GetMacro(macroName);
						if (ciMacro != null)
						{
							GridViewToolbarItem button = new GridViewToolbarItem();
							button.Name = macroName;
							button.Text = ciMacro.MacroCaption;
							button.Command = GridViewToolbarCommand.Custom;
							button.BeginGroup = true;
							toolbar.Items.Add(button);
						}
					}
				}
			}
        }

        private string RunDefaultMacro(DevExpress.Web.Data.ASPxDataInitNewRowEventArgs e)
        {
			ArrayList changedColumnNames = new ArrayList();
			Hashtable followerFields = new Hashtable();
			string followerFieldNames = "";

            if (CiTable != null)
            {
                DataRow drParams = GetState();

                CiMacro ciMacro = CiTable.DefaultMacro;
                if (ciMacro != null)
                {
                    DataTable dt = ciMacro.RunSQL(drParams);
                    if (MyWebUtils.GetNumberOfRows(dt) > 0 && MyWebUtils.GetNumberOfColumns(dt) > 0)
                    {
                        DataRow drResult = dt.Rows[0];

                        foreach (DataColumn dcColumn in dt.Columns)
                        {
                            string columnName = dcColumn.ColumnName;
                            object columnValue = drResult[columnName];

                            e.NewValues[columnName] = columnValue;
                            this[columnName] = columnValue;

                            changedColumnNames.Add(columnName);
                        }
                    }
                }
                else
                {
                    foreach (DataColumn dcColumn in drParams.Table.Columns)
                    {
                        string columnName = dcColumn.ColumnName;

                        e.NewValues[columnName] = null;
                        this[columnName] = null;

                        changedColumnNames.Add(columnName);
					}
                }

				foreach (string columnName in changedColumnNames)
				{
					CiField ciField = CiTable.GetField(columnName);
					if (ciField != null)
					{
						CiField[] ciFollowerFields = ciField.CiFollowerFields;
						foreach (CiField ciFollowerField in ciFollowerFields)
						{
							followerFields[ciFollowerField.FieldName] = ciFollowerField;
						}
					}
				}

				string tableName = CiTable.TableName;
				foreach (string fieldName in followerFields.Keys)
				{
					if (followerFieldNames.Length > 0)
					{
						followerFieldNames += LIST_SEPARATOR;
					}

					followerFieldNames += tableName + "." + fieldName;
				}
			}

			return followerFieldNames;
        }

        private object GetCardValue(string key, int rowIndex = -1)
        {
            if (CiTable != null)
            {
                if (rowIndex == -1)
                {
                    rowIndex = IsInserting ? -1 :
                        (CiTable.IsSearching ? 0 : dxCard.FocusedCardIndex);
                }

                if (rowIndex >= 0)
                {
                    try
                    {
                        return dxCard.GetCardValues(rowIndex, key);
                    }
                    catch
                    {
                        // Do nothing
                    }
                }
            }

            return null;
        }

        private object GetGridValue(string key, int rowIndex = -1)
        {
			if (rowIndex == -1)
			{
				rowIndex = IsInserting ? -1 :
				   (CiTable.IsSearching ? 0 : dxGrid.FocusedRowIndex);
			}

            if (rowIndex >= 0)
            {
                try
                {
                    return dxGrid.GetRowValues(rowIndex, key);
                }
                catch
                {
                    // Do nothing
                }
            }

            return null;
        }

        private ArrayList GetVisibleMacros()
        {
            ArrayList macros = new ArrayList();
            macros.Add("Inspect");
            macros.Add("ExportToPdf");
            macros.Add("Divider");

            if (CiTable != null)
            {
                CiField[] ciSearchableFields = CiTable.CiSearchableFields;
                bool isSearchable = (ciSearchableFields != null) && (ciSearchableFields.Length > 0);
                bool isEditing = IsEditing;
                bool isBrowsing = IsBrowsing && !isEditing;
                bool isTopTable = (UiParentTable == null);
                bool isParentInserting = (UiParentTable != null) && UiParentTable.IsInserting;

                if (isSearchable && isBrowsing && isTopTable)
                {
                    macros.Add("Search");
                }

                if (!MyUtils.IsEmpty(CiTable.InsertMacro) && isBrowsing && !isParentInserting)
                {
                    macros.Add("New");
                }

                if (!MyUtils.IsEmpty(CiTable.UpdateMacro) && isBrowsing && !isParentInserting)
                {
                    macros.Add("Edit");
                }

                if (!MyUtils.IsEmpty(CiTable.DeleteMacro) && isBrowsing && !isParentInserting)
                {
                    macros.Add("Delete");
                }

                if (isEditing)
                {
                    macros.Add("Update");
                    macros.Add("Cancel");
                }

                if (isBrowsing)
                {
                    foreach (CiMacro ciMacro in CiTable.CiMacros)
                    {
                        if (ciMacro != null)
                        {
							if (IsCardView || IsGridView && ciMacro.GetActionParameterNames().Count == 0)
							{
								string macroName = ciMacro.MacroName;
								macros.Add(macroName);
							}
                        }
                    }
                }
            }

            return macros;
        }

        private string GetDisabledMacros()
        {
            string disabledMacros = "";

            if (CiTable != null)
            {
                DataRow drParams = GetState();

                CiMacro insertMacro = CiTable.InsertMacro;
                if (insertMacro == null || !insertMacro.IsVisible(drParams))
                {
                    disabledMacros += LIST_SEPARATOR + "New";
                }

                CiMacro updateMacro = CiTable.UpdateMacro;
                if (updateMacro == null || !updateMacro.IsVisible(drParams) || !RecordsExist)
                {
                    disabledMacros += LIST_SEPARATOR + "Edit";
                }

                CiMacro deleteMacro = CiTable.DeleteMacro;
                if (deleteMacro == null || !deleteMacro.IsVisible(drParams) || !RecordsExist)
                {
                    disabledMacros += LIST_SEPARATOR + "Delete";
                }

                if (IsBrowsing)
                {
                    foreach (CiMacro ciMacro in CiTable.CiMacros)
                    {
                        if (ciMacro != null)
                        {
                            int numberOfSqlParameters = MyUtils.GetParameters(ciMacro.VisibleSQL).Count;
                            bool visibleForRecord = RecordsExist && ciMacro.IsVisible(drParams);

                            if (numberOfSqlParameters > 0 && !visibleForRecord)
                            {
                                string macroName = ciMacro.MacroName;
                                if (disabledMacros.Length > 0)
                                {
                                    disabledMacros += LIST_SEPARATOR;
                                }

                                disabledMacros += macroName;
                            }
                        }
                    }
                }
            }

            return disabledMacros;
        }

        private string OpenMenu(string xPos, string yPos)
        {
            string mouseCoordinates = xPos + LIST_SEPARATOR + yPos;
            dxPopupMenu.Items.Clear();

            if (IsBrowsing && CiTable != null)
            {
                DataRow drParams = GetState();
                string doubleClickMacroName = CiTable.DoubleClickMacroName;

                foreach (CiMacro ciMacro in CiTable.CiMacros)
                {
                    if (IsGridView && ciMacro != null && ciMacro.GetActionParameterNames().Count > 0 && ciMacro.IsVisible(drParams))
                    {
                        string macroName = ciMacro.MacroName;
                        DevExpress.Web.MenuItem menuItem = dxPopupMenu.Items.Add(ciMacro.MacroCaption, macroName);

                        if (doubleClickMacroName == macroName)
                        {
                            menuItem.Checked = true;
                        }
                    }
                }
            }

            return mouseCoordinates;
        }

        private string RunMacro(string macroName)
        {
            string script = "";

            if (CiTable != null && !MyUtils.IsEmpty(macroName))
            {
				CiMacro ciMacro = CiTable.GetMacro(macroName);
                if (ciMacro != null)
                {
                    script = ciMacro.Run(GetState());
                }
            }

            return script;
        }

		private void ApplyFieldStyles(string fieldName, ASPxGridColumnDisplayTextEventArgs e)
		{
			if (CiTable != null)
			{
				CiField ciField = CiTable.GetField(fieldName);
				if (ciField != null)
				{
					string foreColor = ciField.ForeColor;
					if (!MyUtils.IsEmpty(foreColor))
					{
						e.EncodeHtml = false;
						e.DisplayText = string.Format("<span style=\"color:{0}\">{1}</span>", foreColor, e.Value);
					}
				}
			}
		}

        protected void pgCardTabs_TabClick(object source, TabControlCancelEventArgs e)
        {

        }
    }
}