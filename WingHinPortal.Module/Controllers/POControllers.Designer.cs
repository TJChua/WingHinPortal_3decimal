namespace WingHinPortal.Module.Controllers
{
    partial class POControllers
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SubmitPO = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ApprovePO = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CancelPO = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CopyFromPR = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.PrintPO = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            this.EmailSupplier = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ClosePO = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            // 
            // SubmitPO
            // 
            this.SubmitPO.AcceptButtonCaption = null;
            this.SubmitPO.CancelButtonCaption = null;
            this.SubmitPO.Caption = "Submit";
            this.SubmitPO.Category = "ObjectsCreation";
            this.SubmitPO.ConfirmationMessage = null;
            this.SubmitPO.Id = "SubmitPO";
            this.SubmitPO.ToolTip = null;
            this.SubmitPO.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.SubmitPO_CustomizePopupWindowParams);
            this.SubmitPO.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.SubmitPO_Execute);
            // 
            // ApprovePO
            // 
            this.ApprovePO.AcceptButtonCaption = null;
            this.ApprovePO.CancelButtonCaption = null;
            this.ApprovePO.Caption = "Approve";
            this.ApprovePO.Category = "ObjectsCreation";
            this.ApprovePO.ConfirmationMessage = null;
            this.ApprovePO.Id = "ApprovePO";
            this.ApprovePO.ToolTip = null;
            this.ApprovePO.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ApprovePO_CustomizePopupWindowParams);
            this.ApprovePO.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ApprovePO_Execute);
            // 
            // CancelPO
            // 
            this.CancelPO.AcceptButtonCaption = null;
            this.CancelPO.CancelButtonCaption = null;
            this.CancelPO.Caption = "Cancel";
            this.CancelPO.Category = "ObjectsCreation";
            this.CancelPO.ConfirmationMessage = null;
            this.CancelPO.Id = "CancelPO";
            this.CancelPO.ToolTip = null;
            this.CancelPO.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CancelPO_CustomizePopupWindowParams);
            this.CancelPO.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CancelPO_Execute);
            // 
            // CopyFromPR
            // 
            this.CopyFromPR.AcceptButtonCaption = null;
            this.CopyFromPR.CancelButtonCaption = null;
            this.CopyFromPR.Caption = "Copy From PR";
            this.CopyFromPR.Category = "ObjectsCreation";
            this.CopyFromPR.ConfirmationMessage = null;
            this.CopyFromPR.Id = "CopyFromPR";
            this.CopyFromPR.ToolTip = null;
            this.CopyFromPR.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CopyFromPR_CustomizePopupWindowParams);
            this.CopyFromPR.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CopyFromPR_Execute);
            // 
            // PrintPO
            // 
            this.PrintPO.Caption = "Print";
            this.PrintPO.Category = "ObjectsCreation";
            this.PrintPO.ConfirmationMessage = null;
            this.PrintPO.Id = "PrintPO";
            this.PrintPO.ToolTip = null;
            this.PrintPO.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.PrintPO_Execute);
            // 
            // EmailSupplier
            // 
            this.EmailSupplier.AcceptButtonCaption = null;
            this.EmailSupplier.CancelButtonCaption = null;
            this.EmailSupplier.Caption = "Email To Supplier";
            this.EmailSupplier.Category = "ObjectsCreation";
            this.EmailSupplier.ConfirmationMessage = null;
            this.EmailSupplier.Id = "EmailSupplier";
            this.EmailSupplier.ToolTip = null;
            this.EmailSupplier.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.EmailSupplier_CustomizePopupWindowParams);
            this.EmailSupplier.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.EmailSupplier_Execute);
            // 
            // ClosePO
            // 
            this.ClosePO.AcceptButtonCaption = null;
            this.ClosePO.CancelButtonCaption = null;
            this.ClosePO.Caption = "Close";
            this.ClosePO.Category = "ObjectsCreation";
            this.ClosePO.ConfirmationMessage = null;
            this.ClosePO.Id = "ClosePO";
            this.ClosePO.ToolTip = null;
            this.ClosePO.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ClosePO_CustomizePopupWindowParams);
            this.ClosePO.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ClosePO_Execute);
            // 
            // POControllers
            // 
            this.Actions.Add(this.SubmitPO);
            this.Actions.Add(this.ApprovePO);
            this.Actions.Add(this.CancelPO);
            this.Actions.Add(this.CopyFromPR);
            this.Actions.Add(this.PrintPO);
            this.Actions.Add(this.EmailSupplier);
            this.Actions.Add(this.ClosePO);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction SubmitPO;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ApprovePO;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CancelPO;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CopyFromPR;
        private DevExpress.ExpressApp.Actions.SimpleAction PrintPO;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction EmailSupplier;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ClosePO;
    }
}
