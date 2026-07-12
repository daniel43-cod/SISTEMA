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
    public partial class creditos : Form
    {
        private readonly CompraService _compraService = new CompraService();
        public creditos()
        {
            InitializeComponent();
        }

        private async Task creditos_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";

            await cargarDatos();
            columnasocultas();


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
    }
}
