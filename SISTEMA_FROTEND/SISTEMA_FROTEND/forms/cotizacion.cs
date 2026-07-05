using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.helpers;
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
    public partial class cotizacion : Form
    {

        public cotizacion()
        {
            InitializeComponent();
        }

        private bool cargandoClientes = false;
        private List<ClienteBuscarDTOs> clientesEncontrados = new();
        private ClienteService clienteService = new ClienteService();

        /*  private List<ClienteBuscarDTOs> clientesEncontrados = new();
          private ClienteService clienteService = new ClienteService();*/
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btncotizar_Click(object sender, EventArgs e)
        {

        }


        private void cotizacion_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";

            comCliente.DropDownStyle = ComboBoxStyle.DropDown;
            comCliente.AutoCompleteMode = AutoCompleteMode.None;
            comCliente.AutoCompleteSource = AutoCompleteSource.None;

            texapellido.Visible = false;
            labapellido.Visible = false;


        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }


        private async void comCliente_SelectedIndexChanged(object sender, EventArgs e)
        {/*
            if (cargandoClientes)
                return;

            if (comCliente.SelectedItem is ClienteBuscarDTOs cliente)
            {
                texnit.Text = cliente.nit;
                textelefono.Text = cliente.telefono;
                texcorreo.Text = cliente.correo_electronico;
                texdireccion.Text = cliente.direccion;
                texdpi.Text = cliente.dpi;
            }*/
        }


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

        private void comCliente_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comCliente.SelectedItem is ClienteBuscarDTOs cliente)
            {
              //  texapellido.Text = cliente.apellido;
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

        private void button2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && comCliente.SelectedItem is ClienteBuscarDTOs cliente)
            {
                LlenarDatosCliente(cliente);
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
    }
}
