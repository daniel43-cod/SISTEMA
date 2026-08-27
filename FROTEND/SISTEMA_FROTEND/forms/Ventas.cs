using SISTEMA_FROTEND.DTOs.Catalogo;
using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.DTOs.Productos;
using SISTEMA_FROTEND.DTOs.Ventas;
using SISTEMA_FROTEND.forms;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.services;
using SISTEMA_FROTEND.Utilidades;
using System.Data;


namespace SISTEMA_FROTEND.presentacion
{
    public partial class Ventas : Form
    {
        private ClienteService _clienteService = new ClienteService();
        private ListarClienteDTOs _clienteSeleccionado;
        private List<ListarClienteDTOs> _clientes;
        private List<ProductoVentaBuscarDTO> _productosCodigoBarra = new();
        private ProductoService ProductoService;
        private readonly ListBox _selectorPresentaciones = new ListBox();


        //datos que se llena en los campos si el usuario confirma la eleccion de algun cliente existente
        private ClienteBuscarDTOs clienteSeleccionadoActual = null;
        public Ventas()
        {
            InitializeComponent();

            _productoService = new ProductoService();

            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            dataGridView1.EditingControlShowing +=dataGridView1_EditingControlShowing;

            texclientes.Leave += texclientes_Leave;

            // Configuración del selector de presentaciones
            _selectorPresentaciones.Visible = false;
            _selectorPresentaciones.IntegralHeight = true;
            _selectorPresentaciones.Height = 120;
            _selectorPresentaciones.Width = 250;
            _selectorPresentaciones.SelectionMode =SelectionMode.One;
            _selectorPresentaciones.TabStop = true;
            _selectorPresentaciones.KeyDown +=SelectorPresentaciones_KeyDown;
            _selectorPresentaciones.DoubleClick +=SelectorPresentaciones_DoubleClick;
            // Agregarlo al mismo contenedor del DataGridView
            dataGridView1.Parent.Controls.Add(
                _selectorPresentaciones
            );

            _selectorPresentaciones.BringToFront();
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

        private void SelectorPresentaciones_KeyDown(object? sender,KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConfirmarPresentacionSeleccionada();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }

            if (e.KeyCode == Keys.Escape)
            {
                _selectorPresentaciones.Visible = false;

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
        private void SelectorPresentaciones_DoubleClick(object? sender,EventArgs e)
        {
            ConfirmarPresentacionSeleccionada();
        }

        private void ConfirmarPresentacionSeleccionada()
        {
            if (_selectorPresentaciones.SelectedItem == null)
                return;

            if (_selectorPresentaciones.Tag is not int rowIndex)
                return;

            string seleccion =_selectorPresentaciones.SelectedItem.ToString() ?? "";

            var producto = _productosCodigoBarra.FirstOrDefault(p =>p.nombreMostrar.Equals(seleccion,StringComparison.OrdinalIgnoreCase));

            if (producto == null)
                return;

            var fila = dataGridView1.Rows[rowIndex];
            fila.Cells["id_producto"].Value =producto.id_producto;
            fila.Cells["id_producto_presentacion"].Value =producto.id_producto_presentacion;
            fila.Cells["producto"].Value =producto.nombreMostrar;
            fila.Cells["stock"].Value =producto.stock;
            fila.Cells["precio"].Value =producto.precio;
            if (fila.Cells["cantidad"].Value == null ||string.IsNullOrWhiteSpace(fila.Cells["cantidad"].Value?.ToString()))
            {
                fila.Cells["cantidad"].Value = 1;
            }

            if (fila.Cells["descuento"].Value == null ||string.IsNullOrWhiteSpace(fila.Cells["descuento"].Value?.ToString()))
            {
                fila.Cells["descuento"].Value = 0;
            }

            // MUY IMPORTANTE para crear la venta después
            fila.Tag = producto;

            CalcularSubtotal(rowIndex);
            _selectorPresentaciones.Visible = false;
            _productosCodigoBarra.Clear();
            dataGridView1.CurrentCell =fila.Cells["cantidad"];

            dataGridView1.BeginEdit(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void cotizacion_Load(object sender, EventArgs e)
        {
            labusuario.ForeColor = Color.Black; // o el color que necesites
            labusuario.Text = $"Usuario: {SesionUsuario.Nombre}";

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

            dataGridView1.Columns["id_producto_presentacion"].Visible = false;
            dataGridView1.Columns["stock"].Visible = false;


            dataGridView1.Columns["codigobarra"].Width = 130;
            dataGridView1.Columns["producto"].Width = 280;
            dataGridView1.Columns["cantidad"].Width = 90;
            dataGridView1.Columns["precio"].Width = 100;
            dataGridView1.Columns["descuento"].Width = 100;
            dataGridView1.Columns["subtotal1"].Width = 120;

            _productos = await _productoService.ListarProducto();

            var colProducto = (DataGridViewTextBoxColumn)dataGridView1.Columns["producto"];

            dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.DefaultCellStyle.BackColor = Color.White;

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

            var cliente = _clientes.FirstOrDefault(c => c.nombre.Trim().Equals(texto, StringComparison.OrdinalIgnoreCase));

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


        private void button2_Click(object sender, EventArgs e)
        {
            limpiar();
        }

        private void limpiar()
        {
            textelefono.Text = "";
            texnit.Text = "";
            texdpi.Text = "";
            texdireccion.Text = "";
            texcorreo.Text = "";
            texclientes.Text = "";
            dataGridView1.Rows.Clear();
            textotal.Text = "";
            texdescuento.Text = "";
            texsubtotal.Text = "";
        }


        //arma el autocompletado en la celda producto 
        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is not TextBox textBox)
                return;

            // Es importante quitar primero el evento para evitar
            // que se conecte varias veces.
            textBox.KeyPress -= SoloEnteros_KeyPress;
            textBox.KeyPress -= SoloDecimales_KeyPress;

            string columna = dataGridView1.CurrentCell.OwningColumn.Name;

            if (columna == "producto")
            {
                textBox.AutoCompleteMode =AutoCompleteMode.SuggestAppend;
                textBox.AutoCompleteSource =AutoCompleteSource.CustomSource;

                var fuente =new AutoCompleteStringCollection();

                if (_productosCodigoBarra.Count > 0)
                {
                    fuente.AddRange(_productosCodigoBarra.Select(p => p.nombreMostrar).ToArray()
                    );
                }
                else
                {
                    fuente.AddRange( _productos .Select(p => p.nombreMostrar).ToArray()
                    );
                }

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
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void SoloDecimales_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            char separador = Convert.ToChar(System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            if (char.IsDigit(e.KeyChar) || char.IsControl(e.KeyChar))
                return;

            if (e.KeyChar == separador &&!textBox.Text.Contains(separador))
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

            fila.Cells["subtotal1"].Value = (cantidad * precio) - descuento;

            RecalcularTotales();
        }

        private async void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            //everigua el nombde del producto seleccionado o editado

            string columna = dataGridView1.Columns[e.ColumnIndex].Name;

            if (columna == "codigobarra")
            {
                await BuscarProductoCodigoBarra(e.RowIndex);
                return;
            }
            //si la columna editada es la de producto, busca el producto en la lista de productos y llena los campos correspondientes
            if (columna == "producto")
            {
                var fila = dataGridView1.Rows[e.RowIndex];
                //toma el texto o producto elejido 
                string textoElegido = dataGridView1.Rows[e.RowIndex].Cells["producto"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(textoElegido)) return;
                //busca que el producto conicida con algunos de los productos en la lista de productos o almacenados en memoria 
                var listaBusqueda = _productosCodigoBarra.Count > 0 ? _productosCodigoBarra : _productos;
                var producto = listaBusqueda.FirstOrDefault(p => p.nombreMostrar.Trim().Equals(textoElegido.Trim(),StringComparison.OrdinalIgnoreCase));

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
                _productosCodigoBarra.Clear(); // Limpiar la lista de productos por código de barra después de seleccionar un producto
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

            var detalles = new List<CrearDetalleVentaDTO>();
            decimal subtotal = 0;
            decimal descuentoTotal = 0;

            foreach (DataGridViewRow fila in dataGridView1.Rows)
            {
                if (fila.IsNewRow) continue;
                if (fila.Tag is not ProductoVentaBuscarDTO producto) continue;

                int cantidad = Convert.ToInt32(fila.Cells["cantidad"].Value ?? 0);
                decimal descuento = Convert.ToDecimal(fila.Cells["descuento"].Value ?? 0);

                if (cantidad <= 0) continue;

                detalles.Add(new CrearDetalleVentaDTO
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

            using var formCobro = new formCobro(detalles, _clienteSeleccionado, texclientes.Text.Trim(), texnit.Text.Trim(), textelefono.Text.Trim(), texcorreo.Text.Trim(), texdireccion.Text.Trim(),
                texdpi.Text.Trim(), Sesion.IdUsuario, subtotal, descuentoTotal, total
            );

            if (formCobro.ShowDialog() == DialogResult.OK)
            {
                limpiar();
            }
        }

        private void cliente_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            var catalogo = new Catalog();
            catalogo.ProductosSeleccionados += Catalogo_ProductosSeleccionados;
            catalogo.ShowDialog();
        }

        private void Catalogo_ProductosSeleccionados(object? sender,List<ProductoSeleccionadoDTO> productos)
{
            foreach (var producto in productos)
            {
                AgregarProductoAlGrid(producto);
            }
        }

        private void AgregarProductoAlGrid(ProductoSeleccionadoDTO producto)
        {
            var productoGrid = new ProductoVentaBuscarDTO
            {
                id_producto = producto.id_producto,
                id_producto_presentacion = producto.id_producto_presentacion,
                nombre_producto = producto.nombre_producto,
                presentacion = producto.presentacion,
                //  nombreMostrar =$"{producto.nombre_producto} - {producto.presentacion}",
                precio = producto.precio,
                stock = producto.stock,
                unidades_equivalentes = producto.unidades_equivalentes
            };

            DataGridViewRow? filaExistente = null;

            foreach (DataGridViewRow fila in dataGridView1.Rows)
            {
                if (fila.IsNewRow) continue;

                int idProducto = Convert.ToInt32(fila.Cells["id_producto"].Value ?? 0);
                int idPresentacion = Convert.ToInt32(fila.Cells["id_producto_presentacion"].Value ?? 0);

                if (idProducto == producto.id_producto && idPresentacion == producto.id_producto_presentacion)
                {
                    filaExistente = fila;
                    break;
                }
            }

            if (filaExistente != null)
            {
                int cantidadActual = Convert.ToInt32(filaExistente.Cells["cantidad"].Value ?? 0);
                int nuevaCantidad = cantidadActual + producto.cantidad;
                filaExistente.Cells["cantidad"].Value = nuevaCantidad;
                filaExistente.Tag = productoGrid;
                CalcularSubtotal(filaExistente.Index);

                return;
            }

            int indice = dataGridView1.Rows.Add();

            DataGridViewRow nuevaFila =dataGridView1.Rows[indice];

            nuevaFila.Cells["id_producto"].Value = producto.id_producto;
            nuevaFila.Cells["id_producto_presentacion"].Value = producto.id_producto_presentacion;
            nuevaFila.Cells["producto"].Value = $"{producto.nombre_producto} - {producto.presentacion}";
            nuevaFila.Cells["cantidad"].Value = producto.cantidad;
            nuevaFila.Cells["precio"].Value = producto.precio;
            nuevaFila.Cells["stock"].Value = producto.stock;
            nuevaFila.Cells["descuento"].Value = 0;
            nuevaFila.Tag = productoGrid;

            CalcularSubtotal(indice);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private async void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private async Task BuscarProductoCodigoBarra(int rowIndex)
        {
            var fila = dataGridView1.Rows[rowIndex];

            string codigo = fila.Cells["codigobarra"]
                .Value?
                .ToString()?
                .Trim() ?? "";

            if (string.IsNullOrWhiteSpace(codigo))
                return;

            try
            {
                var producto = await _productoService
                    .BuscarPorCodigoBarra(codigo);

                if (producto == null)
                {
                    MessageBox.Show(
                        "No existe un producto con ese código de barras.",
                        "Producto no encontrado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                // 1. Crear la lista de presentaciones
                _productosCodigoBarra = producto.presentaciones
                    .Select(p => new ProductoVentaBuscarDTO
                    {
                        id_producto = producto.id_producto,
                        id_producto_presentacion =p.id_producto_presentacion,
                        nombre_producto =producto.nombre_producto,
                        presentacion =p.presentacion,
                        unidades_equivalentes =p.unidades_equivalentes,
                        precio =p.precio,
                        stock =producto.stock
                    })
                    .ToList();

                fila.Cells["producto"].Value =
                    producto.nombre_producto;
                MostrarSelectorPresentaciones(
                    rowIndex,
                    producto.nombre_producto
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al buscar producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void MostrarSelectorPresentaciones(int rowIndex,string nombreProducto)
        {
            if (_productosCodigoBarra == null ||
                _productosCodigoBarra.Count == 0)
            {
                return;
            }
            _selectorPresentaciones.Items.Clear();

            foreach (var producto in _productosCodigoBarra)
            {
                _selectorPresentaciones.Items.Add(
                    producto.nombreMostrar
                );
            }

            _selectorPresentaciones.SelectedIndex = -1;

            var celda = dataGridView1.Rows[rowIndex].Cells["producto"];

  
            Rectangle rectangulo =dataGridView1.GetCellDisplayRectangle(celda.ColumnIndex,celda.RowIndex,true
                );

            // 6. Convertir posición del DataGridView a pantalla
            Point posicionPantalla =dataGridView1.PointToScreen(
                    new Point(
                        rectangulo.Left,
                        rectangulo.Bottom
                    )
                );

            // 7. Convertir posición de pantalla al contenedor
            // donde agregamos el ListBox
            Point posicionContenedor =dataGridView1.Parent.PointToClient(posicionPantalla
                );

            // 8. Posicionar el ListBox debajo de Producto
            _selectorPresentaciones.Location =posicionContenedor;

            // 9. Tamaño
            _selectorPresentaciones.Width =Math.Max(rectangulo.Width, 250);

            _selectorPresentaciones.Height =Math.Min(_productosCodigoBarra.Count * 25 + 5,150
                );

            // 10. Guardar la fila que estamos editando
            _selectorPresentaciones.Tag = rowIndex;

            // 11. Mostrar
            _selectorPresentaciones.Visible = true;
            _selectorPresentaciones.BringToFront();
            _selectorPresentaciones.Focus();
        }


    }


}