using SISTEMA_FROTEND.helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SISTEMA_FROTEND.presentacion
{
    public partial class creditos : Form
    {
        public creditos()
        {
            InitializeComponent();
        }

        private void creditos_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";

        }
    }
}
