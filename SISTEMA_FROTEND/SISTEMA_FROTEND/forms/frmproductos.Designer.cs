namespace SISTEMA_FROTEND.forms
{
    partial class frmcompras
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
            datacompras = new DataGridView();
            lblUsuario = new Label();
            label1 = new Label();
            texEmpresa = new TextBox();
            ((System.ComponentModel.ISupportInitialize)datacompras).BeginInit();
            SuspendLayout();
            // 
            // datacompras
            // 
            datacompras.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datacompras.Location = new Point(44, 260);
            datacompras.Name = "datacompras";
            datacompras.RowHeadersWidth = 51;
            datacompras.Size = new Size(1429, 396);
            datacompras.TabIndex = 0;
            datacompras.CellContentClick += dataGridView1_CellContentClick;
            datacompras.SelectionChanged += dataProductos_SelectionChanged;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(1285, 34);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(71, 20);
            lblUsuario.TabIndex = 15;
            lblUsuario.Text = "USUARIO";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(170, 69);
            label1.Name = "label1";
            label1.Size = new Size(138, 20);
            label1.TabIndex = 16;
            label1.Text = "NOMBRE EMPRESA";
            // 
            // texEmpresa
            // 
            texEmpresa.Location = new Point(336, 62);
            texEmpresa.Name = "texEmpresa";
            texEmpresa.Size = new Size(481, 27);
            texEmpresa.TabIndex = 17;
            // 
            // frmcompras
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1554, 716);
            Controls.Add(texEmpresa);
            Controls.Add(label1);
            Controls.Add(lblUsuario);
            Controls.Add(datacompras);
            Name = "frmcompras";
            Text = "COMPRAS";
            Load += frmproductos_Load;
            ((System.ComponentModel.ISupportInitialize)datacompras).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView datacompras;
        private Label lblUsuario;
        private Label label1;
        private TextBox texEmpresa;
    }
}