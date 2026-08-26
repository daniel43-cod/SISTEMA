using SISTEMA_FROTEND.DTOs.Caja;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.services;
using SISTEMA_FROTEND.Utilidades;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SISTEMA_FROTEND
{
    public partial class Caja : Form
    {

        private readonly CajaService _cajaService;
        public Caja()
        {
            InitializeComponent();
            _cajaService = new CajaService();
        }

        private void usuarioscs_Load(object sender, EventArgs e)
        {

        
        }



        private void ocultarcolumnas()
        {
           dataMovimientos.Columns["id_movimiento_caja"].Visible = false;
           dataMovimientos.Columns["id_sesion_caja"].Visible = false;
           dataMovimientos.Columns["id_tipo_movimiento"].Visible = false;
           dataMovimientos.Columns["id_usuario"].Visible = false;
           dataMovimientos.Columns["id_venta"].Visible = false;
           dataMovimientos.Columns["id_compra"].Visible = false;
           dataMovimientos.Columns["id_pago_venta"].Visible = false;
            dataMovimientos.Columns["id_pago_compra"].Visible = false;
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {


            if (!decimal.TryParse(texmontocontado.Text, out decimal montoContado))
            {
                MessageBox.Show(
                    "Ingrese un monto contado válido.",
                    "Cerrar caja",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            var cierreCaja = new CierreCajaDTOs
            {
                monto_contado = montoContado,
                observacion_cierre =
                    texobservacioncierre.Text.Trim()
            };

            try
            {
                var resultado =
                    await _cajaService.CerrarCaja(cierreCaja);

                MessageBox.Show(
                    $"{resultado.mensaje}\n\n" +
                    $"Caja: {ConfiguracionApp.NombreCaja}\n" +
                    $"Sesión: {resultado.id_sesion_caja}\n" +
                    $"Fecha de apertura: {resultado.fecha_apertura:dd/MM/yyyy HH:mm}\n" +
                    $"Fecha de cierre: {resultado.fecha_cierre:dd/MM/yyyy HH:mm}\n\n" +
                    $"Monto inicial: Q{resultado.monto_inicial:N2}\n" +
                    $"Monto esperado: Q{resultado.monto_esperado:N2}\n" +
                    $"Monto contado: Q{resultado.monto_contado:N2}\n" +
                    $"Diferencia: Q{resultado.diferencia:N2}",
                    "Cierre de caja",
                    MessageBoxButtons.OK,
                    resultado.diferencia == 0
                        ? MessageBoxIcon.Information
                        : MessageBoxIcon.Warning
                );

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cerrar la caja",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private async void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {

            if (!decimal.TryParse(textmontoinicial.Text, out decimal monto))
            {
                MessageBox.Show("Ingrese un monto inicial válido.");
                return;
            }

            var apertura_caja = new AperturaCajaDT0s
            {
                id_caja = ConfiguracionApp.IdCaja,
                monto_inicial = decimal.Parse(textmontoinicial.Text),
                observacion = texobservacionapertura.Text

            };

            try
            {
                await _cajaService.AbrirCaja(apertura_caja);

                MessageBox.Show(
                    $"La {ConfiguracionApp.NombreCaja} fue abierta correctamente."
                );

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al abrir la caja",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {
            if (!ConfiguracionApp.EstaConfigurada())
            {
                MessageBox.Show(
                    "Este dispositivo aún no tiene una caja o servidor configurado."
                );

                Close();
                return;
            }

            lblCaja.Text = ConfiguracionApp.NombreCaja;
        }

        private async void tabcaja_SelectedIndexChanged(object sender, EventArgs e)
        {


            if (tabcaja.SelectedTab?.Name == "tabPage3")
            {
                await CargarMovimientosCaja();
                ocultarcolumnas();
            }

            try
            {
                var sesiones = await _cajaService.ListarSesiones();


                datasesiones.AutoGenerateColumns = true;
                datasesiones.DataSource = null;
                datasesiones.DataSource = sesiones;

                datasesiones.Columns["id_sesion_caja"].Visible = false;
                datasesiones.Columns["id_caja"].Visible = false;
                datasesiones.Columns["id_usuario_apertura"].Visible = false;
                datasesiones.Columns["id_usuario_cierre"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cargar sesiones",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

           


        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        // cargar movimientos de caja
        private async Task CargarMovimientosCaja()
        {
            try
            {
                var movimientos =
                    await _cajaService.ListarMovimientoCaja();

                dataMovimientos.AutoGenerateColumns = true;

                dataMovimientos.DataSource = null;
                dataMovimientos.DataSource = movimientos;

                ocultarcolumnas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cargar movimientos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
