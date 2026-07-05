namespace SISTEMA_FROTEND.presentacion
{
    partial class cotizacion
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
            button1 = new Button();
            groupBox1 = new GroupBox();
            comCliente = new ComboBox();
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
            labapellido = new Label();
            texapellido = new TextBox();
            label1 = new Label();
            dataGridView1 = new DataGridView();
            btncotizar = new Button();
            button2 = new Button();
            lblUsuario = new Label();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.BackColor = Color.Red;
            button1.Location = new Point(920, 664);
            button1.Name = "button1";
            button1.Size = new Size(158, 42);
            button1.TabIndex = 0;
            button1.Text = "SALIR";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(comCliente);
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
            groupBox1.Controls.Add(labapellido);
            groupBox1.Controls.Add(texapellido);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(205, 23);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(873, 267);
            groupBox1.TabIndex = 11;
            groupBox1.TabStop = false;
            groupBox1.Text = "Datos del Cliente";
            groupBox1.Enter += groupBox1_Enter;
            // 
            // comCliente
            // 
            comCliente.FormattingEnabled = true;
            comCliente.Location = new Point(177, 36);
            comCliente.Name = "comCliente";
            comCliente.Size = new Size(151, 28);
            comCliente.TabIndex = 15;
            comCliente.SelectedIndexChanged += comCliente_SelectedIndexChanged;
            comCliente.SelectionChangeCommitted += comCliente_SelectionChangeCommitted;
            comCliente.TextChanged += comCliente_TextChanged;
            // 
            // labtelefono
            // 
            labtelefono.AutoSize = true;
            labtelefono.Location = new Point(96, 165);
            labtelefono.Name = "labtelefono";
            labtelefono.Size = new Size(67, 20);
            labtelefono.TabIndex = 14;
            labtelefono.Text = "Telefono";
            // 
            // textelefono
            // 
            textelefono.Location = new Point(177, 162);
            textelefono.Name = "textelefono";
            textelefono.Size = new Size(151, 27);
            textelefono.TabIndex = 13;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(96, 119);
            label6.Name = "label6";
            label6.Size = new Size(72, 20);
            label6.TabIndex = 12;
            label6.Text = "Direccion";
            // 
            // texdireccion
            // 
            texdireccion.Location = new Point(177, 116);
            texdireccion.Name = "texdireccion";
            texdireccion.Size = new Size(151, 27);
            texdireccion.TabIndex = 11;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(430, 123);
            label5.Name = "label5";
            label5.Size = new Size(54, 20);
            label5.TabIndex = 10;
            label5.Text = "Correo";
            // 
            // texcorreo
            // 
            texcorreo.Location = new Point(511, 120);
            texcorreo.Name = "texcorreo";
            texcorreo.Size = new Size(125, 27);
            texcorreo.TabIndex = 9;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(430, 79);
            label4.Name = "label4";
            label4.Size = new Size(32, 20);
            label4.TabIndex = 8;
            label4.Text = "DPI";
            // 
            // texdpi
            // 
            texdpi.Location = new Point(511, 76);
            texdpi.Name = "texdpi";
            texdpi.Size = new Size(125, 27);
            texdpi.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(430, 36);
            label3.Name = "label3";
            label3.Size = new Size(32, 20);
            label3.TabIndex = 6;
            label3.Text = "NIT";
            // 
            // texnit
            // 
            texnit.Location = new Point(511, 33);
            texnit.Name = "texnit";
            texnit.Size = new Size(125, 27);
            texnit.TabIndex = 5;
            // 
            // labapellido
            // 
            labapellido.AutoSize = true;
            labapellido.Location = new Point(96, 76);
            labapellido.Name = "labapellido";
            labapellido.Size = new Size(66, 20);
            labapellido.TabIndex = 4;
            labapellido.Text = "Apellido";
            // 
            // texapellido
            // 
            texapellido.Location = new Point(177, 73);
            texapellido.Name = "texapellido";
            texapellido.Size = new Size(151, 27);
            texapellido.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(96, 33);
            label1.Name = "label1";
            label1.Size = new Size(64, 20);
            label1.TabIndex = 2;
            label1.Text = "Nombre";
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(205, 333);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 51;
            dataGridView1.Size = new Size(873, 287);
            dataGridView1.TabIndex = 10;
            // 
            // btncotizar
            // 
            btncotizar.BackColor = Color.Green;
            btncotizar.Location = new Point(205, 664);
            btncotizar.Name = "btncotizar";
            btncotizar.Size = new Size(158, 42);
            btncotizar.TabIndex = 12;
            btncotizar.Text = "COTIZAR";
            btncotizar.UseVisualStyleBackColor = false;
            btncotizar.Click += btncotizar_Click;
            // 
            // button2
            // 
            button2.BackColor = Color.Green;
            button2.Location = new Point(569, 664);
            button2.Name = "button2";
            button2.Size = new Size(158, 42);
            button2.TabIndex = 13;
            button2.Text = "CANCELAR";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            button2.HelpRequested += button2_HelpRequested;
            button2.KeyDown += button2_KeyDown;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(1280, 53);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(71, 20);
            lblUsuario.TabIndex = 14;
            lblUsuario.Text = "USUARIO";
            // 
            // cotizacion
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1438, 822);
            Controls.Add(lblUsuario);
            Controls.Add(button2);
            Controls.Add(btncotizar);
            Controls.Add(groupBox1);
            Controls.Add(dataGridView1);
            Controls.Add(button1);
            Name = "cotizacion";
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
        private Label labapellido;
        private TextBox texapellido;
        private Label label1;
        private DataGridView dataGridView1;
        private Button btncotizar;
        private Button button2;
        private Label labtelefono;
        private TextBox textelefono;
        private Label lblUsuario;
        private ComboBox comCliente;
    }
}