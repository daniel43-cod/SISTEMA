namespace SISTEMA_FROTEND.forms
{
    partial class Catalog
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
            flowCatalogo = new FlowLayoutPanel();
            label1 = new Label();
            label2 = new Label();
            textBox1 = new TextBox();
            label3 = new Label();
            comboCategorias = new ComboBox();
            SuspendLayout();
            // 
            // flowCatalogo
            // 
            flowCatalogo.AutoScroll = true;
            flowCatalogo.Location = new Point(12, 152);
            flowCatalogo.Name = "flowCatalogo";
            flowCatalogo.Size = new Size(1535, 659);
            flowCatalogo.TabIndex = 0;
            flowCatalogo.Paint += flowCatalogo_Paint;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(683, 20);
            label1.Name = "label1";
            label1.Size = new Size(193, 20);
            label1.TabIndex = 1;
            label1.Text = "CATALOGO DE PRODUCTOS";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(79, 78);
            label2.Name = "label2";
            label2.Size = new Size(147, 20);
            label2.TabIndex = 2;
            label2.Text = "BUSCAR PRODUCTO:";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(232, 71);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(314, 27);
            textBox1.TabIndex = 3;
            textBox1.TextChanged += textBox1_TextChanged;
            textBox1.KeyDown += textBox1_KeyDown;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(649, 78);
            label3.Name = "label3";
            label3.Size = new Size(149, 20);
            label3.TabIndex = 4;
            label3.Text = "VER POR CATEGORIA";
            // 
            // comboCategorias
            // 
            comboCategorias.FormattingEnabled = true;
            comboCategorias.Location = new Point(851, 70);
            comboCategorias.Name = "comboCategorias";
            comboCategorias.Size = new Size(344, 28);
            comboCategorias.TabIndex = 5;
            comboCategorias.SelectedIndexChanged += comboCategorias_SelectedIndexChanged;
            // 
            // Catalog
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1559, 823);
            Controls.Add(comboCategorias);
            Controls.Add(label3);
            Controls.Add(textBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(flowCatalogo);
            Name = "Catalog";
            Text = "Form1";
            Load += Catalog_Load;
            ResumeLayout(false);
            PerformLayout();
        }





        #endregion

        private FlowLayoutPanel flowCatalogo;
        private Label label1;
        private Label label2;
        private TextBox textBox1;
        private Label label3;
        private ComboBox comboCategorias;
    }
}