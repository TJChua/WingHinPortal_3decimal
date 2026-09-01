using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using WingHinPortal.Module.BusinessObjects;
using WingHinPortal.Module.BusinessObjects.PR;
using WingHinPortal.Module.BusinessObjects.Setup;

namespace WingHinPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class PRControllers : ViewController
    {
        GenControllers genCon;
        public PRControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
            TargetObjectType = typeof(PurchaseRequest);
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

            if (View.Id == "PurchaseRequest_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.Edit)
                {
                    this.SubmitPR.Active.SetItemValue("Enabled", false);
                    this.CancelPR.Active.SetItemValue("Enabled", false);
                    this.PrintPR.Active.SetItemValue("Enabled", false);
                }
                else
                {
                    this.SubmitPR.Active.SetItemValue("Enabled", true);
                    this.CancelPR.Active.SetItemValue("Enabled", true);
                    this.PrintPR.Active.SetItemValue("Enabled", true);
                }
            }
            else
            {
                this.SubmitPR.Active.SetItemValue("Enabled", false);
                this.CancelPR.Active.SetItemValue("Enabled", false);
                this.PrintPR.Active.SetItemValue("Enabled", false);
            }
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        public void openNewView(IObjectSpace os, object target, ViewEditMode viewmode)
        {
            ShowViewParameters svp = new ShowViewParameters();
            DetailView dv = Application.CreateDetailView(os, target);
            dv.ViewEditMode = viewmode;
            dv.IsRoot = true;
            svp.CreatedView = dv;

            Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));

        }
        public void showMsg(string caption, string msg, InformationType msgtype)
        {
            MessageOptions options = new MessageOptions();
            options.Duration = 3000;
            //options.Message = string.Format("{0} task(s) have been successfully updated!", e.SelectedObjects.Count);
            options.Message = string.Format("{0}", msg);
            options.Type = msgtype;
            options.Web.Position = InformationPosition.Right;
            options.Win.Caption = caption;
            options.Win.Type = WinMessageType.Flyout;
            Application.ShowViewStrategy.ShowMessage(options);
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
            newWindow.document.write('<iframe src=""Download.aspx?filename=" + fileInfo.Name + @""" frameborder =""0"" allowfullscreen style=""width: 100%;height: 100%""></iframe>');
            ";
        }

        private void SubmitPR_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
                PurchaseRequest selectedObject = (PurchaseRequest)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                if (p.IsErr) return;

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                if (selectedObject.PurchaseRequestDetail.Count() <= 0)
                {
                    showMsg("Error", "No allow to submit due to no item detail.", InformationType.Error);
                    return;
                }

                if (selectedObject.IsNew == true)
                {
                    IObjectSpace dos = Application.CreateObjectSpace();
                    DocTypes number = dos.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.PurchaseRequests));

                    number.CurrectDocNum = number.NextDocNum;
                    number.NextDocNum = number.NextDocNum + 1;

                    selectedObject.DocNum = "PR" + number.CurrectDocNum;

                    //foreach(DocTypeDocNum dtl in number.DocTypeDocNum)
                    //{
                    //    if (dtl.Entity.EntityCode == selectedObject.Entity.EntityCode)
                    //    {
                    //        dtl.CurrectDocNum = dtl.NextDocNum;
                    //        dtl.NextDocNum = dtl.NextDocNum + 1;

                    //        selectedObject.DocNum = selectedObject.CreateUser.Staff.StaffDepartment.DepartmentCode + dtl.CurrectDocNum;
                    //        break;
                    //    }
                    //}

                    dos.CommitChanges();
                    dos.Refresh();
                }

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                #region Get approval
                List<string> ToEmails = new List<string>();
                string emailbody = "";
                string emailsubject = "";
                string emailaddress = "";
                Guid emailuser;
                DateTime emailtime = DateTime.Now;

                string getapproval = "EXEC sp_GetApproval '" + user.UserName + "', '" + selectedObject.Oid + "', 'PurchaseRequest'";
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();
                SqlCommand cmd = new SqlCommand(getapproval, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    emailbody = "Dear Sir/Madam, " + System.Environment.NewLine + System.Environment.NewLine +
                               reader.GetString(3) + System.Environment.NewLine + GeneralSettings.appurl + reader.GetString(2) +
                               System.Environment.NewLine + System.Environment.NewLine +
                               "Regards" + System.Environment.NewLine +
                               "e-Proc Portal";

                    emailsubject = "Purchase Request Approval";
                    emailaddress = reader.GetString(1);
                    emailuser = reader.GetGuid(0);

                    ToEmails.Add(emailaddress);
                }
                conn.Close();

                if (ToEmails.Count > 0)
                {
                    if (genCon.SendEmail(emailsubject, emailbody, ToEmails) == 1)
                    {
                        foreach (string ToEmail in ToEmails)
                        {
                            string INSEmailLog = "INSERT INTO EmailLog VALUES (GETDATE(), '" + ToEmail + "', '" + emailsubject + "', " +
                            "'" + emailbody + "', 'GetApproval', 'PurchaseRequest', '" + selectedObject.DocNum + "')";
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                            conn.Open();
                            SqlCommand cmdlog = new SqlCommand(INSEmailLog, conn);
                            SqlDataReader readerlog = cmdlog.ExecuteReader();
                            cmdlog.Dispose();
                            conn.Close();
                        }
                    }
                }

                #endregion

                IObjectSpace os = Application.CreateObjectSpace();
                PurchaseRequest prtrx = os.FindObject<PurchaseRequest>(new BinaryOperator("Oid", selectedObject.Oid));

                if (prtrx.ApprovalStatus != ApprovalStatusType.Not_Applicable)
                {
                    prtrx.DocStatus = DocStatus.Submit;
                    PurchaseRequestDocStatus ds = os.CreateObject<PurchaseRequestDocStatus>();
                    ds.DocStatus = DocStatus.Submit;
                    ds.DocRemarks = p.ParamString;
                    prtrx.PurchaseRequestDocStatus.Add(ds);

                    os.CommitChanges();
                    os.Refresh();

                    IObjectSpace tos = Application.CreateObjectSpace();
                    PurchaseRequest trx = tos.FindObject<PurchaseRequest>(new BinaryOperator("Oid", selectedObject.Oid));
                    openNewView(tos, trx, ViewEditMode.View);
                    showMsg("Successful", "Submit Done.", InformationType.Success);
                }
                else
                {
                    showMsg("Error", "Approval criteria no hit.", InformationType.Error);
                }
            }
            else
            {
                showMsg("Fail", "No PR Selected/More Than One PR Selected.", InformationType.Error);
            }
        }

        private void SubmitPR_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void ApprovePR_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count > 1)
            {
                int totaldoc = 0;

                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;
                ApprovalParameters p = (ApprovalParameters)e.PopupWindow.View.CurrentObject;

                if (p.AppStatus == ApprovalActions.No && (p.ParamString == null || p.ParamString == ""))
                {
                    showMsg("Failed", "Please input approval remarks.", InformationType.Error);
                    return;
                }

                foreach (PurchaseRequest dtl in e.SelectedObjects)
                {
                    bool process = true;
                    IObjectSpace pos = Application.CreateObjectSpace();
                    PurchaseRequest pr = pos.FindObject<PurchaseRequest>(new BinaryOperator("Oid", dtl.Oid));

                    if (pr.DocStatus == DocStatus.Submit && pr.ApprovalStatus == ApprovalStatusType.Approved)
                    {
                        showMsg("Failed", "Document already approved, please refresh data.", InformationType.Error);
                        process = false;
                    }

                    if (pr.NextApprover != null)
                    {
                        if (!pr.NextApprover.Contains(user.Staff.StaffName))
                        {
                            showMsg("Failed", "No allow approve document due to you are not the approver.", InformationType.Error);
                            process = false;
                        }
                    }

                    if (process == true)
                    {
                        if (pr.NextApprover != null)
                        {
                            pr.WhoApprove = pr.WhoApprove + user.Staff.StaffName + ", ";
                            ApprovalStatusType appstatus = ApprovalStatusType.Required_Approval;

                            if (appstatus == ApprovalStatusType.Not_Applicable)
                                appstatus = ApprovalStatusType.Required_Approval;


                            if (p.IsErr) return;
                            if (appstatus == ApprovalStatusType.Required_Approval && p.AppStatus == ApprovalActions.NA)
                            {
                                showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                                return;
                            }
                            else if (appstatus == ApprovalStatusType.Approved && p.AppStatus == ApprovalActions.Yes)
                            {
                                showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                                return;
                            }
                            else if (appstatus == ApprovalStatusType.Rejected && p.AppStatus == ApprovalActions.No)
                            {
                                showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                                return;
                            }
                            if (p.AppStatus == ApprovalActions.NA)
                            {
                                appstatus = ApprovalStatusType.Required_Approval;
                            }
                            if (p.AppStatus == ApprovalActions.Yes)
                            {
                                appstatus = ApprovalStatusType.Approved;
                            }
                            if (p.AppStatus == ApprovalActions.No)
                            {
                                appstatus = ApprovalStatusType.Rejected;
                            }

                            PurchaseRequestAppStatus ds = pos.CreateObject<PurchaseRequestAppStatus>();
                            ds.PurchaseRequest = pos.GetObjectByKey<PurchaseRequest>(pr.Oid);
                            ds.AppStatus = appstatus;
                            if (appstatus == ApprovalStatusType.Rejected)
                            {
                                pr.DocStatus = DocStatus.New;
                                ds.AppRemarks = p.ParamString +
                                    System.Environment.NewLine + "(Reject User: " + user.Staff.StaffName + ")" +
                                    System.Environment.NewLine + "(Reason: Approval Rejected)";
                                ds.CreateUser = pos.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                            }
                            else
                            {
                                ds.AppRemarks = p.ParamString +
                                    System.Environment.NewLine + "(Approved User: " + user.Staff.StaffName + ")";
                            }
                            pr.PurchaseRequestAppStatus.Add(ds);

                            pos.CommitChanges();
                            pos.Refresh();

                            totaldoc++;

                            #region approval

                            List<string> ToEmails = new List<string>();
                            string emailbody = "";
                            string emailsubject = "";
                            string emailaddress = "";
                            Guid emailuser;
                            DateTime emailtime = DateTime.Now;

                            string getapproval = "EXEC sp_Approval '" + user.UserName + "', '" + pr.Oid + "', 'PurchaseRequest', " + ((int)appstatus);
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                            conn.Open();
                            SqlCommand cmd = new SqlCommand(getapproval, conn);
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                emailbody = "Dear Sir/Madam, " + System.Environment.NewLine + System.Environment.NewLine +
                                          reader.GetString(3) + System.Environment.NewLine + GeneralSettings.appurl + reader.GetString(2) +
                                          System.Environment.NewLine + System.Environment.NewLine +
                                          "Regards" + System.Environment.NewLine +
                                          "e-Proc Portal";

                                if (appstatus == ApprovalStatusType.Approved)
                                    emailsubject = "Purchase Request Approval";
                                else if (appstatus == ApprovalStatusType.Rejected)
                                    emailsubject = "Purchase Request Approval Rejected";

                                emailaddress = reader.GetString(1);
                                emailuser = reader.GetGuid(0);

                                ToEmails.Add(emailaddress);
                            }
                            conn.Close();

                            if (ToEmails.Count > 0)
                            {
                                if (genCon.SendEmail(emailsubject, emailbody, ToEmails) == 1)
                                {
                                    foreach (string ToEmail in ToEmails)
                                    {
                                        string INSEmailLog = "INSERT INTO EmailLog VALUES (GETDATE(), '" + ToEmail + "', '" + emailsubject + "', " +
                                        "'" + emailbody + "', 'Approval', 'PurchaseRequest', '" + pr.DocNum + "')";
                                        if (conn.State == ConnectionState.Open)
                                        {
                                            conn.Close();
                                        }
                                        conn.Open();
                                        SqlCommand cmdlog = new SqlCommand(INSEmailLog, conn);
                                        SqlDataReader readerlog = cmdlog.ExecuteReader();
                                        cmdlog.Dispose();
                                        conn.Close();
                                    }
                                }
                            }
                            #endregion
                        }
                    }

                    //ObjectSpace.CommitChanges(); //This line persists created object(s).
                    //ObjectSpace.Refresh();

                    showMsg("Info", "Total Document : " + totaldoc + " Approval Done.", InformationType.Info);

                    ((DevExpress.ExpressApp.ListView)Frame.View).ObjectSpace.Refresh();
                }
            }
            else if (e.SelectedObjects.Count == 1)
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;
                ApprovalParameters p = (ApprovalParameters)e.PopupWindow.View.CurrentObject;

                if (p.AppStatus == ApprovalActions.No && (p.ParamString == null || p.ParamString == ""))
                {
                    showMsg("Failed", "Please input approval remarks.", InformationType.Error);
                    return;
                }

                foreach (PurchaseRequest dtl in e.SelectedObjects)
                {
                    IObjectSpace pos = Application.CreateObjectSpace();
                    PurchaseRequest pr = pos.FindObject<PurchaseRequest>(new BinaryOperator("Oid", dtl.Oid));

                    if (pr.DocStatus == DocStatus.Submit && pr.ApprovalStatus == ApprovalStatusType.Approved)
                    {
                        showMsg("Failed", "Document already approved, please refresh data.", InformationType.Error);
                        return;
                    }

                    if (pr.NextApprover != null)
                    {
                        if (!pr.NextApprover.Contains(user.Staff.StaffName))
                        {
                            showMsg("Failed", "No allow approve document due to you are not the approver.", InformationType.Error);
                            return;
                        }
                    }

                    if (pr.NextApprover != null)
                    {
                        pr.WhoApprove = pr.WhoApprove + user.Staff.StaffName + ", ";
                        ApprovalStatusType appstatus = ApprovalStatusType.Required_Approval;

                        if (appstatus == ApprovalStatusType.Not_Applicable)
                            appstatus = ApprovalStatusType.Required_Approval;


                        if (p.IsErr) return;
                        if (appstatus == ApprovalStatusType.Required_Approval && p.AppStatus == ApprovalActions.NA)
                        {
                            showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                            return;
                        }
                        else if (appstatus == ApprovalStatusType.Approved && p.AppStatus == ApprovalActions.Yes)
                        {
                            showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                            return;
                        }
                        else if (appstatus == ApprovalStatusType.Rejected && p.AppStatus == ApprovalActions.No)
                        {
                            showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                            return;
                        }
                        if (p.AppStatus == ApprovalActions.NA)
                        {
                            appstatus = ApprovalStatusType.Required_Approval;
                        }
                        if (p.AppStatus == ApprovalActions.Yes)
                        {
                            appstatus = ApprovalStatusType.Approved;
                        }
                        if (p.AppStatus == ApprovalActions.No)
                        {
                            appstatus = ApprovalStatusType.Rejected;
                        }

                        PurchaseRequestAppStatus ds = pos.CreateObject<PurchaseRequestAppStatus>();
                        ds.PurchaseRequest = pos.GetObjectByKey<PurchaseRequest>(pr.Oid);
                        ds.AppStatus = appstatus;
                        if (appstatus == ApprovalStatusType.Rejected)
                        {
                            pr.DocStatus = DocStatus.New;
                            ds.AppRemarks = p.ParamString +
                                System.Environment.NewLine + "(Reject User: " + user.Staff.StaffName + ")" +
                                System.Environment.NewLine + "(Reason: Approval Rejected)";
                            ds.CreateUser = pos.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                        }
                        else
                        {
                            ds.AppRemarks = p.ParamString +
                                System.Environment.NewLine + "(Approved User: " + user.Staff.StaffName + ")";
                        }
                        pr.PurchaseRequestAppStatus.Add(ds);

                        pos.CommitChanges();
                        pos.Refresh();

                        #region approval

                        List<string> ToEmails = new List<string>();
                        string emailbody = "";
                        string emailsubject = "";
                        string emailaddress = "";
                        Guid emailuser;
                        DateTime emailtime = DateTime.Now;

                        string getapproval = "EXEC sp_Approval '" + user.UserName + "', '" + pr.Oid + "', 'PurchaseRequest', " + ((int)appstatus);
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(getapproval, conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            emailbody = "Dear Sir/Madam, " + System.Environment.NewLine + System.Environment.NewLine +
                                      reader.GetString(3) + System.Environment.NewLine + GeneralSettings.appurl + reader.GetString(2) +
                                      System.Environment.NewLine + System.Environment.NewLine +
                                      "Regards" + System.Environment.NewLine +
                                      "e-Proc Portal";

                            if (appstatus == ApprovalStatusType.Approved)
                                emailsubject = "Purchase Request Approval";
                            else if (appstatus == ApprovalStatusType.Rejected)
                                emailsubject = "Purchase Request Approval Rejected";

                            emailaddress = reader.GetString(1);
                            emailuser = reader.GetGuid(0);

                            ToEmails.Add(emailaddress);
                        }
                        conn.Close();

                        if (ToEmails.Count > 0)
                        {
                            if (genCon.SendEmail(emailsubject, emailbody, ToEmails) == 1)
                            {
                                foreach (string ToEmail in ToEmails)
                                {
                                    string INSEmailLog = "INSERT INTO EmailLog VALUES (GETDATE(), '" + ToEmail + "', '" + emailsubject + "', " +
                                    "'" + emailbody + "', 'Approval', 'PurchaseRequest', '" + pr.DocNum + "')";
                                    if (conn.State == ConnectionState.Open)
                                    {
                                        conn.Close();
                                    }
                                    conn.Open();
                                    SqlCommand cmdlog = new SqlCommand(INSEmailLog, conn);
                                    SqlDataReader readerlog = cmdlog.ExecuteReader();
                                    cmdlog.Dispose();
                                    conn.Close();
                                }
                            }
                        }
                        #endregion

                        IObjectSpace tos = Application.CreateObjectSpace();
                        PurchaseRequest trx = tos.FindObject<PurchaseRequest>(new BinaryOperator("Oid", pr.Oid));
                        openNewView(tos, trx, ViewEditMode.View);
                        showMsg("Successful", "Approve Done.", InformationType.Success);
                    }
                }
            }
            else
            {
                showMsg("Fail", "No PR selected.", InformationType.Error);
            }
        }

        private void ApprovePR_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            bool err = false;
            PurchaseRequest selectedObject = (PurchaseRequest)View.CurrentObject;

            SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

            ApprovalStatusType appstatus = ApprovalStatusType.Required_Approval;

            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<ApprovalParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            switch (appstatus)
            {
                case ApprovalStatusType.Required_Approval:
                    ((ApprovalParameters)dv.CurrentObject).AppStatus = ApprovalActions.NA;
                    break;
                case ApprovalStatusType.Approved:
                    ((ApprovalParameters)dv.CurrentObject).AppStatus = ApprovalActions.Yes;
                    break;
                case ApprovalStatusType.Rejected:
                    ((ApprovalParameters)dv.CurrentObject).AppStatus = ApprovalActions.No;
                    break;
            }
            ((ApprovalParameters)dv.CurrentObject).IsErr = err;
            ((ApprovalParameters)dv.CurrentObject).ActionMessage = "Press Choose From Approval Status 'Approve or Reject' and Press OK to CONFIRM the action and SAVE else press Cancel.";

            e.View = dv;
        }

        private void CancelPR_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                PurchaseRequest selectedObject = (PurchaseRequest)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                if (p.IsErr) return;

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                selectedObject.DocStatus = DocStatus.Cancel;
                PurchaseRequestDocStatus ds = ObjectSpace.CreateObject<PurchaseRequestDocStatus>();
                ds.DocStatus = DocStatus.Cancel;
                ds.DocRemarks = p.ParamString;
                selectedObject.PurchaseRequestDocStatus.Add(ds);

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                IObjectSpace os = Application.CreateObjectSpace();
                PurchaseRequest prtrx = os.FindObject<PurchaseRequest>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, prtrx, ViewEditMode.View);
                showMsg("Successful", "Cancel Done.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "No PR Selected/More Than One PR Selected.", InformationType.Error);
            }
        }

        private void CancelPR_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void PrintPR_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string strServer;
            string strDatabase;
            string strUserID;
            string strPwd;
            string filename;

            if (e.SelectedObjects.Count == 1)
            {
                PurchaseRequest pr = (PurchaseRequest)View.CurrentObject;
                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                try
                {
                    ReportDocument doc = new ReportDocument();
                    strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                    doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\Purchase Request.rpt"));
                    strDatabase = ConfigurationManager.AppSettings.Get("ReportDB").ToString();
                    strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                    strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                    doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                    doc.Refresh();

                    doc.SetParameterValue("@DocNum", pr.DocNum);

                    filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + ConfigurationManager.AppSettings.Get("ReportDB").ToString()
                        + "_" + pr.DocNum + "_" + user.UserName + "_"
                        + DateTime.Parse(pr.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

                    doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                    doc.Close();
                    doc.Dispose();

                    WebWindow.CurrentRequestWindow.RegisterStartupScript("DownloadFile", GetScript(filename));
                }
                catch (Exception ex)
                {
                    genCon.showMsg("Fail", ex.Message, InformationType.Error);
                }
            }
            else if (e.SelectedObjects.Count > 1)
            {
                genCon.showMsg("Fail", "More than one Transfer In Selected.", InformationType.Error);
            }
            else
            {
                genCon.showMsg("Fail", "No Transfer In Selected.", InformationType.Error);
            }
        }
    }
}
