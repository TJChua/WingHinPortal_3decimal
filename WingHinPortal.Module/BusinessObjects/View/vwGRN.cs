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

// 20260512 - add cost center 2 - ver 1.0.2

namespace WingHinPortal.Module.BusinessObjects.View
{
    [DefaultClassOptions]
    [XafDisplayName("GRN")]
    [DefaultProperty("DocNum")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("hideSave", AppearanceItemType = "Action", TargetItems = "Save", Context = "Any", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    //[Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    //[Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    public class vwGRN : XPLiteObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public vwGRN(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        [Key]
        [Browsable(true)]
        [VisibleInLookupListView(false), VisibleInListView(false), VisibleInDetailView(false)]
        [Appearance("PriKey", Enabled = false)]
        public string PriKey { get; set; }

        [VisibleInLookupListView(true), VisibleInListView(true), VisibleInDetailView(true)]
        [Appearance("DocNum", Enabled = false)]
        public string DocNum { get; set; }

        [VisibleInLookupListView(true), VisibleInListView(true), VisibleInDetailView(true)]
        [Appearance("Baseline", Enabled = false)]
        public int Baseline { get; set; }

        [VisibleInLookupListView(true), VisibleInListView(true), VisibleInDetailView(true)]
        [Appearance("Item", Enabled = false)]
        public string Item { get; set; }

        [VisibleInLookupListView(false), VisibleInListView(false), VisibleInDetailView(false)]
        [Appearance("ItemDesc", Enabled = false)]
        public string ItemDesc { get; set; }

        [VisibleInLookupListView(false), VisibleInListView(false), VisibleInDetailView(false)]
        [Appearance("ItemDetails", Enabled = false)]
        public string ItemDetails { get; set; }

        [VisibleInLookupListView(true), VisibleInListView(true), VisibleInDetailView(true)]
        [Appearance("Quantity", Enabled = false)]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        public decimal Quantity { get; set; }

        [VisibleInLookupListView(true), VisibleInListView(true), VisibleInDetailView(true)]
        [Appearance("Unitprice", Enabled = false)]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        public decimal Unitprice { get; set; }

        [VisibleInLookupListView(false), VisibleInListView(false), VisibleInDetailView(false)]
        [Appearance("VendorCode", Enabled = false)]
        public string VendorCode { get; set; }

        [VisibleInLookupListView(false), VisibleInListView(false), VisibleInDetailView(false)]
        [Appearance("PortalDocNum", Enabled = false)]
        public string PortalDocNum { get; set; }

        [VisibleInLookupListView(true), VisibleInListView(true), VisibleInDetailView(true)]
        [Appearance("PostingDate", Enabled = false)]
        public DateTime PostingDate { get; set; }

        // Start ver 1.0.2
        [VisibleInLookupListView(true), VisibleInListView(true), VisibleInDetailView(false)]
        [Appearance("CostCenter", Enabled = false)]
        public string CostCenter { get; set; }

        [VisibleInLookupListView(true), VisibleInListView(true), VisibleInDetailView(false)]
        [Appearance("SubCostCenter", Enabled = false)]
        public string SubCostCenter { get; set; }
        // End ver 1.0.2
    }
}