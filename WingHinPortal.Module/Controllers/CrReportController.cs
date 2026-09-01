using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using WingHinPortal.Module.BusinessObjects;
using WingHinPortal.Module.BusinessObjects.Setup;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Configuration;
using System.Web;
using DevExpress.ExpressApp.Web;
using System.IO;

namespace WingHinPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class CrReportController : ViewController
    {
        GenControllers genCon;
        public CrReportController()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
            TargetObjectType = typeof(CrReports);
            TargetViewType = ViewType.ListView;
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
            genCon = Frame.GetController<GenControllers>();
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        private void GetCrRpt_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            int oid = 0;
            if (View.SelectedObjects.Count == 1)
            {
                foreach (CrReports selectedObject in View.SelectedObjects)
                {
                    oid = selectedObject.Oid;
                }
            }

            string url = System.Web.HttpContext.Current.Request.Url.AbsoluteUri.Replace("CrReports_ListView/", "CrParams.aspx?id=" + oid.ToString());

            string script = "window.open('" + url + "');";

            DevExpress.ExpressApp.Web.WebWindow.CurrentRequestWindow.RegisterStartupScript("CrParams", script, true);
        }
        private void GetReport_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            int oid = 0;
            if (View.SelectedObjects.Count == 1)
            {
                foreach (CrReports selectedObject in View.SelectedObjects)
                {
                    oid = selectedObject.Oid;
                }
            }

            IObjectSpace os = Application.CreateObjectSpace(typeof(DateFromTo));
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<DateFromTo>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;

            ((DateFromTo)dv.CurrentObject).DateFrom = DateTime.Today.Date;
            ((DateFromTo)dv.CurrentObject).DateTo = DateTime.Today.Date;
            ((DateFromTo)dv.CurrentObject).IsErr = false;
            ((DateFromTo)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action, else press Cancel.";

            if (oid <= 0)
            {
                ((DateFromTo)dv.CurrentObject).IsErr = true;
                genCon.showMsg("Fail", "Please select only 1 Cr Report.", InformationType.Error);
            }
            e.View = dv;
        }

        private void GetReport_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            DateFromTo p = (DateFromTo)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;

            string strServer;
            string strDatabase;
            string strUserID;
            string strPwd;
            string filename;

            CrReports cr = null;
            if (View.SelectedObjects.Count == 1)
            {
                foreach (CrReports selectedObject in View.SelectedObjects)
                {
                    cr = selectedObject;
                }
            }
            if (cr == null)
            {
                genCon.showMsg("Fail", "Please select only 1 Cr Report.", InformationType.Error);
                return;
            }

            SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

            try
            {
                ReportDocument doc = new ReportDocument();
                strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                //doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\Purchase Order.rpt"));
                doc.Load(HttpContext.Current.Server.MapPath(cr.ReportPathFile));
                strDatabase = ConfigurationManager.AppSettings.Get("ReportDB").ToString();
                strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                doc.Refresh();

                doc.SetParameterValue("@DateFrom", p.DateFrom.Date);
                doc.SetParameterValue("@DateTo", p.DateTo.Date);

                filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + ConfigurationManager.AppSettings.Get("ReportDB").ToString()
                    + "_" + cr.ReportName + "_" + user.UserName + "_"
                    + DateTime.Parse(p.DateFrom.ToString()).ToString("yyyyMMdd") + "_"
                    + DateTime.Parse(p.DateTo.ToString()).ToString("yyyyMMdd") + ".xls";

                doc.ExportToDisk(ExportFormatType.Excel, filename);
                doc.Close();
                doc.Dispose();

                WebWindow.CurrentRequestWindow.RegisterStartupScript("DownloadFile", GetScript(filename));
            }
            catch (Exception ex)
            {
                genCon.showMsg("Fail", ex.Message, InformationType.Error);
            }

        }
        protected string GetScript(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);

            //To Download PDF
            //return @"var mainDocument = window.parent.document;
            //var iframe = mainDocument.getElementById('reportout');
            //if (iframe != null) {
            //  mainDocument.body.removeChild(iframe);
            //}
            //iframe = mainDocument.createElement('iframe');
            //iframe.setAttribute('id', 'reportout');
            //iframe.style.width = 0 + 'px';
            //iframe.style.height = 0 + 'px';
            //iframe.style.display = 'none';
            //mainDocument.body.appendChild(iframe);
            //mainDocument.getElementById('reportout').contentWindow.location = 'DownloadFile.aspx?filename=" + fileInfo.Name + "';";

            //To View PDF
            return @"var newWindow = window.open();
            newWindow.document.write('<iframe src=""DownloadExcel.aspx?filename=" + fileInfo.Name + @""" frameborder =""0"" allowfullscreen style=""width: 100%;height: 100%""></iframe>');
            ";
        }

    }
}
