using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;

// Extra references
using CodistriCore;
using DevExpress.Web;

namespace CodeClay
{
    public partial class Default : System.Web.UI.Page
    {
        // --------------------------------------------------------------------------------------------------
        // Constants
        // --------------------------------------------------------------------------------------------------

        const string PLUGIN_SRC_KEY = "PluginSrc";
        const string APPLICATION_PUX_FILE = "UiApplication.pux";

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected void Page_PreInit(Object sender, EventArgs e)
        {
            if (Request.QueryString.Count > 0)
            {
                MyWebUtils.QueryString = Request.QueryString;
            }
            else
            {
                MyWebUtils.QueryString = new System.Collections.Specialized.NameValueCollection();
            }

            if (MyWebUtils.IsPopup(this))
            {
                this.MasterPageFile = "~/Site.Popup.Master";
            }
            else
            {
                this.MasterPageFile = "~/Site.Master";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            UiApplication.Me = CreateUiPlugin(APPLICATION_PUX_FILE) as UiApplication;

            if (!MyWebUtils.IsTimeOutReached(this))
            {
                MyWebUtils.RegisterPluginScripts(this);

                myOuterPanel.Controls.AddAt(0, UiApplication.Me);

                object justLoggedIn = Session["JustLoggedIn"];
                if (justLoggedIn != null && Convert.ToBoolean(justLoggedIn))
                {
                    Session["JustLoggedIn"] = null;
                    UiApplication.Me.RunLoginMacro();
                }

                string puxFile = MyWebUtils.QueryString[PLUGIN_SRC_KEY];
                if (puxFile != APPLICATION_PUX_FILE)
                {
                    if (MyUtils.IsEmpty(puxFile))
                    {
                        puxFile = UiApplication.Me.CiApplication.HomePluginSrc;
                    }

                    LoadUiPlugin(puxFile);
                }
            }
        }

        protected void myCallbackPanel_Callback(object sender, CallbackEventArgsBase e)
        {
            if (!MyWebUtils.IsTimeOutReached(this))
            {
                foreach (Control control in myInnerPanel.Controls)
                {
                    control.Dispose();
                }

                myInnerPanel.Controls.Clear();

                string puxFile = e.Parameter;

                if (!MyUtils.IsEmpty(puxFile))
                {
                    LoadUiPlugin(puxFile);
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Methods
        // --------------------------------------------------------------------------------------------------

        public virtual void LoadUiPlugin(string puxFile)
        {
            LoadUiPlugin(CiPlugin.CreateCiPlugin(puxFile));
        }

        public virtual void LoadUiPlugin(CiPlugin ciPlugin)
        {
            UiPlugin uiPlugin = CreateUiPlugin(ciPlugin);
            if (uiPlugin != null)
            {
                if (uiPlugin.IsInstanceOf(typeof(UiMenu)))
                {
                    UiApplication.Me.CiApplication.Add(uiPlugin.CiPlugin);
                }
                else
                {
                    myInnerPanel.Controls.Add(uiPlugin);
                }
            }
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private UiPlugin CreateUiPlugin(string puxFile)
        {
            if (!MyUtils.IsEmpty(puxFile))
            {
                return CreateUiPlugin(CiPlugin.CreateCiPlugin(puxFile));
            }

            return null;
        }

        private UiPlugin CreateUiPlugin(CiPlugin ciPlugin)
        {
            if (ciPlugin != null)
            {
                return ciPlugin.CreateUiPlugin(this);
            }

            return null;
        }
    }
}