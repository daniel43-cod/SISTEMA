namespace SISTEMA_FROTEND
{
    partial class Caja
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
            tabcaja = new TabControl();
            tabPage1 = new TabPage();
            lblUsuario = new Label();
            lblCaja = new Label();
            groupBox2 = new GroupBox();
            button1 = new Button();
            texmontoesperado = new TextBox();
            label6 = new Label();
            texobservacioncierre = new TextBox();
            button2 = new Button();
            texmontocontado = new TextBox();
            label4 = new Label();
            label5 = new Label();
            groupBox1 = new GroupBox();
            texobservacionapertura = new TextBox();
            butcrearcaja = new Button();
            textmontoinicial = new TextBox();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            tabPage2 = new TabPage();
            datasesiones = new DataGridView();
            tabcaja.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)datasesiones).BeginInit();
            SuspendLayout();
            // 
            // tabcaja
            // 
            tabcaja.Controls.Add(tabPage1);
            tabcaja.Controls.Add(tabPage2);
            tabcaja.Location = new Point(12, 12);
            tabcaja.Name = "tabcaja";
            tabcaja.SelectedIndex = 0;
            tabcaja.Size = new Size(997, 500);
            tabcaja.TabIndex = 0;
            tabcaja.SelectedIndexChanged += tabcaja_SelectedIndexChanged;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(lblUsuario);
            tabPage1.Controls.Add(lblCaja);
            tabPage1.Controls.Add(groupBox2);
            tabPage1.Controls.Add(groupBox1);
            tabPage1.Controls.Add(label1);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(989, 467);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "CAJA";
            tabPage1.UseVisualStyleBackColor = true;
            tabPage1.Click += tabPage1_Click;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(30, 19);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(50, 20);
            lblUsuario.TabIndex = 15;
            lblUsuario.Text = "label7";
            // 
            // lblCaja
            // 
            lblCaja.AutoSize = true;
            lblCaja.Location = new Point(530, 41);
            lblCaja.Name = "lblCaja";
            lblCaja.Size = new Size(96, 20);
            lblCaja.TabIndex = 14;
            lblCaja.Text = "Monto Inicial";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(button1);
            groupBox2.Controls.Add(texmontoesperado);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(texobservacioncierre);
            groupBox2.Controls.Add(button2);
            groupBox2.Controls.Add(texmontocontado);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(label5);
            groupBox2.ForeColor = Color.Black;
            groupBox2.Location = new Point(492, 95);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(464, 294);
            groupBox2.TabIndex = 13;
            groupBox2.TabStop = false;
            groupBox2.Text = "CIERRE";
            groupBox2.Enter += groupBox2_Enter;
            // 
            // button1
            // 
            button1.ForeColor = Color.Black;
            button1.Location = new Point(306, 249);
            button1.Name = "button1";
            button1.Size = new Size(94, 29);
            button1.TabIndex = 15;
            button1.Text = "Cerrar";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // texmontoesperado
            // 
            texmontoesperado.Location = new Point(222, 101);
            texmontoesperado.Name = "texmontoesperado";
            texmontoesperado.Size = new Size(178, 27);
            texmontoesperado.TabIndex = 12;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = Color.Black;
            label6.Location = new Point(47, 108);
            label6.Name = "label6";
            label6.Size = new Size(119, 20);
            label6.TabIndex = 13;
            label6.Text = "Monto Esperado";
            // 
            // texobservacioncierre
            // 
            texobservacioncierre.Location = new Point(222, 156);
            texobservacioncierre.Multiline = true;
            texobservacioncierre.Name = "texobservacioncierre";
            texobservacioncierre.Size = new Size(178, 75);
            texobservacioncierre.TabIndex = 9;
            // 
            // button2
            // 
            button2.ForeColor = Color.Black;
            button2.Location = new Point(183, 249);
            button2.Name = "button2";
            button2.Size = new Size(94, 29);
            button2.TabIndex = 11;
            button2.Text = "Ver";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // texmontocontado
            // 
            texmontocontado.Location = new Point(222, 33);
            texmontocontado.Name = "texmontocontado";
            texmontocontado.Size = new Size(178, 27);
            texmontocontado.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = Color.Black;
            label4.Location = new Point(38, 185);
            label4.Name = "label4";
            label4.Size = new Size(153, 20);
            label4.TabIndex = 10;
            label4.Text = "Observacion de cierre";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = Color.Black;
            label5.Location = new Point(47, 40);
            label5.Name = "label5";
            label5.Size = new Size(114, 20);
            label5.TabIndex = 8;
            label5.Text = "Monto Contado";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(texobservacionapertura);
            groupBox1.Controls.Add(butcrearcaja);
            groupBox1.Controls.Add(textmontoinicial);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Location = new Point(22, 95);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(464, 294);
            groupBox1.TabIndex = 12;
            groupBox1.TabStop = false;
            groupBox1.Text = "APERTURA";
            // 
            // texobservacionapertura
            // 
            texobservacionapertura.Location = new Point(222, 79);
            texobservacionapertura.Multiline = true;
            texobservacionapertura.Name = "texobservacionapertura";
            texobservacionapertura.Size = new Size(178, 75);
            texobservacionapertura.TabIndex = 9;
            // 
            // butcrearcaja
            // 
            butcrearcaja.Location = new Point(205, 223);
            butcrearcaja.Name = "butcrearcaja";
            butcrearcaja.Size = new Size(94, 29);
            butcrearcaja.TabIndex = 11;
            butcrearcaja.Text = "Crear";
            butcrearcaja.UseVisualStyleBackColor = true;
            butcrearcaja.Click += button1_Click;
            // 
            // textmontoinicial
            // 
            textmontoinicial.Location = new Point(222, 33);
            textmontoinicial.Name = "textmontoinicial";
            textmontoinicial.Size = new Size(178, 27);
            textmontoinicial.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = Color.Black;
            label3.Location = new Point(38, 108);
            label3.Name = "label3";
            label3.Size = new Size(172, 20);
            label3.TabIndex = 10;
            label3.Text = "Observacion de apertura";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = Color.Black;
            label2.Location = new Point(47, 40);
            label2.Name = "label2";
            label2.Size = new Size(96, 20);
            label2.TabIndex = 8;
            label2.Text = "Monto Inicial";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(202, 41);
            label1.Name = "label1";
            label1.Size = new Size(171, 20);
            label1.TabIndex = 6;
            label1.Text = "Apertura y cierre de caja";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(datasesiones);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(989, 467);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "SESION DE CAJA";
            tabPage2.UseVisualStyleBackColor = true;
            tabPage2.Click += tabPage2_Click;
            // 
            // datasesiones
            // 
            datasesiones.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            datasesiones.Location = new Point(34, 48);
            datasesiones.Name = "datasesiones";
            datasesiones.RowHeadersWidth = 51;
            datasesiones.Size = new Size(921, 379);
            datasesiones.TabIndex = 0;
            // 
            // Caja
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Gray;
            ClientSize = new Size(1021, 568);
            Controls.Add(tabcaja);
            Name = "Caja";
            Text = "usuarioscs";
            Load += usuarioscs_Load;
            tabcaja.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)datasesiones).EndInit();
            ResumeLayout(false);
        }



        #endregion

        private TabControl tabcaja;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Button butcrearcaja;
        private Label label3;
        private TextBox texobservacionapertura;
        private Label label2;
        private TextBox textmontoinicial;
        private Label label1;
        private GroupBox groupBox2;
        private TextBox texobservacioncierre;
        private Button button2;
        private TextBox texmontocontado;
        private Label label4;
        private Label label5;
        private GroupBox groupBox1;
        private TextBox texmontoesperado;
        private Label label6;
        private Label lblCaja;
        private Button button1;
        private Label lblUsuario;
        private DataGridView datasesiones;
    }
}