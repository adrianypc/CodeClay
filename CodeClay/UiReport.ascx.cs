using System;
using System.Data;
using System.IO;
using System.Web;
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

        [XmlElement("Designer")]
        public bool Designer { get; set; } = false;

        [XmlElement("ReportUrl")]
        public string ReportUrl { get; set; } = "";

        [XmlElement("ReportSQL")]
        public string ReportSQL { get; set; } = "";
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
                string url = CiReport.ReportUrl;
                DataRow drParams = GetState();
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
                        report.DataSource = GetReportData(drParams);
                    }
                    else
                    {
                        dxWebDocumentViewer.OpenReport(report);
                        dxWebDocumentViewer.Visible = true;

                        dxReportDesigner.Visible = false;

                        report.DataSource = GetReportData(drParams);
                    }
                }
            }
        }

        protected void dxReportDesigner_SaveReportLayout(object sender, DevExpress.XtraReports.Web.SaveReportLayoutEventArgs e)
        {
            if (CiReport != null)
            {
                string url = CiReport.ReportUrl;
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

        private DataSet GetReportData(DataRow drParams)
        {
            if (CiReport != null)
            {
                DataSet ds = new DataSet();
                string reportSQL = CiReport.ReportSQL;

                if (!MyUtils.IsEmpty(reportSQL))
                {
                    DataTable dt = MyWebUtils.GetBySQL(reportSQL, drParams);
                    if (dt != null)
                    {
                        dt.TableName = "ReportFields";

                        if (dt.DataSet != null)
                        {
                            ds = dt.DataSet;
                        }
                        else
                        {
                            ds.Tables.Add(dt);
                        }
                    }
                }

                return ds;
            }

            return null;
        }
    }
}