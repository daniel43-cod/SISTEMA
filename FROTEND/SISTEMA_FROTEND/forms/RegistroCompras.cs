using SISTEMA_FROTEND.DTOs;
using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.DTOs.Compras;
using SISTEMA_FROTEND.forms;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.models;
using SISTEMA_FROTEND.services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SISTEMA_FROTEND.presentacion
{
    public partial class RegistroCompras : Form
    {
        private readonly CompraService _compraService =
            new CompraService();

        private readonly EmpresaService _empresaService =
            new EmpresaService();

        private List<ListarComprasDTOs> _compras = new();
        private List<EmpresaDTOs> _empresas = new();

        public RegistroCompras()
        {
            InitializeComponent();

            dataGridView1.CellContentClick +=
                dataGridView1_CellContentClick;

            comproveedor.TextUpdate +=
                comproveedor_TextUpdate;
        }

        private async void creditos_Load(
            object sender,
            EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";

            await CargarDatos();
            ColumnasOcultas();
            AgregarBotonDetalle();

            dataGridView1.DefaultCellStyle.ForeColor =
                Color.Black;

            dataGridView1.DefaultCellStyle.BackColor =
                Color.White;

            dataGridView1.DefaultCellStyle.SelectionForeColor =
                Color.White;

            dataGridView1.DefaultCellStyle.SelectionBackColor =
                Color.DodgerBlue;

            dataGridView1.EnableHeadersVisualStyles = false;

            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor =
                Color.DarkSlateGray;

            await CargarProveedor();
        }

        private async Task CargarDatos()
        {
            try
            {
                _compras =
                    await _compraService.ListarCompras();

                dataGridView1.AutoGenerateColumns = true;

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = _compras;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al listar compras",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private async Task CargarProveedor()
        {
            try
            {
                _empresas =
                    await _empresaService.ListarEmpresas();

                comproveedor.DataSource = null;

                comproveedor.DisplayMember =
                    "nombre_empresa";

                comproveedor.ValueMember =
                    "id_empresa";

                comproveedor.DataSource =
                    _empresas;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar proveedores: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ColumnasOcultas()
        {
            if (dataGridView1.Columns["id_compra"] != null)
                dataGridView1.Columns["id_compra"].Visible = false;

            if (dataGridView1.Columns["id_usuario"] != null)
                dataGridView1.Columns["id_usuario"].Visible = false;

            if (dataGridView1.Columns["id_empresa"] != null)
                dataGridView1.Columns["id_empresa"].Visible = false;

            if (dataGridView1.Columns["id_estado_compra"] != null)
                dataGridView1.Columns["id_estado_compra"].Visible = false;
        }

        private void AgregarBotonDetalle()
        {
            if (dataGridView1.Columns["Detalle"] != null)
                return;

            var botonDetalle =
                new DataGridViewButtonColumn
                {
                    Name = "Detalle",
                    HeaderText = "Acción",
                    Text = "Ver detalle",
                    UseColumnTextForButtonValue = true
                };

            dataGridView1.Columns.Add(
                botonDetalle
            );
        }

        private void dataGridView1_CellContentClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
                return;

            if (dataGridView1
                .Columns[e.ColumnIndex]
                .Name != "Detalle")
                return;

            int idCompra =
                Convert.ToInt32(
                    dataGridView1
                        .Rows[e.RowIndex]
                        .Cells["id_compra"]
                        .Value
                );

            var frm =
                new formdetallecompra(idCompra);

            frm.ShowDialog();
        }

        private void comproveedor_SelectionChangeCommitted(
            object sender,
            EventArgs e)
        {
            if (comproveedor.SelectedItem
                is not EmpresaDTOs empresa)
                return;

            var comprasFiltradas =
                _compras
                    .Where(c =>
                        c.id_empresa ==
                        empresa.id_empresa
                    )
                    .ToList();

            dataGridView1.DataSource = null;
            dataGridView1.DataSource =
                comprasFiltradas;

            ColumnasOcultas();
        }

        private void comproveedor_TextUpdate(
            object sender,
            EventArgs e)
        {
            string texto =
                comproveedor.Text.Trim();

            int posicionCursor =
                comproveedor.SelectionStart;

            var filtrados =
                _empresas
                    .Where(e =>
                        (
                            e.nombre_empresa != null &&
                            e.nombre_empresa.Contains(
                                texto,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                        ||
                        (
                            e.nit != null &&
                            e.nit.Contains(
                                texto,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
                    .ToList();

            comproveedor.DataSource = null;

            comproveedor.DisplayMember =
                "nombre_empresa";

            comproveedor.ValueMember =
                "id_empresa";

            comproveedor.DataSource =
                filtrados;

            comproveedor.Text =
                texto;

            comproveedor.SelectionStart =
                Math.Min(
                    posicionCursor,
                    comproveedor.Text.Length
                );

            comproveedor.SelectionLength = 0;

            comproveedor.DroppedDown = true;
        }
    }
}
