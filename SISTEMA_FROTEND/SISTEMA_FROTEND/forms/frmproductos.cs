using SISTEMA_FROTEND.DTOs;
using SISTEMA_FROTEND.DTOs.Productos;
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

namespace SISTEMA_FROTEND.forms
{
    public partial class frmcompras : Form
    {
        private EmpresaService _empresaService = new EmpresaService();
        private List<EmpresaDTOs> _empresas;
        private EmpresaDTOs _empresaSeleccionada;
        public frmcompras()
        {
            InitializeComponent();
        }

        private CancellationTokenSource _cts;



        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private async void frmproductos_Load(object sender, EventArgs e)
        {
            _empresas = await _empresaService.ListarEmpresas();

            var fuenteEmpresas = new AutoCompleteStringCollection();
            fuenteEmpresas.AddRange(_empresas.Select(e => e.nombre_empresa).ToArray());

            texEmpresa.AutoCompleteMode = AutoCompleteMode.Suggest;
            texEmpresa.AutoCompleteSource = AutoCompleteSource.CustomSource;
            texEmpresa.AutoCompleteCustomSource = fuenteEmpresas;
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
            var empresa = _empresas.FirstOrDefault(x =>
                x.nombre_empresa.Trim().Equals(texEmpresa.Text.Trim(), StringComparison.OrdinalIgnoreCase));

            if (empresa == null)
            {
                MessageBox.Show("Seleccioná una empresa válida de la lista.");
                _empresaSeleccionada = null;
                texEmpresa.Text = "";
                return;
            }

            _empresaSeleccionada = empresa;
        }
    }
}
