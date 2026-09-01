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

namespace WingHinPortal.Module.BusinessObjects
{
    public enum CrReportEnum
    {
        STRING = 0,
        DATE = 1
    }

    public enum DocTypeList
    {
        [XafDisplayName("Purchase Requests")] PurchaseRequests = 0,
        [XafDisplayName("Purchase Orders")] PurchaseOrders = 1,
        [XafDisplayName("Goods Receipt Note")] GRN = 2,
        [XafDisplayName("Goods Issue")] GI = 3,
        [XafDisplayName("Blanket Agreement")] BA = 4
    }

    public enum DocStatus
    {
        New = 0,
        Submit = 1,
        Cancel = 2,
        [XafDisplayName("Pending Approve")] PendingApp = 3,
        Approved = 4,
        Rejected = 5,
        Posted = 6,
        Closed = 7,
        Terminated = 8
    }

    public enum ApprovalStatusType
    {
        Not_Applicable = 0,
        Approved = 1,
        Required_Approval = 2,
        Rejected = 3
    }

    public enum ApprovalActions
    {
        [XafDisplayName("Please Select Action...")] NA = 0,
        [XafDisplayName("Approve")] Yes = 1,
        [XafDisplayName("Reject")] No = 2
    }

    public enum ApprovalTypes
    {
        Document = 0
    }

    public enum Billing
    {
        [XafDisplayName("Please Select Action...")] NA = 0,
        Monthly = 1,
        [XafDisplayName("Bi-Monthly")]  BiMonthly = 2,
        [XafDisplayName("Half Yearly")] HalfYearly = 3,
        Annually = 4
    }
}