using System;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.XtraReports.Native;
using DevExpress.XtraReports.UI;

namespace CodeClay
{
    [XmlType("CiReport")]
    public class CiReport: CiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("Url")]
        public string Url { get; set; } = "";

        [XmlElement("ReportSQL")]
        public string ReportSQL { get; set; } = "";

        [XmlElement("Designer")]
        public bool Designer { get; set; } = false;
    }

    public partial class UiReport : UiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public CiReport CiReport
        {
            get { return CiPlugin as CiReport; }
            set { CiPlugin = value; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            if (CiReport != null)
            {
                string url = CiReport.Url;
                if (!MyUtils.IsEmpty(url))
                {
                    XtraReport report = new XtraReport();
                    string filePath = MyWebUtils.MapPath(MyWebUtils.ApplicationFolder + @"\" + url);

                    if (File.Exists(filePath))
                    {
                        report.LoadLayoutFromXml(filePath);
                    }

                    if (CiReport.Designer)
                    {
                        dxReportDesigner.OpenReport(report);
                        dxReportDesigner.Visible = true;

                        dxWebDocumentViewer.Visible = false;

                        report.Extensions[SerializationService.Guid] = DataSetSerializer.Name;
                        report.DataSource = GetReportData();
                    }
                    else
                    {
                        dxWebDocumentViewer.OpenReport(report);
                        dxWebDocumentViewer.Visible = true;

                        dxReportDesigner.Visible = false;

                        report.DataSource = GetReportData();
                    }
                }
            }
        }

        protected void dxReportDesigner_SaveReportLayout(object sender, DevExpress.XtraReports.Web.SaveReportLayoutEventArgs e)
        {
            if (CiReport != null)
            {
                string url = CiReport.Url;
                if (!MyUtils.IsEmpty(url))
                {
                    XtraReport report = new XtraReport();
                    report.LoadLayout(new MemoryStream(e.ReportLayout));

                    string filePath = MyWebUtils.MapPath(MyWebUtils.ApplicationFolder + @"\" + url);
                    report.SaveLayoutToXml(filePath);
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private DataSet GetReportData()
        {
            if (CiReport != null)
            {
                DataSet ds = new DataSet("ReportTable");

                DataTable dt = MyWebUtils.GetBySQL(CiReport.ReportSQL, GetState());
                if (dt != null)
                {
                    dt.TableName = "ReportFields";
                    ds.Tables.Add(dt);
                }

                return ds;
            }

            return null;
        }
    }
}