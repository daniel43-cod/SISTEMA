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

        private void creditos_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";
            cargarDatos();

        }

        private async void cargarDatos()
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
