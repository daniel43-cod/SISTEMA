namespace SISTEMA_FROTEND
{
    partial class MenuPrincipal 
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuPrincipal));
            panelContenido = new Panel();
            panelMenu = new Panel();
            imageList1 = new ImageList(components);
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            btnnuevoproducto = new Button();
            btnregistrocompras = new Button();
            btnregistroventas = new Button();
            btncompras = new Button();
            btnusuarios = new Button();
            btnregistrar = new Button();
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
            panelMenu.Controls.Add(label6);
            panelMenu.Controls.Add(label5);
            panelMenu.Controls.Add(label4);
            panelMenu.Controls.Add(label3);
            panelMenu.Controls.Add(label2);
            panelMenu.Controls.Add(label1);
            panelMenu.Controls.Add(btnnuevoproducto);
            panelMenu.Controls.Add(btnregistrocompras);
            panelMenu.Controls.Add(btnregistroventas);
            panelMenu.Controls.Add(btncompras);
            panelMenu.Controls.Add(btnusuarios);
            panelMenu.Controls.Add(btnregistrar);
            panelMenu.Dock = DockStyle.Left;
            panelMenu.Location = new Point(0, 0);
            panelMenu.Name = "panelMenu";
            panelMenu.Size = new Size(377, 677);
            panelMenu.TabIndex = 4;
            panelMenu.Paint += panelMenu_Paint;
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
            imageList1.Images.SetKeyName(7, "Cancelar.png");
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label6.ForeColor = Color.White;
            label6.Location = new Point(24, 531);
            label6.Margin = new Padding(0);
            label6.Name = "label6";
            label6.Size = new Size(57, 25);
            label6.TabIndex = 10;
            label6.Text = "CAJA";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.White;
            label5.Location = new Point(24, 443);
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
            label4.Location = new Point(24, 332);
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
            label3.Location = new Point(24, 230);
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
            label2.Location = new Point(24, 127);
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
            // btnnuevoproducto
            // 
            btnnuevoproducto.BackColor = Color.Gainsboro;
            btnnuevoproducto.ImageIndex = 4;
            btnnuevoproducto.ImageList = imageList1;
            btnnuevoproducto.Location = new Point(262, 425);
            btnnuevoproducto.Name = "btnnuevoproducto";
            btnnuevoproducto.Size = new Size(75, 65);
            btnnuevoproducto.TabIndex = 5;
            btnnuevoproducto.UseVisualStyleBackColor = false;
            btnnuevoproducto.Click += btningreso_Click;
            // 
            // btnregistrocompras
            // 
            btnregistrocompras.BackColor = Color.Gainsboro;
            btnregistrocompras.ForeColor = Color.Gainsboro;
            btnregistrocompras.ImageIndex = 3;
            btnregistrocompras.ImageList = imageList1;
            btnregistrocompras.Location = new Point(262, 314);
            btnregistrocompras.Margin = new Padding(0);
            btnregistrocompras.Name = "btnregistrocompras";
            btnregistrocompras.Size = new Size(75, 65);
            btnregistrocompras.TabIndex = 4;
            btnregistrocompras.UseVisualStyleBackColor = false;
            btnregistrocompras.Click += btncreditos_Click;
            // 
            // btnregistroventas
            // 
            btnregistroventas.BackColor = Color.Gainsboro;
            btnregistroventas.ImageIndex = 1;
            btnregistroventas.ImageList = imageList1;
            btnregistroventas.Location = new Point(262, 109);
            btnregistroventas.Name = "btnregistroventas";
            btnregistroventas.Size = new Size(75, 65);
            btnregistroventas.TabIndex = 1;
            btnregistroventas.UseVisualStyleBackColor = false;
            btnregistroventas.Click += btnregistroventas_Click;
            // 
            // btncompras
            // 
            btncompras.BackColor = Color.Gainsboro;
            btncompras.ImageIndex = 2;
            btncompras.ImageList = imageList1;
            btncompras.Location = new Point(262, 212);
            btncompras.Name = "btncompras";
            btncompras.Size = new Size(75, 65);
            btncompras.TabIndex = 2;
            btncompras.UseVisualStyleBackColor = false;
            btncompras.Click += btnproductos_Click;
            // 
            // btnusuarios
            // 
            btnusuarios.BackColor = Color.Gainsboro;
            btnusuarios.ImageIndex = 5;
            btnusuarios.ImageList = imageList1;
            btnusuarios.Location = new Point(262, 513);
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
            // MenuPrincipal
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1374, 677);
            Controls.Add(panelMenu);
            Controls.Add(panelContenido);
            Name = "MenuPrincipal";
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
        private Button btncompras;
        private Button btnregistrocompras;
        private Button btnnuevoproducto;
    }
}
