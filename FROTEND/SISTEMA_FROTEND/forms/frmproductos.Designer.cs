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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmcompras));
            datacompras = new DataGridView();
            producto = new DataGridViewTextBoxColumn();
            cantidad = new DataGridViewTextBoxColumn();
            precio = new DataGridViewTextBoxColumn();
            preciounitario = new DataGridViewTextBoxColumn();
            lblUsuario = new Label();
            label1 = new Label();
            texEmpresa = new TextBox();
            butguardar = new Button();
            imageList1 = new ImageList(components);
            textotalcompras = new TextBox();
            label2 = new Label();
            label3 = new Label();
            imageList2 = new ImageList(components);
            label4 = new Label();
            label5 = new Label();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)datacompras).BeginInit();
            SuspendLayout();
            // 
            // datacompras
            // 
            dataGridViewCellStyle1.BackColor = Color.Silver;
            dataGridViewCellStyle1.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle1.ForeColor = Color.Silver;
            dataGridViewCellStyle1.SelectionBackColor = Color.Silver;
            dataGridViewCellStyle1.SelectionForeColor = Color.Silver;
            datacompras.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.Gray;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.ControlDark;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            datacompras.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            datacompras.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datacompras.Columns.AddRange(new DataGridViewColumn[] { producto, cantidad, precio, preciounitario });
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = Color.White;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            datacompras.DefaultCellStyle = dataGridViewCellStyle3;
            datacompras.Location = new Point(240, 221);
            datacompras.Name = "datacompras";
            datacompras.RowHeadersWidth = 51;
            datacompras.Size = new Size(852, 494);
            datacompras.TabIndex = 0;
            datacompras.CellContentClick += dataGridView1_CellContentClick;
            datacompras.CellEndEdit += datacompras_CellEndEdit;
            datacompras.EditingControlShowing += datacompras_EditingControlShowing;
            datacompras.SelectionChanged += dataProductos_SelectionChanged;
            // 
            // producto
            // 
            producto.HeaderText = "PRODUCTO";
            producto.MinimumWidth = 6;
            producto.Name = "producto";
            producto.Width = 350;
            // 
            // cantidad
            // 
            cantidad.HeaderText = "CANTIDAD";
            cantidad.MinimumWidth = 6;
            cantidad.Name = "cantidad";
            cantidad.Width = 125;
            // 
            // precio
            // 
            precio.HeaderText = "TOTAL";
            precio.MinimumWidth = 6;
            precio.Name = "precio";
            precio.Width = 125;
            // 
            // preciounitario
            // 
            preciounitario.HeaderText = "PRECIO UNITARIO";
            preciounitario.MinimumWidth = 6;
            preciounitario.Name = "preciounitario";
            preciounitario.Width = 200;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(173, 44);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(71, 20);
            lblUsuario.TabIndex = 15;
            lblUsuario.Text = "USUARIO";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(240, 168);
            label1.Name = "label1";
            label1.Size = new Size(165, 23);
            label1.TabIndex = 16;
            label1.Text = "NOMBRE EMPRESA";
            // 
            // texEmpresa
            // 
            texEmpresa.BackColor = Color.FromArgb(64, 64, 64);
            texEmpresa.Location = new Point(411, 168);
            texEmpresa.Name = "texEmpresa";
            texEmpresa.Size = new Size(481, 27);
            texEmpresa.TabIndex = 17;
            // 
            // butguardar
            // 
            butguardar.ImageIndex = 1;
            butguardar.ImageList = imageList1;
            butguardar.Location = new Point(1245, 491);
            butguardar.Name = "butguardar";
            butguardar.Size = new Size(75, 65);
            butguardar.TabIndex = 18;
            butguardar.UseVisualStyleBackColor = true;
            butguardar.Click += butguardar_Click;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "Usuario.png");
            imageList1.Images.SetKeyName(1, "Guardar.png");
            imageList1.Images.SetKeyName(2, "Cerrar.png");
            // 
            // textotalcompras
            // 
            textotalcompras.Location = new Point(967, 721);
            textotalcompras.Name = "textotalcompras";
            textotalcompras.Size = new Size(125, 27);
            textotalcompras.TabIndex = 19;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(876, 728);
            label2.Name = "label2";
            label2.Size = new Size(61, 23);
            label2.TabIndex = 20;
            label2.Text = "TOTAL";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ImageIndex = 0;
            label3.ImageList = imageList1;
            label3.Location = new Point(151, 40);
            label3.Name = "label3";
            label3.Padding = new Padding(2);
            label3.Size = new Size(16, 24);
            label3.TabIndex = 21;
            label3.Text = ".";
            // 
            // imageList2
            // 
            imageList2.ColorDepth = ColorDepth.Depth32Bit;
            imageList2.ImageSize = new Size(16, 16);
            imageList2.TransparentColor = Color.Transparent;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(1138, 513);
            label4.Name = "label4";
            label4.Size = new Size(93, 23);
            label4.TabIndex = 22;
            label4.Text = "GUARDAR";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(1138, 630);
            label5.Name = "label5";
            label5.Size = new Size(98, 23);
            label5.TabIndex = 24;
            label5.Text = "CANCELAR";
            // 
            // button1
            // 
            button1.ImageIndex = 2;
            button1.ImageList = imageList1;
            button1.Location = new Point(1245, 608);
            button1.Name = "button1";
            button1.Size = new Size(75, 65);
            button1.TabIndex = 23;
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // frmcompras
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Gray;
            ClientSize = new Size(1554, 760);
            Controls.Add(label5);
            Controls.Add(button1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(textotalcompras);
            Controls.Add(butguardar);
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
        private Button butguardar;
        private TextBox textotalcompras;
        private Label label2;
        private ImageList imageList1;
        private Label label3;
        private ImageList imageList2;
        private Label label4;
        private DataGridViewTextBoxColumn producto;
        private DataGridViewTextBoxColumn cantidad;
        private DataGridViewTextBoxColumn precio;
        private DataGridViewTextBoxColumn preciounitario;
        private Label label5;
        private Button button1;
    }
}