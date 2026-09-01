using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WingHinPortal.Web
{
    public partial class DownloadExcel : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + Request.QueryString["filename"];
            if (System.IO.File.Exists(filename))
            {
                FileInfo fileInfo = new FileInfo(filename);
                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();
                //To Download PDF
                //Response.AddHeader("Content-Disposition", "attachment; filename=" + fileInfo.Name);

                //To View PDF
                Response.AddHeader("Content-Disposition", "inline; filename=" + fileInfo.Name);

                Response.AddHeader("Content-Length", fileInfo.Length.ToString());
                Response.ContentType = "application/xls";
                Response.WriteFile(fileInfo.FullName);
                Response.Flush();
                Response.End();
            }
        }
    }
}