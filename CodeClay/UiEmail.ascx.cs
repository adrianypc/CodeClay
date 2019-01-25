using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;

// Extra references
using CodistriCore;

namespace CodeClay
{
    [XmlType("CiEmail")]
    public class CiEmail: CiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Member Variables
        // --------------------------------------------------------------------------------------------------

        private string mBody = "";

        // --------------------------------------------------------------------------------------------------
        // Properties (PUX)
        // --------------------------------------------------------------------------------------------------

        [XmlElement("SelectSQL")]
        public string SelectSQL { get; set; } = "";

        [XmlElement("From")]
        public string From { get; set; } = "";

        [XmlElement("To")]
        public string To { get; set; } = "";

        [XmlElement("Port")]
        public int Port { get; set; } = 25;

        [XmlElement("Host")]
        public string Host { get; set; } = "";

        [XmlElement("UserName")]
        public string UserName { get; set; } = "";

        [XmlElement("Password")]
        public string Password { get; set; } = "";

        [XmlElement("Subject")]
        public string Subject { get; set; } = "";

        [XmlAnyElement("Body")]
        public XmlElement Body
        {
            get
            {
                var x = new XmlDocument();
                x.LoadXml(string.Format("<Body>{0}</Body>", mBody));
                return x.DocumentElement;
            }

            set { mBody = value.InnerXml; }
        }

        [XmlElement("IsHtml")]
        public bool IsHtml { get; set; } = false;
    }

    public partial class UiEmail : UiPlugin
    {
        // --------------------------------------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------------------------------------

        public CiEmail CiEmail
        {
            get { return CiPlugin as CiEmail; }
            set { CiPlugin = value; }
        }

        // --------------------------------------------------------------------------------------------------
        // Event Handlers
        // --------------------------------------------------------------------------------------------------

        protected override void Page_Load(object sender, EventArgs e)
        {
            SendEmail();
        }

        // --------------------------------------------------------------------------------------------------
        // Helpers
        // --------------------------------------------------------------------------------------------------

        private void SendEmail()
        {
            if (CiEmail != null)
            {
                SmtpClient client = new SmtpClient();
                client.Port = CiEmail.Port;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(CiEmail.UserName, CiEmail.Password);
                client.Host = CiEmail.Host;

                string selectSQL = CiEmail.SelectSQL;
                DataTable dt = !MyUtils.IsEmpty(selectSQL)
                    ? MyWebUtils.GetBySQL(selectSQL, GetState())
                    : null;

                uiTable.CiTable.DataTable = dt;

                if (dt != null)
                {
                    DataColumnCollection columns = dt.Columns;
                    columns.Add("EmailStatus");

                    foreach (DataRow dr in dt.Rows)
                    {
                        string toEmailAddress = MyUtils.Coalesce(CiEmail.To, "").ToString();
                        string subject = MyUtils.Coalesce(CiEmail.Subject, "").ToString();
                        string body = MyUtils.Coalesce(CiEmail.Body.InnerXml, "").ToString();

                        foreach (DataColumn column in columns)
                        {
                            string columnName = column.ColumnName;

                            toEmailAddress = toEmailAddress.Replace("@" + columnName, MyUtils.Coalesce(dr[columnName], "").ToString());
                            subject = subject.Replace("@" + columnName, MyUtils.Coalesce(dr[columnName], "").ToString());
                            body = body.Replace("@" + columnName, MyUtils.Coalesce(dr[columnName], "").ToString());
                        }

                        MailMessage mail = new MailMessage(CiEmail.From, toEmailAddress);
                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = CiEmail.IsHtml;

                        dr["EmailStatus"] = string.Format("Email successfully sent on {0} at {1}",
                            DateTime.Now.ToShortDateString(),
                            DateTime.Now.ToShortTimeString());

                        try
                        {
                            client.Send(mail);
                        }
                        catch (Exception ex)
                        {
                            dr["EmailStatus"] = ex.Message;
                        }
                    }
                }
            }
        }
    }
}