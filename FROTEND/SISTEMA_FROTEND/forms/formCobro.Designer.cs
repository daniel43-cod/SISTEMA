namespace SISTEMA_FROTEND.forms
{
    partial class formCobro
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
            label1 = new Label();
            textotal = new TextBox();
            label2 = new Label();
            texefectivo = new TextBox();
            butcobrar = new Button();
            label5 = new Label();
            texobservacion = new TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(293, 166);
            label1.Name = "label1";
            label1.Size = new Size(50, 20);
            label1.TabIndex = 0;
            label1.Text = "TOTAL";
            // 
            // textotal
            // 
            textotal.Location = new Point(359, 162);
            textotal.Name = "textotal";
            textotal.Size = new Size(125, 27);
            textotal.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(270, 232);
            label2.Name = "label2";
            label2.Size = new Size(73, 20);
            label2.TabIndex = 2;
            label2.Text = "EFECTIVO";
            // 
            // texefectivo
            // 
            texefectivo.Location = new Point(359, 227);
            texefectivo.Name = "texefectivo";
            texefectivo.Size = new Size(125, 27);
            texefectivo.TabIndex = 3;
            // 
            // butcobrar
            // 
            butcobrar.Location = new Point(309, 479);
            butcobrar.Name = "butcobrar";
            butcobrar.Size = new Size(197, 44);
            butcobrar.TabIndex = 8;
            butcobrar.Text = "COBRAR";
            butcobrar.UseVisualStyleBackColor = true;
            butcobrar.Click += butcobrar_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(635, 136);
            label5.Name = "label5";
            label5.Size = new Size(107, 20);
            label5.TabIndex = 9;
            label5.Text = "OBSERVACION";
            // 
            // texobservacion
            // 
            texobservacion.Location = new Point(640, 161);
            texobservacion.Name = "texobservacion";
            texobservacion.Size = new Size(125, 27);
            texobservacion.TabIndex = 10;
            // 
            // formCobro
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(925, 608);
            Controls.Add(texobservacion);
            Controls.Add(label5);
            Controls.Add(butcobrar);
            Controls.Add(texefectivo);
            Controls.Add(label2);
            Controls.Add(textotal);
            Controls.Add(label1);
            Name = "formCobro";
            Text = "formCobro";
            Load += formCobro_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textotal;
        private Label label2;
        private TextBox texefectivo;
        private Button butcobrar;
        private Label label5;
        private TextBox texobservacion;
    }
}