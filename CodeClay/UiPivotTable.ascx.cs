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

    public class XiPivotField : XiField
    {
        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        public XiPivotField()
        {
            PluginType = typeof(XiPivotField);
        }

        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        private bool IsRowKeyChecked { get; set; } = false;

        private Hashtable mPropertySQL = new Hashtable();

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        protected override DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            DataTable dt = MyWebUtils.GetBySQL("?exec spField_sel @AppID, @TableID, null, 'Column,Data,Row'", drPluginKey, 0);

            if (dt != null)
            {
                dt.Columns.Add("AreaIndex", typeof(int));

                int rowAreaIndex = 0;
                int columnAreaIndex = 0;

                foreach (DataRow dr in dt.Rows)
                {
                    string type = MyWebUtils.GetStringField(dr, "Type");
                    switch (type)
                    {
                        case "Row":
                            dr["AreaIndex"] = rowAreaIndex;
                            rowAreaIndex++;
                            break;

                        case "Column":
                            dr["AreaIndex"] = columnAreaIndex;
                            columnAreaIndex++;
                            break;
                    }
                }
            }

            return dt;
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                xPluginDefinition.Name = "CiPivotField";

                MyWebUtils.CreateXChild(xPluginDefinition,
                    "FieldName",
                    MyWebUtils.GetStringField(drPluginDefinition, "FieldName"));

                MyWebUtils.CreateXChild(xPluginDefinition,
                    "Caption",
                    MyWebUtils.GetStringField(drPluginDefinition, "Caption"));

                MyWebUtils.CreateXChild(xPluginDefinition,
                    "AreaIndex",
                    MyWebUtils.GetStringField(drPluginDefinition, "AreaIndex"));

                MyWebUtils.CreateXChild(xPluginDefinition,
                    "Area",
                    string.Format("{0}Area", MyWebUtils.GetStringField(drPluginDefinition, "Type")));

                MyWebUtils.CreateXChild(xPluginDefinition,
                    "Hidden",
                    MyWebUtils.GetField<bool>(drPluginDefinition, "Hidden").ToString());

                MyWebUtils.CreateXChild(xPluginDefinition, "Wrap", "true");

                string mask = MyWebUtils.GetStringField(drPluginDefinition, "Mask");
                switch (mask)
                {
                    case "Link":
                        MyWebUtils.CreateXChild(xPluginDefinition, "Mask", "Link");
                        MyWebUtils.CreateXChild(xPluginDefinition, "Computed", "true");
                        break;

                    case "DateTime":
                    case "Numeric":
                        MyWebUtils.CreateXChild(xPluginDefinition, "Mask", mask);
                        break;
                }

                MyWebUtils.CreateXChild(xPluginDefinition,
                    "Value",
                    MyWebUtils.GetStringField(drPluginDefinition, "Value"));

                string summary = MyWebUtils.GetStringField(drPluginDefinition, "Summary");

                if (summary != "None")
                {
                    MyWebUtils.CreateXChild(xPluginDefinition,
                      "SummaryType",
                      summary);
                }
            }
        }

        protected override void UploadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            DataColumnCollection dcPluginColumns = drPluginDefinition.Table.Columns;

            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                const string cArea = "Area";
                string area = xPluginDefinition.Element(cArea).Value;
                if (!MyUtils.IsEmpty(area) && area.EndsWith(cArea))
                drPluginDefinition["Type"] = area.Substring(0, area.Length - cArea.Length);
            }
        }
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

        public override string GetPuxUrl(DataRow drPluginKey)
        {
            string appName = MyWebUtils.GetApplicationName(drPluginKey);

            if (PluginType != null)
            {
                string keyName = "TableName";
                string keyValue = "";

                DataRow drPluginDefinition = GetPluginDefinition(drPluginKey);

                if (drPluginDefinition != null && drPluginDefinition.Table.Columns.Contains(keyName))
                {
                    object objKeyValue = drPluginDefinition[keyName];

                    if (objKeyValue != null)
                    {
                        keyValue = objKeyValue.ToString();
                    }

                    if (!MyUtils.IsEmpty(appName) && !MyUtils.IsEmpty(keyValue))
                    {
                        return MyWebUtils.MapPath(string.Format("Sites/{0}/{1}.pux", appName, keyValue));
                    }
                }
            }

            return null;
        }

        protected override List<XElement> GetPluginDefinitions(List<XElement> xElements)
        {
            if (xElements != null)
            {
                return xElements.FindAll(el => el.Name == "CiPivotTable");
            }

            return null;
        }

        protected override string GetXPropertyName(Type pluginType, string dPropertyName)
        {
            string xPropertyName = base.GetXPropertyName(pluginType, dPropertyName);

            switch (xPropertyName)
            {
                case "TableName":
                case "TableCaption":
                    // Do nothing
                    break;

                default:
                    // Remove other properties belonging to CiTable
                    xPropertyName = null;
                    break;
            }

            return xPropertyName;
        }

        protected override void UploadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            DataColumnCollection dcPluginColumns = drPluginDefinition.Table.Columns;

            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                drPluginDefinition["TableName"] = Convert.DBNull;
                drPluginDefinition["DefaultView"] = "Pivot";
            }
        }

        protected override void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            // Do not REMOVE as this empty method negates the functionality of the base class XiTable
        }

        protected override List<XiPlugin> GetXiChildPlugins()
        {
            return new List<XiPlugin>() { new XiPivotField(), new XiMacro() };
        }
    }
}