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

namespace SISTEMA_FROTEND.forms
{
    public partial class frmcompras : Form
    {
        private ProductoService _productoService = new ProductoService();
        private List<ProductoVentaBuscarDTO> _productos=new ();    
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
            MessageBox.Show("Entró al autocompletado");

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
                MessageBox.Show("Seleccioná una empresa válida antes de guardar.");
                return;
            }

            var detalle = new List<DetalleCompraDTOs>();

            foreach (DataGridViewRow fila in datacompras.Rows)
            {
                if (fila.IsNewRow) continue;
                if (fila.Tag is not ProductoVentaBuscarDTO producto) continue;

                decimal cantidad = Convert.ToDecimal(fila.Cells["cantidad"].Value ?? 0);
                decimal precio = Convert.ToDecimal(fila.Cells["precio"].Value ?? 0);

                if (cantidad <= 0) continue;

                detalle.Add(new DetalleCompraDTOs
                {
                    id_producto = producto.id_producto,
                    cantidad = (int)cantidad,
                    precio = precio
                });
            }

            if (detalle.Count == 0)
            {
                MessageBox.Show("Agregá al menos un producto antes de guardar.");
                return;
            }

            var compraDto = new ComprasDTOs
            {
                id_usuario = Sesion.IdUsuario,
                id_empresa = _empresaSeleccionada.id_empresa,
                id_estado_compra = 1, // ajustar según tus estados reales
                detalle_compra = detalle
            };

            butguardar.Enabled = false;

            try
            {
                var compra = await _compraService.CrearCompra(compraDto);
                MessageBox.Show("Compra guardada correctamente.");
                // acá podés limpiar el formulario
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la compra: {ex.Message}");
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
    }
}
