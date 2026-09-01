using SISTEMA_FROTEND.DTOs.Catalogo;
using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.DTOs.Productos;
using SISTEMA_FROTEND.DTOs.Ventas;
using SISTEMA_FROTEND.forms;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.services;
using SISTEMA_FROTEND.Utilidades;
using System.Data;
using static System.Net.WebRequestMethods;


namespace SISTEMA_FROTEND.presentacion
{
    public partial class Ventas : Form
    {
        private ClienteService _clienteService = new ClienteService();
        private ListarClienteDTOs _clienteSeleccionado;
        private List<ListarClienteDTOs> _clientes;
        private List<ProductoVentaBuscarDTO> _productosCodigoBarra = new();
        private readonly VentaService _ventaservice;
        private readonly ListBox _selectorPresentaciones = new ListBox();


        public Ventas()
        {
            InitializeComponent();

            _productoService = new ProductoService();
            _ventaservice = new VentaService();

            dataGridView1.CellEndEdit += dataGridView1_CellEndEdit;
            dataGridView1.EditingControlShowing += dataGridView1_EditingControlShowing;
            dataGridView1.CellValidating += dataGridView1_CellValidating;            //dataGridView1.KeyDown += dataGridView1_KeyDown;
            // dataGridView1.CellValidating += dataGridView1_CellValidating;
            texclientes.Leave += texclientes_Leave; 

            // Configuración del selector de presentaciones
            _selectorPresentaciones.Visible = false;
            _selectorPresentaciones.IntegralHeight = true;
            _selectorPresentaciones.Height = 120;
            _selectorPresentaciones.Width = 250;
            _selectorPresentaciones.SelectionMode = SelectionMode.One;
            _selectorPresentaciones.TabStop = true;
            _selectorPresentaciones.KeyDown += SelectorPresentaciones_KeyDown;
            _selectorPresentaciones.DoubleClick += SelectorPresentaciones_DoubleClick;
            // Agregarlo al mismo contenedor del DataGridView
            dataGridView1.Parent.Controls.Add(
                _selectorPresentaciones
            );

            _selectorPresentaciones.BringToFront();
        }

        protected override bool ProcessCmdKey(
     ref Message msg,
     Keys keyData)
        {
            Keys tecla = keyData & Keys.KeyCode;

            // Si no es Enter, dejar que WinForms procese la tecla normal
            if (tecla != Keys.Enter)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            // =========================================
            // TEXTBOX DEL FORMULARIO
            // =========================================

            if (texclientes.Focused)
            {
                texdireccion.Focus();
                return true;
            }

            if (texdireccion.Focused)
            {
                texcorreo.Focus();
                return true;
            }

            if (texcorreo.Focused)
            {
                textelefono.Focus();
                return true;
            }

            if (textelefono.Focused)
            {
                texnit.Focus();
                return true;
            }

            if (texnit.Focused)
            {
                texdpi.Focus();
                return true;
            }

            // DPI -> primera fila / codigo de barras
            if (texdpi.Focused)
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.Focus();

                    dataGridView1.CurrentCell =
                        dataGridView1.Rows[0]
                            .Cells["codigobarra"];

                    dataGridView1.BeginEdit(true);
                }

                return true;
            }

            // =========================================
            // DATAGRIDVIEW
            // =========================================

            if (dataGridView1.ContainsFocus &&
                dataGridView1.CurrentCell != null)
            {
                int filaActual =
                    dataGridView1.CurrentCell.RowIndex;

                string columna =
                    dataGridView1.CurrentCell
                        .OwningColumn.Name;

                var fila =
                    dataGridView1.Rows[filaActual];

                // CANTIDAD -> DESCUENTO
                if (columna == "cantidad")
                {
                    string texto = "";

                    if (dataGridView1.EditingControl is TextBox textBox)
                    {
                        texto = textBox.Text.Trim();
                    }
                    else
                    {
                        texto =
                            dataGridView1.CurrentCell.Value?
                                .ToString()?
                                .Trim() ?? "";
                    }

                    if (!int.TryParse(
                            texto,
                            out int cantidad) ||
                        cantidad <= 0)
                    {
                        MessageBox.Show(
                            "Debe ingresar una cantidad mayor a 0.",
                            "Cantidad requerida",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        dataGridView1.BeginEdit(true);

                        return true;
                    }

                    dataGridView1.EndEdit();

                    BeginInvoke(new Action(() =>
                    {
                        dataGridView1.CurrentCell =
                            fila.Cells["descuento"];

                        dataGridView1.BeginEdit(true);
                    }));

                    return true;
                }

                // DESCUENTO -> CODIGO BARRA SIGUIENTE FILA
                if (columna == "descuento")
                {
                    dataGridView1.EndEdit();

                    int siguienteFila =
                        filaActual + 1;

                    BeginInvoke(new Action(() =>
                    {
                        if (siguienteFila <
                            dataGridView1.Rows.Count)
                        {
                            dataGridView1.CurrentCell =
                                dataGridView1
                                    .Rows[siguienteFila]
                                    .Cells["codigobarra"];

                            dataGridView1.BeginEdit(true);
                        }
                    }));

                    return true;
                }

                // CODIGO BARRA -> PRODUCTO
                if (columna == "codigobarra")
                {
                    dataGridView1.EndEdit();

                    BeginInvoke(new Action(() =>
                    {
                        dataGridView1.CurrentCell =
                            fila.Cells["producto"];

                        dataGridView1.BeginEdit(true);
                    }));

                    return true;
                }
            }

            return base.ProcessCmdKey(
                ref msg,
                keyData);
        }

        //instanciamos el servicio de productos para poder acceder a la lista de productos y sus detalles
        private ProductoService _productoService = new ProductoService();
        //guardamos la lista de productos en memoria para poder acceder a ellos sin tener que hacer otra consulta a la api
        private List<ProductoVentaBuscarDTO> _productos;
        

        private void SelectorPresentaciones_KeyDown(object? sender, KeyEventArgs e)
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
        private void SelectorPresentaciones_DoubleClick(object? sender, EventArgs e)
        {
            ConfirmarPresentacionSeleccionada();
        }

        private void ConfirmarPresentacionSeleccionada()
        {
            if (_selectorPresentaciones.SelectedItem == null)
                return;

            if (_selectorPresentaciones.Tag is not int rowIndex)
                return;

            string seleccion = _selectorPresentaciones.SelectedItem.ToString() ?? "";

            var producto = _productosCodigoBarra.FirstOrDefault(p => p.nombreMostrar.Equals(seleccion, StringComparison.OrdinalIgnoreCase));

            if (producto == null)
                return;

            var fila = dataGridView1.Rows[rowIndex];
            fila.Cells["id_producto"].Value = producto.id_producto;
            fila.Cells["id_producto_presentacion"].Value = producto.id_producto_presentacion;
            fila.Cells["producto"].Value = producto.nombreMostrar;
            fila.Cells["stock"].Value = producto.stock;
            fila.Cells["precio"].Value = producto.precio;

            if (fila.Cells["descuento"].Value == null || string.IsNullOrWhiteSpace(fila.Cells["descuento"].Value?.ToString()))
            {
                fila.Cells["descuento"].Value = 0;
            }

            // MUY IMPORTANTE para crear la venta después
            fila.Tag = producto;

            CalcularSubtotal(rowIndex);
            _selectorPresentaciones.Visible = false;
            _productosCodigoBarra.Clear();
            dataGridView1.CurrentCell = fila.Cells["cantidad"];

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
           // EliminarColumna();

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

            textBox.KeyPress -= SoloEnteros_KeyPress;
            textBox.KeyPress -= SoloDecimales_KeyPress;

           

            string columna = dataGridView1.CurrentCell.OwningColumn.Name;

            if (columna == "producto")
            {
                textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;

                var fuente = new AutoCompleteStringCollection();

                if (_productosCodigoBarra.Count > 0)
                {
                    fuente.AddRange(_productosCodigoBarra.Select(p => p.nombreMostrar).ToArray()
                    );
                }
                else
                {
                    fuente.AddRange(_productos.Select(p => p.nombreMostrar).ToArray()
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

            if (e.KeyChar == separador && !textBox.Text.Contains(separador))
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
                var producto = listaBusqueda.FirstOrDefault(p => p.nombreMostrar.Trim().Equals(textoElegido.Trim(), StringComparison.OrdinalIgnoreCase));

                if (producto == null) return;
                // se llena automaticamente los campos de precio y descuento con los valores del producto encontrado
                dataGridView1.Rows[e.RowIndex].Cells["id_producto"].Value = producto.id_producto;
                dataGridView1.Rows[e.RowIndex].Cells["stock"].Value = producto.stock;
                dataGridView1.Rows[e.RowIndex].Cells["precio"].Value = producto.precio;
        

                // Mismo criterio para descuento
                if (fila.Cells["descuento"].Value == null || string.IsNullOrWhiteSpace(fila.Cells["descuento"].Value.ToString()))
                {
                    fila.Cells["descuento"].Value = 0;
                }

                dataGridView1.Rows[e.RowIndex].Tag = producto;
                _productosCodigoBarra.Clear(); // Limpiar la lista de productos por código de barra después de seleccionar un producto
                CalcularSubtotal(e.RowIndex);

                int filaActual = e.RowIndex;

                BeginInvoke(new Action(() =>
                {
                    if (filaActual < dataGridView1.Rows.Count)
                    {
                        dataGridView1.CurrentCell =
                            dataGridView1.Rows[filaActual].Cells["cantidad"];

                        dataGridView1.BeginEdit(true);
                    }
                }));
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


        private void cliente_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            var catalogo = new Catalog();
            catalogo.ProductosSeleccionados += Catalogo_ProductosSeleccionados;
            catalogo.ShowDialog();
        }

        private void Catalogo_ProductosSeleccionados(object? sender, List<ProductoSeleccionadoDTO> productos)
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

            DataGridViewRow nuevaFila = dataGridView1.Rows[indice];
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

       

        private async void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var fila = dataGridView1.Rows[e.RowIndex];

            if (fila.IsNewRow)
                return;

            if (dataGridView1.Columns[e.ColumnIndex].Name != "eliminar")
                return;

            dataGridView1.Rows.RemoveAt(e.RowIndex);

            RecalcularTotales();
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
                        id_producto_presentacion = p.id_producto_presentacion,
                        nombre_producto = producto.nombre_producto,
                        presentacion = p.presentacion,
                        unidades_equivalentes = p.unidades_equivalentes,
                        precio = p.precio,
                        stock = producto.stock
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
        private void MostrarSelectorPresentaciones(int rowIndex, string nombreProducto)
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


            Rectangle rectangulo = dataGridView1.GetCellDisplayRectangle(celda.ColumnIndex, celda.RowIndex, true
                );

            // 6. Convertir posición del DataGridView a pantalla
            Point posicionPantalla = dataGridView1.PointToScreen(
                    new Point(
                        rectangulo.Left,
                        rectangulo.Bottom
                    )
                );

            // 7. Convertir posición de pantalla al contenedor
            // donde agregamos el ListBox
            Point posicionContenedor = dataGridView1.Parent.PointToClient(posicionPantalla
                );

            // 8. Posicionar el ListBox debajo de Producto
            _selectorPresentaciones.Location = posicionContenedor;

            // 9. Tamaño
            _selectorPresentaciones.Width = Math.Max(rectangulo.Width, 250);

            _selectorPresentaciones.Height = Math.Min(_productosCodigoBarra.Count * 25 + 5, 150
                );

            // 10. Guardar la fila que estamos editando
            _selectorPresentaciones.Tag = rowIndex;

            // 11. Mostrar
            _selectorPresentaciones.Visible = true;
            _selectorPresentaciones.BringToFront();
            _selectorPresentaciones.Focus();
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            try
            {


                dataGridView1.EndEdit();

                var detalles = new List<CrearDetalleVentaDTO>();
                decimal subtotal = 0;
                decimal descuentoTotal = 0;

                foreach (DataGridViewRow fila in dataGridView1.Rows)
                {
                    if (fila.IsNewRow ||
                        fila.Tag is not ProductoVentaBuscarDTO producto)
                        continue;

                    int cantidad = Convert.ToInt32(
                        fila.Cells["cantidad"].Value ?? 0);

                    decimal descuento = Convert.ToDecimal(
                        fila.Cells["descuento"].Value ?? 0);

                    if (cantidad <= 0)
                        continue;

                    // Cantidad real que se descontará del stock
                    int unidadesSolicitadas =
                        cantidad * producto.unidades_equivalentes;

                    if (unidadesSolicitadas > producto.stock)
                    {
                        MessageBox.Show(
                            $"Existencia insuficiente para {producto.nombreMostrar}.\n\n" +
                            $"Disponible: {producto.stock} unidades\n" +
                            $"Solicitado: {unidadesSolicitadas} unidades",
                            "Stock insuficiente",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        return;
                    }

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
                    MessageBox.Show(
                        "Agregá al menos un producto antes de cobrar."
                    );
                    return;
                }

                if (_clienteSeleccionado == null &&
                    string.IsNullOrWhiteSpace(texclientes.Text))
                {
                    MessageBox.Show(
                        "Ingresá o seleccioná un cliente."
                    );
                    return;
                }

                if (!decimal.TryParse(
                        texefectivorecibido.Text,
                        out decimal efectivoRecibido) ||
                    efectivoRecibido < 0)
                {
                    MessageBox.Show(
                        "Ingrese un monto válido."
                    );
                    return;
                }

                decimal total = subtotal - descuentoTotal;

                // Dinero que realmente pertenece a la venta
                decimal montoPagado =
                    Math.Min(efectivoRecibido, total);

                // Si entrega más que el total
                decimal cambio =
                    Math.Max(0, efectivoRecibido - total);

                // Si entrega menos que el total
                decimal saldoPendiente =
                    Math.Max(0, total - montoPagado);

                var venta = new CrearVentaDTO
                {
                    origen = "WinForms",
                    detalles = detalles,

                    pago = new CrearPagoVentaDTO
                    {
                        // IMPORTANTE:
                        monto = montoPagado,
                        metodo_pago = "EFECTIVO"
                    }
                };

                // Cliente existente
                if (_clienteSeleccionado != null)
                {
                    venta.id_cliente =
                        _clienteSeleccionado.id_Cliente;
                }
                else
                {
                    // Cliente nuevo
                    venta.id_cliente = 0;

                    venta.clienteNuevo =
                        new CrearClienteVentaDTO
                        {
                            nombre = texclientes.Text.Trim(),
                            nit = texnit.Text.Trim(),
                            dpi = texdpi.Text.Trim(),
                            telefono = textelefono.Text.Trim(),
                            correo_electronico =
                                texcorreo.Text.Trim(),
                            direccion =
                                texdireccion.Text.Trim()
                        };
                }

                // Registrar en la API
                await _ventaservice.CrearVenta(venta);

                string mensaje;

                if (saldoPendiente > 0)
                {
                    mensaje =
                        $"Venta registrada correctamente.\n\n" +
                        $"Total: Q{total:N2}\n" +
                        $"Pagado: Q{montoPagado:N2}\n" +
                        $"Saldo pendiente: Q{saldoPendiente:N2}";
                }
                else
                {
                    mensaje =
                        $"Venta registrada correctamente.\n\n" +
                        $"Total: Q{total:N2}\n" +
                        $"Recibido: Q{efectivoRecibido:N2}\n" +
                        $"Cambio: Q{cambio:N2}";
                }

                MessageBox.Show(
                    mensaje,
                    "Venta registrada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                limpiar();
                texefectivorecibido.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al registrar venta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            texefectivorecibido.Text = textotal.Text;
        }



        private void dataGridView1_CellValidating(object sender,DataGridViewCellValidatingEventArgs e)
        {

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var fila = dataGridView1.Rows[e.RowIndex];

            string columna =
                dataGridView1.Columns[e.ColumnIndex].Name;

          
            if (columna == "producto")
            {
                string texto =
                    e.FormattedValue?.ToString()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(texto))
                    return;

                var listaBusqueda =
                    _productosCodigoBarra.Count > 0
                        ? _productosCodigoBarra
                        : _productos;

                var producto = listaBusqueda.FirstOrDefault(p =>
                    p.nombreMostrar.Trim().Equals(
                        texto,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

                if (producto == null)
                {
                    MessageBox.Show(
                        "Debe seleccionar un producto válido de la lista.",
                        "Producto no válido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    e.Cancel = true;
                    return;
                }
            }

       
            if (columna == "cantidad" ||
                columna == "descuento")
            {
                // Tag solo existe cuando ya seleccionamos
                // correctamente un producto
                if (fila.Tag == null)
                {
                    string valor =
                        e.FormattedValue?.ToString()?.Trim() ?? "";

                    // Solo mostrar mensaje si realmente escribió algo
                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        MessageBox.Show(
                            "Primero debe seleccionar un producto.",
                            "Producto requerido",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        // Borrar lo que escribió
                        fila.Cells[columna].Value = null;

                        e.Cancel = true;
                    }

                    return;
                }
            }

            if (columna == "cantidad")
            {
                string texto =
                    e.FormattedValue?.ToString()?.Trim() ?? "";

                if (!int.TryParse(texto, out int cantidad) ||
                    cantidad <= 0)
                {
                    MessageBox.Show(
                        "Debe ingresar una cantidad mayor a 0.",
                        "Cantidad requerida",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    e.Cancel = true;
                    return;
                }
            }

            if (columna == "descuento")
            {
                string texto =
                    e.FormattedValue?.ToString()?.Trim() ?? "";

                // Si está vacío, puedes asumir descuento = 0
                if (string.IsNullOrWhiteSpace(texto))
                    return;

                if (!decimal.TryParse(texto, out decimal descuento) ||
                    descuento < 0)
                {
                    MessageBox.Show(
                        "Ingrese un descuento válido.",
                        "Descuento no válido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    e.Cancel = true;
                }
            }
        }
    }


}