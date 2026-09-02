using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.DTOs.Ventas;
using SISTEMA_FROTEND.forms;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.services;
using System.Globalization;


namespace SISTEMA_FROTEND.presentacion
{
    public partial class RegistroVenta : Form
    {
        private readonly VentaService _service = new VentaService();
        private List<ListarClienteDTOs> _clientes = new();
        private List<ListarVentasDTOs> _ventas = new();

        public RegistroVenta()
        {
            InitializeComponent();
            ComClientes.DropDownStyle = ComboBoxStyle.DropDown;
            ComClientes.TextUpdate += ComClientes_TextUpdate;
            dataregistrodiario.CellContentClick += dataregistrodiario_CellContentClick;
        }

        private async void RegistroVenta_load(object sender, EventArgs e)
        {

            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";
            await CargarClientes();

            /* var ventas = await _service.ListarVentasCajaActiva();
             dataregistrodiario.DataSource = ventas;*/
            _ventas = await _service.ListarVentasCajaActiva();

            dataregistrodiario.DataSource = _ventas;

            OcultarColumnas();

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

            textotalvendido.Text = "Q. 0.00";
            textotalvendido.Enabled = false;
            textotalventas.Enabled = false;

            CalcularTotalVentas();
            TotalVentas();
        }

        private void OcultarColumnas()
        {
            dataregistrodiario.Columns["id_ventas"].Visible = false;
            dataregistrodiario.Columns["id_cliente"].Visible = false;
            dataregistrodiario.Columns["impuesto"].Visible = false;
            dataregistrodiario.Columns["id_usuario"].Visible = false;
            dataregistrodiario.Columns["origen"].Visible = false;
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

        private void ComClientes_TextUpdate(object sender, EventArgs e)
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

        private async void ComClientes_SelectionChangeCommitted(object sender, EventArgs e)
        {
           
            if (ComClientes.SelectedItem is not ListarClienteDTOs cliente)
                return;

            var ventasFiltradas = _ventas
                .Where(v => v.id_cliente == cliente.id_Cliente)
                .ToList();

            dataregistrodiario.DataSource = null;
            dataregistrodiario.DataSource = ventasFiltradas;

            OcultarColumnas();
        }


        private void CalcularTotalVentas()
        {
            decimal totalVentas = 0;
            foreach (DataGridViewRow row in dataregistrodiario.Rows)
            {
                if (row.Cells["total"].Value != null)
                {
                    if (decimal.TryParse(row.Cells["total"].Value.ToString(), out decimal total))
                    {
                        totalVentas += total;
                    }
                }
            }
            textotalvendido.Text = totalVentas.ToString( "C2", new CultureInfo("es-GT"));
        }

        private void TotalVentas()
        {
            int totalVentas = 0;
            foreach (DataGridViewRow row in dataregistrodiario.Rows)
            {
                if (!row.IsNewRow)
                {
                    totalVentas++;
                }
            }

            textotalventas.Text=totalVentas.ToString();
        }

        private void guna2TextBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}