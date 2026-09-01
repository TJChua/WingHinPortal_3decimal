namespace WingHinPortal.Module.Controllers
{
    partial class GIControllers
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
            this.SubmitGI = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CancelGI = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CopyFromGIPR = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CopyFromGRN = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            // 
            // SubmitGI
            // 
            this.SubmitGI.AcceptButtonCaption = null;
            this.SubmitGI.CancelButtonCaption = null;
            this.SubmitGI.Caption = "Submit";
            this.SubmitGI.Category = "ObjectsCreation";
            this.SubmitGI.ConfirmationMessage = null;
            this.SubmitGI.Id = "SubmitGI";
            this.SubmitGI.ToolTip = null;
            this.SubmitGI.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.SubmitGI_CustomizePopupWindowParams);
            this.SubmitGI.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.SubmitGI_Execute);
            // 
            // CancelGI
            // 
            this.CancelGI.AcceptButtonCaption = null;
            this.CancelGI.CancelButtonCaption = null;
            this.CancelGI.Caption = "Cancel";
            this.CancelGI.Category = "ObjectsCreation";
            this.CancelGI.ConfirmationMessage = null;
            this.CancelGI.Id = "CancelGI";
            this.CancelGI.ToolTip = null;
            this.CancelGI.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CancelGI_CustomizePopupWindowParams);
            this.CancelGI.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CancelGI_Execute);
            // 
            // CopyFromGIPR
            // 
            this.CopyFromGIPR.AcceptButtonCaption = null;
            this.CopyFromGIPR.CancelButtonCaption = null;
            this.CopyFromGIPR.Caption = "Copy From PR";
            this.CopyFromGIPR.Category = "ObjectsCreation";
            this.CopyFromGIPR.ConfirmationMessage = null;
            this.CopyFromGIPR.Id = "CopyFromGIPR";
            this.CopyFromGIPR.ToolTip = null;
            this.CopyFromGIPR.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CopyFromGIPR_CustomizePopupWindowParams);
            this.CopyFromGIPR.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CopyFromGIPR_Execute);
            // 
            // CopyFromGRN
            // 
            this.CopyFromGRN.AcceptButtonCaption = null;
            this.CopyFromGRN.CancelButtonCaption = null;
            this.CopyFromGRN.Caption = "Copy From GRN";
            this.CopyFromGRN.Category = "ObjectsCreation";
            this.CopyFromGRN.ConfirmationMessage = null;
            this.CopyFromGRN.Id = "CopyFromGRN";
            this.CopyFromGRN.ToolTip = null;
            this.CopyFromGRN.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CopyFromGRN_CustomizePopupWindowParams);
            this.CopyFromGRN.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CopyFromGRN_Execute);
            // 
            // GIControllers
            // 
            this.Actions.Add(this.SubmitGI);
            this.Actions.Add(this.CancelGI);
            this.Actions.Add(this.CopyFromGIPR);
            this.Actions.Add(this.CopyFromGRN);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction SubmitGI;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CancelGI;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CopyFromGIPR;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CopyFromGRN;
    }
}
