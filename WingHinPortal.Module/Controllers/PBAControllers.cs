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
using WingHinPortal.Module.BusinessObjects.PurchaseBlanketAgreement;
using WingHinPortal.Module.BusinessObjects.Setup;

namespace WingHinPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class PBAControllers : ViewController
    {
        public PBAControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
            TargetObjectType = typeof(PurchaseBlanketAgreement);
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
            if (View.Id == "PurchaseBlanketAgreement_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.Edit)
                {
                    this.SubmitPBA.Active.SetItemValue("Enabled", false);
                    this.CancelPBA.Active.SetItemValue("Enabled", false);
                    this.TerminatePBA.Active.SetItemValue("Enabled", false);
                }
                else
                {
                    this.SubmitPBA.Active.SetItemValue("Enabled", true);
                    this.CancelPBA.Active.SetItemValue("Enabled", true);
                    this.TerminatePBA.Active.SetItemValue("Enabled", true);
                }
            }
            else
            {
                this.SubmitPBA.Active.SetItemValue("Enabled", false);
                this.CancelPBA.Active.SetItemValue("Enabled", false);
                this.TerminatePBA.Active.SetItemValue("Enabled", false);
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

        private void SubmitPBA_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                PurchaseBlanketAgreement selectedObject = (PurchaseBlanketAgreement)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                int attachment = 0;
                if (p.IsErr) return;

                foreach (PurchaseBlanketAgreementAttachment att in selectedObject.PurchaseBlanketAgreementAttachment)
                {
                    attachment = attachment + 1;
                }

                if (attachment == 0)
                {
                    showMsg("Error", "No attachment uploaded.", InformationType.Error);
                    return;
                }

                if (selectedObject.PurchaseBlanketAgreementDetails.Count() <= 0)
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

                selectedObject.DocStatus = DocStatus.Submit;
                PurchaseBlanketAgreementDocStatus ds = ObjectSpace.CreateObject<PurchaseBlanketAgreementDocStatus>();
                ds.DocStatus = DocStatus.Submit;
                ds.DocRemarks = p.ParamString;
                selectedObject.PurchaseBlanketAgreementDocStatus.Add(ds);

                if (selectedObject.IsNew == true)
                {
                    IObjectSpace dos = Application.CreateObjectSpace();
                    DocTypes number = dos.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.BA));

                    number.CurrectDocNum = number.NextDocNum;
                    number.NextDocNum = number.NextDocNum + 1;

                    selectedObject.DocNum = "PBA" + number.CurrectDocNum;

                    dos.CommitChanges();
                    dos.Refresh();
                }

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                IObjectSpace os = Application.CreateObjectSpace();
                PurchaseBlanketAgreement potrx = os.FindObject<PurchaseBlanketAgreement>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, potrx, ViewEditMode.View);
                showMsg("Successful", "Submit Done.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "No blanket agreement Selected/More Than One blanket agreement Selected.", InformationType.Error);
            }
        }

        private void SubmitPBA_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CancelPBA_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                PurchaseBlanketAgreement selectedObject = (PurchaseBlanketAgreement)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                if (p.IsErr) return;

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                selectedObject.DocStatus = DocStatus.Cancel;
                PurchaseBlanketAgreementDocStatus ds = ObjectSpace.CreateObject<PurchaseBlanketAgreementDocStatus>();
                ds.DocStatus = DocStatus.Cancel;
                ds.DocRemarks = p.ParamString;
                selectedObject.PurchaseBlanketAgreementDocStatus.Add(ds);

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                IObjectSpace os = Application.CreateObjectSpace();
                PurchaseBlanketAgreement potrx = os.FindObject<PurchaseBlanketAgreement>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, potrx, ViewEditMode.View);
                showMsg("Successful", "Cancel Done.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "No blanket agreement Selected/More Than One blanket agreement Selected.", InformationType.Error);
            }
        }

        private void CancelPBA_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void TerminatePBA_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                PurchaseBlanketAgreement selectedObject = (PurchaseBlanketAgreement)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                if (p.IsErr) return;

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                if (selectedObject.PurchaseBlanketAgreementAttachment.Count() <= 0)
                {
                    showMsg("Error", "Please attach attachment before terminate.", InformationType.Error);
                    return;
                }

                if (selectedObject.TerminateDate < DateTime.Today)
                {
                    showMsg("Error", "No date/No back date.", InformationType.Error);
                    return;
                }

                selectedObject.DocStatus = DocStatus.Terminated;
                selectedObject.TerminateDate = DateTime.Now;
                PurchaseBlanketAgreementDocStatus ds = ObjectSpace.CreateObject<PurchaseBlanketAgreementDocStatus>();
                ds.DocStatus = DocStatus.Terminated;
                ds.DocRemarks = p.ParamString;
                selectedObject.PurchaseBlanketAgreementDocStatus.Add(ds);

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                IObjectSpace os = Application.CreateObjectSpace();
                PurchaseBlanketAgreement potrx = os.FindObject<PurchaseBlanketAgreement>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, potrx, ViewEditMode.View);
                showMsg("Successful", "Terminate Done.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "No blanket agreement Selected/More Than One blanket agreement Selected.", InformationType.Error);
            }
        }

        private void TerminatePBA_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
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
