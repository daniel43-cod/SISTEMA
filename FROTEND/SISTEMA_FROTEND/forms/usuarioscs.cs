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
    public partial class usuarioscs : Form
    {

        private readonly CajaService _cajaService;
        public usuarioscs()
        {
            InitializeComponent();
            _cajaService = new CajaService();
        }

        private void usuarioscs_Load(object sender, EventArgs e)
        {
            //  lblUsuario.Text = $"Usuario: {Sesion.Nombre}";


        }


        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private  async void button1_Click(object sender, EventArgs e)
        {

            if (!decimal.TryParse(textmontoinicial.Text, out decimal monto))
            {
                MessageBox.Show("Ingrese un monto inicial válido.");
                return;
            }

            var apertura_caja = new AperturaCajaDT0s
            {
                id_caja= ConfiguracionApp.IdCaja,
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
    }
}
