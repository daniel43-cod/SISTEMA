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
            producto = new DataGridViewTextBoxColumn();
            cantidad = new DataGridViewTextBoxColumn();
            precio = new DataGridViewTextBoxColumn();
            preciounitario = new DataGridViewTextBoxColumn();
            lblUsuario = new Label();
            label1 = new Label();
            texEmpresa = new TextBox();
            butguardar = new Button();
            textotalcompras = new TextBox();
            label2 = new Label();
            ((System.ComponentModel.ISupportInitialize)datacompras).BeginInit();
            SuspendLayout();
            // 
            // datacompras
            // 
            datacompras.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datacompras.Columns.AddRange(new DataGridViewColumn[] { producto, cantidad, precio, preciounitario });
            datacompras.Location = new Point(44, 260);
            datacompras.Name = "datacompras";
            datacompras.RowHeadersWidth = 51;
            datacompras.Size = new Size(1429, 396);
            datacompras.TabIndex = 0;
            datacompras.CellContentClick += dataGridView1_CellContentClick;
            datacompras.CellEndEdit += datacompras_CellEndEdit;
            datacompras.EditingControlShowing += datacompras_EditingControlShowing;
            datacompras.SelectionChanged += dataProductos_SelectionChanged;
            // 
            // producto
            // 
            producto.HeaderText = "producto";
            producto.MinimumWidth = 6;
            producto.Name = "producto";
            producto.Width = 350;
            // 
            // cantidad
            // 
            cantidad.HeaderText = "cantidad";
            cantidad.MinimumWidth = 6;
            cantidad.Name = "cantidad";
            cantidad.Width = 125;
            // 
            // precio
            // 
            precio.HeaderText = "precio";
            precio.MinimumWidth = 6;
            precio.Name = "precio";
            precio.Width = 125;
            // 
            // preciounitario
            // 
            preciounitario.HeaderText = "precio unitario";
            preciounitario.MinimumWidth = 6;
            preciounitario.Name = "preciounitario";
            preciounitario.Width = 200;
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
            // butguardar
            // 
            butguardar.Location = new Point(998, 695);
            butguardar.Name = "butguardar";
            butguardar.Size = new Size(94, 53);
            butguardar.TabIndex = 18;
            butguardar.Text = "GUARDAR";
            butguardar.UseVisualStyleBackColor = true;
            butguardar.Click += butguardar_Click;
            // 
            // textotalcompras
            // 
            textotalcompras.Location = new Point(448, 704);
            textotalcompras.Name = "textotalcompras";
            textotalcompras.Size = new Size(125, 27);
            textotalcompras.TabIndex = 19;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(357, 711);
            label2.Name = "label2";
            label2.Size = new Size(50, 20);
            label2.TabIndex = 20;
            label2.Text = "TOTAL";
            // 
            // frmcompras
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1554, 760);
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
        private DataGridViewTextBoxColumn producto;
        private DataGridViewTextBoxColumn cantidad;
        private DataGridViewTextBoxColumn precio;
        private DataGridViewTextBoxColumn preciounitario;
        private Button butguardar;
        private TextBox textotalcompras;
        private Label label2;
    }
}