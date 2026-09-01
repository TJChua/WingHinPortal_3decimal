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
using WingHinPortal.Module.BusinessObjects.PO;
using WingHinPortal.Module.BusinessObjects.PR;
using WingHinPortal.Module.BusinessObjects.Setup;
using WingHinPortal.Module.BusinessObjects.View;

// 20260512 - add cost center 2 - ver 1.0.2

namespace WingHinPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class POControllers : ViewController
    {
        GenControllers genCon;
        public POControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
            TargetObjectType = typeof(PurchaseOrders);
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

            if (View.Id == "PurchaseOrders_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.Edit)
                {
                    this.CopyFromPR.Active.SetItemValue("Enabled", true);
                    this.SubmitPO.Active.SetItemValue("Enabled", false);
                    this.CancelPO.Active.SetItemValue("Enabled", false);
                    this.ClosePO.Active.SetItemValue("Enabled", false);
                    this.PrintPO.Active.SetItemValue("Enabled", false);
                    this.EmailSupplier.Active.SetItemValue("Enabled", false);
                }
                else
                {
                    this.CopyFromPR.Active.SetItemValue("Enabled", false);
                    this.SubmitPO.Active.SetItemValue("Enabled", true);
                    this.CancelPO.Active.SetItemValue("Enabled", true);
                    this.ClosePO.Active.SetItemValue("Enabled", true);
                    this.PrintPO.Active.SetItemValue("Enabled", true);
                    this.EmailSupplier.Active.SetItemValue("Enabled", true);
                }
            }
            else
            {
                this.SubmitPO.Active.SetItemValue("Enabled", false);
                this.CancelPO.Active.SetItemValue("Enabled", false);
                this.ClosePO.Active.SetItemValue("Enabled", false);
                this.PrintPO.Active.SetItemValue("Enabled", false);
                this.EmailSupplier.Active.SetItemValue("Enabled", false);
                this.CopyFromPR.Active.SetItemValue("Enabled", false);
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

        private void SubmitPO_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
                PurchaseOrders selectedObject = (PurchaseOrders)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                int attachment = 0;
                if (p.IsErr) return;

                if (selectedObject.ExpenditureType.ExpenditureTypeName == "Fixed Assets")
                {
                    foreach (PurchaseOrderAttachment att in selectedObject.PurchaseOrderAttachment)
                    {
                        attachment = attachment + 1;
                    }

                    if (attachment == 0)
                    {
                        showMsg("Error", "Fixed Assets expentidure type must attach document.", InformationType.Error);
                        return;
                    }
                }

                if (selectedObject.PurchaseOrderDetails.Count() <= 0)
                {
                    showMsg("Error", "No allow to submit due to no item detail.", InformationType.Error);
                    return;
                }

                if (selectedObject.Total <= 0)
                {
                    showMsg("Error", "No allow to submit due to zero total.", InformationType.Error);
                    return;
                }

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                if (selectedObject.IsNew == true)
                {
                    IObjectSpace dos = Application.CreateObjectSpace();
                    DocTypes number = dos.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.PurchaseOrders));

                    number.CurrectDocNum = number.NextDocNum;
                    number.NextDocNum = number.NextDocNum + 1;

                    selectedObject.DocNum = "PO" + number.CurrectDocNum;

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

                string getapproval = "EXEC sp_GetApproval '" + user.UserName + "', '" + selectedObject.Oid + "', 'PurchaseOrders'";
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

                    emailsubject = "Purchase Order Approval";
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
                            "'" + emailbody + "', 'GetApproval', 'PurchaseOrder', '" + selectedObject.DocNum + "')";
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

                foreach (PurchaseOrderDetails podetail in selectedObject.PurchaseOrderDetails)
                {
                    IObjectSpace pos = Application.CreateObjectSpace();
                    PurchaseRequestDetails pr = pos.FindObject<PurchaseRequestDetails>(new BinaryOperator("Oid", podetail.BaseOid));

                    if (pr != null)
                    {
                        pr.OpenQuantity += podetail.OpenQuantity - podetail.Quantity;
                        pos.CommitChanges();
                    }
                }

                IObjectSpace os = Application.CreateObjectSpace();
                PurchaseOrders potrx = os.FindObject<PurchaseOrders>(new BinaryOperator("Oid", selectedObject.Oid));

                if (potrx.ApprovalStatus != ApprovalStatusType.Not_Applicable)
                {
                    potrx.DocStatus = DocStatus.Submit;
                    PurchaseOrderDocStatus ds = os.CreateObject<PurchaseOrderDocStatus>();
                    ds.DocStatus = DocStatus.Submit;
                    ds.DocRemarks = p.ParamString;
                    potrx.PurchaseOrderDocStatus.Add(ds);

                    os.CommitChanges();
                    os.Refresh();

                    IObjectSpace tos = Application.CreateObjectSpace();
                    PurchaseOrders trx = tos.FindObject<PurchaseOrders>(new BinaryOperator("Oid", selectedObject.Oid));
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
                showMsg("Fail", "No PO Selected/More Than One PO Selected.", InformationType.Error);
            }
        }

        private void SubmitPO_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void ApprovePO_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
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

                foreach (PurchaseOrders dtl in e.SelectedObjects)
                {
                    bool process = true;
                    IObjectSpace pos = Application.CreateObjectSpace();
                    PurchaseOrders po = pos.FindObject<PurchaseOrders>(new BinaryOperator("Oid", dtl.Oid));

                    if (po.DocStatus == DocStatus.Submit && po.ApprovalStatus == ApprovalStatusType.Approved)
                    {
                        showMsg("Failed", "Document already approved, please refresh data.", InformationType.Error);
                        process = false;
                    }

                    if (po.NextApprover != null)
                    {
                        if (!po.NextApprover.Contains(user.Staff.StaffName))
                        {
                            showMsg("Failed", "No allow approve document due to you are not the approver.", InformationType.Error);
                            process = false;
                        }
                    }

                    if (process == true)
                    {
                        if (po.NextApprover != null)
                        {
                            po.WhoApprove = po.WhoApprove + user.Staff.StaffName + ", ";
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

                            PurchaseOrderAppStatus ds = pos.CreateObject<PurchaseOrderAppStatus>();
                            ds.PurchaseOrders = pos.GetObjectByKey<PurchaseOrders>(po.Oid);
                            ds.AppStatus = appstatus;
                            if (appstatus == ApprovalStatusType.Rejected)
                            {
                                po.DocStatus = DocStatus.New;
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
                            po.PurchaseOrderAppStatus.Add(ds);

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

                            string getapproval = "EXEC sp_Approval '" + user.UserName + "', '" + po.Oid + "', 'PurchaseOrders', " + ((int)appstatus);
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
                                    emailsubject = "Purchase Order Approval";
                                else if (appstatus == ApprovalStatusType.Rejected)
                                    emailsubject = "Purchase Order Approval Rejected";

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
                                        "'" + emailbody + "', 'Approval', 'PurchaseOrder', '" + po.DocNum + "')";
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
            else if (e.SelectedObjects.Count >= 1)
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;
                ApprovalParameters p = (ApprovalParameters)e.PopupWindow.View.CurrentObject;

                if (p.AppStatus == ApprovalActions.No && (p.ParamString == null || p.ParamString == ""))
                {
                    showMsg("Failed", "Please input approval remarks.", InformationType.Error);
                    return;
                }

                foreach (PurchaseOrders dtl in e.SelectedObjects)
                {
                    IObjectSpace pos = Application.CreateObjectSpace();
                    PurchaseOrders po = pos.FindObject<PurchaseOrders>(new BinaryOperator("Oid", dtl.Oid));

                    if (po.DocStatus == DocStatus.Submit && po.ApprovalStatus == ApprovalStatusType.Approved)
                    {
                        showMsg("Failed", "Document already approved, please refresh data.", InformationType.Error);
                        return;
                    }

                    if (po.NextApprover != null)
                    {
                        if (!po.NextApprover.Contains(user.Staff.StaffName))
                        {
                            showMsg("Failed", "No allow approve document due to you are not the approver.", InformationType.Error);
                            return;
                        }
                    }

                    if (po.NextApprover != null)
                    {
                        po.WhoApprove = po.WhoApprove + user.Staff.StaffName + ", ";
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

                        PurchaseOrderAppStatus ds = pos.CreateObject<PurchaseOrderAppStatus>();
                        ds.PurchaseOrders = pos.GetObjectByKey<PurchaseOrders>(po.Oid);
                        ds.AppStatus = appstatus;
                        if (appstatus == ApprovalStatusType.Rejected)
                        {
                            po.DocStatus = DocStatus.New;
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
                        po.PurchaseOrderAppStatus.Add(ds);

                        pos.CommitChanges();
                        pos.Refresh();

                        #region approval

                        List<string> ToEmails = new List<string>();
                        string emailbody = "";
                        string emailsubject = "";
                        string emailaddress = "";
                        Guid emailuser;
                        DateTime emailtime = DateTime.Now;

                        string getapproval = "EXEC sp_Approval '" + user.UserName + "', '" + po.Oid + "', 'PurchaseOrders', " + ((int)appstatus);
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
                                emailsubject = "Purchase Order Approval";
                            else if (appstatus == ApprovalStatusType.Rejected)
                                emailsubject = "Purchase Order Approval Rejected";

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
                                    "'" + emailbody + "', 'Approval', 'PurchaseOrder', '" + po.DocNum + "')";
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
                        PurchaseOrders trx = tos.FindObject<PurchaseOrders>(new BinaryOperator("Oid", po.Oid));
                        openNewView(tos, trx, ViewEditMode.View);
                        showMsg("Successful", "Approve Done.", InformationType.Success);
                    }
                }
            }
            else
            {
                showMsg("Fail", "No PO selected.", InformationType.Error);
            }
        }

        private void ApprovePO_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            bool err = false;
            PurchaseOrders selectedObject = (PurchaseOrders)View.CurrentObject;

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

        private void CancelPO_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                PurchaseOrders selectedObject = (PurchaseOrders)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                if (p.IsErr) return;

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                selectedObject.DocStatus = DocStatus.Cancel;
                PurchaseOrderDocStatus ds = ObjectSpace.CreateObject<PurchaseOrderDocStatus>();
                ds.DocStatus = DocStatus.Cancel;
                ds.DocRemarks = p.ParamString;
                selectedObject.PurchaseOrderDocStatus.Add(ds);

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                foreach (PurchaseOrderDetails podetail in selectedObject.PurchaseOrderDetails)
                {
                    IObjectSpace pos = Application.CreateObjectSpace();
                    PurchaseRequestDetails pr = pos.FindObject<PurchaseRequestDetails>(new BinaryOperator("Oid", podetail.BaseOid));

                    if (pr != null)
                    {
                        pr.OpenQuantity += podetail.OpenQuantity;
                        pos.CommitChanges();
                    }
                }

                IObjectSpace os = Application.CreateObjectSpace();
                PurchaseOrders potrx = os.FindObject<PurchaseOrders>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, potrx, ViewEditMode.View);
                showMsg("Successful", "Cancel Done.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "No PO Selected/More Than One PO Selected.", InformationType.Error);
            }
        }

        private void CancelPO_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CopyFromPR_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count > 0)
            {
                try
                {
                    string temp = ConfigurationManager.AppSettings["Internal"].ToString().ToUpper();
                    if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                    {
                        IObjectSpace os = Application.CreateObjectSpace();
                        PurchaseOrders newPO = os.CreateObject<PurchaseOrders>();

                        foreach (vwPRInternalPO dtl in e.PopupWindowViewSelectedObjects)
                        {
                            newPO.VendorCode = newPO.Session.GetObjectByKey<vwVendors>(dtl.VendorCode);

                            PurchaseOrderDetails newPOItem = os.CreateObject<PurchaseOrderDetails>();

                            newPOItem.Item = newPOItem.Session.FindObject<vwItemMasters>(CriteriaOperator.Parse("ItemCode = ?", dtl.Item));
                            newPOItem.ItemDesc = dtl.ItemDesc;
                            newPOItem.ItemDetails = dtl.ItemDetails;
                            newPOItem.Unitprice = dtl.Unitprice;
                            newPOItem.BaseDoc = dtl.DocNum;

                            PurchaseRequest master = ObjectSpace.FindObject<PurchaseRequest>(CriteriaOperator.Parse("DocNum = ?", dtl.DocNum));

                            if (master.ExpenditureType != null)
                            {
                                newPO.ExpenditureType = newPO.Session.GetObjectByKey<ExpenditureType>(master.ExpenditureType.Oid);
                            }
                            if (master.CompanyAddress != null)
                            {
                                newPO.CompanyAddress = newPO.Session.GetObjectByKey<CompanyAddress>(master.CompanyAddress.Oid);
                            }
                            newPO.Remarks = master.Remarks;
                            newPO.Attn = master.Attn;

                            foreach (PurchaseRequestDetails detail in master.PurchaseRequestDetail)
                            {
                                if (dtl.Item == detail.Item.ItemCode)
                                {
                                    newPOItem.OpenQuantity = detail.OpenQuantity;
                                    newPOItem.Quantity = detail.OpenQuantity;
                                    if (detail.Tax != null)
                                    {
                                        newPOItem.Tax = newPOItem.Session.GetObjectByKey<vwTax>(detail.Tax.BoCode);
                                    }
                                    if (detail.ExpenditureType != null)
                                    {
                                        newPOItem.ExpenditureType = newPOItem.Session.GetObjectByKey<ExpenditureType>(master.ExpenditureType.Oid);
                                    }
                                    if (detail.ItemGroup != null)
                                    {
                                        newPOItem.ItemGroup = newPOItem.Session.GetObjectByKey<vwItemGroup>(detail.ItemGroup.Code);
                                    }
                                    if (detail.CostCenter != null)
                                    {
                                        newPOItem.CostCenter = newPOItem.Session.GetObjectByKey<vwCostCenter>(detail.CostCenter.PrcCode);
                                    }
                                    // Start ver 1.0.2
                                    if (detail.SubCostCenter != null)
                                    {
                                        newPOItem.SubCostCenter = newPOItem.Session.GetObjectByKey<vwSubCostCenter>(detail.SubCostCenter.PrcCode);
                                    }
                                    // End ver 1.0.2
                                    newPOItem.BaseOid = detail.Oid.ToString();
                                    newPOItem.Discount = detail.Discount;
                                    newPOItem.Vehicle = detail.Vehicle;
                                }
                            }

                            newPO.PurchaseOrderDetails.Add(newPOItem);
                        }

                        ShowViewParameters svp = new ShowViewParameters();
                        DetailView dv = Application.CreateDetailView(os, newPO);
                        dv.ViewEditMode = ViewEditMode.Edit;
                        dv.IsRoot = true;
                        svp.CreatedView = dv;

                        Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
                        showMsg("Success", "Copy Success.", InformationType.Success);
                    }
                    else
                    {
                        IObjectSpace os = Application.CreateObjectSpace();
                        PurchaseOrders newPO = os.CreateObject<PurchaseOrders>();

                        foreach (vwPR dtl in e.PopupWindowViewSelectedObjects)
                        {
                            newPO.VendorCode = newPO.Session.GetObjectByKey<vwVendors>(dtl.VendorCode);

                            PurchaseOrderDetails newPOItem = os.CreateObject<PurchaseOrderDetails>();

                            newPOItem.Item = newPOItem.Session.FindObject<vwItemMasters>(CriteriaOperator.Parse("ItemCode = ?", dtl.Item));
                            newPOItem.ItemDesc = dtl.ItemDesc;
                            newPOItem.ItemDetails = dtl.ItemDetails;
                            newPOItem.Unitprice = dtl.Unitprice;
                            newPOItem.BaseDoc = dtl.DocNum;

                            PurchaseRequest master = ObjectSpace.FindObject<PurchaseRequest>(CriteriaOperator.Parse("DocNum = ?", dtl.DocNum));

                            if (master.ExpenditureType != null)
                            {
                                newPO.ExpenditureType = newPO.Session.GetObjectByKey<ExpenditureType>(master.ExpenditureType.Oid);
                            }
                            if (master.CompanyAddress != null)
                            {
                                newPO.CompanyAddress = newPO.Session.GetObjectByKey<CompanyAddress>(master.CompanyAddress.Oid);
                            }

                            foreach (PurchaseRequestDetails detail in master.PurchaseRequestDetail)
                            {
                                if (dtl.Item == detail.Item.ItemCode)
                                {
                                    newPOItem.OpenQuantity = detail.OpenQuantity;
                                    newPOItem.Quantity = detail.OpenQuantity;
                                    if (detail.Tax != null)
                                    {
                                        newPOItem.Tax = newPOItem.Session.GetObjectByKey<vwTax>(detail.Tax.BoCode);
                                    }
                                    if (detail.ExpenditureType != null)
                                    {
                                        newPOItem.ExpenditureType = newPOItem.Session.GetObjectByKey<ExpenditureType>(master.ExpenditureType.Oid);
                                    }
                                    if (detail.ItemGroup != null)
                                    {
                                        newPOItem.ItemGroup = newPOItem.Session.GetObjectByKey<vwItemGroup>(detail.ItemGroup.Code);
                                    }
                                    if (detail.CostCenter != null)
                                    {
                                        newPOItem.CostCenter = newPOItem.Session.GetObjectByKey<vwCostCenter>(detail.CostCenter.PrcCode);
                                    }
                                    // Start ver 1.0.2
                                    if (detail.SubCostCenter != null)
                                    {
                                        newPOItem.SubCostCenter = newPOItem.Session.GetObjectByKey<vwSubCostCenter>(detail.SubCostCenter.PrcCode);
                                    }
                                    // End ver 1.0.2
                                    newPOItem.BaseOid = detail.Oid.ToString();
                                    newPOItem.Discount = detail.Discount;
                                    newPOItem.Vehicle = detail.Vehicle;
                                }
                            }

                            newPO.PurchaseOrderDetails.Add(newPOItem);
                        }

                        ShowViewParameters svp = new ShowViewParameters();
                        DetailView dv = Application.CreateDetailView(os, newPO);
                        dv.ViewEditMode = ViewEditMode.Edit;
                        dv.IsRoot = true;
                        svp.CreatedView = dv;

                        Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
                        showMsg("Success", "Copy Success.", InformationType.Success);
                    }
                }
                catch (Exception)
                {
                    showMsg("Fail", "Copy Fail.", InformationType.Error);
                }
            }
            else
            {
                showMsg("Fail", "No PR Selected.", InformationType.Error);
            }
        }

        private void CopyFromPR_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            PurchaseOrders purchaseorder = (PurchaseOrders)View.CurrentObject;

            string temp = ConfigurationManager.AppSettings["Internal"].ToString().ToUpper();
            if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
            {
                var os = Application.CreateObjectSpace();
                var viewId = Application.FindListViewId(typeof(vwPRInternalPO));
                var cs = Application.CreateCollectionSource(os, typeof(vwPRInternalPO), viewId);
                if (purchaseorder.VendorCode != null)
                {
                    cs.Criteria["VendorCode"] = new BinaryOperator("VendorCode", purchaseorder.VendorCode.CardCode);
                }
                var lv1 = Application.CreateListView(viewId, cs, true);
                e.View = lv1;
            }
            else
            {
                var os = Application.CreateObjectSpace();
                var viewId = Application.FindListViewId(typeof(vwPR));
                var cs = Application.CreateCollectionSource(os, typeof(vwPR), viewId);
                if (purchaseorder.VendorCode != null)
                {
                    cs.Criteria["VendorCode"] = new BinaryOperator("VendorCode", purchaseorder.VendorCode.CardCode);
                }
                var lv1 = Application.CreateListView(viewId, cs, true);
                e.View = lv1;
            }
        }

        private void PrintPO_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            string strServer;
            string strDatabase;
            string strUserID;
            string strPwd;
            string filename;

            if (e.SelectedObjects.Count == 1)
            {
                PurchaseOrders po = (PurchaseOrders)View.CurrentObject;
                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                try
                {
                    ReportDocument doc = new ReportDocument();
                    strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                    doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\Purchase Order.rpt"));
                    strDatabase = ConfigurationManager.AppSettings.Get("ReportDB").ToString();
                    strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                    strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                    doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                    doc.Refresh();

                    doc.SetParameterValue("@DocNum", po.DocNum);

                    filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + ConfigurationManager.AppSettings.Get("ReportDB").ToString()
                        + "_" + po.DocNum + "_" + user.UserName + "_"
                        + DateTime.Parse(po.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

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

        private void EmailSupplier_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            string strServer;
            string strDatabase;
            string strUserID;
            string strPwd;
            string filename = null;
            SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;
            PurchaseOrders selectedObject = (PurchaseOrders)e.CurrentObject;
            EmailForm email = (EmailForm)e.PopupWindow.View.CurrentObject;

            if (email.Email != null)
            {
                #region get layout
                try
                {
                    ReportDocument doc = new ReportDocument();
                    strServer = ConfigurationManager.AppSettings.Get("SQLserver").ToString();
                    doc.Load(HttpContext.Current.Server.MapPath("~\\Reports\\Purchase Order.rpt"));
                    strDatabase = ConfigurationManager.AppSettings.Get("ReportDB").ToString();
                    strUserID = ConfigurationManager.AppSettings.Get("SQLID").ToString();
                    strPwd = ConfigurationManager.AppSettings.Get("SQLPass").ToString();
                    doc.DataSourceConnections[0].SetConnection(strServer, strDatabase, strUserID, strPwd);
                    doc.Refresh();

                    doc.SetParameterValue("@DocNum", selectedObject.DocNum);

                    filename = ConfigurationManager.AppSettings.Get("ReportPath").ToString() + ConfigurationManager.AppSettings.Get("ReportDB").ToString()
                        + "_" + selectedObject.DocNum + "_" + user.UserName + "_"
                        + DateTime.Parse(selectedObject.DocDate.ToString()).ToString("yyyyMMdd") + ".pdf";

                    doc.ExportToDisk(ExportFormatType.PortableDocFormat, filename);
                    doc.Close();
                    doc.Dispose();
                }
                catch (Exception ex)
                {
                    genCon.showMsg("Fail", ex.Message, InformationType.Error);
                }
                #endregion

                #region email
                List<string> ToEmails = new List<string>();
                string emailbody = "";
                string emailsubject = "";
                string emailaddress = "";
                Guid emailuser;
                DateTime emailtime = DateTime.Now;

                emailbody = email.Message;
                emailsubject = email.Subject;

                emailaddress = email.Email;
                emailuser = user.Oid;

                ToEmails.Add(emailaddress);

                if (ToEmails.Count > 0)
                {
                    if (genCon.SupplierSendEmail(emailsubject, emailbody, ToEmails, filename) == 1)
                    {
                    }
                }
                #endregion

                showMsg("Success", "Email sent.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "Incorrect email.", InformationType.Error);
            }
        }

        private void EmailSupplier_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            PurchaseOrders po = (PurchaseOrders)View.CurrentObject;

            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<EmailForm>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((EmailForm)dv.CurrentObject).IsErr = false;
            ((EmailForm)dv.CurrentObject).Email = po.VendorCode.Emails;

            e.View = dv;
        }

        private void ClosePO_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                PurchaseOrders selectedObject = (PurchaseOrders)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                if (p.IsErr) return;

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                selectedObject.DocStatus = DocStatus.Closed;
                PurchaseOrderDocStatus ds = ObjectSpace.CreateObject<PurchaseOrderDocStatus>();
                ds.DocStatus = DocStatus.Closed;
                ds.DocRemarks = p.ParamString;
                selectedObject.PurchaseOrderDocStatus.Add(ds);

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                IObjectSpace os = Application.CreateObjectSpace();
                PurchaseOrders potrx = os.FindObject<PurchaseOrders>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, potrx, ViewEditMode.View);
                showMsg("Successful", "Cancel Done.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "No PO Selected/More Than One PO Selected.", InformationType.Error);
            }
        }

        private void ClosePO_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }
    }
}
