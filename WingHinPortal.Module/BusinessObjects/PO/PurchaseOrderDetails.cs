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
using WingHinPortal.Module.BusinessObjects.View;
using WingHinPortal.Module.BusinessObjects.Setup;

// 20250827 - allow negative quantity and no allow negative unit price - ver 0.1
// 20260512 - add cost center 2 - ver 1.0.2

namespace WingHinPortal.Module.BusinessObjects.PO
{
    [DefaultClassOptions]
    //[Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    //[Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("LinkDoc", AppearanceItemType = "Action", TargetItems = "Link", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("UnlinkDoc", AppearanceItemType = "Action", TargetItems = "Unlink", Context = "ListView", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [XafDisplayName("Purchase Order Details")]
    public class PurchaseOrderDetails : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public PurchaseOrderDetails(Session session)
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
            CreateDate = DateTime.Now;

            //Tax = Session.FindObject<vwTax>(new BinaryOperator("BoCode", "X0"));
            Quantity = 1;
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
                    Tax = Session.FindObject<vwTax>(CriteriaOperator.Parse("BoCode = ?", Item.PuchaseTax));

                    vwSupplierPrice tempprice;
                    tempprice = Session.FindObject<vwSupplierPrice>(CriteriaOperator.Parse("ItemCode = ?", Item.ItemCode));

                    if (tempprice != null)
                    {
                        Unitprice = tempprice.Price;
                    }
                }
                else if (!IsLoading && value == null)
                {
                    ItemDesc = null;
                    UOM = null;
                    Unitprice = 0;
                    Tax = null;
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

        private string _Vehicle;
        [XafDisplayName("Vehicle")]
        [Index(7), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public string Vehicle
        {
            get { return _Vehicle; }
            set
            {
                SetPropertyValue("Vehicle", ref _Vehicle, value);
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
                    if (BaseOid != null)
                    {
                        if (OpenQuantity > 0)
                        {
                            if (Quantity > OpenQuantity)
                            {
                                Quantity = OpenQuantity;
                            }
                        }
                    }
                    else
                    {
                        OpenQuantity = Quantity;
                    }

                    TaxAmount = (Quantity * Unitprice) * (TaxRate / 100);
                    if (Discount > 0)
                    {
                        SubTotalWithoutTax = Quantity * Unitprice - ((Discount / 100) * (Quantity * Unitprice + TaxAmount));
                        SubTotal = Quantity * Unitprice + TaxAmount - ((Discount / 100) * (Quantity * Unitprice + TaxAmount));
                    }
                    else
                    {
                        SubTotalWithoutTax = Quantity * Unitprice;
                        SubTotal = Quantity * Unitprice + TaxAmount;
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

        private decimal _Discount;
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n3}")]
        [ModelDefault("EditMask", "n3")]
        [XafDisplayName("Discount %")]
        [Index(10), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal Discount
        {
            get { return _Discount; }
            set
            {
                SetPropertyValue("Discount", ref _Discount, value);
                if (!IsLoading && value > 0)
                {
                    if (SubTotalWithoutTax != Quantity * Unitprice - ((Discount / 100) * (Quantity * Unitprice + TaxAmount)))
                    {
                        SubTotalWithoutTax = Quantity * Unitprice - ((Discount / 100) * (Quantity * Unitprice + TaxAmount));
                    }
                    if (SubTotal != Quantity * Unitprice + TaxAmount - ((Discount / 100) * (Quantity * Unitprice + TaxAmount)))
                    {
                        SubTotal = Quantity * Unitprice + TaxAmount - ((Discount / 100) * (Quantity * Unitprice + TaxAmount));
                    }
                }
                if (!IsLoading && value <= 0)
                {
                    if (SubTotalWithoutTax != Quantity * Unitprice)
                    {
                        SubTotalWithoutTax = Quantity * Unitprice;
                    }
                    if (SubTotal != Quantity * Unitprice + TaxAmount)
                    {
                        SubTotal = Quantity * Unitprice + TaxAmount;
                    }
                }
            }
        }

        private decimal _Unitprice;
        [ImmediatePostData]
        [XafDisplayName("Unit Price")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n3}")]
        [ModelDefault("EditMask", "n3")]
        [Index(11), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal Unitprice
        {
            get { return _Unitprice; }
            set
            {
                SetPropertyValue("Unitprice", ref _Unitprice, value);
                if (!IsLoading)
                {
                    // Start ver 0.1
                    if (Unitprice < 0)
                    {
                        Unitprice = 0;
                    }
                    // End ver 0.1

                    TaxAmount = (Quantity * Unitprice) * (TaxRate / 100);
                    if (Discount > 0)
                    {
                        SubTotalWithoutTax = Quantity * Unitprice - ((Discount / 100) * (Quantity * Unitprice + TaxAmount));
                        SubTotal = Quantity * Unitprice + TaxAmount - ((Discount / 100) * (Quantity * Unitprice + TaxAmount));
                    }
                    else
                    {
                        SubTotalWithoutTax = Quantity * Unitprice;
                        SubTotal = Quantity * Unitprice + TaxAmount;
                    }
                }
            }
        }

        private vwTax _Tax;
        [NoForeignKey]
        [XafDisplayName("Tax")]
        [Index(13), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        public vwTax Tax
        {
            get { return _Tax; }
            set
            {
                SetPropertyValue("Tax", ref _Tax, value);
                if (!IsLoading && value != null)
                {
                    TaxRate = Tax.Rate;
                }
                else if (!IsLoading && value == null)
                {
                    TaxRate = 0;
                }
            }
        }

        private decimal _TaxRate;
        [ImmediatePostData]
        [XafDisplayName("Tax Rate")]
        [Appearance("TaxRate", Enabled = false)]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n3}")]
        [ModelDefault("EditMask", "n3")]
        [Index(15), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        public decimal TaxRate
        {
            get { return _TaxRate; }
            set
            {
                SetPropertyValue("TaxRate", ref _TaxRate, value);
                if (!IsLoading && value != 0)
                {
                    TaxAmount = (Quantity * Unitprice) * (TaxRate / 100);
                    if (Discount > 0)
                    {
                        SubTotalWithoutTax = Quantity * Unitprice - ((Discount / 100) * (Quantity * Unitprice + TaxAmount));
                        SubTotal = Quantity * Unitprice + TaxAmount - ((Discount / 100) * (Quantity * Unitprice + TaxAmount));
                    }
                    else
                    {
                        SubTotalWithoutTax = Quantity * Unitprice;
                        SubTotal = Quantity * Unitprice + TaxAmount;
                    }
                }
            }
        }

        private decimal _TaxAmount;
        [XafDisplayName("Tax Amount")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n3}")]
        [ModelDefault("EditMask", "n3")]
        [Appearance("TaxAmount", Enabled = false)]
        [Index(21), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        public decimal TaxAmount
        {
            get { return _TaxAmount; }
            set
            {
                SetPropertyValue("TaxAmount", ref _TaxAmount, value);
            }
        }

        private decimal _SubTotalWithoutTax;
        [XafDisplayName("SubTotal w/o Tax")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n3}")]
        [ModelDefault("EditMask", "n3")]
        [Appearance("SubTotalWithoutTax", Enabled = false)]
        [Index(23), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        public decimal SubTotalWithoutTax
        {
            get { return _SubTotalWithoutTax; }
            set
            {
                SetPropertyValue("SubTotalWithoutTax", ref _SubTotalWithoutTax, value);
            }
        }

        private decimal _SubTotal;
        [XafDisplayName("SubTotal")]
        [DbType("numeric(18,6)")]
        [ImmediatePostData]
        [ModelDefault("DisplayFormat", "{0:n3}")]
        [ModelDefault("EditMask", "n3")]
        //[Appearance("SubTotal", Enabled = false)]
        [Index(25), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public decimal SubTotal
        {
            get { return _SubTotal; }
            set
            {
                SetPropertyValue("SubTotal", ref _SubTotal, value);
                if (!IsLoading && value != 0)
                {
                    Discount = ((Quantity * Unitprice + TaxAmount) - SubTotal) / (Quantity * Unitprice + TaxAmount) * 100;
                }
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

        private ExpenditureType _ExpenditureType;
        [ImmediatePostData]
        [XafDisplayName("Expenditure Type")]
        [Index(33), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [RuleRequiredField(DefaultContexts.Save)]
        [DataSourceCriteria("Expenditure = '@this.ExpenditureType.ExpenditureTypeCode'")]
        [Index(35), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
        public vwItemGroup ItemGroup
        {
            get { return _ItemGroup; }
            set
            {
                SetPropertyValue("ItemGroup", ref _ItemGroup, value);
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
        [Index(38), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
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
        [Index(40), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
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

        private PurchaseOrders _PurchaseOrders;
        [Association("PurchaseOrders-PurchaseOrderDetails")]
        [Index(99), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("PurchaseOrders", Enabled = false)]
        public PurchaseOrders PurchaseOrders
        {
            get { return _PurchaseOrders; }
            set { SetPropertyValue("PurchaseOrders", ref _PurchaseOrders, value); }
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