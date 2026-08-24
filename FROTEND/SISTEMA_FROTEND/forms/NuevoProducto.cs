using SISTEMA_FROTEND.DTOs;
using SISTEMA_FROTEND.DTOs.Productos;
using SISTEMA_FROTEND.helpers;
using SISTEMA_FROTEND.models;
using SISTEMA_FROTEND.services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SISTEMA_FROTEND.presentacion
{
    public partial class NuevoProducto : Form
    {
        public NuevoProducto()
        {
            InitializeComponent();
            ConfigurarComboCategoria();
            timer1.Interval = 3000; // 3000 ms = 3 segundos

            texproducto.KeyDown += EnterComoTab;
            texdescripcion.KeyDown += EnterComoTab;
            texcodigobarras.KeyDown += EnterComoTab;
            //texcantidad.KeyDown += EnterComoTab;
            texminima.KeyDown += EnterComoTab;
            //texpreciocompra.KeyDown += EnterComoTab;
        }
        //metodo para moverse entre los campos de texto con el enter
        private void EnterComoTab(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.SelectNextControl(
                    (Control)sender,
                    true,
                    true,
                    true,
                    true);
                e.SuppressKeyPress = true;
            }
        }

        private void ConfigurarComboCategoria()
        {
            comcategoria.DropDownStyle = ComboBoxStyle.DropDown;
            comcategoria.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comcategoria.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        private string rutaimagen = "";
        private readonly CategoriaServices _categoriaServices = new CategoriaServices();
        private List<CategoriaDto> _categorias = new();



        private readonly ProductoService _service = new();

        private async void button2_Click(object sender, EventArgs e)
        {

  
            if (comcategoria.Text == "")
            {
                MostrarError(labcategoria, "Debe ingresar la categoría del producto");
                return;
            }
          
            if (texproducto.Text == "")
            {
                MostrarError(labproducto, "Ingresa el nombre del producto");
                return;
            }
           

            List<ProductoPresentacionDTO> presentaciones = new();

            foreach (DataGridViewRow fila in dgvPresentaciones.Rows)
            {
                if (fila.IsNewRow) continue;

                if (fila.Cells["descripcion"].Value == null ||
                    fila.Cells["unidades_equivalentes"].Value == null ||
                    fila.Cells["precio"].Value == null)
                {
                    MessageBox.Show("Completa todas las presentaciones.");
                    return;
                }

                presentaciones.Add(new ProductoPresentacionDTO
                {
                    descripcion = fila.Cells["descripcion"].Value.ToString(),
                    unidades_equivalentes = Convert.ToInt32(fila.Cells["unidades_equivalentes"].Value),
                    precio = Convert.ToDecimal(fila.Cells["precio"].Value)
                });
            }

            if (presentaciones.Count == 0)
            {
                MessageBox.Show("Agrega al menos una presentación.");
                return;
            }

            ProductoDTOs producto = new ProductoDTOs
            {
                nombre = texproducto.Text,
                descripcion = texdescripcion.Text,
                stock_minimo = Convert.ToInt32(texminima.Text),
                codigo_barra = texcodigobarras.Text,
                id_categoria = Convert.ToInt32(comcategoria.SelectedValue),
                presentaciones = presentaciones,
                imagen = rutaimagen,

            };

            try
            {
                var respuesta = await _service.CrearProducto(producto);

                if (respuesta == null)
                {
                    MessageBox.Show("No se pudo obtener el ID del producto.");
                    return;
                }

                if (!string.IsNullOrEmpty(producto.imagen))
                {
                    await _service.SubirImagen(respuesta.id_producto, producto.imagen);
                }

                MessageBox.Show("Producto registrado correctamente.");
                limpiar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al registrar el producto: {ex.Message}\n\nDetalles técnicos:\n{ex.StackTrace}",
                    "Detalle del Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }


        }


        private void limpiarlabels()
        {
            labcategoria.Text = "";
            labproducto.Text = "";
            labdescripcion.Text = "";
            labcantidad.Text = "";
            labexistenciamin.Text = "";
        }

        private void MostrarError(Label lbl, string mensaje)
        {
            lbl.Text = mensaje;
            lbl.ForeColor = Color.Red;

            timer1.Stop();
            timer1.Tag = lbl;
            timer1.Start();
        }

        private async void ingreso_Load(object sender, EventArgs e)
        {
            await CargarCategoriasAsync();
            ConfigurarGridPresentaciones();
            lblUsuario.Text = $"Usuario: {Sesion.Nombre}";

        }

        //ingresar la imagen
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "Imagenes|*.jpg;*.jpeg;*.png";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                rutaimagen = openFile.FileName;

                picimagen.Image = Image.FromFile(rutaimagen);
            }
        }

        private async void FrmProductos_Load(object sender, EventArgs e)
        {
            await CargarCategoriasAsync();
        }

        private async Task CargarCategoriasAsync()
        {
            try
            {
                _categorias = await _categoriaServices.ListarCategorias();

                // _categorias.Insert(0, new CategoriaDto { Id = 0, Nombre = "" }); // fila vacía

                comcategoria.DataSource = null;
                comcategoria.DataSource = _categorias;
                comcategoria.DisplayMember = "Nombre";
                comcategoria.ValueMember = "Id";
                comcategoria.SelectedIndex = 0;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"No se pudo conectar con la API: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void limpiar()
        {
            comcategoria.Text = string.Empty;
            texproducto.Text = string.Empty;
            texdescripcion.Text = string.Empty;
            //texcantidad.Text = string.Empty;
            texcodigobarras.Text = string.Empty;
            texminima.Text = string.Empty;
            //  texpreciocompra.Text = string.Empty;
            picimagen.Image = null;
            //texcantidad.Text = string.Empty;
            dgvPresentaciones.Rows.Clear();

        }

        private void bulimpiar_Click(object sender, EventArgs e)
        {
            limpiar();
            limpiarlabels();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timer1.Tag is Label lbl)
            {
                lbl.Text = "";
            }

            timer1.Stop();
        }



        private void butsalir_Click(object sender, EventArgs e)
        {
            Close();
        }



        private void ConfigurarGridPresentaciones()
        {
            dgvPresentaciones.Columns.Clear();
            dgvPresentaciones.AutoGenerateColumns = false;
            dgvPresentaciones.AllowUserToAddRows = true;

            dgvPresentaciones.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "descripcion",
                HeaderText = "Presentación",
                DataPropertyName = "descripcion"
            });

            dgvPresentaciones.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "unidades_equivalentes",
                HeaderText = "Unidades",
                DataPropertyName = "unidades_equivalentes"
            });

            dgvPresentaciones.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "precio",
                HeaderText = "Precio",
                DataPropertyName = "precio"
            });
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void comcategoria_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dgvPresentaciones_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
