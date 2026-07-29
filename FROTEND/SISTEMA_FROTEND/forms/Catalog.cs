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
    public partial class Catalog : Form
    {

        private VentaService _ventaService = new VentaService();

        public Catalog()
        {
            InitializeComponent();
        }


        private async void Catalog_Load(object sender, EventArgs e)
        {
            await CargarCatalogo();
        }


        private async Task CargarCatalogo()
        {
            try
            {
                var productos = await _ventaService.ListarCatalogo();

                flowCatalogo.Controls.Clear();

                foreach (var producto in productos)
                {
                    var tarjeta = new Catalogo();

                    tarjeta.CargarProducto(producto);

                    flowCatalogo.Controls.Add(tarjeta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cargar catálogo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void flowCatalogo_Paint(object sender, PaintEventArgs e)
        {

        }
    }








}



