using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    public class CiPlugin : MarshalByRefObject
    {
        // --------------------------------------------------------------------------------------------------
        // Constants
        // --------------------------------------------------------------------------------------------------

        public const string LIST_SEPARATOR = "|";
        public const string PARAMETER_PREFIX = "P_";

        // --------------------------------------------------------------------------------------------------
        // Member variables
        // --------------------------------------------------------------------------------------------------

        private string mSrc = "";
        private ArrayList mCiPlugins = new ArrayList();
        private string mLayoutUrl = "";
        private XmlDocument mLayoutXml = new XmlDocument();
        private string mDefaultValue = "";

        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlAttribute("src")]
        public string Src
        {
            get { return mSrc; }

            set
            {
                mSrc = value;
                SrcParams = new Hashtable();

                if (!MyUtils.IsEmpty(mSrc) && mSrc.Contains("?"))
                {
                    string[] tokens = mSrc.Split('?');
                    mSrc = tokens[0];
                    string nameValueList = tokens[1];
                    if (!MyUtils.IsEmpty(nameValueList))
                    {
                        foreach (string nameValuePair in nameValueList.Split('&'))
                        {
                            string[] nvpTokens = nameValuePair.Split('=');
                            if (nvpTokens.Length == 2)
                            {
                                SrcParams.Add(nvpTokens[0], nvpTokens[1]);
                            }
                        }
                    }
                }

                CiPlugin ciSrcPlugin = CiPlugin.CreateCiPlugin(mSrc);
                this.Inherits(ciSrcPlugin);

                foreach (string parameterName in SrcParams.Keys)
                {
                    CiPlugin ciChildPlugin = GetById(parameterName);
                    if (ciChildPlugin != null)
                    {
                        object parameterValue = SrcParams[parameterName];
                        string strParameterValue = MyUtils.Coalesce(parameterValue, "").ToString();
                        if (!strParameterValue.StartsWith("@"))
                        {
                            ciChildPlugin.Value = strParameterValue;
                        }
                    }
                }
            }
        }

        [XmlSqlElement("Hidden", typeof(bool))]
        public XmlElement Hidden { get; set; } = MyWebUtils.CreateXmlElement("Hidden", false);

        [XmlElement("Searchable")]
        public bool Searchable { get; set; } = false;

        [XmlElement("Comment")]
        public string Comment { get; set; } = "";

        [XmlElement("RowKey")]
        public string RowKey { get; set; } = "";

        [XmlElement("Enabled")]
        public bool Enabled { get; set; } = true;

        [XmlElement("LayoutUrl")]
        public string LayoutUrl
        {
            get
            {
                return mLayoutUrl;
            }

            set
            {
                mLayoutUrl = value;
                if (!MyUtils.IsEmpty(mLayoutUrl))
                {
                    string layoutPath = MyWebUtils.MapPath(MyWebUtils.ApplicationFolder + @"\" + mLayoutUrl);

                    XmlDocument tableXml = new XmlDocument();
                    tableXml.Load(layoutPath);

                    foreach (XmlNode table in tableXml.GetElementsByTagName("table"))
                    {
                        XmlAttribute cssClass = tableXml.CreateAttribute("class");
                        cssClass.Value = "CodeClay";
                        table.Attributes.Append(cssClass);

                        XmlAttribute runat = tableXml.CreateAttribute("runat");
                        runat.Value = "server";
                        table.Attributes.Append(runat);
                    }

                    foreach (XmlNode cell in tableXml.GetElementsByTagName("td"))
                    {
                        XmlAttribute cssClass = tableXml.CreateAttribute("class");
                        cssClass.Value = "CodeClay";
                        cell.Attributes.Append(cssClass);
                    }

                    XmlElement html = mLayoutXml.CreateElement("html");
                    mLayoutXml.AppendChild(html);

                    XmlElement head = mLayoutXml.CreateElement("head");
                    html.AppendChild(head);

                    XmlElement link = mLayoutXml.CreateElement("link");
                    head.AppendChild(link);

                    XmlAttribute rel = mLayoutXml.CreateAttribute("rel");
                    rel.Value = "stylesheet";
                    link.Attributes.Append(rel);

                    XmlAttribute type = mLayoutXml.CreateAttribute("type");
                    type.Value = "text/css";
                    link.Attributes.Append(type);

                    XmlAttribute href = mLayoutXml.CreateAttribute("href");
                    href.Value = "CodeClay.css";
                    link.Attributes.Append(href);

                    XmlElement body = mLayoutXml.CreateElement("body");
                    html.AppendChild(body);

                    body.InnerXml = tableXml.InnerXml;
                }
            }
        }

        [XmlAnyElement("Value")]
        public XmlElement DefaultXml
        {
            get
            {
                if (!MyUtils.IsEmpty(mDefaultValue))
                {
                    var x = new XmlDocument();
                    x.LoadXml(string.Format("<Value>{0}</Value>", mDefaultValue));
                    return x.DocumentElement;
                }

                return null;
            }

            set
            {
                if (value != null)
                {
                    mDefaultValue = value.InnerXml;
                }
                else
                {
                    mDefaultValue = "";
                }
            }
        }

        [XmlElement("CiButtonField", typeof(CiButtonField))]
        [XmlElement("CiCheckField", typeof(CiCheckField))]
        [XmlElement("CiComboField", typeof(CiComboField))]
        [XmlElement("CiCurrencyField", typeof(CiCurrencyField))]
        [XmlElement("CiDateField", typeof(CiDateField))]
        [XmlElement("CiField", typeof(CiField))]
        [XmlElement("CiImageField", typeof(CiImageField))]
        [XmlElement("CiLinkField", typeof(CiLinkField))]
        [XmlElement("CiFieldExitMacro", typeof(CiFieldExitMacro))]
        [XmlElement("CiMacro", typeof(CiMacro))]
        [XmlElement("CiMemoField", typeof(CiMemoField))]
        [XmlElement("CiMenu", typeof(CiMenu))]
        [XmlElement("CiNumericField", typeof(CiNumericField))]
        [XmlElement("CiRadioField", typeof(CiRadioField))]
        [XmlElement("CiReport", typeof(CiReport))]
        [XmlElement("CiSerializeMacro", typeof(CiSerializeMacro))]
        [XmlElement("CiTable", typeof(CiTable))]
        [XmlElement("CiPivotTable", typeof(CiPivotTable))]
        [XmlElement("CiPivotField", typeof(CiPivotField))]
        [XmlElement("CiTextField", typeof(CiTextField))]
        [XmlElement("CiTimeField", typeof(CiTimeField))]
        public CiPlugin[] CiPlugins
        {
            get
            {
                CiPlugin[] ciPlugins = (CiPlugin[])mCiPlugins.ToArray(typeof(CiPlugin));
                return ciPlugins;
            }
            set
            {
                if (value != null)
                {
                    CiPlugin[] ciNewPlugins = (CiPlugin[])value;
                    foreach (CiPlugin ciNewPlugin in ciNewPlugins)
                    {
                        CiPlugin ciOldPlugin = null;

                        string pluginID = ciNewPlugin.ID;
                        if (!MyUtils.IsEmpty(pluginID))
                        {
                            ciOldPlugin = GetById(pluginID);
                        }

                        if (ciOldPlugin != null)
                        {
                            int oldIndex = mCiPlugins.IndexOf(ciOldPlugin);
                            mCiPlugins.Remove(ciOldPlugin);
                            mCiPlugins.Insert(oldIndex, ciNewPlugin);
                        }
                        else
                        {
                            mCiPlugins.Add(ciNewPlugin);
                        }

                        ciNewPlugin.CiParentPlugin = this;
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Properties (Derived)
        // --------------------------------------------------------------------------------------------------

        [XmlIgnore]
        public virtual string ID { get; set; } = null;

        [XmlIgnore]
        public string SearchableID
        {
            get { return PARAMETER_PREFIX + ID; }
        }

        [XmlIgnore]
        public virtual bool IsSearching
        {
            get
            {
                return MyUtils.Coalesce(MyWebUtils.QueryStringCommand,
                    UiApplication.Me.GetCommandFired(ID)) == "Search";
            }
        }

        [XmlIgnore]
        public Hashtable SrcParams { get; set; } = new Hashtable();

        [XmlIgnore]
        public string PuxFile { get; set; } = "";

        [XmlIgnore]
        public string Value
        {
            get { return mDefaultValue; }
            set { mDefaultValue = value; }
        }

        [XmlIgnore]
        public string[] RowKeyNames
        {
            get
            {
                if (!MyUtils.IsEmpty(RowKey))
                {
                    return RowKey.Split(',');
                }

                return new string[] { };
            }
        }

        [XmlIgnore]
        public XmlElement LayoutXml
        {
            get
            {
                return mLayoutXml.DocumentElement;
            }
        }

        [XmlIgnore]
        public CiPlugin CiParentPlugin { get; set; }

        [XmlIgnore]
        public XiPlugin XiPlugin { get; set; }

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual bool IsHidden(DataRow drParams)
        {
            return MyWebUtils.Eval<bool>(Hidden, drParams);
        }

        public virtual string GetUiPluginName()
        {
            string ciPluginName = this.GetType().Name;

            return "Ui" + ciPluginName.Substring(2);
        }

        public virtual bool IsInstanceOf(Type type)
        {
            Type pluginType = GetType();

            return (pluginType == type || pluginType.IsSubclassOf(type));
        }

        public virtual void Inherits(CiPlugin ciSrcPlugin)
        {
            if (ciSrcPlugin != null)
            {
                foreach (PropertyInfo p in ciSrcPlugin.GetType().GetProperties())
                {
                    if (!("Src, CiPlugins".Contains(p.Name)))
                    {
                        Attribute a = MyUtils.Coalesce(p.GetCustomAttribute(typeof(XmlElementAttribute)),
                          p.GetCustomAttribute(typeof(XmlAttributeAttribute)),
                          p.GetCustomAttribute(typeof(XmlAnyElementAttribute))) as Attribute;

                        if (a != null)
                        {
                            p.SetValue(this, p.GetValue(ciSrcPlugin));
                        }
                    }
                }

                PuxFile = ciSrcPlugin.PuxFile;
                CiPlugins = ciSrcPlugin.CiPlugins;
            }
        }

        public static CiPlugin CreateCiPlugin(string pux, bool isFile = true)
        {
            if (isFile)
            {
                string puxContents = "";

                if (pux.ToLower().EndsWith(".pux"))
                {
                    string puxPath = MyWebUtils.MapPath(MyWebUtils.ApplicationFolder + @"\" + pux);

                    if (File.Exists(puxPath))
                    {
                        puxContents = File.ReadAllText(puxPath);
                    }
                }
                else
                {
                    puxContents = pux;
                }

                CiPlugin ciPlugin = CreateCiPlugin(puxContents, false);
                if (ciPlugin != null)
                {
                    ciPlugin.PuxFile = pux;
                }

                return ciPlugin;
            }
            else
            {
                string puxContents = pux;

                if (!puxContents.StartsWith("<"))
                {
                    puxContents = string.Format("<{0} />", puxContents);
                }

                XElement pluginXML = XElement.Parse(puxContents);
                string ciPluginName = pluginXML.Name.ToString();
                Type pluginType = Type.GetType("CodeClay." + ciPluginName);

                TextReader reader = new StringReader(puxContents);
                if (reader != null && pluginType != null)
                {
                    XmlSerializer serializer = new XmlSerializer(pluginType);
                    if (serializer != null)
                    {
                        try
                        {
                            CiPlugin ciPlugin = serializer.Deserialize(reader) as CiPlugin;

                            return ciPlugin;
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }
                }
            }

            return null;
        }

        public virtual UiPlugin CreateUiPlugin(Page webPage)
        {
            string uiPluginName = GetUiPluginName();

            if (webPage != null)
            {
                string userControlFile = uiPluginName + ".ascx";
                if (!MyUtils.IsEmpty(userControlFile) && File.Exists(MyWebUtils.MapPath(userControlFile)))
                {
                    UiPlugin uiPlugin = webPage.LoadControl(userControlFile) as UiPlugin;
                    if (uiPlugin != null)
                    {
                        uiPlugin.ID = ID;
                        uiPlugin.CiPlugin = this;

                        return uiPlugin;
                    }
                }
            }

            return null;
        }

        public virtual T[] Get<T>() where T: CiPlugin
        {
            if (CiPlugins != null)
            {
                return CiPlugins
                   .Where(c => c.IsInstanceOf(typeof(T)))
                   .Select(c => c as T)
                   .ToArray<T>();
            }

            return default(T[]);
        }

        public virtual CiPlugin GetById(string id)
        {
            if (CiPlugins != null)
            {
                CiPlugin[] PluginsFound = CiPlugins.Where(c => c.ID == id).ToArray();
                if (PluginsFound.Length > 0)
                {
                    return PluginsFound[0];
                }
            }

            return null;
        }

        public virtual CiPlugin GetContainerPlugin()
        {
            return null;
        }

        public virtual void Add(CiPlugin ciPlugin)
        {
            if (mCiPlugins != null)
            {
                mCiPlugins.Add(ciPlugin);
            }
        }

        public virtual Type GetNativeType(object pluginValue)
        {
            return (!MyUtils.IsEmpty(pluginValue)) ? pluginValue.GetType() : typeof(string);
        }

        public virtual object GetNativeValue(object pluginValue)
        {
            return !MyUtils.IsEmpty(pluginValue) ? pluginValue : Convert.DBNull;
        }
    }

    [ParseChildren(false)]
    public class UiPlugin : UserControl
    {
        // --------------------------------------------------------------------------------------------------
        // Constants
        // --------------------------------------------------------------------------------------------------

        public const string LIST_SEPARATOR = "|";
        public char[] WHITESPACE = new char[] { ' ', '\t', '\n', '\r' };

        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public virtual CiPlugin CiPlugin { get; set; } = null;

        public UiPlugin UiParentPlugin { get; set; } = null;

        public virtual bool IsInserting { get; set; } = false;

        public virtual bool IsInsertSaving { get; set; } = false;

        public virtual bool IsEditing { get; set; } = false;

        public virtual bool IsEditSaving { get; set; } = false;

        public virtual bool IsFieldExiting { get; set; } = false;

        public virtual bool IsSearchSaving { get; set; } = false;

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected virtual void Page_PreInit(object sender, EventArgs e)
        {
            // Do nothing
        }

        protected virtual void Page_Load(object sender, EventArgs e)
        {
            // Do nothing
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual void ConfigureIn(Control container)
        {
            // Do nothing
        }

        public virtual int GetFocusedIndex()
        {
            return -1;
        }

        public virtual bool InContainer()
        {
            CiPlugin ciParentPlugin = CiPlugin.CiParentPlugin;
            bool hasLayout = (ciParentPlugin != null) && (ciParentPlugin.LayoutXml != null);

            return Parent.GetType().IsSubclassOf(typeof(TemplateContainerBase)) || hasLayout;
        }

        public virtual string GetFormattedKey(string key)
        {
            return key;
        }

        public virtual bool IsInstanceOf(Type type)
        {
            Type pluginType = GetType();

            return (pluginType == type || pluginType.IsSubclassOf(type));
        }

        public virtual string FormatEvent(string handlerName, string otherScript = "")
        {
            return string.Format("function(s,e) {{ {0}(s,e); {1}}}", handlerName, otherScript);
        }

        public virtual PluginTemplate CreateTemplate(CiPlugin ciPlugin)
        {
            return new PluginTemplate(this, ciPlugin);
        }

        public virtual ITemplate CreateLayout(bool enabled)
        {
            return new LayoutTemplate(this, enabled);
        }

        public virtual UiPlugin CreateUiPlugin(CiPlugin ciPlugin)
        {
            UiPlugin uiPlugin = null;

            if (ciPlugin != null)
            {
                uiPlugin = ciPlugin.CreateUiPlugin(this.Page);
                if (uiPlugin != null)
                {
                    uiPlugin.UiParentPlugin = this;
                }
            }

            return uiPlugin;
        }

        public virtual object this[string key, int rowIndex = -1]
        {
            get
            {
                if (rowIndex < 0)
                {
                    rowIndex = GetFocusedIndex();
                }

                return MyUtils.Coalesce(
                    GetEditorValue(key, rowIndex),
                    GetSrcParamsValue(key),
                    GetQueryStringValue(key),
                    GetParentValue(key),
                    GetDefaultValue(key));
            }

            set
            {
                SetClientValue(key, value);
            }
        }

        public virtual DataRow GetState(int rowIndex = -1)
        {
            DataRow dr = GetEditorState(rowIndex);
            dr = MyUtils.AppendColumns(dr, GetSrcParamsState());
            dr = MyUtils.AppendColumns(dr, GetQueryStringState());
            dr = MyUtils.AppendColumns(dr, GetParentState());
            dr = MyUtils.AppendColumns(dr, GetDefaultState());

            return dr;
        }

        // --------------------------------------------------------------------------------------------------
        // Methods (override)
        // --------------------------------------------------------------------------------------------------

        protected override void AddParsedSubObject(object content)
        {
            LiteralControl literal = content as LiteralControl;
            if (literal != null)
            {
                string textContent = MyUtils.Coalesce(literal.Text, "").ToString().Trim();
                if (textContent.StartsWith("<Ci"))
                {
                    CiPlugin = CiPlugin.CreateCiPlugin(textContent, false);
                }
            }

            if (CiPlugin == null)
            {
                base.AddParsedSubObject(content);
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        protected virtual object GetEditorValue(string key, int rowIndex = -1)
        {
            bool isSearching = !MyUtils.IsEmpty(key) && key.StartsWith(CiPlugin.PARAMETER_PREFIX);

            object editorValue = GetServerValue(key, rowIndex);

            if (IsInserting || IsInsertSaving || IsEditSaving || IsFieldExiting || isSearching)
            {
                editorValue = GetClientValue(key);
            }

            if (CiPlugin != null)
            {
                editorValue = CiPlugin.GetNativeValue(editorValue);
            }

            return editorValue;
        }

        protected virtual object GetClientValue(string key)
        {
            return UiApplication.Me[GetFormattedKey(key)];
        }

        protected virtual void SetClientValue(string key, object value)
        {
            UiApplication.Me[GetFormattedKey(key)] = value;
        }

        protected virtual object GetServerValue(string key, int rowIndex = -1)
        {
            GridBaseTemplateContainer container = this.NamingContainer as GridBaseTemplateContainer;
            if (container != null)
            {
                try
                {
                    return DataBinder.Eval(container.DataItem, key);
                }
                catch
                {
                    // Do nothing
                }
            }

            return null;
        }

        protected virtual object GetSrcParamsValue(string key)
        {
            if (CiPlugin != null)
            {
                Hashtable srcParams = CiPlugin.SrcParams;
                if (srcParams != null && srcParams.ContainsKey(key))
                {
                    object value = srcParams[key];
                    string strValue = MyUtils.Coalesce(value, "").ToString();

                    if (strValue.StartsWith("@"))
                    {
                        key = strValue.Substring(1);
                        value = UiParentPlugin != null ? GetParentValue(key) : this[key];
                    }

                    return value;
                }
            }

            return null;
        }

        protected virtual object GetQueryStringValue(string key)
        {
            string prefix = ""; // CiPlugin.PARAMETER_PREFIX;
            object queryStringValue = MyWebUtils.QueryString[key.StartsWith(prefix) ? key.Substring(prefix.Length) : key];

            if (!MyUtils.IsEmpty(queryStringValue))
            {
                return queryStringValue;
            }

            return null;
        }

        protected virtual object GetParentValue(string key)
        {
            if (CiPlugin != null)
            {
                string[] rowKeyNames = CiPlugin.RowKeyNames;
                CiPlugin ciParentPlugin = CiPlugin.CiParentPlugin;

                if (rowKeyNames != null && ciParentPlugin != null)
                {
                    rowKeyNames = rowKeyNames.Union(ciParentPlugin.RowKeyNames).ToArray();

                    if (rowKeyNames.Contains(key))
                    {
                        object parentValue = (UiParentPlugin != null)
                        ? UiParentPlugin.GetServerValue(key)
                        : null;

                        if (!MyUtils.IsEmpty(parentValue))
                        {
                            return parentValue;
                        }
                    }
                }
            }

            return null;
        }

        public virtual object GetDefaultValue(string key)
        {
            CiPlugin ciChildPlugin = CiPlugin.GetContainerPlugin().GetById(key);

            if (ciChildPlugin != null)
            {
                object value = ciChildPlugin.Value;

                return ciChildPlugin.GetNativeValue(value);
            }

            return null;
        }

        protected virtual DataRow GetEditorState(int rowIndex = -1)
        {
            DataRow dr = MyWebUtils.CreateDataRow();

            if (dr != null && CiPlugin != null)
            {
                DataColumnCollection dc = dr.Table.Columns;

                CiPlugin ciContainerPlugin = CiPlugin.GetContainerPlugin();
                if (ciContainerPlugin != null)
                {
                    foreach (CiPlugin ciChildPlugin in ciContainerPlugin.CiPlugins)
                    {
                        string key = ciChildPlugin.ID;

                        if (!MyUtils.IsEmpty(key))
                        {
                            object value = GetEditorValue(key, rowIndex);
                            Type valueType = ciChildPlugin.GetNativeType(value);

                            dc.Add(key, valueType);
                            dr[key] = ciChildPlugin.GetNativeValue(value);

                            if (ciChildPlugin.Searchable)
                            {
                                string searchKey = ciChildPlugin.SearchableID;

                                value = GetEditorValue(searchKey, rowIndex);
                                valueType = ciChildPlugin.GetNativeType(value);

                                dc.Add(searchKey, valueType);
                                dr[searchKey] = ciChildPlugin.GetNativeValue(value);
                            }
                        }
                    }
                }
            }

            return dr;
        }

        protected virtual DataRow GetSrcParamsState()
        {
            DataRow dr = MyWebUtils.CreateDataRow();

            if (dr != null && CiPlugin != null)
            {
                DataColumnCollection dc = dr.Table.Columns;

                Hashtable srcParams = CiPlugin.SrcParams;
                if (srcParams != null)
                {
                    foreach (var item in srcParams.Keys)
                    {
                        string key = item as string;
                        if (!MyUtils.IsEmpty(key))
                        {
                            object value = GetSrcParamsValue(key);
                            dc.Add(key);
                            dr[key] = value;
                        }
                    }
                }
            }

            return dr;
        }

        protected virtual DataRow GetQueryStringState()
        {
            DataRow dr = MyWebUtils.CreateDataRow();

            if (dr != null)
            {
                DataColumnCollection dc = dr.Table.Columns;
                foreach (string key in MyWebUtils.QueryString.Keys)
                {
                    object value = MyWebUtils.QueryString[key];

                    if (!MyUtils.IsEmpty(key))
                    {
                        dc.Add(key);
                        dr[key] = value;
                    }
                }
            }

            return dr;
        }

        protected virtual DataRow GetParentState()
        {
            DataRow dr = MyWebUtils.CreateDataRow();

            if (dr != null && CiPlugin != null)
            {
                DataColumnCollection dc = dr.Table.Columns;
                string[] rowKeyNames = CiPlugin.RowKeyNames;
                CiPlugin ciParentPlugin = CiPlugin.CiParentPlugin;

                if (rowKeyNames != null && ciParentPlugin != null)
                {
                    rowKeyNames = rowKeyNames.Union(ciParentPlugin.RowKeyNames).ToArray();

                    foreach (string key in rowKeyNames)
                    {
                        object value = (UiParentPlugin != null)
                        ? MyUtils.Coalesce(UiParentPlugin.GetClientValue(key), UiParentPlugin.GetServerValue(key))
                        : null;

                        if (!MyUtils.IsEmpty(key))
                        {
                            CiPlugin ciChildPlugin = CiPlugin.GetById(key);

                            if (ciChildPlugin != null)
                            {
                                value = ciChildPlugin.GetNativeValue(value);
                                Type valueType = ciChildPlugin.GetNativeType(value);

                                dc.Add(key, valueType);
                                dr[key] = value;

                                if (ciChildPlugin.Searchable)
                                {
                                    string searchKey = ciChildPlugin.SearchableID;

                                    dc.Add(searchKey, valueType);
                                    dr[searchKey] = value;
                                }
                            }
                        }
                    }
                }
            }

            return dr;
        }

        protected virtual DataRow GetDefaultState()
        {
            DataRow dr = MyWebUtils.CreateDataRow();

            if (dr != null && CiPlugin != null)
            {
                DataColumnCollection dc = dr.Table.Columns;
                CiPlugin ciContainerPlugin = CiPlugin.GetContainerPlugin();

                if (ciContainerPlugin != null)
                {
                    foreach (CiPlugin ciChildPlugin in ciContainerPlugin.CiPlugins)
                    {
                        if (ciChildPlugin != null)
                        {
                            string key = ciChildPlugin.ID;
                            object value = ciChildPlugin.Value;
                            if (!MyUtils.IsEmpty(key))
                            {
                                dc.Add(key, ciChildPlugin.GetNativeType(value));
                                dr[key] = ciChildPlugin.GetNativeValue(value);
                            }
                        }
                    }
                }
            }

            return dr;
        }
    }

    public class PluginTemplate : ITemplate
    {
        // --------------------------------------------------------------------------------------------------
        // Member Variables
        // --------------------------------------------------------------------------------------------------

        private UiPlugin mUiParentPlugin = null;
        private CiPlugin mCiPlugin = null;

        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        public PluginTemplate(UiPlugin uiParentPlugin, CiPlugin CiPlugin)
        {
            mUiParentPlugin = uiParentPlugin;
            mCiPlugin = CiPlugin;
        }

        // --------------------------------------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------------------------------------

        public void InstantiateIn(Control container)
        {
            // Required when implementing ITemplate interface
            if (container != null && mUiParentPlugin != null)
            {
                UiPlugin uiPlugin = mUiParentPlugin.CreateUiPlugin(mCiPlugin);
                if (uiPlugin != null)
                {
                    uiPlugin.ConfigureIn(container);
                    container.Controls.Add(uiPlugin);
                }
            }
        }
    }

    public class LayoutTemplate: ITemplate
    {
        // --------------------------------------------------------------------------------------------------
        // Member Variables
        // --------------------------------------------------------------------------------------------------

        private UiPlugin mUiPlugin = null;

        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public UiPlugin UiPlugin
        {
            get
            {
                return mUiPlugin;
            }
        }

        public CiPlugin CiPlugin
        {
            get
            {
                if (mUiPlugin != null)
                {
                    return mUiPlugin.CiPlugin;
                }

                return null;
            }
        }

        public bool Enabled { get; set; }

        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        public LayoutTemplate(UiPlugin uiPlugin, bool enabled)
        {
            mUiPlugin = uiPlugin;
            Enabled = enabled;
        }

        // --------------------------------------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------------------------------------

        public void InstantiateIn(Control container)
        {
            // Required when implementing ITemplate interface
            if (CiPlugin != null)
            {
                XmlElement layoutXml = CiPlugin.LayoutXml;
                if (layoutXml != null)
                {
                    Control control = UiPlugin.ParseControl(layoutXml.OuterXml);
                    container.Controls.Add(control);

                    FillWithPlugins(container, control);
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private void FillWithPlugins(Control container, Control control)
        {
            if (control != null && CiPlugin != null)
            {
                foreach (Control childControl in control.Controls)
                {
                    FillWithPlugins(container, childControl);
                }

                Type controlType = control.GetType();
                if (controlType == typeof(HtmlTableCell))
                {
                    string id = control.ID;
                    if (!MyUtils.IsEmpty(id))
                    {
                        CiPlugin ciChildPlugin = CiPlugin.GetById(id);
                        if (ciChildPlugin != null)
                        {
                            ciChildPlugin.Enabled = Enabled;

                            UiPlugin uiPlugin = ciChildPlugin.CreateUiPlugin(UiPlugin.Page);
                            uiPlugin.UiParentPlugin = UiPlugin;
                            uiPlugin.ConfigureIn(container);
                            uiPlugin.ID = "ui" + id;

                            control.Controls.Add(uiPlugin);
                        }
                    }
                }
            }
        }
    }

    public class XiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        protected Type PluginType { get; set; } = null;

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

        public virtual void DownloadFile(DataRow drKey, string puxUrl)
        {
            XElement xPlugin = new XElement("Root");

            DownloadToXml(drKey, xPlugin);

            if (!MyUtils.IsEmpty(puxUrl))
            {
                if (!File.Exists(puxUrl))
                {
                    FileInfo fi = new FileInfo(puxUrl);
                    fi.Directory.Create();
                }

                xPlugin.Save(puxUrl);
            }
        }

        public virtual void UploadFile(DataRow drKey, string puxUrl)
        {
            if (!MyUtils.IsEmpty(puxUrl) && File.Exists(puxUrl))
            {
                XElement xPlugin = XElement.Load(puxUrl);
                UploadFromXml(drKey, new List<XElement>() { xPlugin });
            }
        }

        public virtual void DeleteFile(DataRow drKey)
        {
            DataTable dtPluginDefinitions = GetPluginDefinitions(drKey);
            if (dtPluginDefinitions != null)
            {
                if (dtPluginDefinitions.Rows.Count > 0)
                {
                    DataRow drPluginDefinition = dtPluginDefinitions.Rows[0];
                }
            }

            string puxUrl = "";
            if (!MyUtils.IsEmpty(puxUrl))
            {
                if (File.Exists(puxUrl))
                {
                    File.Delete(puxUrl);
                }
            }
        }

        public virtual void DownloadToXml(DataRow drKey, XElement xPluginDefinition)
        {
            XElement xSavedPluginDefinition = xPluginDefinition;

            DataTable dtPluginDefinitions = GetPluginDefinitions(drKey);
            if (dtPluginDefinitions != null)
            {
                foreach (DataRow drPluginDefinition in dtPluginDefinitions.Rows)
                {
                    string pluginTypeName = GetPluginTypeName(drPluginDefinition);
                    if (!MyUtils.IsEmpty(pluginTypeName))
                    {
                        Type pluginType = GetPluginType(pluginTypeName);

                        if (xPluginDefinition.Name == "Root")
                        {
                            xPluginDefinition.Name = pluginTypeName;
                        }
                        else
                        {
                            xPluginDefinition = new XElement(pluginTypeName);
                            xSavedPluginDefinition.Add(xPluginDefinition);
                        }

                        foreach (DataColumn dcPluginProperty in drPluginDefinition.Table.Columns)
                        {
                            string dPropertyName = dcPluginProperty.ColumnName;
                            string xPropertyName = GetXPropertyName(pluginType, dPropertyName);

                            if (!MyUtils.IsEmpty(xPropertyName))
                            {
                                bool isAttribute = xPropertyName.StartsWith("@");
                                if (isAttribute)
                                {
                                    xPropertyName = xPropertyName.Substring(1);
                                }

                                object dPropertyValue = drPluginDefinition[dPropertyName];

                                if (!IsEqualToDefaultValue(pluginType, xPropertyName, dPropertyValue) &&
                                    !IsEqualToDefaultValue(pluginType, dPropertyName, dPropertyValue))
                                {
                                    if (isAttribute)
                                    {
                                        xPluginDefinition.Add(new XAttribute(xPropertyName, dPropertyValue));
                                    }
                                    else
                                    {
                                        xPluginDefinition.Add(CreateXElement(xPropertyName, dPropertyValue));
                                    }
                                }
                            }
                        }

                        DownloadDerivedValues(drPluginDefinition, xPluginDefinition);

                        foreach (XiPlugin xiChildPlugin in GetXiChildPlugins())
                        {
                            xiChildPlugin.DownloadToXml(drPluginDefinition, xPluginDefinition);
                        }
                    }
                }
            }
        }

        public virtual void UploadFromXml(DataRow drKey, List<XElement> xElements)
        {
            DeletePluginDefinitions(drKey);

            // Should be empty, only need structure of record
            DataTable dtPluginDefinitions = GetPluginDefinitions(drKey);
            dtPluginDefinitions.Rows.Clear();
            DataColumnCollection dcPluginColumns = dtPluginDefinitions.Columns;
            DataRow drPluginDefinition = dtPluginDefinitions.NewRow();
            dtPluginDefinitions.Rows.Add(drPluginDefinition);

            List<XElement> xPluginDefinitions = GetPluginDefinitions(xElements);

            if (xPluginDefinitions != null)
            {
                foreach (XElement xPluginDefinition in xPluginDefinitions)
                {
                    SetupDefaultValues(drPluginDefinition, xPluginDefinition);
                    SetupKeyValues(drPluginDefinition, drKey);
                    SetupXmlValues(drPluginDefinition, xPluginDefinition);
                    UploadDerivedValues(drPluginDefinition, xPluginDefinition);

                    WriteToDB(drPluginDefinition);

                    foreach (XiPlugin xiChildPlugin in GetXiChildPlugins())
                    {
                        DataRow drChildKey = GetRowKeyValues(drPluginDefinition, xPluginDefinition);
                        xiChildPlugin.UploadFromXml(drChildKey, xPluginDefinition.Elements().ToList<XElement>());
                    }
                }
            }
        }

        public virtual string GetPuxUrl(DataRow drPluginKey)
        {
            string appName = MyWebUtils.GetApplicationName(drPluginKey);

            if (PluginType != null)
            {
                string keyName = PluginType.ToString().Substring("CodeClay.Ci".Length) + "Name";
                string keyValue = "";

                DataRow drPluginDefinition = GetPluginDefinition(drPluginKey);

                if (drPluginDefinition != null && drPluginDefinition.Table.Columns.Contains(keyName))
                {
                    object objKeyValue = drPluginDefinition[keyName];

                    if (objKeyValue != null)
                    {
                        keyValue = objKeyValue.ToString();
                    }

                    if (!MyUtils.IsEmpty(appName) && !MyUtils.IsEmpty(keyValue))
                    {
                        return MyWebUtils.MapPath(string.Format("Sites/{0}/{1}.pux", appName, keyValue));
                    }
                }
            }

            return null;
        }

        protected virtual bool IsPluginTypeOk(Type pluginType)
        {
            return pluginType != null && PluginType != null &&
                (pluginType.IsSubclassOf(PluginType) || pluginType == PluginType);
        }

        protected virtual string GetPluginTypeName(DataRow drPluginDefinition)
        {
            if (PluginType != null)
            {
                return PluginType.ToString().Substring("CodeClay.".Length);
            }

            return null;
        }

        protected virtual string GetPluginTypeName(XElement xPluginDefinition)
        {
            if (xPluginDefinition != null)
            {
                return xPluginDefinition.Name.ToString();
            }

            return null;
        }

        protected virtual Type GetPluginType(string pluginTypeName)
        {
            return TypeInfo.GetType(string.Format("CodeClay.{0}", pluginTypeName));
        }

        protected virtual DataTable GetPluginDefinitions(DataRow drPluginKey)
        {
            return null;
        }

        protected virtual List<XElement> GetPluginDefinitions(List<XElement> xElements)
        {
            return xElements;
        }

        protected virtual DataRow GetPluginDefinition(DataRow drPluginKey)
        {
            DataTable dtPluginDefinitions = GetPluginDefinitions(drPluginKey);

            if (MyWebUtils.GetNumberOfRows(dtPluginDefinitions) > 0)
            {
                return dtPluginDefinitions.Rows[0];
            }

            return null;
        }

        protected virtual void DeletePluginDefinitions(DataRow drPluginKey)
        {
        }

        protected virtual void WriteToDB(DataRow drPluginDefinition)
        {
        }

        protected virtual string GetXPropertyName(Type pluginType, string dPropertyName)
        {
            if (pluginType != null)
            {
                PropertyInfo pi = pluginType.GetProperty(dPropertyName);
                if (pi != null)
                {
                    XmlAttributeAttribute a = pi.GetCustomAttribute(typeof(XmlAttributeAttribute)) as XmlAttributeAttribute;
                    if (a != null)
                    {
                        return "@" + a.AttributeName;
                    }

                    return dPropertyName;
                }
            }

            return null;
        }

        protected virtual string GetDPropertyName(string xPropertyName)
        {
            return xPropertyName;
        }

        protected virtual void DownloadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
        }

        protected virtual void UploadDerivedValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
        }

        protected virtual List<XiPlugin> GetXiChildPlugins()
        {
            return new List<XiPlugin>();
        }

        protected virtual DataRow GetRowKeyValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            XElement xRowKey = xPluginDefinition.Element("RowKey");

            string rowKeyFromXml = (xRowKey != null) ? xRowKey.Value : "";

            DataRow drKey = MyUtils.CloneDataRow(drPluginDefinition);
            DataColumnCollection dcKeyColumns = drKey.Table.Columns;
            string[] rowKeyNames = ("AppID,TableID,FieldID,MacroID," + rowKeyFromXml).Split(',');

            foreach (DataColumn dcKeyColumn in drPluginDefinition.Table.Columns)
            {
                string columnName = dcKeyColumn.ColumnName;
                if (!rowKeyNames.Contains(columnName))
                {
                    dcKeyColumns.Remove(columnName);
                }
            }

            return drKey;
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private bool IsEqualToDefaultValue(Type pluginType, string dPropertyName, object dPropertyValue)
        {
            if (pluginType != null)
            {
                CiPlugin ciPlugin = Activator.CreateInstance(pluginType) as CiPlugin;

                PropertyInfo p = pluginType.GetProperty(dPropertyName);

                if (p != null)
                {
                    object defaultPropertyValue = p.GetValue(ciPlugin);
                    XmlElement xDefaultPropertyValue = defaultPropertyValue as XmlElement;
                    if (xDefaultPropertyValue != null)
                    {
                        defaultPropertyValue = xDefaultPropertyValue.InnerText;
                    }

                    string dPropertyValueString = MyUtils.Coalesce(dPropertyValue, "").ToString();
                    string defaultPropertyValueString = MyUtils.Coalesce(defaultPropertyValue, "").ToString();

                    if (dPropertyValueString == defaultPropertyValueString)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SetupDefaultValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            string pluginTypeName = GetPluginTypeName(xPluginDefinition);
            Type pluginType = GetPluginType(pluginTypeName);

            if (IsPluginTypeOk(pluginType) && drPluginDefinition != null)
            {
                CiPlugin ciPlugin = Activator.CreateInstance(pluginType) as CiPlugin;

                foreach (PropertyInfo p in pluginType.GetProperties())
                {
                    string xPropertyName = p.Name;
                    string dPropertyName = GetDPropertyName(xPropertyName);

                    if (!MyUtils.IsEmpty(dPropertyName) && drPluginDefinition.Table.Columns.Contains(dPropertyName))
                    {
                        XmlSqlElementAttribute a = p.GetCustomAttribute(typeof(XmlSqlElementAttribute)) as XmlSqlElementAttribute;
                        object dPropertyValue = p.GetValue(ciPlugin);

                        if (a != null)
                        {
                            dPropertyValue = MyWebUtils.Eval(dPropertyValue as XmlElement, null, a.Type);
                        }

                        drPluginDefinition[dPropertyName] = dPropertyValue;
                    }
                }
            }
        }

        private void SetupKeyValues(DataRow drPluginDefinition, DataRow drKey)
        {
            if (drPluginDefinition != null && drKey != null)
            {
                DataTable dtPluginDefinition = drPluginDefinition.Table;
                DataTable dtKey = drKey.Table;

                if (dtPluginDefinition != null && dtKey != null)
                {
                    foreach (DataColumn dcKeyColumn in dtKey.Columns)
                    {
                        string columnName = dcKeyColumn.ColumnName;
                        if (dtPluginDefinition.Columns.Contains(columnName))
                        {
                            drPluginDefinition[columnName] = drKey[columnName];
                        }
                    }
                }
            }
        }

        private void SetupXmlValues(DataRow drPluginDefinition, XElement xPluginDefinition)
        {
            if (drPluginDefinition != null && xPluginDefinition != null)
            {
                DataTable dtPluginDefinition = drPluginDefinition.Table;

                foreach (XAttribute xPluginProperty in xPluginDefinition.Attributes())
                {
                    string xPropertyName = xPluginProperty.Name.ToString();
                    object xPropertyValue = xPluginProperty.Value;
                    string dPropertyName = GetDPropertyName(xPropertyName);

                    if (!MyUtils.IsEmpty(dPropertyName) && dtPluginDefinition.Columns.Contains(dPropertyName))
                    {
                        drPluginDefinition[dPropertyName] = MyUtils.Coalesce(xPropertyValue, Convert.DBNull);
                    }
                }

                foreach (XElement xPluginProperty in xPluginDefinition.Elements())
                {
                    string xPropertyName = xPluginProperty.Name.ToString();
                    if (xPluginProperty.Attribute("lang") == null)
                    {
                        object xPropertyValue = GetXPropertyValue(xPluginProperty);
                        string dPropertyName = GetDPropertyName(xPropertyName);

                        if (!MyUtils.IsEmpty(dPropertyName) && dtPluginDefinition.Columns.Contains(dPropertyName))
                        {
                            drPluginDefinition[dPropertyName] = MyUtils.Coalesce(xPropertyValue, Convert.DBNull);
                        }
                    }
                }
            }
        }

        private object GetXPropertyValue(XElement xElement)
        {
            if (xElement != null)
            {
                if (xElement.HasElements)
                {
                    var reader = xElement.CreateReader();
                    reader.MoveToContent();

                    return reader.ReadInnerXml();
                }
                else
                {
                    return xElement.Value;
                }
            }

            return null;
        }

        private XElement CreateXElement(string propertyName, object propertyValue)
        {
            string openingTag = string.Format("<{0}>", propertyName);
            string closingTag = string.Format("</{0}>", propertyName);
            string contents = "";

            if (propertyValue != null)
            {
                if (propertyValue.GetType() == typeof(string))
                {
                    contents = propertyValue.ToString();
                }
                else
                {
                    return new XElement(propertyName, propertyValue);
                }
            }

            return XElement.Parse(openingTag + contents + closingTag);
        }
    }
}