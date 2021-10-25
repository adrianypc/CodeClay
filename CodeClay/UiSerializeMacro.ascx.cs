using System;
using System.Data;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    public enum DirectionTypes
    {
        Upload, Download
    }

    [XmlType("CiSerializeMacro")]
    public class CiSerializeMacro: CiMacro
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        public DirectionTypes Direction { get; set; } = DirectionTypes.Upload;

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        protected override string RunValidateSQL(DataRow drParams)
        {
            string message = base.RunValidateSQL(drParams);

            if (MyUtils.IsEmpty(message))
            {
                if (Direction != DirectionTypes.Download && Direction != DirectionTypes.Upload)
                {
                    message = "Unknown serializer direction";
                }
            }

            return message;
        }

        protected override DataTable RunActionSQL(DataRow drParams)
        {
            DataTable dt = null;

            if (ActionSQL.Length == 0)
            {
                dt = drParams.Table;
            }
            else
            {
                dt = base.RunActionSQL(drParams);
            }

            int? appID = MyWebUtils.GetField<int>(drParams, "AppID");
            int? tableID = MyWebUtils.GetField<int>(drParams, "TableID");
            string dbChangeSQL = MyWebUtils.GetStringField(drParams, "DBChangeSQL");

            XiPlugin xiPlugin = null;
            if (tableID != null)
            {
                xiPlugin = new XiTable();
            }
            else
            {
                xiPlugin = new XiApplication();
            }

            string puxUrl = xiPlugin.GetPuxUrl(drParams);

            string message = "";
            switch (Direction)
            {
                case DirectionTypes.Download:
                    xiPlugin.DownloadFile(drParams, puxUrl);
                    message = "Download complete";
                    break;

                case DirectionTypes.Upload:
                    xiPlugin.UploadFile(drParams, puxUrl);
                    message = "Upload complete";
                    break;
            }

            ResultScript = string.Format("alert('{0}')", message);

            if (!MyUtils.IsEmpty(dbChangeSQL))
            {
                dbChangeSQL = dbChangeSQL.Replace("'", "''");
                dbChangeSQL = string.Format("exec ('{0}')", dbChangeSQL);
                UiApplication.Me.GetBySQL(dbChangeSQL, drParams, appID);
            }

            return dt;
        }
    }

    public partial class UiSerializeMacro : UiMacro
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public virtual CiSerializeMacro CiSerializeMacro
        {
            get { return CiPlugin as CiSerializeMacro; }
            set { CiPlugin = value; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            if (CiSerializeMacro != null)
            {
                switch (CiSerializeMacro.Direction)
                {
                    case DirectionTypes.Download:
                        dxImportPUX.Visible = false;

                        if (CiSerializeMacro != null)
                        {
                            CiSerializeMacro.Run(GetState());

                            dxImportPUX.JSProperties["cpScript"] = CiSerializeMacro.ResultScript;
                        }
                        break;

                    case DirectionTypes.Upload:
                        dxImportPUX.Visible = true;
                        break;
                }
            }
        }

        protected void dxImportPUX_FileUploadComplete(object sender, FileUploadCompleteEventArgs e)
        {
            UploadedFile uploadedFile = e.UploadedFile;
            if (uploadedFile != null)
            {
                DataRow drParams = GetState();

                XiTable xiTable = new XiTable();

                string puxUrl = xiTable.GetPuxUrl(drParams);
                uploadedFile.SaveAs(puxUrl);                

                if (CiSerializeMacro != null)
                {
                    CiSerializeMacro.Run(drParams);

                    dxImportPUX.JSProperties["cpScript"] = CiSerializeMacro.ResultScript;
                }
            }
        }
    }
}