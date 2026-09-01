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
using DevExpress.ExpressApp.Web.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using WingHinPortal.Module.BusinessObjects;
using WingHinPortal.Module.BusinessObjects.GoodsIssue;
using WingHinPortal.Module.BusinessObjects.GoodsReceipt;
using WingHinPortal.Module.BusinessObjects.PO;
using WingHinPortal.Module.BusinessObjects.PR;
using WingHinPortal.Module.BusinessObjects.PurchaseBlanketAgreement;
using WingHinPortal.Module.BusinessObjects.Setup;

namespace WingHinPortal.Module.Web.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class WebModificationControllers : WebModificationsController
    {
        public WebModificationControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            Frame.GetController<ModificationsController>().SaveAndNewAction.Active.SetItemValue("Enabled", false);
            Frame.GetController<ModificationsController>().SaveAndCloseAction.Active.SetItemValue("Enabled", false);
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
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

        protected override void Save(SimpleActionExecuteEventArgs args)
        {
            SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;
            if (View.ObjectTypeInfo.Type == typeof(PurchaseRequest))
            {
                PurchaseRequest newPR = (PurchaseRequest)args.CurrentObject;
                if (newPR.IsNew == true)
                {
                    //if (newPR.IsValid == false)
                    //{
                    //    showMsg("Error", "Please attach at least 1 documents.", InformationType.Error);
                    //}
                    //else
                    //{
                        base.Save(args);

                        IObjectSpace os = Application.CreateObjectSpace();
                        DocTypes number = os.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.PurchaseRequests));

                        number.CurrectDocNum = number.NextDocNum;
                        number.NextDocNum = number.NextDocNum + 1;

                        newPR.DocNum = "PR" + number.CurrectDocNum;

                        //foreach (DocTypeDocNum dtl in number.DocTypeDocNum)
                        //{
                        //    if (dtl.Entity.EntityCode == newPR.Entity.EntityCode)
                        //    {
                        //        dtl.CurrectDocNum = dtl.NextDocNum;
                        //        dtl.NextDocNum = dtl.NextDocNum + 1;

                        //        newPR.DocNum = newPR.CreateUser.Staff.StaffDepartment.DepartmentCode + dtl.CurrectDocNum;
                        //        break;
                        //    }
                        //}

                        os.CommitChanges();
                        os.Refresh();

                        base.Save(args);
                        ((DetailView)View).ViewEditMode = ViewEditMode.View;
                        View.BreakLinksToControls();
                        View.CreateControls();
                    //}
                }
                else
                {
                    //if (newPR.IsValid == false)
                    //{
                    //    showMsg("Error", "Please attach at least 1 documents.", InformationType.Error);
                    //}
                    //else
                    //{
                        base.Save(args);

                        ((DetailView)View).ViewEditMode = ViewEditMode.View;
                        View.BreakLinksToControls();
                        View.CreateControls();
                    //}
                }
            }
            else if (View.ObjectTypeInfo.Type == typeof(PurchaseOrders))
            {
                PurchaseOrders newPO = (PurchaseOrders)args.CurrentObject;
                if (newPO.IsNew == true)
                {
                    //if (newPO.IsValid == false)
                    //{
                    //    showMsg("Error", "Please attach at least 1 documents.", InformationType.Error);
                    //}
                    //else
                    //{
                        base.Save(args);

                        IObjectSpace os = Application.CreateObjectSpace();
                        DocTypes number = os.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.PurchaseOrders));

                        number.CurrectDocNum = number.NextDocNum;
                        number.NextDocNum = number.NextDocNum + 1;

                        newPO.DocNum = "PO" + number.CurrectDocNum;

                        //foreach (DocTypeDocNum dtl in number.DocTypeDocNum)
                        //{
                        //    if (dtl.Entity.EntityCode == newPO.Entity.EntityCode)
                        //    {
                        //        dtl.CurrectDocNum = dtl.NextDocNum;
                        //        dtl.NextDocNum = dtl.NextDocNum + 1;

                        //        newPO.DocNum = newPO.CreateUser.Staff.StaffDepartment.DepartmentCode + dtl.CurrectDocNum;
                        //        break;
                        //    }
                        //}

                        os.CommitChanges();
                        os.Refresh();
                        base.Save(args);

                        foreach (PurchaseOrderDetails podetail in newPO.PurchaseOrderDetails)
                        {
                            IObjectSpace pos = Application.CreateObjectSpace();
                            PurchaseRequestDetails pr = pos.FindObject<PurchaseRequestDetails>(new BinaryOperator("Oid", podetail.BaseOid));

                            if (pr != null)
                            {
                                pr.OpenQuantity -= podetail.Quantity;
                                pos.CommitChanges();
                            }
                        }

                        ((DetailView)View).ViewEditMode = ViewEditMode.View;
                        View.BreakLinksToControls();
                        View.CreateControls();
                    //}
                }
                else
                {
                    //if (newPO.IsValid == false)
                    //{
                    //    showMsg("Error", "Please attach at least 1 documents.", InformationType.Error);
                    //}
                    //else
                    //{
                        base.Save(args);
                        ((DetailView)View).ViewEditMode = ViewEditMode.View;
                        View.BreakLinksToControls();
                        View.CreateControls();
                    //}
                }
            }
            else if (View.ObjectTypeInfo.Type == typeof(GoodsReceipt))
            {
                GoodsReceipt newGRN = (GoodsReceipt)args.CurrentObject;
                if (newGRN.IsNew == true)
                {
                    // Start TJC 2025/06/11
                    //if (newGRN.IsValid == false)
                    //{
                    //    showMsg("Error", "Please attach at least 1 documents.", InformationType.Error);
                    //}
                    //else
                    //{
                    // End TJC 2025/06/11
                        base.Save(args);

                        IObjectSpace os = Application.CreateObjectSpace();
                        DocTypes number = os.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.GRN));

                        number.CurrectDocNum = number.NextDocNum;
                        number.NextDocNum = number.NextDocNum + 1;

                        newGRN.DocNum = "GRN" + number.CurrectDocNum;

                        //foreach (DocTypeDocNum dtl in number.DocTypeDocNum)
                        //{
                        //    if (dtl.Entity.EntityCode == newPO.Entity.EntityCode)
                        //    {
                        //        dtl.CurrectDocNum = dtl.NextDocNum;
                        //        dtl.NextDocNum = dtl.NextDocNum + 1;

                        //        newPO.DocNum = newPO.CreateUser.Staff.StaffDepartment.DepartmentCode + dtl.CurrectDocNum;
                        //        break;
                        //    }
                        //}

                        os.CommitChanges();
                        os.Refresh();
                        base.Save(args);

                        //foreach (GoodsReceiptDetails podetail in newGRN.GoodsReceiptDetails)
                        //{
                        //    IObjectSpace pos = Application.CreateObjectSpace();
                        //    PurchaseOrderDetails po = pos.FindObject<PurchaseOrderDetails>(new BinaryOperator("Oid", podetail.BaseOid));

                        //    if (po != null)
                        //    {
                        //        po.OpenQuantity -= podetail.Quantity;
                        //        pos.CommitChanges();
                        //    }
                        //}

                        ((DetailView)View).ViewEditMode = ViewEditMode.View;
                        View.BreakLinksToControls();
                        View.CreateControls();
                    // Start TJC 2025/06/11
                    //}
                    // End TJC 2025/06/11
                }
                else
                {
                    // Start TJC 2025/06/11
                    //if (newGRN.IsValid == false)
                    //{
                    //    showMsg("Error", "Please attach at least 1 documents.", InformationType.Error);
                    //}
                    //else
                    //{
                    // End TJC 2025/06/11
                        base.Save(args);
                        ((DetailView)View).ViewEditMode = ViewEditMode.View;
                        View.BreakLinksToControls();
                        View.CreateControls();
                    // Start TJC 2025/06/11
                    //}
                    // End TJC 2025/06/11
                }
            }
            else if (View.ObjectTypeInfo.Type == typeof(GoodsIssue))
            {
                GoodsIssue newGI = (GoodsIssue)args.CurrentObject;
                if (newGI.IsNew == true)
                {
                    //if (newGI.IsValid == false)
                    //{
                    //    showMsg("Error", "Please attach at least 1 documents.", InformationType.Error);
                    //}
                    //else
                    //{
                        base.Save(args);

                        IObjectSpace os = Application.CreateObjectSpace();
                        DocTypes number = os.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.GI));

                        number.CurrectDocNum = number.NextDocNum;
                        number.NextDocNum = number.NextDocNum + 1;

                        newGI.DocNum = "GI" + number.CurrectDocNum;

                        //foreach (DocTypeDocNum dtl in number.DocTypeDocNum)
                        //{
                        //    if (dtl.Entity.EntityCode == newPO.Entity.EntityCode)
                        //    {
                        //        dtl.CurrectDocNum = dtl.NextDocNum;
                        //        dtl.NextDocNum = dtl.NextDocNum + 1;

                        //        newPO.DocNum = newPO.CreateUser.Staff.StaffDepartment.DepartmentCode + dtl.CurrectDocNum;
                        //        break;
                        //    }
                        //}

                        os.CommitChanges();
                        os.Refresh();
                        base.Save(args);

                        //foreach (GoodsReceiptDetails podetail in newGRN.GoodsReceiptDetails)
                        //{
                        //    IObjectSpace pos = Application.CreateObjectSpace();
                        //    PurchaseOrderDetails po = pos.FindObject<PurchaseOrderDetails>(new BinaryOperator("Oid", podetail.BaseOid));

                        //    if (po != null)
                        //    {
                        //        po.OpenQuantity -= podetail.Quantity;
                        //        pos.CommitChanges();
                        //    }
                        //}

                        ((DetailView)View).ViewEditMode = ViewEditMode.View;
                        View.BreakLinksToControls();
                        View.CreateControls();
                    //}
                }
                else
                {
                    //if (newGI.IsValid == false)
                    //{
                    //    showMsg("Error", "Please attach at least 1 documents.", InformationType.Error);
                    //}
                    //else
                    //{
                        base.Save(args);
                        ((DetailView)View).ViewEditMode = ViewEditMode.View;
                        View.BreakLinksToControls();
                        View.CreateControls();
                    //}
                }
            }
            else if (View.ObjectTypeInfo.Type == typeof(PurchaseBlanketAgreement))
            {
                PurchaseBlanketAgreement newBA = (PurchaseBlanketAgreement)args.CurrentObject;
                if (newBA.IsNew == true)
                {
                    //if (newBA.IsValid == false)
                    //{
                    //    showMsg("Error", "Please attach at least 1 documents.", InformationType.Error);
                    //}
                    //else
                    //{
                        base.Save(args);

                        IObjectSpace os = Application.CreateObjectSpace();
                        DocTypes number = os.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.BA));

                        number.CurrectDocNum = number.NextDocNum;
                        number.NextDocNum = number.NextDocNum + 1;

                        newBA.DocNum = "PBA" + number.CurrectDocNum;

                        //foreach (DocTypeDocNum dtl in number.DocTypeDocNum)
                        //{
                        //    if (dtl.Entity.EntityCode == newPO.Entity.EntityCode)
                        //    {
                        //        dtl.CurrectDocNum = dtl.NextDocNum;
                        //        dtl.NextDocNum = dtl.NextDocNum + 1;

                        //        newPO.DocNum = newPO.CreateUser.Staff.StaffDepartment.DepartmentCode + dtl.CurrectDocNum;
                        //        break;
                        //    }
                        //}

                        os.CommitChanges();
                        os.Refresh();
                        base.Save(args);

                        ((DetailView)View).ViewEditMode = ViewEditMode.View;
                        View.BreakLinksToControls();
                        View.CreateControls();
                    //}
                }
                else
                {
                    //if (newBA.IsValid == false)
                    //{
                    //    showMsg("Error", "Please attach at least 1 documents.", InformationType.Error);
                    //}
                    //else
                    //{
                        base.Save(args);
                        ((DetailView)View).ViewEditMode = ViewEditMode.View;
                        View.BreakLinksToControls();
                        View.CreateControls();
                    //}
                }
            }
            else
            {
                base.Save(args);
                ((DetailView)View).ViewEditMode = ViewEditMode.View;
                View.BreakLinksToControls();
                View.CreateControls();
            }
        }
    }
}
