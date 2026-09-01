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

namespace WingHinPortal.Module.BusinessObjects.PurchaseBlanketAgreement
{
    [DefaultClassOptions]
    [XafDisplayName("Purchase Blanket Agreement")]
    [NavigationItem("Purchase Blanket Agreement")]
    [DefaultProperty("DocNum")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Criteria = "not (DocStatus in (0, 1))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmit", AppearanceItemType.Action, "True", TargetItems = "SubmitPBA", Criteria = "not (DocStatus in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "CancelPBA", Criteria = "not (DocStatus in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideTerminate", AppearanceItemType.Action, "True", TargetItems = "TerminatePBA", Criteria = "not (DocStatus in (1))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    [RuleCriteria("BillingValidation", DefaultContexts.Save, "IsValid1 = 1", "Please select billing type.")]

    public class PurchaseBlanketAgreement : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public PurchaseBlanketAgreement(Session session)
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
            DocDate = DateTime.Now;
            DocType = Session.FindObject<DocTypes>
                (new BinaryOperator("BoCode", DocTypeList.BA, BinaryOperatorType.Equal));
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
        [XafDisplayName("No.")]
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
        [Appearance("VendorCode", Enabled = false, Criteria = "(DocStatus in (1))")]
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

        private vwWarehouse _Warehouse;
        [NoForeignKey]
        [ImmediatePostData]
        [DataSourceCriteria("Inactive = 'N'")]
        [XafDisplayName("Warehouse")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Appearance("Warehouse", Enabled = false, Criteria = "(DocStatus in (1))")]
        [Index(11), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwWarehouse Warehouse
        {
            get { return _Warehouse; }
            set
            {
                SetPropertyValue("Warehouse", ref _Warehouse, value);
            }
        }

        private DateTime _DocDate;
        [XafDisplayName("Doc Date")]
        [Appearance("DocDate", Enabled = false, Criteria = "(DocStatus in (1))")]
        [Index(13), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime DocDate
        {
            get { return _DocDate; }
            set
            {
                SetPropertyValue("DocDate", ref _DocDate, value);
            }
        }

        private Billing _Billing;
        [XafDisplayName("Billing")]
        [Appearance("Billing", Enabled = false, Criteria = "(DocStatus in (1))")]
        [Index(15), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public Billing Billing
        {
            get { return _Billing; }
            set
            {
                SetPropertyValue("Billing", ref _Billing, value);
            }
        }

        private DateTime _StartDate;
        [ImmediatePostData]
        [XafDisplayName("Start Date")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Appearance("StartDate", Enabled = false, Criteria = "(DocStatus in (1))")]
        [Index(18), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime StartDate
        {
            get { return _StartDate; }
            set
            {
                SetPropertyValue("StartDate", ref _StartDate, value);
                if (!IsLoading)
                {
                    PreviousDate = StartDate;
                }
            }
        }

        private DateTime _EndDate;
        [XafDisplayName("End Date")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Appearance("EndDate", Enabled = false, Criteria = "(DocStatus in (1))")]
        [Index(20), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime EndDate
        {
            get { return _EndDate; }
            set
            {
                SetPropertyValue("EndDate", ref _EndDate, value);
            }
        }


        private DateTime _PreviousDate;
        [XafDisplayName("Previous Date")]
        [Appearance("PreviousDate", Enabled = false, Criteria = "(DocStatus in (1))")]
        [Index(21), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime PreviousDate
        {
            get { return _PreviousDate; }
            set
            {
                SetPropertyValue("PreviousDate", ref _PreviousDate, value);
            }
        }

        private DateTime _ManualDate;
        [XafDisplayName("Manual Date")]
        //[Appearance("ManualDate", Enabled = false, Criteria = "(DocStatus in (1))")]
        [Index(22), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime ManualDate
        {
            get { return _ManualDate; }
            set
            {
                SetPropertyValue("ManualDate", ref _ManualDate, value);
            }
        }

        private DocStatus _DocStatus;
        [XafDisplayName("Doc Status")]
        [Appearance("DocStatus", Enabled = false)]
        [Index(23), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public DocStatus DocStatus
        {
            get { return _DocStatus; }
            set
            {
                SetPropertyValue("DocStatus", ref _DocStatus, value);
            }
        }

        private DateTime _TerminateDate;
        [XafDisplayName("Terminate Date")]
        [Index(24), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime TerminateDate
        {
            get { return _TerminateDate; }
            set
            {
                SetPropertyValue("TerminateDate", ref _TerminateDate, value);
            }
        }

        private ExpenditureType _ExpenditureType;
        [ImmediatePostData]
        [DataSourceCriteria("IsActive = 'True'")]
        [XafDisplayName("Expenditure Type")]
        [Appearance("ExpenditureType", Enabled = false, Criteria = "(DocStatus in (1))")]
        [Index(25), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
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
        [Appearance("ItemGroup", Enabled = false, Criteria = "(DocStatus in (1))")]
        [Index(28), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwItemGroup ItemGroup
        {
            get { return _ItemGroup; }
            set
            {
                SetPropertyValue("ItemGroup", ref _ItemGroup, value);
            }
        }

        private CompanyAddress _CompanyAddress;
        [XafDisplayName("Delivery Address")]
        [Appearance("CompanyAddress", Enabled = false, Criteria = "(DocStatus in (1))")]
        [Index(30), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [RuleRequiredField(DefaultContexts.Save)]
        public CompanyAddress CompanyAddress
        {
            get { return _CompanyAddress; }
            set
            {
                SetPropertyValue("CompanyAddress", ref _CompanyAddress, value);
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
                    if (PurchaseBlanketAgreementDetails != null)
                        rtn += PurchaseBlanketAgreementDetails.Sum(p => p.SubTotal);

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
        [Appearance("Remarks", Enabled = false, Criteria = "(DocStatus in (1))")]
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
                foreach (PurchaseBlanketAgreementAttachment dtl in this.PurchaseBlanketAgreementAttachment)
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
                if (this.Billing == Billing.NA)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [Association("PurchaseBlanketAgreement-PurchaseBlanketAgreementDetails")]
        [XafDisplayName("Items")]
        [Appearance("PurchaseBlanketAgreementDetails", Enabled = false, Criteria = "(DocStatus in (1))")]
        public XPCollection<PurchaseBlanketAgreementDetails> PurchaseBlanketAgreementDetails
        {
            get { return GetCollection<PurchaseBlanketAgreementDetails>("PurchaseBlanketAgreementDetails"); }
        }

        [Association("PurchaseBlanketAgreement-PurchaseBlanketAgreementDocStatus")]
        [XafDisplayName("Document Status")]
        public XPCollection<PurchaseBlanketAgreementDocStatus> PurchaseBlanketAgreementDocStatus
        {
            get { return GetCollection<PurchaseBlanketAgreementDocStatus>("PurchaseBlanketAgreementDocStatus"); }
        }

        [Association("PurchaseBlanketAgreement-PurchaseBlanketAgreementAttachment")]
        [XafDisplayName("Attachments")]
        //[Appearance("PurchaseBlanketAgreementAttachment", Enabled = false, Criteria = "(DocStatus in (1))")]
        public XPCollection<PurchaseBlanketAgreementAttachment> PurchaseBlanketAgreementAttachment
        {
            get { return GetCollection<PurchaseBlanketAgreementAttachment>("PurchaseBlanketAgreementAttachment"); }
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
                    PurchaseBlanketAgreementDocStatus ds = new PurchaseBlanketAgreementDocStatus(Session);
                    ds.DocStatus = DocStatus.New;
                    ds.DocRemarks = "";
                    ds.CreateUser = Session.GetObjectByKey<SystemUsers>(SecuritySystem.CurrentUserId);
                    ds.CreateDate = DateTime.Now;
                    ds.UpdateUser = Session.GetObjectByKey<SystemUsers>(SecuritySystem.CurrentUserId);
                    ds.UpdateDate = DateTime.Now;
                    this.PurchaseBlanketAgreementDocStatus.Add(ds);
                }
            }
        }
    }
}