using SISTEMA_FROTEND.DTOs;
using SISTEMA_FROTEND.helpers;
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

            var login = new LoginDTO
            {
                usuario = texusuario.Text,
                password =texcontraseña.Text
            };

            LoginService service = new LoginService();

            var respuesta = await service.Login(login);

            if (respuesta == null)
            {
                MessageBox.Show("Usuario o contraseña incorrectos");
                return;
            }

            Sesion.IdUsuario = respuesta.id_usuario;
            Sesion.Nombre = respuesta.nombre;
            Sesion.Rol = respuesta.rol;
            Sesion.Token = respuesta.token;

            Form1 frm = new Form1();
            frm.Show();

            this.Hide();
        }
    }
}
