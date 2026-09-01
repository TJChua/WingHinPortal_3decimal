using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WingHinPortal.Module.BusinessObjects.GoodsIssue;
using WingHinPortal.Module.BusinessObjects.GoodsReceipt;
using WingHinPortal.Module.BusinessObjects.PO;
using WingHinPortal.Module.BusinessObjects.PR;
using WingHinPortal.Module.BusinessObjects.PurchaseBlanketAgreement;
using WingHinPortal.Module.BusinessObjects.Setup;
using WingHinPortal.Module.BusinessObjects.View;

// 20260512 - add cost center 2 - ver 1.0.2

namespace PortalIntegration
{
    static class Program
    {
        private static System.Threading.Mutex mutex = null;
        [STAThread]
        static void Main()
        {
            const string appName = "WH PR Portal Integration";
            bool createdNew;

            mutex = new System.Threading.Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Application.Exit();
                return;
            }

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);

            RegisterEntities();
            AuthenticationStandard authentication = new AuthenticationStandard();
            SecurityStrategyComplex security = new SecurityStrategyComplex(typeof(PermissionPolicyUser), typeof(PermissionPolicyRole), authentication);
            //security.RegisterXPOAdapterProviders();
            string connectionString = ConfigurationManager.ConnectionStrings["DataSourceConnectionString"].ConnectionString;
            IObjectSpaceProvider objectSpaceProvider = new SecuredObjectSpaceProvider(security, connectionString, null);

            #region Allow Store Proc
            ((SecuredObjectSpaceProvider)objectSpaceProvider).AllowICommandChannelDoWithSecurityContext = true;
            #endregion

            DevExpress.Persistent.Base.PasswordCryptographer.EnableRfc2898 = true;
            DevExpress.Persistent.Base.PasswordCryptographer.SupportLegacySha512 = false;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            PortalIntegration mainForm = new PortalIntegration(security, objectSpaceProvider);

            //mainForm.defuserid = ConfigurationManager.AppSettings["DataSourceUserID"].ToString();
            //mainForm.defpassword = ConfigurationManager.AppSettings["DataSourcePassword"].ToString();
            string temp = ConfigurationManager.AppSettings["AutoPostAfterLogin"].ToString().ToUpper();
            if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                mainForm.autopostafterlogin = true;
            else
                mainForm.autopostafterlogin = false;

            temp = "";
            temp = ConfigurationManager.AppSettings["AutoLogin"].ToString().ToUpper();
            if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                mainForm.autologin = true;
            else
                mainForm.autologin = false;

            Application.Run(mainForm);
        }

        private static void RegisterEntities()
        {
            XpoTypesInfoHelper.GetXpoTypeInfoSource();

            XafTypesInfo.Instance.RegisterEntity(typeof(GoodsIssue));
            XafTypesInfo.Instance.RegisterEntity(typeof(GoodsIssueDetails));
            XafTypesInfo.Instance.RegisterEntity(typeof(GoodsIssueAttachment));
            XafTypesInfo.Instance.RegisterEntity(typeof(GoodsIssueDocStatus));

            XafTypesInfo.Instance.RegisterEntity(typeof(GoodsReceipt));
            XafTypesInfo.Instance.RegisterEntity(typeof(GoodsReceiptDetails));
            XafTypesInfo.Instance.RegisterEntity(typeof(GoodsReceiptAttachment));
            XafTypesInfo.Instance.RegisterEntity(typeof(GoodsReceiptDocStatus));

            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseOrders));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseOrderDetails));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseOrderAttachment));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseOrderAppStage));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseOrderAppStatus));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseOrderDocStatus));

            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseRequest));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseRequestAttachment));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseRequestDetails));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseRequestAppStage));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseRequestAppStatus));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseRequestDocStatus));

            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseBlanketAgreement));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseBlanketAgreementDetails));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseBlanketAgreementAttachment));
            XafTypesInfo.Instance.RegisterEntity(typeof(PurchaseBlanketAgreementDocStatus));

            XafTypesInfo.Instance.RegisterEntity(typeof(vwCostCenter));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwGRN));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwItemGroup));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwItemMasters));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwPO));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwPR));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwPriceList));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwPRInternalGI));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwPRInternalPO));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwStockBalance));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwSupplierPrice));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwTax));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwVendors));
            XafTypesInfo.Instance.RegisterEntity(typeof(vwWarehouse));
            // Start ver 1.0.2
            XafTypesInfo.Instance.RegisterEntity(typeof(vwSubCostCenter));
            // End ver 1.0.2

            XafTypesInfo.Instance.RegisterEntity(typeof(Approvals));
            XafTypesInfo.Instance.RegisterEntity(typeof(ApprovalUsers));
            XafTypesInfo.Instance.RegisterEntity(typeof(CompanyAddress));
            XafTypesInfo.Instance.RegisterEntity(typeof(Department));
            XafTypesInfo.Instance.RegisterEntity(typeof(DocTypes));
            XafTypesInfo.Instance.RegisterEntity(typeof(ExpenditureType));
            XafTypesInfo.Instance.RegisterEntity(typeof(StaffInfo));

            XafTypesInfo.Instance.RegisterEntity(typeof(SystemUsers));
            XafTypesInfo.Instance.RegisterEntity(typeof(PermissionPolicyUser));
            XafTypesInfo.Instance.RegisterEntity(typeof(PermissionPolicyRole));
        }
    }
}
