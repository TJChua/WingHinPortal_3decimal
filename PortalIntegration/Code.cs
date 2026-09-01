using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingHinPortal.Module.BusinessObjects;
using WingHinPortal.Module.BusinessObjects.GoodsIssue;
using WingHinPortal.Module.BusinessObjects.GoodsReceipt;
using WingHinPortal.Module.BusinessObjects.PO;
using WingHinPortal.Module.BusinessObjects.PurchaseBlanketAgreement;
using WingHinPortal.Module.BusinessObjects.Setup;
using WingHinPortal.Module.BusinessObjects.View;
using Department = WingHinPortal.Module.BusinessObjects.Setup.Department;

// 20260512 - add cost center 2 - ver 1.0.2

namespace PortalIntegration
{
    class Code
    {
        private SortedDictionary<string, List<string>> logs = new SortedDictionary<string, List<string>>();
        private DateTime nulldate;

        public Code(SecurityStrategyComplex security, IObjectSpaceProvider ObjectSpaceProvider)
        {
            logs.Clear();
            WriteLog("[Log]", "--------------------------------------------------------------------------------");
            WriteLog("[Log]", "Post Begin:[" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt") + "]");

            #region Connect to SAP  
            SAPCompany sap = new SAPCompany();
            if (sap.connectSAP())
            {
                WriteLog("[Log]", "Connected to SAP:[" + sap.oCom.CompanyName + "] Time:[" + DateTime.Now.ToString("hh:mm:ss tt") + "]");
            }
            else
            {
                WriteLog("[Error]", "SAP Connection:[" + sap.oCom.CompanyDB + "] Message:[" + sap.errMsg + "] Time:[" + DateTime.Now.ToString("hh: mm:ss tt") + "]");
                sap.oCom = null;
                goto EndApplication;
            }
            #endregion

            try
            {
                string temp = "";
                IObjectSpace ListObjectSpace = ObjectSpaceProvider.CreateObjectSpace();
                IObjectSpace securedObjectSpace = ObjectSpaceProvider.CreateObjectSpace();

                temp = ConfigurationManager.AppSettings["POPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--PO Posting Start--");

                    #region PO 
                    IList<PurchaseOrders> polist = ListObjectSpace.GetObjects<PurchaseOrders>
                    (CriteriaOperator.Parse("Sap = ? and ((DocStatus = ? and ApprovalStatus = ?) or (DocStatus = ? and ApprovalStatus = ?))",
                    0, 1, 1, 1, 0));

                    foreach (PurchaseOrders dtlpo in polist)
                    {
                        try
                        {
                            IObjectSpace poos = ObjectSpaceProvider.CreateObjectSpace();
                            PurchaseOrders poobj = poos.GetObjectByKey<PurchaseOrders>(dtlpo.Oid);

                            #region Post PO
                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                            int temppo = 0;

                            temppo = PostPOtoSAP(poobj, ObjectSpaceProvider, sap);
                            if (temppo == 1)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                poobj.Sap = true;

                                PurchaseOrderDocStatus ds = poos.CreateObject<PurchaseOrderDocStatus>();
                                ds.CreateUser = poos.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = dtlpo.DocStatus;
                                ds.DocRemarks = "Posted SAP";
                                poobj.PurchaseOrderDocStatus.Add(ds);

                                GC.Collect();
                            }
                            else if (temppo <= 0)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                GC.Collect();
                            }
                            #endregion

                            poos.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            WriteLog("[Error]", "Message: POST PO Failed - OID : " + dtlpo.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--PO Posting End--");
                }

                temp = ConfigurationManager.AppSettings["GRNPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--GRN Posting Start--");

                    #region GRN
                    IList<GoodsReceipt> grnlist = ListObjectSpace.GetObjects<GoodsReceipt>
                        (CriteriaOperator.Parse("Sap = ? and DocStatus = ?", 0, 1));

                    foreach (GoodsReceipt dtlgrn in grnlist)
                    {
                        try
                        {
                            IObjectSpace grnos = ObjectSpaceProvider.CreateObjectSpace();
                            GoodsReceipt grnobj = grnos.GetObjectByKey<GoodsReceipt>(dtlgrn.Oid);

                            #region Post GRN
                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                            int tempgrn = 0;

                            tempgrn = PostGRNtoSAP(grnobj, ObjectSpaceProvider, sap);
                            if (tempgrn == 1)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                grnobj.Sap = true;

                                GoodsReceiptDocStatus ds = grnos.CreateObject<GoodsReceiptDocStatus>();
                                ds.CreateUser = grnos.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = dtlgrn.DocStatus;
                                ds.DocRemarks = "Posted SAP";
                                grnobj.GoodsReceiptDocStatus.Add(ds);

                                GC.Collect();
                            }
                            else if (tempgrn <= 0)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                GC.Collect();
                            }
                            #endregion

                            grnos.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            WriteLog("[Error]", "Message: GRN Post Failed - OID : " + dtlgrn.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--GRN Posting End--");
                }

                temp = ConfigurationManager.AppSettings["GIPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--GI Posting Start--");

                    #region GI
                    IList<GoodsIssue> gilist = ListObjectSpace.GetObjects<GoodsIssue>
                        (CriteriaOperator.Parse("Sap = ? and DocStatus = ?", 0, 1));

                    foreach (GoodsIssue dtlgi in gilist)
                    {
                        try
                        {
                            IObjectSpace grnos = ObjectSpaceProvider.CreateObjectSpace();
                            GoodsIssue giobj = grnos.GetObjectByKey<GoodsIssue>(dtlgi.Oid);

                            #region Post GI
                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                            int tempgrn = 0;

                            tempgrn = PostGItoSAP(giobj, ObjectSpaceProvider, sap);
                            if (tempgrn == 1)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                giobj.Sap = true;

                                GoodsIssueDocStatus ds = grnos.CreateObject<GoodsIssueDocStatus>();
                                ds.CreateUser = grnos.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = dtlgi.DocStatus;
                                ds.DocRemarks = "Posted SAP";
                                giobj.GoodsIssueDocStatus.Add(ds);

                                GC.Collect();
                            }
                            else if (tempgrn <= 0)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                GC.Collect();
                            }
                            #endregion

                            grnos.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            WriteLog("[Error]", "Message: GI Post Failed - OID : " + dtlgi.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--GI Posting End--");
                }

                temp = ConfigurationManager.AppSettings["BAPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--PO BA Posting Start--");

                    #region PO BA
                    IList<PurchaseBlanketAgreement> pobalist = ListObjectSpace.GetObjects<PurchaseBlanketAgreement>
                    (CriteriaOperator.Parse("? >= StartDate and ? <= EndDate",
                    DateTime.Now.Date, DateTime.Now.Date));

                    foreach (PurchaseBlanketAgreement dtlpoba in pobalist)
                    {
                        try
                        {
                            IObjectSpace pobaos = ObjectSpaceProvider.CreateObjectSpace();
                            PurchaseBlanketAgreement pobaobj = pobaos.GetObjectByKey<PurchaseBlanketAgreement>(dtlpoba.Oid);

                            if (pobaobj.DocStatus != DocStatus.Terminated)
                            {
                                if (pobaobj.Billing == Billing.Monthly)
                                {
                                    if (DateTime.Now.Date == pobaobj.PreviousDate.AddMonths(1).Date)
                                    {
                                        //#region Post PO BA
                                        //if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                        //int temppo = 0;

                                        //temppo = PostPOBAtoSAP(pobaobj, ObjectSpaceProvider, sap);
                                        //if (temppo == 1)
                                        //{
                                        //    if (sap.oCom.InTransaction)
                                        //        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                        //    pobaobj.Sap = true;
                                        //    pobaobj.PreviousDate = DateTime.Now;

                                        //    PurchaseBlanketAgreementDocStatus ds = pobaos.CreateObject<PurchaseBlanketAgreementDocStatus>();
                                        //    ds.CreateUser = pobaos.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                                        //    ds.CreateDate = DateTime.Now;
                                        //    ds.DocStatus = dtlpoba.DocStatus;
                                        //    ds.DocRemarks = "Posted SAP";
                                        //    pobaobj.PurchaseBlanketAgreementDocStatus.Add(ds);

                                        //    GC.Collect();
                                        //}
                                        //else if (temppo <= 0)
                                        //{
                                        //    if (sap.oCom.InTransaction)
                                        //        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                        //    GC.Collect();
                                        //}
                                        //#endregion

                                        #region Add PO
                                        pobaobj.PreviousDate = DateTime.Now;
                                        IObjectSpace addos = ObjectSpaceProvider.CreateObjectSpace();
                                        PurchaseOrders newPO = addos.CreateObject<PurchaseOrders>();

                                        IObjectSpace docos = ObjectSpaceProvider.CreateObjectSpace();
                                        DocTypes number = docos.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.PurchaseOrders));

                                        number.CurrectDocNum = number.NextDocNum;
                                        number.NextDocNum = number.NextDocNum + 1;

                                        newPO.DocNum = "PO" + number.CurrectDocNum;

                                        docos.CommitChanges();

                                        if (pobaobj.VendorCode != null)
                                        {
                                            newPO.VendorCode = addos.GetObjectByKey<vwVendors>(pobaobj.VendorCode.PriKey);
                                        }
                                        if (pobaobj.Warehouse != null)
                                        {
                                            newPO.Warehouse = addos.GetObjectByKey<vwWarehouse>(pobaobj.Warehouse.BoCode);
                                        }
                                        if (pobaobj.Department != null)
                                        {
                                            newPO.Department = addos.GetObjectByKey<Department>(pobaobj.Department.Oid);
                                        }
                                        if (pobaobj.ExpenditureType != null)
                                        {
                                            newPO.ExpenditureType = addos.GetObjectByKey<ExpenditureType>(pobaobj.ExpenditureType.Oid);
                                        }
                                        if (pobaobj.ItemGroup != null)
                                        {
                                            newPO.ItemGroup = addos.GetObjectByKey<vwItemGroup>(pobaobj.ItemGroup.Code);
                                        }
                                        if (pobaobj.CompanyAddress != null)
                                        {
                                            newPO.CompanyAddress = addos.GetObjectByKey<CompanyAddress>(pobaobj.CompanyAddress.Oid);
                                        }
                                        newPO.DocStatus = DocStatus.Submit;

                                        PurchaseOrderDetails newPOItem = addos.CreateObject<PurchaseOrderDetails>();

                                        foreach (PurchaseBlanketAgreementDetails dtl in pobaobj.PurchaseBlanketAgreementDetails)
                                        {
                                            newPOItem.Item = addos.FindObject<vwItemMasters>(CriteriaOperator.Parse("ItemCode = ?", dtl.Item.ItemCode));
                                            newPOItem.ItemDesc = dtl.ItemDesc;
                                            newPOItem.ItemDetails = dtl.ItemDetails;
                                            newPOItem.Unitprice = dtl.Unitprice;
                                            if (dtl.ExpenditureType != null)
                                            {
                                                newPOItem.ExpenditureType = addos.GetObjectByKey<ExpenditureType>(dtl.ExpenditureType.Oid);
                                            }
                                            if (dtl.ItemGroup != null)
                                            {
                                                newPOItem.ItemGroup = addos.GetObjectByKey<vwItemGroup>(dtl.ItemGroup.Code);
                                            }
                                            if (dtl.CostCenter != null)
                                            {
                                                newPOItem.CostCenter = addos.GetObjectByKey<vwCostCenter>(dtl.CostCenter.PrcCode);
                                            }
                                            // Start ver 1.0.2
                                            if (dtl.SubCostCenter != null)
                                            {
                                                newPOItem.SubCostCenter = addos.GetObjectByKey<vwSubCostCenter>(dtl.SubCostCenter.PrcCode);
                                            }
                                            // End ver 1.0.2
                                            newPOItem.BaseDoc = pobaobj.DocNum;
                                            newPOItem.BaseOid = dtl.Oid.ToString();
                                            newPOItem.OpenQuantity = dtl.Quantity;
                                            newPOItem.Quantity = dtl.Quantity;
                                            if (dtl.Tax != null)
                                            {
                                                newPOItem.Tax = addos.GetObjectByKey<vwTax>(dtl.Tax.BoCode);
                                            }
                                            newPOItem.Discount = dtl.Discount;
                                            newPOItem.Vehicle = dtl.Vehicle;

                                            newPO.PurchaseOrderDetails.Add(newPOItem);
                                        }

                                        addos.CommitChanges();
                                        #endregion

                                        pobaos.CommitChanges();
                                    }
                                }
                                else if (pobaobj.Billing == Billing.BiMonthly)
                                {
                                    if (DateTime.Now.Date == pobaobj.PreviousDate.AddMonths(2).Date)
                                    {
                                        //            #region Post PO BA
                                        //            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                        //            int temppo = 0;

                                        //            temppo = PostPOBAtoSAP(pobaobj, ObjectSpaceProvider, sap);
                                        //            if (temppo == 1)
                                        //            {
                                        //                if (sap.oCom.InTransaction)
                                        //                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                        //                pobaobj.Sap = true;
                                        //                pobaobj.PreviousDate = DateTime.Now;

                                        //                PurchaseBlanketAgreementDocStatus ds = pobaos.CreateObject<PurchaseBlanketAgreementDocStatus>();
                                        //                ds.CreateUser = pobaos.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                                        //                ds.CreateDate = DateTime.Now;
                                        //                ds.DocStatus = dtlpoba.DocStatus;
                                        //                ds.DocRemarks = "Posted SAP";
                                        //                pobaobj.PurchaseBlanketAgreementDocStatus.Add(ds);

                                        //                GC.Collect();
                                        //            }
                                        //            else if (temppo <= 0)
                                        //            {
                                        //                if (sap.oCom.InTransaction)
                                        //                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                        //                GC.Collect();
                                        //            }
                                        //            #endregion

                                        #region Add PO
                                        pobaobj.PreviousDate = DateTime.Now;
                                        IObjectSpace addos = ObjectSpaceProvider.CreateObjectSpace();
                                        PurchaseOrders newPO = addos.CreateObject<PurchaseOrders>();

                                        IObjectSpace docos = ObjectSpaceProvider.CreateObjectSpace();
                                        DocTypes number = docos.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.PurchaseOrders));

                                        number.CurrectDocNum = number.NextDocNum;
                                        number.NextDocNum = number.NextDocNum + 1;

                                        newPO.DocNum = "PO" + number.CurrectDocNum;

                                        docos.CommitChanges();

                                        if (pobaobj.VendorCode != null)
                                        {
                                            newPO.VendorCode = addos.GetObjectByKey<vwVendors>(pobaobj.VendorCode.PriKey);
                                        }
                                        if (pobaobj.Warehouse != null)
                                        {
                                            newPO.Warehouse = addos.GetObjectByKey<vwWarehouse>(pobaobj.Warehouse.BoCode);
                                        }
                                        if (pobaobj.Department != null)
                                        {
                                            newPO.Department = addos.GetObjectByKey<Department>(pobaobj.Department.Oid);
                                        }
                                        if (pobaobj.ExpenditureType != null)
                                        {
                                            newPO.ExpenditureType = addos.GetObjectByKey<ExpenditureType>(pobaobj.ExpenditureType.Oid);
                                        }
                                        if (pobaobj.ItemGroup != null)
                                        {
                                            newPO.ItemGroup = addos.GetObjectByKey<vwItemGroup>(pobaobj.ItemGroup.Code);
                                        }
                                        if (pobaobj.CompanyAddress != null)
                                        {
                                            newPO.CompanyAddress = addos.GetObjectByKey<CompanyAddress>(pobaobj.CompanyAddress.Oid);
                                        }
                                        newPO.DocStatus = DocStatus.Submit;

                                        PurchaseOrderDetails newPOItem = addos.CreateObject<PurchaseOrderDetails>();

                                        foreach (PurchaseBlanketAgreementDetails dtl in pobaobj.PurchaseBlanketAgreementDetails)
                                        {
                                            newPOItem.Item = addos.FindObject<vwItemMasters>(CriteriaOperator.Parse("ItemCode = ?", dtl.Item.ItemCode));
                                            newPOItem.ItemDesc = dtl.ItemDesc;
                                            newPOItem.ItemDetails = dtl.ItemDetails;
                                            newPOItem.Unitprice = dtl.Unitprice;
                                            if (dtl.ExpenditureType != null)
                                            {
                                                newPOItem.ExpenditureType = addos.GetObjectByKey<ExpenditureType>(dtl.ExpenditureType.Oid);
                                            }
                                            if (dtl.ItemGroup != null)
                                            {
                                                newPOItem.ItemGroup = addos.GetObjectByKey<vwItemGroup>(dtl.ItemGroup.Code);
                                            }
                                            if (dtl.CostCenter != null)
                                            {
                                                newPOItem.CostCenter = addos.GetObjectByKey<vwCostCenter>(dtl.CostCenter.PrcCode);
                                            }
                                            // Start ver 1.0.2
                                            if (dtl.SubCostCenter != null)
                                            {
                                                newPOItem.SubCostCenter = addos.GetObjectByKey<vwSubCostCenter>(dtl.SubCostCenter.PrcCode);
                                            }
                                            // End ver 1.0.2
                                            newPOItem.BaseDoc = pobaobj.DocNum;
                                            newPOItem.BaseOid = dtl.Oid.ToString();
                                            newPOItem.OpenQuantity = dtl.Quantity;
                                            newPOItem.Quantity = dtl.Quantity;
                                            if (dtl.Tax != null)
                                            {
                                                newPOItem.Tax = addos.GetObjectByKey<vwTax>(dtl.Tax.BoCode);
                                            }
                                            newPOItem.Discount = dtl.Discount;
                                            newPOItem.Vehicle = dtl.Vehicle;

                                            newPO.PurchaseOrderDetails.Add(newPOItem);
                                        }

                                        addos.CommitChanges();
                                        #endregion

                                        pobaos.CommitChanges();
                                    }
                                }
                                else if (pobaobj.Billing == Billing.HalfYearly)
                                {
                                    if (DateTime.Now.Date == pobaobj.PreviousDate.AddMonths(6).Date)
                                    {
                                        //            #region Post PO BA
                                        //            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                        //            int temppo = 0;

                                        //            temppo = PostPOBAtoSAP(pobaobj, ObjectSpaceProvider, sap);
                                        //            if (temppo == 1)
                                        //            {
                                        //                if (sap.oCom.InTransaction)
                                        //                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                        //                pobaobj.Sap = true;
                                        //                pobaobj.PreviousDate = DateTime.Now;

                                        //                PurchaseBlanketAgreementDocStatus ds = pobaos.CreateObject<PurchaseBlanketAgreementDocStatus>();
                                        //                ds.CreateUser = pobaos.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                                        //                ds.CreateDate = DateTime.Now;
                                        //                ds.DocStatus = dtlpoba.DocStatus;
                                        //                ds.DocRemarks = "Posted SAP";
                                        //                pobaobj.PurchaseBlanketAgreementDocStatus.Add(ds);

                                        //                GC.Collect();
                                        //            }
                                        //            else if (temppo <= 0)
                                        //            {
                                        //                if (sap.oCom.InTransaction)
                                        //                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                        //                GC.Collect();
                                        //            }
                                        //            #endregion

                                        #region Add PO
                                        pobaobj.PreviousDate = DateTime.Now;
                                        IObjectSpace addos = ObjectSpaceProvider.CreateObjectSpace();
                                        PurchaseOrders newPO = addos.CreateObject<PurchaseOrders>();

                                        IObjectSpace docos = ObjectSpaceProvider.CreateObjectSpace();
                                        DocTypes number = docos.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.PurchaseOrders));

                                        number.CurrectDocNum = number.NextDocNum;
                                        number.NextDocNum = number.NextDocNum + 1;

                                        newPO.DocNum = "PO" + number.CurrectDocNum;

                                        docos.CommitChanges();

                                        if (pobaobj.VendorCode != null)
                                        {
                                            newPO.VendorCode = addos.GetObjectByKey<vwVendors>(pobaobj.VendorCode.PriKey);
                                        }
                                        if (pobaobj.Warehouse != null)
                                        {
                                            newPO.Warehouse = addos.GetObjectByKey<vwWarehouse>(pobaobj.Warehouse.BoCode);
                                        }
                                        if (pobaobj.Department != null)
                                        {
                                            newPO.Department = addos.GetObjectByKey<Department>(pobaobj.Department.Oid);
                                        }
                                        if (pobaobj.ExpenditureType != null)
                                        {
                                            newPO.ExpenditureType = addos.GetObjectByKey<ExpenditureType>(pobaobj.ExpenditureType.Oid);
                                        }
                                        if (pobaobj.ItemGroup != null)
                                        {
                                            newPO.ItemGroup = addos.GetObjectByKey<vwItemGroup>(pobaobj.ItemGroup.Code);
                                        }
                                        if (pobaobj.CompanyAddress != null)
                                        {
                                            newPO.CompanyAddress = addos.GetObjectByKey<CompanyAddress>(pobaobj.CompanyAddress.Oid);
                                        }
                                        newPO.DocStatus = DocStatus.Submit;

                                        PurchaseOrderDetails newPOItem = addos.CreateObject<PurchaseOrderDetails>();

                                        foreach (PurchaseBlanketAgreementDetails dtl in pobaobj.PurchaseBlanketAgreementDetails)
                                        {
                                            newPOItem.Item = addos.FindObject<vwItemMasters>(CriteriaOperator.Parse("ItemCode = ?", dtl.Item.ItemCode));
                                            newPOItem.ItemDesc = dtl.ItemDesc;
                                            newPOItem.ItemDetails = dtl.ItemDetails;
                                            newPOItem.Unitprice = dtl.Unitprice;
                                            if (dtl.ExpenditureType != null)
                                            {
                                                newPOItem.ExpenditureType = addos.GetObjectByKey<ExpenditureType>(dtl.ExpenditureType.Oid);
                                            }
                                            if (dtl.ItemGroup != null)
                                            {
                                                newPOItem.ItemGroup = addos.GetObjectByKey<vwItemGroup>(dtl.ItemGroup.Code);
                                            }
                                            if (dtl.CostCenter != null)
                                            {
                                                newPOItem.CostCenter = addos.GetObjectByKey<vwCostCenter>(dtl.CostCenter.PrcCode);
                                            }
                                            // Start ver 1.0.2
                                            if (dtl.SubCostCenter != null)
                                            {
                                                newPOItem.SubCostCenter = addos.GetObjectByKey<vwSubCostCenter>(dtl.SubCostCenter.PrcCode);
                                            }
                                            // End ver 1.0.2
                                            newPOItem.BaseDoc = pobaobj.DocNum;
                                            newPOItem.BaseOid = dtl.Oid.ToString();
                                            newPOItem.OpenQuantity = dtl.Quantity;
                                            newPOItem.Quantity = dtl.Quantity;
                                            if (dtl.Tax != null)
                                            {
                                                newPOItem.Tax = addos.GetObjectByKey<vwTax>(dtl.Tax.BoCode);
                                            }
                                            newPOItem.Discount = dtl.Discount;
                                            newPOItem.Vehicle = dtl.Vehicle;

                                            newPO.PurchaseOrderDetails.Add(newPOItem);
                                        }

                                        addos.CommitChanges();
                                        #endregion

                                        pobaos.CommitChanges();
                                    }
                                }
                                else if (pobaobj.Billing == Billing.Annually)
                                {
                                    if (DateTime.Now.Date == pobaobj.PreviousDate.AddYears(1).Date)
                                    {
                                        //            #region Post PO BA
                                        //            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                        //            int temppo = 0;

                                        //            temppo = PostPOBAtoSAP(pobaobj, ObjectSpaceProvider, sap);
                                        //            if (temppo == 1)
                                        //            {
                                        //                if (sap.oCom.InTransaction)
                                        //                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                        //                pobaobj.Sap = true;
                                        //                pobaobj.PreviousDate = DateTime.Now;

                                        //                PurchaseBlanketAgreementDocStatus ds = pobaos.CreateObject<PurchaseBlanketAgreementDocStatus>();
                                        //                ds.CreateUser = pobaos.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                                        //                ds.CreateDate = DateTime.Now;
                                        //                ds.DocStatus = dtlpoba.DocStatus;
                                        //                ds.DocRemarks = "Posted SAP";
                                        //                pobaobj.PurchaseBlanketAgreementDocStatus.Add(ds);

                                        //                GC.Collect();
                                        //            }
                                        //            else if (temppo <= 0)
                                        //            {
                                        //                if (sap.oCom.InTransaction)
                                        //                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                        //                GC.Collect();
                                        //            }
                                        //            #endregion

                                        #region Add PO
                                        pobaobj.PreviousDate = DateTime.Now;
                                        IObjectSpace addos = ObjectSpaceProvider.CreateObjectSpace();
                                        PurchaseOrders newPO = addos.CreateObject<PurchaseOrders>();

                                        IObjectSpace docos = ObjectSpaceProvider.CreateObjectSpace();
                                        DocTypes number = docos.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.PurchaseOrders));

                                        number.CurrectDocNum = number.NextDocNum;
                                        number.NextDocNum = number.NextDocNum + 1;

                                        newPO.DocNum = "PO" + number.CurrectDocNum;

                                        docos.CommitChanges();

                                        if (pobaobj.VendorCode != null)
                                        {
                                            newPO.VendorCode = addos.GetObjectByKey<vwVendors>(pobaobj.VendorCode.PriKey);
                                        }
                                        if (pobaobj.Warehouse != null)
                                        {
                                            newPO.Warehouse = addos.GetObjectByKey<vwWarehouse>(pobaobj.Warehouse.BoCode);
                                        }
                                        if (pobaobj.Department != null)
                                        {
                                            newPO.Department = addos.GetObjectByKey<Department>(pobaobj.Department.Oid);
                                        }
                                        if (pobaobj.ExpenditureType != null)
                                        {
                                            newPO.ExpenditureType = addos.GetObjectByKey<ExpenditureType>(pobaobj.ExpenditureType.Oid);
                                        }
                                        if (pobaobj.ItemGroup != null)
                                        {
                                            newPO.ItemGroup = addos.GetObjectByKey<vwItemGroup>(pobaobj.ItemGroup.Code);
                                        }
                                        if (pobaobj.CompanyAddress != null)
                                        {
                                            newPO.CompanyAddress = addos.GetObjectByKey<CompanyAddress>(pobaobj.CompanyAddress.Oid);
                                        }
                                        newPO.DocStatus = DocStatus.Submit;

                                        PurchaseOrderDetails newPOItem = addos.CreateObject<PurchaseOrderDetails>();

                                        foreach (PurchaseBlanketAgreementDetails dtl in pobaobj.PurchaseBlanketAgreementDetails)
                                        {
                                            newPOItem.Item = addos.FindObject<vwItemMasters>(CriteriaOperator.Parse("ItemCode = ?", dtl.Item.ItemCode));
                                            newPOItem.ItemDesc = dtl.ItemDesc;
                                            newPOItem.ItemDetails = dtl.ItemDetails;
                                            newPOItem.Unitprice = dtl.Unitprice;
                                            if (dtl.ExpenditureType != null)
                                            {
                                                newPOItem.ExpenditureType = addos.GetObjectByKey<ExpenditureType>(dtl.ExpenditureType.Oid);
                                            }
                                            if (dtl.ItemGroup != null)
                                            {
                                                newPOItem.ItemGroup = addos.GetObjectByKey<vwItemGroup>(dtl.ItemGroup.Code);
                                            }
                                            if (dtl.CostCenter != null)
                                            {
                                                newPOItem.CostCenter = addos.GetObjectByKey<vwCostCenter>(dtl.CostCenter.PrcCode);
                                            }
                                            // Start ver 1.0.2
                                            if (dtl.SubCostCenter != null)
                                            {
                                                newPOItem.SubCostCenter = addos.GetObjectByKey<vwSubCostCenter>(dtl.SubCostCenter.PrcCode);
                                            }
                                            // End ver 1.0.2
                                            newPOItem.BaseDoc = pobaobj.DocNum;
                                            newPOItem.BaseOid = dtl.Oid.ToString();
                                            newPOItem.OpenQuantity = dtl.Quantity;
                                            newPOItem.Quantity = dtl.Quantity;
                                            if (dtl.Tax != null)
                                            {
                                                newPOItem.Tax = addos.GetObjectByKey<vwTax>(dtl.Tax.BoCode);
                                            }
                                            newPOItem.Discount = dtl.Discount;
                                            newPOItem.Vehicle = dtl.Vehicle;

                                            newPO.PurchaseOrderDetails.Add(newPOItem);
                                        }

                                        addos.CommitChanges();
                                        #endregion

                                        pobaos.CommitChanges();
                                    }
                                }

                                temp = ConfigurationManager.AppSettings["BATrigger"].ToString().ToUpper();
                                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                                {
                                    if (pobaobj.ManualDate.Date == DateTime.Now.Date)
                                    {
                                        #region Add PO
                                        pobaobj.PreviousDate = DateTime.Now;
                                        pobaobj.ManualDate = nulldate;
                                        IObjectSpace addos = ObjectSpaceProvider.CreateObjectSpace();
                                        PurchaseOrders newPO = addos.CreateObject<PurchaseOrders>();

                                        IObjectSpace docos = ObjectSpaceProvider.CreateObjectSpace();
                                        DocTypes number = docos.FindObject<DocTypes>(new BinaryOperator("BoCode", DocTypeList.PurchaseOrders));

                                        number.CurrectDocNum = number.NextDocNum;
                                        number.NextDocNum = number.NextDocNum + 1;

                                        newPO.DocNum = "PO" + number.CurrectDocNum;

                                        docos.CommitChanges();

                                        if (pobaobj.VendorCode != null)
                                        {
                                            newPO.VendorCode = addos.GetObjectByKey<vwVendors>(pobaobj.VendorCode.PriKey);
                                        }
                                        if (pobaobj.Warehouse != null)
                                        {
                                            newPO.Warehouse = addos.GetObjectByKey<vwWarehouse>(pobaobj.Warehouse.BoCode);
                                        }
                                        if (pobaobj.Department != null)
                                        {
                                            newPO.Department = addos.GetObjectByKey<Department>(pobaobj.Department.Oid);
                                        }
                                        if (pobaobj.ExpenditureType != null)
                                        {
                                            newPO.ExpenditureType = addos.GetObjectByKey<ExpenditureType>(pobaobj.ExpenditureType.Oid);
                                        }
                                        if (pobaobj.ItemGroup != null)
                                        {
                                            newPO.ItemGroup = addos.GetObjectByKey<vwItemGroup>(pobaobj.ItemGroup.Code);
                                        }
                                        if (pobaobj.CompanyAddress != null)
                                        {
                                            newPO.CompanyAddress = addos.GetObjectByKey<CompanyAddress>(pobaobj.CompanyAddress.Oid);
                                        }
                                        newPO.DocStatus = DocStatus.Submit;

                                        foreach (PurchaseBlanketAgreementDetails dtl in pobaobj.PurchaseBlanketAgreementDetails)
                                        {
                                            PurchaseOrderDetails newPOItem = addos.CreateObject<PurchaseOrderDetails>();
                                            newPOItem.Item = addos.FindObject<vwItemMasters>(CriteriaOperator.Parse("ItemCode = ?", dtl.Item.ItemCode));
                                            newPOItem.ItemDesc = dtl.ItemDesc;
                                            newPOItem.ItemDetails = dtl.ItemDetails;
                                            newPOItem.Unitprice = dtl.Unitprice;
                                            if (dtl.ExpenditureType != null)
                                            {
                                                newPOItem.ExpenditureType = addos.GetObjectByKey<ExpenditureType>(dtl.ExpenditureType.Oid);
                                            }
                                            if (dtl.ItemGroup != null)
                                            {
                                                newPOItem.ItemGroup = addos.GetObjectByKey<vwItemGroup>(dtl.ItemGroup.Code);
                                            }
                                            if (dtl.CostCenter != null)
                                            {
                                                newPOItem.CostCenter = addos.GetObjectByKey<vwCostCenter>(dtl.CostCenter.PrcCode);
                                            }
                                            // Start ver 1.0.2
                                            if (dtl.SubCostCenter != null)
                                            {
                                                newPOItem.SubCostCenter = addos.GetObjectByKey<vwSubCostCenter>(dtl.SubCostCenter.PrcCode);
                                            }
                                            // End ver 1.0.2
                                            newPOItem.BaseDoc = pobaobj.DocNum;
                                            newPOItem.BaseOid = dtl.Oid.ToString();
                                            newPOItem.OpenQuantity = dtl.Quantity;
                                            newPOItem.Quantity = dtl.Quantity;
                                            if (dtl.Tax != null)
                                            {
                                                newPOItem.Tax = addos.GetObjectByKey<vwTax>(dtl.Tax.BoCode);
                                            }
                                            newPOItem.Discount = dtl.Discount;
                                            newPOItem.Vehicle = dtl.Vehicle;

                                            newPO.PurchaseOrderDetails.Add(newPOItem);
                                        }

                                        addos.CommitChanges();
                                        #endregion
                                    }
                                }

                                pobaos.CommitChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog("[Error]", "Message: POST PO PA Failed - OID : " + dtlpoba.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--PO BA Posting End--");
                }
            }
            catch (Exception ex)
            {
                WriteLog("[Error]", "Message:" + ex.Message);
            }

        // End Post ======================================================================================
        EndApplication:
            return;
        }

        private void WriteLog(string lvl, string str)
        {
            FileStream fileStream = null;

            string filePath = "C:\\WingHin_Posting_Log\\";
            filePath = filePath + "[" + "Posting Status" + "] Log_" + System.DateTime.Today.ToString("yyyyMMdd") + "." + "txt";

            FileInfo fileInfo = new FileInfo(filePath);
            DirectoryInfo dirInfo = new DirectoryInfo(fileInfo.DirectoryName);
            if (!dirInfo.Exists) dirInfo.Create();

            if (!fileInfo.Exists)
            {
                fileStream = fileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(filePath, FileMode.Append);
            }

            StreamWriter log = new StreamWriter(fileStream);
            string status = lvl.ToString().Replace("[Log]", "");

            //For Portal_UpdateStatus_Log
            log.WriteLine("{0}{1}", status, str.ToString());

            log.Close();
        }

        public int PostPOtoSAP(PurchaseOrders oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    bool found = false;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    if (oTargetDoc.PurchaseOrderAttachment != null && oTargetDoc.PurchaseOrderAttachment.Count > 0)
                    {
                        foreach (PurchaseOrderAttachment obj in oTargetDoc.PurchaseOrderAttachment)
                        {
                            string fullpath = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString() + g.ToString() + obj.File.FileName;
                            using (System.IO.FileStream fs = System.IO.File.OpenWrite(fullpath))
                            {
                                obj.File.SaveToStream(fs);
                            }
                        }
                    }

                    int sapempid = 0;
                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseOrders);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.CardCode = oTargetDoc.VendorCode.CardCode;
                    oDoc.CardName = oTargetDoc.VendorCode.CardName;
                    oDoc.DocDate = oTargetDoc.PostingDate;
                    oDoc.UserFields.Fields.Item("U_PortalNum").Value = oTargetDoc.DocNum;
                    oDoc.Comments = oTargetDoc.Remarks;
                    if (oTargetDoc.CompanyAddress != null)
                    {
                        if (oTargetDoc.CompanyAddress.Address != null)
                        {
                            oDoc.Address2 = oTargetDoc.CompanyAddress.Address;
                        }
                    }

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;

                    int cnt = 0;
                    foreach (PurchaseOrderDetails dtl in oTargetDoc.PurchaseOrderDetails)
                    {
                        //if (dtl.LineAmount > 0)
                        //{
                        cnt++;
                        if (cnt == 1)
                        {
                            string journal = oTargetDoc.VendorCode.CardCode + "-" + dtl.Item.ItemName;
                            int countchar = 0;
                            foreach (char c in journal)
                            {
                                countchar++;
                            }
                            if (countchar >= 50)
                            {
                                oDoc.JournalMemo = journal.Substring(1, 49).ToString();
                            }
                            else
                            {
                                oDoc.JournalMemo = journal;
                            }
                        }
                        else
                        {
                            oDoc.Lines.Add();
                            oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                        }

                        if (dtl.Tax != null)
                        {
                            oDoc.Lines.VatGroup = dtl.Tax.BoCode;
                        }
                        oDoc.Lines.TaxTotal = (double)dtl.TaxAmount;
                        oDoc.Lines.WarehouseCode = oTargetDoc.Warehouse.BoCode;
                        if (dtl.CostCenter != null)
                        {
                            oDoc.Lines.CostingCode = dtl.CostCenter.PrcCode;
                        }
                        // Start ver 1.0.2
                        if (dtl.SubCostCenter != null)
                        {
                            oDoc.Lines.CostingCode2 = dtl.SubCostCenter.PrcCode;
                        }
                        // End ver 1.0.2

                        oDoc.Lines.ItemCode = dtl.Item.ItemCode;
                        oDoc.Lines.ItemDescription = dtl.ItemDesc;
                        if (dtl.ItemDetails != null)
                        {
                            oDoc.Lines.ItemDetails = dtl.ItemDetails.ToString().ToUpper();
                        }
                        //if (oTargetDoc.VehicleNo != null)
                        //{
                        //    oDoc.Lines.UserFields.Fields.Item("U_Ref2").Value = oTargetDoc.VehicleNo;
                        //}
                        if (dtl.Vehicle != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_VehicleNum").Value = dtl.Vehicle;
                        }
                        oDoc.Lines.Quantity = (double)dtl.Quantity;
                        oDoc.Lines.UnitPrice = (double)dtl.Unitprice;
                        oDoc.Lines.DiscountPercent = (double)dtl.Discount;
                        //oDoc.Lines.LineTotal = (double)dtl.SubTotal;
                    }
                    if (oTargetDoc.PurchaseOrderAttachment != null && oTargetDoc.PurchaseOrderAttachment.Count > 0)
                    {
                        cnt = 0;
                        SAPbobsCOM.Attachments2 oAtt = (SAPbobsCOM.Attachments2)sap.oCom.GetBusinessObject(BoObjectTypes.oAttachments2);
                        foreach (PurchaseOrderAttachment dtl in oTargetDoc.PurchaseOrderAttachment)
                        {

                            cnt++;
                            if (cnt == 1)
                            {
                                if (oAtt.Lines.Count == 0)
                                    oAtt.Lines.Add();
                            }
                            else
                                oAtt.Lines.Add();

                            string attfile = "";
                            string[] fexe = dtl.File.FileName.Split('.');
                            if (fexe.Length <= 2)
                                attfile = fexe[0];
                            else
                            {
                                for (int x = 0; x < fexe.Length - 1; x++)
                                {
                                    if (attfile == "")
                                        attfile = fexe[x];
                                    else
                                        attfile += "." + fexe[x];
                                }
                            }
                            oAtt.Lines.FileName = g.ToString() + attfile;
                            if (fexe.Length > 1)
                                oAtt.Lines.FileExtension = fexe[fexe.Length - 1];
                            string path = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString();
                            path = path.Replace("\\\\", "\\");
                            path = path.Substring(0, path.Length - 1);
                            oAtt.Lines.SourcePath = path;
                            oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                        }
                        int iAttEntry = -1;
                        if (oAtt.Add() == 0)
                        {
                            iAttEntry = int.Parse(sap.oCom.GetNewObjectKey());
                        }
                        else
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            PurchaseOrders obj = osupdate.GetObjectByKey<PurchaseOrders>(oTargetDoc.Oid);

                            PurchaseOrderDocStatus ds = osupdate.CreateObject<PurchaseOrderDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = oTargetDoc.DocStatus;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.PurchaseOrderDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: PO Attachement Error :" + oTargetDoc + "-" + temp);
                            return -1;
                        }
                        oDoc.AttachmentEntry = iAttEntry;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        PurchaseOrders obj = osupdate.GetObjectByKey<PurchaseOrders>(oTargetDoc.Oid);

                        PurchaseOrderDocStatus ds = osupdate.CreateObject<PurchaseOrderDocStatus>();
                        ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = oTargetDoc.DocStatus;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.PurchaseOrderDocStatus.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: PO Posting :" + oTargetDoc + "-" + temp);
                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                PurchaseOrders obj = osupdate.GetObjectByKey<PurchaseOrders>(oTargetDoc.Oid);

                PurchaseOrderDocStatus ds = osupdate.CreateObject<PurchaseOrderDocStatus>();
                ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = oTargetDoc.DocStatus;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.PurchaseOrderDocStatus.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: PO Posting :" + oTargetDoc + "-" + ex.Message);
                return -1;
            }
        }

        public int PostGRNtoSAP(GoodsReceipt oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (oTargetDoc.Sap)
                    return 0;

                if (oTargetDoc.DocStatus == DocStatus.Submit)
                {
                    bool found = false;
                    DateTime postdate = DateTime.Now;
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    if (oTargetDoc.GoodsReceiptAttachment != null && oTargetDoc.GoodsReceiptAttachment.Count > 0)
                    {
                        foreach (GoodsReceiptAttachment obj in oTargetDoc.GoodsReceiptAttachment)
                        {
                            string fullpath = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString() + g.ToString() + obj.File.FileName;
                            using (System.IO.FileStream fs = System.IO.File.OpenWrite(fullpath))
                            {
                                obj.File.SaveToStream(fs);
                            }
                        }
                    }

                    int sapempid = 0;

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.CardCode = oTargetDoc.VendorCode.CardCode;
                    oDoc.CardName = oTargetDoc.VendorCode.CardName;
                    oDoc.DocDate = postdate;
                    oDoc.UserFields.Fields.Item("U_PortalNum").Value = oTargetDoc.DocNum;
                    oDoc.Comments = oTargetDoc.Remarks;
                    if (oTargetDoc.CompanyAddress != null)
                    {
                        if (oTargetDoc.CompanyAddress.Address != null)
                        {
                            oDoc.Address2 = oTargetDoc.CompanyAddress.Address;
                        }
                    }

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;
                    oDoc.DocDate = postdate;

                    int cnt = 0;
                    foreach (GoodsReceiptDetails dtl in oTargetDoc.GoodsReceiptDetails)
                    {
                        if (dtl.Quantity > 0)
                        {
                            cnt++;
                            if (cnt == 1)
                            {
                                string journal = oTargetDoc.VendorCode.CardCode + "-" + dtl.Item.ItemName;
                                int countchar = 0;
                                foreach (char c in journal)
                                {
                                    countchar++;
                                }
                                if (countchar >= 50)
                                {
                                    oDoc.JournalMemo = journal.Substring(1, 49).ToString();
                                }
                                else
                                {
                                    oDoc.JournalMemo = journal;
                                }
                            }
                            else
                            {
                                //oDoc.Lines.BatchNumbers.Add();
                                //oDoc.Lines.BatchNumbers.SetCurrentLine(oDoc.Lines.Count - 1);
                                oDoc.Lines.Add();
                                oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                            }

                            if (dtl.Tax != null)
                            {
                                oDoc.Lines.TaxCode = dtl.Tax.BoCode;
                            }
                            oDoc.Lines.TaxTotal = (double)dtl.TaxAmount;
                            oDoc.Lines.WarehouseCode = oTargetDoc.Warehouse.BoCode;
                            // Start ver 1.0.2
                            if (dtl.CostCenter != null)
                            {
                                oDoc.Lines.CostingCode = dtl.CostCenter.PrcCode;
                            }
                            if (dtl.SubCostCenter != null)
                            {
                                oDoc.Lines.CostingCode2 = dtl.SubCostCenter.PrcCode;
                            }
                            // End ver 1.0.2

                            oDoc.Lines.ItemCode = dtl.Item.ItemCode;
                            oDoc.Lines.ItemDescription = dtl.ItemDesc;
                            if (dtl.ItemDetails != null)
                            {
                                oDoc.Lines.ItemDetails = dtl.ItemDetails.ToString().ToUpper();
                            }
                            if (dtl.Vehicle != null)
                            {
                                oDoc.Lines.UserFields.Fields.Item("U_VehicleNum").Value = dtl.Vehicle;
                            }
                            oDoc.Lines.Quantity = (double)dtl.Quantity;// * (double)link.Packsize;
                            oDoc.Lines.UnitPrice = (double)dtl.POPrice;// / oDoc.Lines.Quantity;

                        }

                        if (dtl.BaseEntry != 0)
                        {
                            oDoc.Lines.BaseType = 22;
                            oDoc.Lines.BaseEntry = dtl.BaseEntry;//PR Docentry
                            oDoc.Lines.BaseLine = int.Parse(dtl.BaseOid);//line no
                        }
                    }

                    if (oTargetDoc.GoodsReceiptAttachment != null && oTargetDoc.GoodsReceiptAttachment.Count > 0)
                    {
                        cnt = 0;
                        SAPbobsCOM.Attachments2 oAtt = (SAPbobsCOM.Attachments2)sap.oCom.GetBusinessObject(BoObjectTypes.oAttachments2);
                        foreach (GoodsReceiptAttachment dtl in oTargetDoc.GoodsReceiptAttachment)
                        {

                            cnt++;
                            if (cnt == 1)
                            {
                                if (oAtt.Lines.Count == 0)
                                    oAtt.Lines.Add();
                            }
                            else
                                oAtt.Lines.Add();

                            string attfile = "";
                            string[] fexe = dtl.File.FileName.Split('.');
                            if (fexe.Length <= 2)
                                attfile = fexe[0];
                            else
                            {
                                for (int x = 0; x < fexe.Length - 1; x++)
                                {
                                    if (attfile == "")
                                        attfile = fexe[x];
                                    else
                                        attfile += "." + fexe[x];
                                }
                            }
                            oAtt.Lines.FileName = g.ToString() + attfile;
                            if (fexe.Length > 1)
                                oAtt.Lines.FileExtension = fexe[fexe.Length - 1];
                            string path = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString();
                            path = path.Replace("\\\\", "\\");
                            path = path.Substring(0, path.Length - 1);
                            oAtt.Lines.SourcePath = path;
                            oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                        }
                        int iAttEntry = -1;
                        if (oAtt.Add() == 0)
                        {
                            iAttEntry = int.Parse(sap.oCom.GetNewObjectKey());
                        }
                        else
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            GoodsReceipt obj = osupdate.GetObjectByKey<GoodsReceipt>(oTargetDoc.Oid);

                            GoodsReceiptDocStatus ds = osupdate.CreateObject<GoodsReceiptDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = oTargetDoc.DocStatus;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.GoodsReceiptDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: GRN Attachement Error :" + oTargetDoc + "-" + temp);

                            return -1;
                        }
                        oDoc.AttachmentEntry = iAttEntry;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        GoodsReceipt obj = osupdate.GetObjectByKey<GoodsReceipt>(oTargetDoc.Oid);

                        GoodsReceiptDocStatus ds = osupdate.CreateObject<GoodsReceiptDocStatus>();
                        ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = oTargetDoc.DocStatus;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.GoodsReceiptDocStatus.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: GRN Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                GoodsReceipt obj = osupdate.GetObjectByKey<GoodsReceipt>(oTargetDoc.Oid);

                GoodsReceiptDocStatus ds = osupdate.CreateObject<GoodsReceiptDocStatus>();
                ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = oTargetDoc.DocStatus;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.GoodsReceiptDocStatus.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: GRN Posting :" + oTargetDoc + "-" + ex.Message);
                return -1;
            }
        }

        public int PostGItoSAP(GoodsIssue oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (oTargetDoc.Sap)
                    return 0;

                if (oTargetDoc.DocStatus == DocStatus.Submit)
                {
                    bool found = false;
                    DateTime postdate = DateTime.Now;

                    foreach (GoodsIssueDetails dtl in oTargetDoc.GoodsIssueDetails)
                    {
                        found = true;
                    }
                    if (!found) return 0;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    if (oTargetDoc.GoodsIssueAttachment != null && oTargetDoc.GoodsIssueAttachment.Count > 0)
                    {
                        foreach (GoodsIssueAttachment obj in oTargetDoc.GoodsIssueAttachment)
                        {
                            string fullpath = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString() + g.ToString() + obj.File.FileName;
                            using (System.IO.FileStream fs = System.IO.File.OpenWrite(fullpath))
                            {
                                obj.File.SaveToStream(fs);
                            }
                        }
                    }

                    int sapempid = 0;

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.DocDate = postdate;

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;
                    oDoc.DocDate = oTargetDoc.PostingDate;
                    oDoc.TaxDate = oTargetDoc.DocDate;
                    oDoc.UserFields.Fields.Item("U_PortalNum").Value = oTargetDoc.DocNum;
                    oDoc.Comments = oTargetDoc.Remarks;

                    int cnt = 0;
                    foreach (GoodsIssueDetails dtl in oTargetDoc.GoodsIssueDetails)
                    {
                        if (dtl.Quantity > 0)
                        {
                            cnt++;
                            if (cnt == 1)
                            {
                                string journal = oTargetDoc.VendorCode.CardCode + "-" + dtl.Item.ItemName;
                                int countchar = 0;
                                foreach (char c in journal)
                                {
                                    countchar++;
                                }
                                if (countchar >= 50)
                                {
                                    oDoc.JournalMemo = journal.Substring(1, 49).ToString();
                                }
                                else
                                {
                                    oDoc.JournalMemo = journal;
                                }
                            }
                            else
                            {
                                //oDoc.Lines.BatchNumbers.Add();
                                //oDoc.Lines.BatchNumbers.SetCurrentLine(oDoc.Lines.Count - 1);
                                oDoc.Lines.Add();
                                oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                            }

                            oDoc.Lines.WarehouseCode = oTargetDoc.Warehouse.BoCode;

                            oDoc.Lines.ItemCode = dtl.Item.ItemCode;
                            oDoc.Lines.ItemDescription = dtl.ItemDesc;
                            if (dtl.ItemDetails != null)
                            {
                                oDoc.Lines.ItemDetails = dtl.ItemDetails.ToString().ToUpper();
                            }
                            // Start ver 1.0.2
                            if (dtl.CostCenter != null)
                            {
                                oDoc.Lines.CostingCode = dtl.CostCenter.PrcCode;
                            }
                            if (dtl.SubCostCenter != null)
                            {
                                oDoc.Lines.CostingCode2 = dtl.SubCostCenter.PrcCode;
                            }
                            // End ver 1.0.2
                            oDoc.Lines.Quantity = (double)dtl.Quantity;
                        }
                    }
                    if (oTargetDoc.GoodsIssueAttachment != null && oTargetDoc.GoodsIssueAttachment.Count > 0)
                    {
                        cnt = 0;
                        SAPbobsCOM.Attachments2 oAtt = (SAPbobsCOM.Attachments2)sap.oCom.GetBusinessObject(BoObjectTypes.oAttachments2);
                        foreach (GoodsIssueAttachment dtl in oTargetDoc.GoodsIssueAttachment)
                        {

                            cnt++;
                            if (cnt == 1)
                            {
                                if (oAtt.Lines.Count == 0)
                                    oAtt.Lines.Add();
                            }
                            else
                                oAtt.Lines.Add();

                            string attfile = "";
                            string[] fexe = dtl.File.FileName.Split('.');
                            if (fexe.Length <= 2)
                                attfile = fexe[0];
                            else
                            {
                                for (int x = 0; x < fexe.Length - 1; x++)
                                {
                                    if (attfile == "")
                                        attfile = fexe[x];
                                    else
                                        attfile += "." + fexe[x];
                                }
                            }
                            oAtt.Lines.FileName = g.ToString() + attfile;
                            if (fexe.Length > 1)
                                oAtt.Lines.FileExtension = fexe[fexe.Length - 1];
                            string path = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString();
                            path = path.Replace("\\\\", "\\");
                            path = path.Substring(0, path.Length - 1);
                            oAtt.Lines.SourcePath = path;
                            oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                        }
                        int iAttEntry = -1;
                        if (oAtt.Add() == 0)
                        {
                            iAttEntry = int.Parse(sap.oCom.GetNewObjectKey());
                        }
                        else
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            GoodsIssue obj = osupdate.GetObjectByKey<GoodsIssue>(oTargetDoc.Oid);

                            GoodsIssueDocStatus ds = osupdate.CreateObject<GoodsIssueDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = oTargetDoc.DocStatus;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.GoodsIssueDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: GI Attachement Error :" + oTargetDoc + "-" + temp);

                            return -1;
                        }
                        oDoc.AttachmentEntry = iAttEntry;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        GoodsIssue obj = osupdate.GetObjectByKey<GoodsIssue>(oTargetDoc.Oid);

                        GoodsIssueDocStatus ds = osupdate.CreateObject<GoodsIssueDocStatus>();
                        ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = oTargetDoc.DocStatus;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.GoodsIssueDocStatus.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: GI Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                GoodsIssue obj = osupdate.GetObjectByKey<GoodsIssue>(oTargetDoc.Oid);

                GoodsIssueDocStatus ds = osupdate.CreateObject<GoodsIssueDocStatus>();
                ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = oTargetDoc.DocStatus;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.GoodsIssueDocStatus.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: GI Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostPOBAtoSAP(PurchaseBlanketAgreement oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    bool found = false;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    if (oTargetDoc.PurchaseBlanketAgreementAttachment != null && oTargetDoc.PurchaseBlanketAgreementAttachment.Count > 0)
                    {
                        foreach (PurchaseBlanketAgreementAttachment obj in oTargetDoc.PurchaseBlanketAgreementAttachment)
                        {
                            string fullpath = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString() + g.ToString() + obj.File.FileName;
                            using (System.IO.FileStream fs = System.IO.File.OpenWrite(fullpath))
                            {
                                obj.File.SaveToStream(fs);
                            }
                        }
                    }

                    int sapempid = 0;
                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseOrders);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.CardCode = oTargetDoc.VendorCode.CardCode;
                    oDoc.CardName = oTargetDoc.VendorCode.CardName;

                    if (oTargetDoc.Billing == Billing.Monthly)
                    {
                        if (DateTime.Now.Date == oTargetDoc.PreviousDate.AddMonths(1).Date)
                        {
                            oDoc.DocDate = oTargetDoc.PreviousDate.AddMonths(1);
                        }
                    }
                    else if (oTargetDoc.Billing == Billing.BiMonthly)
                    {
                        if (DateTime.Now.Date == oTargetDoc.PreviousDate.AddMonths(2).Date)
                        {
                            oDoc.DocDate = oTargetDoc.PreviousDate.AddMonths(2);
                        }
                    }
                    else if (oTargetDoc.Billing == Billing.HalfYearly)
                    {
                        if (DateTime.Now.Date == oTargetDoc.PreviousDate.AddMonths(6).Date)
                        {
                            oDoc.DocDate = oTargetDoc.PreviousDate.AddMonths(6);
                        }
                    }
                    else if (oTargetDoc.Billing == Billing.Annually)
                    {
                        if (DateTime.Now.Date == oTargetDoc.PreviousDate.AddYears(1).Date)
                        {
                            oDoc.DocDate = oTargetDoc.PreviousDate.AddYears(1);
                        }
                    }

                    oDoc.Comments = oTargetDoc.Remarks;

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;

                    int cnt = 0;
                    foreach (PurchaseBlanketAgreementDetails dtl in oTargetDoc.PurchaseBlanketAgreementDetails)
                    {
                        //if (dtl.LineAmount > 0)
                        //{
                        cnt++;
                        if (cnt == 1)
                        {
                        }
                        else
                        {
                            oDoc.Lines.Add();
                            oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                        }

                        if (dtl.Tax != null)
                        {
                            oDoc.Lines.VatGroup = dtl.Tax.BoCode;
                        }
                        oDoc.Lines.TaxTotal = (double)dtl.TaxAmount;
                        oDoc.Lines.WarehouseCode = oTargetDoc.Warehouse.BoCode;
                        // Start ver 1.0.2
                        if (dtl.CostCenter != null)
                        {
                            oDoc.Lines.CostingCode = dtl.CostCenter.PrcCode;
                        }
                        if (dtl.SubCostCenter != null)
                        {
                            oDoc.Lines.CostingCode2 = dtl.SubCostCenter.PrcCode;
                        }
                        // End ver 1.0.2

                        oDoc.Lines.ItemCode = dtl.Item.ItemCode;
                        oDoc.Lines.ItemDescription = dtl.ItemDesc;
                        oDoc.Lines.Quantity = (double)dtl.Quantity;
                        oDoc.Lines.UnitPrice = (double)dtl.Unitprice;
                    }
                    if (oTargetDoc.PurchaseBlanketAgreementAttachment != null && oTargetDoc.PurchaseBlanketAgreementAttachment.Count > 0)
                    {
                        cnt = 0;
                        SAPbobsCOM.Attachments2 oAtt = (SAPbobsCOM.Attachments2)sap.oCom.GetBusinessObject(BoObjectTypes.oAttachments2);
                        foreach (PurchaseBlanketAgreementAttachment dtl in oTargetDoc.PurchaseBlanketAgreementAttachment)
                        {

                            cnt++;
                            if (cnt == 1)
                            {
                                if (oAtt.Lines.Count == 0)
                                    oAtt.Lines.Add();
                            }
                            else
                                oAtt.Lines.Add();

                            string attfile = "";
                            string[] fexe = dtl.File.FileName.Split('.');
                            if (fexe.Length <= 2)
                                attfile = fexe[0];
                            else
                            {
                                for (int x = 0; x < fexe.Length - 1; x++)
                                {
                                    if (attfile == "")
                                        attfile = fexe[x];
                                    else
                                        attfile += "." + fexe[x];
                                }
                            }
                            oAtt.Lines.FileName = g.ToString() + attfile;
                            if (fexe.Length > 1)
                                oAtt.Lines.FileExtension = fexe[fexe.Length - 1];
                            string path = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString();
                            path = path.Replace("\\\\", "\\");
                            path = path.Substring(0, path.Length - 1);
                            oAtt.Lines.SourcePath = path;
                            oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                        }
                        int iAttEntry = -1;
                        if (oAtt.Add() == 0)
                        {
                            iAttEntry = int.Parse(sap.oCom.GetNewObjectKey());
                        }
                        else
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            PurchaseBlanketAgreement obj = osupdate.GetObjectByKey<PurchaseBlanketAgreement>(oTargetDoc.Oid);

                            PurchaseBlanketAgreementDocStatus ds = osupdate.CreateObject<PurchaseBlanketAgreementDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = oTargetDoc.DocStatus;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.PurchaseBlanketAgreementDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: PO BA Attachement Error :" + oTargetDoc + "-" + temp);
                            return -1;
                        }
                        oDoc.AttachmentEntry = iAttEntry;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        PurchaseBlanketAgreement obj = osupdate.GetObjectByKey<PurchaseBlanketAgreement>(oTargetDoc.Oid);

                        PurchaseBlanketAgreementDocStatus ds = osupdate.CreateObject<PurchaseBlanketAgreementDocStatus>();
                        ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = oTargetDoc.DocStatus;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.PurchaseBlanketAgreementDocStatus.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: PO BA Posting :" + oTargetDoc + "-" + temp);
                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                PurchaseBlanketAgreement obj = osupdate.GetObjectByKey<PurchaseBlanketAgreement>(oTargetDoc.Oid);

                PurchaseBlanketAgreementDocStatus ds = osupdate.CreateObject<PurchaseBlanketAgreementDocStatus>();
                ds.CreateUser = osupdate.GetObjectByKey<SystemUsers>(Guid.Parse("2B9F40E0-BA80-4856-9848-917F00967CE5"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = oTargetDoc.DocStatus;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.PurchaseBlanketAgreementDocStatus.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: PO BA Posting :" + oTargetDoc + "-" + ex.Message);
                return -1;
            }
        }
    }
}
