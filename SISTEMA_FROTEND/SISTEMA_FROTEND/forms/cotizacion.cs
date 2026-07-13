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


        private ClienteService _clienteService = new ClienteService();
        private ListarClienteDTOs _clienteSeleccionado;
        private List<ListarClienteDTOs> _clientes;


        //datos que se llena en los campos si el usuario confirma la eleccion de algun cliente existente
        private ClienteBuscarDTOs clienteSeleccionadoActual = null;
        public cotizacion()
        {
            InitializeComponent();
            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
            texclientes.Leave += texclientes_Leave;
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




        private async void cotizacion_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";
            texsubtotal.Enabled = false;
            texdescuento.Enabled = false;
            textotal.Enabled = false;
            _clientes = await _clienteService.ListarClientes(); // ajustá el nombre real del método

            var fuenteClientes = new AutoCompleteStringCollection();
            fuenteClientes.AddRange(_clientes.Select(c => c.nombre).ToArray());

            texclientes.AutoCompleteMode = AutoCompleteMode.Suggest;
            texclientes.AutoCompleteSource = AutoCompleteSource.CustomSource;
            texclientes.AutoCompleteCustomSource = fuenteClientes;

            if (!dataGridView1.Columns.Contains("id_producto"))
            {
                dataGridView1.Columns.Add("id_producto", "id_producto");
                dataGridView1.Columns["id_producto"].Visible = false;
            }



            texapellido.Visible = false;
            labapellido.Visible = false;

            _productos = await _productoService.ListarProducto();

            var colProducto = (DataGridViewTextBoxColumn)dataGridView1.Columns["producto"];

        }


        //EVENTO PARA SELECCIONAR EL CLIENTE DE LA LISTA DE AUTOCOMPLETADO
        private void texclientes_Leave(object sender, EventArgs e)
        {
            string texto = texclientes.Text.Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                _clienteSeleccionado = null;
                LimpiarCamposCliente(habilitar: true);
                return;
            }

            var cliente = _clientes.FirstOrDefault(c =>
                c.nombre.Trim().Equals(texto, StringComparison.OrdinalIgnoreCase));

            if (cliente != null)
            {
                // Cliente existente: llenar y bloquear
                _clienteSeleccionado = cliente;

                texnit.Text = cliente.nit ?? "";
                textelefono.Text = cliente.telefono ?? "";
                texcorreo.Text = cliente.correo_electronico ?? "";
                texdireccion.Text = cliente.direccion ?? "";
                texdpi.Text = cliente.dpi ?? "";

                texnit.ReadOnly = true;
                textelefono.ReadOnly = true;
                texcorreo.ReadOnly = true;
                texdireccion.ReadOnly = true;
                texdpi.ReadOnly = true;
            }
            else
            {
                // Cliente nuevo: limpiar y habilitar para que el usuario escriba
                _clienteSeleccionado = null;
                LimpiarCamposCliente(habilitar: true);
            }
        }

        private void LimpiarCamposCliente(bool habilitar)
        {
            texnit.Text = "";
            textelefono.Text = "";
            texcorreo.Text = "";
            texdireccion.Text = "";
            texdpi.Text = "";

            texnit.ReadOnly = !habilitar;
            textelefono.ReadOnly = !habilitar;
            texcorreo.ReadOnly = !habilitar;
            texdireccion.ReadOnly = !habilitar;
            texdpi.ReadOnly = !habilitar;
        }





        private void limpiar()
        {
            textelefono.Text = "";
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

        //arma el autocompletado en la celda producto 
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {/*
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
            }*/

            if (e.Control is not TextBox textBox)
                return;

            // Es importante quitar primero el evento para evitar
            // que se conecte varias veces.
            textBox.KeyPress -= SoloEnteros_KeyPress;
            textBox.KeyPress -= SoloDecimales_KeyPress;

            string columna = dataGridView1.CurrentCell.OwningColumn.Name;

            if (columna == "producto")
            {
                textBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;

                var fuente = new AutoCompleteStringCollection();

                fuente.AddRange(
                    _productos.Select(p => p.nombreMostrar).ToArray());

                textBox.AutoCompleteCustomSource = fuente;
            }
            else
            {
                // Quita el autocompletado al editar otra columna.
                textBox.AutoCompleteMode = AutoCompleteMode.None;
                textBox.AutoCompleteSource = AutoCompleteSource.None;
                textBox.AutoCompleteCustomSource = null;
            }

            if (columna == "cantidad")
            {
                textBox.KeyPress += SoloEnteros_KeyPress;
            }

            if (columna == "descuento")
            {
                textBox.KeyPress += SoloDecimales_KeyPress;
            }



        }

        private void SoloEnteros_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) &&
                !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void SoloDecimales_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            char separador = Convert.ToChar(
                System.Globalization.CultureInfo.CurrentCulture
                .NumberFormat.NumberDecimalSeparator);

            if (char.IsDigit(e.KeyChar) ||
                char.IsControl(e.KeyChar))
                return;

            if (e.KeyChar == separador &&
                !textBox.Text.Contains(separador))
                return;

            e.Handled = true;
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

                dataGridView1.Rows[e.RowIndex].Tag = producto;

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
                if (fila.IsNewRow) continue;
                if (fila.Tag == null) continue;

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

            dataGridView1.EndEdit();

            var detalles = new List<DetalleDTOs>();
            decimal subtotal = 0;
            decimal descuentoTotal = 0;

            foreach (DataGridViewRow fila in dataGridView1.Rows)
            {
                if (fila.IsNewRow) continue;
                if (fila.Tag is not ProductoVentaBuscarDTO producto) continue;

                int cantidad = Convert.ToInt32(fila.Cells["cantidad"].Value ?? 0);
                decimal descuento = Convert.ToDecimal(fila.Cells["descuento"].Value ?? 0);

                if (cantidad <= 0) continue;

                detalles.Add(new DetalleDTOs
                {
                    id_producto = producto.id_producto,
                    id_producto_presentacion = producto.id_producto_presentacion,
                    cantidad = cantidad,
                    descuento = descuento
                });

                subtotal += cantidad * producto.precio;
                descuentoTotal += descuento;
            }

            if (detalles.Count == 0)
            {
                MessageBox.Show("Agregá al menos un producto antes de cobrar.");
                return;
            }

            if (_clienteSeleccionado == null && string.IsNullOrWhiteSpace(texclientes.Text.Trim()))
            {
                MessageBox.Show("Ingresá o seleccioná un cliente antes de cobrar.");
                return;
            }

            decimal total = subtotal - descuentoTotal;

            using var formCobro = new formCobro(
                detalles,
                _clienteSeleccionado,
                texclientes.Text.Trim(),
                texnit.Text.Trim(),
                textelefono.Text.Trim(),
                texcorreo.Text.Trim(),
                texdireccion.Text.Trim(),
                texdpi.Text.Trim(),
                Sesion.IdUsuario,
                subtotal,
                descuentoTotal,
                total
            );

            if (formCobro.ShowDialog() == DialogResult.OK)
            {
                limpiar();
            }
        }

    
    }
}