namespace SISTEMA_FROTEND.presentacion
{
    partial class Ventas
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Ventas));
            button1 = new Button();
            imageList1 = new ImageList(components);
            groupBox1 = new GroupBox();
            cliente = new Label();
            texclientes = new TextBox();
            labtelefono = new Label();
            textelefono = new TextBox();
            label6 = new Label();
            texdireccion = new TextBox();
            label5 = new Label();
            texcorreo = new TextBox();
            label4 = new Label();
            texdpi = new TextBox();
            label3 = new Label();
            texnit = new TextBox();
            label1 = new Label();
            dataGridView1 = new DataGridView();
            cantidad = new DataGridViewTextBoxColumn();
            producto = new DataGridViewTextBoxColumn();
            stock = new DataGridViewTextBoxColumn();
            descuento = new DataGridViewTextBoxColumn();
            precio = new DataGridViewTextBoxColumn();
            subtotal = new DataGridViewTextBoxColumn();
            id_producto_presentacion = new DataGridViewTextBoxColumn();
            button2 = new Button();
            lblUsuario = new Label();
            texsubtotal = new TextBox();
            texdescuento = new TextBox();
            textotal = new TextBox();
            label2 = new Label();
            label7 = new Label();
            label8 = new Label();
            button3 = new Button();
            label9 = new Label();
            label10 = new Label();
            label11 = new Label();
            btnCatalogo = new Button();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackColor = Color.White;
            button1.ImageIndex = 1;
            button1.ImageList = imageList1;
            button1.Location = new Point(1303, 579);
            button1.Name = "button1";
            button1.Size = new Size(75, 65);
            button1.TabIndex = 0;
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "Cancelar.png");
            imageList1.Images.SetKeyName(1, "Cerrar.png");
            imageList1.Images.SetKeyName(2, "Cobrar.png");
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.Gray;
            groupBox1.Controls.Add(cliente);
            groupBox1.Controls.Add(texclientes);
            groupBox1.Controls.Add(labtelefono);
            groupBox1.Controls.Add(textelefono);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(texdireccion);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(texcorreo);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(texdpi);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(texnit);
            groupBox1.Controls.Add(label1);
            groupBox1.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            groupBox1.Location = new Point(205, 23);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(931, 242);
            groupBox1.TabIndex = 11;
            groupBox1.TabStop = false;
            groupBox1.Text = "DATOS DEL CLIENTE";
            // 
            // cliente
            // 
            cliente.AutoSize = true;
            cliente.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            cliente.Location = new Point(29, 59);
            cliente.Name = "cliente";
            cliente.Size = new Size(154, 23);
            cliente.TabIndex = 17;
            cliente.Text = "NOMBRE CLIENTE";
            cliente.Click += cliente_Click;
            // 
            // texclientes
            // 
            texclientes.BackColor = Color.FromArgb(64, 64, 64);
            texclientes.Location = new Point(300, 61);
            texclientes.Name = "texclientes";
            texclientes.Size = new Size(342, 31);
            texclientes.TabIndex = 16;
            // 
            // labtelefono
            // 
            labtelefono.AutoSize = true;
            labtelefono.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labtelefono.Location = new Point(29, 143);
            labtelefono.Name = "labtelefono";
            labtelefono.Size = new Size(95, 23);
            labtelefono.TabIndex = 14;
            labtelefono.Text = "TELEFONO";
            // 
            // textelefono
            // 
            textelefono.BackColor = Color.FromArgb(64, 64, 64);
            textelefono.Location = new Point(300, 145);
            textelefono.Name = "textelefono";
            textelefono.Size = new Size(342, 31);
            textelefono.TabIndex = 13;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label6.Location = new Point(31, 101);
            label6.Name = "label6";
            label6.Size = new Size(100, 23);
            label6.TabIndex = 12;
            label6.Text = "DIRECCION";
            // 
            // texdireccion
            // 
            texdireccion.BackColor = Color.FromArgb(64, 64, 64);
            texdireccion.Location = new Point(300, 103);
            texdireccion.Name = "texdireccion";
            texdireccion.Size = new Size(342, 31);
            texdireccion.TabIndex = 11;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(31, 185);
            label5.Name = "label5";
            label5.Size = new Size(197, 23);
            label5.TabIndex = 10;
            label5.Text = "CORREO ELECTRONICO";
            // 
            // texcorreo
            // 
            texcorreo.BackColor = Color.FromArgb(64, 64, 64);
            texcorreo.Location = new Point(300, 187);
            texcorreo.Name = "texcorreo";
            texcorreo.Size = new Size(342, 31);
            texcorreo.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(658, 189);
            label4.Name = "label4";
            label4.Size = new Size(38, 23);
            label4.TabIndex = 8;
            label4.Text = "DPI";
            // 
            // texdpi
            // 
            texdpi.BackColor = Color.FromArgb(64, 64, 64);
            texdpi.Location = new Point(739, 186);
            texdpi.Name = "texdpi";
            texdpi.Size = new Size(178, 31);
            texdpi.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(658, 146);
            label3.Name = "label3";
            label3.Size = new Size(38, 23);
            label3.TabIndex = 6;
            label3.Text = "NIT";
            // 
            // texnit
            // 
            texnit.BackColor = Color.FromArgb(64, 64, 64);
            texnit.Location = new Point(739, 143);
            texnit.Name = "texnit";
            texnit.Size = new Size(178, 31);
            texnit.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(-168, 162);
            label1.Name = "label1";
            label1.Size = new Size(79, 25);
            label1.TabIndex = 2;
            label1.Text = "Nombre";
            // 
            // dataGridView1
            // 
            dataGridView1.BackgroundColor = Color.Gray;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { cantidad, producto, stock, descuento, precio, subtotal, id_producto_presentacion });
            dataGridView1.Location = new Point(205, 318);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(931, 441);
            dataGridView1.TabIndex = 10;
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
            // 
            // cantidad
            // 
            cantidad.HeaderText = "CANTIDAD";
            cantidad.MinimumWidth = 6;
            cantidad.Name = "cantidad";
            cantidad.Width = 125;
            // 
            // producto
            // 
            producto.HeaderText = "PRODUCTO";
            producto.MinimumWidth = 6;
            producto.Name = "producto";
            producto.Width = 250;
            // 
            // stock
            // 
            stock.HeaderText = "EXISTENCIA";
            stock.MinimumWidth = 6;
            stock.Name = "stock";
            stock.ReadOnly = true;
            stock.Width = 125;
            // 
            // descuento
            // 
            descuento.HeaderText = "DESCUENTO";
            descuento.MinimumWidth = 6;
            descuento.Name = "descuento";
            descuento.Width = 125;
            // 
            // precio
            // 
            precio.HeaderText = "PRECIO";
            precio.MinimumWidth = 6;
            precio.Name = "precio";
            precio.ReadOnly = true;
            precio.Width = 125;
            // 
            // subtotal
            // 
            subtotal.HeaderText = "SUBTOTAL";
            subtotal.MinimumWidth = 6;
            subtotal.Name = "subtotal";
            subtotal.ReadOnly = true;
            subtotal.Width = 125;
            // 
            // id_producto_presentacion
            // 
            id_producto_presentacion.HeaderText = "Column1";
            id_producto_presentacion.MinimumWidth = 6;
            id_producto_presentacion.Name = "id_producto_presentacion";
            id_producto_presentacion.Width = 125;
            // 
            // button2
            // 
            button2.BackColor = Color.White;
            button2.ImageIndex = 0;
            button2.ImageList = imageList1;
            button2.Location = new Point(1303, 661);
            button2.Name = "button2";
            button2.Size = new Size(75, 65);
            button2.TabIndex = 13;
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblUsuario.Location = new Point(29, 43);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(93, 25);
            lblUsuario.TabIndex = 14;
            lblUsuario.Text = "USUARIO";
            // 
            // texsubtotal
            // 
            texsubtotal.BackColor = Color.FromArgb(64, 64, 64);
            texsubtotal.Location = new Point(308, 771);
            texsubtotal.Name = "texsubtotal";
            texsubtotal.Size = new Size(125, 27);
            texsubtotal.TabIndex = 15;
            // 
            // texdescuento
            // 
            texdescuento.BackColor = Color.FromArgb(64, 64, 64);
            texdescuento.Location = new Point(679, 771);
            texdescuento.Name = "texdescuento";
            texdescuento.Size = new Size(125, 27);
            texdescuento.TabIndex = 16;
            // 
            // textotal
            // 
            textotal.BackColor = Color.FromArgb(64, 64, 64);
            textotal.Location = new Point(1011, 771);
            textotal.Name = "textotal";
            textotal.Size = new Size(125, 27);
            textotal.TabIndex = 17;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(208, 775);
            label2.Name = "label2";
            label2.Size = new Size(94, 23);
            label2.TabIndex = 18;
            label2.Text = "SUBTOTAL";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.Location = new Point(564, 775);
            label7.Name = "label7";
            label7.Size = new Size(109, 23);
            label7.TabIndex = 19;
            label7.Text = "DESCUENTO";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label8.Location = new Point(944, 775);
            label8.Name = "label8";
            label8.Size = new Size(61, 23);
            label8.TabIndex = 20;
            label8.Text = "TOTAL";
            // 
            // button3
            // 
            button3.BackColor = Color.White;
            button3.ImageIndex = 2;
            button3.ImageList = imageList1;
            button3.Location = new Point(1303, 745);
            button3.Name = "button3";
            button3.Size = new Size(75, 65);
            button3.TabIndex = 21;
            button3.UseVisualStyleBackColor = false;
            button3.Click += button3_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.Location = new Point(1166, 681);
            label9.Name = "label9";
            label9.Size = new Size(98, 23);
            label9.TabIndex = 22;
            label9.Text = "CANCELAR";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.Location = new Point(1166, 599);
            label10.Name = "label10";
            label10.Size = new Size(57, 23);
            label10.TabIndex = 23;
            label10.Text = "SALIR";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label11.Location = new Point(1166, 772);
            label11.Name = "label11";
            label11.Size = new Size(79, 23);
            label11.TabIndex = 24;
            label11.Text = "COBRAR";
            // 
            // btnCatalogo
            // 
            btnCatalogo.ForeColor = Color.Black;
            btnCatalogo.Location = new Point(1231, 48);
            btnCatalogo.Name = "btnCatalogo";
            btnCatalogo.Size = new Size(94, 67);
            btnCatalogo.TabIndex = 25;
            btnCatalogo.Text = "CATALOGO";
            btnCatalogo.UseVisualStyleBackColor = true;
            btnCatalogo.Click += button4_Click;
            // 
            // Ventas
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DimGray;
            ClientSize = new Size(1438, 822);
            Controls.Add(btnCatalogo);
            Controls.Add(label11);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(button3);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label2);
            Controls.Add(textotal);
            Controls.Add(texdescuento);
            Controls.Add(texsubtotal);
            Controls.Add(lblUsuario);
            Controls.Add(button2);
            Controls.Add(groupBox1);
            Controls.Add(dataGridView1);
            Controls.Add(button1);
            Name = "Ventas";
            Text = "VENTAS";
            Load += cotizacion_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private GroupBox groupBox1;
        private Label label6;
        private TextBox texdireccion;
        private Label label5;
        private TextBox texcorreo;
        private Label label4;
        private TextBox texdpi;
        private Label label3;
        private TextBox texnit;
        private Label label1;
        private DataGridView dataGridView1;
        private Button button2;
        private Label labtelefono;
        private TextBox textelefono;
        private Label lblUsuario;
        private TextBox texsubtotal;
        private TextBox texdescuento;
        private TextBox textotal;
        private Label label2;
        private Label label7;
        private Label label8;
        private Button button3;
        private Label cliente;
        private TextBox texclientes;
        private DataGridViewTextBoxColumn cantidad;
        private DataGridViewTextBoxColumn producto;
        private DataGridViewTextBoxColumn stock;
        private DataGridViewTextBoxColumn descuento;
        private DataGridViewTextBoxColumn precio;
        private DataGridViewTextBoxColumn subtotal;
        private ImageList imageList1;
        private Label label9;
        private Label label10;
        private Label label11;
        private Button btnCatalogo;
        private DataGridViewTextBoxColumn id_producto_presentacion;
    }
}