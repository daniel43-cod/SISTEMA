using SISTEMA_FROTEND.DTOs;
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
    public partial class Catalog : Form
    {
        public event EventHandler<List<ProductoSeleccionadoDTO>>? ProductosSeleccionados;
        private VentaService _ventaService = new VentaService();
        private ProductoCatalogoDTO? _productoActual;
        private readonly CategoriaServices _categoriaServices = new CategoriaServices();
        private List<CategoriaDto> _categorias = new();
        private bool _cargandoCategorias;
        public Catalog()
        {
            InitializeComponent();
            comboCategorias.SelectedIndexChanged += comboCategorias_SelectedIndexChanged;
       
        }


        private async void Catalog_Load(object sender, EventArgs e)
        {
            await CargarCategorias();
            //await CargarCatalogo();

        }


        private async Task CargarCategorias()
        {
            try
            {
                _cargandoCategorias = true;
                _categorias = await _categoriaServices.ListarCategorias();

                _categorias.Insert(0, new CategoriaDto
                {
                    Id = 0,
                    Nombre = "Todos"
                });

                comboCategorias.DataSource = null;
                comboCategorias.DataSource = _categorias;
                comboCategorias.DisplayMember = "Nombre";
                comboCategorias.ValueMember = "Id";
                comboCategorias.SelectedIndex = -1;

                
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cargar categorías",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                _cargandoCategorias = false;
            }
        }

        private async Task CargarCatalogo()
        {
            try
            {
                var productos = await _ventaService.ListarCatalogo();

                flowCatalogo.Controls.Clear();

                foreach (var producto in productos)
                {
                    var tarjeta = new Catalogo();

                    tarjeta.CargarProducto(
                        producto,
                        producto.imagen ?? ""
                    );

                    tarjeta.ProductosAgregados +=
                        Tarjeta_ProductosAgregados;

                    flowCatalogo.Controls.Add(tarjeta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cargar catálogo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
        private void flowCatalogo_Paint(object sender, PaintEventArgs e)
        {

        }
        private void Tarjeta_ProductosAgregados(object? sender, List<ProductoSeleccionadoDTO> productos)
        {
            ProductosSeleccionados?.Invoke(this, productos);
        }

        private async void comboCategorias_SelectedIndexChanged(object sender, EventArgs e)
        {
   
            if (_cargandoCategorias)
                return;

            if (comboCategorias.SelectedIndex < 0)
                return;
            if (comboCategorias.SelectedValue == null)
                return;

            if (!int.TryParse(
                comboCategorias.SelectedValue.ToString(),
                out int idCategoria))
            {
                MessageBox.Show("No se pudo obtener el ID de la categoría.");
                return;
            }
            if(idCategoria == 0)
            {
                await CargarCatalogo();
            }
            else
            {
                await CargarCatalogoPorCategoria(idCategoria);
            }

          

        }


        private async Task CargarCatalogoPorCategoria(int idCategoria)
        {
            try
            {
                var productos =
                    await _categoriaServices.ListarProductoPorCategoria(idCategoria);


                flowCatalogo.Controls.Clear();

                foreach (var producto in productos)
                {
                    var tarjeta = new Catalogo();

                    tarjeta.CargarProducto(
                        producto,
                        producto.imagen ?? string.Empty
                    );

                    tarjeta.ProductosAgregados +=
                        Tarjeta_ProductosAgregados;

                    flowCatalogo.Controls.Add(tarjeta);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al filtrar productos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}



