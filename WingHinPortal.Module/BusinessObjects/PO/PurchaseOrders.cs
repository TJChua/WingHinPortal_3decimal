using System;
using System.Linq;
using System.Text;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using WingHinPortal.Module.BusinessObjects.Setup;
using DevExpress.ExpressApp.ConditionalAppearance;
using WingHinPortal.Module.BusinessObjects.View;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;

namespace WingHinPortal.Module.BusinessObjects.PO
{
    [DefaultClassOptions]
    [XafDisplayName("Purchase Order")]
    [NavigationItem("Purchase Order")]
    [DefaultProperty("DocNum")]

    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "PurchaseOrders_ListView_PendApp")]
    [Appearance("HideNew1", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "PurchaseOrders_ListView_Approved")]
    [Appearance("HideNew2", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "PurchaseOrders_DetailView_PendApp")]
    [Appearance("HideNew3", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "PurchaseOrders_DetailView_Approved")]

    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Criteria = "not (DocStatus in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmit", AppearanceItemType.Action, "True", TargetItems = "SubmitPO", Criteria = "not (DocStatus in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "CancelPO", Criteria = "not (DocStatus in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideClose", AppearanceItemType.Action, "True", TargetItems = "ClosePO", Criteria = "(not (ApprovalStatus in (1))) or ((ApprovalStatus in (1)) and (not (DocStatus in (1))))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideApproval", AppearanceItemType = "Action", TargetItems = "ApprovePO", Criteria = "(ApprovalStatus != 'Required_Approval')", Context = "Any", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("HideApproval1", AppearanceItemType = "Action", TargetItems = "ApprovePO", Context = "PurchaseOrders_DetailView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("HideApproval2", AppearanceItemType = "Action", TargetItems = "ApprovePR", Context = "PurchaseOrders_DetailView_Approved", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]

    [Appearance("HideCopyFrom", AppearanceItemType.Action, "True", TargetItems = "CopyFromPR", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "ListView")]
    [Appearance("HideCopyFrom1", AppearanceItemType.Action, "True", TargetItems = "CopyFromPR", Criteria = "not IsNew", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    //[Appearance("HidePrint", AppearanceItemType.Action, "True", TargetItems = "PrintPO", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "ListView")]

    [Appearance("HideEmails", AppearanceItemType.Action, "True", TargetItems = "EmailSupplier", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "ListView")]

    public class PurchaseOrders : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public PurchaseOrders(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
            SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;
            if (user != null)
            {
                CreateUser = Session.GetObjectByKey<SystemUsers>(user.Oid);
            }
            CreateDate = DateTime.Now;
            PostingDate = DateTime.Now;
            DocDate = DateTime.Now;
            DocType = Session.FindObject<DocTypes>
                (new BinaryOperator("BoCode", DocTypeList.PurchaseOrders, BinaryOperatorType.Equal));

            if (user != null)
            {
                if (user.Warehouse != null)
                {
                    Warehouse = Session.FindObject<vwWarehouse>
                        (new BinaryOperator("BoCode", user.Warehouse.BoCode, BinaryOperatorType.Equal));
                }

                if (user.Staff != null)
                {
                    Department = Session.FindObject<Department>
                        (new BinaryOperator("DepartmentCode", user.Staff.StaffDepartment.DepartmentCode, BinaryOperatorType.Equal));
                }
            }
        }

        private SystemUsers _CreateUser;
        [XafDisplayName("Create User")]
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
        [Appearance("CreateUser", Enabled = false)]
        [Index(300), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public SystemUsers CreateUser
        {
            get { return _CreateUser; }
            set
            {
                SetPropertyValue("CreateUser", ref _CreateUser, value);
            }
        }

        private DateTime? _CreateDate;
        [Index(301), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("CreateDate", Enabled = false)]
        public DateTime? CreateDate
        {
            get { return _CreateDate; }
            set
            {
                SetPropertyValue("CreateDate", ref _CreateDate, value);
            }
        }

        private SystemUsers _UpdateUser;
        [XafDisplayName("Update User"), ToolTip("Enter Text")]
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
        [Appearance("UpdateUser", Enabled = false)]
        [Index(302), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public SystemUsers UpdateUser
        {
            get { return _UpdateUser; }
            set
            {
                SetPropertyValue("UpdateUser", ref _UpdateUser, value);
            }
        }

        private DateTime? _UpdateDate;
        [Index(303), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("UpdateDate", Enabled = false)]
        public DateTime? UpdateDate
        {
            get { return _UpdateDate; }
            set
            {
                SetPropertyValue("UpdateDate", ref _UpdateDate, value);
            }
        }

        private DocTypes _DocType;
        [Appearance("DocType", Enabled = false, Criteria = "not IsNew")]
        [Index(304), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public virtual DocTypes DocType
        {
            get { return _DocType; }
            set
            {
                SetPropertyValue("DocType", ref _DocType, value);
            }
        }

        private string _DocNum;
        [XafDisplayName("PO No.")]
        [Appearance("DocNum", Enabled = false)]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public string DocNum
        {
            get { return _DocNum; }
            set
            {
                SetPropertyValue("DocNum", ref _DocNum, value);
            }
        }

        private vwVendors _VendorCode;
        [NoForeignKey]
        [ImmediatePostData]
        [DataSourceCriteria("ValidFor = 'Y' and Expenditure = '@this.ExpenditureType.ExpenditureTypeCode'")]
        [XafDisplayName("Vendor Code")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Index(5), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwVendors VendorCode
        {
            get { return _VendorCode; }
            set
            {
                SetPropertyValue("VendorCode", ref _VendorCode, value);
                if (!IsLoading && value != null)
                {
                    VendorName = VendorCode.CardName.ToUpper().ToString();
                }
                else if (!IsLoading && value == null)
                {
                    VendorName = null;
                }
            }
        }

        private string _VendorName;
        [XafDisplayName("Vendor Name.")]
        [Index(8), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        [Appearance("VendorName", Enabled = false)]
        public string VendorName
        {
            get { return _VendorName; }
            set
            {
                SetPropertyValue("VendorName", ref _VendorName, value);
            }
        }

        private vwWarehouse _Warehouse;
        [NoForeignKey]
        [ImmediatePostData]
        [DataSourceCriteria("Inactive = 'N'")]
        [XafDisplayName("Warehouse")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Index(9), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwWarehouse Warehouse
        {
            get { return _Warehouse; }
            set
            {
                SetPropertyValue("Warehouse", ref _Warehouse, value);
            }
        }

        private Department _Department;
        [ImmediatePostData]
        //[DataSourceCriteria("(Entity.MasterData = '@this.Entity.MasterData')")]
        [Appearance("Department", Enabled = false)]
        [XafDisplayName("Department")]
        [Index(10), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public Department Department
        {
            get { return _Department; }
            set
            {
                SetPropertyValue("Department", ref _Department, value);
            }
        }

        private ExpenditureType _ExpenditureType;
        [ImmediatePostData]
        [DataSourceCriteria("IsActive = 'True'")]
        [XafDisplayName("Expenditure Type")]
        [Index(11), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public ExpenditureType ExpenditureType
        {
            get { return _ExpenditureType; }
            set
            {
                SetPropertyValue("ExpenditureType", ref _ExpenditureType, value);
            }
        }

        private vwItemGroup _ItemGroup;
        [NoForeignKey]
        [ImmediatePostData]
        [XafDisplayName("ItemGroup")]
        [Index(12), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwItemGroup ItemGroup
        {
            get { return _ItemGroup; }
            set
            {
                SetPropertyValue("ItemGroup", ref _ItemGroup, value);
            }
        }

        private DateTime _DocDate;
        [XafDisplayName("Doc Date")]
        [Index(15), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime DocDate
        {
            get { return _DocDate; }
            set
            {
                SetPropertyValue("DocDate", ref _DocDate, value);
            }
        }

        private DateTime _PostingDate;
        [XafDisplayName("Posting Date")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Index(20), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime PostingDate
        {
            get { return _PostingDate; }
            set
            {
                SetPropertyValue("PostingDate", ref _PostingDate, value);
            }
        }

        private DocStatus _DocStatus;
        [XafDisplayName("Doc Status")]
        [Appearance("DocStatus", Enabled = false)]
        [Index(18), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DocStatus DocStatus
        {
            get { return _DocStatus; }
            set
            {
                SetPropertyValue("DocStatus", ref _DocStatus, value);
            }
        }

        private ApprovalStatusType _ApprovalStatus;
        [Index(20), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("ApprovalStatus", Enabled = false)]
        public ApprovalStatusType ApprovalStatus
        {
            get { return _ApprovalStatus; }
            set
            {
                SetPropertyValue("ApprovalStatus", ref _ApprovalStatus, value);
            }
        }

        private CompanyAddress _CompanyAddress;
        [XafDisplayName("Delivery Address")]
        [Index(23), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [RuleRequiredField(DefaultContexts.Save)]
        public CompanyAddress CompanyAddress
        {
            get { return _CompanyAddress; }
            set
            {
                SetPropertyValue("CompanyAddress", ref _CompanyAddress, value);
            }
        }

        private string _Attn;
        [XafDisplayName("Attn")]
        [Size(25000)]
        [Index(25), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Attn
        {
            get { return _Attn; }
            set
            {
                SetPropertyValue("Attn", ref _Attn, value);
            }
        }

        private string _VehicleNo;
        [XafDisplayName("Vehicle No")]
        [Index(28), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string VehicleNo
        {
            get { return _VehicleNo; }
            set
            {
                SetPropertyValue("VehicleNo", ref _VehicleNo, value);
            }
        }

        private decimal _Total;
        [XafDisplayName("Total")]
        [Appearance("Total", Enabled = false)]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n3}")]
        [Index(80), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public decimal Total
        {
            get
            {
                if (Session.IsObjectsSaving != true)
                {
                    decimal rtn = 0;
                    if (PurchaseOrderDetails != null)
                        rtn += PurchaseOrderDetails.Sum(p => p.SubTotal);

                    return rtn;
                }
                else
                {
                    return _Total;
                }
            }
            set
            {
                SetPropertyValue("Total", ref _Total, value);
            }
        }

        private string _Remarks;
        [XafDisplayName("Remarks")]
        [Size(254)]
        [Index(81), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Remarks
        {
            get { return _Remarks; }
            set
            {
                SetPropertyValue("Remarks", ref _Remarks, value);
            }
        }

        private bool _Sap;
        [XafDisplayName("Sap")]
        [Index(82), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public bool Sap
        {
            get { return _Sap; }
            set
            {
                SetPropertyValue("Sap", ref _Sap, value);
            }
        }

        private string _NextApprover;
        [ImmediatePostData]
        [XafDisplayName("Next Approver")]
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
        [Index(83), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("Next Approver", Enabled = false)]
        public string NextApprover
        {
            get { return _NextApprover; }
            set
            {
                SetPropertyValue("NextApprover", ref _NextApprover, value);
            }
        }

        private string _WhoApprove;
        [ImmediatePostData]
        [XafDisplayName("WhoApprove")]
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
        [Index(84), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("WhoApprove", Enabled = false)]
        public string WhoApprove
        {
            get { return _WhoApprove; }
            set
            {
                SetPropertyValue("WhoApprove", ref _WhoApprove, value);
            }
        }

        private string _AppUser;
        [ImmediatePostData]
        [XafDisplayName("AppUser")]
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
        [Index(85), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("AppUser", Enabled = false)]
        public string AppUser
        {
            get { return _AppUser; }
            set
            {
                SetPropertyValue("AppUser", ref _AppUser, value);
            }
        }

        [Browsable(false)]
        public bool IsNew
        {
            get
            { return Session.IsNewObject(this); }
        }

        [Browsable(false)]
        public bool IsValid
        {
            get
            {
                int count = 0;
                foreach (PurchaseOrderAttachment dtl in this.PurchaseOrderAttachment)
                {
                    count += 1;
                }

                if (count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Association("PurchaseOrders-PurchaseOrderDetails")]
        [XafDisplayName("Items")]
        public XPCollection<PurchaseOrderDetails> PurchaseOrderDetails
        {
            get { return GetCollection<PurchaseOrderDetails>("PurchaseOrderDetails"); }
        }

        [Association("PurchaseOrders-PurchaseOrderDocStatus")]
        [XafDisplayName("Document Status")]
        public XPCollection<PurchaseOrderDocStatus> PurchaseOrderDocStatus
        {
            get { return GetCollection<PurchaseOrderDocStatus>("PurchaseOrderDocStatus"); }
        }

        [Association("PurchaseOrders-PurchaseOrderAppStatus")]
        [XafDisplayName("Approval Info")]
        public XPCollection<PurchaseOrderAppStatus> PurchaseOrderAppStatus
        {
            get { return GetCollection<PurchaseOrderAppStatus>("PurchaseOrderAppStatus"); }
        }

        [Association("PurchaseOrders-PurchaseOrderAppStage")]
        [XafDisplayName("Approval Stage")]
        public XPCollection<PurchaseOrderAppStage> PurchaseOrderAppStage
        {
            get { return GetCollection<PurchaseOrderAppStage>("PurchaseOrderAppStage"); }
        }

        [Association("PurchaseOrders-PurchaseOrderAttachment")]
        [XafDisplayName("Attachments")]
        public XPCollection<PurchaseOrderAttachment> PurchaseOrderAttachment
        {
            get { return GetCollection<PurchaseOrderAttachment>("PurchaseOrderAttachment"); }
        }

        private XPCollection<AuditDataItemPersistent> auditTrail;
        public XPCollection<AuditDataItemPersistent> AuditTrail
        {
            get
            {
                if (auditTrail == null)
                {
                    auditTrail = AuditedObjectWeakReference.GetAuditTrail(Session, this);
                }
                return auditTrail;
            }
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (!(Session is NestedUnitOfWork)
                && (Session.DataLayer != null)
                    && (Session.ObjectLayer is SimpleObjectLayer)
                        )
            {
                SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;
                if (user != null)
                {
                    UpdateUser = Session.GetObjectByKey<SystemUsers>(user.Oid);
                }
                UpdateDate = DateTime.Now;

                if (Session.IsNewObject(this))
                {
                    PurchaseOrderDocStatus ds = new PurchaseOrderDocStatus(Session);
                    ds.DocStatus = DocStatus.New;
                    ds.DocRemarks = "";
                    if (user != null)
                    {
                        ds.CreateUser = Session.GetObjectByKey<SystemUsers>(user.Oid);
                        ds.UpdateUser = Session.GetObjectByKey<SystemUsers>(user.Oid);
                    }
                    else
                    {
                        PermissionPolicyUser getuser = Session.FindObject<PermissionPolicyUser>(CriteriaOperator.Parse("UserName = ?", "Admin"));

                        ds.CreateUser = Session.GetObjectByKey<SystemUsers>(getuser.Oid);
                        ds.UpdateUser = Session.GetObjectByKey<SystemUsers>(getuser.Oid);
                    }
                    ds.CreateDate = DateTime.Now;
                    ds.UpdateDate = DateTime.Now;
                    this.PurchaseOrderDocStatus.Add(ds);
                }
            }
        }
    }
}