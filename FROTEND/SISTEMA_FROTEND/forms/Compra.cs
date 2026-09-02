using SISTEMA_FROTEND.DTOs;
using SISTEMA_FROTEND.DTOs.Compras;
using SISTEMA_FROTEND.DTOs.Productos;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.models;
using SISTEMA_FROTEND.services;
using SISTEMA_FROTEND.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace SISTEMA_FROTEND.forms
{
    public partial class frmcompras : Form
    {
        private ProductoService _productoService = new ProductoService();
        private List<ProductoVentaBuscarDTO> _productos = new();
        private EmpresaService _empresaService = new EmpresaService();
        private List<EmpresaDTOs> _empresas;
        private EmpresaDTOs _empresaSeleccionada;

        private CompraService _compraService = new CompraService();
        public frmcompras()
        {
            InitializeComponent();
            datacompras.CellEndEdit += datacompras_CellEndEdit;
            datacompras.EditingControlShowing += datacompras_EditingControlShowing;
            texEmpresa.Leave += texEmpresa_Leave;
        }

        private CancellationTokenSource _cts;



        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private async void frmproductos_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {SesionUsuario.Nombre}";

            _empresas = await _empresaService.ListarEmpresas();

            _productos = await _productoService.ListarProducto();

            var fuenteEmpresas = new AutoCompleteStringCollection();
            fuenteEmpresas.AddRange(_empresas.Select(e => e.nombre_empresa).ToArray());

            texEmpresa.AutoCompleteMode = AutoCompleteMode.Suggest;
            texEmpresa.AutoCompleteSource = AutoCompleteSource.CustomSource;
            texEmpresa.AutoCompleteCustomSource = fuenteEmpresas;

            datacompras.DefaultCellStyle.ForeColor = Color.Black;
            datacompras.DefaultCellStyle.BackColor = Color.White;

            datacompras.DefaultCellStyle.SelectionForeColor = Color.White;
            datacompras.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            datacompras.EnableHeadersVisualStyles = false;
            datacompras.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            datacompras.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkSlateGray;

        }

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {



        }

        private async void combuscar_TextChanged(object sender, EventArgs e)
        {

        }
        private void dataProductos_SelectionChanged(object sender, EventArgs e)
        {

        }

        private async void combuscar_Enter(object sender, EventArgs e)
        {

        }

        private async Task CargarTodosLosProductos()
        {
        }

        private void comempresa_TextChanged(object sender, EventArgs e)
        {

        }

        private void comempresa_SelectionChangeCommitted(object sender, EventArgs e)
        {

        }
        private void texEmpresa_Leave(object sender, EventArgs e)
        {
            var empresa = _empresas.FirstOrDefault(x => x.nombre_empresa.Trim().Equals(texEmpresa.Text.Trim(), StringComparison.OrdinalIgnoreCase));

            if (empresa == null)
            {
                MessageBox.Show("Seleccioná una empresa válida de la lista.");
                _empresaSeleccionada = null;
                texEmpresa.Text = "";
                return;
            }

            _empresaSeleccionada = empresa;
        }

        private void datacompras_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {

            if (datacompras.CurrentCell.OwningColumn.Name == "producto")
            {
                var textBox = e.Control as TextBox;

                if (textBox != null)
                {
                    textBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                    textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;

                    var fuente = new AutoCompleteStringCollection();
                    fuente.AddRange(_productos.Select(p => p.nombre_producto).ToArray());
                    textBox.AutoCompleteCustomSource = fuente;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys tecla = keyData & Keys.KeyCode;

            if (tecla != Keys.Enter)
                return base.ProcessCmdKey(ref msg, keyData);

            // EMPRESA -> NIT
            if (texEmpresa.Focused)
            {
                texnit.Focus();
                return true;
            }


            if (texnit.Focused)
            {
                if (datacompras.Rows.Count > 0)
                {
                    datacompras.Focus();

                    datacompras.CurrentCell =
                        datacompras.Rows[0]
                            .Cells["producto"];

                    datacompras.BeginEdit(true);
                }

                return true;
            }

            if (datacompras.ContainsFocus &&
                datacompras.CurrentCell != null)
            {
                int filaActual = datacompras.CurrentCell.RowIndex;

                string columna =
                    datacompras.CurrentCell.OwningColumn.Name;

                var fila = datacompras.Rows[filaActual];

                // PRODUCTO -> CANTIDAD
                if (columna == "producto")
                {
                    datacompras.EndEdit();

                    BeginInvoke(new Action(() =>
                    {
                        datacompras.CurrentCell =
                            fila.Cells["cantidad"];

                        datacompras.BeginEdit(true);
                    }));

                    return true;
                }

                // CANTIDAD -> PRECIO
                if (columna == "cantidad")
                {
                    string texto;

                    if (datacompras.EditingControl is TextBox textBox)
                        texto = textBox.Text.Trim();
                    else
                        texto = fila.Cells["cantidad"].Value?
                            .ToString()?.Trim() ?? "";

                    // Solo entero y mayor a 0
                    if (!int.TryParse(texto, out int cantidad) ||
                        cantidad <= 0)
                    {
                        MessageBox.Show(
                            "La cantidad debe ser un número entero mayor a 0.",
                            "Cantidad no válida",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        datacompras.BeginEdit(true);

                        return true;
                    }

                    datacompras.EndEdit();

                    BeginInvoke(new Action(() =>
                    {
                        datacompras.CurrentCell =
                            fila.Cells["precio"];

                        datacompras.BeginEdit(true);
                    }));

                    return true;
                }

                // PRECIO -> PRODUCTO DE LA SIGUIENTE FILA
                if (columna == "precio")
                {
                    string texto;

                    if (datacompras.EditingControl is TextBox textBox)
                        texto = textBox.Text.Trim();
                    else
                        texto = fila.Cells["precio"].Value?
                            .ToString()?.Trim() ?? "";

                    // Permite decimal pero debe ser mayor a 0
                    if (!decimal.TryParse(texto, out decimal precio) ||
                        precio <= 0)
                    {
                        MessageBox.Show(
                            "El precio debe ser un número mayor a 0.",
                            "Precio no válido",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        datacompras.BeginEdit(true);

                        return true;
                    }

                    datacompras.EndEdit();

                    int siguienteFila = filaActual + 1;

                    BeginInvoke(new Action(() =>
                    {
                        if (siguienteFila < datacompras.Rows.Count)
                        {
                            datacompras.CurrentCell =
                                datacompras.Rows[siguienteFila]
                                    .Cells["producto"];

                            datacompras.BeginEdit(true);
                        }
                    }));

                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void datacompras_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string columna = datacompras.Columns[e.ColumnIndex].Name;
            var fila = datacompras.Rows[e.RowIndex];

            if (columna == "producto")
            {
                string textoElegido = fila.Cells["producto"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(textoElegido)) return;

                var producto = _productos.FirstOrDefault(p =>
                    p.nombre_producto.Trim().Equals(textoElegido.Trim(), StringComparison.OrdinalIgnoreCase));

                if (producto == null) return;

                fila.Tag = producto;
            }

            if (columna == "cantidad" || columna == "precio")
            {
                CalcularPrecioUnitario(e.RowIndex);
                RecalcularTotalCompra();
            }

        }


        private void CalcularPrecioUnitario(int rowIndex)
        {
            var fila = datacompras.Rows[rowIndex];

            decimal cantidad = Convert.ToDecimal(fila.Cells["cantidad"].Value ?? 0);
            decimal precioTotal = Convert.ToDecimal(fila.Cells["precio"].Value ?? 0);

            fila.Cells["preciounitario"].Value = cantidad > 0 ? (precioTotal / cantidad) : 0;
        }

        private void RecalcularTotalCompra()
        {
            decimal totalCompra = 0;

            foreach (DataGridViewRow fila in datacompras.Rows)
            {
                if (fila.IsNewRow) continue;
                totalCompra += Convert.ToDecimal(fila.Cells["precio"].Value ?? 0);
            }

            textotalcompras.Text = totalCompra.ToString("N2"); // ajustá al nombre real de tu TextBox
        }

        private async void butguardar_Click(object sender, EventArgs e)
        {
            datacompras.EndEdit();

            if (_empresaSeleccionada == null)
            {
                MessageBox.Show(
                    "Debe seleccionar una empresa válida.",
                    "Empresa requerida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                texEmpresa.Focus();
                return;
            }

            var detalle = new List<DetalleCompraDTOs>();

            foreach (DataGridViewRow fila in datacompras.Rows)
            {
                if (fila.IsNewRow)
                    continue;

                bool filaVacia = string.IsNullOrWhiteSpace(
                    fila.Cells["producto"].Value?.ToString()
                );

                if (filaVacia)
                    continue;

                if (fila.Tag is not ProductoVentaBuscarDTO producto)
                {
                    MessageBox.Show(
                        "Hay una fila con un producto no válido.",
                        "Producto requerido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    datacompras.CurrentCell = fila.Cells["producto"];
                    datacompras.BeginEdit(true);
                    return;
                }

                string textoCantidad =
                    fila.Cells["cantidad"].Value?.ToString()?.Trim() ?? "";

                if (!int.TryParse(textoCantidad, out int cantidad) ||
                    cantidad <= 0)
                {
                    MessageBox.Show(
                        $"La cantidad de '{producto.nombre_producto}' " +
                        "debe ser un número entero mayor a 0.",
                        "Cantidad no válida",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    datacompras.CurrentCell = fila.Cells["cantidad"];
                    datacompras.BeginEdit(true);
                    return;
                }

                string textoPrecio =
                    fila.Cells["precio"].Value?.ToString()?.Trim() ?? "";

                if (!decimal.TryParse(textoPrecio, out decimal precio) ||
                    precio <= 0)
                {
                    MessageBox.Show(
                        $"El precio de '{producto.nombre_producto}' " +
                        "debe ser mayor a 0.",
                        "Precio no válido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    datacompras.CurrentCell = fila.Cells["precio"];
                    datacompras.BeginEdit(true);
                    return;
                }

                detalle.Add(new DetalleCompraDTOs
                {
                    id_producto = producto.id_producto,
                    cantidad = cantidad,
                    precio = precio
                });
            }

            if (detalle.Count == 0)
            {
                MessageBox.Show(
                    "Debe agregar al menos un producto a la compra.",
                    "Compra vacía",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            decimal totalCompra = 0;

            foreach (DataGridViewRow fila in datacompras.Rows)
            {
                if (fila.IsNewRow)
                    continue;

                if (fila.Cells["precio"].Value == null)
                    continue;

                if (decimal.TryParse(
                    fila.Cells["precio"].Value.ToString(),
                    out decimal totalFila))
                {
                    totalCompra += totalFila;
                }
            }

            if (totalCompra <= 0)
            {
                MessageBox.Show(
                    "El total de la compra debe ser mayor a 0.",
                    "Total no válido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string textoEfectivo = texabonar.Text.Trim();

            decimal efectivoRecibido = 0;

            if (!string.IsNullOrWhiteSpace(textoEfectivo))
            {
                if (!decimal.TryParse(
                    textoEfectivo,
                    out efectivoRecibido))
                {
                    MessageBox.Show(
                        "Ingrese un monto válido.",
                        "Monto no válido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    texabonar.Focus();
                    texabonar.SelectAll();
                    return;
                }
            }

            if (efectivoRecibido < 0)
            {
                MessageBox.Show(
                    "El monto ingresado no puede ser negativo.",
                    "Monto no válido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                texabonar.Focus();
                texabonar.SelectAll();
                return;
            }

            decimal montoPagado =
                Math.Min(efectivoRecibido, totalCompra);

            decimal cambioEsperado =
                Math.Max(0, efectivoRecibido - totalCompra);

            decimal saldoPendiente =
                Math.Max(0, totalCompra - montoPagado);

            var compraDto = new RegistroComprasDTO
            {
                id_empresa = _empresaSeleccionada.id_empresa,
                monto_pagado = montoPagado,
                observacion = texobservacion.Text.Trim(),
                detalle_compra = detalle
            };

            string mensaje =
                $"Total compra: Q {totalCompra:N2}\n" +
                $"Efectivo recibido: Q {efectivoRecibido:N2}\n" +
                $"Monto pagado: Q {montoPagado:N2}\n";

            if (cambioEsperado > 0)
            {
                mensaje +=
                    $"Cambio esperado: Q {cambioEsperado:N2}";
            }
            else
            {
                mensaje +=
                    $"Saldo pendiente: Q {saldoPendiente:N2}";
            }

            DialogResult resultado = MessageBox.Show(
                mensaje + "\n\n¿Desea registrar la compra?",
                "Confirmar compra",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (resultado != DialogResult.Yes)
                return;

            butguardar.Enabled = false;

            try
            {
                await _compraService.CrearCompra(compraDto);

                string mensajeExito =
                    $"Compra registrada correctamente.\n\n" +
                    $"Total: Q {totalCompra:N2}\n" +
                    $"Pagado: Q {montoPagado:N2}\n";

                if (cambioEsperado > 0)
                {
                    mensajeExito +=
                        $"Cambio: Q {cambioEsperado:N2}";
                }
                else
                {
                    mensajeExito +=
                        $"Saldo pendiente: Q {saldoPendiente:N2}";
                }

                MessageBox.Show(
                    mensajeExito,
                    "Compra registrada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                limpiar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al registrar la compra:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                butguardar.Enabled = true;
            }
        }

 

        private void button1_Click(object sender, EventArgs e)
        {
            limpiar();
        }

        private void limpiar()
        {
            texEmpresa.Text = "";
            datacompras.Rows.Clear();
        }

        private void buttodo_Click(object sender, EventArgs e)
        {
            texabonar.Text=textotalcompras.Text;
        }
    }
}
