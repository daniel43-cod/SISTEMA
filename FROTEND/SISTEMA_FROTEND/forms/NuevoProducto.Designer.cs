namespace SISTEMA_FROTEND.presentacion
{
    partial class NuevoProducto
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NuevoProducto));
            label1 = new Label();
            texcantidad = new TextBox();
            texpreciocompra = new TextBox();
            label4 = new Label();
            texcodigobarras = new TextBox();
            label6 = new Label();
            label7 = new Label();
            texdescripcion = new TextBox();
            button1 = new Button();
            imageList1 = new ImageList(components);
            label8 = new Label();
            butIngresarproducto = new Button();
            texproducto = new TextBox();
            label9 = new Label();
            comcategoria = new ComboBox();
            texminima = new TextBox();
            groupBox1 = new GroupBox();
            labdescripcion = new Label();
            labcodigobarras = new Label();
            labproducto = new Label();
            labcategoria = new Label();
            labpreciocompra = new Label();
            labcantidad = new Label();
            labexistenciamin = new Label();
            labpreciomayor = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            label5 = new Label();
            label2 = new Label();
            groupBox2 = new GroupBox();
            picimagen = new PictureBox();
            bulimpiar = new Button();
            groupBox3 = new GroupBox();
            groupBox4 = new GroupBox();
            butsalir = new Button();
            label11 = new Label();
            label12 = new Label();
            lblUsuario = new Label();
            dgvPresentaciones = new DataGridView();
            label13 = new Label();
            label3 = new Label();
            label10 = new Label();
            label14 = new Label();
            label15 = new Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picimagen).BeginInit();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPresentaciones).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(102, 149);
            label1.Name = "label1";
            label1.Size = new Size(127, 23);
            label1.TabIndex = 1;
            label1.Text = "PRODUCTO:(*)";
            // 
            // texcantidad
            // 
            texcantidad.BackColor = Color.FromArgb(64, 64, 64);
            texcantidad.Location = new Point(271, 42);
            texcantidad.Name = "texcantidad";
            texcantidad.Size = new Size(321, 31);
            texcantidad.TabIndex = 4;
            // 
            // texpreciocompra
            // 
            texpreciocompra.BackColor = Color.FromArgb(64, 64, 64);
            texpreciocompra.Location = new Point(285, 26);
            texpreciocompra.Name = "texpreciocompra";
            texpreciocompra.Size = new Size(334, 31);
            texpreciocompra.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(14, 34);
            label4.Name = "label4";
            label4.Size = new Size(257, 23);
            label4.TabIndex = 7;
            label4.Text = "TOTAL DE PRECIO DE COMPRA";
            // 
            // texcodigobarras
            // 
            texcodigobarras.BackColor = Color.FromArgb(64, 64, 64);
            texcodigobarras.Location = new Point(324, 275);
            texcodigobarras.Name = "texcodigobarras";
            texcodigobarras.Size = new Size(309, 27);
            texcodigobarras.TabIndex = 3;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label6.Location = new Point(102, 278);
            label6.Name = "label6";
            label6.Size = new Size(191, 23);
            label6.TabIndex = 11;
            label6.Text = "CODIGO DE BARRA:(*)";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.Location = new Point(102, 213);
            label7.Name = "label7";
            label7.Size = new Size(131, 23);
            label7.TabIndex = 14;
            label7.Text = "DESCRIPCION: ";
            // 
            // texdescripcion
            // 
            texdescripcion.BackColor = Color.FromArgb(64, 64, 64);
            texdescripcion.Location = new Point(324, 210);
            texdescripcion.Name = "texdescripcion";
            texdescripcion.Size = new Size(309, 27);
            texdescripcion.TabIndex = 2;
            // 
            // button1
            // 
            button1.ImageIndex = 0;
            button1.ImageList = imageList1;
            button1.Location = new Point(265, 30);
            button1.Name = "button1";
            button1.Size = new Size(75, 65);
            button1.TabIndex = 16;
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "Imagen.png");
            imageList1.Images.SetKeyName(1, "Guardar.png");
            imageList1.Images.SetKeyName(2, "Salir.png");
            imageList1.Images.SetKeyName(3, "Cerrar.png");
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label8.Location = new Point(64, 64);
            label8.Name = "label8";
            label8.Size = new Size(165, 23);
            label8.TabIndex = 17;
            label8.Text = "INGRESAR IMAGEN";
            // 
            // butIngresarproducto
            // 
            butIngresarproducto.BackColor = Color.White;
            butIngresarproducto.ImageIndex = 1;
            butIngresarproducto.ImageList = imageList1;
            butIngresarproducto.Location = new Point(582, 770);
            butIngresarproducto.Name = "butIngresarproducto";
            butIngresarproducto.Size = new Size(75, 65);
            butIngresarproducto.TabIndex = 19;
            butIngresarproducto.UseVisualStyleBackColor = false;
            butIngresarproducto.Click += button2_Click;
            // 
            // texproducto
            // 
            texproducto.BackColor = Color.FromArgb(64, 64, 64);
            texproducto.ForeColor = Color.FromArgb(64, 64, 64);
            texproducto.Location = new Point(324, 146);
            texproducto.Name = "texproducto";
            texproducto.Size = new Size(309, 27);
            texproducto.TabIndex = 1;
            texproducto.TextChanged += texproducto_TextChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.Location = new Point(102, 85);
            label9.Name = "label9";
            label9.Size = new Size(129, 23);
            label9.TabIndex = 21;
            label9.Text = "CATEGORIA:(*)";
            // 
            // comcategoria
            // 
            comcategoria.BackColor = Color.FromArgb(64, 64, 64);
            comcategoria.FormattingEnabled = true;
            comcategoria.Location = new Point(324, 81);
            comcategoria.Name = "comcategoria";
            comcategoria.Size = new Size(309, 28);
            comcategoria.TabIndex = 0;
            // 
            // texminima
            // 
            texminima.BackColor = Color.FromArgb(64, 64, 64);
            texminima.Location = new Point(271, 102);
            texminima.Name = "texminima";
            texminima.Size = new Size(321, 31);
            texminima.TabIndex = 5;
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.Gray;
            groupBox1.Controls.Add(labcodigobarras);
            groupBox1.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox1.Location = new Point(64, 45);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(663, 298);
            groupBox1.TabIndex = 25;
            groupBox1.TabStop = false;
            groupBox1.Text = "DATOS DEL PRODUCTO";
            groupBox1.Enter += groupBox1_Enter;
            // 
            // labdescripcion
            // 
            labdescripcion.AutoSize = true;
            labdescripcion.ForeColor = Color.Gray;
            labdescripcion.Location = new Point(324, 240);
            labdescripcion.Name = "labdescripcion";
            labdescripcion.Size = new Size(12, 20);
            labdescripcion.TabIndex = 32;
            labdescripcion.Text = ".";
            // 
            // labcodigobarras
            // 
            labcodigobarras.AutoSize = true;
            labcodigobarras.Location = new Point(259, 260);
            labcodigobarras.Name = "labcodigobarras";
            labcodigobarras.Size = new Size(17, 25);
            labcodigobarras.TabIndex = 29;
            labcodigobarras.Text = ".";
            // 
            // labproducto
            // 
            labproducto.AutoSize = true;
            labproducto.ForeColor = Color.Gray;
            labproducto.Location = new Point(324, 176);
            labproducto.Name = "labproducto";
            labproducto.Size = new Size(12, 20);
            labproducto.TabIndex = 28;
            labproducto.Text = ".";
            // 
            // labcategoria
            // 
            labcategoria.AutoSize = true;
            labcategoria.ForeColor = Color.Gray;
            labcategoria.Location = new Point(324, 112);
            labcategoria.Name = "labcategoria";
            labcategoria.Size = new Size(12, 20);
            labcategoria.TabIndex = 26;
            labcategoria.Text = ".";
            labcategoria.Click += label10_Click;
            // 
            // labpreciocompra
            // 
            labpreciocompra.AutoSize = true;
            labpreciocompra.ForeColor = Color.Gray;
            labpreciocompra.Location = new Point(285, 56);
            labpreciocompra.Name = "labpreciocompra";
            labpreciocompra.Size = new Size(17, 25);
            labpreciocompra.TabIndex = 31;
            labpreciocompra.Text = ".";
            // 
            // labcantidad
            // 
            labcantidad.AutoSize = true;
            labcantidad.ForeColor = Color.Gray;
            labcantidad.Location = new Point(271, 72);
            labcantidad.Name = "labcantidad";
            labcantidad.Size = new Size(17, 25);
            labcantidad.TabIndex = 33;
            labcantidad.Text = ".";
            // 
            // labexistenciamin
            // 
            labexistenciamin.AutoSize = true;
            labexistenciamin.ForeColor = Color.Gray;
            labexistenciamin.Location = new Point(271, 136);
            labexistenciamin.Name = "labexistenciamin";
            labexistenciamin.Size = new Size(17, 25);
            labexistenciamin.TabIndex = 34;
            labexistenciamin.Text = ".";
            // 
            // labpreciomayor
            // 
            labpreciomayor.AutoSize = true;
            labpreciomayor.Location = new Point(285, 179);
            labpreciomayor.Name = "labpreciomayor";
            labpreciomayor.Size = new Size(17, 25);
            labpreciomayor.TabIndex = 35;
            labpreciomayor.Text = ".";
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(24, 109);
            label5.Name = "label5";
            label5.Size = new Size(177, 23);
            label5.TabIndex = 23;
            label5.Text = "EXISTENCIA MINIMA";
            label5.Click += label5_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(24, 45);
            label2.Name = "label2";
            label2.Size = new Size(104, 23);
            label2.TabIndex = 3;
            label2.Text = "CANTIDAD:";
            // 
            // groupBox2
            // 
            groupBox2.BackColor = Color.Gray;
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(labexistenciamin);
            groupBox2.Controls.Add(texcantidad);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(labcantidad);
            groupBox2.Controls.Add(texminima);
            groupBox2.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox2.Location = new Point(64, 349);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(663, 176);
            groupBox2.TabIndex = 38;
            groupBox2.TabStop = false;
            groupBox2.Text = "INVENTARIO ";
            groupBox2.Enter += groupBox2_Enter;
            // 
            // picimagen
            // 
            picimagen.BackColor = SystemColors.ControlDarkDark;
            picimagen.Location = new Point(41, 134);
            picimagen.Name = "picimagen";
            picimagen.Size = new Size(362, 279);
            picimagen.TabIndex = 18;
            picimagen.TabStop = false;
            picimagen.Click += picimagen_Click;
            // 
            // bulimpiar
            // 
            bulimpiar.BackColor = Color.White;
            bulimpiar.ImageIndex = 3;
            bulimpiar.ImageList = imageList1;
            bulimpiar.Location = new Point(131, 770);
            bulimpiar.Name = "bulimpiar";
            bulimpiar.Size = new Size(75, 65);
            bulimpiar.TabIndex = 27;
            bulimpiar.UseVisualStyleBackColor = false;
            bulimpiar.Click += bulimpiar_Click;
            // 
            // groupBox3
            // 
            groupBox3.BackColor = Color.Gray;
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(texpreciocompra);
            groupBox3.Controls.Add(labpreciocompra);
            groupBox3.Controls.Add(labpreciomayor);
            groupBox3.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox3.Location = new Point(64, 561);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(663, 92);
            groupBox3.TabIndex = 39;
            groupBox3.TabStop = false;
            groupBox3.Text = "PRECIOS";
            // 
            // groupBox4
            // 
            groupBox4.BackColor = Color.Gray;
            groupBox4.Controls.Add(button1);
            groupBox4.Controls.Add(picimagen);
            groupBox4.Controls.Add(label8);
            groupBox4.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox4.Location = new Point(755, 68);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(445, 433);
            groupBox4.TabIndex = 40;
            groupBox4.TabStop = false;
            groupBox4.Text = "PRESENTACION";
            // 
            // butsalir
            // 
            butsalir.BackColor = Color.White;
            butsalir.ImageIndex = 2;
            butsalir.ImageList = imageList1;
            butsalir.Location = new Point(373, 770);
            butsalir.Name = "butsalir";
            butsalir.Size = new Size(75, 65);
            butsalir.TabIndex = 41;
            butsalir.UseVisualStyleBackColor = false;
            butsalir.Click += butsalir_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(504, 45);
            label11.Name = "label11";
            label11.Size = new Size(0, 20);
            label11.TabIndex = 42;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label12.Location = new Point(667, 9);
            label12.Name = "label12";
            label12.Size = new Size(254, 28);
            label12.TabIndex = 43;
            label12.Text = "INGRESO DE PRODUCTOS";
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(1488, 9);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(71, 20);
            lblUsuario.TabIndex = 44;
            lblUsuario.Text = "USUARIO";
            // 
            // dgvPresentaciones
            // 
            dgvPresentaciones.BackgroundColor = Color.Gray;
            dgvPresentaciones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPresentaciones.Location = new Point(755, 561);
            dgvPresentaciones.Name = "dgvPresentaciones";
            dgvPresentaciones.RowHeadersWidth = 51;
            dgvPresentaciones.Size = new Size(764, 262);
            dgvPresentaciones.TabIndex = 45;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(755, 535);
            label13.Name = "label13";
            label13.Size = new Size(139, 20);
            label13.TabIndex = 46;
            label13.Text = "FORMAS DE VENTA";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(1255, 381);
            label3.Name = "label3";
            label3.Size = new Size(47, 20);
            label3.TabIndex = 47;
            label3.Text = "SALIR";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(26, 792);
            label10.Name = "label10";
            label10.Size = new Size(58, 20);
            label10.TabIndex = 48;
            label10.Text = "label10";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label14.Location = new Point(475, 792);
            label14.Name = "label14";
            label14.Size = new Size(101, 23);
            label14.TabIndex = 49;
            label14.Text = "REGISTRAR";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label15.Location = new Point(265, 792);
            label15.Name = "label15";
            label15.Size = new Size(101, 23);
            label15.TabIndex = 50;
            label15.Text = "REGISTRAR";
            // 
            // ingreso
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Gray;
            ClientSize = new Size(1571, 913);
            Controls.Add(label15);
            Controls.Add(label14);
            Controls.Add(label10);
            Controls.Add(label3);
            Controls.Add(label9);
            Controls.Add(label13);
            Controls.Add(label7);
            Controls.Add(dgvPresentaciones);
            Controls.Add(label6);
            Controls.Add(bulimpiar);
            Controls.Add(lblUsuario);
            Controls.Add(labdescripcion);
            Controls.Add(label12);
            Controls.Add(label11);
            Controls.Add(label1);
            Controls.Add(butsalir);
            Controls.Add(texproducto);
            Controls.Add(texdescripcion);
            Controls.Add(groupBox4);
            Controls.Add(labproducto);
            Controls.Add(groupBox3);
            Controls.Add(comcategoria);
            Controls.Add(groupBox2);
            Controls.Add(labcategoria);
            Controls.Add(texcodigobarras);
            Controls.Add(groupBox1);
            Controls.Add(butIngresarproducto);
            Name = "ingreso";
            WindowState = FormWindowState.Maximized;
            Load += ingreso_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picimagen).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPresentaciones).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void picimagen_Click(object sender, EventArgs e)
        {
          //  throw new NotImplementedException();
        }

        private void label5_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void label10_Click(object sender, EventArgs e)
        {
           // throw new NotImplementedException();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
          //  throw new NotImplementedException();
        }

        private void texproducto_TextChanged(object sender, EventArgs e)
        {
           // throw new NotImplementedException();
        }

        #endregion
        private Label label1;
        private TextBox texcantidad;
        private TextBox texpreciocompra;
        private Label label4;
        private TextBox texpre;
        private TextBox texcodigobarras;
        private Label label6;
        private Label label7;
        private TextBox texdescripcion;
        private Button button1;
        private Label label8;
        private Button butIngresarproducto;
        private TextBox texproducto;
        private Label label9;
        private ComboBox comcategoria;
        private TextBox texminima;
        private GroupBox groupBox1;
        private ImageList imageList1;
        private Label labcategoria;
        private Label labproducto;
        private Label labcodigobarras;
        private Label labpreciocompra;
        private Label labdescripcion;
        private Label labcantidad;
        private Label labexistenciamin;
        private Label labpreciomayor;
        private System.Windows.Forms.Timer timer1;
        private Label label5;
        private Label label2;
        private GroupBox groupBox2;
        private PictureBox picimagen;
        private Button bulimpiar;
        private GroupBox groupBox3;
        private GroupBox groupBox4;
        private Button butsalir;
        private Label label11;
        private Label label12;
        private Label lblUsuario;
        private DataGridView dgvPresentaciones;
        private Label label13;
        private Label label3;
        private Label label10;
        private Label label14;
        private Label label15;
    }
}