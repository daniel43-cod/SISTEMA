namespace SISTEMA_FROTEND.forms
{
    partial class frmproductos
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
            dataProductos = new DataGridView();
            label1 = new Label();
            label2 = new Label();
            combuscar = new ComboBox();
            comcategoria = new ComboBox();
            lblUsuario = new Label();
            ((System.ComponentModel.ISupportInitialize)dataProductos).BeginInit();
            SuspendLayout();
            // 
            // dataProductos
            // 
            dataProductos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataProductos.Location = new Point(26, 200);
            dataProductos.Name = "dataProductos";
            dataProductos.RowHeadersWidth = 51;
            dataProductos.Size = new Size(1339, 425);
            dataProductos.TabIndex = 0;
            dataProductos.CellContentClick += dataGridView1_CellContentClick;
            dataProductos.SelectionChanged += dataProductos_SelectionChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(78, 96);
            label1.Name = "label1";
            label1.Size = new Size(122, 20);
            label1.TabIndex = 1;
            label1.Text = "Buscar Productos";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(454, 96);
            label2.Name = "label2";
            label2.Size = new Size(124, 20);
            label2.TabIndex = 4;
            label2.Text = "Ver Por Categoria";
            // 
            // combuscar
            // 
            combuscar.FormattingEnabled = true;
            combuscar.Location = new Point(205, 88);
            combuscar.Name = "combuscar";
            combuscar.Size = new Size(151, 28);
            combuscar.TabIndex = 5;
            combuscar.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            combuscar.TextChanged += combuscar_TextChanged;
            // 
            // comcategoria
            // 
            comcategoria.FormattingEnabled = true;
            comcategoria.Location = new Point(590, 88);
            comcategoria.Name = "comcategoria";
            comcategoria.Size = new Size(151, 28);
            comcategoria.TabIndex = 6;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(832, 43);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(71, 20);
            lblUsuario.TabIndex = 15;
            lblUsuario.Text = "USUARIO";
            // 
            // frmproductos
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1554, 716);
            Controls.Add(lblUsuario);
            Controls.Add(comcategoria);
            Controls.Add(combuscar);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(dataProductos);
            Name = "frmproductos";
            Text = "frmproductos";
            Load += frmproductos_Load;
            ((System.ComponentModel.ISupportInitialize)dataProductos).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataProductos;
        private Label label1;
        private Label label2;
        private ComboBox combuscar;
        private ComboBox comcategoria;
        private Label lblUsuario;
    }
}