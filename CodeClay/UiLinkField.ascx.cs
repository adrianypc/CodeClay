using System;
using System.Drawing;
using System.IO;
using System.Web.UI;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiLinkField")]
    public class CiLinkField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlSqlElement("Folder", typeof(string))]
        public XmlElement Folder { get; set; } = null;

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override CardViewColumn CreateCardColumn(UiTable uiTable)
        {
            CardViewHyperLinkColumn dxColumn = new CardViewHyperLinkColumn();

            if (uiTable != null)
            {
                dxColumn.DataItemTemplate = uiTable.CreateTemplate(this);
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

        public override GridViewDataColumn CreateGridColumn(UiTable uiTable)
        {
            GridViewDataHyperLinkColumn dxColumn = new GridViewDataHyperLinkColumn();

            if (uiTable != null)
            {
                dxColumn.DataItemTemplate = uiTable.CreateTemplate(this);
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

		public override bool HasBorder(UiTable uiTable)
		{
			return false;
		}
    }

    public partial class UiLinkField : UiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public string NavigateUrl { get; set; } = "";

        public CiLinkField CiLinkField
        {
            get { return CiField as CiLinkField; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            mEditor = dxUpdateText;
            base.Page_Load(sender, e);

            string defaultNavigateUrl = (CiLinkField != null) ? CiLinkField.Value : "";
            bool folderExists = (CiLinkField != null) && (CiLinkField.Folder != null);
            string tableName = (CiTable != null) ? CiTable.TableName : "";
            string fieldName = (CiLinkField != null) ? CiLinkField.FieldName : "";
            string navigateUrl = MyUtils.Coalesce(NavigateUrl, defaultNavigateUrl);
            bool emptyUrl = MyUtils.IsEmpty(navigateUrl);
            string linkText = folderExists ? Path.GetFileName(navigateUrl) : navigateUrl;

            dxLink.Caption = null;
            dxLink.BackColor = Color.Transparent;
            dxLink.Visible = !IsItemEditing;
            dxLink.ClientVisible = !emptyUrl;
            dxLink.Text = linkText;
            dxLink.NavigateUrl = navigateUrl;
            dxLink.JSProperties["cpTableName"] = tableName;
            dxLink.JSProperties["cpFieldName"] = fieldName;
            dxLink.JSProperties["cpShortNavigateUrl"] = navigateUrl;

            dxEditLink.Visible = folderExists && IsItemEditing;
            dxEditLink.ClientVisible = !emptyUrl;
            dxEditLink.Text = linkText;
            dxEditLink.NavigateUrl = navigateUrl;
            dxEditLink.JSProperties["cpTableName"] = tableName;
            dxEditLink.JSProperties["cpFieldName"] = fieldName;
            dxEditLink.JSProperties["cpShortNavigateUrl"] = navigateUrl;

            deleteButtonPanel.Visible = folderExists && IsItemEditing;
            dxDelete.ClientVisible = !emptyUrl;

            dxUpload.Visible = folderExists && IsItemEditing;
            dxUpload.ClientVisible = emptyUrl;

            dxUpdateText.Visible = !folderExists && IsItemEditing;
            dxUpdateText.Text = linkText;
            dxUpdateText.JSProperties["cpTableName"] = tableName;
            dxUpdateText.JSProperties["cpFieldName"] = fieldName;
            dxUpdateText.JSProperties["cpShortNavigateUrl"] = navigateUrl;
        }

        protected void dxLinkPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh();
        }

        protected void dxUpload_FileUploadComplete(object sender, FileUploadCompleteEventArgs e)
        {
            UploadedFile uploadedFile = e.UploadedFile;
            if (uploadedFile != null)
            {
                string currentFolder = GetSaveLocation();

                if (!MyUtils.IsEmpty(currentFolder))
                {
                    string currentFolderPath = MapPath(currentFolder);

                    if (!Directory.Exists(currentFolderPath))
                    {
                        Directory.CreateDirectory(currentFolderPath);
                    }

                    string fileName = CleanFileName(uploadedFile.FileName);
                    string savedFileURL = currentFolder + @"\" + fileName;

                    uploadedFile.SaveAs(MapPath(savedFileURL));

                    e.CallbackData = fileName + LIST_SEPARATOR + savedFileURL;
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override void ConfigureIn(Control container)
        {
            base.ConfigureIn(container);

            if (UiTable != null && CiField != null && ItemIndex >= 0)
            {
                string fieldName = CiField.FieldName;

                if (UiTable.IsCardView)
                {
                    CardViewBaseTemplateContainer cardContainer = container as CardViewBaseTemplateContainer;
                    if (cardContainer != null)
                    {
                        ASPxCardView dxCard = cardContainer.CardView;
                        if (dxCard != null)
                        {
                            NavigateUrl = dxCard.GetCardValues(ItemIndex, fieldName).ToString();
                        }
                    }
                }
                else if (UiTable.IsGridView)
                {
                    GridViewBaseTemplateContainer gridContainer = container as GridViewBaseTemplateContainer;
                    if (gridContainer != null)
                    {
                        ASPxGridView dxGrid = gridContainer.Grid;
                        if (dxGrid != null)
                        {
                            NavigateUrl = dxGrid.GetRowValues(ItemIndex, fieldName).ToString();
                        }
                    }
                }
            }

            if (NavigateUrl == "&nbsp;")
            {
                NavigateUrl = "";
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private string GetSaveLocation()
        {
            string currentFolder = "";

            string currentFileUrl = dxLink.NavigateUrl;
            if (!MyUtils.IsEmpty(currentFileUrl) && UiTable == null)
            {
                currentFolder = Path.GetDirectoryName(currentFileUrl);
            }
            else
            {
                currentFolder = MyWebUtils.Eval<string>(CiLinkField.Folder, GetState());
            }

            return currentFolder;
        }

        private string CleanFileName(string filename)
        {
            if (!MyUtils.IsEmpty(filename))
            {
                return filename.Replace(" ", "_");
            }

            return "";
        }
    }
}