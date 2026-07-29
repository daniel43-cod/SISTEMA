using SISTEMA_FROTEND.forms;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SISTEMA_FROTEND.presentacion
{
    public partial class RegistroCompras : Form
    {
        private readonly CompraService _compraService = new CompraService();
        public RegistroCompras()
        {
            InitializeComponent();
            dataGridView1.CellContentClick +=dataGridView1_CellContentClick;
        }

        private async void creditos_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";

            await cargarDatos();
            columnasocultas();
            AgregarBotonDetalle();



            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkSlateGray;

        }
        private void columnasocultas()
        {
            dataGridView1.Columns["id_compra"].Visible = false;
            dataGridView1.Columns["id_usuario"].Visible = false;
            dataGridView1.Columns["id_empresa"].Visible = false;
            dataGridView1.Columns["id_estado_compra"].Visible = false;
          
        }
        private async Task cargarDatos()
        {
            try
            {
                var compras = await _compraService.ListarCompras();

                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.DataSource = compras;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al listar compras",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void AgregarBotonDetalle()
        {
            if (dataGridView1.Columns["Detalle"] != null)
                return;

            DataGridViewButtonColumn botonDetalle =
                new DataGridViewButtonColumn();

            botonDetalle.Name = "Detalle";
            botonDetalle.HeaderText = "Acción";
            botonDetalle.Text = "Ver detalle";
            botonDetalle.UseColumnTextForButtonValue = true;

            dataGridView1.Columns.Add(botonDetalle);
        }


        private void dataGridView1_CellContentClick(
    object sender,
    DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (dataGridView1.Columns[e.ColumnIndex].Name == "Detalle")
            {
                int idCompra = Convert.ToInt32(
                    dataGridView1.Rows[e.RowIndex]
                        .Cells["id_compra"]
                        .Value);

                formdetallecompra frm =
                    new formdetallecompra(idCompra);

                frm.ShowDialog();
            }
        }
    }
}
