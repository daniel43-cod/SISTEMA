using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.DTOs.Ventas;
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
        private List<ListarClienteDTOs> _clientes = new();

        public RegistroVenta()
        {
            InitializeComponent();
            ComClientes.DropDownStyle = ComboBoxStyle.DropDown;
            ComClientes.TextUpdate += ComClientes_TextUpdate;
        }

        private async void RegistroVenta_load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";
            await CargarClientes();

            var ventas = await _service.ListarVentasCajaActiva();
            dataregistrodiario.DataSource = ventas;

            if (dataregistrodiario.Columns["id_ventas"] != null)
                dataregistrodiario.Columns["id_ventas"].Visible = false;

            if (dataregistrodiario.Columns["id_cliente"] != null)
                dataregistrodiario.Columns["id_cliente"].Visible = false;

            if (dataregistrodiario.Columns["id_usuario"] != null)
                dataregistrodiario.Columns["id_usuario"].Visible = false;
            dataregistrodiario.Columns["origen"].Visible = false;
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

           
        }

        private async Task CargarClientes()
        {
            try
            {
                _clientes = await new ClienteService().ListarClientes();
                ComClientes.DataSource = _clientes;
                ComClientes.DisplayMember = "nombre_completo";
                ComClientes.ValueMember = "id_cliente";

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar clientes: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
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

        private void dataregistrodiario_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ComClientes_TextUpdate(object sender,EventArgs e)
        {
            string texto = ComClientes.Text.Trim();

            int posicionCursor =
                ComClientes.SelectionStart;

            var filtrados = _clientes
                .Where(c =>
                    $"{c.nombre} {c.apellido}"
                        .Contains(
                            texto,
                            StringComparison.OrdinalIgnoreCase
                        )
                    ||
                    (c.nit != null &&
                     c.nit.Contains(
                         texto,
                         StringComparison.OrdinalIgnoreCase
                     ))
                )
                .ToList();

            ComClientes.DataSource = null;

            ComClientes.DisplayMember = "nombre_completo";
            ComClientes.ValueMember = "id_Cliente";

            ComClientes.DataSource = filtrados;

            ComClientes.Text = texto;

            ComClientes.SelectionStart =
                posicionCursor;

            ComClientes.SelectionLength = 0;

            ComClientes.DroppedDown = true;
        }
    }
}