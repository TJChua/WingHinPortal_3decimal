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

#region update log
// YKW - 20200212 - comments

#endregion

namespace WingHinPortal.Module.BusinessObjects
{
    //[DefaultClassOptions]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (https://documentation.devexpress.com/#eXpressAppFramework/CustomDocument112701).
    public class CrReportParams : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        public CrReportParams(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
            ParamType = CrReportEnum.STRING;

        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
        }
        //private string _PersistentProperty;
        //[XafDisplayName("My display name"), ToolTip("My hint message")]
        //[ModelDefault("EditMask", "(000)-00"), Index(0), VisibleInListView(false)]
        //[Persistent("DatabaseColumnName"), RuleRequiredField(DefaultContexts.Save)]
        //public string PersistentProperty {
        //    get { return _PersistentProperty; }
        //    set { SetPropertyValue("PersistentProperty", ref _PersistentProperty, value); }
        //}

        //[Action(Caption = "My UI Action", ConfirmationMessage = "Are you sure?", ImageName = "Attention", AutoCommit = true)]
        //public void ActionMethod() {
        //    // Trigger a custom business logic for the current record in the UI (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112619.aspx).
        //    this.PersistentProperty = "Paid";
        //}
        private CrReports _CrReport;
        [Browsable(false)]
        [Association("CrReport-Param")]
        public CrReports CrReport
        {
            get { return _CrReport; }
            set { SetPropertyValue("CrReport", ref _CrReport, value); }
        }

        private string _ParamCode;
        [Index(0), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string ParamCode
        {
            get { return _ParamCode; }
            set { SetPropertyValue("ParamCode", ref _ParamCode, value); }
        }

        private string _ParamName;
        [Index(1)]
        public string ParamName
        {
            get { return _ParamName; }
            set { SetPropertyValue("ParamName", ref _ParamName, value); }
        }



        private CrReportEnum _ParamType;
        [Index(2), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public CrReportEnum ParamType
        {
            get { return _ParamType; }
            set { SetPropertyValue("ParamType", ref _ParamType, value); }
        }

        private string _StringParam;
        [NonPersistent]
        [Index(3), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(true)]
        //[Appearance("ParamString", BackColor = "Black", Criteria = "ParamType != 0")]
        public string StringParam
        {
            get { return _StringParam; }
            set { SetPropertyValue("StringParam", ref _StringParam, value); }
        }


        private string _ParamSQL;
        [Size(SizeAttribute.Unlimited)]
        [XafDisplayName("ParamSQL")]
        [Index(20), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        public string ParamSQL
        {
            get { return _ParamSQL; }
            set
            {
                SetPropertyValue("ParamSQL", ref _ParamSQL, value);
            }
        }

        //[NonPersistent]
        //[Index(4), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(true)]
        //[Appearance("ParamDate", BackColor = "Black", Criteria = "ParamType != 1")]
        //public string ParamDate { get; set; }

        //[NonPersistent]
        //[Index(5), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(true)]
        //[Appearance("ParamInt", BackColor = "Black", Criteria = "ParamType != 2")]
        //public string ParamInt { get; set; }
    }
}