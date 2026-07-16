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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmregistro_ventas));
            comboBox1 = new ComboBox();
            label1 = new Label();
            dataregistrodiario = new DataGridView();
            lblUsuario = new Label();
            imageList1 = new ImageList(components);
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)dataregistrodiario).BeginInit();
            SuspendLayout();
            // 
            // comboBox1
            // 
            comboBox1.BackColor = Color.FromArgb(64, 64, 64);
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(873, 74);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(392, 28);
            comboBox1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(782, 79);
            label1.Name = "label1";
            label1.Size = new Size(85, 23);
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
            lblUsuario.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblUsuario.Location = new Point(126, 44);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(93, 25);
            lblUsuario.TabIndex = 15;
            lblUsuario.Text = "USUARIO";
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "Usuario.png");
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ImageIndex = 0;
            label2.ImageList = imageList1;
            label2.Location = new Point(108, 49);
            label2.Name = "label2";
            label2.Size = new Size(12, 20);
            label2.TabIndex = 16;
            label2.Text = ".";
            label2.Click += label2_Click;
            // 
            // frmregistro_ventas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Gray;
            ClientSize = new Size(1773, 575);
            Controls.Add(label2);
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
        private ImageList imageList1;
        private Label label2;
    }
}