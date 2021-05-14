using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web.ASPxPivotGrid;
using DevExpress.Data.PivotGrid;
using DevExpress.XtraPivotGrid;
using DevExpress.Utils;

namespace CodeClay
{
    [XmlType("CiPivotRowField")]
    public class CiPivotField: CiField
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
        public DefaultBoolean Wrap { get; set; } = DefaultBoolean.False;

        // --------------------------------------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------------------------------------

        public void AppendField(ASPxPivotGrid dxPivotGrid, DataRow drParams)
        {
            PivotGridField pField = new PivotGridField();
            pField.FieldName = FieldName;
            pField.Caption = MyWebUtils.Eval<string>(Caption, drParams);
            pField.Area = Area;
            pField.AreaIndex = AreaIndex;
            pField.GroupIndex = GroupIndex;
            pField.InnerGroupIndex = InnerGroupIndex;
            pField.SummaryType = SummaryType;
            pField.ValueFormat.FormatType = Format;
            pField.DataBinding = new DataSourceColumnBinding(FieldName);
            pField.Visible = !MyWebUtils.Eval<bool>(Hidden, drParams);
            pField.ValueStyle.Wrap = Wrap;

            dxPivotGrid.Fields.Add(pField);
        }
    }

    public partial class UiPivotGrid : System.Web.UI.Page
    {   
        protected void Page_Load(object sender, EventArgs e)
        {
            XmlDocument xDoc = new XmlDocument();
            XmlElement xCaption = xDoc.CreateElement("Caption");
            xCaption.InnerText = "Supply";

            CiPivotField pField = new CiPivotField();
            pField.FieldName = "Inventory_QOH";
            pField.Area = PivotArea.RowArea;
            pField.AreaIndex = 1;
            pField.Caption = xCaption;
            pField.GroupIndex = 1;
            pField.InnerGroupIndex = 1;
            pField.SummaryType = PivotSummaryType.Min;
            pField.Format = FormatType.Numeric;

            pField.AppendField(dxPivotGrid, null);
        }

        protected void dxPivotGrid_HtmlFieldValuePrepared(object sender, DevExpress.Web.ASPxPivotGrid.PivotHtmlFieldValuePreparedEventArgs e)
        {
            HyperLink link = new HyperLink();
            if (e.Field == dxPivotGrid.GetFieldByArea(DevExpress.XtraPivotGrid.PivotArea.ColumnArea, 6))
            {
                string getValue = (string)e.Value;
                string[] getParaList = getValue.Split('=');
                string getTextValue = getParaList[getParaList.Length - 1];

                link.NavigateUrl = getValue;
                link.Text = getTextValue;
                
                foreach (Control control in e.Cell.Controls)
                {
                    if (control is LiteralControl)
                    {
                        e.Cell.Controls.Remove(control);
                        break;
                    }
                }
                e.Cell.Controls.Add(link);
            }
        }
    }
}