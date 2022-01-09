using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using DevExpress.Web.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;
using DevExpress.XtraPrinting;
using LumenWorks.Framework.IO.Csv;

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
						mSelectMacro.Caption = "Browse";
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
                        mUpdateMacro.MacroName = "Edit";
						mUpdateMacro.Caption = "Edit";
                        mUpdateMacro.IconID = "edit_edit_16x16office2013";
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
                        mInsertMacro.MacroName = "New";
						mInsertMacro.Caption = "New";
                        mInsertMacro.IconID = "actions_additem_16x16office2013";
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
						mDeleteMacro.Caption = "Delete";
                        mDeleteMacro.IconID = "edit_delete_16x16office2013";
                        mDeleteMacro.Confirm = true;
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
        public CiMacro SearchMacro
        {
            get
            {
                if (mSelectMacro != null)
                {
                    ArrayList sqlList = new ArrayList();
                    foreach (string actionSQL in mSelectMacro.ActionSQL)
                    {
                        string sql = actionSQL;
                        foreach (CiField ciField in CiSearchableFields)
                        {
                            string fieldName = ciField.FieldName;
                            sql = sql.Replace("@" + fieldName, "@" + ciField.SearchableID);
                        }

                        sqlList.Add(sql);
                    }

                    CiMacro searchMacro = new CiMacro();
                    searchMacro.Caption = "Search";
                    searchMacro.VisibleSQL = mSelectMacro.VisibleSQL;
                    searchMacro.ValidateSQL = mSelectMacro.ValidateSQL;
                    searchMacro.ActionSQL = sqlList.ToArray(typeof(string)) as string[];

                    return searchMacro;
                }


                return null;
            }
        }

        [XmlIgnore]
        public CiMacro QuitMacro
        {
            get
            {
                CiMacro quitMacro = new CiMacro();
                quitMacro.Caption = "Quit";

                return quitMacro;
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
                return Get<CiMacro>().Where(c => (IsListableMacro(c))).ToArray();
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

        public override CiPlugin GetContainerPlugin()
        {
            return this;
        }

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

        public virtual CiField[] GetBrowsableFields(DataRow dr)
        {
            CiField[] ciFields = CiFields;
            CiField[] sortedFields = new CiField[ciFields.Length];

            if (DefaultView == "Card")
            {
                // Invisible fields go at the end of the list
                int totalVisibleFields = ciFields.Length;
                foreach (CiField ciField in ciFields)
                {
                    if (!ciField.IsVisible(dr))
                    {
                        sortedFields[--totalVisibleFields] = ciField;
                    }
                }

                int totalColumns = ColCount;
                int rowNumber = 0;
                int columnNumber = 0;

                foreach (CiField ciField in ciFields)
                {
                    if (ciField.IsVisible(dr))
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

        public virtual CiMacro[] GetMenuMacros()
        {
            if (CiMacros != null)
            {
                return CiMacros.Where(c => !c.Toolbar).ToArray();
            }

            return new CiMacro[] { };
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

        public virtual string GetTableCaption(DataRow dr)
        {
            return MyWebUtils.Eval<string>(TableCaption, dr);
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private bool IsListableMacro(object objMacro)
        {
            CiMacro ciMacro = objMacro as CiMacro;

            if (ciMacro != null)
            {
                return ciMacro.IsListable;
            }

            return false;
        }
    }

    public partial class UiTable : UiPlugin, IUiColumn
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (Override)
        // --------------------------------------------------------------------------------------------------

        public override bool IsInserting
        {
            get
            {
                string command = UiApplication.Me.GetCommandFired(CiTable.TableName);

                bool isStartEditing = (command == "New") ||
                    dxCard.IsNewCardEditing ||
                    dxGrid.IsNewRowEditing;

                bool isFinishEditing = (command == "UpdateNew") || (command == "Cancel");

                return isStartEditing && !isFinishEditing;
            }
        }

        public override bool IsInsertSaving
        {
            get
            {
                string command = UiApplication.Me.GetCommandFired(CiTable.TableName);

                bool isStartEditing = true;

                bool isFinishEditing = (command == "UpdateNew");

                return isStartEditing && isFinishEditing;
            }
        }

        public override bool IsEditing
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

        public override bool IsEditSaving
        {
            get
            {
                string command = UiApplication.Me.GetCommandFired(CiTable.TableName);

                bool isStartEditing = (command == "Edit") || (command == "New") ||
                    dxCard.IsEditing || dxCard.IsNewCardEditing ||
                    dxGrid.IsEditing || dxGrid.IsNewRowEditing;

                bool isFinishEditing = (command == "Update");

                return isStartEditing && isFinishEditing;
            }
        }

        public override bool IsSearchSaving
        {
            get
            {
                string command = UiApplication.Me.GetCommandFired(CiTable.TableName);
                string view = MyUtils.Coalesce(GetClientValue("_View"), "").ToString();

                return (view == "Search") && command == "Update";
            }

        }

        // --------------------------------------------------------------------------------------------------
        // Properties (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual bool RecordsExist
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

        public virtual bool IsNavigating
        {
            get
            {
                string command = MyUtils.Coalesce(MyWebUtils.QueryStringCommand,
                    UiApplication.Me.GetCommandFired(CiTable.TableName));

                return (MyUtils.IsEmpty(command) || !IsEditing);
            }
        }

        public virtual bool IsParentEditing
        {
            get
            {
                return (UiParentTable != null && UiParentTable.IsEditing);
            }
        }

        public virtual bool IsChildEditing
        {
            get
            {
                if (CiTable != null)
                {
                    foreach (CiTable ciSubTable in CiTable.CiTables)
                    {
                        string command = UiApplication.Me.GetCommandFired(ciSubTable.TableName);

                        bool isStartEditing = (command == "Edit") || (command == "New");

                        bool isFinishEditing = (command == "UpdateNew") || (command == "Update") || (command == "Cancel");

                        if (isStartEditing && !isFinishEditing)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }


        public virtual bool IsBrowsing
        {
            get
            {
                bool isSearching = (CiTable != null) && CiTable.IsSearching;

                return !isSearching;
            }
        }

        public virtual bool IsNonEditable
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

        public virtual bool IsCardView
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

        public virtual bool IsGridView
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

        public virtual CiTable CiTable
        {
            get { return CiPlugin as CiTable; }
            set { CiPlugin = value; }
        }

        public virtual UiTable UiParentTable
        {
            get { return UiParentPlugin as UiTable; }
        }

        public virtual string ErrorFieldName { get; set; } = null;

        public virtual string ErrorMessage { get; set; } = null;

        public virtual string ClientScript
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
                Page.Title = CiTable.GetTableCaption(GetState());
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers (Search)
        // --------------------------------------------------------------------------------------------------

        protected void dxSearch_Init(object sender, EventArgs e)
        {
            DataRow drParams = GetState();
            bool isVisible = CiTable != null && CiTable.IsSearching && !CiTable.IsHidden(drParams);

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
                        searchableColumnName = ciField.SearchableID;
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
            DataRow drParams = GetState();
            bool isVisible = IsCardView && (CiTable != null) && !CiTable.IsSearching && !CiTable.IsHidden(drParams);

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
            e.Properties["cpConfirmableMacros"] = GetConfirmableMacros();
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
            RunDefaultMacro(dxCard, e);
            dxCard.SettingsPager.SettingsTableLayout.RowsPerPage = 0;
        }

        protected void dxCard_CardValidating(object sender, ASPxCardViewDataValidationEventArgs e)
        {
            DoValidation();
        }

        protected void dxCard_CardInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e)
        {
            this.dxCard.SettingsPager.SettingsTableLayout.RowsPerPage = 1;
            e.Cancel = !ValidationOK("New");
            UiApplication.Me.SetCommandFired(CiTable.TableName, "UpdateNew");

            ASPxCardView dxCard = sender as ASPxCardView;
            if (dxCard != null)
            {
                dxCard.JSProperties["cpCommand"] = "UpdateNew";
            }
        }

        protected void dxCard_CardUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            e.Cancel = !ValidationOK("Edit");
            UiApplication.Me.SetCommandFired(CiTable.TableName, "Update");

            ASPxCardView dxCard = sender as ASPxCardView;
            if (dxCard != null)
            {
                dxCard.JSProperties["cpCommand"] = "Update";
            }
        }

        protected void dxCard_CancelCardEditing(object sender, ASPxStartCardEditingEventArgs e)
        {
            ASPxCardView dxCard = sender as ASPxCardView;
            if (dxCard != null)
            { 
                dxCard.SettingsPager.SettingsTableLayout.RowsPerPage = 1;
                dxCard.DataBind(); // Allows label text to be displayed

                if (dxCard.IsNewCardEditing && CiTable != null)
                {
                    foreach (string key in CiTable.RowKeyNames)
                    {
                        SetClientValue(key, this[key]);
                    }
                }

                dxCard.JSProperties["cpCommand"] = "Cancel";
            }
        }

        protected void dxCard_ToolbarItemClick(object source, ASPxCardViewToolbarItemClickEventArgs e)
        {
            ProcessCardCommand();
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
			SetupFieldDisplay(e.Column.FieldName, e);
		}

        protected void pgCardTabs_Init(object sender, EventArgs e)
        {
            if (CiTable != null && MyUtils.IsEmpty(CiTable.LayoutUrl))
            {
                ASPxPageControl pgCardTabs = sender as ASPxPageControl;

                if (pgCardTabs != null && pgCardTabs.TabPages.Count == 0 && CiTable != null)
                {
                    pgCardTabs.ID = string.Format("pgCardTabs_{0}", CiTable.TableName);
                    DataRow drParams = GetState();
                    foreach (CiTable ciSubTable in CiTable.Get<CiTable>())
                    {
                        IUiColumn uiColumn = this.CreateUiPlugin(ciSubTable) as IUiColumn;
                        if (uiColumn != null && !ciSubTable.IsHidden(drParams))
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
            DataRow drParams = GetState();
            bool isVisible = IsGridView && CiTable != null && !CiTable.IsSearching && !CiTable.IsHidden(drParams);

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
            e.Properties["cpConfirmableMacros"] = GetConfirmableMacros();
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
            UiApplication.Me.SetCommandFired(CiTable.TableName, "UpdateNew");

            ASPxGridView dxGrid = sender as ASPxGridView;
            if (dxGrid != null)
            {
                dxGrid.JSProperties["cpCommand"] = "UpdateNew";
            }
        }

        protected void dxGrid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            e.Cancel = !ValidationOK("Edit");
            UiApplication.Me.SetCommandFired(CiTable.TableName, "Update");

            ASPxGridView dxGrid = sender as ASPxGridView;
            if (dxGrid != null)
            {
                dxGrid.JSProperties["cpCommand"] = "Update";
            }
        }

        protected void dxGrid_CancelRowEditing(object sender, ASPxStartRowEditingEventArgs e)
        {
            ASPxGridView dxGrid = sender as ASPxGridView;
            if (dxGrid != null)
            {
                if (dxGrid.IsNewRowEditing && CiTable != null)
                {
                    foreach (string key in CiTable.RowKeyNames)
                    {
                        SetClientValue(key, this[key]);
                    }
                }

                dxGrid.JSProperties["cpCommand"] = "Cancel";
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

        protected void dxGrid_InitNewRow(object sender, ASPxDataInitNewRowEventArgs e)
        {
            RunDefaultMacro(dxGrid, e);
            ProcessGridCommand();
            AddAcceptCancelColumn();
        }

        protected void dxGrid_StartRowEditing(object sender, ASPxStartRowEditingEventArgs e)
        {
            ProcessGridCommand();
            AddAcceptCancelColumn();
        }

        protected void dxGrid_ToolbarItemClick(object source, DevExpress.Web.Data.ASPxGridViewToolbarItemClickEventArgs e)
        {
            ProcessGridCommand();
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
						if (ciField != null && ciField.GetType() == typeof(CiCurrencyField))
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
			SetupFieldDisplay(e.Column.FieldName, e);
		}

        protected void pgGridTabs_Init(object sender, EventArgs e)
        {
            ASPxPageControl pgGridTabs = sender as ASPxPageControl;

            if (pgGridTabs != null && pgGridTabs.TabPages.Count == 0 && CiTable != null)
            {
                DataRow drParams = GetState();

                foreach (CiTable ciSubTable in CiTable.Get<CiTable>())
                {
                    IUiColumn uiColumn = this.CreateUiPlugin(ciSubTable) as IUiColumn;
                    if (uiColumn != null && !ciSubTable.IsHidden(drParams))
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
        // Event Handlers (UploadControl)
        // --------------------------------------------------------------------------------------------------

        protected void dxImportCSV_FileUploadComplete(object sender, FileUploadCompleteEventArgs e)
        {
            UploadedFile uploadedFile = e.UploadedFile;
            if (uploadedFile != null)
            {
                DataRow drParams = GetState();
                DataTable dtCSV = new DataTable();
                using (CsvReader csvReader = new CsvReader(new StreamReader(uploadedFile.FileContent), true))
                {
                    dtCSV.Load(csvReader);
                }

                if (CiTable != null)
                {
                    CiMacro ciInsertMacro = CiTable.InsertMacro;
                    if (ciInsertMacro != null)
                    {
                        foreach (DataRow drCSV in dtCSV.Rows)
                        {
                            ciInsertMacro.Run(drCSV);
                            string errorText = ciInsertMacro.ErrorMessage;
                            if (!MyUtils.IsEmpty(errorText))
                            {
                                e.ErrorText = errorText;
                                break;
                            }
                        }

                        e.CallbackData = CiTable.TableName;
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers (ObjectDataSource)
        // --------------------------------------------------------------------------------------------------

        protected void MyTableData_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (!CiTable.IsSearching)
            {
                e.InputParameters["table"] = CiTable;
                e.InputParameters["view"] = GetClientValue("_View");
                e.InputParameters["parameters"] = GetState(IsNavigating || IsEditing ? -2 : -1);
            }
        }

        protected void MyTableData_Inserting(object sender, ObjectDataSourceMethodEventArgs e)
        {
            e.InputParameters["table"] = CiTable;
            e.InputParameters["view"] = GetClientValue("_View");
            e.InputParameters["parameters"] = GetState(-2);
        }

        protected void MyTableData_Updating(object sender, ObjectDataSourceMethodEventArgs e)
        {
            e.InputParameters["table"] = CiTable;
            e.InputParameters["view"] = GetClientValue("_View");
            e.InputParameters["parameters"] = GetState();
        }

        protected void MyTableData_Deleting(object sender, ObjectDataSourceMethodEventArgs e)
        {
            e.InputParameters["table"] = CiTable;
            e.InputParameters["view"] = GetClientValue("_View");
            e.InputParameters["parameters"] = GetState();
        }

        protected void MyTableData_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if (e.OutputParameters.Count > 0)
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

            DataTable dt = e.ReturnValue as DataTable;
            if (dt != null && IsGridView)
            {
                int rowCount = dt.Rows.Count;
                if (rowCount > 10)
                {
                    int scrollableHeight = (rowCount < 20) ? 250 : 700;
                    //dxGrid.Settings.VerticalScrollBarMode = ScrollBarMode.Visible;
                    //dxGrid.Settings.VerticalScrollableHeight = scrollableHeight;
                    SetClientValue("ScrollableHeight", scrollableHeight);
                }
            }
        }

        protected void MyTableData_Inserted(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if (e.OutputParameters.Count > 0 && CiTable != null)
            {
                object rowKey = MyUtils.Coalesce(e.OutputParameters["RowKey"], "");
                string script = MyUtils.Coalesce(e.OutputParameters["Script"], "").ToString();
                bool isInvalid = Convert.ToBoolean(MyUtils.Coalesce(e.OutputParameters["IsInvalid"], false));

                if (!MyUtils.IsEmpty(rowKey))
                {
                    string[] keyNames = CiTable.RowKeyNames;
                    string[] keyValues = (rowKey as string).Trim().Split(',');
                    NameValueCollection queryString = new NameValueCollection(MyWebUtils.QueryString);
                    queryString.Remove("Command");
                    string url = "Default.aspx";

                    for (int i = 0; i < queryString.AllKeys.Length; i++)
                    {
                        string qsKey = queryString.Keys[i];
                        string qsValue = queryString[qsKey];

                        if (i == 0)
                        {
                            url += "?";
                        }
                        else
                        {
                            url += "&";
                        }

                        url += string.Format("{0}={1}", qsKey, qsValue);
                    }

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

                        url += string.Format("&{0}={1}", keyName, keyValue);
                    }

                    if (IsCardView)
                    {
                        script += string.Format("window.open('{0}', '_self');", url);
                    }
                }

                if (!MyUtils.IsEmpty(script))
                {
                    if (IsCardView)
                    {
                        dxCard.JSProperties["cpScript"] = script;
                        dxCard.JSProperties["cpIsInvalid"] = isInvalid;
                    }
                    else if (IsGridView)
                    {
                        dxGrid.JSProperties["cpScript"] = script;
                        dxGrid.JSProperties["cpIsInvalid"] = isInvalid;
                    }
                }
            }
        }

        protected void MyTableData_Updated(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if (e.OutputParameters.Count > 0 && CiTable != null)
            {
                string script = MyUtils.Coalesce(e.OutputParameters["Script"], "").ToString();
                bool isInvalid = Convert.ToBoolean(MyUtils.Coalesce(e.OutputParameters["IsInvalid"], false));

                if (!MyUtils.IsEmpty(script))
                {
                    if (IsCardView)
                    {
                        dxCard.JSProperties["cpScript"] = script;
                        dxCard.JSProperties["cpIsInvalid"] = isInvalid;
                    }
                    else if (IsGridView)
                    {
                        dxGrid.JSProperties["cpScript"] = script;
                        dxGrid.JSProperties["cpIsInvalid"] = isInvalid;
                    }
                }
            }
        }

        protected void MyTableData_Deleted(object sender, ObjectDataSourceStatusEventArgs e)
        {
            if (e.OutputParameters.Count > 0 && CiTable != null)
            {
                string script = MyUtils.Coalesce(e.OutputParameters["Script"], "").ToString();
                bool isInvalid = Convert.ToBoolean(MyUtils.Coalesce(e.OutputParameters["IsInvalid"], false));

                if (!MyUtils.IsEmpty(script))
                {
                    if (IsCardView)
                    {
                        dxCard.JSProperties["cpScript"] = script;
                        dxCard.JSProperties["cpIsInvalid"] = isInvalid;
                    }
                    else if (IsGridView)
                    {
                        dxGrid.JSProperties["cpScript"] = script;
                        dxGrid.JSProperties["cpIsInvalid"] = isInvalid;
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

        protected override object GetServerValue(string key, int rowIndex = -1)
        {
            return IsCardView ? GetCardValue(key, rowIndex) : GetGridValue(key, rowIndex);
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
            if (pgCardTabs != null && CiTable != null && !CiTable.IsHidden(GetState()))
            {
                string tableName = CiTable.TableName;
                string tableCaption = CiTable.GetTableCaption(GetState());

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
            DataRow drParams = GetState();
            if (pgGridTabs != null && CiTable != null && !CiTable.IsHidden(drParams))
            {
                string tableCaption = CiTable.GetTableCaption(drParams);
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
            DataRow drParams = GetState();
            if (CiTable != null && !CiTable.IsHidden(drParams))
            {
                int totalColumns = isSearchMode ? 1 : CiTable.ColCount;
                string titlePrefix = isSearchMode ? "Search for: " : "";

                dxTable.Styles.TitlePanel.BackColor = isSearchMode ? Color.SpringGreen
                    : Color.Transparent;
                dxTable.SettingsText.Title = titlePrefix + CiTable.GetTableCaption(drParams);

                dxTable.CardLayoutProperties.ColCount = totalColumns;

                // Set JSProperties
                dxTable.JSProperties["cpTableName"] = CiTable.TableName;
                dxTable.JSProperties["cpQuickInsert"] = CiTable.QuickInsert;

                // Build field columns
                CiField[] ciFields = isSearchMode ? CiTable.CiSearchableFields : CiTable.GetBrowsableFields(GetState());
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
            DataRow drParams = GetState();
            if (CiTable != null && !CiTable.IsHidden(drParams))
            {
                dxTable.SettingsText.Title = CiTable.GetTableCaption(drParams);
                dxTable.SettingsDetail.ShowDetailRow = (CiTable.Get<CiTable>().Count() > 0);
                dxTable.SettingsEditing.Mode = GridViewEditingMode.Inline;
                dxTable.SettingsEditing.NewItemRowPosition = CiTable.InsertRowAtBottom
                    ? GridViewNewItemRowPosition.Bottom
                    : GridViewNewItemRowPosition.Top;

                // Set JSProperties
                dxTable.JSProperties["cpTableName"] = CiTable.TableName;
                dxTable.JSProperties["cpQuickInsert"] = CiTable.QuickInsert;

                // Build field columns
                CiField[] ciFields = CiTable.GetBrowsableFields(drParams);
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
                    fieldName = ciField.SearchableID;
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

				ciField.FormatCardColumn(dxColumn, GetState());
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

				ciField.FormatGridColumn(dxColumn, GetState());
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
            bool isUser = MyWebUtils.IsUserAuthorised("User");
            if (dxMoreMenu != null)
            {
                dxMoreMenu.Visible = isUser;

                bool isDeveloper = MyWebUtils.IsUserAuthorised("Developer");
                DevExpress.Web.MenuItem dxMenuItem = null;

                dxMenuItem = dxMoreMenu.Items.FindByName("Inspect");
                if (dxMenuItem != null)
                {
                    dxMenuItem.Visible = isDeveloper;
                }

                dxMenuItem = dxMoreMenu.Items.FindByName("Designer");
                if (dxMenuItem != null)
                {
                    if (isDeveloper)
                    {
                        dxMenuItem.Visible = true;

                        string tableName = CiTable.TableName;
                        DataRow dr = MyWebUtils.GetTableDetails(tableName);
                        bool? bound = MyWebUtils.GetField<bool>(dr, "Bound");
                        string designerPuxFile = bound.HasValue && bound.Value ? "CiTableDetails.pux" : "CiFormDetails.pux";

                        dxMenuItem.NavigateUrl = string.Format("Default.aspx?Application=CPanel&PluginSrc={0}&AppID={1}&TableID={2}",
                                designerPuxFile,
                                MyWebUtils.GetField<int>(dr, "AppID"),
                                MyWebUtils.GetField<int>(dr, "TableID"));
                    }
                    else
                    {
                        dxMenuItem.Visible = false;
                    }
                }

                dxMenuItem = dxMoreMenu.Items.FindByName("ShareURL");
                if (dxMenuItem != null)
                {
                    dxMenuItem.Visible = (CiTable.CiParentTable == null);
                    string shareURL = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                    shareURL += ResolveUrl("~/");
                    shareURL += string.Format("Default.aspx?{0}", MyWebUtils.QueryString);

                    dxMenuItem.NavigateUrl = string.Format("javascript:copyToClipboard('{0}')", shareURL);
                }
            }
        }

        private void RunDefaultMacro(ASPxGridBase dxTable, ASPxDataInitNewRowEventArgs e)
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

            dxTable.JSProperties["cpScript"] = GetInvisibleFieldScript();
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

                if (!isEditing && MyWebUtils.IsQuitable(Page) && isTopTable)
                {
                    macros.Add("Quit");
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

        private ArrayList GetDisabledMacroList()
        {
            ArrayList ciDisabledMacros = new ArrayList();

            if (CiTable != null)
            {
                DataRow drParams = GetState();

                bool isParentOrChildEditing = IsParentEditing || IsChildEditing;

                CiMacro insertMacro = CiTable.InsertMacro;
                if (insertMacro == null || isParentOrChildEditing || !insertMacro.IsVisible(drParams))
                {
                    ciDisabledMacros.Add(insertMacro);
                }

                CiMacro updateMacro = CiTable.UpdateMacro;
                if (updateMacro == null || isParentOrChildEditing || !RecordsExist || !updateMacro.IsVisible(drParams))
                {
                    ciDisabledMacros.Add(updateMacro);
                }

                CiMacro deleteMacro = CiTable.DeleteMacro;
                if (deleteMacro == null || isParentOrChildEditing || !RecordsExist || !deleteMacro.IsVisible(drParams))
                {
                    ciDisabledMacros.Add(deleteMacro);
                }

                if (IsBrowsing)
                {
                    foreach (CiMacro ciMacro in CiTable.CiMacros)
                    {
                        if (ciMacro != null)
                        {
                            string macroName = ciMacro.MacroName;

                            if (isParentOrChildEditing || (IsCardView && !RecordsExist) || !ciMacro.IsVisible(drParams))
                            {
                                ciDisabledMacros.Add(ciMacro);
                            }
                        }
                    }
                }
            }

            return ciDisabledMacros;
        }

        private string GetDisabledMacros()
        {
            string disabledMacros = "";

            ArrayList ciDisabledMacros = GetDisabledMacroList();
            foreach (CiMacro ciMacro in ciDisabledMacros)
            {
                if (ciMacro != null)
                {
                    disabledMacros += LIST_SEPARATOR + ciMacro.MacroName;
                }
            }

            return disabledMacros;
        }

        private string GetConfirmableMacros()
        {
            string confirmableMacros = "";

            ArrayList ciConfirmableMacros = new ArrayList();
            CiMacro[] ciCrudMacros = new CiMacro[] {
                CiTable.SelectMacro,
                CiTable.InsertMacro,
                CiTable.UpdateMacro,
                CiTable.DeleteMacro,
                CiTable.DefaultMacro};

            int i = 0;
            foreach (CiMacro ciMacro in CiTable.CiMacros.Union(ciCrudMacros).ToArray())
            {
                if (ciMacro != null && ciMacro.Confirm)
                {
                    if (i++ > 0)
                    {
                        confirmableMacros += LIST_SEPARATOR;
                    }

                    confirmableMacros += ciMacro.MacroName;
                }
            }

            return confirmableMacros;
        }

        private string OpenMenu(string xPos, string yPos)
        {
            string mouseCoordinates = xPos + LIST_SEPARATOR + yPos;
            dxPopupMenu.Items.Clear();

            if (IsBrowsing && CiTable != null && IsGridView)
            {
                string doubleClickMacroName = CiTable.DoubleClickMacroName;

                DataRow drParams = GetState();

                CiMacro[] ciMacros = { CiTable.UpdateMacro, CiTable.DeleteMacro };
                ciMacros = ciMacros.Concat(CiTable.GetMenuMacros()).ToArray();
                //CiMacro[] ciMacros = CiTable.GetMenuMacros();
                ArrayList ciDisabledMacros = GetDisabledMacroList();

                foreach (CiMacro ciMacro in ciMacros)
                {
                    if (ciMacro != null && !ciDisabledMacros.Contains(ciMacro))
                    {
                        string macroName = ciMacro.MacroName;
                        string macroCaption = ciMacro.Caption;

                        DevExpress.Web.MenuItem menuItem = dxPopupMenu.Items.Add(macroCaption, macroName);

                        menuItem.Image.IconID = ciMacro.IconID;

                        if (doubleClickMacroName == macroName)
                        {
                            menuItem.ItemStyle.ForeColor = Color.Red;
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
                    ciMacro.Run(GetState());
                    script = ciMacro.ResultScript;
                }
            }

            return script;
        }

		private void SetupFieldDisplay(string fieldName, ASPxGridColumnDisplayTextEventArgs e)
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

        private void StoreQueryStringCommand()
        {
            string command = MyWebUtils.QueryStringCommand;

            if (CiTable != null)
            {
                string tableName = CiTable.TableName;
                string subTableName = (CiTable.CiTables.Length > 0) ? CiTable.CiTables[0].TableName : "";

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
                        else if (subTableName.Length > 0)
                        {
                            UiApplication.Me.SetCommandFired(tableName, "");
                            UiApplication.Me.SetCommandFired(subTableName, "New");
                        }
                        break;

                    case "Edit":
                        if (CiTable.UpdateMacro != null)
                        {
                            UiApplication.Me.SetCommandFired(tableName, command);
                        }
                        else if (subTableName.Length > 0)
                        {
                            UiApplication.Me.SetCommandFired(tableName, "");
                            UiApplication.Me.SetCommandFired(subTableName, "Edit");
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
                    if (ciField.IsMandatory(GetState()) && MyUtils.IsEmpty(this[fieldName]))
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

        private string GetInvisibleFieldScript()
        {
            string script = "";

            if (CiTable != null)
            {
                string tableName = CiTable.TableName;
                DataRow drParams = GetState();
                foreach (CiField ciField in CiTable.CiFields)
                {
                    if (!ciField.IsVisible(drParams))
                    {
                        string fieldName = ciField.FieldName;
                        script += string.Format("InitField(\'{0}\', \'{1}\', \'{2}\'); ",
                            tableName,
                            fieldName,
                            this[fieldName]);
                    }
                }
            }

            if (MyUtils.IsEmpty(script))
            {
                script = null;
            }

            return script;
        }

        private string GetSearchFieldScript()
        {
            string script = "";

            if (CiTable != null)
            {
                string tableName = CiTable.TableName;
                foreach (CiField ciField in CiTable.CiSearchableFields)
                {
                    string searchKey = ciField.SearchableID;
                    script += string.Format("InitField(\'{0}\', \'{1}\', \'{2}\'); ",
                        tableName,
                        searchKey,
                        this[searchKey]);
                }
            }

            return script;
        }

        private void ProcessCardCommand()
        {
            if (CiTable != null && !MyWebUtils.IsTimeOutReached(Page))
            {
                string macroName = UiApplication.Me.GetCommandFired(CiTable.TableName);

                switch (macroName)
                {
                    case "New":
                        dxCard.SettingsPager.Visible = false;
                        break;

                    case "Edit":
                        dxCard.SettingsPager.Visible = false;
                        dxCard.JSProperties["cpScript"] = GetInvisibleFieldScript();
                        break;

                    case "Delete":
                        dxCard.SettingsPager.Visible = true;
                        break;

                    case "UpdateNew":
                    case "Update":
                    case "Cancel":
                        string script = "";

                        CiMacro ciMacro = IsInsertSaving ? CiTable.InsertMacro : CiTable.UpdateMacro;
                        string navigatePos = ciMacro.NavigatePos;
                        string navigateUrl = MyWebUtils.Eval<string>(ciMacro.NavigateUrl, GetState()) + "|" + navigatePos;

                        if (!MyUtils.IsEmpty(navigateUrl))
                        {
                            script = string.Format("GotoUrl(\'{0}\'); ", HttpUtility.JavaScriptStringEncode(navigateUrl));
                        }
                        else
                        {
                            script = GetSearchFieldScript();
                        }

                        script += GetInvisibleFieldScript();

                        dxCard.JSProperties["cpScript"] = script;
                        dxCard.SettingsPager.Visible = true;
                        break;

                    default:
                        dxCard.JSProperties["cpScript"] = RunMacro(macroName);
                        break;
                }
            }
        }

        private void ProcessGridCommand()
        {
            if (!MyWebUtils.IsTimeOutReached(Page) && CiTable != null)
            {
                BuildGridToolbar();

                string macroName = UiApplication.Me.GetCommandFired(CiTable.TableName);

                switch (macroName)
                {
                    case "New":
                        break;

                    case "Edit":
                        dxGrid.JSProperties["cpScript"] = GetInvisibleFieldScript();
                        break;

                    case "Delete":
                        // Do nothing
                        break;

                    case "Update":
                    case "UpdateNew":
                    case "Cancel":
                        dxGrid.JSProperties["cpScript"] = GetSearchFieldScript();
                        break;

                    default:
                        dxGrid.JSProperties["cpScript"] = RunMacro(macroName);
                        break;
                }
            }
        }

        private void AddAcceptCancelColumn()
        {
            GridViewCommandColumn dxColumn = new GridViewCommandColumn();
            dxColumn.Visible = true;
            dxColumn.Caption = "* Actions *";
            dxColumn.Width = Unit.Pixel(100);
            dxColumn.ShowUpdateButton = true;
            dxColumn.ShowCancelButton = true;
            dxGrid.SettingsCommandButton.UpdateButton.Text = "Accept";
            dxGrid.Columns.Add(dxColumn);
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

        public override void DownloadFile(DataRow drKey, string puxUrl)
        {
            base.DownloadFile(drKey, puxUrl);

            object oldTableName = MyWebUtils.GetField(drKey, "OldTableName");
            if (!MyUtils.IsEmpty(oldTableName))
            {
                string appName = MyWebUtils.GetApplicationName(drKey);
                string oldPuxUrl = MyWebUtils.MapPath(string.Format("Sites/{0}/{1}.pux", appName, oldTableName));

                if (File.Exists(oldPuxUrl) && oldPuxUrl != puxUrl)
                {
                    File.Delete(oldPuxUrl);
                }
            }
        }

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return MyWebUtils.GetBySQL("?exec spTable_sel @AppID, @TableID", drPluginKey, 0);
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
            MyWebUtils.GetBySQL("exec spField_del @AppID, @TableID", drPluginKey, 0);
            MyWebUtils.GetBySQL("exec spMacro_del @AppID, @TableID", drPluginKey, 0);
            MyWebUtils.GetBySQL("exec spSubTable_del @AppID, @TableID", drPluginKey, 0);
            MyWebUtils.GetBySQL("exec spTriggerField_del @AppID, @TableID", drPluginKey, 0);
        }

        protected override string GetXPropertyName(Type pluginType, string dPropertyName)
        {
            string xPropertyName = base.GetXPropertyName(pluginType, dPropertyName);

            switch (dPropertyName)
            {
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

        protected override void WriteToDB(DataRow drPluginDefinition)
        {
            MyWebUtils.GetBySQL("exec spTable_updLong " +
                "@AppID, @TableID, @TableName, @Src, @Caption, @DefaultView, " +
                "@LayoutUrl, @ColCount, @BubbleUpdate, @QuickInsert, @InsertRowAtBottom, " +
                "@DoubleClickMacroName",
                drPluginDefinition,
                0);

            DataColumnCollection dcPluginDefinition = drPluginDefinition.Table.Columns;

            if (!dcPluginDefinition.Contains("SQLType"))
            {
                dcPluginDefinition.Add("SQLType");
            }

            if (!dcPluginDefinition.Contains("SQL"))
            {
                dcPluginDefinition.Add("SQL");
            }

            foreach (string sqlType in mPropertySQL.Keys)
            {
                drPluginDefinition["SQLType"] = sqlType;
                drPluginDefinition["SQL"] = mPropertySQL[sqlType];
                MyWebUtils.GetBySQL("exec spAttributeSQL_upd @AppID, @TableID, @SQLType, @SQL", drPluginDefinition, 0);
            }
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            DataTable dtPrimaryKey = MyWebUtils.GetBySQL("select FieldName from dbo.fnGetFields(@AppID, @TableID, null, null) where InRowKey = 1", drPluginDefinition);

            string primaryKey = "";
            if (MyWebUtils.GetNumberOfRows(dtPrimaryKey) > 0)
            {
                int i = 0;
                foreach (DataRow drKey in dtPrimaryKey.Rows)
                {
                    if (i++ > 0)
                    {
                        primaryKey += ",";
                    }

                    primaryKey += MyWebUtils.GetStringField(drKey, "FieldName");
                }
            }

            xPluginDefinition.Add(new XElement("RowKey", primaryKey));

            XAttribute xSrcAttr = xPluginDefinition.Attribute("src");
            if (xSrcAttr != null)
            {
                xSrcAttr.Value += ".pux";
            }

            XElement xDummyField = new XElement("CiField");
            xDummyField.Add(new XElement("FieldName", "DummyForInsert"));
            xDummyField.Add(new XElement("Hidden", true));
            xPluginDefinition.Add(xDummyField);

            DataTable dtAttributeSQL = MyWebUtils.GetBySQL("?exec spAttributeSQL_sel @AppID, @TableID", drPluginDefinition, 0);
            foreach (DataRow drAttributeSQL in dtAttributeSQL.Rows)
            {
                string sqlType = MyWebUtils.GetStringField(drAttributeSQL, "SQLType");
                string SQL = MyWebUtils.GetStringField(drAttributeSQL, "SQL");
                string xPropertyName = GetXPropertyName(typeof(CiTable), sqlType);

                XElement xSQLType = xPluginDefinition.Element(xPropertyName);
                if (xSQLType == null)
                {
                    xSQLType = new XElement(xPropertyName);
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

        protected override void UploadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            DataColumnCollection dcPluginColumns = drPluginDefinition.Table.Columns;

            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                drPluginDefinition["TableName"] = Convert.DBNull;

                foreach (XAttribute xProperty in xPluginDefinition.Attributes())
                {
                    string xPropertyName = xProperty.Name.ToString();
                    switch (xPropertyName)
                    {
                        case "src":
                            string filename = xProperty.Value;
                            if (MyWebUtils.IsPuxFile(filename))
                            {
                                filename = MyWebUtils.GetPuxFileName(filename);
                                if (!MyUtils.IsEmpty(filename))
                                {
                                    filename = filename.Substring(0, filename.Length - 4);
                                }
                            }
                            drPluginDefinition["src"] = filename;
                            break;
                    }
                }

                foreach (XElement xProperty in xPluginDefinition.Elements())
                {
                    string xPropertyName = xProperty.Name.ToString();
                    switch (xPropertyName)
                    {
                        case "TableCaption":
                            drPluginDefinition["Caption"] = xProperty.Value;
                            break;
                    }
                }

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
            return new List<XiPlugin>() { new XiField(), new XiSubTable(), new XiMacro(), new XiButtonField() };
        }
    }

    public class XiSubTable: XiTable
    {
        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return MyWebUtils.GetBySQL("select * from CiTable where AppID = @AppID and ParentTableID = @TableID",
                drPluginKey,
                0);
        }

        protected override void DeletePluginDefinitions(DataRow drPluginKey)
        {
            // Do nothing
        }

        protected override void WriteToDB(DataRow drPluginDefinition)
        {
            DataTable dtPluginDefinition = drPluginDefinition.Table;
            DataTable dtSrcTable = null;
            object objSrcTableID = null;

            if (dtPluginDefinition != null)
            {
                DataColumnCollection dcPluginDefinition = dtPluginDefinition.Columns;
                if (!dcPluginDefinition.Contains("SubTableID"))
                {
                    dcPluginDefinition.Add("SubTableID");
                }

                dtSrcTable = MyWebUtils.GetBySQL("select TableID from CiTable where AppID = @AppID and TableName = @Src", drPluginDefinition);

                if (MyWebUtils.GetNumberOfRows(dtSrcTable) == 0)
                {
                    objSrcTableID = MyWebUtils.EvalSQL("?exec spTable_ins @AppID, @Src, @Caption", drPluginDefinition);
                }
                else
                {
                    objSrcTableID = MyWebUtils.GetField(dtSrcTable.Rows[0], "TableID");
                }

                bool? bound = MyWebUtils.GetField<bool>(drPluginDefinition, "Bound");
                if (bound.HasValue && bound.Value)
                {
                    drPluginDefinition["SubTableID"] = objSrcTableID;
                    MyWebUtils.GetBySQL("exec spSubTable_upd @AppID, @TableID, @SubTableID", drPluginDefinition);
                }
                else
                {
                    drPluginDefinition["SubTableID"] = MyWebUtils.EvalSQL("?exec spSubTable_ins @AppID, @TableID, @Src", drPluginDefinition);
                }
            }
        }

        protected override DataRow GetRowKeyValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            DataRow drKey =  base.GetRowKeyValues(drPluginDefinition, xPluginDefinition);
            drKey["TableID"] = drPluginDefinition["SubTableID"];

            return drKey;
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (drPluginDefinition != null)
            {
                // Download Src attribute
                XAttribute xSrc = xPluginDefinition.Attribute("src");
                if (xSrc == null)
                {
                    xSrc = new XAttribute("src", "");
                    xPluginDefinition.Add(xSrc);
                }

                string queryString = "";
                DataTable dtIdentity = MyWebUtils.GetBySQL("select * from dbo.fnGetFields(@AppID, @TableID, null, 'Identity')", drPluginDefinition);
                if (MyWebUtils.GetNumberOfRows(dtIdentity) > 0)
                {
                    queryString = "?ParentID=@ID";
                }
                xSrc.Value = MyWebUtils.GetStringField(drPluginDefinition, "TableName") + ".pux" + queryString;
            }
        }
    }
}