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
using DevExpress.ExpressApp.ConditionalAppearance;
using WingHinPortal.Module.BusinessObjects.Setup;
using WingHinPortal.Module.BusinessObjects.View;

namespace WingHinPortal.Module.BusinessObjects.PR
{
    [DefaultClassOptions]
    [XafDisplayName("Purchase Request")]
    [NavigationItem("Purchase Request")]
    [DefaultProperty("DocNum")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "PurchaseRequest_ListView_PendApp")]
    [Appearance("HideNew1", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "PurchaseRequest_ListView_Approved")]
    [Appearance("HideNew2", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "PurchaseRequest_DetailView_PendApp")]
    [Appearance("HideNew3", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "PurchaseRequest_DetailView_Approved")]

    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Criteria = "not (DocStatus in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmit", AppearanceItemType.Action, "True", TargetItems = "SubmitPR", Criteria = "not (DocStatus in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "CancelPR", Criteria = "(not (DocStatus in (0)) and ApprovalStatus != 'Approved') or (IsValid1 and not (DocStatus in (0)) and ApprovalStatus = 'Approved')", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideApproval", AppearanceItemType = "Action", TargetItems = "ApprovePR", Criteria = "(ApprovalStatus != 'Required_Approval')", Context = "Any", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("HideApproval1", AppearanceItemType = "Action", TargetItems = "ApprovePR", Context = "PurchaseRequest_DetailView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("HideApproval2", AppearanceItemType = "Action", TargetItems = "ApprovePR", Context = "PurchaseRequest_DetailView_Approved", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]

    public class PurchaseRequest : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public PurchaseRequest(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
            SystemUsers user = (SystemUsers)SecuritySystem.CurrentUser;
            CreateUser = Session.GetObjectByKey<SystemUsers>(user.Oid);
            CreateDate = DateTime.Now;
            RequestDate = DateTime.Now;
            DocDate = DateTime.Now;
            DocType = Session.FindObject<DocTypes>
                    (new BinaryOperator("BoCode", DocTypeList.PurchaseRequests, BinaryOperatorType.Equal));
            if (user.Warehouse != null)
            {
                Warehouse = Session.FindObject<vwWarehouse>
                    (new BinaryOperator("BoCode", user.Warehouse.BoCode, BinaryOperatorType.Equal));
            }

            if (user.Staff != null)
            {
                Department = Session.FindObject<Department>
                    (new BinaryOperator("DepartmentCode", user.Staff.StaffDepartment.DepartmentCode, BinaryOperatorType.Equal));
                Requestor = Session.FindObject<StaffInfo>
                    (new BinaryOperator("StaffID", user.Staff.StaffID, BinaryOperatorType.Equal));
            }
            ApprovalStatus = ApprovalStatusType.Not_Applicable;
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
        [XafDisplayName("PR No.")]
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
        //[RuleRequiredField(DefaultContexts.Save)]
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
        //[Appearance("VendorName", Enabled = false)]
        public string VendorName
        {
            get { return _VendorName; }
            set
            {
                SetPropertyValue("VendorName", ref _VendorName, value);
            }
        }

        private Department _Department;
        [ImmediatePostData]
        [DataSourceCriteria("IsActive = 'True'")]
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

        private vwWarehouse _Warehouse;
        [NoForeignKey]
        [ImmediatePostData]
        [DataSourceCriteria("Inactive = 'N'")]
        [XafDisplayName("Warehouse")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Index(12), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwWarehouse Warehouse
        {
            get { return _Warehouse; }
            set
            {
                SetPropertyValue("Warehouse", ref _Warehouse, value);
            }
        }

        private StaffInfo _Requestor;
        [ImmediatePostData]
        [XafDisplayName("Requestor")]
        [Appearance("Requestor", Enabled = false)]
        [Index(13), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public StaffInfo Requestor
        {
            get { return _Requestor; }
            set
            {
                SetPropertyValue("Requestor", ref _Requestor, value);
                if (!IsLoading && value != null)
                {
                    Department = Session.FindObject<Department>(CriteriaOperator.Parse("Oid = ?", Requestor.StaffDepartment.Oid));
                }
            }
        }

        private vwItemGroup _ItemGroup;
        [NoForeignKey]
        [ImmediatePostData]
        [XafDisplayName("ItemGroup")]
        [Index(14), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Appearance("DocDate", Enabled = false)]
        [Index(15), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime DocDate
        {
            get { return _DocDate; }
            set
            {
                SetPropertyValue("DocDate", ref _DocDate, value);
            }
        }

        private DateTime _RequestDate;
        [XafDisplayName("Request Date")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Appearance("RequestDate", Enabled = false)]
        [Index(18), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public DateTime RequestDate
        {
            get { return _RequestDate; }
            set
            {
                SetPropertyValue("RequestDate", ref _RequestDate, value);
            }
        }

        private DocStatus _DocStatus;
        [XafDisplayName("Doc Status")]
        [Appearance("DocStatus", Enabled = false)]
        [Index(20), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DocStatus DocStatus
        {
            get { return _DocStatus; }
            set
            {
                SetPropertyValue("DocStatus", ref _DocStatus, value);
            }
        }

        private ApprovalStatusType _ApprovalStatus;
        [Index(23), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
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
        [Index(25), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
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
        [Index(28), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Attn
        {
            get { return _Attn; }
            set
            {
                SetPropertyValue("Attn", ref _Attn, value);
            }
        }

        private decimal _Total;
        [XafDisplayName("Total")]
        [Appearance("Total", Enabled = false)]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n3}")]
        [Index(80), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public decimal Total
        {
            get
            {
                if (Session.IsObjectsSaving != true)
                {
                    decimal rtn = 0;
                    if (PurchaseRequestDetail != null)
                        rtn += PurchaseRequestDetail.Sum(p => p.SubTotal);

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

        private string _NextApprover;
        [ImmediatePostData]
        [XafDisplayName("Next Approver")]
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
        [Index(82), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
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
        [Index(83), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
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
        [Index(84), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
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
                foreach (PurchaseRequestAttachment dtl in this.PurchaseRequestAttachment)
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

        [Browsable(false)]
        public bool IsValid1
        {
            get
            {
                if (this.PurchaseRequestDetail.Where(x => x.OpenQuantity != x.Quantity).Count() > 0)
                {
                    return true;
                }

                return false;
            }
        }

        [Association("PurchaseRequest-PurchaseRequestDetails")]
        [XafDisplayName("Items")]
        public XPCollection<PurchaseRequestDetails> PurchaseRequestDetail
        {
            get { return GetCollection<PurchaseRequestDetails>("PurchaseRequestDetail"); }
        }

        [Association("PurchaseRequest-PurchaseRequestDocStatus")]
        [XafDisplayName("Document Status")]
        public XPCollection<PurchaseRequestDocStatus> PurchaseRequestDocStatus
        {
            get { return GetCollection<PurchaseRequestDocStatus>("PurchaseRequestDocStatus"); }
        }

        [Association("PurchaseRequest-PurchaseRequestAppStatus")]
        [XafDisplayName("Approval Info")]
        public XPCollection<PurchaseRequestAppStatus> PurchaseRequestAppStatus
        {
            get { return GetCollection<PurchaseRequestAppStatus>("PurchaseRequestAppStatus"); }
        }

        [Association("PurchaseRequest-PurchaseRequestAppStage")]
        [XafDisplayName("Approval Stage")]
        public XPCollection<PurchaseRequestAppStage> PurchaseRequestAppStage
        {
            get { return GetCollection<PurchaseRequestAppStage>("PurchaseRequestAppStage"); }
        }

        [Association("PurchaseRequest-PurchaseRequestAttachment")]
        [XafDisplayName("Attachments")]
        public XPCollection<PurchaseRequestAttachment> PurchaseRequestAttachment
        {
            get { return GetCollection<PurchaseRequestAttachment>("PurchaseRequestAttachment"); }
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
                UpdateUser = Session.GetObjectByKey<SystemUsers>(SecuritySystem.CurrentUserId);
                UpdateDate = DateTime.Now;

                if (Session.IsNewObject(this))
                {
                    PurchaseRequestDocStatus ds = new PurchaseRequestDocStatus(Session);
                    ds.DocStatus = DocStatus.New;
                    ds.DocRemarks = "";
                    ds.CreateUser = Session.GetObjectByKey<SystemUsers>(SecuritySystem.CurrentUserId);
                    ds.CreateDate = DateTime.Now;
                    ds.UpdateUser = Session.GetObjectByKey<SystemUsers>(SecuritySystem.CurrentUserId);
                    ds.UpdateDate = DateTime.Now;
                    this.PurchaseRequestDocStatus.Add(ds);

                }
            }
        }
    }
}