using SISTEMA_FROTEND.DTOs.Cliente;
using SISTEMA_FROTEND.DTOs.Ventas;
using SISTEMA_FROTEND.models;
using SISTEMA_FROTEND.services;
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

    public partial class formCobro : Form
    {
        private VentaDTOs _venta;
        private decimal _total;

        public formCobro(
            List<DetalleDTOs> detalles,
            ListarClienteDTOs clienteSeleccionado,
            string nombreCliente,
            string nitCliente,
            string telefonoCliente,
            string correoCliente,
            string direccionCliente,
            string dpiCliente,
            int idUsuario,
            decimal subtotal,
            decimal descuentoTotal,
            decimal total)
        {
            InitializeComponent();

            _total = total;

            _venta = new VentaDTOs
            {
                id_usuario = idUsuario,
                origen = "WinForms",
                detalles = detalles
            };

            if (clienteSeleccionado != null)
            {
                _venta.id_cliente = clienteSeleccionado.id_Cliente;
                _venta.clienteNuevo = null;
            }
            else
            {
                _venta.id_cliente = null;
                _venta.clienteNuevo = new ClienteNuevoDTOs
                {
                    nombre = nombreCliente,
                    nit = nitCliente,
                    telefono = telefonoCliente,
                    correo_electronico = correoCliente,
                    direccion = direccionCliente,
                    dpi = dpiCliente
                };  
              
            }

            textotal.Text = _total.ToString("N2");
        }

        private void formCobro_Load(object sender, EventArgs e)
        {
            textotal.Enabled = false;
            textotal.Text = _total.ToString("N2");
        }

        private async void butcobrar_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(textotal.Text, out decimal total))
            {
                MessageBox.Show("El total no es válido.");
                return;
            }

            if (!decimal.TryParse(texefectivo.Text, out decimal efectivo))
            {
                MessageBox.Show("Ingrese el efectivo recibido.");
                return;
            }

            if (efectivo <= 0)
            {
                MessageBox.Show("El efectivo debe ser mayor a 0.");
                return;
            }

            decimal montoPagado;
            decimal cambio = 0;
            decimal saldoPendiente = 0;

            if (efectivo >= total)
            {
                montoPagado = total;
                cambio = efectivo - total;
                saldoPendiente = 0;
            }
            else
            {
                montoPagado = efectivo;
                cambio = 0;
                saldoPendiente = total - efectivo;
            }

            _venta.pago = new PagoVentaDTO
            {
                monto_pagado = montoPagado,
                observacion_pago = texobservacion.Text
            };

            VentaService service = new VentaService();

            try
            {
                await service.CrearVenta(_venta);

                MessageBox.Show(
                    $"Venta realizada correctamente.\n\n" +
                    $"Total: Q {total:N2}\n" +
                    $"Monto pagado: Q {montoPagado:N2}\n" +
                    $"Cambio: Q {cambio:N2}\n" +
                    $"Saldo pendiente: Q {saldoPendiente:N2}"
                );

                DialogResult = DialogResult.OK; // agregado: para que cotizacion sepa que se guardó bien
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

}
