using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.DTOs.Productos;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;


namespace SISTEMA_FROTEND.presentacion
{
    public partial class cotizacion : Form
    {
        private TextBox comboProductoActual;
        private bool cargandoProductoGrid = false;
        private System.Windows.Forms.Timer _debounceTimer;
        public cotizacion()
        {
            InitializeComponent();
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
        }

        private ProductoService _productoService = new ProductoService();
        private List<ProductoVentaBuscarDTO> _productos;
        private bool cargandoClientes = false;
        private List<ClienteBuscarDTOs> clientesEncontrados = new();
        private ClienteService clienteService = new ClienteService();


        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btncotizar_Click(object sender, EventArgs e)
        {

        }


        private async void cotizacion_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";
            texsubtotal.Enabled = false ;
            texdescuento.Enabled = false ;
            textotal.Enabled = false;

            comCliente.DropDownStyle = ComboBoxStyle.DropDown;
            comCliente.AutoCompleteMode = AutoCompleteMode.None;
            comCliente.AutoCompleteSource = AutoCompleteSource.None;

            texapellido.Visible = false;
            labapellido.Visible = false;

            _productos = await _productoService.ListarProducto();

            var colProducto = (DataGridViewTextBoxColumn)dataGridView1.Columns["producto"];

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }


        private async void comCliente_SelectedIndexChanged(object sender, EventArgs e)
        {
        }


        private async void comCliente_TextChanged(object sender, EventArgs e)
        {
            if (cargandoClientes)
                return;

            string texto = comCliente.Text;

            if (texto.Length < 3)
                return;

            cargandoClientes = true;

            try
            {
                comCliente.DroppedDown = false;

                clientesEncontrados = await clienteService.BuscarClientes(texto);

                comCliente.DataSource = null;
                comCliente.DisplayMember = "nombreCompleto";
                comCliente.ValueMember = "id_cliente";
                comCliente.DataSource = clientesEncontrados;

                comCliente.Text = texto;

                if (texto.Length <= comCliente.Text.Length)
                    comCliente.SelectionStart = texto.Length;

                if (clientesEncontrados.Count > 0)
                    comCliente.DroppedDown = true;
            }
            finally
            {
                cargandoClientes = false;
            }

        }

        private void comCliente_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comCliente.SelectedItem is ClienteBuscarDTOs cliente)
            {
                //  texapellido.Text = cliente.apellido;
                texnit.Text = cliente.nit ?? "";
                textelefono.Text = cliente.telefono ?? "";
                texcorreo.Text = cliente.correo_electronico ?? "";
                texdireccion.Text = cliente.direccion ?? "";
                texdpi.Text = cliente.dpi ?? "";
            }
        }


        private void limpiar()
        {
            textelefono.Text = "";
            comCliente.Text = "";
            texapellido.Text = "";
            texnit.Text = "";
            texdpi.Text = "";
            texdireccion.Text = "";
            texcorreo.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            limpiar();
        }

        private void button2_HelpRequested(object sender, HelpEventArgs hlpevent)
        {

        }

        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && comCliente.SelectedItem is ClienteBuscarDTOs cliente)
            {
                LlenarDatosCliente(cliente);
                e.Handled = true;
            }
        }

        private void LlenarDatosCliente(ClienteBuscarDTOs cliente)
        {
            texnit.Text = cliente.nit ?? "";
            textelefono.Text = cliente.telefono ?? "";
            texcorreo.Text = cliente.correo_electronico ?? "";
            texdireccion.Text = cliente.direccion ?? "";
            texdpi.Text = cliente.dpi ?? "";
        }


        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dataGridView1.CurrentCell.OwningColumn.Name == "producto")
            {
                var textBox = e.Control as TextBox;

                if (textBox != null)
                {
                    textBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                    textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;

                    var fuente = new AutoCompleteStringCollection();
                    fuente.AddRange(_productos.Select(p => p.nombreMostrar).ToArray());
                    textBox.AutoCompleteCustomSource = fuente;
                }
            }
        }

        //calcula el subtotal 
        private void CalcularSubtotal(int rowIndex)
        {
            var fila = dataGridView1.Rows[rowIndex];

            decimal cantidad = Convert.ToDecimal(fila.Cells["cantidad"].Value ?? 0);
            decimal precio = Convert.ToDecimal(fila.Cells["precio"].Value ?? 0);
            decimal descuento = Convert.ToDecimal(fila.Cells["descuento"].Value ?? 0);

            fila.Cells["subtotal"].Value = (cantidad * precio) - descuento;

            RecalcularTotales();
        }


        //evento que se dispara cuando se termina de editar una celda en el DataGridView
        //traendo consigo el toda la informaciopn del producto seleccionado y calculando el subtotal
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            //everigua el nombde del producto seleccionado o editado

            string columna = dataGridView1.Columns[e.ColumnIndex].Name;
            //si la columna editada es la de producto, busca el producto en la lista de productos y llena los campos correspondientes
            if (columna == "producto")
            {
                var fila = dataGridView1.Rows[e.RowIndex];
                //toma el texto o producto elejido 
                string textoElegido = dataGridView1.Rows[e.RowIndex].Cells["producto"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(textoElegido)) return;
                //busca que el producto conicida con algunos de los productos en la lista de productos o almacenados en memoria 
                var producto = _productos.FirstOrDefault(p =>
                    p.nombreMostrar.Trim().Equals(textoElegido.Trim(), StringComparison.OrdinalIgnoreCase));

                if (producto == null) return;
                // se llena automaticamente los campos de precio y descuento con los valores del producto encontrado
                dataGridView1.Rows[e.RowIndex].Cells["stock"].Value = producto.stock;
                dataGridView1.Rows[e.RowIndex].Cells["precio"].Value = producto.precio;
                // Solo poner 1 por defecto si la celda de cantidad está vacía
                if (fila.Cells["cantidad"].Value == null || string.IsNullOrWhiteSpace(fila.Cells["cantidad"].Value.ToString()))
                {
                    fila.Cells["cantidad"].Value = 1;
                }

                // Mismo criterio para descuento
                if (fila.Cells["descuento"].Value == null || string.IsNullOrWhiteSpace(fila.Cells["descuento"].Value.ToString()))
                {
                    fila.Cells["descuento"].Value = 0;
                }

                dataGridView1.Rows[e.RowIndex].Tag = producto.id_producto_presentacion;

                CalcularSubtotal(e.RowIndex);
            }

            if (columna == "cantidad" || columna == "descuento")
            {
                CalcularSubtotal(e.RowIndex);
            }
        }

        private void RecalcularTotales()
        {
            decimal subtotalGeneral = 0;
            decimal descuentoGeneral = 0;

            foreach (DataGridViewRow fila in dataGridView1.Rows)
            {
                if (fila.IsNewRow) continue; // saltar la fila fantasma vacía
                if (fila.Tag == null) continue; // saltar filas sin producto válido

                decimal cantidad = Convert.ToDecimal(fila.Cells["cantidad"].Value ?? 0);
                decimal precio = Convert.ToDecimal(fila.Cells["precio"].Value ?? 0);
                decimal descuento = Convert.ToDecimal(fila.Cells["descuento"].Value ?? 0);

                subtotalGeneral += cantidad * precio;
                descuentoGeneral += descuento;
            }

            decimal totalGeneral = subtotalGeneral - descuentoGeneral;

            texsubtotal.Text = subtotalGeneral.ToString("N2");
            texdescuento.Text = descuentoGeneral.ToString("N2");
            textotal.Text = totalGeneral.ToString("N2");
        }


    }
}
