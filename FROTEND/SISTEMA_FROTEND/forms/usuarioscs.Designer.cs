namespace SISTEMA_FROTEND
{
    partial class usuarioscs
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
            lblCaja = new Label();
            groupBox2 = new GroupBox();
            button3 = new Button();
            textBox5 = new TextBox();
            label6 = new Label();
            textBox3 = new TextBox();
            button2 = new Button();
            textBox4 = new TextBox();
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
            tabcaja.SuspendLayout();
            tabPage1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
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
            // 
            // tabPage1
            // 
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
            groupBox2.Controls.Add(button3);
            groupBox2.Controls.Add(textBox5);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(textBox3);
            groupBox2.Controls.Add(button2);
            groupBox2.Controls.Add(textBox4);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(label5);
            groupBox2.Location = new Point(492, 95);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(464, 294);
            groupBox2.TabIndex = 13;
            groupBox2.TabStop = false;
            groupBox2.Text = "CIERRE";
            // 
            // button3
            // 
            button3.Location = new Point(323, 249);
            button3.Name = "button3";
            button3.Size = new Size(94, 29);
            button3.TabIndex = 14;
            button3.Text = "Cerrar";
            button3.UseVisualStyleBackColor = true;
            // 
            // textBox5
            // 
            textBox5.Location = new Point(222, 101);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(178, 27);
            textBox5.TabIndex = 12;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(47, 108);
            label6.Name = "label6";
            label6.Size = new Size(119, 20);
            label6.TabIndex = 13;
            label6.Text = "Monto Esperado";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(222, 156);
            textBox3.Multiline = true;
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(178, 75);
            textBox3.TabIndex = 9;
            // 
            // button2
            // 
            button2.Location = new Point(183, 249);
            button2.Name = "button2";
            button2.Size = new Size(94, 29);
            button2.TabIndex = 11;
            button2.Text = "Ver";
            button2.UseVisualStyleBackColor = true;
            // 
            // textBox4
            // 
            textBox4.Location = new Point(222, 33);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(178, 27);
            textBox4.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(38, 185);
            label4.Name = "label4";
            label4.Size = new Size(153, 20);
            label4.TabIndex = 10;
            label4.Text = "Observacion de cierre";
            // 
            // label5
            // 
            label5.AutoSize = true;
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
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(989, 467);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            tabPage2.Click += tabPage2_Click;
            // 
            // usuarioscs
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Gray;
            ClientSize = new Size(1021, 568);
            Controls.Add(tabcaja);
            Name = "usuarioscs";
            Text = "usuarioscs";
            Load += usuarioscs_Load;
            tabcaja.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
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
        private TextBox textBox3;
        private Button button2;
        private TextBox textBox4;
        private Label label4;
        private Label label5;
        private GroupBox groupBox1;
        private Button button3;
        private TextBox textBox5;
        private Label label6;
        private Label lblCaja;
    }
}