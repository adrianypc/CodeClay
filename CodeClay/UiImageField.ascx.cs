using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using Azure;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Sas;
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiImageField")]
    public class CiImageField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlSqlElement("Folder", typeof(string))]
        public XmlElement Folder { get; set; } = null;

        [XmlElement("IsLinkAzure")]
        public bool IsLinkAzure { get; set; } = UiApplication.Me.CiApplication.IsLinkAzure;

        [XmlElement("ImageWidth")]
        public int ImageWidth { get; set; } = 0;

        [XmlElement("ImageHeight")]
        public int ImageHeight { get; set; } = 0;

        // --------------------------------------------------------------------------------------------------
        // Methods (Override)
        // --------------------------------------------------------------------------------------------------

        public override CardViewColumn CreateCardColumn(UiTable uiTable)
        {
            CardViewImageColumn dxColumn = new CardViewImageColumn();

            if (uiTable != null)
            {
                dxColumn.DataItemTemplate = uiTable.CreateTemplate(this);
                dxColumn.EditItemTemplate = uiTable.CreateTemplate(this);
            }

            return dxColumn;
        }

        public override GridViewDataColumn CreateGridColumn(UiTable uiTable)
        {
            GridViewDataImageColumn dxColumn = new GridViewDataImageColumn();

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

    public partial class UiImageField : UiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public string ImageUrl { get; set; } = "";

        public CiImageField CiImageField
        {
            get { return CiField as CiImageField; }
        }

        public override bool IsEditing
        {
            get
            {
                return (Request.QueryString["Command"] == "Edit") || IsItemEditing;
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            mEditor = dxImage;
            base.Page_Load(sender, e);

            if (CiImageField != null)
            {
                string defaultImageUrl = CiImageField.Value;
                bool folderExists = (CiImageField.Folder != null);
                string tableName = CiTable.TableName;
                string fieldName = CiImageField.FieldName;
                string imageUrl = MyUtils.Coalesce(ImageUrl, defaultImageUrl);
                bool emptyUrl = MyUtils.IsEmpty(imageUrl);
                bool isInserting = (UiParentPlugin != null && UiParentPlugin.IsInserting);
                bool fileExists = IsEditing || !emptyUrl && DoesImageExist(imageUrl);
                int imageWidth = fileExists ? CiImageField.ImageWidth : 1;
                int imageHeight = fileExists ? CiImageField.ImageHeight : 1;
                
                dxImage.Caption = null;
                dxImage.BackColor = Color.Transparent;
                dxImage.Visible = !IsEditing;
                dxImage.ClientVisible = !emptyUrl;
                dxImage.ImageUrl = imageUrl;
                dxImage.JSProperties["cpTableName"] = tableName;
                dxImage.JSProperties["cpFieldName"] = fieldName;
                dxImage.JSProperties["cpShortImageUrl"] = imageUrl;

                dxEditImage.Visible = folderExists && IsEditing;
                dxEditImage.ClientVisible = !emptyUrl;
                dxEditImage.ImageUrl = imageUrl;
                dxEditImage.JSProperties["cpTableName"] = tableName;
                dxEditImage.JSProperties["cpFieldName"] = fieldName;
                dxEditImage.JSProperties["cpShortImageUrl"] = imageUrl;

                if (imageWidth > 0)
                {
                    dxImage.Width = Unit.Pixel(imageWidth);
                    dxEditImage.Width = Unit.Pixel(imageWidth);
                }

                if (imageHeight > 0)
                {
                    dxImage.Height = Unit.Pixel(imageHeight);
                    dxEditImage.Height = Unit.Pixel(imageHeight);
                }

                deleteButtonPanel.Visible = folderExists && IsEditing;
                dxDeleteImage.ClientVisible = !emptyUrl;

                dxUploadImage.Visible = folderExists && IsEditing;
                dxUploadImage.ClientVisible = emptyUrl;
            }
        }

        protected void dxImagePanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh(sender, e);
        }

        protected void dxUploadImage_FileUploadComplete(object sender, FileUploadCompleteEventArgs e)
        {
            UploadedFile uploadedFile = e.UploadedFile;
            if (uploadedFile != null)
            {
                string saveFolder = GetSaveLocation();

                if (!MyUtils.IsEmpty(saveFolder))
                {
                    string filePath = MyWebUtils.GetFilePath(saveFolder, uploadedFile);
                    string linkURL = "";

                    if (CiImageField.IsLinkAzure)
                    {
                        linkURL = MyWebUtils.UploadToAzureFileStorage(saveFolder, uploadedFile);
                    }
                    else
                    {
                        linkURL = MyWebUtils.UploadToWebServer(saveFolder, uploadedFile);
                    }

                    string fileName = MyWebUtils.CleanFileName(uploadedFile.FileName);

                    e.CallbackData = fileName + LIST_SEPARATOR + filePath;
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
                            string filePath = dxCard.GetCardValues(ItemIndex, fieldName).ToString();

                            if (CiImageField.IsLinkAzure)
                            {
                                ImageUrl = MyWebUtils.GetFileSasUri(filePath, DateTime.Now.AddDays(1), ShareFileSasPermissions.Read);
                            }
                            else
                            {
                                ImageUrl = filePath;
                            }
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
                            string filePath = dxGrid.GetRowValues(ItemIndex, fieldName).ToString();

                            if (CiImageField.IsLinkAzure)
                            {
                                ImageUrl = MyWebUtils.GetFileSasUri(filePath, DateTime.Now.AddDays(1), ShareFileSasPermissions.Read);
                            }
                            else
                            {
                                ImageUrl = filePath;
                            }
                        }
                    }
                }
            }

            if (ImageUrl == "&nbsp;")
            {
                ImageUrl = "";
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private string GetSaveLocation()
        {
            string currentFolder = "";

            string currentFileUrl = dxImage.ImageUrl;
            if (!MyUtils.IsEmpty(currentFileUrl) && UiTable == null)
            {
                currentFolder = Path.GetDirectoryName(currentFileUrl);
            }
            else
            {
                currentFolder = MyWebUtils.Eval<string>(CiImageField.Folder, GetState());
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

        private bool DoesImageExist(string imageUrl)
        {
            if (CiImageField.IsLinkAzure)
            {
                try
                {
                    var request = WebRequest.Create(imageUrl) as HttpWebRequest;
                    request.Method = "HEAD";
                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        return response.StatusCode == HttpStatusCode.OK;
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return File.Exists(Server.MapPath(imageUrl));
            }
        }
    }
}