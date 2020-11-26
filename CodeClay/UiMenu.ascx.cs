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

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------
        
        protected void dxMenu_Init(object sender, EventArgs e)
        {
            ASPxMenu dxMenu = sender as ASPxMenu;
            if (dxMenu != null)
            {
                MenuItem dxRootItem = dxMenu.RootItem;

                MenuItem dxUserMenuItem = dxRootItem.Items.FindByName("User");
                if (dxUserMenuItem != null)
                {
                    dxUserMenuItem.Text = Context.User.Identity.Name;
                }
                
                CiApplication ciApplication = UiApplication.Me.CiApplication;
                if (ciApplication != null)
                {
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
                MenuItem dxMenuItem = new MenuItem();

                // Create menu item
                dxMenuItem.Text = ciMenu.MenuName;

                string pluginSrc = ciMenu.PluginSrc;
                if (!MyUtils.IsEmpty(pluginSrc))
                {
                    dxMenuItem.NavigateUrl = GetQueryString(ciMenu.AppName, pluginSrc);
                }

                dxParentMenuItem.Items.Insert(menuIndex, dxMenuItem);

                // Recursively create child menu items
                foreach (CiMenu ciChildMenu in ciMenu.Get<CiMenu>())
                {
                    CreateMenuItem(ciChildMenu, dxMenuItem, dxMenuItem.Items.Count);
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