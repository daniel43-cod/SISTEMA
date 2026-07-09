using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.DTOs.Productos;
using SISTEMA_FROTEND.DTOs.Ventas;
using SISTEMA_FROTEND.forms;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;


namespace SISTEMA_FROTEND.presentacion
{
    public partial class cotizacion : Form
    {
        //datos que se llena en los campos si el usuario confirma la eleccion de algun cliente existente
        private ClienteBuscarDTOs clienteSeleccionadoActual = null;
        public cotizacion()
        {
            InitializeComponent();
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
        }

        //instanciamos el servicio de productos para poder acceder a la lista de productos y sus detalles
        private ProductoService _productoService = new ProductoService();
        //guardamos la lista de productos en memoria para poder acceder a ellos sin tener que hacer otra consulta a la api
        private List<ProductoVentaBuscarDTO> _productos;
        //evita que el metodo autocomplete se ejecute varias veces al mismo tiempo, lo que podria causar errores o resultados inesperados
        private bool cargandoClientes = false;
        //duda
        private List<ClienteBuscarDTOs> clientesEncontrados = new();
        //instanciamos el servicio de clientes para poder acceder a la lista de clientes y sus detalles
        private ClienteService clienteService = new ClienteService();


        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }


        private async void btncotizar_Click(object sender, EventArgs e)
        {
     
        }


        private async void cotizacion_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";
            texsubtotal.Enabled = false;
            texdescuento.Enabled = false;
            textotal.Enabled = false;

            if (!dataGridView1.Columns.Contains("id_producto"))
            {
                dataGridView1.Columns.Add("id_producto", "id_producto");
                dataGridView1.Columns["id_producto"].Visible = false;
            }

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

        //evento que consulta a la api
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
                comCliente.DataSource = clientesEncontrados;

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

        //evento que se dispara cuando se selecciona un cliente de la lista desplegable, llenando los campos correspondientes con la información del cliente seleccionado
        private void comCliente_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comCliente.SelectedItem is ClienteBuscarDTOs cliente)
            {
                clienteSeleccionadoActual = cliente;

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


        //permite seleccionar un producto con el enter y llenar los campos de precio y descuento automaticamente
        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var cliente = clientesEncontrados.FirstOrDefault(c =>
                    c.nombreCompleto.Equals(comCliente.Text.Trim(), StringComparison.OrdinalIgnoreCase));

                if (cliente != null)
                {
                    comCliente.SelectedItem = cliente;
                    LlenarDatosCliente(cliente);
                }

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

        //arma el autocompletado en la celda producto 
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
                dataGridView1.Rows[e.RowIndex].Cells["id_producto"].Value = producto.id_producto;
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

        private void button3_Click(object sender, EventArgs e)
        {

            //se crea el objeto
            VentaDTOs venta = new VentaDTOs
            {
                id_usuario = Sesion.IdUsuario,
                origen = "VENTA",
                detalles = new List<DetalleDTOs>()
            };

            //recorre todas las filas del DataGridView y agrega los detalles de cada producto a la lista de detalles de la venta
            foreach (DataGridViewRow fila in dataGridView1.Rows)
            {
                if (fila.IsNewRow) continue;
                if (fila.Tag == null) continue;

                venta.detalles.Add(new DetalleDTOs
                {
                    id_producto=Convert.ToInt32(fila.Cells["id_producto"].Value),
                    id_producto_presentacion = Convert.ToInt32(fila.Tag),
                    cantidad = Convert.ToInt32(fila.Cells["cantidad"].Value),
                    descuento = Convert.ToDecimal(fila.Cells["descuento"].Value)
                });
            }

            if (venta.detalles.Count == 0)
            {
                MessageBox.Show("Agrega al menos un producto.");
                return;
            }
            ClienteBuscarDTOs cliente1 = null;

            if (comCliente.SelectedItem is ClienteBuscarDTOs clienteSeleccionado)
            {
                cliente1 = clienteSeleccionado;
            }
            else
            {
                cliente1 = clientesEncontrados.FirstOrDefault(c =>
                    c.nombreCompleto.Equals(comCliente.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (clienteSeleccionadoActual == null)
            {
                MessageBox.Show("Selecciona un cliente válido de la lista.");
                return;
            }

            venta.id_cliente = clienteSeleccionadoActual.id_Cliente;
            venta.nombre_cliente = clienteSeleccionadoActual.nombreCompleto;
            venta.clienteNuevo = null;

            formCobro frm = new formCobro(venta, Convert.ToDecimal(textotal.Text));
            frm.ShowDialog();

        }
    }
}