namespace SISTEMA_FROTEND.forms
{
    partial class LOGIN
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
            texusuario = new TextBox();
            texcontraseña = new TextBox();
            labelContraseña = new Label();
            label1 = new Label();
            groupBox1 = new GroupBox();
            button1 = new Button();
            button2 = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // texusuario
            // 
            texusuario.Location = new Point(247, 43);
            texusuario.Name = "texusuario";
            texusuario.Size = new Size(156, 27);
            texusuario.TabIndex = 0;
            texusuario.TextChanged += textBox1_TextChanged;
            // 
            // texcontraseña
            // 
            texcontraseña.Location = new Point(247, 114);
            texcontraseña.Name = "texcontraseña";
            texcontraseña.Size = new Size(156, 27);
            texcontraseña.TabIndex = 1;
            // 
            // labelContraseña
            // 
            labelContraseña.AutoSize = true;
            labelContraseña.Location = new Point(59, 52);
            labelContraseña.Name = "labelContraseña";
            labelContraseña.Size = new Size(77, 20);
            labelContraseña.TabIndex = 2;
            labelContraseña.Text = "USUARIO_";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(59, 121);
            label1.Name = "label1";
            label1.Size = new Size(107, 20);
            label1.TabIndex = 3;
            label1.Text = "CONTRASEÑA:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(texcontraseña);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(texusuario);
            groupBox1.Controls.Add(labelContraseña);
            groupBox1.Location = new Point(223, 97);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(476, 213);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "INICIO DE SESION";
            // 
            // button1
            // 
            button1.Location = new Point(489, 350);
            button1.Name = "button1";
            button1.Size = new Size(137, 63);
            button1.TabIndex = 5;
            button1.Text = "INICIAR SESION";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(292, 350);
            button2.Name = "button2";
            button2.Size = new Size(137, 63);
            button2.TabIndex = 6;
            button2.Text = "CANCELAR";
            button2.UseVisualStyleBackColor = true;
            // 
            // LOGIN
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1064, 494);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(groupBox1);
            Name = "LOGIN";
            Text = "LOGIN";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TextBox texusuario;
        private TextBox texcontraseña;
        private Label labelContraseña;
        private Label label1;
        private GroupBox groupBox1;
        private Button button1;
        private Button button2;
    }
}