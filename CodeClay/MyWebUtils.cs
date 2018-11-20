using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;

// Extra references
using CodistriCore;
using DevExpress.Web;
using DevExpress.XtraReports.Native;

namespace CodeClay
{
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

    public enum eTextMask
    {
        None = 0,
        Currency,
        Integer
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

        private static NameValueCollection mQueryString = new NameValueCollection();

        public static NameValueCollection QueryString
        {
            get
            {
                return mQueryString;
            }

            set
            {
                mQueryString = value;
            }
    }

        public static string Application
        {
            get
            {
                string application = "";

                if (UiApplication.Me != null)
                {
                    application = UiApplication.Me.CiApplication.AppName;
                }

                return MyUtils.Coalesce(application,
                    QueryString["Application"],
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

        public static bool IsTrueSQL(string SQL, DataRow drParams)
        {
            if (!MyUtils.IsEmpty(SQL))
            {
                DataTable dt = GetBySQL(SQL, drParams);

                if (MyWebUtils.GetNumberOfRows(dt) > 0 && MyWebUtils.GetNumberOfColumns(dt) > 0)
                {
                    return Convert.ToBoolean(MyUtils.Coalesce(dt.Rows[0][0], false));
                }

                return false;
            }

            return true;
        }

        public static DataTable GetBySQL(string SQL, DataRow drParams)
        {
            return UiApplication.Me.GetBySQL(SQL, drParams);
        }

        public static object EvalSQL(string SQL, DataRow drParams)
        {
            if (!MyUtils.IsEmpty(SQL))
            {
                DataTable dt = GetBySQL(SQL, drParams);

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
                            if (typeof(T) == typeof(DataTable))
                            {
                                result = dt;
                            }
                            else if (typeof(T) == typeof(DataRow))
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

            if (result == null)
            {
                if (typeof(T) == typeof(bool))
                {
                    result = false;
                }
            }

            return (T)Convert.ChangeType(result, typeof(T));
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

                if (page.Session.Count == 0 && !page.IsCallback || !IsUserAuthorised() || UiApplication.Me == null)
                {
                    page.Response.Redirect(loginUrl);
                }
                else if (page.Session.Count == 0 && page.IsCallback)
                {
                    FormsAuthentication.SignOut();
                    ASPxWebControl.RedirectOnCallback(loginUrl);
                }
                else if (!page.User.Identity.IsAuthenticated)
                {
                    FormsAuthentication.SignOut();
                    FormsAuthentication.RedirectToLoginPage();
                }
            }

            // All earlier redirects will exit before here ...
            return false;
        }

        public static bool IsUserAuthorised()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Application").DefaultValue = Application;

            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            return MyWebUtils.IsTrueSQL("select dbo.fnIsAppOk(@CI_UserEmail, @Application)", dr);
        }

        public static XmlElement CreateXmlElement(string name, object value)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement elem = doc.CreateElement(name);
            elem.InnerText = MyUtils.Coalesce(value, "").ToString();

            return elem;
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