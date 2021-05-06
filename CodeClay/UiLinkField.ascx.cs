using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web.UI;
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
    [XmlType("CiLinkField")]
    public class CiLinkField : CiField
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlSqlElement("Folder", typeof(string))]
        public XmlElement Folder { get; set; } = null;

        [XmlElement("IsAzure")]
        public bool IsAzure { get; set; } = false;

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

        public string LinkText { get; set; } = "";
        public string LinkUrl { get; set; } = "";

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
            string navigateUrl = MyUtils.Coalesce(LinkUrl, defaultNavigateUrl);
            bool emptyUrl = MyUtils.IsEmpty(navigateUrl);

            dxLink.Caption = null;
            dxLink.BackColor = Color.Transparent;
            dxLink.Visible = !IsItemEditing;
            dxLink.ClientVisible = !emptyUrl;
            dxLink.Text = LinkText;
            dxLink.NavigateUrl = navigateUrl;
            dxLink.JSProperties["cpTableName"] = tableName;
            dxLink.JSProperties["cpFieldName"] = fieldName;
            dxLink.JSProperties["cpLinkPath"] = LinkText;
            dxLink.JSProperties["cpTextFieldName"] = CiLinkField.TextFieldName;

            dxEditLink.Visible = folderExists && IsItemEditing;
            dxEditLink.ClientVisible = !emptyUrl;
            dxEditLink.Text = LinkText;
            dxEditLink.NavigateUrl = navigateUrl;
            dxEditLink.JSProperties["cpTableName"] = tableName;
            dxEditLink.JSProperties["cpFieldName"] = fieldName;
            dxEditLink.JSProperties["cpLinkPath"] = LinkText;

            deleteButtonPanel.Visible = folderExists && IsItemEditing;
            dxDelete.ClientVisible = !emptyUrl;

            dxUpload.Visible = folderExists && IsItemEditing;
            dxUpload.ClientVisible = emptyUrl;

            dxUpdateText.Visible = !folderExists && IsItemEditing;
            dxUpdateText.Text = LinkText;
            dxUpdateText.JSProperties["cpTableName"] = tableName;
            dxUpdateText.JSProperties["cpFieldName"] = fieldName;
            dxUpdateText.JSProperties["cpLinkPath"] = LinkText;
        }

        protected void dxLinkPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            Refresh(sender, e);
        }

        protected void dxUpload_FileUploadComplete(object sender, FileUploadCompleteEventArgs e)
        {
            UploadedFile uploadedFile = e.UploadedFile;
            if (uploadedFile != null)
            {
                string currentFolder = GetSaveLocation();

                if (!MyUtils.IsEmpty(currentFolder))
                {
                    string fileName = CleanFileName(uploadedFile.FileName);
                    string filePath = currentFolder + @"\" + fileName;
                    string linkURL = filePath;

                    if (CiLinkField.IsAzure)
                    {
                        ShareFileClient azureFile = GetAzureFile(currentFolder, fileName);
                        linkURL = GetFileSasUri(azureFile.Path, DateTime.Now.AddDays(1), ShareFileSasPermissions.Read);
                        
                        Task task = new Task(() => { AppendToAzureFile(azureFile, uploadedFile.FileBytes); });
                        task.Start();
                    }
                    else
                    {
                        string currentFolderPath = MapPath(currentFolder);

                        if (!Directory.Exists(currentFolderPath))
                        {
                            Directory.CreateDirectory(currentFolderPath);
                        }

                        uploadedFile.SaveAs(MapPath(filePath));
                    }

                    e.CallbackData = filePath + LIST_SEPARATOR + linkURL;
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
                string textFieldName = CiField.TextFieldName;

                if (UiTable.IsCardView)
                {
                    CardViewBaseTemplateContainer cardContainer = container as CardViewBaseTemplateContainer;
                    if (cardContainer != null)
                    {
                        ASPxCardView dxCard = cardContainer.CardView;
                        if (dxCard != null)
                        {
                            string filePath = dxCard.GetCardValues(ItemIndex, fieldName).ToString();

                            if (CiLinkField.IsAzure)
                            {
                                LinkUrl = GetFileSasUri(filePath, DateTime.Now.AddDays(1), ShareFileSasPermissions.Read);
                            }
                            else
                            {
                                LinkUrl = filePath;
                            }

                            if (!MyUtils.IsEmpty(textFieldName))
                            {
                                LinkText = dxCard.GetCardValues(ItemIndex, textFieldName).ToString();
                            }
                            else
                            {
                                LinkText = filePath;
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
                            LinkUrl = dxGrid.GetRowValues(ItemIndex, fieldName).ToString();

                            if (!MyUtils.IsEmpty(textFieldName))
                            {
                                LinkText = dxGrid.GetRowValues(ItemIndex, textFieldName).ToString();
                            }
                            else
                            {
                                LinkText = LinkUrl;
                            }
                        }
                    }
                }
            }

            if (LinkUrl == "&nbsp;")
            {
                LinkUrl = "";
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

        private ShareFileClient GetAzureFile(string folderName, string fileName)
        {
            string shareName = MyWebUtils.Application.ToLower();

            string connectionString = Properties.Settings.Default.StorageConnectionString;

            ShareClient share = new ShareClient(connectionString, shareName);

            // Create the share if it doesn't already exist
            share.CreateIfNotExists();

            if (share.Exists())
            {
                ShareDirectoryClient subFolder = share.GetRootDirectoryClient();

                foreach (string subFolderName in folderName.Split('\\'))
                {
                    subFolder = subFolder.GetSubdirectoryClient(subFolderName);
                    subFolder.CreateIfNotExists();
                }

                return subFolder.GetFileClient(fileName);
            }

            return null;
        }

        private void AppendToAzureFile(ShareFileClient azureFile, byte[] buffer)
        {
            int bufferLength = buffer.Length;
            azureFile.Create(bufferLength);

            int maxChunkSize = 4 * 1024;
            for (int offset = 0; offset < bufferLength; offset += maxChunkSize)
            {
                int chunkSize = Math.Min(maxChunkSize, bufferLength - offset);
                MemoryStream chunk = new MemoryStream();
                chunk.Write(buffer, offset, chunkSize);
                chunk.Position = 0;

                HttpRange httpRange = new HttpRange(offset, chunkSize);
                var resp = azureFile.UploadRange(httpRange, chunk);
            }
        }

        private string GetFileSasUri(string filePath, DateTime expiration, ShareFileSasPermissions permissions)
        {
            string shareName = MyWebUtils.Application.ToLower();

            // Get the account details from app settings
            string accountName = Properties.Settings.Default.StorageAccountName;
            string accountKey = Properties.Settings.Default.StorageAccountKey;

            ShareSasBuilder fileSAS = new ShareSasBuilder()
            {
                ShareName = shareName,
                FilePath = filePath,

                // Specify an Azure file resource
                Resource = "f",

                // Expires in 24 hours
                ExpiresOn = expiration
            };

            // Set the permissions for the SAS
            fileSAS.SetPermissions(permissions);

            // Create a SharedKeyCredential that we can use to sign the SAS token
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(accountName, accountKey);

            // Build a SAS URI
            UriBuilder fileSasUri = new UriBuilder($"https://{accountName}.file.core.windows.net/{fileSAS.ShareName}/{fileSAS.FilePath}");
            fileSasUri.Query = fileSAS.ToSasQueryParameters(credential).ToString();

            // Return the URI
            return fileSasUri.Uri.ToString();
        }
    }
}