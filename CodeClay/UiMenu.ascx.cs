using Microsoft.AspNet.Identity;
using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Xml.Serialization;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    [XmlType("CiMenu")]
    public class CiMenu : CiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("MenuName")]
        public string MenuName { get; set; } = "";

        [XmlElement("AppName")]
        public string AppName { get; set; } = "";

        [XmlElement("PluginSrc")]
        public string PluginSrc { get; set; } = "";

        [XmlElement("PluginPos")]
        public string PluginPos { get; set; } = "";
    }

    public partial class UiMenu : UiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public CiMenu CiMenu
        {
            get { return CiPlugin as CiMenu; }
            set { CiPlugin = value; }
        }

        protected CiMenu CiShortcutMenu { get; set; } = null;

        protected string CurrentUrl { get; set; } = null;

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected void dxMenu_Init(object sender, EventArgs e)
        {
            ASPxMenu dxMenu = sender as ASPxMenu;
            if (dxMenu != null)
            {
                MenuItem dxRootItem = dxMenu.RootItem;
                if (dxRootItem != null)
                {
                    MenuItem dxUserMenuItem = dxRootItem.Items.FindByName("User");
                    if (dxUserMenuItem != null)
                    {
                        dxUserMenuItem.Text = Context.User.Identity.Name;
                    }

                    CurrentUrl = MyWebUtils.QueryString.ToString();

                    CiApplication ciApplication = UiApplication.Me.CiApplication;
                    if (ciApplication != null)
                    {
                        if (!CurrentUrl.Contains(".pux"))
                        {
                            string homePluginSrc = ciApplication.HomePluginSrc;
                            if (!MyUtils.IsEmpty(homePluginSrc))
                            {
                                CurrentUrl += string.Format("&PluginSrc={0}", homePluginSrc);
                            }
                        }

                        CiMenu[] ciMenus = ciApplication.Get<CiMenu>();
                        if (ciMenus != null)
                        {
                            MenuItem dxDividerMenuItem = dxRootItem.Items.FindByName("Divider");
                            if (dxDividerMenuItem != null)
                            {
                                int menuIndex = dxDividerMenuItem.Index;

                                foreach (CiMenu ciMenu in ciMenus)
                                {
                                    CreateMenuItem(ciMenu, dxRootItem, ++menuIndex);
                                }
                            }
                        }

                        MenuItem dxShortcutMenuItem = dxRootItem.Items.FindByName("Shortcut");
                        if (dxShortcutMenuItem != null)
                        {
                            bool hasShortCutMenu = (CiShortcutMenu != null);

                            if (hasShortCutMenu)
                            {
                                string shortcutMenuName = CiShortcutMenu.MenuName;
                                if (shortcutMenuName != "Application")
                                {
                                    dxShortcutMenuItem.Text = shortcutMenuName;
                                    foreach (CiMenu ciMenu in CiShortcutMenu.Get<CiMenu>())
                                    {
                                        CreateMenuItem(ciMenu, dxShortcutMenuItem, dxShortcutMenuItem.Items.Count);
                                    }
                                }
                            }

                            dxShortcutMenuItem.Visible = hasShortCutMenu && (dxShortcutMenuItem.Items.Count > 1);
                        }
                    }
                }
            }
        }

        protected void LoginStatus_LoggingOut(object sender, System.Web.UI.WebControls.LoginCancelEventArgs e)
        {
            UiApplication.Me.RunLogoutMacro();
            Context.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private void CreateMenuItem(CiMenu ciMenu, MenuItem dxParentMenuItem, int menuIndex)
        {
            if (ciMenu != null && dxParentMenuItem != null)
            {
                string menuName = ciMenu.MenuName;
                MenuItem dxMenuItem = new MenuItem();

                // Create menu item
                dxMenuItem.Name = menuName;
                dxMenuItem.Text = menuName;
                dxMenuItem.Visible = !MyWebUtils.Eval<bool>(ciMenu.Hidden, GetState());

                string pluginSrc = ciMenu.PluginSrc;
                if (!MyUtils.IsEmpty(pluginSrc))
                {
                    dxMenuItem.NavigateUrl = GetQueryString(ciMenu.AppName, pluginSrc);
                    dxMenuItem.Target = (ciMenu.PluginPos == "NewTab") ? "_blank" : "_self";
                }

                dxParentMenuItem.Items.Insert(menuIndex, dxMenuItem);

                // Recursively create child menu items
                foreach (CiMenu ciChildMenu in ciMenu.Get<CiMenu>())
                {
                    CreateMenuItem(ciChildMenu, dxMenuItem, dxMenuItem.Items.Count);
                    string childMenuUrl = string.Format("Application={0}&PluginSrc={1}", MyWebUtils.Application, ciChildMenu.PluginSrc);
                    if (CurrentUrl == childMenuUrl && CiShortcutMenu == null)
                    {
                        CiShortcutMenu = ciMenu;
                    }
                }
            }
        }

        private string GetQueryString(string appName, string pluginSrc)
        {
            string url = pluginSrc;

            ArrayList parameterNames = MyUtils.GetParameters(url);
            DataRow drParams = GetState();

            foreach (string parameterName in parameterNames)
            {
                url = url.Replace("@" + parameterName, MyWebUtils.GetStringField(drParams, parameterName));
            }

            if (!MyUtils.IsEmpty(appName))
            {
                if (appName.Trim() == "@CI_AppName")
                {
                    appName = MyWebUtils.GetApplicationName(GetState());
                }
            }
            else
            {
                appName = MyWebUtils.Application;
            }

            if (!url.StartsWith("http://"))
            {
                url = string.Format("~/Default.aspx?Application={0}&PluginSrc={1}", appName, url);
            }

            return url;
        }
    }
}