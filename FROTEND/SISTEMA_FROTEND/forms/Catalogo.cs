using SISTEMA_FROTEND.DTOs.Catalogo;
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
    public partial class Catalogo : UserControl
    {
        public Catalogo()
        {
            InitializeComponent();


            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void Catalogo_Load(object sender, EventArgs e)
        {
        }

        private void label7_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private ProductoCatalogoDTO? _producto;

        public void CargarProducto(ProductoCatalogoDTO producto)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            _producto = producto;

            // Nombre del producto
            lblproducto.Text = producto.nombre;

            // Stock disponible
            lbldisponible.Text = $"Disponible: {producto.stock}";


            CargarImagen(producto.imagen);

            CargarPresentaciones(producto.presentaciones);

        }


        private void CargarImagen(string? nombreImagen)
        {
            pictureBox1.Image = null;

            if (string.IsNullOrWhiteSpace(nombreImagen))
                return;

            string urlImagen = $"https://localhost:44308/imagenes/productos/{nombreImagen}";

            pictureBox1.LoadAsync(urlImagen);
        }

        private void CargarPresentaciones(List<PresentacionCatalogoDTO> presentaciones)
        {
            Label[] labels =
            {
        lblPresentacion1,
        lblPresentacion2,
        lblPresentacion3,
        lblPresentacion4,
        lblPresentacion5
    };

            // Primero limpiamos y ocultamos todos los labels
            foreach (var label in labels)
            {
                label.Text = string.Empty;
                label.Visible = false;
            }

            // Mostramos máximo 5 presentaciones
            int cantidad = Math.Min(presentaciones.Count, labels.Length);

            for (int i = 0; i < cantidad; i++)
            {
                var presentacion = presentaciones[i];

                labels[i].Text =
                    $"{presentacion.presentacion} - Q{presentacion.precio:N2}";

                labels[i].Visible = true;
            }
        }
    }
}
