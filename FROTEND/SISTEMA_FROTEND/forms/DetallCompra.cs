using SISTEMA_FROTEND.services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SISTEMA_FROTEND.forms
{
    public partial class formdetallecompra : Form
    {

        private readonly int _idCompra;
        private readonly CompraService _compraService =
            new CompraService();
        public formdetallecompra(int idCompra)
        {
            InitializeComponent();
            _idCompra = idCompra;
        }

        private async void formdetallecompra_Load(object sender, EventArgs e)
        {
            await cargardatos();
            ocultarcolumnas();
            ConfiguracionColumnas();


            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.BackColor = Color.White;

            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkSlateGray;


        }

        private void ocultarcolumnas()
        {
            dataGridView1.Columns["id_detalle_compra"].Visible = false;
            dataGridView1.Columns["id_registro_compra"].Visible = false;
            dataGridView1.Columns["id_producto"].Visible = false;
            dataGridView1.Columns["subtotal"].Visible = false;
        }




        private void ConfiguracionColumnas()
        {
            dataGridView1.Columns["nombre_producto"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns["nombre_producto"].HeaderText = "PRODUCTO";
            dataGridView1.Columns["cantidad"].Width = 100;
            dataGridView1.Columns["cantidad"].HeaderText = "CANTIDAD";
            dataGridView1.Columns["precio"].Width = 120;
            dataGridView1.Columns["precio"].HeaderText = "SUBTOTAL";
        }
        private async Task cargardatos()
        {

            try
            {
                var detalles = await _compraService.ListarDetalleCompras(_idCompra);

                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.DataSource = detalles;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cargar el detalle",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

        }
    }
}
