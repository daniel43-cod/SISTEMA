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
    public partial class frmproductos : Form
    {
        public frmproductos()
        {
            InitializeComponent();
        }

        private CancellationTokenSource _cts;



        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private async void frmproductos_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";
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

        //metodo para mostrar todos los productos si el usuario no tiene nada escrito en el combuscar
        private async void combuscar_Enter(object sender, EventArgs e)
        {
        }

        private async Task CargarTodosLosProductos()
        {
        }
    }
}
