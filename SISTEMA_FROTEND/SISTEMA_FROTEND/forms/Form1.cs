using SISTEMA_FROTEND.forms;
using SISTEMA_FROTEND.presentacion;

namespace SISTEMA_FROTEND
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //separacion de los paneles
            panelMenu.Dock = DockStyle.None;
            panelMenu.Location = new Point(0, 0);
            panelMenu.Size = new Size(225, this.ClientSize.Height);
            panelMenu.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            panelContenido.Dock = DockStyle.None;
            panelContenido.Location = new Point(225, 0);
            panelContenido.Size = new Size(this.ClientSize.Width - 225, this.ClientSize.Height);
            panelContenido.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void btnregistrar_Click(object sender, EventArgs e)
        {
            AbrirFormulario(new cotizacion());
        }

        private void btnregistroventas_Click(object sender, EventArgs e)
        {
            AbrirFormulario(new frmregistro_ventas());
        }

        private void btnproductos_Click(object sender, EventArgs e)
        {
            AbrirFormulario(new frmproductos());
        }

        private void btncreditos_Click(object sender, EventArgs e)
        {


            AbrirFormulario(new creditos());
        }

        private void btnusuarios_Click(object sender, EventArgs e)
        {

            AbrirFormulario(new usuarioscs());
        }

        private void btningreso_Click(object sender, EventArgs e)
        {
            AbrirFormulario(new ingreso());

        }

        private void Form1_Load_2(object sender, EventArgs e)
        {

        }

        private void AbrirFormulario(Form formulario)
        {
            panelContenido.Controls.Clear();

            formulario.TopLevel = false;
            formulario.FormBorderStyle = FormBorderStyle.None;
            formulario.Dock = DockStyle.Fill;

            panelContenido.Controls.Add(formulario);
            panelContenido.Tag = formulario;

            formulario.Show();
        }

        private void panelMenu_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
