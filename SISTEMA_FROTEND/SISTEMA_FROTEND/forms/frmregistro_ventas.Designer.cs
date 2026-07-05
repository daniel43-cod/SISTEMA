namespace SISTEMA_FROTEND.presentacion
{
    partial class frmregistro_ventas
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
            comboBox1 = new ComboBox();
            label1 = new Label();
            dataregistrodiario = new DataGridView();
            lblUsuario = new Label();
            ((System.ComponentModel.ISupportInitialize)dataregistrodiario).BeginInit();
            SuspendLayout();
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(192, 79);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(151, 28);
            comboBox1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(84, 87);
            label1.Name = "label1";
            label1.Size = new Size(71, 20);
            label1.TabIndex = 1;
            label1.Text = "USUARIO";
            // 
            // dataregistrodiario
            // 
            dataregistrodiario.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataregistrodiario.Location = new Point(12, 149);
            dataregistrodiario.Name = "dataregistrodiario";
            dataregistrodiario.RowHeadersWidth = 51;
            dataregistrodiario.Size = new Size(1602, 280);
            dataregistrodiario.TabIndex = 2;
            dataregistrodiario.CellContentClick += dataregistrodiario_CellContentClick;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(855, 39);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(71, 20);
            lblUsuario.TabIndex = 15;
            lblUsuario.Text = "USUARIO";
            // 
            // frmregistro_ventas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1773, 575);
            Controls.Add(lblUsuario);
            Controls.Add(dataregistrodiario);
            Controls.Add(label1);
            Controls.Add(comboBox1);
            Name = "frmregistro_ventas";
            Text = "frmregistro_ventas";
            Load += frmregistro_ventas_Load;
            ((System.ComponentModel.ISupportInitialize)dataregistrodiario).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox comboBox1;
        private Label label1;
        private DataGridView dataregistrodiario;
        private Label lblUsuario;
    }
}