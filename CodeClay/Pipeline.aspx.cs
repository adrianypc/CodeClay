using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CodeClay
{
    public partial class PivotGrid : System.Web.UI.Page
    {   
        
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void htSQLDataSource_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
        }

        protected void pvgPipeline_HtmlFieldValuePrepared(object sender, DevExpress.Web.ASPxPivotGrid.PivotHtmlFieldValuePreparedEventArgs e)
        {
            HyperLink link = new HyperLink();
            if (e.Field == pvgPipeline.GetFieldByArea(DevExpress.XtraPivotGrid.PivotArea.ColumnArea, 6))
            {
                string getValue = (string)e.Value;
                string[] getParaList = getValue.Split('=');
                string getTextValue = getParaList[getParaList.Length - 1];

                link.NavigateUrl = getValue;
                link.Text = getTextValue;
                
                foreach (Control control in e.Cell.Controls)
                {
                    if (control is LiteralControl)
                    {
                        e.Cell.Controls.Remove(control);
                        break;
                    }
                }
                e.Cell.Controls.Add(link);
            }
        }
    }
}