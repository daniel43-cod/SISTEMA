using SISTEMA_FROTEND.forms;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.services;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SISTEMA_FROTEND.presentacion
{
    public partial class RegistroVenta : Form
    {
        private readonly VentaService _service = new VentaService();

        public RegistroVenta()
        {
            InitializeComponent();
        }

        private async void RegistroVenta_load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";

            var ventas = await _service.ListarVentas();
            dataregistrodiario.DataSource = ventas;

            if (dataregistrodiario.Columns["id_ventas"] != null)
                dataregistrodiario.Columns["id_ventas"].Visible = false;

            if (dataregistrodiario.Columns["id_cliente"] != null)
                dataregistrodiario.Columns["id_cliente"].Visible = false;

            if (dataregistrodiario.Columns["id_usuario"] != null)
                dataregistrodiario.Columns["id_usuario"].Visible = false;

            if (!dataregistrodiario.Columns.Contains("Detalle"))
            {
                DataGridViewButtonColumn btnDetalle = new DataGridViewButtonColumn
                {
                    Name = "Detalle",
                    HeaderText = "Ver detalle",
                    Text = "Ver",
                    UseColumnTextForButtonValue = true
                };

                dataregistrodiario.Columns.Add(btnDetalle);
            }

            dataregistrodiario.DefaultCellStyle.ForeColor = Color.Black;
            dataregistrodiario.DefaultCellStyle.BackColor = Color.White;
            dataregistrodiario.DefaultCellStyle.SelectionForeColor = Color.White;
            dataregistrodiario.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;

            dataregistrodiario.EnableHeadersVisualStyles = false;
            dataregistrodiario.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataregistrodiario.ColumnHeadersDefaultCellStyle.BackColor =
                Color.DarkSlateGray;
        }

        private async Task CargarProductos()
        {
            var ventas = await _service.ListarVentas();
            dataregistrodiario.DataSource = ventas;
        }

        private void dataregistrodiario_CellContentClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (dataregistrodiario.Columns[e.ColumnIndex].Name != "Detalle")
                return;

            int idVenta = Convert.ToInt32(
                dataregistrodiario.Rows[e.RowIndex]
                    .Cells["id_ventas"]
                    .Value
            );

            frmDetalleVenta frm = new frmDetalleVenta(idVenta);
            frm.ShowDialog();
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }
    }
}