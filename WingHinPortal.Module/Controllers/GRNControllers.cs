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
using WingHinPortal.Module.BusinessObjects.GoodsReceipt;
using WingHinPortal.Module.BusinessObjects.PO;
using WingHinPortal.Module.BusinessObjects.Setup;
using WingHinPortal.Module.BusinessObjects.View;

// 20260512 - add cost center 2 - ver 1.0.2

namespace WingHinPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class GRNControllers : ViewController
    {
        public GRNControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
            TargetObjectType = typeof(GoodsReceipt);
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
            if (View.Id == "GoodsReceipt_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.Edit)
                {
                    this.CopyFromPO.Active.SetItemValue("Enabled", true);
                    this.SubmitGRN.Active.SetItemValue("Enabled", false);
                    this.CancelGRN.Active.SetItemValue("Enabled", false);
                }
                else
                {
                    this.CopyFromPO.Active.SetItemValue("Enabled", false);
                    this.SubmitGRN.Active.SetItemValue("Enabled", true);
                    this.CancelGRN.Active.SetItemValue("Enabled", true);
                }
            }
            else
            {
                this.CopyFromPO.Active.SetItemValue("Enabled", false);
                this.SubmitGRN.Active.SetItemValue("Enabled", false);
                this.CancelGRN.Active.SetItemValue("Enabled", false);
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

        private void SubmitGRN_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                GoodsReceipt selectedObject = (GoodsReceipt)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                if (p.IsErr) return;

                if (selectedObject.GoodsReceiptDetails.Count() <= 0)
                {
                    showMsg("Error", "No allow to submit due to no item detail.", InformationType.Error);
                    return;
                }

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                selectedObject.DocStatus = DocStatus.Submit;
                GoodsReceiptDocStatus ds = ObjectSpace.CreateObject<GoodsReceiptDocStatus>();
                ds.DocStatus = DocStatus.Submit;
                ds.DocRemarks = p.ParamString;
                selectedObject.GoodsReceiptDocStatus.Add(ds);

                if (selectedObject.IsNew == true)
                {
                    IObjectSpace dos = Application.CreateObjectSpace();
                    DocTypes number = dos.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.GRN));

                    number.CurrectDocNum = number.NextDocNum;
                    number.NextDocNum = number.NextDocNum + 1;

                    selectedObject.DocNum = "GRN" + number.CurrectDocNum;

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
                GoodsReceipt grntrx = os.FindObject<GoodsReceipt>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, grntrx, ViewEditMode.View);
                showMsg("Successful", "Submit Done.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "No GRN Selected/More Than One GRN Selected.", InformationType.Error);
            }
        }

        private void SubmitGRN_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CancelGRN_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count == 1)
            {
                GoodsReceipt selectedObject = (GoodsReceipt)e.CurrentObject;
                StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
                if (p.IsErr) return;

                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

                selectedObject.DocStatus = DocStatus.Cancel;
                GoodsReceiptDocStatus ds = ObjectSpace.CreateObject<GoodsReceiptDocStatus>();
                ds.DocStatus = DocStatus.Cancel;
                ds.DocRemarks = p.ParamString;
                selectedObject.GoodsReceiptDocStatus.Add(ds);

                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();

                IObjectSpace os = Application.CreateObjectSpace();
                GoodsReceipt grntrx = os.FindObject<GoodsReceipt>(new BinaryOperator("Oid", selectedObject.Oid));
                openNewView(os, grntrx, ViewEditMode.View);
                showMsg("Successful", "Cancel Done.", InformationType.Success);
            }
            else
            {
                showMsg("Fail", "No GRN Selected/More Than One GRN Selected.", InformationType.Error);
            }
        }

        private void CancelGRN_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<StringParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CopyFromPO_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count > 0)
            {
                try
                {
                    IObjectSpace os = Application.CreateObjectSpace();
                    GoodsReceipt newgrn = os.CreateObject<GoodsReceipt>();

                    foreach (vwPO dtl in e.PopupWindowViewSelectedObjects)
                    {
                        newgrn.VendorCode = newgrn.Session.GetObjectByKey<vwVendors>(dtl.VendorCode);

                        PurchaseOrders temppo = os.FindObject<PurchaseOrders>(new BinaryOperator("DocNum", dtl.PortalDocNum));
                        if (temppo != null)
                        {
                            if (temppo.ExpenditureType != null)
                            {
                                newgrn.ExpenditureType = newgrn.Session.GetObjectByKey<ExpenditureType>(temppo.ExpenditureType.Oid);
                            }
                            if (temppo.CompanyAddress != null)
                            {
                                newgrn.CompanyAddress = newgrn.Session.GetObjectByKey<CompanyAddress>(temppo.CompanyAddress.Oid);
                            }
                        }

                        GoodsReceiptDetails newGRNItem = os.CreateObject<GoodsReceiptDetails>();

                        newGRNItem.Item = newGRNItem.Session.FindObject<vwItemMasters>(CriteriaOperator.Parse("ItemCode = ?", dtl.Item));
                        newGRNItem.ItemDesc = dtl.ItemDesc;
                        newGRNItem.ItemDetails = dtl.ItemDetails;
                        newGRNItem.OpenQuantity = dtl.Quantity;
                        newGRNItem.Quantity = dtl.Quantity;
                        newGRNItem.Unitprice = dtl.OriPrice;
                        newGRNItem.POPrice = dtl.Unitprice;
                        newGRNItem.BaseDoc = dtl.DocNum;
                        newGRNItem.BaseOid = dtl.Baseline.ToString();
                        newGRNItem.BaseEntry = dtl.BaseEntry;
                        newGRNItem.Vehicle = dtl.Vehicle;
                        newGRNItem.CostCenter = newGRNItem.Session.FindObject<vwCostCenter>(CriteriaOperator.Parse("PrcCode = ?", dtl.CostCenter));
                        // Start ver 1.0.2
                        if (!string.IsNullOrEmpty(dtl.SubCostCenter))
                        {
                            newGRNItem.SubCostCenter = newGRNItem.Session.FindObject<vwSubCostCenter>(CriteriaOperator.Parse("PrcCode = ?", dtl.SubCostCenter));
                        }
                        // End ver 1.0.2

                        newgrn.GoodsReceiptDetails.Add(newGRNItem);
                    }

                    ShowViewParameters svp = new ShowViewParameters();
                    DetailView dv = Application.CreateDetailView(os, newgrn);
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

        private void CopyFromPO_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            GoodsReceipt goodsreceipt = (GoodsReceipt)View.CurrentObject;

            var os = Application.CreateObjectSpace();
            var viewId = Application.FindListViewId(typeof(vwPO));
            var cs = Application.CreateCollectionSource(os, typeof(vwPO), viewId);
            if (goodsreceipt.VendorCode != null)
            {
                cs.Criteria["VendorCode"] = new BinaryOperator("VendorCode", goodsreceipt.VendorCode.CardCode);
            }
            var lv1 = Application.CreateListView(viewId, cs, true);
            e.View = lv1;
        }
    }
}
