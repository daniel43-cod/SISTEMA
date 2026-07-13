namespace SISTEMA_FROTEND
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            panelContenido = new Panel();
            panelMenu = new Panel();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            btningreso = new Button();
            imageList1 = new ImageList(components);
            btncreditos = new Button();
            btnregistroventas = new Button();
            btnproductos = new Button();
            btnusuarios = new Button();
            btnregistrar = new Button();
            label7 = new Label();
            button1 = new Button();
            panelMenu.SuspendLayout();
            SuspendLayout();
            // 
            // panelContenido
            // 
            panelContenido.BackColor = Color.Gray;
            panelContenido.BorderStyle = BorderStyle.FixedSingle;
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.ForeColor = Color.White;
            panelContenido.Location = new Point(0, 0);
            panelContenido.Name = "panelContenido";
            panelContenido.Padding = new Padding(20);
            panelContenido.Size = new Size(1374, 677);
            panelContenido.TabIndex = 3;
            panelContenido.Paint += panelContenido_Paint;
            // 
            // panelMenu
            // 
            panelMenu.BackColor = Color.FromArgb(64, 64, 64);
            panelMenu.Controls.Add(label7);
            panelMenu.Controls.Add(button1);
            panelMenu.Controls.Add(label6);
            panelMenu.Controls.Add(label5);
            panelMenu.Controls.Add(label4);
            panelMenu.Controls.Add(label3);
            panelMenu.Controls.Add(label2);
            panelMenu.Controls.Add(label1);
            panelMenu.Controls.Add(btningreso);
            panelMenu.Controls.Add(btncreditos);
            panelMenu.Controls.Add(btnregistroventas);
            panelMenu.Controls.Add(btnproductos);
            panelMenu.Controls.Add(btnusuarios);
            panelMenu.Controls.Add(btnregistrar);
            panelMenu.Dock = DockStyle.Left;
            panelMenu.Location = new Point(0, 0);
            panelMenu.Name = "panelMenu";
            panelMenu.Size = new Size(377, 677);
            panelMenu.TabIndex = 4;
            panelMenu.Paint += panelMenu_Paint;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label6.ForeColor = Color.White;
            label6.Location = new Point(24, 644);
            label6.Margin = new Padding(0);
            label6.Name = "label6";
            label6.Size = new Size(162, 25);
            label6.TabIndex = 10;
            label6.Text = "CONFIGURACION";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.White;
            label5.Location = new Point(24, 538);
            label5.Margin = new Padding(0);
            label5.Name = "label5";
            label5.Size = new Size(178, 25);
            label5.TabIndex = 9;
            label5.Text = "NUEVO PRODUCTO";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.ForeColor = Color.White;
            label4.Location = new Point(24, 427);
            label4.Margin = new Padding(0);
            label4.Name = "label4";
            label4.Size = new Size(221, 25);
            label4.TabIndex = 0;
            label4.Text = "REGISTRO DE COMPRAS";
            label4.Click += label4_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = SystemColors.ButtonFace;
            label3.Location = new Point(24, 324);
            label3.Name = "label3";
            label3.Size = new Size(100, 25);
            label3.TabIndex = 8;
            label3.Text = "COMPRAS";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.White;
            label2.Location = new Point(24, 224);
            label2.Name = "label2";
            label2.Size = new Size(202, 25);
            label2.TabIndex = 7;
            label2.Text = "REGISTRO DE VENTAS";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.ButtonHighlight;
            label1.Location = new Point(24, 38);
            label1.Name = "label1";
            label1.Size = new Size(71, 25);
            label1.TabIndex = 6;
            label1.Text = "VENTA";
            // 
            // btningreso
            // 
            btningreso.BackColor = Color.Gainsboro;
            btningreso.ImageIndex = 4;
            btningreso.ImageList = imageList1;
            btningreso.Location = new Point(262, 520);
            btningreso.Name = "btningreso";
            btningreso.Size = new Size(75, 65);
            btningreso.TabIndex = 5;
            btningreso.UseVisualStyleBackColor = false;
            btningreso.Click += btningreso_Click;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageStream = (ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = Color.Transparent;
            imageList1.Images.SetKeyName(0, "Venta.png");
            imageList1.Images.SetKeyName(1, "Registro.png");
            imageList1.Images.SetKeyName(2, "Compras.png");
            imageList1.Images.SetKeyName(3, "Pedidos.png");
            imageList1.Images.SetKeyName(4, "Crear.png");
            imageList1.Images.SetKeyName(5, "Configuracion.png");
            imageList1.Images.SetKeyName(6, "Catalogo.png");
            // 
            // btncreditos
            // 
            btncreditos.BackColor = Color.Gainsboro;
            btncreditos.ForeColor = Color.Gainsboro;
            btncreditos.ImageIndex = 3;
            btncreditos.ImageList = imageList1;
            btncreditos.Location = new Point(262, 409);
            btncreditos.Margin = new Padding(0);
            btncreditos.Name = "btncreditos";
            btncreditos.Size = new Size(75, 65);
            btncreditos.TabIndex = 4;
            btncreditos.UseVisualStyleBackColor = false;
            btncreditos.Click += btncreditos_Click;
            // 
            // btnregistroventas
            // 
            btnregistroventas.BackColor = Color.Gainsboro;
            btnregistroventas.ImageIndex = 1;
            btnregistroventas.ImageList = imageList1;
            btnregistroventas.Location = new Point(262, 206);
            btnregistroventas.Name = "btnregistroventas";
            btnregistroventas.Size = new Size(75, 65);
            btnregistroventas.TabIndex = 1;
            btnregistroventas.UseVisualStyleBackColor = false;
            btnregistroventas.Click += btnregistroventas_Click;
            // 
            // btnproductos
            // 
            btnproductos.BackColor = Color.Gainsboro;
            btnproductos.ImageIndex = 2;
            btnproductos.ImageList = imageList1;
            btnproductos.Location = new Point(262, 306);
            btnproductos.Name = "btnproductos";
            btnproductos.Size = new Size(75, 65);
            btnproductos.TabIndex = 2;
            btnproductos.UseVisualStyleBackColor = false;
            btnproductos.Click += btnproductos_Click;
            // 
            // btnusuarios
            // 
            btnusuarios.BackColor = Color.Gainsboro;
            btnusuarios.ImageIndex = 5;
            btnusuarios.ImageList = imageList1;
            btnusuarios.Location = new Point(262, 626);
            btnusuarios.Name = "btnusuarios";
            btnusuarios.Size = new Size(75, 65);
            btnusuarios.TabIndex = 3;
            btnusuarios.UseVisualStyleBackColor = false;
            btnusuarios.Click += btnusuarios_Click;
            // 
            // btnregistrar
            // 
            btnregistrar.BackColor = Color.Gainsboro;
            btnregistrar.ImageIndex = 0;
            btnregistrar.ImageList = imageList1;
            btnregistrar.Location = new Point(262, 20);
            btnregistrar.Name = "btnregistrar";
            btnregistrar.Size = new Size(75, 65);
            btnregistrar.TabIndex = 0;
            btnregistrar.UseVisualStyleBackColor = false;
            btnregistrar.Click += btnregistrar_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.ForeColor = Color.White;
            label7.Location = new Point(24, 134);
            label7.Name = "label7";
            label7.Size = new Size(108, 25);
            label7.TabIndex = 12;
            label7.Text = "CATALOGO";
            // 
            // button1
            // 
            button1.BackColor = Color.Gainsboro;
            button1.ImageIndex = 6;
            button1.ImageList = imageList1;
            button1.Location = new Point(262, 116);
            button1.Name = "button1";
            button1.Size = new Size(75, 65);
            button1.TabIndex = 11;
            button1.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1374, 677);
            Controls.Add(panelMenu);
            Controls.Add(panelContenido);
            Name = "Form1";
            Text = "POOS";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load_2;
            panelMenu.ResumeLayout(false);
            panelMenu.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panelContenido;
        private Button btningreso;
        private Button btnregistroventas;
        private Button btnusuarios;
        private Button btnregistrar;
        private Button btncreditos;
        private Button btnproductos;
        private Panel panelMenu;
        private Label lblUsuario;
        private Label label1;
        private Label label2;
        private ImageList imageList1;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Button button1;
    }
}
