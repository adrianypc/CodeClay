using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;
using System.Data;
using System.Xml;
using System.Xml.Linq;

namespace CodeClay
{
    public class CiPlugin : MarshalByRefObject
    {
        // --------------------------------------------------------------------------------------------------
        // Constants
        // --------------------------------------------------------------------------------------------------

        public const string LIST_SEPARATOR = "|";

        // --------------------------------------------------------------------------------------------------
        // Member variables
        // --------------------------------------------------------------------------------------------------

        private string mSrc = "";
        private ArrayList mCiPlugins = new ArrayList();
        private string mLayoutUrl = "";
        private XmlDocument mLayoutXml = new XmlDocument();
        private string mDefaultValue = "";

        // --------------------------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------------------------

        public CiPlugin()
        {
            mSrc = string.Format("Ui{0}.pux", GetType().Name.Substring(2));
        }

        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlAttribute("Src")]
        public string Src
        {
            get { return mSrc; }

            set
            {
                mSrc = value;

                CiPlugin ciSrcPlugin = CiPlugin.CreateCiPlugin(mSrc);
                this.Inherits(ciSrcPlugin);
            }
        }

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
        [XmlElement("CiDateField", typeof(CiDateField))]
        [XmlElement("CiField", typeof(CiField))]
        [XmlElement("CiLinkField", typeof(CiLinkField))]
        [XmlElement("CiFieldExitMacro", typeof(CiFieldExitMacro))]
        [XmlElement("CiMacro", typeof(CiMacro))]
        [XmlElement("CiMemoField", typeof(CiMemoField))]
        [XmlElement("CiMenu", typeof(CiMenu))]
        [XmlElement("CiNumericField", typeof(CiNumericField))]
        [XmlElement("CiRadioField", typeof(CiRadioField))]
        [XmlElement("CiReport", typeof(CiReport))]
        [XmlElement("CiTable", typeof(CiTable))]
        [XmlElement("CiTextField", typeof(CiTextField))]
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
                    CiPlugin[] ciPlugins = (CiPlugin[])value;
                    foreach (CiPlugin ciPlugin in ciPlugins)
                    {
                        mCiPlugins.Add(ciPlugin);
                        ciPlugin.CiParentPlugin = this;
                    }
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Properties (Derived)
        // --------------------------------------------------------------------------------------------------

        [XmlIgnore]
        public string PuxFile { get; set; } = "";

        [XmlIgnore]
        public string DefaultValue
        {
            get { return mDefaultValue; }
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

        // --------------------------------------------------------------------------------------------------
        // Methods (Virtual)
        // --------------------------------------------------------------------------------------------------

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

        public virtual void Inherits(CiPlugin ciPlugin)
        {
            if (ciPlugin != null)
            {
                foreach (PropertyInfo p in ciPlugin.GetType().GetProperties())
                {
                    if (!("Src, CiPlugins".Contains(p.Name)))
                    {
                        Attribute a = MyUtils.Coalesce(p.GetCustomAttribute(typeof(XmlElementAttribute)),
                          p.GetCustomAttribute(typeof(XmlAttributeAttribute)),
                          p.GetCustomAttribute(typeof(XmlAnyElementAttribute))) as Attribute;

                        if (a != null)
                        {
                            p.SetValue(this, p.GetValue(ciPlugin));
                        }
                    }
                }

                CiPlugins = ciPlugin.CiPlugins;
            }
        }

        public static CiPlugin CreateCiPlugin(string puxFile)
        {
            string puxPath = MyWebUtils.MapPath(MyWebUtils.ApplicationFolder + @"\" + puxFile);

            if (File.Exists(puxPath))
            {
                string puxContents = File.ReadAllText(puxPath);
                XElement pluginXML = XElement.Parse(puxContents);
                string ciPluginName = pluginXML.Name.ToString();
                Type pluginType = Type.GetType("CodeClay." + ciPluginName);

                TextReader reader = new StringReader(puxContents);
                if (reader != null)
                {
                    XmlSerializer serializer = new XmlSerializer(pluginType);
                    if (serializer != null)
                    {
                        try
                        {
                            CiPlugin ciPlugin = serializer.Deserialize(reader) as CiPlugin;
                            if (ciPlugin != null)
                            {
                                ciPlugin.PuxFile = puxFile;
                            }

                            return ciPlugin;
                        }
                        catch
                        {
                            // Do nothing, NULL will be returned
                        }
                    }

                    reader.Close();
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
                        uiPlugin.ID = PuxFile;
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
            return null;
        }

        public virtual void Add(CiPlugin ciPlugin)
        {
            if (mCiPlugins != null)
            {
                mCiPlugins.Add(ciPlugin);
            }
        }
    }

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

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

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

        public virtual object GetClientValue(string key)
        {
            return UiApplication.Me[GetFormattedKey(key)];
        }

        public virtual void SetClientValue(string key, object value)
        {
            UiApplication.Me[GetFormattedKey(key)] = value;
        }

        public virtual object GetServerValue(string key, int rowIndex = -1)
        {
            return null;
        }

        public virtual object this[string key, int rowIndex = -1]
        {
            get
            {
                if (rowIndex == -1)
                {
                    rowIndex = GetFocusedIndex();
                }

                object clientValue = GetClientValue(key);

                if (!MyUtils.IsEmpty(clientValue))
                {
                    if (clientValue.ToString() == "\a")
                    {
                        clientValue = "";
                    }

                    return clientValue;
                }

                object serverValue = GetServerValue(key, rowIndex);

                if (!MyUtils.IsEmpty(serverValue))
                {
                    return serverValue;
                }

                object parentValue = (CiPlugin != null && CiPlugin.RowKeyNames.Contains(key) && UiParentPlugin != null)
                  ? UiParentPlugin[key]
                  : null;

                if (!MyUtils.IsEmpty(parentValue))
                {
                    return parentValue;
                }

                object queryStringValue = MyWebUtils.QueryString[key];

                if (!MyUtils.IsEmpty(queryStringValue))
                {
                    return queryStringValue;
                }

                CiPlugin ciChildPlugin = CiPlugin.GetById(key);

                if (ciChildPlugin != null)
                {
                    return ciChildPlugin.DefaultValue;
                }

                return null;
            }

            set
            {
                SetClientValue(key, value);
            }
        }

        public virtual DataRow GetState(int rowIndex = -1)
        {
            DataTable dt = new DataTable();
            DataColumnCollection dcColumns = dt.Columns;
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            foreach (string fieldName in MyWebUtils.QueryString.Keys)
            {
                object fieldValue = MyWebUtils.QueryString[fieldName];

                if (!dcColumns.Contains(fieldName))
                {
                    dcColumns.Add(fieldName);
                    dr[fieldName] = fieldValue;
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

                            control.Controls.Add(uiPlugin);
                        }
                    }
                }
            }
        }
    }
}