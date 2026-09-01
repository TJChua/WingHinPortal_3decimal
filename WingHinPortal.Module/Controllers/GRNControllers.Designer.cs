namespace WingHinPortal.Module.Controllers
{
    partial class GRNControllers
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
            this.SubmitGRN = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CancelGRN = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CopyFromPO = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            // 
            // SubmitGRN
            // 
            this.SubmitGRN.AcceptButtonCaption = null;
            this.SubmitGRN.CancelButtonCaption = null;
            this.SubmitGRN.Caption = "Submit";
            this.SubmitGRN.Category = "ObjectsCreation";
            this.SubmitGRN.ConfirmationMessage = null;
            this.SubmitGRN.Id = "SubmitGRN";
            this.SubmitGRN.ToolTip = null;
            this.SubmitGRN.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.SubmitGRN_CustomizePopupWindowParams);
            this.SubmitGRN.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.SubmitGRN_Execute);
            // 
            // CancelGRN
            // 
            this.CancelGRN.AcceptButtonCaption = null;
            this.CancelGRN.CancelButtonCaption = null;
            this.CancelGRN.Caption = "Cancel";
            this.CancelGRN.Category = "ObjectsCreation";
            this.CancelGRN.ConfirmationMessage = null;
            this.CancelGRN.Id = "CancelGRN";
            this.CancelGRN.ToolTip = null;
            this.CancelGRN.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CancelGRN_CustomizePopupWindowParams);
            this.CancelGRN.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CancelGRN_Execute);
            // 
            // CopyFromPO
            // 
            this.CopyFromPO.AcceptButtonCaption = null;
            this.CopyFromPO.CancelButtonCaption = null;
            this.CopyFromPO.Caption = "Copy From PO";
            this.CopyFromPO.Category = "ObjectsCreation";
            this.CopyFromPO.ConfirmationMessage = null;
            this.CopyFromPO.Id = "CopyFromPO";
            this.CopyFromPO.ToolTip = null;
            this.CopyFromPO.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CopyFromPO_CustomizePopupWindowParams);
            this.CopyFromPO.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CopyFromPO_Execute);
            // 
            // GRNControllers
            // 
            this.Actions.Add(this.SubmitGRN);
            this.Actions.Add(this.CancelGRN);
            this.Actions.Add(this.CopyFromPO);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction SubmitGRN;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CancelGRN;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CopyFromPO;
    }
}
