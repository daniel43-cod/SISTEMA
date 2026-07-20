namespace SISTEMA_FROTEND.forms
{
    partial class frmDetalleVenta
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
            datadetalle = new DataGridView();
            lblUsuario = new Label();
            ((System.ComponentModel.ISupportInitialize)datadetalle).BeginInit();
            SuspendLayout();
            // 
            // datadetalle
            // 
            datadetalle.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datadetalle.Location = new Point(34, 76);
            datadetalle.Name = "datadetalle";
            datadetalle.RowHeadersWidth = 51;
            datadetalle.Size = new Size(1183, 276);
            datadetalle.TabIndex = 0;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(1146, 30);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(71, 20);
            lblUsuario.TabIndex = 15;
            lblUsuario.Text = "USUARIO";
            lblUsuario.Click += lblUsuario_Click;
            // 
            // frmDetalleVenta
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1373, 450);
            Controls.Add(lblUsuario);
            Controls.Add(datadetalle);
            Name = "frmDetalleVenta";
            Text = "frmDetalleVenta";
            Load += frmDetalleVenta_Load;
            ((System.ComponentModel.ISupportInitialize)datadetalle).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView datadetalle;
        private Label lblUsuario;
    }
}