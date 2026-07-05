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
    public partial class frmregistro_ventas : Form
    {
        public frmregistro_ventas()
        {
            InitializeComponent();
        }



        VentaService _service = new VentaService();
        private async void frmregistro_ventas_Load(object sender, EventArgs e)
        {
            CargarProductos();
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";

            var ventas = await _service.ListarVentas();
            dataregistrodiario.DataSource = ventas;
            dataregistrodiario.Columns["id_ventas"].Visible = false; // aquí, una sola vez
            dataregistrodiario.Columns["id_cliente"].Visible = false; // aquí, una sola vez
            //dataregistrodiario.Columns["id_producto"].Visible = false; // aquí, una sola vez

            //boton para ver detalle.
            DataGridViewButtonColumn btnDetalle = new DataGridViewButtonColumn();
            btnDetalle.Name = "Detalle";
            btnDetalle.HeaderText = "Ver detalle";
            btnDetalle.Text = "Ver";
            btnDetalle.UseColumnTextForButtonValue = true;

            dataregistrodiario.Columns.Add(btnDetalle);

        }

        //metodo para cargar las ventas del dia
        private async Task CargarProductos()
        {
            VentaService service = new VentaService();

            var venta = await service.ListarVentas();

            dataregistrodiario.DataSource = venta;
        }

        //funcion para el boton ver detalles
        private void dataregistrodiario_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 &&
        dataregistrodiario.Columns[e.ColumnIndex].Name == "Detalle")

            {
                int idVenta = Convert.ToInt32(
                    dataregistrodiario.Rows[e.RowIndex].Cells["id_ventas"].Value);

                frmDetalleVenta frm = new frmDetalleVenta(idVenta);
                frm.ShowDialog();
            }
        }
    }
}
