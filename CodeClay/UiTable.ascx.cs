using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
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

        private DataSet mDataSet = new DataSet();
        private CiMacro mSelectMacro = null;
        private CiMacro mUpdateMacro = null;
        private CiMacro mInsertMacro = null;
        private CiMacro mDeleteMacro = null;
        private CiMacro mDefaultMacro = null;
        private ArrayList mValidateSQL = new ArrayList();

        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("TableName")]
        public string TableName { get; set; } = "";

        [XmlSqlElement("TableCaption", typeof(string))]
        public XmlElement TableCaption { get; set; } = MyWebUtils.CreateXmlElement("TableCaption", "");

        [XmlElement("QuickInsert")]
        public bool QuickInsert { get; set; } = false;

        [XmlElement("InsertRowAtBottom")]
        public bool InsertRowAtBottom { get; set; } = true;

        [XmlElement("PageSize")]
        public int PageSize { get; set; } = 100;

        [XmlElement("DefaultView")]
        public string DefaultView { get; set; } = "Grid";

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
						mSelectMacro.Caption = value.Caption;
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
						mUpdateMacro.Caption = value.Caption;
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
						mInsertMacro.Caption = value.Caption;
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
						mDeleteMacro.Caption = value.Caption;
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

        [XmlElement("ValidateSQL")]
        public string[] ValidateSQL
        {
            get
            {
                return (string[])mValidateSQL.ToArray(typeof(string));
            }
            set
            {
                if (value != null)
                {
                    string[] ValidateSQL = value;
                    mValidateSQL.Clear();
                    foreach (string sql in ValidateSQL)
                    {
                        mValidateSQL.Add(sql);
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
                x.LoadXml(mDataSet.GetXml());
                return x.DocumentElement;
            }

            set
            {
                string innerXml = value.InnerXml;

                if (!MyUtils.IsEmpty(innerXml))
                {
                    StringReader reader = new StringReader(innerXml);

                    mDataSet.ReadXml(reader);
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Properties (Derived)
        // --------------------------------------------------------------------------------------------------

        [XmlIgnore]
        public override string ID
        {
            get { return TableName; }
        }

        [XmlIgnore]
        public bool IsSearching
        {
            get
            {
                return MyUtils.Coalesce(MyWebUtils.QueryStringCommand,
                    UiApplication.Me.GetCommandFired(TableName)) == "Search";
            }
        }

        [XmlIgnore]
        public DataTable DataTable
        {
            get
            {
                if (mDataSet != null && mDataSet.Tables.Count > 0)
                {
                    return mDataSet.Tables[0];
                }

                return null;
            }

            set
            {
                if (value != null)
                {
                    DataSet dataSet = value.DataSet;
                    if (dataSet != null)
                    {
                        mDataSet = dataSet;
                    }
                    else
                    {
                        mDataSet = new DataSet();
                        mDataSet.Tables.Add(value);
                    }
                }
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
                        if (!ciField.IsVisible)
                        {
                            sortedFields[--totalVisibleFields] = ciField;
                        }
                    }

                    int totalColumns = ColCount;
                    int rowNumber = 0;
                    int columnNumber = 0;

                    foreach (CiField ciField in ciFields)
                    {
                        if (ciField.IsVisible)
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
                    sortedFields = ciFields.Where(c => c.Browsable).ToArray();
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

        public virtual string RunValidateSQL(DataRow drParams)
        {
            foreach (string validateSQL in ValidateSQL)
            {
                DataTable dt = UiApplication.Me.GetBySQL(validateSQL, drParams);

                if (dt != null && MyWebUtils.GetNumberOfRows(dt) > 0 && MyWebUtils.GetNumberOfColumns(dt) > 0)
                {
                    return MyUtils.Coalesce(dt.Rows[0][0], "").ToString();
                }
            }

            return null;
        }
    }

    public partial class UiTable : UiPlugin, IUiColumn
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

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
                    dxGrid.IsNewRowEditing;

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
                    dxGrid.IsEditing || dxGrid.IsNewRowEditing;

                bool isFinishEditing = (command == "Update") || (command == "Cancel");

                return isStartEditing && !isFinishEditing;
            }
        }

        public bool IsParentEditing
        {
            get
            {
                return (UiParentTable != null && UiParentTable.IsEditing);
            }
        }

        public bool IsChildEditing
        {
            get
            {
                if (CiTable != null)
                {
                    foreach (CiTable ciChildTable in CiTable.CiTables)
                    {
                        string command = UiApplication.Me.GetCommandFired(ciChildTable.TableName);

                        bool isStartEditing = (command == "Edit") || (command == "New");

                        bool isFinishEditing = (command == "Update") || (command == "Cancel");

                        if (isStartEditing && !isFinishEditing)
                        {
                            return true;
                        }
                    }
                }

                return false;
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

        public string ErrorFieldName { get; set; } = null;

        public string ErrorMessage { get; set; } = null;

        public string ClientScript
        {
            get
            {
                if (IsCardView && dxCard.JSProperties.ContainsKey("cpScript"))
                {
                    return MyUtils.Coalesce(dxCard.JSProperties["cpScript"], "").ToString();
                }
                else if (IsGridView && dxGrid.JSProperties.ContainsKey("cpScript"))
                {
                    return MyUtils.Coalesce(dxGrid.JSProperties["cpScript"], "").ToString();
                }

                return "";
            }

            set
            {
                if (IsCardView)
                {
                    dxCard.JSProperties["cpScript"] = value;
                }
                else if (IsGridView)
                {
                    dxGrid.JSProperties["cpScript"] = value;
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && CiTable != null && UiParentTable == null)
            {
                Page.Title = MyWebUtils.Eval<string>(CiTable.TableCaption, GetState());
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
                StoreQueryStringCommand();
                BuildCardView(dxSearch, true);
                dxSearch.AddNewCard();
            }
        }

        protected void dxSearch_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && dxSearch.Visible)
            {
                RunQueryStringCommand();
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

            DataRow drParams = GetState(-2);

            foreach (DataColumn dcColumn in drParams.Table.Columns)
            {
                string columnName = dcColumn.ColumnName;
                string searchableColumnName = "";
                object columnValue = drParams[columnName];

                if (CiTable != null)
                {
                    CiField ciField = CiTable.GetField(columnName);
                    if (ciField != null && ciField.Searchable)
                    {
                        searchableColumnName = ciField.SearchableFieldName;
                    }
                }

                e.NewValues[columnName] = columnValue;
                if (!MyUtils.IsEmpty(searchableColumnName))
                {
                    e.NewValues[searchableColumnName] = columnValue;
                }
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
            bool isVisible = IsCardView && (CiTable != null) && !CiTable.IsSearching;

            dxCard.Visible = isVisible;

            if (isVisible)
            {
                StoreQueryStringCommand();
                BuildCardView(dxCard, false);
                BuildCardToolbar();
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

        protected void dxCard_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && dxCard.Visible)
            {
                RunQueryStringCommand();
            }
        }

        protected void dxCard_CustomJSProperties(object sender, ASPxCardViewClientJSPropertiesEventArgs e)
        {
            e.Properties["cpDisabledMacros"] = GetDisabledMacros();
            if (CiTable != null)
            {
                e.Properties["cpPuxFile"] = CiTable.PuxFile;
            }

            if (UiParentTable != null)
            {
                e.Properties["cpParentDisabledMacros"] = UiParentTable.GetDisabledMacros();
                e.Properties["cpParentTableName"] = UiParentTable.CiTable.TableName;
            }
        }

        protected void dxCard_InitNewCard(object sender, DevExpress.Web.Data.ASPxDataInitNewRowEventArgs e)
        {
            dxCard.SettingsPager.SettingsTableLayout.RowsPerPage = 0;

            dxCard.JSProperties["cpFollowerFields"] = RunDefaultMacro(e);
        }

        protected void dxCard_CardValidating(object sender, ASPxCardViewDataValidationEventArgs e)
        {
            DoValidation();
        }

        protected void dxCard_CardInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e)
        {
            dxCard.SettingsPager.SettingsTableLayout.RowsPerPage = 1;
            e.Cancel = !ValidationOK("New");
        }

        protected void dxCard_CardUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            e.Cancel = !ValidationOK("Edit");
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

                        // Display hidden fields when developer clicks on Inspect button
                        dxCard.JSProperties["cpScript"] = GetInitFieldScript();
                        break;

                    case "Delete":
                        dxCard.SettingsPager.Visible = true;
                        break;

                    case "Update":
                    case "Cancel":
                        CiMacro ciMacro = IsInserting ? CiTable.InsertMacro : CiTable.UpdateMacro;
                        string navigateUrl = ciMacro.NavigateUrl + "|" + ciMacro.NavigatePos;
                        if (!MyUtils.IsEmpty(navigateUrl))
                        {
                            dxCard.JSProperties["cpScript"] = string.Format("GotoUrl(\'{0}\')", HttpUtility.JavaScriptStringEncode(navigateUrl));
                        }
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
			SetupFieldDisplay(dxCard, e.Column.FieldName, e);
		}

        protected void pgCardTabs_Init(object sender, EventArgs e)
        {
            if (CiTable != null && MyUtils.IsEmpty(CiTable.LayoutUrl))
            {
                ASPxPageControl pgCardTabs = sender as ASPxPageControl;

                if (pgCardTabs != null && pgCardTabs.TabPages.Count == 0 && CiTable != null)
                {
                    pgCardTabs.ID = string.Format("pgCardTabs_{0}", CiTable.TableName);
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
                StoreQueryStringCommand();
                BuildGridView(dxGrid);
                BuildGridToolbar();
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

        protected void dxGrid_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && dxGrid.Visible)
            {
                RunQueryStringCommand();
            }
        }

        protected void dxGrid_CustomJSProperties(object sender, ASPxGridViewClientJSPropertiesEventArgs e)
        {
            e.Properties["cpDisabledMacros"] = GetDisabledMacros();
            if (CiTable != null)
            {
                e.Properties["cpPuxFile"] = CiTable.PuxFile;
            }

            if (UiParentTable != null)
            {
                e.Properties["cpParentDisabledMacros"] = UiParentTable.GetDisabledMacros();
                e.Properties["cpParentTableName"] = UiParentTable.CiTable.TableName;
            }
        }

        protected void dxGrid_RowValidating(object sender, DevExpress.Web.Data.ASPxDataValidationEventArgs e)
        {
            DoValidation();
        }

        protected void dxGrid_RowInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e)
        {
            e.Cancel = !ValidationOK("New");
        }

        protected void dxGrid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            e.Cancel = !ValidationOK("Edit");
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
                        // Display hidden fields when developer clicks on Inspect button
                        dxGrid.JSProperties["cpScript"] = GetInitFieldScript();
                        break;

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
			SetupFieldDisplay(dxGrid, e.Column.FieldName, e);
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

                if (MyUtils.IsEmpty(script) && IsGridView)
                {
                    script = "dxOpenMenuPanel.Grid.Refresh()";
                }
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
            if (!CiTable.IsSearching)
            {
                e.InputParameters["table"] = CiTable;
                e.InputParameters["parameters"] = GetState();
            }
        }

        protected void MyTableData_Inserting(object sender, ObjectDataSourceMethodEventArgs e)
        {
            e.InputParameters["table"] = CiTable;
            e.InputParameters["parameters"] = GetState();
        }

        protected void MyTableData_Updating(object sender, ObjectDataSourceMethodEventArgs e)
        {
            e.InputParameters["table"] = CiTable;
            e.InputParameters["parameters"] = GetState();
        }

        protected void MyTableData_Deleting(object sender, ObjectDataSourceMethodEventArgs e)
        {
            e.InputParameters["table"] = CiTable;
            e.InputParameters["parameters"] = GetState();
        }

        protected void MyTableData_Inserted(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if (e.OutputParameters.Count > 0 && CiTable != null)
            {
                object rowKey = MyUtils.Coalesce(e.OutputParameters["RowKey"], "");
                string script = MyUtils.Coalesce(e.OutputParameters["Script"], "").ToString();

                if (!MyUtils.IsEmpty(rowKey))
                {
                    string[] keyNames = CiTable.RowKeyNames;
                    string[] keyValues = (rowKey as string).Trim().Split(',');

                    for (int i = 0; i < keyNames.Length && i < keyValues.Length; i++)
                    {
                        string keyName = keyNames[i];
                        object keyValue = keyValues[i];

                        if (MyUtils.IsEmpty(keyValue))
                        {
                            break;
                        }

                        this[keyName] = keyValue;

                        for (CiTable ciTable = CiTable; ciTable != null; ciTable = ciTable.CiParentTable)
                        {
                            script += string.Format("SetField(\'{0}\', \'{1}\', \'{2}\'); ",
                              ciTable.TableName,
                              keyName,
                              keyValue);
                        }
                    }

                    if (IsCardView)
                    {
                        dxCard.JSProperties["cpInsertedRowIndex"] = dxCard.FindVisibleIndexByKeyValue(rowKey);
                    }
                    else if (IsGridView)
                    {
                        dxGrid.JSProperties["cpInsertedRowIndex"] = dxGrid.FindVisibleIndexByKeyValue(rowKey);
                    }
                }

                if (!MyUtils.IsEmpty(script))
                {
                    if (IsCardView)
                    {
                        dxCard.JSProperties["cpScript"] = script;
                    }
                    else if (IsGridView)
                    {
                        dxGrid.JSProperties["cpScript"] = script;
                    }
                }
            }
        }

        protected void MyTableData_Updated(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if (e.OutputParameters.Count > 0 && CiTable != null)
            {
                string script = MyUtils.Coalesce(e.OutputParameters["Script"], "").ToString();
                if (!MyUtils.IsEmpty(script))
                {
                    if (IsCardView)
                    {
                        dxCard.JSProperties["cpScript"] = script;
                    }
                    else if (IsGridView)
                    {
                        dxGrid.JSProperties["cpScript"] = script;
                    }
                }
            }
        }

        protected void MyTableData_Deleted(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if (e.OutputParameters.Count > 0 && CiTable != null)
            {
                string script = MyUtils.Coalesce(e.OutputParameters["Script"], "").ToString();
                if (!MyUtils.IsEmpty(script))
                {
                    if (IsCardView)
                    {
                        dxCard.JSProperties["cpScript"] = script;
                    }
                    else if (IsGridView)
                    {
                        dxGrid.JSProperties["cpScript"] = script;
                    }
                }
            }
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
            if (rowIndex < 0)
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
                    for (int i = 0; i < (ciField.Searchable ? 2 : 1); i++)
                    {
                        string fieldName = (i == 1)
                            ? ciField.SearchableFieldName
                            : ciField.FieldName;

                        object fieldValue = this[fieldName, rowIndex];

                        if (!dcColumns.Contains(fieldName))
                        {
                            dcColumns.Add(fieldName, ciField.GetNativeType(fieldValue));
                        }

                        dr[fieldName] = ciField.GetNativeValue(fieldValue);
                    }
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
                string tableName = CiTable.TableName;
                string tableCaption = MyWebUtils.Eval<string>(CiTable.TableCaption, GetState());

                TabPage tabPage = pgCardTabs.TabPages.Add(tableName);
                tabPage.Name = string.Format("pgCardTab_{0}", tableName);
                tabPage.Text = tableCaption;
                tabPage.ToolTip = tableCaption;
                tabPage.Controls.Add(this);
            }
        }

        public virtual void BuildGridColumn(Control container)
        {
            ASPxPageControl pgGridTabs = container as ASPxPageControl;
            if (pgGridTabs != null && CiTable != null)
            {
                string tableCaption = MyWebUtils.Eval<string>(CiTable.TableCaption, GetState());
                TabPage tabPage = pgGridTabs.TabPages.Add(CiTable.TableName);

                tabPage.Text = tableCaption;
                tabPage.ToolTip = tableCaption;
                tabPage.Controls.Add(this);
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

                dxTable.Styles.TitlePanel.BackColor = isSearchMode ? Color.SpringGreen
                    : (MyWebUtils.Application == "CPanel")
                    ? Color.DeepSkyBlue
                    : Color.Transparent;
                dxTable.SettingsText.Title = titlePrefix + MyWebUtils.Eval<string>(CiTable.TableCaption, GetState());

                dxTable.CardLayoutProperties.ColCount = totalColumns;

                // Set JSProperties
                dxTable.JSProperties["cpTableName"] = CiTable.TableName;

                // Build field columns
                CiField[] ciFields = isSearchMode ? CiTable.CiSearchableFields : CiTable.CiBrowsableFields;
                foreach (CiField ciField in ciFields)
                {
                    BuildCardColumn(dxTable, ciField, isSearchMode);
                }

                if (!isSearchMode && CiTable.LayoutXml != null)
                {
                    if (IsEditing)
                    {
                        dxTable.Templates.EditForm = CreateLayout(true);
                    }
                    else
                    {
                        dxTable.Templates.Card = CreateLayout(false);
                    }
                }
            }
        }

        private void BuildGridView(ASPxGridView dxTable)
        {
            if (CiTable != null)
            {
                dxTable.Styles.TitlePanel.BackColor = (MyWebUtils.Application == "CPanel")
                    ? Color.DeepSkyBlue
                    : Color.Transparent;

                dxTable.SettingsText.Title = MyWebUtils.Eval<string>(CiTable.TableCaption, GetState());
                dxTable.SettingsDetail.ShowDetailRow = (CiTable.Get<CiTable>().Length > 0);
                dxTable.SettingsEditing.Mode = GridViewEditingMode.Inline;
                dxTable.SettingsEditing.NewItemRowPosition = CiTable.InsertRowAtBottom
                    ? GridViewNewItemRowPosition.Bottom
                    : GridViewNewItemRowPosition.Top;

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

        private void BuildCardColumn(ASPxCardView dxTable, CiField ciField, bool isSearchMode)
        {
            if (ciField != null)
            {
                string textFieldName = ciField.TextFieldName;
                string fieldName = ciField.FieldName;

                if (isSearchMode)
                {
                    fieldName = ciField.SearchableFieldName;
                }
                else if (!MyUtils.IsEmpty(textFieldName))
                {
                    fieldName = textFieldName;
                }

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

                if (dxColumn == null && ciField.Browsable)
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
                ArrayList macroNames = GetToolbarMacros();

                // Setup standard toolbar buttons
                foreach (CardViewToolbarItem button in toolbar.Items)
                {
                    string buttonName = button.Name;
                    button.Visible = macroNames.IndexOf(buttonName) >= 0;

                    if (buttonName == "More")
                    {
                        BuildMoreMenu(button);
                    }
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
							button.Text = ciMacro.Caption;
							button.Command = CardViewToolbarCommand.Custom;
                            button.Image.IconID = ciMacro.IconID;
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
                ArrayList macroNames = GetToolbarMacros();

                // Setup standard toolbar buttons
                foreach (GridViewToolbarItem button in toolbar.Items)
                {
                    string buttonName = button.Name;
                    button.Visible = macroNames.IndexOf(buttonName) >= 0;

                    if (buttonName == "More")
                    {
                        BuildMoreMenu(button);
                    }
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
							button.Text = ciMacro.Caption;
							button.Command = GridViewToolbarCommand.Custom;
                            button.Image.IconID = ciMacro.IconID;
							toolbar.Items.Add(button);
						}
					}
				}
			}
        }

        private void BuildMoreMenu(DevExpress.Web.MenuItem dxMoreMenu)
        {
            if (dxMoreMenu != null)
            {
                DevExpress.Web.MenuItem dxMenuItem = dxMoreMenu.Items.FindByName("Designer");
                if (dxMenuItem != null)
                {
                    dxMenuItem.NavigateUrl = "Default.aspx?Application=CPanel&PluginSrc=Designer.pux" +
                        string.Format("&AppName={0}&TableName={1}",
                            MyWebUtils.Application,
                            CiTable.TableName);
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
                    ciMacro.Run(drParams);
                    DataTable dt = ciMacro.ResultTable;
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
                if (rowIndex < 0)
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
			if (rowIndex < 0)
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

        private ArrayList GetToolbarMacros()
        {
            ArrayList macros = new ArrayList();
            macros.Add("More");
            macros.Add("Divider");

            if (CiTable != null)
            {
                CiField[] ciSearchableFields = CiTable.CiSearchableFields;
                bool isSearchable = (ciSearchableFields != null) && (ciSearchableFields.Length > 0);
                bool isEditing = IsEditing;
                bool isBrowsing = IsBrowsing && !isEditing;
                bool isTopTable = (UiParentTable == null);
                bool isParentInserting = (UiParentTable != null) && UiParentTable.IsInserting;
                DataRow drParams = GetState();

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
							if (IsCardView || IsGridView && ciMacro.Toolbar)
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

        private ArrayList GetMenuMacros()
        {
            ArrayList macros = new ArrayList();

            if (CiTable != null)
            {
                DataRow drParams = GetState();

                foreach (CiMacro ciMacro in CiTable.CiMacros)
                {
                    if (IsGridView && ciMacro != null && !ciMacro.Toolbar && ciMacro.IsVisible(drParams))
                    {
                        macros.Add(ciMacro.MacroName);
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

                bool isParentOrChildEditing = IsParentEditing || IsChildEditing;

                CiMacro insertMacro = CiTable.InsertMacro;
                if (insertMacro == null || !insertMacro.IsVisible(drParams) || isParentOrChildEditing)
                {
                    disabledMacros += LIST_SEPARATOR + "New";
                }

                CiMacro updateMacro = CiTable.UpdateMacro;
                if (updateMacro == null || !updateMacro.IsVisible(drParams) || isParentOrChildEditing || !RecordsExist)
                {
                    disabledMacros += LIST_SEPARATOR + "Edit";
                }

                CiMacro deleteMacro = CiTable.DeleteMacro;
                if (deleteMacro == null || !deleteMacro.IsVisible(drParams) || isParentOrChildEditing || !RecordsExist)
                {
                    disabledMacros += LIST_SEPARATOR + "Delete";
                }

                if (IsBrowsing)
                {
                    foreach (CiMacro ciMacro in CiTable.CiMacros)
                    {
                        if (ciMacro != null)
                        {
                            string macroName = ciMacro.MacroName;

                            if (!ciMacro.IsVisible(drParams)|| isParentOrChildEditing || (IsCardView && !RecordsExist))
                            {
                                disabledMacros += LIST_SEPARATOR + macroName;
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
                string doubleClickMacroName = CiTable.DoubleClickMacroName;

                foreach (string macroName in GetMenuMacros())
                {
                    CiMacro ciMacro = CiTable.GetMacro(macroName);

                    DevExpress.Web.MenuItem menuItem = dxPopupMenu.Items.Add(ciMacro.Caption, macroName);

                    if (doubleClickMacroName == macroName)
                    {
                        menuItem.Checked = true;
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
                    ciMacro.Run(GetState());
                    script = ciMacro.ResultScript;
                }
            }

            return script;
        }

		private void SetupFieldDisplay(ASPxGridBase dxTable, string fieldName, ASPxGridColumnDisplayTextEventArgs e)
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

                //if (dxTable != null && e.VisibleIndex == GetFocusedIndex())
                //{
                //    string script = "";

                //    if (dxTable.JSProperties.ContainsKey("cpScript"))
                //    {
                //        script = MyUtils.Coalesce(dxTable.JSProperties["cpScript"], "").ToString();
                //    }

                //    string fieldValue = HttpUtility.JavaScriptStringEncode(MyUtils.Coalesce(e.Value, "").ToString());

                //    script += string.Format("InitField(\'{0}\', \'{1}\', \'{2}\'); ",
                //      CiTable.TableName,
                //      fieldName,
                //      fieldValue);

                //    dxTable.JSProperties["cpScript"] = script;
                //}
            }
        }

        private void StoreQueryStringCommand()
        {
            string command = MyWebUtils.QueryStringCommand;

            if (CiTable != null)
            {
                string tableName = CiTable.TableName;
                string childTableName = (CiTable.CiTables.Length > 0) ? CiTable.CiTables[0].TableName : "";

                if (MyUtils.IsEmpty(command))
                {
                    command = UiApplication.Me.GetCommandFired(tableName);
                }

                switch (command)
                {
                    case "New":
                        if (CiTable.InsertMacro != null)
                        {
                            UiApplication.Me.SetCommandFired(tableName, command);
                        }
                        else if (childTableName.Length > 0)
                        {
                            UiApplication.Me.SetCommandFired(tableName, "");
                            UiApplication.Me.SetCommandFired(childTableName, "New");
                        }
                        break;

                    case "Edit":
                        if (CiTable.UpdateMacro != null)
                        {
                            UiApplication.Me.SetCommandFired(tableName, command);
                        }
                        else if (childTableName.Length > 0)
                        {
                            UiApplication.Me.SetCommandFired(tableName, "");
                            UiApplication.Me.SetCommandFired(childTableName, "Edit");
                        }
                        break;

                    case "Search":
                        if (CiTable.CiSearchableFields.Length > 0)
                        {
                            UiApplication.Me.SetCommandFired(tableName, command);
                        }
                        break;

                    default:
                        if (!MyUtils.IsEmpty(command))
                        {
                            UiApplication.Me.SetCommandFired(tableName, command);
                        }
                        break;
                }
            }

            MyWebUtils.QueryStringCommand = null;
        }

        private void RunQueryStringCommand()
        {
            string command = "";

            if (CiTable != null)
            {
                command = UiApplication.Me.GetCommandFired(CiTable.TableName);

                switch (command)
                {
                    case "New":
                        if (CiTable.InsertMacro != null)
                        {
                            if (IsCardView)
                            {
                                dxCard.AddNewCard();
                            }
                            else if (IsGridView)
                            {
                                dxGrid.AddNewRow();
                            }
                        }
                        break;

                    case "Edit":
                        if (CiTable.UpdateMacro != null)
                        {
                            if (IsCardView)
                            {
                                dxCard.StartEdit(0);
                            }
                            else if (IsGridView)
                            {
                                dxGrid.StartEdit(0);
                            }
                        }
                        break;
                }
            }

            dxGrid.JSProperties["cpCheckFocusedRow"] = MyUtils.IsEmpty(command);
        }

        private void DoValidation()
        {
            if (CiTable != null)
            {
                // Check mandatory fields
                foreach (CiField ciField in CiTable.CiFields)
                {
                    string fieldName = ciField.FieldName;
                    if (ciField.Mandatory && MyUtils.IsEmpty(this[fieldName]))
                    {
                        ErrorFieldName = fieldName;
                        ErrorMessage = "Please fill in the missing information";
                        break;
                    }
                }

                // Check validate SQL
                if (MyUtils.IsEmpty(ErrorFieldName))
                {
                    ErrorMessage = CiTable.RunValidateSQL(GetState());
                }
            }
        }

        private bool ValidationOK(string command)
        {
            if (CiTable != null)
            {
                string script = "";

                if (!MyUtils.IsEmpty(ErrorMessage))
                {
                    UiApplication.Me.SetCommandFired(CiTable.TableName, command);

                    if (IsCardView)
                    {
                        BuildCardView(dxCard, false);
                        BuildCardToolbar();
                    }
                    else if (IsGridView)
                    {
                        BuildGridView(dxGrid);
                        BuildGridToolbar();
                    }

                    if (!MyUtils.IsEmpty(ErrorFieldName))
                    {
                        script += string.Format("SetEditorValue(\'{0}\', \'{1}\', \'{2}\'); ",
                          CiTable.TableName,
                          ErrorFieldName,
                          "");

                        script += string.Format("SetEditorFocus(\'{0}\', \'{1}\'); ",
                            CiTable.TableName,
                            ErrorFieldName);
                    }

                    script += string.Format("alert('{0}');", ErrorMessage);

                    if (IsCardView)
                    {
                        dxCard.JSProperties["cpIsInvalid"] = true;
                        dxCard.JSProperties["cpScript"] = script;
                    }
                    else if (IsGridView)
                    {
                        dxGrid.JSProperties["cpIsInvalid"] = true;
                        dxGrid.JSProperties["cpScript"] = script;
                    }

                    ErrorFieldName = null;

                    return false;
                }
            }

            return true;
        }

        private string GetInitFieldScript()
        {
            string script = "";

            if (CiTable != null)
            {
                string tableName = CiTable.TableName;
                foreach (CiField ciField in CiTable.CiFields)
                {
                    if (!ciField.IsVisible)
                    {
                        string fieldName = ciField.FieldName;
                        script += string.Format("InitField(\'{0}\', \'{1}\', \'{2}\'); ",
                            tableName,
                            fieldName,
                            this[fieldName]);
                    }
                }
            }

            return script;
        }
    }

    public class XiTable : XiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        public XiTable()
        {
            PluginType = typeof(CiTable);
        }

        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        private Hashtable mPropertySQL = new Hashtable();

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return MyWebUtils.GetBySQL("select * from CiTable where AppID = @AppID and TableID = @TableID",
                drPluginKey,
                true);
        }

        protected override List<XElement> GetPluginDefinitions(List<XElement> xElements)
        {
            if (xElements != null)
            {
                return xElements.FindAll(el => el.Name == "CiTable");
            }

            return null;
        }

        protected override void DeletePluginDefinitions(DataRow drPluginKey)
        {
            MyWebUtils.GetBySQL("exec spSQL_del 'CiTable', null, @AppID, @TableID", drPluginKey, true);
        }

        protected override void WriteToDB(DataRow drPluginDefinition)
        {
            string sqlParameterList = GetSqlParameterList(drPluginDefinition);

            MyWebUtils.GetBySQL("exec spTable_updLong " + sqlParameterList,
                drPluginDefinition,
                true);

            DataRow drSQL = MyUtils.CloneDataRow(drPluginDefinition);
            DataColumnCollection dcSQL = drSQL.Table.Columns;
            MyWebUtils.AddColumnIfRequired(dcSQL, "EntityType");
            MyWebUtils.AddColumnIfRequired(dcSQL, "SQLType");
            MyWebUtils.AddColumnIfRequired(dcSQL, "EntityID");
            MyWebUtils.AddColumnIfRequired(dcSQL, "SQL");
            drSQL["EntityType"] = "CiTable";
            drSQL["EntityID"] = drSQL["TableID"];

            foreach (string propertyName in mPropertySQL.Keys)
            {
                drSQL["SQLType"] = propertyName + "SQL";
                drSQL["SQL"] = mPropertySQL[propertyName];
                MyWebUtils.GetBySQL("exec spSQL_ins @EntityType, @SQLType, @AppID, @TableID, @EntityID, @SQL", drSQL);
            }
        }

        protected override string GetXPropertyName(string dPropertyName)
        {
            string xPropertyName = dPropertyName;

            switch (dPropertyName)
            {
                case "Src":
                    xPropertyName = "@Src";
                    break;

                case "Caption":
                    xPropertyName = "TableCaption";
                    break;
            }

            return xPropertyName;
        }

        protected override string GetDPropertyName(string xPropertyName)
        {
            string dPropertyName = xPropertyName;

            switch (xPropertyName)
            {
                case "DefaultValue":
                    dPropertyName = "Value";
                    break;

                case "TableCaption":
                    dPropertyName = "Caption";
                    break;
            }

            return dPropertyName;
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            DataTable dtFields = MyWebUtils.GetBySQL("select 1 from CiField where AppID = @AppID and TableID = @TableID",
                drPluginDefinition);

            if (dtFields.Rows.Count > 0)
            {
                xPluginDefinition.Add(new XElement("RowKey", ""));
            }

            DataTable dtTableSQL = MyWebUtils.GetBySQL("?exec spSQL_sel 'CiTable', null, @AppID, @TableID, @TableID", drPluginDefinition, true);
            if (dtTableSQL != null)
            {
                foreach (DataRow drFieldSQL in dtTableSQL.Rows)
                {
                    string sqlType = MyUtils.Coalesce(drFieldSQL["SQLType"], "").ToString();
                    if (sqlType.EndsWith("SQL"))
                    {
                        XElement xTableSQL = new XElement(sqlType.Substring(0, sqlType.Length - 3));
                        xTableSQL.Add(new XAttribute("lang", "sql"));
                        xTableSQL.Value = drFieldSQL["SQL"].ToString();
                        xPluginDefinition.Add(xTableSQL);
                    }
                }
            }
        }

        protected override void UploadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            DataColumnCollection dcPluginColumns = drPluginDefinition.Table.Columns;

            if (drPluginDefinition != null && xPluginDefinition != null)
            {
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

        protected override List<XiPlugin> GetXiChildPlugins()
        {
            return new List<XiPlugin>() { new XiField(), new XiChildTable(), new XiMacro(), new XiButtonField() };
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private string GetSqlParameterList(DataRow dr)
        {
            string sqlParameterList = "";

            foreach (DataColumn dcTableProperty in dr.Table.Columns)
            {
                sqlParameterList += ((MyUtils.IsEmpty(sqlParameterList))
                  ? " @" : " ,@") + dcTableProperty.ColumnName;
            }

            return sqlParameterList;
        }
    }

    public class XiChildTable: XiTable
    {
        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return MyWebUtils.GetBySQL("select * from CiTable where AppID = @AppID and TableID in " +
                "(select NestedTableID from CiField where AppID = @AppID and TableID = @TableID)",
                drPluginKey,
                true);
        }

        protected override void DeletePluginDefinitions(DataRow drPluginKey)
        {
            // Do nothing
        }

        protected override void WriteToDB(DataRow drPluginDefinition)
        {
            if (drPluginDefinition != null)
            {
                DataTable dt = drPluginDefinition.Table;
                if (dt != null)
                {
                    DataColumnCollection dc = dt.Columns;
                    MyWebUtils.AddColumnIfRequired(dc, "FieldID");
                    MyWebUtils.AddColumnIfRequired(dc, "FieldName");
                    MyWebUtils.AddColumnIfRequired(dc, "Type");
                    MyWebUtils.AddColumnIfRequired(dc, "Width");
                    MyWebUtils.AddColumnIfRequired(dc, "InRowKey");

                    drPluginDefinition["FieldName"] = drPluginDefinition["TableName"];
                    drPluginDefinition["Type"] = "Table";

                    string insertColumnNames = "@AppID, @TableID, @FieldName, @Caption, @Type, @Width, @InRowKey";
                    string insertSQL = string.Format("?exec spField_ins {0}", insertColumnNames);

                    drPluginDefinition["FieldID"] = MyWebUtils.EvalSQL(insertSQL, drPluginDefinition, true);

                    DataTable dtField = MyWebUtils.GetBySQL("?exec spField_sel @AppID, @TableID, @FieldID", drPluginDefinition);
                    if (MyWebUtils.GetNumberOfRows(dtField) > 0 && dtField.Columns.Contains("NestedTableID"))
                    {
                        drPluginDefinition["TableID"] = dtField.Rows[0]["NestedTableID"];
                    }

                    DataTable dtNestedTable = MyWebUtils.GetBySQL("select * from CiTable where AppID = @AppID and TableID = @TableID", drPluginDefinition);
                    if (MyWebUtils.GetNumberOfRows(dtNestedTable) > 0 && dtNestedTable.Columns.Contains("Src"))
                    {
                        DataRow drNestedTable = dtNestedTable.Rows[0];
                        drNestedTable["Src"] = drPluginDefinition["Src"];
                        drNestedTable["Caption"] = "";
                        drNestedTable["PageSize"] = 100;
                        base.WriteToDB(drNestedTable);
                    }
                }
            }
        }
    }
}