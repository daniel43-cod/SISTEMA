using SISTEMA_FROTEND.DTOs;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;

namespace SISTEMA_FROTEND.forms
{
    public partial class frmDetalleVenta : Form
    {
        private int _idVenta;
        public frmDetalleVenta()
        {
            InitializeComponent();
        }


        public frmDetalleVenta(int idVenta)
        {
            InitializeComponent();
            _idVenta = idVenta;
        }

        private DetalleService _detalleService = new DetalleService();

        private async void frmDetalleVenta_Load(object sender, EventArgs e)
        {

            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";

            var detalle = await _detalleService.ListarDetalle(_idVenta);

           
            datadetalle.AutoGenerateColumns = true;
            datadetalle.DataSource = detalle;
            datadetalle.Columns["id_producto"].Visible = false;
            datadetalle.Columns["id_producto_presentacion"].Visible = false;



            datadetalle.DefaultCellStyle.ForeColor = Color.Black;
            datadetalle.DefaultCellStyle.BackColor = Color.White;

            datadetalle.DefaultCellStyle.SelectionForeColor = Color.White;
            datadetalle.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            datadetalle.EnableHeadersVisualStyles = false;
            datadetalle.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            datadetalle .ColumnHeadersDefaultCellStyle.BackColor = Color.DarkSlateGray;



        }


        private void posicion_columnas()
        {
            /*
                        // Configuración de columnas
                        datadetalle.Columns["cantidad"].DisplayIndex = 0;
                        datadetalle.Columns["nombre_producto"].DisplayIndex = 1;
                        datadetalle.Columns["precio"].DisplayIndex = 2;
                        datadetalle.Columns["descuento"].DisplayIndex = 3;
                        datadetalle.Columns["subtotal"].DisplayIndex = 4;

                        datadetalle.Columns["id_detalle_venta"].Visible = false;
                        datadetalle.Columns["id_venta"].Visible = false;
                        datadetalle.Columns["id_producto"].Visible = false;

                        datadetalle.Columns["cantidad"].HeaderText = "Cantidad";
                        datadetalle.Columns["nombre_producto"].HeaderText = "Producto";
                        datadetalle.Columns["precio"].HeaderText = "Precio";
                        datadetalle.Columns["descuento"].HeaderText = "Descuento";
                        datadetalle.Columns["subtotal"].HeaderText = "Subtotal";*/
        }

        private void lblUsuario_Click(object sender, EventArgs e)
        {

        }

        private void datadetalle_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
