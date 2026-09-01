namespace WingHinPortal.Module.Controllers
{
    partial class PRControllers
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
            this.SubmitPR = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.ApprovePR = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CancelPR = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.PrintPR = new DevExpress.ExpressApp.Actions.SimpleAction(this.components);
            // 
            // SubmitPR
            // 
            this.SubmitPR.AcceptButtonCaption = null;
            this.SubmitPR.CancelButtonCaption = null;
            this.SubmitPR.Caption = "Submit";
            this.SubmitPR.Category = "ObjectsCreation";
            this.SubmitPR.ConfirmationMessage = null;
            this.SubmitPR.Id = "SubmitPR";
            this.SubmitPR.ToolTip = null;
            this.SubmitPR.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.SubmitPR_CustomizePopupWindowParams);
            this.SubmitPR.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.SubmitPR_Execute);
            // 
            // ApprovePR
            // 
            this.ApprovePR.AcceptButtonCaption = null;
            this.ApprovePR.CancelButtonCaption = null;
            this.ApprovePR.Caption = "Approve";
            this.ApprovePR.Category = "ObjectsCreation";
            this.ApprovePR.ConfirmationMessage = null;
            this.ApprovePR.Id = "ApprovePR";
            this.ApprovePR.ToolTip = null;
            this.ApprovePR.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.ApprovePR_CustomizePopupWindowParams);
            this.ApprovePR.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.ApprovePR_Execute);
            // 
            // CancelPR
            // 
            this.CancelPR.AcceptButtonCaption = null;
            this.CancelPR.CancelButtonCaption = null;
            this.CancelPR.Caption = "Cancel";
            this.CancelPR.Category = "ObjectsCreation";
            this.CancelPR.ConfirmationMessage = null;
            this.CancelPR.Id = "CancelPR";
            this.CancelPR.ToolTip = null;
            this.CancelPR.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CancelPR_CustomizePopupWindowParams);
            this.CancelPR.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CancelPR_Execute);
            // 
            // PrintPR
            // 
            this.PrintPR.Caption = "Print";
            this.PrintPR.Category = "ObjectsCreation";
            this.PrintPR.ConfirmationMessage = null;
            this.PrintPR.Id = "PrintPR";
            this.PrintPR.ToolTip = null;
            this.PrintPR.Execute += new DevExpress.ExpressApp.Actions.SimpleActionExecuteEventHandler(this.PrintPR_Execute);
            // 
            // PRControllers
            // 
            this.Actions.Add(this.SubmitPR);
            this.Actions.Add(this.ApprovePR);
            this.Actions.Add(this.CancelPR);
            this.Actions.Add(this.PrintPR);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction SubmitPR;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction ApprovePR;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CancelPR;
        private DevExpress.ExpressApp.Actions.SimpleAction PrintPR;
    }
}
