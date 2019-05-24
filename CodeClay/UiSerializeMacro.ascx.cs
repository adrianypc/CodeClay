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

        public override void Run(DataRow drParams)
        {
            if (drParams != null)
            {
                ResultTable = drParams.Table;

                ResultScript = GetResultScript(ResultTable);
            }
        }

        protected override string GetResultScript(DataTable dt)
        {
            string script = "alert('Nothing to serialize')";
            DataRow drParams = (MyWebUtils.GetNumberOfRows(dt) > 0)
                ? dt.Rows[0]
                : null;

            string message = "Unknown serializer direction";

            XiTable xiTable = new XiTable();
            string puxUrl = xiTable.GetPuxUrl(drParams);

            switch (Direction)
            {
                case DirectionTypes.Download:
                    xiTable.DownloadFile(drParams, puxUrl);
                    message = "Download complete";
                    break;

                case DirectionTypes.Upload:
                    xiTable.UploadFile(drParams, puxUrl);
                    message = "Upload complete";
                    break;
            }

            script = string.Format("alert('{0}')", message);

            return script;
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

                        XiTable xiTable = new XiTable();

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