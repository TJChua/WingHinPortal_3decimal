namespace WingHinPortal.Module.Controllers
{
    partial class PBAControllers
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
            this.SubmitPBA = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.CancelPBA = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            this.TerminatePBA = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            // 
            // SubmitPBA
            // 
            this.SubmitPBA.AcceptButtonCaption = null;
            this.SubmitPBA.CancelButtonCaption = null;
            this.SubmitPBA.Caption = "Submit";
            this.SubmitPBA.Category = "ObjectsCreation";
            this.SubmitPBA.ConfirmationMessage = null;
            this.SubmitPBA.Id = "SubmitPBA";
            this.SubmitPBA.ToolTip = null;
            this.SubmitPBA.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.SubmitPBA_CustomizePopupWindowParams);
            this.SubmitPBA.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.SubmitPBA_Execute);
            // 
            // CancelPBA
            // 
            this.CancelPBA.AcceptButtonCaption = null;
            this.CancelPBA.CancelButtonCaption = null;
            this.CancelPBA.Caption = "Cancel";
            this.CancelPBA.Category = "ObjectsCreation";
            this.CancelPBA.ConfirmationMessage = null;
            this.CancelPBA.Id = "CancelPBA";
            this.CancelPBA.ToolTip = null;
            this.CancelPBA.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.CancelPBA_CustomizePopupWindowParams);
            this.CancelPBA.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.CancelPBA_Execute);
            // 
            // TerminatePBA
            // 
            this.TerminatePBA.AcceptButtonCaption = null;
            this.TerminatePBA.CancelButtonCaption = null;
            this.TerminatePBA.Caption = "Terminate";
            this.TerminatePBA.Category = "ObjectsCreation";
            this.TerminatePBA.ConfirmationMessage = null;
            this.TerminatePBA.Id = "TerminatePBA";
            this.TerminatePBA.ToolTip = null;
            this.TerminatePBA.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.TerminatePBA_CustomizePopupWindowParams);
            this.TerminatePBA.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.TerminatePBA_Execute);
            // 
            // PBAControllers
            // 
            this.Actions.Add(this.SubmitPBA);
            this.Actions.Add(this.CancelPBA);
            this.Actions.Add(this.TerminatePBA);

        }

        #endregion

        private DevExpress.ExpressApp.Actions.PopupWindowShowAction SubmitPBA;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction CancelPBA;
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction TerminatePBA;
    }
}
