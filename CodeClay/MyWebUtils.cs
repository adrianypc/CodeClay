using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

// Extra references
using Azure;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Sas;
using CodistriCore;
using DevExpress.Web;
using DevExpress.XtraReports.Native;

namespace CodeClay
{
    // --------------------------------------------------------------------------------------------------
    // Custom attributes
    // --------------------------------------------------------------------------------------------------

    public class XmlSqlElementAttribute: XmlAnyElementAttribute
    {
        public Type Type { get; set; } = null;

        public XmlSqlElementAttribute(string name, Type type): base(name)
        {
            Type = type;
        }
    }

    // --------------------------------------------------------------------------------------------------
    // Enumerations
    // --------------------------------------------------------------------------------------------------

    public enum eLangType
    {
        literal = 0,
        column,
        sql,
        xml
    }

	public enum eTableMode
	{
		Browse = 0,
		Search,
		New,
		Edit
	}

    public class MyWebUtils
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public static NameValueCollection QueryString { get; set; }

        public static string QueryStringCommand { get; set; }

        public static bool IsConnectedToCPanel { get; set; } = false;

        public static string Application
        {
            get
            {
                string application = "";

                if (UiApplication.Me != null)
                {
                    application = UiApplication.Me.CiApplication.AppName;
                }

                return MyUtils.Coalesce(QueryString["Application"],
                    application,
                    "CPanel");
            }
        }

        public static string ApplicationFolder
        {
            get { return @"Sites\" + Application; }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------------------------------------

        public static T? GetField<T>(DataRow dr, string fieldName) where T : struct, IComparable
        {
            if (dr != null)
            {
                DataTable dt = dr.Table;
                if (dt != null)
                {
                    DataColumnCollection dc = dt.Columns;
                    if (dc != null && dc.Contains(fieldName))
                    {
                        object value = dr[fieldName];
                        if (Convert.IsDBNull(value))
                        {
                            value = null;
                        }

                        if (value != null)
                        {
                            try
                            {
                                return (T?)Convert.ChangeType(value, typeof(T));
                            }
                            finally
                            {
                                // Do nothing
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static object GetField(DataRow dr, string fieldName)
        {
            if (dr != null)
            {
                DataTable dt = dr.Table;
                if (dt != null)
                {
                    DataColumnCollection dc = dt.Columns;
                    if (dc != null && dc.Contains(fieldName))
                    {
                        return dr[fieldName];
                    }
                }
            }

            return null;
        }

        public static string GetStringField(DataRow dr, string fieldName)
        {
            object fieldValue = GetField(dr, fieldName);

            if (fieldValue != null)
            {
                return fieldValue.ToString();
            }

            return null;
        }

        public static DataRow AppendColumns(DataRow dr, DataRow dr2, bool coalesce = true)
        {
            if (dr == null && dr2 == null)
                return null;

            if (dr == null && dr2 != null)
                return MyUtils.CloneDataRow(dr2);

            if (dr != null && dr2 == null)
                return MyUtils.CloneDataRow(dr);

            DataRow drExtended = MyUtils.CloneDataRow(dr);
            foreach (DataColumn column2 in dr2.Table.Columns)
            {
                string columnName2 = column2.ColumnName;
                object columnValue2 = dr2[columnName2];
                if (drExtended.Table.Columns.Contains(columnName2))
                {
                    if (!MyUtils.IsEmpty(columnValue2) && MyUtils.IsEmpty(drExtended[columnName2]) && coalesce)
                    {
                        drExtended[columnName2] = columnValue2;
                    }
                }
                else
                {
                    drExtended[drExtended.Table.Columns.Add(columnName2, column2.DataType)] = columnValue2;
                }
            }

            return drExtended;
        }

        public static string MapPath(string path)
        {
            if (!Path.IsPathRooted(path) && HttpContext.Current != null)
            {
                path = HttpContext.Current.Server.MapPath(path);
            }

            return path;
        }

        public static void RegisterPluginScripts(Page webPage)
        {
            if (webPage != null)
            {
                string scriptWebFolder = Properties.Settings.Default.ScriptFolder;
                string scriptPhysicalFolder = webPage.Server.MapPath(scriptWebFolder);

                if (Directory.Exists(scriptPhysicalFolder))
                {
                    foreach (string filePath in Directory.GetFiles(scriptPhysicalFolder))
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        string fileName = fileInfo.Name;
                        string fileExtension = fileInfo.Extension;
                        string keyName = fileName.Substring(0, fileName.Length - fileExtension.Length);
                        webPage.ClientScript.RegisterClientScriptInclude(keyName, scriptWebFolder + @"\" + fileName);
                    }
                }
            }
        }

        public static DataTable ToDataTable(string xml, string tableName = null)
        {
            DataTable dt = new DataTable();

            if (!MyUtils.IsEmpty(xml))
            {
                DataColumnCollection dcColumns = dt.Columns;

                XElement dataSource = XElement.Parse(xml);
                foreach (XElement dataItem in dataSource.Elements())
                {
                    DataRow dr = dt.NewRow();

                    foreach (XAttribute field in dataItem.Attributes())
                    {
                        string fieldName = field.Name.LocalName;
                        string fieldValue = field.Value;

                        if (!dcColumns.Contains(fieldName))
                        {
                            dcColumns.Add(fieldName);
                        }

                        dr[fieldName] = fieldValue;
                    }

                    dt.Rows.Add(dr);
                }

                if (tableName == null)
                {
                    dt.TableName = dataSource.Name.LocalName;
                }
                else
                {
                    dt.TableName = tableName;
                }

                return dt;
            }

            return null;
        }

        public static int GetNumberOfRows(DataTable dt)
        {
            if (dt != null)
            {
                return dt.Rows.Count;
            }

            return 0;
        }

        public static int GetNumberOfColumns(DataTable dt)
        {
            if (dt != null)
            {
                return dt.Columns.Count;
            }

            return 0;
        }

        public static bool IsPopup(Page webPage)
        {
            if (webPage != null)
            {
                string isPopup = QueryString["IsPopup"];

                if (!MyUtils.IsEmpty(isPopup))
                {
                    return (isPopup.ToUpper() == "Y");
                }
            }

            return false;
        }

        public static bool IsQuitable(Page webPage)
        {
            if (webPage != null)
            {
                string isQuitable = QueryString["IsQuitable"];

                if (!MyUtils.IsEmpty(isQuitable))
                {
                    return (isQuitable.ToUpper() == "Y");
                }
            }

            return false;
        }

        public static bool IsTrueSQL(string SQL, DataRow drParams, int? appID = null)
        {
            if (!MyUtils.IsEmpty(SQL))
            {
                DataTable dt = GetBySQL(SQL, drParams, appID);

                if (MyWebUtils.GetNumberOfRows(dt) > 0 && MyWebUtils.GetNumberOfColumns(dt) > 0)
                {
                    return Convert.ToBoolean(MyUtils.Coalesce(dt.Rows[0][0], false));
                }

                return false;
            }

            return true;
        }

        public static DataTable GetBySQL(string SQL, DataRow drParams, int? appID = null)
        {
            return UiApplication.Me.GetBySQL(SQL, drParams, appID);
        }

        public static object EvalSQL(string SQL, DataRow drParams, int? appID = null)
        {
            if (!MyUtils.IsEmpty(SQL))
            {
                DataTable dt = GetBySQL(SQL, drParams, appID);

                if (MyWebUtils.GetNumberOfRows(dt) > 0 && MyWebUtils.GetNumberOfColumns(dt) > 0)
                {
                    return dt.Rows[0][0];
                }
            }

            return null;
        }

        public static ArrayList Merge(ArrayList list1, ArrayList list2)
        {
            ArrayList mergedList = null;

            if (list1 != null)
            {
                mergedList = list1;

                if (list2 != null)
                {
                    mergedList.AddRange(list2.ToArray().Where(c => !list1.ToArray().Contains(c)).ToList());
                }
            }

            return mergedList;
        }

        public static T Eval<T>(XmlElement expressionElement, DataRow drParams)
        {
            object result = Eval(expressionElement, drParams, typeof(T));

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public static object Eval(XmlElement expressionElement, DataRow drParams, Type type)
        {
            object result = null;

            if (!MyUtils.IsEmpty(expressionElement))
            {
                eLangType language = eLangType.literal;
                string expression = expressionElement.InnerText;

                string languageString = expressionElement.GetAttribute("lang");
                if (!MyUtils.IsEmpty(languageString))
                {
                    try
                    {
                        language = (eLangType)Enum.Parse(typeof(eLangType), languageString);
                    }
                    catch
                    {
                        // Do nothing
                    }
                }

                switch (language)
                {
                    case eLangType.literal:
                        result = expression;
                        break;

                    case eLangType.column:
                        string columnName = expression;
                        if (drParams != null && drParams.Table.Columns.Contains(columnName))
                        {
                            result = drParams[columnName];
                        }
                        else
                        {
                            result = null;
                        }
                        break;

                    case eLangType.sql:
                        if (!MyUtils.IsEmpty(expression))
                        {
                            DataTable dt = GetBySQL(expression, drParams);
                            if (type == typeof(DataTable))
                            {
                                result = dt;
                            }
                            else if (type == typeof(DataRow))
                            {
                                if (MyWebUtils.GetNumberOfRows(dt) > 0)
                                {
                                    result = dt.Rows[0];
                                }
                            }
                            else
                            {
                                if (MyWebUtils.GetNumberOfRows(dt) > 0 && MyWebUtils.GetNumberOfColumns(dt) > 0)
                                {
                                    result = dt.Rows[0][0];
                                }
                            }
                        }

                        break;

                    case eLangType.xml:
                        result = expressionElement;
                        break;
                }
            }

            if (MyUtils.IsEmpty(result))
            {
                if (type == typeof(bool))
                {
                    result = false;
                }
            }

            return result;
        }

        public static string GetSQLFromXml(XmlElement expressionElement)
        {
            if (!MyUtils.IsEmpty(expressionElement))
            {
                eLangType language = eLangType.literal;
                string expression = expressionElement.InnerText;

                string languageString = expressionElement.GetAttribute("lang");
                if (!MyUtils.IsEmpty(languageString))
                {
                    try
                    {
                        language = (eLangType)Enum.Parse(typeof(eLangType), languageString);
                    }
                    catch
                    {
                        // Do nothing
                    }
                }

                switch (language)
                {
                    case eLangType.sql:
                        return expression;
                }
            }

            return null;
        }

        public static bool IsTimeOutReached(Page page)
        {
            if (page != null && !Properties.Settings.Default.AirplaneMode)
            {
                string loginUrl = "~/Account/Login.aspx";
                HttpSessionState mySession = page.Session;
                int sessionCount = (mySession != null) ? mySession.Count : 0;
                HttpResponse myResponse = page.Response;
                IPrincipal myUser = page.User;

                if ((sessionCount == 0 && !page.IsCallback) && myResponse != null)
                {
                    myResponse.Redirect(loginUrl);
                }
                else if (sessionCount == 0 && page.IsCallback)
                {
                    FormsAuthentication.SignOut();
                    ASPxWebControl.RedirectOnCallback(loginUrl);
                }
                else if (myUser != null)
                {
                    IIdentity myIdentity = myUser.Identity;

                    if (myIdentity != null && !myIdentity.IsAuthenticated)
                    {
                        FormsAuthentication.SignOut();
                        FormsAuthentication.RedirectToLoginPage();
                    }
                    else if (myResponse != null && !IsUserAuthorised())
                    {
                        string authorisedApp = GetAuthorisedApp();
                        string errorMessage = string.Format("You are not authorised to access {0} and are being redirected to {1}", Application, authorisedApp);
                        string redirectUrl = "Default.aspx?Application=" + authorisedApp;

                        string script = string.Format("alert('{0}'); window.location.href = '{1}'", errorMessage, redirectUrl);

                        if (page != null && !page.ClientScript.IsClientScriptBlockRegistered("alert"))
                        {
                            page.ClientScript.RegisterClientScriptBlock(page.GetType(), "alert", script, true /* addScriptTags */);
                        }
                    }
                }
            }

            // All earlier redirects will exit before here ...
            return false;
        }

        public static bool IsUserAuthorised(string role = null)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Application").DefaultValue = Application;
            dt.Columns.Add("Role").DefaultValue = role;

            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            return MyWebUtils.IsTrueSQL("select dbo.fnIsAppOk(@CI_UserEmail, @Application, @Role)", dr, 0);
        }

        public static DataRow GetTableDetails(string tableName)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Application").DefaultValue = Application;
            dt.Columns.Add("Table").DefaultValue = tableName;

            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            DataTable dtResults = MyWebUtils.GetBySQL("?exec spTable_selByName @Application, @Table", dr, 0);
            if (MyWebUtils.GetNumberOfRows(dtResults) > 0)
            {
                return dtResults.Rows[0];
            }

            return null;
        }

        public static string GetAuthorisedApp()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Application").DefaultValue = Application;

            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            object sqlResult = MyWebUtils.EvalSQL("select dbo.fnGetAuthorisedApp(@CI_UserEmail, @Application)", dr, 0);

            return MyUtils.Coalesce(sqlResult, "").ToString();
        }

        public static XmlElement CreateXmlElement(string name, object value)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement elem = doc.CreateElement(name);
            elem.InnerText = MyUtils.Coalesce(value, "").ToString();

            return elem;
        }

        public static string GetXmlChildValue(XElement xElement, string childName)
        {
            if (xElement != null)
            {
                XElement xChildElement = xElement.Element(childName);
                if (xChildElement != null)
                {
                    return MyUtils.Coalesce(xChildElement.Value, "");
                }
            }

            return "";
        }

        public static void AddColumnIfRequired(DataColumnCollection dc, string columnName)
        {
            if (dc != null && !dc.Contains(columnName))
            {
                dc.Add(columnName);
            }
        }

        public static DataRow CreateDataRow()
        {
            DataTable dt = new DataTable();
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            return dr;
        }

        public static void ShowAlert(Page page, string message, string webFileName = null)
        {
            string script = string.Format("alert('{0}')",
                message);

            if (MyUtils.IsEmpty(script))
            {
                script += string.Format("; window.location.href = '{0}'",
                    webFileName);
            }

            if (page != null && !page.ClientScript.IsClientScriptBlockRegistered("alert"))
            {
                page.ClientScript.RegisterClientScriptBlock(page.GetType(), "alert", script, true);
            }
        }

        public static string GetApplicationName(DataRow drAppKey)
        {
            DataTable dt = MyWebUtils.GetBySQL("?exec spApplication_sel @AppID", drAppKey, 0);

            if (dt != null && dt.Columns.Contains("AppName") && MyWebUtils.GetNumberOfRows(dt) > 0)
            {
                object objAppName = dt.Rows[0]["AppName"];

                if (objAppName != null)
                {
                    return objAppName.ToString();
                }
            }

            return null;
        }

        public static string GetDatabaseName(DataRow drAppKey)
        {
            DataTable dt = MyWebUtils.GetBySQL("?exec spApplication_sel @AppID", drAppKey, 0);

            if (dt != null && dt.Columns.Contains("SQLDatabaseName") && MyWebUtils.GetNumberOfRows(dt) > 0)
            {
                object objAppName = dt.Rows[0]["SQLDatabaseName"];

                if (objAppName != null)
                {
                    return objAppName.ToString();
                }
            }

            return null;
        }

        public static Color ToColor(string color)
        {
            byte a = 255;

            byte r = (byte)(Convert.ToUInt32(color.Substring(1, 2), 16));
            byte g = (byte)(Convert.ToUInt32(color.Substring(3, 2), 16));
            byte b = (byte)(Convert.ToUInt32(color.Substring(5, 2), 16));

            return Color.FromArgb(a, r, g, b);
        }

        public static string GetFilePath(string folderName, UploadedFile uploadedFile)
        {
            string fileName = CleanFileName(uploadedFile.FileName);
            string filePath = folderName + @"\" + fileName;

            return filePath;
        }

        public static string UploadToAzureFileStorage(string folderName, UploadedFile uploadedFile)
        {
            string fileName = CleanFileName(uploadedFile.FileName);
            ShareFileClient azureFile = GetAzureFile(folderName, fileName);
            string linkURL = GetFileSasUri(azureFile.Path, DateTime.Now.AddDays(1), ShareFileSasPermissions.Read);

            Task task = new Task(() => { AppendToAzureFile(azureFile, uploadedFile.FileBytes); });
            task.Start();

            return linkURL;
        }

        public static string UploadToWebServer(string folderName, UploadedFile uploadedFile)
        {
            string fileName = CleanFileName(uploadedFile.FileName);
            string filePath = folderName + @"\" + fileName;
            string linkURL = filePath;

            string folderPath = MapPath(folderName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            uploadedFile.SaveAs(MapPath(filePath));

            return linkURL;
        }

        public static string CleanFileName(string filename)
        {
            if (!MyUtils.IsEmpty(filename))
            {
                return filename.Replace(" ", "_");
            }

            return "";
        }

        public static ShareFileClient GetAzureFile(string folderName, string fileName)
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

        public static void AppendToAzureFile(ShareFileClient azureFile, byte[] buffer)
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

        public static string GetFileSasUri(string filePath, DateTime expiration, ShareFileSasPermissions permissions)
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

    public class DataSetSerializer : IDataSerializer
    {
        public const string Name = "DataSetSerializer";

        public bool CanSerialize(object data, object extensionProvider)
        {
            return (data is DataSet);
        }

        public string Serialize(object data, object extensionProvider)
        {
            if (data is DataSet)
            {
                DataSet ds = data as DataSet;
                StringBuilder sb = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(sb);
                ds.WriteXml(writer, XmlWriteMode.WriteSchema);

                return sb.ToString();
            }

            return string.Empty;
        }

        public bool CanDeserialize(string value, string typeName, object extensionProvider)
        {
            return typeName == "System.Data.DataSet";
        }

        public object Deserialize(string value, string typeName, object extensionProvider)
        {
            DataSet ds = new DataSet();

            using (XmlReader reader = XmlReader.Create(new StringReader(value)))
            {
                ds.ReadXml(reader, XmlReadMode.ReadSchema);
            }

            return ds;
        }
    }
}