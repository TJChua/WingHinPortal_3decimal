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

namespace WingHinPortal.Module.BusinessObjects.View
{
    [DefaultClassOptions]
    [NavigationItem("SAP")]
    [XafDisplayName("Vendors")]
    [DefaultProperty("BoFullName")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideLink", AppearanceItemType.Action, "True", TargetItems = "Link", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideUnlink", AppearanceItemType.Action, "True", TargetItems = "Unlink", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("hideSave", AppearanceItemType = "Action", TargetItems = "Save", Context = "Any", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    //[Appearance("HideResetViewSetting", AppearanceItemType.Action, "True", TargetItems = "ResetViewSettings", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    //[Appearance("HideExport", AppearanceItemType.Action, "True", TargetItems = "Export", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideRefresh", AppearanceItemType.Action, "True", TargetItems = "Refresh", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    public class vwVendors : XPLiteObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public vwVendors(Session session)
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

        [XafDisplayName("CardCode")]
        [Appearance("CardCode", Enabled = false)]
        [Index(0)]
        public string CardCode
        {
            get; set;
        }

        [XafDisplayName("CardName")]
        [Appearance("CardName", Enabled = false)]
        [Index(3)]
        public string CardName
        {
            get; set;
        }

        [XafDisplayName("ValidFor")]
        [Appearance("ValidFor", Enabled = false)]
        [Index(5), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public string ValidFor
        {
            get; set;
        }

        [VisibleInLookupListView(false), VisibleInListView(false), VisibleInDetailView(false)]
        [Appearance("Currency", Enabled = false)]
        public string Currency { get; set; }

        [VisibleInLookupListView(false), VisibleInListView(true), VisibleInDetailView(true)]
        [Appearance("Emails", Enabled = false)]
        public string Emails { get; set; }

        [VisibleInLookupListView(false), VisibleInListView(true), VisibleInDetailView(true)]
        [Appearance("Expenditure", Enabled = false)]
        public string Expenditure { get; set; }

        [Index(30), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(true)]
        public string BoFullName
        {
            get { return CardCode + "-" + CardName; }
        }
    }
}