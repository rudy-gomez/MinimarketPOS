using System.Drawing;
using System.Windows.Forms;
using MinimarketPOS.Models;
using MinimarketPOS.Services;
using MinimarketPOS.Utils;

namespace MinimarketPOS.Forms
{
    public class PanelInventario : Panel
    {
        private Usuario usuarioActual;
        private DataGridView dgvProductos;
        private TextBox txtBuscar;
        private Button btnBuscar;
        private Button btnNuevoProducto;
        private Button btnImportarExcel;
        private Button btnExportarExcel;

        // Botones de filtro
        private Button btnTodos;
        private Button btnStockBajo;
        private Button btnProximoVencer;
        private Button btnVencidos;

        // Labels de alertas
        private Label lblAlertaStockBajo;
        private Label lblAlertaProximoVencer;
        private Label lblAlertaVencidos;

        private string filtroActivo = "TODOS";
        private List<Producto> productosActuales;

        private int paginaActual = 1;
        private int registrosPorPagina = 20; // Puedes cambiar a 10, 15, 25, etc.
        private int totalPaginas = 0;
        private int totalRegistros = 0;

        private Label lblPaginacion;
        private Button btnPaginaAnterior;
        private Button btnPaginaSiguiente;

        public PanelInventario(Usuario usuario)
        {
            this.usuarioActual = usuario;
            this.BackColor = Color.FromArgb(240, 242, 245); // ← Mover aquí
            InitializeComponent();
            CargarProductos();
            ActualizarAlertas();
        }

        private void InitializeComponent()
        {
            // ← ELIMINAR: this.Text, this.Size, this.StartPosition
            // Ya no son necesarios porque es un Panel, no un Form

            // Panel superior
            Panel panelSuperior = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // Título
            Label lblTitulo = new Label
            {
                Text = "📦 GESTIÓN DE INVENTARIO",
                Font = new Font("Arial", 24, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(700, 40),
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            // Búsqueda
            txtBuscar = new TextBox
            {
                Location = new Point(20, 75),
                Size = new Size(400, 35),
                Font = new Font("Arial", 12),
                PlaceholderText = "Buscar por código o nombre..."
            };
            txtBuscar.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    BtnBuscar_Click(s, e);
                }
            };

            btnBuscar = new Button
            {
                Text = "🔍 BUSCAR",
                Location = new Point(430, 73),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Click += BtnBuscar_Click;

            // Botón Exportar Excel (SIEMPRE visible)
            if (usuarioActual.EsAdmin())
            {
                btnExportarExcel = new Button
                {
                    Text = "📤 Exportar Excel",
                    Location = new Point(usuarioActual.EsAdmin() ? 820 : 580, 73),
                    Size = new Size(190, 40),
                    BackColor = Color.FromArgb(22, 160, 133),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Arial", 11, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnExportarExcel.FlatAppearance.BorderSize = 0;
                btnExportarExcel.Click += BtnExportarExcel_Click;
            }
            // Botones de acción (solo si es admin)
            if (usuarioActual.EsAdmin())
            {
                btnNuevoProducto = new Button
                {
                    Text = "+ Nuevo Producto",
                    Location = new Point(1020, 73),
                    Size = new Size(180, 40),
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnNuevoProducto.FlatAppearance.BorderSize = 0;
                btnNuevoProducto.Click += BtnNuevoProducto_Click;
                panelSuperior.Controls.Add(btnNuevoProducto);

                btnImportarExcel = new Button
                {
                    Text = "📥 Importar Excel",
                    Location = new Point(1220, 73),
                    Size = new Size(180, 40),
                    BackColor = Color.FromArgb(155, 89, 182),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Arial", 10, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnImportarExcel.FlatAppearance.BorderSize = 0;
                btnImportarExcel.Click += BtnImportarExcel_Click;
                panelSuperior.Controls.Add(btnImportarExcel);
            }

            // Botones de filtro
            int xPos = 20;
            btnTodos = CrearBotonFiltro("TODOS", xPos, true);
            xPos += 120;

            btnStockBajo = CrearBotonFiltro("🔴 STOCK BAJO", xPos, false);
            btnStockBajo.BackColor = Color.FromArgb(231, 76, 60);
            xPos += 160;

            btnProximoVencer = CrearBotonFiltro("⚠️ PRÓXIMO A VENCER", xPos, false);
            btnProximoVencer.BackColor = Color.FromArgb(243, 156, 18);
            xPos += 220;

            btnVencidos = CrearBotonFiltro("❌ VENCIDOS", xPos, false);
            btnVencidos.BackColor = Color.FromArgb(192, 57, 43);
            btnVencidos.Width = 180;

            panelSuperior.Controls.Add(lblTitulo);
            panelSuperior.Controls.Add(txtBuscar);
            panelSuperior.Controls.Add(btnBuscar);
            panelSuperior.Controls.Add(btnExportarExcel);
            panelSuperior.Controls.Add(btnTodos);
            panelSuperior.Controls.Add(btnStockBajo);
            panelSuperior.Controls.Add(btnProximoVencer);
            panelSuperior.Controls.Add(btnVencidos);

            // ========== PANEL DE ALERTAS (ARRIBA) ==========
            // Panel principal de alertas
            Panel panelAlertas = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(255, 243, 224),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15, 15, 15, 15)
            };

            // Contenedor horizontal
            FlowLayoutPanel flowAlertas = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false
            };

            // Título
            Label lblTituloAlertas = new Label
            {
                Text = "⚠️ ALERTAS DEL SISTEMA:",
                Font = new Font("Arial", 11, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 5, 20, 0)
            };

            // Alertas
            lblAlertaStockBajo = new Label
            {
                Text = "• 0 productos con stock bajo",
                Font = new Font("Arial", 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(192, 57, 43),
                Margin = new Padding(0, 5, 25, 0)
            };

            lblAlertaProximoVencer = new Label
            {
                Text = "• 0 productos próximos a vencer",
                Font = new Font("Arial", 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(211, 84, 0),
                Margin = new Padding(0, 5, 25, 0)
            };

            lblAlertaVencidos = new Label
            {
                Text = "• 0 productos vencidos",
                Font = new Font("Arial", 10),
                AutoSize = true,
                ForeColor = Color.FromArgb(120, 40, 31),
                Margin = new Padding(0, 5, 0, 0)
            };

            // Agregar controles
            flowAlertas.Controls.Add(lblTituloAlertas);
            flowAlertas.Controls.Add(lblAlertaStockBajo);
            flowAlertas.Controls.Add(lblAlertaProximoVencer);
            flowAlertas.Controls.Add(lblAlertaVencidos);

            panelAlertas.Controls.Add(flowAlertas);


            // ========== PANEL CONTENEDOR CON BORDE ==========
            Panel panelContenedor = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40), // Márgenes externos
                BackColor = Color.FromArgb(240, 242, 245)
            };

            // ========== PANEL CON BORDE (TABLA + PAGINACIÓN) ==========
            Panel panelTabla = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // DataGridView (SIN Dock.Fill, con tamaño específico)
            dgvProductos = new DataGridView
            {
                Dock = DockStyle.Fill, // Ocupará todo excepto el panel inferior
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoGenerateColumns = false,
                ColumnHeadersHeight = 45,
                RowTemplate = { Height = 40 },
                AllowUserToResizeRows = false
            };

            dgvProductos.EnableHeadersVisualStyles = false;
            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Bold);
            dgvProductos.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvProductos.DefaultCellStyle.Font = new Font("Arial", 10);
            dgvProductos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvProductos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 241);

            // ========== PANEL DE PAGINACIÓN (ABAJO DENTRO DEL BORDE) ==========
            Panel panelPaginacion = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(245, 245, 245),
                BorderStyle = BorderStyle.None
            };

            // Calcular posiciones centradas (asumiendo ancho de ~1320px del panel)
            int anchoPanelEstimado = 1320;
            int anchoBoton = 130;
            int anchoLabel = 300;
            int espaciado = 15;
            int anchoTotal = (anchoBoton * 2) + anchoLabel + (espaciado * 2);
            int inicioX = (anchoPanelEstimado - anchoTotal) / 2;

            btnPaginaAnterior = new Button
            {
                Text = "◀ Anterior",
                Location = new Point(inicioX, 12),
                Size = new Size(anchoBoton, 36),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnPaginaAnterior.FlatAppearance.BorderSize = 0;
            btnPaginaAnterior.Click += BtnPaginaAnterior_Click;

            lblPaginacion = new Label
            {
                Text = "Página 1 de 1",
                Location = new Point(inicioX + anchoBoton + espaciado, 12),
                Size = new Size(anchoLabel, 36),
                Font = new Font("Arial", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(52, 73, 94)
            };

            btnPaginaSiguiente = new Button
            {
                Text = "Siguiente ▶",
                Location = new Point(inicioX + anchoBoton + anchoLabel + (espaciado * 2), 12),
                Size = new Size(anchoBoton, 36),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnPaginaSiguiente.FlatAppearance.BorderSize = 0;
            btnPaginaSiguiente.Click += BtnPaginaSiguiente_Click;
            panelPaginacion.Controls.Add(btnPaginaAnterior);
            panelPaginacion.Controls.Add(lblPaginacion);
            panelPaginacion.Controls.Add(btnPaginaSiguiente);

            // ========== ENSAMBLAR TODO ==========
            panelTabla.Controls.Add(dgvProductos);
            panelTabla.Controls.Add(panelPaginacion);
            panelContenedor.Controls.Add(panelTabla);

            ConfigurarColumnas();

            this.Controls.Add(panelContenedor);
            this.Controls.Add(panelAlertas);
            this.Controls.Add(panelSuperior);
        }

        private Button CrearBotonFiltro(string texto, int x, bool seleccionado)
        {
            var btn = new Button
            {
                Text = texto,
                Location = new Point(x, 135),
                Size = new Size(texto.Length > 15 ? 230 : texto.Length > 10 ? 170 : 130, 40),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = seleccionado ? Color.White : Color.FromArgb(189, 195, 199),
                ForeColor = seleccionado ? Color.Black : Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = seleccionado ? 2 : 0;
            btn.FlatAppearance.BorderColor = Color.FromArgb(52, 152, 219);
            btn.Click += BtnFiltro_Click;

            return btn;
        }

        private void ConfigurarColumnas()
        {
            dgvProductos.Columns.Clear();
            dgvProductos.DataBindingComplete += DgvProductos_DataBindingComplete;

            var colNumero = new DataGridViewTextBoxColumn
            {
                Name = "Numero",
                HeaderText = "#",
                Width = 50,
                ReadOnly = true,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            // ← NO PONER DataPropertyName aquí
            dgvProductos.Columns.Add(colNumero);

            // ID (oculto)
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                DataPropertyName = "Id",
                Visible = false
            });

            // Código
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Codigo",
                HeaderText = "CÓDIGO",
                DataPropertyName = "CodigoBarras",
                Width = 130,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Nombre
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nombre",
                HeaderText = "NOMBRE",
                DataPropertyName = "Nombre",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                Width = 200
            });

            // Categoría
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Categoria",
                HeaderText = "CATEGORÍA",
                DataPropertyName = "Categoria",
                Width = 120
            });

            // Precio
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Precio",
                HeaderText = "PRECIO",
                DataPropertyName = "Precio",
                Width = 90,
                DefaultCellStyle = {
                    Alignment = DataGridViewContentAlignment.MiddleRight,
                    Format = "C2"
                }
            });

            // Stock
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Stock",
                HeaderText = "STOCK",
                DataPropertyName = "Stock",
                Width = 80,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Stock Mínimo
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "StockMinimo",
                HeaderText = "STOCK MÍN",
                DataPropertyName = "StockMinimo",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Vencimiento
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FechaVencimiento",
                HeaderText = "VENCIMIENTO",
                Width = 150,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Botón Editar (solo admin)
            if (usuarioActual.EsAdmin())
            {
                var btnEditar = new DataGridViewButtonColumn
                {
                    Name = "Editar",
                    HeaderText = "EDITAR",
                    Text = "✏️",
                    UseColumnTextForButtonValue = true,
                    Width = 80
                };
                btnEditar.DefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
                btnEditar.DefaultCellStyle.ForeColor = Color.White;
                dgvProductos.Columns.Add(btnEditar);

                // Botón Desactivar
                var btnDesactivar = new DataGridViewButtonColumn
                {
                    Name = "Desactivar",
                    HeaderText = "DESACTIVAR",
                    Text = "🚫",
                    UseColumnTextForButtonValue = true,
                    Width = 140
                };
                btnDesactivar.DefaultCellStyle.BackColor = Color.FromArgb(243, 156, 18);
                btnDesactivar.DefaultCellStyle.ForeColor = Color.White;
                dgvProductos.Columns.Add(btnDesactivar);

                // Botón Eliminar
                var btnEliminar = new DataGridViewButtonColumn
                {
                    Name = "Eliminar",
                    HeaderText = "ELIMINAR",
                    Text = "🗑️",
                    UseColumnTextForButtonValue = true,
                    Width = 100
                };
                btnEliminar.DefaultCellStyle.BackColor = Color.FromArgb(231, 76, 60);
                btnEliminar.DefaultCellStyle.ForeColor = Color.White;
                dgvProductos.Columns.Add(btnEliminar);
            }

            dgvProductos.CellContentClick += DgvProductos_CellContentClick;
            dgvProductos.CellFormatting += DgvProductos_CellFormatting;
        }
        private void DgvProductos_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            int inicio = ((paginaActual - 1) * registrosPorPagina) + 1;

            for (int i = 0; i < dgvProductos.Rows.Count; i++)
            {
                dgvProductos.Rows[i].Cells["Numero"].Value = inicio + i;
            }
        }

        private void DgvProductos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var producto = (Producto)dgvProductos.Rows[e.RowIndex].DataBoundItem;

            // Formatear fecha de vencimiento
            if (dgvProductos.Columns[e.ColumnIndex].Name == "FechaVencimiento")
            {
                if (producto.FechaVencimiento.HasValue)
                {
                    DateTime venc = producto.FechaVencimiento.Value;
                    TimeSpan diferencia = venc - DateTime.Now;

                    if (diferencia.TotalDays < 0)
                    {
                        e.Value = "❌ VENCIDO";
                        e.CellStyle.BackColor = Color.FromArgb(255, 220, 220);
                        e.CellStyle.ForeColor = Color.FromArgb(192, 57, 43);
                    }
                    else if (diferencia.TotalDays <= 30)
                    {
                        e.Value = $"⚠️ {(int)diferencia.TotalDays} días";
                        e.CellStyle.BackColor = Color.FromArgb(255, 243, 205);
                        e.CellStyle.ForeColor = Color.FromArgb(243, 156, 18);
                    }
                    else
                    {
                        e.Value = venc.ToString("dd/MM/yyyy");
                    }
                }
                else
                {
                    e.Value = "Sin venc.";
                    e.CellStyle.ForeColor = Color.Gray;
                }
                e.FormattingApplied = true;
            }

            // Resaltar stock bajo
            if (dgvProductos.Columns[e.ColumnIndex].Name == "Stock")
            {
                if (producto.Stock <= producto.StockMinimo)
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 220, 220);
                    e.CellStyle.ForeColor = Color.FromArgb(192, 57, 43);
                    e.CellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
                }
            }
        }

        private void CargarProductos()
        {
            try
            {
                productosActuales = ProductoService.ListarTodos();
                AplicarFiltro();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarFiltro()
        {
            List<Producto> productosFiltrados;

            switch (filtroActivo)
            {
                case "🔴 STOCK BAJO":
                    productosFiltrados = ProductoService.ObtenerConStockBajo();
                    break;
                case "⚠️ PRÓXIMO A VENCER":
                    productosFiltrados = ProductoService.ObtenerProximosAVencer();
                    break;
                case "❌ VENCIDOS":
                    productosFiltrados = ProductoService.ObtenerVencidos();
                    break;
                default: // TODOS
                    productosFiltrados = productosActuales;
                    break;
            }

            // ← CALCULAR PAGINACIÓN
            totalRegistros = productosFiltrados.Count;
            totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

            // Validar página actual
            if (paginaActual > totalPaginas && totalPaginas > 0)
                paginaActual = totalPaginas;
            if (paginaActual < 1)
                paginaActual = 1;

            // ← APLICAR PAGINACIÓN
            var productosPaginados = productosFiltrados
                .Skip((paginaActual - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToList();

            dgvProductos.DataSource = null;
            dgvProductos.DataSource = productosPaginados;

            // ← ACTUALIZAR CONTROLES
            ActualizarControlesPaginacion();
        }

        private void ActualizarAlertas()
        {
            int stockBajo = ProductoService.ObtenerConStockBajo().Count;
            int proximoVencer = ProductoService.ObtenerProximosAVencer().Count;
            int vencidos = ProductoService.ObtenerVencidos().Count;

            lblAlertaStockBajo.Text = $"• {stockBajo} producto{(stockBajo != 1 ? "s" : "")} con stock bajo";
            lblAlertaProximoVencer.Text = $"• {proximoVencer} producto{(proximoVencer != 1 ? "s" : "")} próximo{(proximoVencer != 1 ? "s" : "")} a vencer";
            lblAlertaVencidos.Text = $"• {vencidos} producto{(vencidos != 1 ? "s" : "")} vencido{(vencidos != 1 ? "s" : "")}";
        }

        private void BtnFiltro_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            filtroActivo = btn.Text;

            // Actualizar estilo de botones
            foreach (Control ctrl in btn.Parent.Controls)
            {
                if (ctrl is Button btnFiltro && btnFiltro.Location.Y == 120)
                {
                    bool esSeleccionado = btnFiltro == btn;
                    btnFiltro.BackColor = esSeleccionado ? Color.White : btnFiltro.Text.Contains("STOCK") ? Color.FromArgb(231, 76, 60) :
                         btnFiltro.Text.Contains("PRÓXIMO") ? Color.FromArgb(243, 156, 18) :
                         btnFiltro.Text.Contains("VENCIDOS") ? Color.FromArgb(192, 57, 43) :
                         Color.FromArgb(189, 195, 199);
                    btnFiltro.ForeColor = esSeleccionado ? Color.Black : Color.White;
                    btnFiltro.FlatAppearance.BorderSize = esSeleccionado ? 2 : 0;
                }
            }

            paginaActual = 1;
            AplicarFiltro();
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            string criterio = txtBuscar.Text.Trim();

            if (string.IsNullOrEmpty(criterio))
            {
                CargarProductos();
                return;
            }

            productosActuales = ProductoService.Buscar(criterio);
            paginaActual = 1; 
            AplicarFiltro();
        }

        private void DgvProductos_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var productoId = Convert.ToInt32(dgvProductos.Rows[e.RowIndex].Cells["Id"].Value);
            var nombreProducto = dgvProductos.Rows[e.RowIndex].Cells["Nombre"].Value.ToString();

            if (dgvProductos.Columns[e.ColumnIndex].Name == "Editar")
            {
                MostrarModalEditar(productoId);
            }
            else if (dgvProductos.Columns[e.ColumnIndex].Name == "Desactivar")
            {
                var resultado = MessageBox.Show(
                    $"¿Desactivar este producto?\n\n{nombreProducto}\n\nEl producto no se eliminará, solo quedará inactivo.",
                    "Confirmar Desactivación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (resultado == DialogResult.Yes)
                {
                    if (ProductoService.Desactivar(productoId, usuarioActual.Id))
                    {
                        MessageBox.Show("Producto desactivado correctamente.", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarProductos();
                        ActualizarAlertas();
                    }
                }
            }
            else if (dgvProductos.Columns[e.ColumnIndex].Name == "Eliminar")
            {
                var resultado = MessageBox.Show(
                    $"⚠️ ¿ELIMINAR PERMANENTEMENTE este producto?\n\n{nombreProducto}\n\n❌ ESTA ACCIÓN NO SE PUEDE DESHACER.\n\nSi el producto tiene ventas asociadas, no se podrá eliminar.",
                    "⚠️ ADVERTENCIA: Eliminación Permanente",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (resultado == DialogResult.Yes)
                {
                    if (ProductoService.EliminarDefinitivamente(productoId, usuarioActual.Id))
                    {
                        MessageBox.Show("Producto eliminado permanentemente.", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarProductos();
                        ActualizarAlertas();
                    }
                }
            }
        }

        private void BtnNuevoProducto_Click(object sender, EventArgs e)
        {
            MostrarModalNuevo();
        }

        private void BtnImportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Archivos Excel (*.xlsx)|*.xlsx";
                openDialog.Title = "Seleccionar archivo Excel";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    int importados = ProductoService.ImportarDesdeExcel(openDialog.FileName, usuarioActual.Id);

                    MessageBox.Show($"✅ Se importaron {importados} productos correctamente.",
                                  "Importación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    CargarProductos();
                    ActualizarAlertas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al importar: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Archivo Excel|*.xlsx",
                    FileName = $"Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    Title = "Guardar Inventario en Excel"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Obtener TODOS los productos filtrados (sin paginación)
                    List<Producto> productosExportar;

                    switch (filtroActivo)
                    {
                        case "🔴 STOCK BAJO":
                            productosExportar = ProductoService.ObtenerConStockBajo();
                            break;
                        case "⚠️ PRÓXIMO A VENCER":
                            productosExportar = ProductoService.ObtenerProximosAVencer();
                            break;
                        case "❌ VENCIDOS":
                            productosExportar = ProductoService.ObtenerVencidos();
                            break;
                        default:
                            productosExportar = productosActuales;
                            break;
                    }

                    if (productosExportar == null || productosExportar.Count == 0)
                    {
                        MessageBox.Show("No hay productos para exportar", "Información",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    bool exito = ExcelExportService.ExportarInventario(productosExportar, saveDialog.FileName);

                    if (exito)
                    {
                        MessageBox.Show(
                            $"✅ Se exportaron {productosExportar.Count} productos correctamente.\n\nArchivo guardado en:\n{saveDialog.FileName}",
                            "Exportación Exitosa",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // MODAL NUEVO PRODUCTO
        private void MostrarModalNuevo()
        {
            Form modal = new Form();
            modal.Text = "Nuevo Producto";
            modal.Size = new Size(500, 550);
            modal.StartPosition = FormStartPosition.CenterParent;
            modal.FormBorderStyle = FormBorderStyle.FixedDialog;
            modal.MaximizeBox = false;
            modal.MinimizeBox = false;

            Label lblTitulo = new Label
            {
                Text = "➕ AGREGAR NUEVO PRODUCTO",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(450, 30)
            };

            Label lblCodigo = new Label { Text = "Código de Barras:", Location = new Point(20, 70), AutoSize = true };
            TextBox txtCodigo = new TextBox { Location = new Point(160, 68), Size = new Size(300, 25) };

            Label lblNombre = new Label { Text = "Nombre:", Location = new Point(20, 110), AutoSize = true };
            TextBox txtNombre = new TextBox { Location = new Point(160, 108), Size = new Size(300, 25) };

            Label lblCategoria = new Label { Text = "Categoría:", Location = new Point(20, 150), AutoSize = true };
            TextBox txtCategoria = new TextBox { Location = new Point(160, 148), Size = new Size(300, 25) };

            Label lblPrecio = new Label { Text = "Precio:", Location = new Point(20, 190), AutoSize = true };
            TextBox txtPrecio = new TextBox { Location = new Point(160, 188), Size = new Size(140, 25), Text = "0.00" };

            Label lblStock = new Label { Text = "Stock:", Location = new Point(20, 230), AutoSize = true };
            TextBox txtStock = new TextBox { Location = new Point(160, 228), Size = new Size(140, 25), Text = "0" };

            Label lblStockMin = new Label { Text = "Stock Mínimo:", Location = new Point(20, 270), AutoSize = true };
            TextBox txtStockMin = new TextBox { Location = new Point(160, 268), Size = new Size(140, 25), Text = "10" };

            Label lblVenc = new Label { Text = "F. Vencimiento:", Location = new Point(20, 310), AutoSize = true };
            DateTimePicker dtpVenc = new DateTimePicker { Location = new Point(160, 308), Size = new Size(200, 25) };
            CheckBox chkSinVenc = new CheckBox { Text = "Sin vencimiento", Location = new Point(160, 338), Checked = true };
            chkSinVenc.CheckedChanged += (s, e) => dtpVenc.Enabled = !chkSinVenc.Checked;
            dtpVenc.Enabled = false;

            Button btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(250, 450),
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel
            };

            Button btnGuardar = new Button
            {
                Text = "Guardar",
                Location = new Point(360, 450),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtCodigo.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("Código y nombre son obligatorios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPrecio.Text, out decimal precio) || precio <= 0)
                {
                    MessageBox.Show("Ingrese un precio válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("Ingrese un stock válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var producto = new Producto
                {
                    CodigoBarras = txtCodigo.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Categoria = txtCategoria.Text.Trim(),
                    Precio = precio,
                    Stock = stock,
                    StockMinimo = int.TryParse(txtStockMin.Text, out int stockMin) ? stockMin : 10,
                    FechaVencimiento = chkSinVenc.Checked ? null : dtpVenc.Value
                };
                if (ProductoService.Agregar(producto, usuarioActual.Id))
                {
                    MessageBox.Show("✅ Producto agregado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    modal.DialogResult = DialogResult.OK;
                    modal.Close();
                }
            };

            modal.Controls.Add(lblTitulo);
            modal.Controls.Add(lblCodigo);
            modal.Controls.Add(txtCodigo);
            modal.Controls.Add(lblNombre);
            modal.Controls.Add(txtNombre);
            modal.Controls.Add(lblCategoria);
            modal.Controls.Add(txtCategoria);
            modal.Controls.Add(lblPrecio);
            modal.Controls.Add(txtPrecio);
            modal.Controls.Add(lblStock);
            modal.Controls.Add(txtStock);
            modal.Controls.Add(lblStockMin);
            modal.Controls.Add(txtStockMin);
            modal.Controls.Add(lblVenc);
            modal.Controls.Add(dtpVenc);
            modal.Controls.Add(chkSinVenc);
            modal.Controls.Add(btnCancelar);
            modal.Controls.Add(btnGuardar);

            if (modal.ShowDialog() == DialogResult.OK)
            {
                CargarProductos();
                ActualizarAlertas();
            }
        }

        // MODAL EDITAR PRODUCTO
        private void MostrarModalEditar(int productoId)
        {
            var producto = ProductoService.ObtenerPorId(productoId);
            if (producto == null)
            {
                MessageBox.Show("Producto no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Form modal = new Form();
            modal.Text = "Editar Producto";
            modal.Size = new Size(500, 550);
            modal.StartPosition = FormStartPosition.CenterParent;
            modal.FormBorderStyle = FormBorderStyle.FixedDialog;
            modal.MaximizeBox = false;
            modal.MinimizeBox = false;

            Label lblTitulo = new Label
            {
                Text = "✏️ EDITAR PRODUCTO",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(450, 30)
            };

            Label lblCodigo = new Label { Text = "Código:", Location = new Point(20, 70), AutoSize = true };
            Label lblCodigoVal = new Label { Text = producto.CodigoBarras, Location = new Point(160, 70), AutoSize = true, Font = new Font("Arial", 10, FontStyle.Bold) };

            Label lblNombre = new Label { Text = "Nombre:", Location = new Point(20, 110), AutoSize = true };
            TextBox txtNombre = new TextBox { Location = new Point(160, 108), Size = new Size(300, 25), Text = producto.Nombre };

            Label lblCategoria = new Label { Text = "Categoría:", Location = new Point(20, 150), AutoSize = true };
            TextBox txtCategoria = new TextBox { Location = new Point(160, 148), Size = new Size(300, 25), Text = producto.Categoria };

            Label lblPrecio = new Label { Text = "Precio:", Location = new Point(20, 190), AutoSize = true };
            TextBox txtPrecio = new TextBox { Location = new Point(160, 188), Size = new Size(140, 25), Text = producto.Precio.ToString("F2") };

            Label lblStock = new Label { Text = "Stock:", Location = new Point(20, 230), AutoSize = true };
            TextBox txtStock = new TextBox { Location = new Point(160, 228), Size = new Size(140, 25), Text = producto.Stock.ToString() };

            Label lblStockMin = new Label { Text = "Stock Mínimo:", Location = new Point(20, 270), AutoSize = true };
            TextBox txtStockMin = new TextBox { Location = new Point(160, 268), Size = new Size(140, 25), Text = producto.StockMinimo.ToString() };

            Label lblVenc = new Label { Text = "F. Vencimiento:", Location = new Point(20, 310), AutoSize = true };
            DateTimePicker dtpVenc = new DateTimePicker { Location = new Point(160, 308), Size = new Size(200, 25) };
            CheckBox chkSinVenc = new CheckBox { Text = "Sin vencimiento", Location = new Point(160, 338) };

            if (producto.FechaVencimiento.HasValue)
            {
                dtpVenc.Value = producto.FechaVencimiento.Value;
                chkSinVenc.Checked = false;
            }
            else
            {
                chkSinVenc.Checked = true;
                dtpVenc.Enabled = false;
            }

            chkSinVenc.CheckedChanged += (s, e) => dtpVenc.Enabled = !chkSinVenc.Checked;

            Button btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(250, 450),
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel
            };

            Button btnGuardar = new Button
            {
                Text = "Guardar Cambios",
                Location = new Point(360, 450),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("El nombre es obligatorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPrecio.Text, out decimal precio) || precio <= 0)
                {
                    MessageBox.Show("Ingrese un precio válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("Ingrese un stock válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                producto.Nombre = txtNombre.Text.Trim();
                producto.Categoria = txtCategoria.Text.Trim();
                producto.Precio = precio;
                producto.Stock = stock;
                producto.StockMinimo = int.TryParse(txtStockMin.Text, out int stockMin) ? stockMin : 10;
                producto.FechaVencimiento = chkSinVenc.Checked ? null : dtpVenc.Value;

                if (ProductoService.Actualizar(producto, usuarioActual.Id))
                {
                    MessageBox.Show("✅ Producto actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    modal.DialogResult = DialogResult.OK;
                    modal.Close();
                }
            };

            modal.Controls.Add(lblTitulo);
            modal.Controls.Add(lblCodigo);
            modal.Controls.Add(lblCodigoVal);
            modal.Controls.Add(lblNombre);
            modal.Controls.Add(txtNombre);
            modal.Controls.Add(lblCategoria);
            modal.Controls.Add(txtCategoria);
            modal.Controls.Add(lblPrecio);
            modal.Controls.Add(txtPrecio);
            modal.Controls.Add(lblStock);
            modal.Controls.Add(txtStock);
            modal.Controls.Add(lblStockMin);
            modal.Controls.Add(txtStockMin);
            modal.Controls.Add(lblVenc);
            modal.Controls.Add(dtpVenc);
            modal.Controls.Add(chkSinVenc);
            modal.Controls.Add(btnCancelar);
            modal.Controls.Add(btnGuardar);

            if (modal.ShowDialog() == DialogResult.OK)
            {
                CargarProductos();
                ActualizarAlertas();
            }
        }
        private void BtnPaginaAnterior_Click(object sender, EventArgs e)
        {
            if (paginaActual > 1)
            {
                paginaActual--;
                AplicarFiltro();
            }
        }

        private void BtnPaginaSiguiente_Click(object sender, EventArgs e)
        {
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                AplicarFiltro();
            }
        }

        private void ActualizarControlesPaginacion()
        {
            // Texto simplificado
            lblPaginacion.Text = $"Página {paginaActual} de {totalPaginas} ({totalRegistros} productos)";

            // Habilitar/deshabilitar botones
            btnPaginaAnterior.Enabled = paginaActual > 1;
            btnPaginaSiguiente.Enabled = paginaActual < totalPaginas;

            // Cambiar color según estado
            btnPaginaAnterior.BackColor = btnPaginaAnterior.Enabled
                ? Color.FromArgb(52, 152, 219)
                : Color.FromArgb(189, 195, 199);

            btnPaginaSiguiente.BackColor = btnPaginaSiguiente.Enabled
                ? Color.FromArgb(52, 152, 219)
                : Color.FromArgb(189, 195, 199);
        }
    }
}