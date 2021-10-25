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
using DevExpress.Data.PivotGrid;
using DevExpress.Web.ASPxPivotGrid;
using DevExpress.XtraPivotGrid;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

namespace CodeClay
{
    [XmlType("CiPivotField")]
    public class CiPivotField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("Area")]
        public PivotArea Area { get; set; } = PivotArea.ColumnArea;

        [XmlElement("AreaIndex")]
        public int AreaIndex { get; set; } = 0;

        [XmlElement("GroupIndex")]
        public int GroupIndex { get; set; } = 0;

        [XmlElement("InnerGroupIndex")]
        public int InnerGroupIndex { get; set; } = 0;

        [XmlElement("SummaryType")]
        public PivotSummaryType SummaryType { get; set; } = PivotSummaryType.Custom;

        [XmlElement("FormatType")]
        public FormatType Format { get; set; } = FormatType.None;

        [XmlElement("Wrap")]
        public bool Wrap { get; set; } = false;
    }

    [XmlType("CiPivotTable")]
    public class CiPivotTable : CiTable
    {
        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual CiPivotField GetPivotField(string fieldName)
        {
            return GetField(fieldName) as CiPivotField;
        }

        public virtual CiPivotField[] GetLinkFields()
        {
            CiField[] ciFields = CiFields.Where(c => !MyUtils.IsEmpty(c.TextFieldName)).ToArray();

            return ciFields.OfType<CiPivotField>().ToArray();
        }
    }

    public partial class UiPivotTable : UiTable
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual CiPivotTable CiPivotTable
        {
            get { return CiPlugin as CiPivotTable; }
            set { CiPlugin = value; }
        }

        // --------------------------------------------------------------------------------------------------
        // Properties (Override)
        // --------------------------------------------------------------------------------------------------

        public override bool IsInserting
        {
            get
            {
                return false;
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && CiPivotTable != null)
            {
                DataRow drParams = GetQueryStringState();
                Page.Title = CiPivotTable.GetTableCaption(drParams);
                BuildPivotView(dxPivotGrid, drParams);
            }
        }

        protected void dxPivotGrid_HtmlFieldValuePrepared(object sender, PivotHtmlFieldValuePreparedEventArgs e)
        {
            if (CiPivotTable != null)
            {
                PivotGridField piField = e.Field;
                if (piField != null)
                {
                    string fieldName = piField.ID.Substring(1);
                    CiPivotField ciPivotField = CiPivotTable.GetPivotField(fieldName);

                    if (ciPivotField != null && ciPivotField.Mask == "Link")
                    {
                        HyperLink link = CreateHyperLink(e.Cell);
                        string textAndUrl = (string)e.Value;
                        if (!MyUtils.IsEmpty(textAndUrl))
                        {
                            string[] tokens = textAndUrl.Split(',');
                            link.Text = tokens[0];
                            link.NavigateUrl = (tokens.Length > 1) ? tokens[1] : tokens[0];
                            link.Target = "_blank";
                        }
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers (ObjectDataSource)
        // --------------------------------------------------------------------------------------------------

        new protected void MyTableData_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {
            if (!CiTable.IsSearching)
            {
                e.InputParameters["table"] = CiPivotTable;
                e.InputParameters["view"] = GetClientValue("_View");
                e.InputParameters["parameters"] = GetQueryStringState();
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private void BuildPivotView(ASPxPivotGrid dxTable, DataRow drParams)
        {
            if (CiPivotTable != null && !CiPivotTable.IsHidden(drParams))
            {
                // Set JSProperties
                dxTable.JSProperties["cpTableName"] = CiPivotTable.TableName;

                // Build field columns
                CiField[] ciFields = CiPivotTable.GetBrowsableFields(drParams);
                foreach (CiField ciField in ciFields)
                {
                    BuildPivotColumn(dxTable, ciField as CiPivotField, drParams);
                }
            }
        }

        private void BuildPivotColumn(ASPxPivotGrid dxPivotGrid, CiPivotField ciPivotField, DataRow drParams)
        {
            string fieldName = ciPivotField.FieldName;

            PivotGridField pField = new PivotGridField();
            pField.ID = "f" + fieldName;
            pField.FieldName = fieldName;
            pField.Caption = MyWebUtils.Eval<string>(ciPivotField.Caption, drParams);
            pField.Area = ciPivotField.Area;
            pField.AreaIndex = ciPivotField.AreaIndex;
            pField.GroupIndex = ciPivotField.GroupIndex;
            pField.InnerGroupIndex = ciPivotField.InnerGroupIndex;

            if (ciPivotField.SummaryType != PivotSummaryType.Custom)
            {
                pField.SummaryType = ciPivotField.SummaryType;
            }

            pField.ValueFormat.FormatType = ciPivotField.Format;
            pField.DataBinding = new DataSourceColumnBinding(ciPivotField.FieldName);
            pField.Visible = !MyWebUtils.Eval<bool>(ciPivotField.Hidden, drParams);
            pField.ValueStyle.Wrap = ciPivotField.Wrap ? DefaultBoolean.True : DefaultBoolean.False;

            dxPivotGrid.Fields.Add(pField);
        }

        private HyperLink CreateHyperLink(TableCell cell)
        {
            if (cell != null)
            {
                HyperLink link = new HyperLink();

                // Clear out all Literal controls from cell
                foreach (Control control in cell.Controls)
                {
                    if (control is LiteralControl)
                    {
                        cell.Controls.Remove(control);
                        break;
                    }
                }

                // Add hyperlink
                cell.Controls.Add(link);

                return link;
            }

            return null;
        }
    }

    public class XiPivotTable : XiTable
    {
        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        public XiPivotTable()
        {
            PluginType = typeof(CiPivotTable);
        }

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
                return xElements.FindAll(el => el.Name == "CiPivotTable");
            }

            return null;
        }

        protected override void DeletePluginDefinitions(DataRow drPluginKey)
        {
            MyWebUtils.GetBySQL("exec spField_del @AppID, @TableID", drPluginKey, 0);
            MyWebUtils.GetBySQL("exec spMacro_del @AppID, @TableID", drPluginKey, 0);
            MyWebUtils.GetBySQL("exec spSubTable_del @AppID, @TableID", drPluginKey, 0);
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

            DataTable dtSubTable = MyWebUtils.GetBySQL("select * from dbo.fnGetSubTablesFromSrc(@AppID, @TableID)", drPluginDefinition, 0);
            if (dtSubTable != null)
            {
                foreach (DataRow drSubTable in dtSubTable.Rows)
                {
                    string tableName = MyWebUtils.GetStringField(drSubTable, "TableName");
                    if (!MyUtils.IsEmpty(tableName))
                    {
                        XElement xTable = new XElement("CiPivotTable");
                        xPluginDefinition.Add(xTable);

                        XElement xTableName = new XElement("TableName", tableName);
                        xTable.Add(xTableName);

                        XElement xHidden = new XElement("Hidden", true);
                        xTable.Add(xHidden);
                    }
                }
            }

        }

        protected override void UploadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            DataColumnCollection dcPluginColumns = drPluginDefinition.Table.Columns;

            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                foreach (XAttribute xProperty in xPluginDefinition.Attributes())
                {
                    string xPropertyName = xProperty.Name.ToString();
                    switch (xPropertyName)
                    {
                        case "src":
                            string filename = xProperty.Value;
                            if (!MyUtils.IsEmpty(filename) && filename.ToLower().EndsWith(".pux"))
                            {
                                filename = filename.Substring(0, filename.Length - 4);
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
            }
        }

        protected override List<XiPlugin> GetXiChildPlugins()
        {
            return new List<XiPlugin>() { new XiField(), new XiMacro(), new XiButtonField() };
        }
    }
}