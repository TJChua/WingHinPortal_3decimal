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
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Persistent.Validation;
using WingHinPortal.Module.BusinessObjects.GoodsIssue;
using WingHinPortal.Module.BusinessObjects.GoodsReceipt;
using WingHinPortal.Module.BusinessObjects.PO;
using WingHinPortal.Module.BusinessObjects.PR;
using WingHinPortal.Module.BusinessObjects.PurchaseBlanketAgreement;
using WingHinPortal.Module.BusinessObjects.Setup;

namespace WingHinPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class FilterControllers : ViewController
    {
        public FilterControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;

            if (View.ObjectTypeInfo.Type == typeof(PurchaseRequest))
            {
                if (View.Id == "PurchaseRequest_ListView")
                {
                    PermissionPolicyRole PRRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('PRUserRole')"));
                    PermissionPolicyRole SuperPRRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('SuperPRRole')"));

                    if (PRRole != null && SuperPRRole == null)
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Department.Oid]=? or [CreateUser.Oid]=?",
                            user.Staff.StaffDepartment.Oid, user.Oid);
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(PurchaseRequest))
            {
                if (View.Id == "PurchaseRequest_ListView_PendApp")
                {
                    PermissionPolicyRole AppRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ApprovalUserRole')"));

                    if (AppRole != null)
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse(" [ApprovalStatus] = ? and Contains([NextApprover],?)", 2, user.Staff.StaffName);
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(PurchaseRequest))
            {
                if (View.Id == "PurchaseRequest_ListView_Approved")
                {
                    PermissionPolicyRole AppRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ApprovalUserRole')"));

                    if (AppRole != null)
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("Contains([WhoApprove],?)", user.Staff.StaffName);

                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(PurchaseOrders))
            {
                if (View.Id == "PurchaseOrders_ListView")
                {
                    PermissionPolicyRole PORole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('POUserRole')"));
                    PermissionPolicyRole SuperPORole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('SuperPORole')"));

                    if (PORole != null && SuperPORole == null)
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Department.Oid]=? or [CreateUser.Oid]=?",
                            user.Staff.StaffDepartment.Oid, user.Oid);
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(PurchaseOrders))
            {
                if (View.Id == "PurchaseOrders_ListView_PendApp")
                {
                    PermissionPolicyRole AppRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ApprovalPORole')"));

                    if (AppRole != null)
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse(" [ApprovalStatus] = ? and Contains([NextApprover],?)", 2, user.Staff.StaffName);
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(PurchaseOrders))
            {
                if (View.Id == "PurchaseOrders_ListView_Approved")
                {
                    PermissionPolicyRole AppRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('ApprovalPORole')"));

                    if (AppRole != null)
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("Contains([WhoApprove],?)", user.Staff.StaffName);

                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(GoodsReceipt))
            {
                if (View.Id == "GoodsReceipt_ListView")
                {
                    PermissionPolicyRole GRNRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('GRNUserRole')"));
                    PermissionPolicyRole SuperGRNRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('SuperGRNRole')"));

                    if (GRNRole != null && SuperGRNRole == null)
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Department.Oid]=? or [CreateUser.Oid]=?",
                            user.Staff.StaffDepartment.Oid, user.Oid);
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(GoodsIssue))
            {
                if (View.Id == "GoodsIssue_ListView")
                {
                    PermissionPolicyRole InvenRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('InventoryUserRole')"));
                    PermissionPolicyRole SuperInvenRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('SuperInvenRole')"));

                    if (InvenRole != null && SuperInvenRole == null)
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Department.Oid]=? or [CreateUser.Oid]=?",
                            user.Staff.StaffDepartment.Oid, user.Oid);
                    }
                }
            }

            if (View.ObjectTypeInfo.Type == typeof(PurchaseBlanketAgreement))
            {
                if (View.Id == "PurchaseBlanketAgreement_ListView")
                {
                    PermissionPolicyRole BlankerRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('BlanketAgreementUserRole')"));
                    PermissionPolicyRole SuperBlankerRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('SuperBlanketAgreementUserRole')"));

                    if (BlankerRole != null && SuperBlankerRole == null)
                    {
                        ((ListView)View).CollectionSource.Criteria["Filter1"] = CriteriaOperator.Parse("[Department.Oid]=? or [CreateUser.Oid]=?",
                            user.Staff.StaffDepartment.Oid, user.Oid);
                    }
                }
            }
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
    }
}
