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

// 20260512 - add cost center 2 - ver 1.0.2

namespace WingHinPortal.Module.BusinessObjects.GoodsIssue
{
    [DefaultClassOptions]
    //[Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    //[Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("LinkDoc", AppearanceItemType = "Action", TargetItems = "Link", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("UnlinkDoc", AppearanceItemType = "Action", TargetItems = "Unlink", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [XafDisplayName("Goods Issue Details")]
    public class GoodsIssueDetails : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public GoodsIssueDetails(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
            CreateUser = Session.GetObjectByKey<SystemUsers>(SecuritySystem.CurrentUserId);
            CreateDate = DateTime.Now;

            //Tax = Session.FindObject<vwTax>(new BinaryOperator("BoCode", "X0"));
            Quantity = 1;
            // Start ver 1.0.2
            if (CreateUser.Staff != null)
            {
            // End ver 1.0.2
                if (CreateUser.Staff.CostCenter != null)
                {
                    if (DocType != null)
                    {
                        if (DocType.Dimension1 == true)
                        {
                            CostCenter = Session.FindObject<vwCostCenter>(new BinaryOperator("PrcCode", CreateUser.Staff.CostCenter.PrcCode));
                        }
                    }
                }

                // Start ver 1.0.2
                if (CreateUser.Staff.SubCostCenter != null)
                {
                    if (DocType != null)
                    {
                        if (DocType.Dimension2 == true)
                        {
                            SubCostCenter = Session.FindObject<vwSubCostCenter>(new BinaryOperator("PrcCode", CreateUser.Staff.SubCostCenter.PrcCode));
                        }
                    }
                }
                // End ver 1.0.2
            // Start ver 1.0.2
            }
            // End ver 1.0.2
        }

        private SystemUsers _CreateUser;
        [XafDisplayName("Create User")]
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
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
        public DateTime? UpdateDate
        {
            get { return _UpdateDate; }
            set
            {
                SetPropertyValue("UpdateDate", ref _UpdateDate, value);
            }
        }

        private vwItemMasters _Item;
        [ImmediatePostData]
        [NoForeignKey]
        [DataSourceCriteria("frozenFor = 'N' and U_EXPENDITURETYPE = '@this.ExpenditureType.ExpenditureTypeCode' and " +
            "U_ItemGroup = '@this.ItemGroup.Code'")]
        [XafDisplayName("Item")]
        [Index(0), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        [Appearance("Item", Enabled = false, Criteria = "not IsNew")]
        [RuleRequiredField(DefaultContexts.Save)]
        public vwItemMasters Item

        {
            get { return _Item; }
            set
            {
                SetPropertyValue("Item", ref _Item, value);
                if (!IsLoading && value != null)
                {
                    ItemDesc = Item.ItemName;
                    UOM = Item.UOM;
                }
                else if (!IsLoading && value == null)
                {
                    ItemDesc = null;
                    UOM = null;
                }
            }
        }

        private string _ItemDesc;
        [RuleRequiredField(DefaultContexts.Save)]
        [XafDisplayName("Item Description")]
        [Index(3), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string ItemDesc
        {
            get { return _ItemDesc; }
            set
            {
                SetPropertyValue("ItemDesc", ref _ItemDesc, value);
            }
        }

        private string _ItemDetails;
        [XafDisplayName("Item Details")]
        [Index(5), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string ItemDetails
        {
            get { return _ItemDetails; }
            set
            {
                SetPropertyValue("ItemDetails", ref _ItemDetails, value);
            }
        }

        private string _UOM;
        [XafDisplayName("UOM")]
        [Appearance("UOM", Enabled = false)]
        [Index(6), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string UOM
        {
            get { return _UOM; }
            set
            {
                SetPropertyValue("UOM", ref _UOM, value);
            }
        }

        private decimal _Quantity;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n3}")]
        [ModelDefault("EditMask", "n3")]
        [XafDisplayName("Quantity")]
        [Index(8), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal Quantity
        {
            get { return _Quantity; }
            set
            {
                SetPropertyValue("Quantity", ref _Quantity, value);
                if (!IsLoading)
                {
                    if (OpenQuantity > 0)
                    {
                        if (Quantity > OpenQuantity)
                        {
                            Quantity = OpenQuantity;
                        }
                    }
                }
            }
        }

        private decimal _OpenQuantity;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n3}")]
        [ModelDefault("EditMask", "n3")]
        [XafDisplayName("Open Quantity")]
        [Index(9), VisibleInListView(true), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public decimal OpenQuantity
        {
            get { return _OpenQuantity; }
            set
            {
                SetPropertyValue("OpenQuantity", ref _OpenQuantity, value);
            }
        }

        private ExpenditureType _ExpenditureType;
        [ImmediatePostData]
        [XafDisplayName("Expenditure Type")]
        [Index(13), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [DataSourceCriteria("Expenditure = '@this.ExpenditureType.ExpenditureTypeCode'")]
        [Index(15), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwItemGroup ItemGroup
        {
            get { return _ItemGroup; }
            set
            {
                SetPropertyValue("ItemGroup", ref _ItemGroup, value);
            }
        }

        private string _BaseDoc;
        [XafDisplayName("BaseDoc")]
        [Index(28), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public string BaseDoc
        {
            get { return _BaseDoc; }
            set
            {
                SetPropertyValue("BaseDoc", ref _BaseDoc, value);
            }
        }

        private string _BaseOid;
        [XafDisplayName("BaseOid")]
        [Index(29), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public string BaseOid
        {
            get { return _BaseOid; }
            set
            {
                SetPropertyValue("BaseOid", ref _BaseOid, value);
            }
        }

        private vwCostCenter _CostCenter;
        [NoForeignKey]
        [ImmediatePostData]
        [XafDisplayName("Cost Center")]
        // Start ver 1.0.2
        //[RuleRequiredField(DefaultContexts.Save)]
        [RuleRequiredField(DefaultContexts.Save, TargetCriteria = "DocType.Dimension1 = 'True'")]
        // End ver 1.0.2
        [DataSourceCriteria("IsActive = 'Y'")]
        [Index(30), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwCostCenter CostCenter
        {
            get { return _CostCenter; }
            set
            {
                SetPropertyValue("CostCenter", ref _CostCenter, value);
            }
        }

        // Start ver 1.0.2
        private vwSubCostCenter _SubCostCenter;
        [NoForeignKey]
        [ImmediatePostData]
        [XafDisplayName("Sub Cost Center")]
        //[RuleRequiredField(DefaultContexts.Save, TargetCriteria = "DocType.Dimension2 = 'True'")]
        [DataSourceCriteria("IsActive = 'Y'")]
        [Appearance("SubCostCenter", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Criteria = "DocType.Dimension2 != 'True'")]
        [Index(33), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwSubCostCenter SubCostCenter
        {
            get { return _SubCostCenter; }
            set
            {
                SetPropertyValue("SubCostCenter", ref _SubCostCenter, value);
            }
        }

        private DocTypes _DocType;
        [ImmediatePostData]
        [Appearance("DocType", Enabled = false)]
        [Index(80), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public virtual DocTypes DocType
        {
            get { return _DocType; }
            set
            {
                SetPropertyValue("DocType", ref _DocType, value);
            }
        }
        // End ver 1.0.2

        [Browsable(false)]
        public bool IsNew
        {
            get
            { return Session.IsNewObject(this); }
        }

        private GoodsIssue _GoodsIssue;
        [Association("GoodsIssue-GoodsIssueDetails")]
        [Index(99), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("GoodsIssue", Enabled = false)]
        public GoodsIssue GoodsIssue
        {
            get { return _GoodsIssue; }
            set { SetPropertyValue("GoodsIssue", ref _GoodsIssue, value); }
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

                }
            }
        }
    }
}