using System;
using System.Drawing;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;

// Extra references
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
                bool fileExists = IsEditing || !emptyUrl && File.Exists(Server.MapPath(imageUrl));
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
            Refresh();
        }

        protected void dxUploadImage_FileUploadComplete(object sender, FileUploadCompleteEventArgs e)
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
                    else
                    {
                        DirectoryInfo di = new DirectoryInfo(currentFolderPath);

                        foreach (FileInfo fi in di.EnumerateFiles())
                        {
                            fi.Delete();
                        }
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
                            ImageUrl = dxCard.GetCardValues(ItemIndex, fieldName).ToString();
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
                            ImageUrl = dxGrid.GetRowValues(ItemIndex, fieldName).ToString();
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
    }
}