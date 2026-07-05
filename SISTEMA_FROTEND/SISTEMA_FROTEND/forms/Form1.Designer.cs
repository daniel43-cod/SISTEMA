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
            panelContenido = new Panel();
            panelMenu = new Panel();
            btningreso = new Button();
            btncreditos = new Button();
            btnregistroventas = new Button();
            btnproductos = new Button();
            btnusuarios = new Button();
            btnregistrar = new Button();
            panelMenu.SuspendLayout();
            SuspendLayout();
            // 
            // panelContenido
            // 
            panelContenido.BackColor = SystemColors.ActiveCaption;
            panelContenido.BorderStyle = BorderStyle.FixedSingle;
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(0, 0);
            panelContenido.Name = "panelContenido";
            panelContenido.Padding = new Padding(20);
            panelContenido.Size = new Size(1374, 541);
            panelContenido.TabIndex = 3;
            // 
            // panelMenu
            // 
            panelMenu.BackColor = SystemColors.HotTrack;
            panelMenu.Controls.Add(btningreso);
            panelMenu.Controls.Add(btncreditos);
            panelMenu.Controls.Add(btnregistroventas);
            panelMenu.Controls.Add(btnproductos);
            panelMenu.Controls.Add(btnusuarios);
            panelMenu.Controls.Add(btnregistrar);
            panelMenu.Dock = DockStyle.Left;
            panelMenu.Location = new Point(0, 0);
            panelMenu.Name = "panelMenu";
            panelMenu.Size = new Size(216, 541);
            panelMenu.TabIndex = 4;
            panelMenu.Paint += panelMenu_Paint;
            // 
            // btningreso
            // 
            btningreso.BackColor = Color.FromArgb(0, 192, 0);
            btningreso.Location = new Point(12, 281);
            btningreso.Name = "btningreso";
            btningreso.Size = new Size(188, 63);
            btningreso.TabIndex = 5;
            btningreso.Text = "INGRESO";
            btningreso.UseVisualStyleBackColor = false;
            btningreso.Click += btningreso_Click;
            // 
            // btncreditos
            // 
            btncreditos.BackColor = Color.FromArgb(0, 192, 0);
            btncreditos.Location = new Point(12, 367);
            btncreditos.Name = "btncreditos";
            btncreditos.Size = new Size(188, 63);
            btncreditos.TabIndex = 4;
            btncreditos.Text = "CREDITOS";
            btncreditos.UseVisualStyleBackColor = false;
            btncreditos.Click += btncreditos_Click;
            // 
            // btnregistroventas
            // 
            btnregistroventas.BackColor = Color.FromArgb(0, 192, 0);
            btnregistroventas.Location = new Point(12, 108);
            btnregistroventas.Name = "btnregistroventas";
            btnregistroventas.Size = new Size(188, 63);
            btnregistroventas.TabIndex = 1;
            btnregistroventas.Text = "REGISTRO DE VENTAS DIARIO";
            btnregistroventas.UseVisualStyleBackColor = false;
            btnregistroventas.Click += btnregistroventas_Click;
            // 
            // btnproductos
            // 
            btnproductos.BackColor = Color.FromArgb(0, 192, 0);
            btnproductos.Location = new Point(12, 194);
            btnproductos.Name = "btnproductos";
            btnproductos.Size = new Size(188, 63);
            btnproductos.TabIndex = 2;
            btnproductos.Text = "PRODUCTOS";
            btnproductos.UseVisualStyleBackColor = false;
            btnproductos.Click += btnproductos_Click;
            // 
            // btnusuarios
            // 
            btnusuarios.BackColor = Color.FromArgb(0, 192, 0);
            btnusuarios.Location = new Point(12, 454);
            btnusuarios.Name = "btnusuarios";
            btnusuarios.Size = new Size(188, 63);
            btnusuarios.TabIndex = 3;
            btnusuarios.Text = "USUARIOS";
            btnusuarios.UseVisualStyleBackColor = false;
            btnusuarios.Click += btnusuarios_Click;
            // 
            // btnregistrar
            // 
            btnregistrar.BackColor = Color.FromArgb(0, 192, 0);
            btnregistrar.Location = new Point(12, 19);
            btnregistrar.Name = "btnregistrar";
            btnregistrar.Size = new Size(188, 63);
            btnregistrar.TabIndex = 0;
            btnregistrar.Text = "REGISTRAR";
            btnregistrar.UseVisualStyleBackColor = false;
            btnregistrar.Click += btnregistrar_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1374, 541);
            Controls.Add(panelMenu);
            Controls.Add(panelContenido);
            Name = "Form1";
            Text = "SISTEMA DE PUNTO DE VENTA";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load_2;
            panelMenu.ResumeLayout(false);
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
    }
}
