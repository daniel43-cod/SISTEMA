using SISTEMA_FROTEND.helpers;
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
        public usuarioscs()
        {
            InitializeComponent();
        }

        private void usuarioscs_Load(object sender, EventArgs e)
        {
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";

        }
    }
}
