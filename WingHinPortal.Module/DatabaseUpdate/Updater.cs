using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using WingHinPortal.Module.BusinessObjects.Setup;
using WingHinPortal.Module.BusinessObjects.PR;
using WingHinPortal.Module.BusinessObjects.PO;
using WingHinPortal.Module.BusinessObjects.View;
using WingHinPortal.Module.BusinessObjects.PurchaseBlanketAgreement;
using WingHinPortal.Module.BusinessObjects.GoodsReceipt;
using WingHinPortal.Module.BusinessObjects.GoodsIssue;

namespace WingHinPortal.Module.DatabaseUpdate {
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppUpdatingModuleUpdatertopic.aspx
    public class Updater : ModuleUpdater
    {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion)
        {
        }
        public override void UpdateDatabaseAfterUpdateSchema()
        {
            base.UpdateDatabaseAfterUpdateSchema();
            //string name = "MyName";
            //DomainObject1 theObject = ObjectSpace.FindObject<DomainObject1>(CriteriaOperator.Parse("Name=?", name));
            //if(theObject == null) {
            //    theObject = ObjectSpace.CreateObject<DomainObject1>();
            //    theObject.Name = name;
            //}
            SystemUsers sampleUser = ObjectSpace.FindObject<SystemUsers>(new BinaryOperator("UserName", "User"));
            if (sampleUser == null)
            {
                sampleUser = ObjectSpace.CreateObject<SystemUsers>();
                sampleUser.UserName = "User";
                sampleUser.SetPassword("");
            }
            PermissionPolicyRole defaultRole = CreateDefaultRole();
            sampleUser.Roles.Add(defaultRole);

            SystemUsers userAdmin = ObjectSpace.FindObject<SystemUsers>(new BinaryOperator("UserName", "Admin"));
            if (userAdmin == null)
            {
                userAdmin = ObjectSpace.CreateObject<SystemUsers>();
                userAdmin.UserName = "Admin";
                // Set a password if the standard authentication type is used
                userAdmin.SetPassword("");
            }
            // If a role with the Administrators name doesn't exist in the database, create this role
            PermissionPolicyRole adminRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "Administrators"));
            if (adminRole == null)
            {
                adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                adminRole.Name = "Administrators";
            }
            adminRole.IsAdministrative = true;
            userAdmin.Roles.Add(adminRole);

            PermissionPolicyRole AppRole = CreateAppUserRole();
            PermissionPolicyRole PRRole = CreatePRUserRole();
            PermissionPolicyRole PORole = CreatePOUserRole();
            PermissionPolicyRole GRNRole = CreateGRNUserRole();
            PermissionPolicyRole InvenRole = CreateInvenUserRole();
            PermissionPolicyRole PBARole = CreatePBAUserRole();

            ObjectSpace.CommitChanges(); //This line persists created object(s).
        }
        public override void UpdateDatabaseBeforeUpdateSchema()
        {
            base.UpdateDatabaseBeforeUpdateSchema();
            //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
            //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
            //}
        }
        private PermissionPolicyRole CreateDefaultRole()
        {
            PermissionPolicyRole defaultRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "Default"));
            if (defaultRole == null)
            {
                defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                defaultRole.Name = "Default";

                defaultRole.AddObjectPermission<PermissionPolicyUser>(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                defaultRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                defaultRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
                defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
            }
            return defaultRole;
        }

        private PermissionPolicyRole CreateAppUserRole()
        {
            PermissionPolicyRole ApprovalUserRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "ApprovalUserRole"));
            if (ApprovalUserRole == null)
            {
                ApprovalUserRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                ApprovalUserRole.Name = "ApprovalUserRole";

                ApprovalUserRole.AddObjectPermission<PermissionPolicyUser>(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
                ApprovalUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                ApprovalUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                ApprovalUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);

                ApprovalUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Purchase Request/Items/Purchase Request - Pending Approve", SecurityPermissionState.Allow);
                ApprovalUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Purchase Request/Items/Purchase Request - Approved", SecurityPermissionState.Allow);
                ApprovalUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Purchase Order/Items/Purchase Order - Pending Approve", SecurityPermissionState.Allow);
                ApprovalUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Purchase Order/Items/Purchase Order - Approved", SecurityPermissionState.Allow);

                // PR
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequest>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequest>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestDetails>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestDetails>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestDetails>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStage>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStage>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStage>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestAttachment>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestAttachment>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestAttachment>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestDocStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestDocStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseRequestDocStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                // PO
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrders>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrders>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderDetails>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderDetails>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderDetails>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStage>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStage>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStage>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderAttachment>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderAttachment>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderAttachment>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderDocStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderDocStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<PurchaseOrderDocStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //SAP
                ApprovalUserRole.AddTypePermissionsRecursively<vwCostCenter>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<vwItemMasters>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<vwPriceList>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<vwVendors>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<vwTax>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<vwPR>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<vwPRInternalGI>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<vwPRInternalPO>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<vwStockBalance>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<vwWarehouse>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<vwGRN>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //Setup
                ApprovalUserRole.AddTypePermissionsRecursively<Approvals>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<ApprovalUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<CompanyAddress>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<Department>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<DocTypes>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<ExpenditureType>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<StaffInfo>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //SystemUsers
                ApprovalUserRole.AddTypePermissionsRecursively<SystemUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //File data
                ApprovalUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Create, SecurityPermissionState.Allow);
                ApprovalUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //Audit Trail
                ApprovalUserRole.AddTypePermissionsRecursively<AuditDataItemPersistent>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
            }

            return ApprovalUserRole;
        }

        private PermissionPolicyRole CreatePRUserRole()
        {
            PermissionPolicyRole PRUserRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "PRUserRole"));
            if (PRUserRole == null)
            {
                PRUserRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                PRUserRole.Name = "PRUserRole";

                PRUserRole.AddObjectPermission<PermissionPolicyUser>(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
                PRUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                PRUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                PRUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);

                PRUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Purchase Request/Items/PurchaseRequest_ListView", SecurityPermissionState.Allow);

                // PR
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequest>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequest>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestDetails>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestDetails>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestDetails>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStage>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStage>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStage>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestAppStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestAttachment>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestAttachment>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestAttachment>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestDocStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestDocStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<PurchaseRequestDocStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //SAP
                PRUserRole.AddTypePermissionsRecursively<vwCostCenter>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<vwItemMasters>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<vwPriceList>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<vwVendors>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<vwTax>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<vwPR>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<vwPRInternalGI>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<vwPRInternalPO>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<vwStockBalance>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<vwWarehouse>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<vwGRN>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //Setup
                PRUserRole.AddTypePermissionsRecursively<Approvals>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<ApprovalUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<CompanyAddress>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<Department>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<DocTypes>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<ExpenditureType>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<StaffInfo>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //SystemUsers
                PRUserRole.AddTypePermissionsRecursively<SystemUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //File data
                PRUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PRUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //Audit Trail
                PRUserRole.AddTypePermissionsRecursively<AuditDataItemPersistent>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
            }

            return PRUserRole;
        }

        private PermissionPolicyRole CreatePOUserRole()
        {
            PermissionPolicyRole POUserRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "POUserRole"));
            if (POUserRole == null)
            {
                POUserRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                POUserRole.Name = "POUserRole";

                POUserRole.AddObjectPermission<PermissionPolicyUser>(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
                POUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                POUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                POUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);

                POUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Purchase Order/Items/PurchaseOrders_ListView", SecurityPermissionState.Allow);

                // PO
                POUserRole.AddTypePermissionsRecursively<PurchaseOrders>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrders>(SecurityOperations.Create, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderDetails>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderDetails>(SecurityOperations.Create, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderDetails>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStage>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStage>(SecurityOperations.Create, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStage>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderAppStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderAttachment>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderAttachment>(SecurityOperations.Create, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderAttachment>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderDocStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderDocStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<PurchaseOrderDocStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //SAP
                POUserRole.AddTypePermissionsRecursively<vwCostCenter>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<vwItemMasters>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<vwPriceList>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<vwVendors>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<vwTax>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<vwPR>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<vwPRInternalGI>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<vwPRInternalPO>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<vwStockBalance>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<vwWarehouse>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<vwGRN>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //Setup
                POUserRole.AddTypePermissionsRecursively<Approvals>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<ApprovalUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<CompanyAddress>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<Department>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<DocTypes>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<ExpenditureType>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<StaffInfo>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //SystemUsers
                POUserRole.AddTypePermissionsRecursively<SystemUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //File data
                POUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Create, SecurityPermissionState.Allow);
                POUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //Audit Trail
                POUserRole.AddTypePermissionsRecursively<AuditDataItemPersistent>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
            }

            return POUserRole;
        }

        private PermissionPolicyRole CreatePBAUserRole()
        {
            PermissionPolicyRole PBAUserRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "BlanketAgreementUserRole"));
            if (PBAUserRole == null)
            {
                PBAUserRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                PBAUserRole.Name = "BlanketAgreementUserRole";

                PBAUserRole.AddObjectPermission<PermissionPolicyUser>(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
                PBAUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                PBAUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                PBAUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);

                PBAUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Purchase Blanket Agreement/Items/PurchaseBlanketAgreement_ListView", SecurityPermissionState.Allow);

                // PBA
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreement>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreement>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreementDetails>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreementDetails>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreementDetails>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreementAttachment>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreementAttachment>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreementAttachment>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreementDocStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreementDocStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<PurchaseBlanketAgreementDocStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //SAP
                PBAUserRole.AddTypePermissionsRecursively<vwCostCenter>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<vwItemMasters>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<vwPriceList>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<vwVendors>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<vwTax>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<vwPR>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<vwPRInternalGI>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<vwPRInternalPO>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<vwStockBalance>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<vwWarehouse>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<vwGRN>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //Setup
                PBAUserRole.AddTypePermissionsRecursively<Approvals>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<ApprovalUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<CompanyAddress>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<Department>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<DocTypes>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<ExpenditureType>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<StaffInfo>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //SystemUsers
                PBAUserRole.AddTypePermissionsRecursively<SystemUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //File data
                PBAUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Create, SecurityPermissionState.Allow);
                PBAUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //Audit Trail
                PBAUserRole.AddTypePermissionsRecursively<AuditDataItemPersistent>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
            }

            return PBAUserRole;
        }

        private PermissionPolicyRole CreateGRNUserRole()
        {
            PermissionPolicyRole GRNUserRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "GRNUserRole"));
            if (GRNUserRole == null)
            {
                GRNUserRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                GRNUserRole.Name = "GRNUserRole";

                GRNUserRole.AddObjectPermission<PermissionPolicyUser>(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
                GRNUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                GRNUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                GRNUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);

                GRNUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Goods Receipt Note/Items/GoodsReceipt_ListView", SecurityPermissionState.Allow);

                // GRN
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceipt>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceipt>(SecurityOperations.Create, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceiptDetails>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceiptDetails>(SecurityOperations.Create, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceiptDetails>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceiptAttachment>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceiptAttachment>(SecurityOperations.Create, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceiptAttachment>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceiptDocStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceiptDocStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<GoodsReceiptDocStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //SAP
                GRNUserRole.AddTypePermissionsRecursively<vwCostCenter>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<vwItemMasters>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<vwPriceList>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<vwVendors>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<vwTax>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<vwPR>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<vwPRInternalGI>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<vwPRInternalPO>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<vwStockBalance>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<vwWarehouse>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<vwGRN>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //Setup
                GRNUserRole.AddTypePermissionsRecursively<Approvals>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<ApprovalUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<CompanyAddress>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<Department>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<DocTypes>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<ExpenditureType>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<StaffInfo>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //SystemUsers
                GRNUserRole.AddTypePermissionsRecursively<SystemUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //File data
                GRNUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Create, SecurityPermissionState.Allow);
                GRNUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //Audit Trail
                GRNUserRole.AddTypePermissionsRecursively<AuditDataItemPersistent>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
            }

            return GRNUserRole;
        }

        private PermissionPolicyRole CreateInvenUserRole()
        {
            PermissionPolicyRole InventoryUserRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "InventoryUserRole"));
            if (InventoryUserRole == null)
            {
                InventoryUserRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                InventoryUserRole.Name = "InventoryUserRole";

                InventoryUserRole.AddObjectPermission<PermissionPolicyUser>(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<PermissionPolicyUser>(SecurityOperations.Read, SecurityPermissionState.Allow);
                InventoryUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                InventoryUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                InventoryUserRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);

                InventoryUserRole.AddNavigationPermission(@"Application/NavigationItems/Items/Inventory/Items/GoodsIssue_ListView", SecurityPermissionState.Allow);

                // GRN
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssue>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssue>(SecurityOperations.Create, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssueDetails>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssueDetails>(SecurityOperations.Create, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssueDetails>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssueAttachment>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssueAttachment>(SecurityOperations.Create, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssueAttachment>(SecurityOperations.Delete, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssueDocStatus>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssueDocStatus>(SecurityOperations.Create, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<GoodsIssueDocStatus>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //SAP
                InventoryUserRole.AddTypePermissionsRecursively<vwCostCenter>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<vwItemMasters>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<vwPriceList>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<vwVendors>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<vwTax>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<vwPR>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<vwPRInternalGI>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<vwPRInternalPO>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<vwStockBalance>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<vwWarehouse>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<vwGRN>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //Setup
                InventoryUserRole.AddTypePermissionsRecursively<Approvals>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<ApprovalUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<CompanyAddress>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<Department>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<DocTypes>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<ExpenditureType>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<StaffInfo>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //SystemUsers
                InventoryUserRole.AddTypePermissionsRecursively<SystemUsers>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);

                //File data
                InventoryUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Create, SecurityPermissionState.Allow);
                InventoryUserRole.AddTypePermissionsRecursively<FileData>(SecurityOperations.Delete, SecurityPermissionState.Allow);

                //Audit Trail
                InventoryUserRole.AddTypePermissionsRecursively<AuditDataItemPersistent>(SecurityOperations.ReadOnlyAccess, SecurityPermissionState.Allow);
            }

            return InventoryUserRole;
        }
    }
}
