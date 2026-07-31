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
        public event EventHandler<List<ProductoSeleccionadoDTO>>? ProductosAgregados;
        private ProductoCatalogoDTO? _productoActual;
        public Catalogo()
        {
            InitializeComponent();
            ConfigurarBotonesCantidad();

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void Catalogo_Load(object sender, EventArgs e)
        {
        }

        private void label7_Click(object sender, EventArgs e)
        {
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


            //CargarImagen(producto.imagen);

            CargarPresentaciones(producto.presentaciones);

        }


     

         public void CargarProducto(ProductoCatalogoDTO producto, string nombreImagen)
          {
              _productoActual = producto;

              lblproducto.Text = producto.nombre;
              lbldisponible.Text = $"Stock: {producto.stock}";

              CargarPresentaciones(producto.presentaciones);

              // Cargar imagen...
              pictureBox1.Image = null;

              if (string.IsNullOrWhiteSpace(nombreImagen))
                  return;

              string urlImagen = $"https://localhost:44308/imagenes/productos/{nombreImagen}";

              pictureBox1.LoadAsync(urlImagen);


          }

        /*
                private void CargarImagen(string? nombreImagen)
                {
                    pictureBox1.Image = null;

                    if (string.IsNullOrWhiteSpace(nombreImagen))
                        return;

                    string urlImagen = $"{nombreImagen}";

                    pictureBox1.LoadAsync(urlImagen);
                }*/

        //Carga las presentacion dependiento cuantos son

        private void CargarPresentaciones(List<PresentacionCatalogoDTO> presentaciones)
        {
            Panel[] paneles =
            {
        panelPresentacion1,
        panelPresentacion2,
        panelPresentacion3,
        panelPresentacion4,
        panelPresentacion5
            };

            Label[] labels =
            {
        lblPresentacion1,
        lblPresentacion2,
        lblPresentacion3,
        lblPresentacion4,
        lblPresentacion5
           };

            // Ocultar todas las filas
            for (int i = 0; i < paneles.Length; i++)
            {
                paneles[i].Visible = false;
            }

            // Mostrar únicamente las que existan
            for (int i = 0; i < presentaciones.Count && i < paneles.Length; i++)
            {
                paneles[i].Visible = true;

                labels[i].Text =
                    $"{presentaciones[i].presentacion} - Q{presentaciones[i].precio:N2}";
            }
        }




        private void ConfigurarBotonesCantidad()
        {
            Button[] botonesMas =
            {btnMas1, btnMas2,btnMas3,btnMas4,btnMas5 };

            Button[] botonesMenos =
            { btnMenos1,btnMenos2, btnMenos3,btnMenos4,btnMenos5 };

            TextBox[] cantidades =
            {txtCantidad1,txtCantidad2,txtCantidad3,txtCantidad4,txtCantidad5};

            for (int i = 0; i < 5; i++)
            {
                botonesMas[i].Tag = cantidades[i];
                botonesMenos[i].Tag = cantidades[i];

                botonesMas[i].Click += BotonMas_Click;
                botonesMenos[i].Click += BotonMenos_Click;

                cantidades[i].Text = "0";
                cantidades[i].TextAlign = HorizontalAlignment.Center;
            }
        }



        private void BotonMas_Click(object? sender, EventArgs e)
        {
            if (sender is not Button boton)
                return;

            if (boton.Tag is not TextBox txtCantidad)
                return;

            int cantidadActual = 0;

            int.TryParse(txtCantidad.Text, out cantidadActual);

            cantidadActual++;

            txtCantidad.Text = cantidadActual.ToString();
        }


        private void BotonMenos_Click(object? sender, EventArgs e)
        {
            if (sender is not Button boton)
                return;

            if (boton.Tag is not TextBox txtCantidad)
                return;

            int cantidadActual = 0;

            int.TryParse(txtCantidad.Text, out cantidadActual);

            if (cantidadActual > 0)
            {
                cantidadActual--;
            }

            txtCantidad.Text = cantidadActual.ToString();
        }

        private void btnAgregarVenta_Click(object sender, EventArgs e)
        {
          

            if (_productoActual == null)
            {
                return;
            }
            var listaProductos = new List<ProductoSeleccionadoDTO>();

            TextBox[] cantidades =
            {
        txtCantidad1,
        txtCantidad2,
        txtCantidad3,
        txtCantidad4,
        txtCantidad5
         };

            int cantidadPresentaciones = Math.Min(
                _productoActual.presentaciones.Count,
                cantidades.Length
            );

            for (int i = 0; i < cantidadPresentaciones; i++)
            {
                if (!int.TryParse(cantidades[i].Text, out int cantidad))
                    continue;

                if (cantidad <= 0)
                    continue;

                var presentacion = _productoActual.presentaciones[i];

                int unidadesSolicitadas =
                    cantidad * presentacion.unidades_equivalentes;

                if (unidadesSolicitadas > _productoActual.stock)
                {
                    MessageBox.Show(
                        $"Stock insuficiente para {_productoActual.nombre} - " +
                        $"{presentacion.presentacion}.\n\n" +
                        $"Stock disponible: {_productoActual.stock} unidades.\n" +
                        $"Unidades solicitadas: {unidadesSolicitadas}.",
                        "Stock insuficiente",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                listaProductos.Add(new ProductoSeleccionadoDTO
                {
                    id_producto = _productoActual.id_producto,

                    id_producto_presentacion =
                        presentacion.id_producto_presentacion,

                    nombre_producto = _productoActual.nombre,

                    presentacion = presentacion.presentacion,

                    cantidad = cantidad,

                    precio = presentacion.precio,

                    unidades_equivalentes =
                        presentacion.unidades_equivalentes,

                    stock = _productoActual.stock
                });
            }

            if (listaProductos.Count == 0)
            {
                MessageBox.Show(
                    "Seleccione al menos una cantidad.",
                    "Catálogo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            ProductosAgregados?.Invoke(this, listaProductos);

            MessageBox.Show("Producto Agregado Correctamente", "Catalogo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
                );

            limpiar();
        }   


        private void limpiar()
        {
            txtCantidad1.Text = "";
            txtCantidad2.Text = "";
            txtCantidad3.Text = "";
            txtCantidad4.Text = "";
            txtCantidad5.Text = "";

        }
    }
}
