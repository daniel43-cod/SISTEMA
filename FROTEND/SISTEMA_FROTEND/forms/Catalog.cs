using SISTEMA_FROTEND.DTOs;
using SISTEMA_FROTEND.DTOs.Catalogo;
using SISTEMA_FROTEND.DTOs.Productos;
using SISTEMA_FROTEND.models;
using SISTEMA_FROTEND.services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
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
        private List<ProductoCatalogoDTO> _productos = new();
        public Catalog()
        {
            InitializeComponent();
            comboCategorias.SelectedIndexChanged += comboCategorias_SelectedIndexChanged;
            textBox1.TextChanged += textBox1_TextChanged;
            textBox1.KeyDown += textBox1_KeyDown;
        }


        private async void Catalog_Load(object sender, EventArgs e)
        {
            await CargarCategorias();
            //await CargarCatalogo();

            await CargarProductosEnMemoria();
            //_productos = await _ventaService.ListarCatalogo();

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
            if (idCategoria == 0)
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



        private async Task CargarProductosEnMemoria()
        {
            try
            {
                _productos = await _ventaService.ListarCatalogo();

                var fuente = new AutoCompleteStringCollection();

                fuente.AddRange(
                    _productos
                        .Where(p => !string.IsNullOrWhiteSpace(p.nombre))
                        .Select(p => p.nombre)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(nombre => nombre)
                        .ToArray()
                );

                textBox1.AutoCompleteMode =
                    AutoCompleteMode.SuggestAppend;

                textBox1.AutoCompleteSource =
                    AutoCompleteSource.CustomSource;

                textBox1.AutoCompleteCustomSource =
                    fuente;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cargar productos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            /*string texto = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                flowCatalogo.Controls.Clear();
                return;
            }

            var coincidencias = _productos
                .Where(p =>
                    !string.IsNullOrWhiteSpace(p.nombre) &&
                    p.nombre.Contains(
                        texto,
                        StringComparison.OrdinalIgnoreCase
                    ))
                .ToList();

            MostrarProductos(coincidencias);*/
        }

       

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;

            string texto = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(texto))
                return;

            var producto = _productos.FirstOrDefault(p =>
                p.nombre.Trim().Equals(
                    texto,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (producto == null)
            {
                MessageBox.Show(
                    "No se encontró un producto con ese nombre.",
                    "Buscar producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            MostrarProducto(producto);
        }

        private void MostrarProducto(
    ProductoCatalogoDTO producto)
        {
            flowCatalogo.Controls.Clear();

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
}



