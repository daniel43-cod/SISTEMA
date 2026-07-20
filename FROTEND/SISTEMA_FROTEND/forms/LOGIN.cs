using SISTEMA_FROTEND.DTOs;
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
using System.Xml.Serialization;

namespace SISTEMA_FROTEND.forms
{
    public partial class LOGIN : Form
    {
        public LOGIN()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(texusuario.Text) || string.IsNullOrWhiteSpace(texcontraseña.Text))
            {
                MessageBox.Show("Ingresa el Usuario o la Contraseña");
                return;
            }

            button1.Enabled = false;
            try
            {
                var login = new DTOs.Login.LoginDTO
                {
                    usuario = texusuario.Text,
                    password = texcontraseña.Text
                };

                LoginService service = new LoginService();

                var respuesta = await service.Login(login);

                if (respuesta == null)
                {
                    MessageBox.Show("Usuario o contraseña incorrectos");
                    limpiar();
                    return;
                }
                SesionUsuario.IniciarSesion(
                 respuesta.id_usuario,
                 respuesta.nombre,
                 respuesta.rol,
                 respuesta.token

                    );

                Form1 frm = new Form1();
                frm.Show();

                this.Hide();
            }
            catch (HttpRequestException ex) {

                MessageBox.Show("No se pudo conectar con el servidor, intentelo de nuevo o mas tarde");
                limpiar();
                button1.Enabled = true;
                return;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                button1.Enabled = true;
                return;
            }
            catch
            {
                button1.Enabled = true;
            }
           
        }

        private void LOGIN_Load(object sender, EventArgs e)
        {

        }

        private void limpiar()
        {
            texcontraseña.Text = "";
            texusuario.Text = string.Empty;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            limpiar();
        }
    }
}
