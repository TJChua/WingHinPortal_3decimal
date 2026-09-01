namespace WingHinPortal.Module.Controllers
{
    partial class CrReportController
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
            this.GetReport = new DevExpress.ExpressApp.Actions.PopupWindowShowAction(this.components);
            // 
            // GetReport
            // 
            this.GetReport.AcceptButtonCaption = null;
            this.GetReport.CancelButtonCaption = null;
            this.GetReport.Caption = "Get Report";
            this.GetReport.ConfirmationMessage = null;
            this.GetReport.Id = "GetCrRpt";
            this.GetReport.ToolTip = null;
            this.GetReport.CustomizePopupWindowParams += new DevExpress.ExpressApp.Actions.CustomizePopupWindowParamsEventHandler(this.GetReport_CustomizePopupWindowParams);
            this.GetReport.Execute += new DevExpress.ExpressApp.Actions.PopupWindowShowActionExecuteEventHandler(this.GetReport_Execute);
            // 
            // CrReportController
            // 
            this.Actions.Add(this.GetReport);

        }

        #endregion
        private DevExpress.ExpressApp.Actions.PopupWindowShowAction GetReport;
    }
}
