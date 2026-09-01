using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
using WingHinPortal.Module.BusinessObjects.GoodsIssue;
using WingHinPortal.Module.BusinessObjects.GoodsReceipt;
using WingHinPortal.Module.BusinessObjects.PR;
using WingHinPortal.Module.BusinessObjects.Setup;
using WingHinPortal.Module.BusinessObjects.View;

// 20260512 - add cost center 2 - ver 1.0.2

namespace WingHinPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class GIControllers : ViewController
    {
        public GIControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
            TargetObjectType = typeof(GoodsIssue);
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
            if (View.Id == "GoodsIssue_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.Edit)
                {
                    this.CopyFromGIPR.Active.SetItemValue("Enabled", true);
                    this.CopyFromGRN.Active.SetItemValue("Enabled", true);
                    this.SubmitGI.Active.SetItemValue("Enabled", false);
                    this.CancelGI.Active.SetItemValue("Enabled", false);
                }
                else
                {
                    this.CopyFromGIPR.Active.SetItemValue("Enabled", false);
                    this.CopyFromGRN.Active.SetItemValue("Enabled", false);
                    this.SubmitGI.Active.SetItemValue("Enabled", true);
                    this.CancelGI.Active.SetItemValue("Enabled", true);
                }
            }
            else
            {
                this.CopyFromGIPR.Active.SetItemValue("Enabled", false);
                this.CopyFromGRN.Active.SetItemValue("Enabled", false);
                this.SubmitGI.Active.SetItemValue("Enabled", false);
                this.CancelGI.Active.SetItemValue("Enabled", false);
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

        private void SubmitGI_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                GoodsIssue selectedObject = (GoodsIssue)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                if (p.IsErr) return;

                if (selectedObject.GoodsIssueDetails.Count() <= 0)
                {
                    showMsg("Error", "No allow to submit due to no item detail.", InformationType.Error);
                    return;
                }

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                selectedObject.DocStatus = DocStatus.Submit;
                GoodsIssueDocStatus ds = ObjectSpace.CreateObject<GoodsIssueDocStatus>();
                ds.DocStatus = DocStatus.Submit;
                ds.DocRemarks = p.ParamString;
                selectedObject.GoodsIssueDocStatus.Add(ds);

                if (selectedObject.IsNew == true)
                {
                    IObjectSpace dos = Application.CreateObjectSpace();
                    DocTypes number = dos.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.GI));

                    number.CurrectDocNum = number.NextDocNum;
                    number.NextDocNum = number.NextDocNum + 1;

                    selectedObject.DocNum = "GI" + number.CurrectDocNum;

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

                IObjectSpace os = Application.CreateObjectSpace();
                GoodsIssue gitrx = os.FindObject<GoodsIssue>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, gitrx, ViewEditMode.View);
                showMsg("Successful", "Submit Done.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "No GI Selected/More Than One GI Selected.", InformationType.Error);
            }
        }

        private void SubmitGI_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CancelGI_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                GoodsIssue selectedObject = (GoodsIssue)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                if (p.IsErr) return;

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                selectedObject.DocStatus = DocStatus.Cancel;
                GoodsIssueDocStatus ds = ObjectSpace.CreateObject<GoodsIssueDocStatus>();
                ds.DocStatus = DocStatus.Cancel;
                ds.DocRemarks = p.ParamString;
                selectedObject.GoodsIssueDocStatus.Add(ds);

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                IObjectSpace os = Application.CreateObjectSpace();
                GoodsIssue gitrx = os.FindObject<GoodsIssue>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, gitrx, ViewEditMode.View);
                showMsg("Successful", "Cancel Done.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "No GI Selected/More Than One GI Selected.", InformationType.Error);
            }
        }

        private void CancelGI_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CopyFromGIPR_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count > 0)
            {
                try
                {
                    IObjectSpace os = Application.CreateObjectSpace();
                    GoodsIssue newissue = os.CreateObject<GoodsIssue>();

                    foreach (vwPRInternalPO dtl in e.PopupWindowViewSelectedObjects)
                    {
                        newissue.VendorCode = newissue.Session.GetObjectByKey<vwVendors>(dtl.VendorCode);
                        if (dtl.ExpenditureType != null)
                        {
                            newissue.ExpenditureType = newissue.Session.GetObjectByKey<ExpenditureType>(dtl.ExpenditureType.Oid);
                        }

                        GoodsIssueDetails newissueItem = os.CreateObject<GoodsIssueDetails>();

                        newissueItem.Item = newissueItem.Session.FindObject<vwItemMasters>(CriteriaOperator.Parse("ItemCode = ?", dtl.Item));
                        newissueItem.ItemDesc = dtl.ItemDesc;
                        newissueItem.ItemDetails = dtl.ItemDetails;
                        newissueItem.CostCenter = newissueItem.Session.FindObject<vwCostCenter>(CriteriaOperator.Parse("PrcCode = ?", dtl.CostCenter));
                        // Start ver 1.0.2
                        if (!string.IsNullOrEmpty(dtl.SubCostCenter))
                        {
                            newissueItem.SubCostCenter = newissueItem.Session.FindObject<vwSubCostCenter>(CriteriaOperator.Parse("PrcCode = ?", dtl.SubCostCenter));
                        }
                        // End ver 1.0.2
                        newissueItem.BaseDoc = dtl.DocNum;

                        PurchaseRequest master = ObjectSpace.FindObject<PurchaseRequest>(CriteriaOperator.Parse("DocNum = ?", dtl.DocNum));

                        foreach (PurchaseRequestDetails detail in master.PurchaseRequestDetail)
                        {
                            if (dtl.Item == detail.Item.ItemCode)
                            {
                                newissueItem.OpenQuantity = detail.OpenQuantity;
                                newissueItem.Quantity = detail.OpenQuantity;
                                newissueItem.BaseOid = detail.Oid.ToString();
                            }
                        }

                        newissue.GoodsIssueDetails.Add(newissueItem);
                    }

                    ShowViewParameters svp = new ShowViewParameters();
                    DetailView dv = Application.CreateDetailView(os, newissue);
                    dv.ViewEditMode = ViewEditMode.Edit;
                    dv.IsRoot = true;
                    svp.CreatedView = dv;

                    Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
                    showMsg("Success", "Copy Success.", InformationType.Success);
                }
                catch (Exception)
                {
                    showMsg("Fail", "Copy Fail.", InformationType.Error);
                }
            }
        }

        private void CopyFromGIPR_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            GoodsIssue goodsissue = (GoodsIssue)View.CurrentObject;

            var os = Application.CreateObjectSpace();
            var viewId = Application.FindListViewId(typeof(vwPRInternalPO));
            var cs = Application.CreateCollectionSource(os, typeof(vwPRInternalPO), viewId);
            if (goodsissue.VendorCode != null)
            {
                cs.Criteria["VendorCode"] = new BinaryOperator("VendorCode", goodsissue.VendorCode.CardCode);
            }
            var lv1 = Application.CreateListView(viewId, cs, true);
            e.View = lv1;
        }

        private void CopyFromGRN_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count > 0)
            {
                try
                {
                    IObjectSpace os = Application.CreateObjectSpace();
                    GoodsIssue newissue = os.CreateObject<GoodsIssue>();

                    foreach (vwGRN dtl in e.PopupWindowViewSelectedObjects)
                    {
                        newissue.VendorCode = newissue.Session.GetObjectByKey<vwVendors>(dtl.VendorCode);
                        GoodsReceipt tempgrn = os.FindObject<GoodsReceipt>(new BinaryOperator("DocNum", dtl.PortalDocNum));
                        if (tempgrn != null)
                        {
                            if (tempgrn.ExpenditureType != null)
                            {
                                newissue.ExpenditureType = newissue.Session.GetObjectByKey<ExpenditureType>(tempgrn.ExpenditureType.Oid);
                            }
                        }

                        GoodsIssueDetails newissueItem = os.CreateObject<GoodsIssueDetails>();

                        newissueItem.Item = newissueItem.Session.FindObject<vwItemMasters>(CriteriaOperator.Parse("ItemCode = ?", dtl.Item));
                        newissueItem.ItemDesc = dtl.ItemDesc;
                        newissueItem.ItemDetails = dtl.ItemDetails;
                        // Start ver 1.0.2
                        //if (tempgrn != null)
                        //{
                        //    foreach(GoodsReceiptDetails grndetail in tempgrn.GoodsReceiptDetails)
                        //    {
                        //        if (grndetail.Item.ItemCode == dtl.Item)
                        //        {
                        //            newissueItem.CostCenter = newissueItem.Session.FindObject<vwCostCenter>(CriteriaOperator.Parse("PrcCode = ?", grndetail.CostCenter.PrcCode));
                        //        }
                        //    }
                        //}
                        if (!string.IsNullOrEmpty(dtl.CostCenter))
                        {
                            newissueItem.CostCenter = newissueItem.Session.FindObject<vwCostCenter>(CriteriaOperator.Parse("PrcCode = ?", dtl.CostCenter));
                        }
                        if (!string.IsNullOrEmpty(dtl.SubCostCenter))
                        {
                            newissueItem.SubCostCenter = newissueItem.Session.FindObject<vwSubCostCenter>(CriteriaOperator.Parse("PrcCode = ?", dtl.SubCostCenter));
                        }
                        // End ver 1.0.2

                        newissueItem.OpenQuantity = dtl.Quantity;
                        newissueItem.Quantity = dtl.Quantity;
                        newissueItem.BaseOid = dtl.Baseline.ToString();
                        newissueItem.BaseDoc = dtl.DocNum;

                        newissue.GoodsIssueDetails.Add(newissueItem);
                    }

                    ShowViewParameters svp = new ShowViewParameters();
                    DetailView dv = Application.CreateDetailView(os, newissue);
                    dv.ViewEditMode = ViewEditMode.Edit;
                    dv.IsRoot = true;
                    svp.CreatedView = dv;

                    Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
                    showMsg("Success", "Copy Success.", InformationType.Success);
                }
                catch (Exception)
                {
                    showMsg("Fail", "Copy Fail.", InformationType.Error);
                }
            }
        }

        private void CopyFromGRN_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            GoodsIssue goodsissue = (GoodsIssue)View.CurrentObject;

            var os = Application.CreateObjectSpace();
            var viewId = Application.FindListViewId(typeof(vwGRN));
            var cs = Application.CreateCollectionSource(os, typeof(vwGRN), viewId);
            if (goodsissue.VendorCode != null)
            {
                cs.Criteria["VendorCode"] = new BinaryOperator("VendorCode", goodsissue.VendorCode.CardCode);
            }
            var lv1 = Application.CreateListView(viewId, cs, true);
            e.View = lv1;
        }
    }
}
