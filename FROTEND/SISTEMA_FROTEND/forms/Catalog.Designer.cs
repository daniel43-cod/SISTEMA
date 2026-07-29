namespace SISTEMA_FROTEND.forms
{
    partial class Catalog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            flowCatalogo = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // flowCatalogo
            // 
            flowCatalogo.AutoScroll = true;
            flowCatalogo.Location = new Point(12, 152);
            flowCatalogo.Name = "flowCatalogo";
            flowCatalogo.Size = new Size(1368, 625);
            flowCatalogo.TabIndex = 0;
            flowCatalogo.Paint += flowCatalogo_Paint;
            // 
            // Catalog
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1412, 801);
            Controls.Add(flowCatalogo);
            Name = "Catalog";
            Text = "Form1";
            Load += Catalog_Load;
            ResumeLayout(false);
        }





        #endregion

        private FlowLayoutPanel flowCatalogo;
    }
}