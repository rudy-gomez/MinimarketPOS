using MinimarketPOS.Database;
using MinimarketPOS.Models;
using MinimarketPOS.Services;
using MinimarketPOS.Utils;
using ScottPlot.WinForms;
using System.Drawing;
using System.Windows.Forms;
using static MinimarketPOS.Services.ReporteService;

namespace MinimarketPOS.Forms
{
    public class PanelReportes : Panel
    {
        private Usuario usuarioActual;
        private DateTime fechaInicio;
        private DateTime fechaFin;

        // Controles fijos
        private ComboBox cboPeriodo;
        private DateTimePicker dtpInicio;
        private DateTimePicker dtpFin;
        private Button btnActualizar;

        // Métricas
        private Label lblTotalVentas;
        private Label lblNumTransacciones;
        private Label lblTicketPromedio;
        private Label lblProductosVendidos;

        // Tabs
        private TabControl tabControl;
        private TabPage tabResumen;
        private TabPage tabHistorial;

        // Grids
        private DataGridView dgvEmpleados;
        private DataGridView dgvProductos;
        private DataGridView dgvHistorialVentas;

        // Gráficas ScottPlot
        private FormsPlot plotVentasPorDia;
        private FormsPlot plotTopProductos;

        // Paginación
        private int paginaActual = 1;
        private int registrosPorPagina = 15;
        private int totalRegistros = 0;
        private Label lblPaginacion;
        private Button btnPaginaAnterior;
        private Button btnPaginaSiguiente;

        // Filtros del historial
        private TextBox txtBusquedaProducto;
        private ComboBox cboFiltroEstado;
        private string filtroEstadoActual = "todas";
        private string busquedaProductoActual = "";
        public PanelReportes(Usuario usuario)
        {
            this.usuarioActual = usuario;
            this.fechaInicio = DateTime.Today;
            this.fechaFin = DateTime.Today;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.AutoScroll = true;
            ConfigurarPanel();
            CargarDatos();
        }

        private void ConfigurarPanel()
        {
            int margen = 50;
            int anchoContenido = 1535;
            int posY = 30;

            // Título
            Label lblTitulo = new Label();
            lblTitulo.Text = usuarioActual.EsAdmin() ? "📊 REPORTES Y ESTADÍSTICAS" : "📊 MIS VENTAS";
            lblTitulo.Font = new Font("Arial", 24, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(52, 73, 94);
            lblTitulo.Location = new Point(margen, posY);
            lblTitulo.AutoSize = true;
            this.Controls.Add(lblTitulo);
            posY += 70;

            // Panel de filtros
            ConfigurarPanelFiltros(margen, posY);
            posY += 100;

            // Panel de métricas
            ConfigurarPanelMetricas(margen, posY, anchoContenido);
            posY += 140;

            // TabControl
            tabControl = new TabControl();
            tabControl.Location = new Point(margen, posY);
            tabControl.Size = new Size(anchoContenido, 700);
            tabControl.Font = new Font("Arial", 11);

            // Tab 1: Resumen (2 COLUMNAS)
            tabResumen = new TabPage("📋 Resumen");
            ConfigurarTabResumen();
            tabControl.TabPages.Add(tabResumen);

            // Tab 2: Historial de Ventas (GRID COMPLETO)
            tabHistorial = new TabPage("🗂️ Historial de Ventas");
            ConfigurarTabHistorial();
            tabControl.TabPages.Add(tabHistorial);

            this.Controls.Add(tabControl);
        }

        private void ConfigurarPanelFiltros(int margen, int posY)
        {
            int anchoContenido = 1535;
            Panel panelFiltros = new Panel();
            panelFiltros.BackColor = Color.White;
            panelFiltros.Location = new Point(margen, posY);
            panelFiltros.Size = new Size(anchoContenido, 80);
            panelFiltros.BorderStyle = BorderStyle.FixedSingle;

            Label lblPeriodo = new Label();
            lblPeriodo.Text = "Período:";
            lblPeriodo.Font = new Font("Arial", 10);
            lblPeriodo.Location = new Point(20, 15);
            lblPeriodo.AutoSize = true;
            panelFiltros.Controls.Add(lblPeriodo);

            cboPeriodo = new ComboBox();
            cboPeriodo.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPeriodo.Font = new Font("Arial", 10);
            cboPeriodo.Location = new Point(20, 38);
            cboPeriodo.Size = new Size(150, 30);

            // ✅ RESTRICCIÓN: Empleados solo ven "Hoy"
            if (usuarioActual.EsAdmin())
            {
                cboPeriodo.Items.AddRange(new object[] { "Hoy", "Esta semana", "Este mes", "Personalizado" });
            }
            else
            {
                cboPeriodo.Items.Add("Hoy");
                cboPeriodo.Enabled = false; // No pueden cambiar
            }

            cboPeriodo.SelectedIndex = 0;
            cboPeriodo.SelectedIndexChanged += CboPeriodo_SelectedIndexChanged;
            panelFiltros.Controls.Add(cboPeriodo);

            Label lblDesde = new Label();
            lblDesde.Text = "Desde:";
            lblDesde.Font = new Font("Arial", 10);
            lblDesde.Location = new Point(200, 15);
            lblDesde.AutoSize = true;
            panelFiltros.Controls.Add(lblDesde);

            dtpInicio = new DateTimePicker();
            dtpInicio.Font = new Font("Arial", 10);
            dtpInicio.Location = new Point(200, 38);
            dtpInicio.Size = new Size(150, 30);
            dtpInicio.Format = DateTimePickerFormat.Short;
            dtpInicio.Value = DateTime.Today;
            dtpInicio.Enabled = false;
            panelFiltros.Controls.Add(dtpInicio);

            Label lblHasta = new Label();
            lblHasta.Text = "Hasta:";
            lblHasta.Font = new Font("Arial", 10);
            lblHasta.Location = new Point(380, 15);
            lblHasta.AutoSize = true;
            panelFiltros.Controls.Add(lblHasta);

            dtpFin = new DateTimePicker();
            dtpFin.Font = new Font("Arial", 10);
            dtpFin.Location = new Point(380, 38);
            dtpFin.Size = new Size(150, 30);
            dtpFin.Format = DateTimePickerFormat.Short;
            dtpFin.Value = DateTime.Today;
            dtpFin.Enabled = false;
            panelFiltros.Controls.Add(dtpFin);

            // ✅ RESTRICCIÓN: Empleados no tienen botón "Actualizar" (solo "Hoy")
            if (usuarioActual.EsAdmin())
            {
                btnActualizar = new Button();
                btnActualizar.Text = "🔄 Actualizar";
                btnActualizar.Font = new Font("Arial", 10, FontStyle.Bold);
                btnActualizar.BackColor = Color.FromArgb(0, 123, 255);
                btnActualizar.ForeColor = Color.White;
                btnActualizar.FlatStyle = FlatStyle.Flat;
                btnActualizar.FlatAppearance.BorderSize = 0;
                btnActualizar.Location = new Point(560, 28);
                btnActualizar.Size = new Size(130, 38);
                btnActualizar.Cursor = Cursors.Hand;
                btnActualizar.Click += BtnActualizar_Click;
                panelFiltros.Controls.Add(btnActualizar);
            }

            if (usuarioActual.EsAdmin())
            {
                Button btnExportarPdf = new Button();
                btnExportarPdf.Text = "📄 Exportar PDF";
                btnExportarPdf.Font = new Font("Arial", 10, FontStyle.Bold);
                btnExportarPdf.BackColor = Color.FromArgb(220, 53, 69);
                btnExportarPdf.ForeColor = Color.White;
                btnExportarPdf.FlatStyle = FlatStyle.Flat;
                btnExportarPdf.FlatAppearance.BorderSize = 0;
                btnExportarPdf.Location = new Point(usuarioActual.EsAdmin() ? 710 : 560, 28);
                btnExportarPdf.Size = new Size(150, 38);
                btnExportarPdf.Cursor = Cursors.Hand;
                btnExportarPdf.Click += BtnExportarPdf_Click;
                panelFiltros.Controls.Add(btnExportarPdf);
            }
            this.Controls.Add(panelFiltros);
        }
        int margenInterno = 30;

        private void ConfigurarPanelMetricas(int margen, int posY, int anchoContenido)
        {
            Panel panelMetricas = new Panel();
            panelMetricas.BackColor = Color.Transparent;
            panelMetricas.Location = new Point(margen, posY);
            panelMetricas.Size = new Size(anchoContenido, 120);

            // ← CALCULAR ANCHO DE CADA TARJETA (4 tarjetas con espacio entre ellas)
            int espacioEntreTarjetas = 20;
            int anchoUtil = anchoContenido - (margenInterno * 2);
            int anchoTarjeta = (anchoUtil - (3 * espacioEntreTarjetas)) / 4;


            Panel card1 = CrearTarjetaMetrica("💰 VENTAS TOTALES", "S/ 0.00", Color.FromArgb(40, 167, 69), 0, anchoTarjeta);
            lblTotalVentas = (Label)card1.Controls[1];
            panelMetricas.Controls.Add(card1);

            Panel card2 = CrearTarjetaMetrica("📝 TRANSACCIONES", "0", Color.FromArgb(0, 123, 255), anchoTarjeta + espacioEntreTarjetas, anchoTarjeta);
            lblNumTransacciones = (Label)card2.Controls[1];
            panelMetricas.Controls.Add(card2);

            Panel card3 = CrearTarjetaMetrica("📊 TICKET PROMEDIO", "S/ 0.00", Color.FromArgb(255, 193, 7), (anchoTarjeta + espacioEntreTarjetas) * 2, anchoTarjeta);
            lblTicketPromedio = (Label)card3.Controls[1];
            panelMetricas.Controls.Add(card3);

            Panel card4 = CrearTarjetaMetrica("📦 PRODUCTOS", "0", Color.FromArgb(156, 39, 176), (anchoTarjeta + espacioEntreTarjetas) * 3, anchoTarjeta);
            lblProductosVendidos = (Label)card4.Controls[1];
            panelMetricas.Controls.Add(card4);

            this.Controls.Add(panelMetricas);
        }

        private void ConfigurarTabResumen()
        {
            tabResumen.BackColor = Color.FromArgb(240, 242, 245);

            // ========== COLUMNA IZQUIERDA: TABLAS ==========
            int colIzqX = 30;
            int colIzqWidth = 700;
            int posY = 20;

            // Si es admin, mostrar ventas por empleado
            if (usuarioActual.EsAdmin())
            {
                Label lblEmpleados = new Label();
                lblEmpleados.Text = "👥 Ventas por Empleado";
                lblEmpleados.Font = new Font("Arial", 12, FontStyle.Bold);
                lblEmpleados.Location = new Point(colIzqX, posY);
                lblEmpleados.AutoSize = true;
                tabResumen.Controls.Add(lblEmpleados);
                posY += 35;

                dgvEmpleados = new DataGridView();
                dgvEmpleados.Location = new Point(colIzqX, posY);
                dgvEmpleados.Size = new Size(colIzqWidth, 180);
                dgvEmpleados.BackgroundColor = Color.White;
                dgvEmpleados.AllowUserToAddRows = false;
                dgvEmpleados.ReadOnly = true;
                dgvEmpleados.AutoGenerateColumns = false;
                dgvEmpleados.RowHeadersVisible = false;
                dgvEmpleados.RowTemplate.Height = 35;
                dgvEmpleados.DefaultCellStyle.Font = new Font("Arial", 10);
                ConfigurarGridEmpleados();
                tabResumen.Controls.Add(dgvEmpleados);
                posY += 195;
            }

            // Productos más vendidos
            Label lblProductos = new Label();
            lblProductos.Text = "🏆 Top Productos Más Vendidos";
            lblProductos.Font = new Font("Arial", 12, FontStyle.Bold);
            lblProductos.Location = new Point(colIzqX, posY);
            lblProductos.AutoSize = true;
            tabResumen.Controls.Add(lblProductos);
            posY += 35;

            dgvProductos = new DataGridView();
            dgvProductos.Location = new Point(colIzqX, posY);
            dgvProductos.Size = new Size(colIzqWidth, usuarioActual.EsAdmin() ? 240 : 440);
            dgvProductos.BackgroundColor = Color.White;
            dgvProductos.AllowUserToAddRows = false;
            dgvProductos.ReadOnly = true;
            dgvProductos.AutoGenerateColumns = false;
            dgvProductos.RowHeadersVisible = false;
            dgvProductos.RowTemplate.Height = 35;
            dgvProductos.DefaultCellStyle.Font = new Font("Arial", 10);
            ConfigurarGridProductos();
            tabResumen.Controls.Add(dgvProductos);

            // ========== COLUMNA DERECHA: GRÁFICAS ==========
            int colDerX = 760;
            int colDerWidth = 720;
            posY = 20;

            // Gráfica 1: Ventas por día
            Label lblGrafica1 = new Label();
            lblGrafica1.Text = "📈 Ventas por Día";
            lblGrafica1.Font = new Font("Arial", 12, FontStyle.Bold);
            lblGrafica1.Location = new Point(colDerX, posY);
            lblGrafica1.AutoSize = true;
            tabResumen.Controls.Add(lblGrafica1);
            posY += 35;

            plotVentasPorDia = new ScottPlot.WinForms.FormsPlot();
            plotVentasPorDia.Location = new Point(colDerX, posY);
            plotVentasPorDia.Size = new Size(colDerWidth, 300);
            plotVentasPorDia.BackColor = Color.White;
            tabResumen.Controls.Add(plotVentasPorDia);
            posY += 265;

            // Gráfica 2: Top productos (circular)
            Label lblGrafica2 = new Label();
            lblGrafica2.Text = "📊 Distribución de Productos Vendidos";
            lblGrafica2.Font = new Font("Arial", 12, FontStyle.Bold);
            lblGrafica2.Location = new Point(colDerX, posY);
            lblGrafica2.AutoSize = true;
            tabResumen.Controls.Add(lblGrafica2);
            posY += 35;

            plotTopProductos = new ScottPlot.WinForms.FormsPlot();
            plotTopProductos.Location = new Point(colDerX, posY);
            plotTopProductos.Size = new Size(colDerWidth, 280);
            plotTopProductos.BackColor = Color.White;
            tabResumen.Controls.Add(plotTopProductos);
        }


        private void ConfigurarTabHistorial()
        {
            tabHistorial.BackColor = Color.FromArgb(240, 242, 245);

            Label lblVentas = new Label();
            lblVentas.Text = "📋 Registro Completo de Ventas";
            lblVentas.Font = new Font("Arial", 13, FontStyle.Bold); // ← CAMBIAR de 12 a 13
            lblVentas.Location = new Point(30, 20); // ← CAMBIAR de 20 a 30
            lblVentas.AutoSize = true;
            tabHistorial.Controls.Add(lblVentas);

            // Panel de filtros
            Panel panelFiltrosHistorial = new Panel();
            panelFiltrosHistorial.Location = new Point(30, 55); // ← CAMBIAR de 20 a 30
            panelFiltrosHistorial.Size = new Size(1450, 65); // ← CAMBIAR de 1120 a 1540
            panelFiltrosHistorial.BackColor = Color.White;
            panelFiltrosHistorial.BorderStyle = BorderStyle.FixedSingle;

            // Búsqueda por producto
            Label lblBusqueda = new Label();
            lblBusqueda.Text = "🔍 Buscar producto:";
            lblBusqueda.Font = new Font("Arial", 9);
            lblBusqueda.Location = new Point(10, 5);
            lblBusqueda.AutoSize = true;
            panelFiltrosHistorial.Controls.Add(lblBusqueda);

            txtBusquedaProducto = new TextBox();
            txtBusquedaProducto.Location = new Point(10, 23);
            txtBusquedaProducto.Size = new Size(300, 25); // ← CAMBIAR de 250 a 300
            txtBusquedaProducto.Font = new Font("Arial", 10);
            txtBusquedaProducto.PlaceholderText = "Nombre del producto...";
            panelFiltrosHistorial.Controls.Add(txtBusquedaProducto);

            // Filtro por estado
            Label lblEstado = new Label();
            lblEstado.Text = "Estado:";
            lblEstado.Font = new Font("Arial", 9);
            lblEstado.Location = new Point(330, 5); // ← CAMBIAR de 280 a 330
            lblEstado.AutoSize = true;
            panelFiltrosHistorial.Controls.Add(lblEstado);

            cboFiltroEstado = new ComboBox();
            cboFiltroEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFiltroEstado.Location = new Point(330, 23); // ← CAMBIAR de 280 a 330
            cboFiltroEstado.Size = new Size(150, 25);
            cboFiltroEstado.Font = new Font("Arial", 10);
            cboFiltroEstado.Items.AddRange(new object[] { "Todas", "Completadas", "Anuladas" });
            cboFiltroEstado.SelectedIndex = 0;
            panelFiltrosHistorial.Controls.Add(cboFiltroEstado);

            // Botón filtrar
            Button btnFiltrar = new Button();
            btnFiltrar.Text = "🔎 Filtrar";
            btnFiltrar.Font = new Font("Arial", 9, FontStyle.Bold);
            btnFiltrar.BackColor = Color.FromArgb(0, 123, 255);
            btnFiltrar.ForeColor = Color.White;
            btnFiltrar.FlatStyle = FlatStyle.Flat;
            btnFiltrar.FlatAppearance.BorderSize = 0;
            btnFiltrar.Location = new Point(500, 18); // ← CAMBIAR de 450 a 500
            btnFiltrar.Size = new Size(100, 28);
            btnFiltrar.Cursor = Cursors.Hand;
            btnFiltrar.Click += BtnFiltrar_Click;
            panelFiltrosHistorial.Controls.Add(btnFiltrar);

            // Botón limpiar filtros
            Button btnLimpiar = new Button();
            btnLimpiar.Text = "✖ Limpiar";
            btnLimpiar.Font = new Font("Arial", 9);
            btnLimpiar.BackColor = Color.Gray;
            btnLimpiar.ForeColor = Color.White;
            btnLimpiar.FlatStyle = FlatStyle.Flat;
            btnFiltrar.FlatAppearance.BorderSize = 0;
            btnLimpiar.Location = new Point(610, 18); // ← CAMBIAR de 560 a 610
            btnLimpiar.Size = new Size(100, 28);
            btnLimpiar.Cursor = Cursors.Hand;
            btnLimpiar.Click += BtnLimpiarFiltros_Click;
            panelFiltrosHistorial.Controls.Add(btnLimpiar);

            tabHistorial.Controls.Add(panelFiltrosHistorial);

            if (usuarioActual.EsAdmin())
            {
                // Botones de exportación
                Button btnExportarExcel = new Button();
                btnExportarExcel.Text = "📥 Exportar Excel";
                btnExportarExcel.Font = new Font("Arial", 10, FontStyle.Bold);
                btnExportarExcel.BackColor = Color.FromArgb(40, 167, 69);
                btnExportarExcel.ForeColor = Color.White;
                btnExportarExcel.FlatStyle = FlatStyle.Flat;
                btnExportarExcel.FlatAppearance.BorderSize = 0;
                btnExportarExcel.Location = new Point(1270, 18); // ← CAMBIAR de 970 a 1360
                btnExportarExcel.Size = new Size(160, 35);
                btnExportarExcel.Cursor = Cursors.Hand;
                btnExportarExcel.Click += BtnExportarExcel_Click;
                tabHistorial.Controls.Add(btnExportarExcel);
            }

            dgvHistorialVentas = new DataGridView();
            dgvHistorialVentas.Location = new Point(30, 115); // ← CAMBIAR de 20 a 30
            dgvHistorialVentas.Size = new Size(1450, 380); // ← CAMBIAR de (1120, 310) a (1540, 380)
            dgvHistorialVentas.BackgroundColor = Color.White;
            dgvHistorialVentas.AllowUserToAddRows = false;
            dgvHistorialVentas.ReadOnly = true;
            dgvHistorialVentas.AutoGenerateColumns = false;
            dgvHistorialVentas.RowHeadersVisible = false;
            dgvHistorialVentas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvHistorialVentas.RowTemplate.Height = 35; // ← AGREGAR: Filas más altas
            dgvHistorialVentas.DefaultCellStyle.Font = new Font("Arial", 10); // ← AGREGAR
            dgvHistorialVentas.CellContentClick += DgvHistorialVentas_CellContentClick;
            ConfigurarGridHistorialVentas();
            tabHistorial.Controls.Add(dgvHistorialVentas);

            // Paginación
            Panel panelPaginacion = new Panel();
            panelPaginacion.Location = new Point(30, 505); // ← CAMBIAR de (20, 435) a (30, 505)
            panelPaginacion.Size = new Size(1450, 55); // ← CAMBIAR de (1120, 40) a (1540, 45)
            panelPaginacion.BackColor = Color.White;

            btnPaginaAnterior = new Button();
            btnPaginaAnterior.Text = "◀ Anterior";
            btnPaginaAnterior.Location = new Point(10, 8); // ← CAMBIAR de (10, 5) a (10, 8)
            btnPaginaAnterior.Size = new Size(130, 32); // ← CAMBIAR de (120, 30) a (130, 32)
            btnPaginaAnterior.Font = new Font("Arial", 10); // ← AGREGAR
            btnPaginaAnterior.BackColor = Color.FromArgb(0, 123, 255); // ← AGREGAR
            btnPaginaAnterior.ForeColor = Color.White; // ← AGREGAR
            btnPaginaAnterior.FlatStyle = FlatStyle.Flat; // ← AGREGAR
            btnPaginaAnterior.FlatAppearance.BorderSize = 0; // ← AGREGAR
            btnPaginaAnterior.Cursor = Cursors.Hand; // ← AGREGAR
            btnPaginaAnterior.Click += BtnPaginaAnterior_Click;
            panelPaginacion.Controls.Add(btnPaginaAnterior);

            lblPaginacion = new Label();
            lblPaginacion.Text = "Página 1 de 1";
            lblPaginacion.Font = new Font("Arial", 11, FontStyle.Bold); // ← CAMBIAR de 10 a 11
            lblPaginacion.Location = new Point(660, 12); // ← CAMBIAR de (450, 10) a (660, 12)
            lblPaginacion.Size = new Size(280, 25); // ← CAMBIAR de 200 a 250
            lblPaginacion.TextAlign = ContentAlignment.MiddleCenter;
            panelPaginacion.Controls.Add(lblPaginacion);

            btnPaginaSiguiente = new Button();
            btnPaginaSiguiente.Text = "Siguiente ▶";
            btnPaginaSiguiente.Location = new Point(1310, 8); // ← CAMBIAR de (990, 5) a (1400, 8)
            btnPaginaSiguiente.Size = new Size(130, 32); // ← CAMBIAR de (120, 30) a (130, 32)
            btnPaginaSiguiente.Font = new Font("Arial", 10); // ← AGREGAR
            btnPaginaSiguiente.BackColor = Color.FromArgb(0, 123, 255); // ← AGREGAR
            btnPaginaSiguiente.ForeColor = Color.White; // ← AGREGAR
            btnPaginaSiguiente.FlatStyle = FlatStyle.Flat; // ← AGREGAR
            btnPaginaSiguiente.FlatAppearance.BorderSize = 0; // ← AGREGAR
            btnPaginaSiguiente.Cursor = Cursors.Hand; // ← AGREGAR
            btnPaginaSiguiente.Click += BtnPaginaSiguiente_Click;
            panelPaginacion.Controls.Add(btnPaginaSiguiente);

            tabHistorial.Controls.Add(panelPaginacion);
        }

        private Panel CrearTarjetaMetrica(string titulo, string valor, Color colorFondo, int posX, int ancho)
        {
            Panel card = new Panel();
            card.BackColor = colorFondo;
            card.Location = new Point(posX, 0);
            card.Size = new Size(ancho, 110); // ← Usar ancho dinámico

            Label lblTitulo = new Label();
            lblTitulo.Text = titulo;
            lblTitulo.Font = new Font("Arial", 10, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.Location = new Point(15, 15);
            lblTitulo.AutoSize = true;
            card.Controls.Add(lblTitulo);

            Label lblValor = new Label();
            lblValor.Text = valor;
            lblValor.Font = new Font("Arial", 20, FontStyle.Bold);
            lblValor.ForeColor = Color.White;
            lblValor.Location = new Point(15, 45);
            lblValor.AutoSize = true;
            card.Controls.Add(lblValor);

            return card;
        }

        private void ConfigurarGridEmpleados()
        {
            dgvEmpleados.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Empleado",
                HeaderText = "EMPLEADO",
                DataPropertyName = "NombreEmpleado",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvEmpleados.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Ventas",
                HeaderText = "VENTAS",
                DataPropertyName = "NumeroVentas",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvEmpleados.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Total",
                HeaderText = "TOTAL",
                DataPropertyName = "TotalVendido",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
        }

        private void ConfigurarGridProductos()
        {
            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Producto",
                HeaderText = "PRODUCTO",
                DataPropertyName = "NombreProducto",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Cantidad",
                HeaderText = "CANT.",
                DataPropertyName = "CantidadVendida",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvProductos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Total",
                HeaderText = "TOTAL",
                DataPropertyName = "TotalGenerado",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
        }

        private void ConfigurarGridHistorialVentas()
        {
            dgvHistorialVentas.Columns.Clear();

            // Columna oculta: ID
            dgvHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                Visible = false
            });

            // #
            dgvHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Numero",
                HeaderText = "#",
                Width = 50,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Ticket
            dgvHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Ticket",
                HeaderText = "TICKET",
                Width = 90
            });

            // Fecha
            dgvHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Fecha",
                HeaderText = "FECHA/HORA",
                Width = 140
            });

            // Producto (primera item o resumen)
            dgvHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Producto",
                HeaderText = "PRODUCTOS",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 200
            });

            // Cantidad total de items
            dgvHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CantItems",
                HeaderText = "ITEMS",
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Total
            dgvHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Total",
                HeaderText = "TOTAL",
                Width = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Usuario",
                HeaderText = "CAJERO",
                Width = 120
            });
            /* // Usuario/Cajero RESTRICCIÓN ADMIN VE Y EMPLEADO NO
            if (usuarioActual.EsAdmin())
            {
                dgvHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = "Usuario",
                    HeaderText = "CAJERO",
                    Width = 120
                });
            }*/

            // Estado
            dgvHistorialVentas.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Estado",
                HeaderText = "ESTADO",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // Botón de acciones (...)
            DataGridViewButtonColumn btnAcciones = new DataGridViewButtonColumn();
            btnAcciones.Name = "Acciones";
            btnAcciones.HeaderText = "⋮";
            btnAcciones.Text = "⋮";
            btnAcciones.UseColumnTextForButtonValue = true;
            btnAcciones.Width = 50;
            btnAcciones.FlatStyle = FlatStyle.Flat;
            dgvHistorialVentas.Columns.Add(btnAcciones);
        }

        private void DgvHistorialVentas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvHistorialVentas.Columns[e.ColumnIndex].Name == "Acciones")
            {
                // Obtener el objeto VentaDetallada guardado en el Tag de la fila
                var venta = (VentaDetallada)dgvHistorialVentas.Rows[e.RowIndex].Tag;
                if (venta != null)
                {
                    MostrarMenuContextual(venta, e.RowIndex);
                }
            }
        }

        private void MostrarMenuContextual(VentaDetallada venta, int rowIndex)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Font = new Font("Arial", 10);

            // Opción: Ver detalles (TODOS pueden ver)
            ToolStripMenuItem itemDetalles = new ToolStripMenuItem("📄 Ver Detalles");
            itemDetalles.Click += (s, e) => MostrarDetallesVenta(venta);
            menu.Items.Add(itemDetalles);

            // ✅ RESTRICCIÓN: Anular solo si:
            // - Está completada
            // - Es admin O es su propia venta
            if (venta.Estado == "completada")
            {
                bool puedeAnular = usuarioActual.EsAdmin() ||
                                  venta.NombreUsuario.Equals(usuarioActual.Nombre, StringComparison.OrdinalIgnoreCase);

                if (puedeAnular)
                {
                    ToolStripMenuItem itemAnular = new ToolStripMenuItem("❌ Anular Venta");
                    itemAnular.Click += (s, e) => AnularVenta(venta);
                    menu.Items.Add(itemAnular);
                }
            }

            // Mostrar menú
            Point cellLocation = dgvHistorialVentas.GetCellDisplayRectangle(
                dgvHistorialVentas.Columns["Acciones"].Index, rowIndex, false).Location;
            Point menuLocation = dgvHistorialVentas.PointToScreen(new Point(cellLocation.X, cellLocation.Y + 30));
            menu.Show(menuLocation);
        }

        private void MostrarDetallesVenta(VentaDetallada venta)
        {
            Form formDetalle = new Form();
            formDetalle.Text = $"Detalle Venta - Ticket #{venta.NumeroTicket:D6}";
            formDetalle.Size = new Size(700, 500);
            formDetalle.StartPosition = FormStartPosition.CenterParent;
            formDetalle.FormBorderStyle = FormBorderStyle.FixedDialog;
            formDetalle.MaximizeBox = false;
            formDetalle.MinimizeBox = false;

            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = Color.FromArgb(240, 242, 245);
            panel.Padding = new Padding(20);

            // Información de la venta
            Label lblInfo = new Label();
            lblInfo.Text = $"TICKET: #{venta.NumeroTicket:D6}\n" +
                          $"FECHA: {venta.FechaHora:dd/MM/yyyy HH:mm}\n" +
                          $"CAJERO: {venta.NombreUsuario}\n" +
                          $"ESTADO: {venta.Estado.ToUpper()}";
            lblInfo.Font = new Font("Arial", 11);
            lblInfo.Location = new Point(20, 20);
            lblInfo.AutoSize = true;
            panel.Controls.Add(lblInfo);

            // Botón descargar ticket PDF
            Button btnDescargarTicket = new Button();
            btnDescargarTicket.Text = "📄 Descargar Ticket PDF";
            btnDescargarTicket.Font = new Font("Arial", 9, FontStyle.Bold);
            btnDescargarTicket.BackColor = Color.FromArgb(220, 53, 69);
            btnDescargarTicket.ForeColor = Color.White;
            btnDescargarTicket.FlatStyle = FlatStyle.Flat;
            btnDescargarTicket.FlatAppearance.BorderSize = 0;
            btnDescargarTicket.Location = new Point(450, 20);
            btnDescargarTicket.Size = new Size(180, 35);
            btnDescargarTicket.Cursor = Cursors.Hand;
            btnDescargarTicket.Click += (s, e) =>
            {
                // Obtener detalles completos de la venta
                var detalles = ReporteService.ObtenerDetalleVenta(venta.Id);

                // Obtener datos de efectivo y vuelto desde la BD
                decimal efectivo = 0;
                decimal vuelto = 0;

                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT efectivo_recibido, vuelto FROM ventas WHERE id = @id";
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", venta.Id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                efectivo = reader["efectivo_recibido"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["efectivo_recibido"])
                                    : venta.Total;
                                vuelto = reader["vuelto"] != DBNull.Value
                                    ? Convert.ToDecimal(reader["vuelto"])
                                    : 0;
                            }
                        }
                    }
                }

                // Generar PDF del ticket
                TicketPdfService.GenerarTicketPdf(
                    venta.NumeroTicket,
                    detalles,
                    venta.Total,
                    efectivo,
                    vuelto,
                    venta.NombreUsuario,
                    venta.FechaHora
                );
            };
            panel.Controls.Add(btnDescargarTicket);

            // Grid de detalles
            DataGridView dgvDetalle = new DataGridView();
            dgvDetalle.Location = new Point(20, 120);
            dgvDetalle.Size = new Size(640, 280);
            dgvDetalle.BackgroundColor = Color.White;
            dgvDetalle.AllowUserToAddRows = false;
            dgvDetalle.ReadOnly = true;
            dgvDetalle.AutoGenerateColumns = false;
            dgvDetalle.RowHeadersVisible = false;

            dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Producto",
                HeaderText = "PRODUCTO",
                DataPropertyName = "NombreProducto",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Cantidad",
                HeaderText = "CANT.",
                DataPropertyName = "Cantidad",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Precio",
                HeaderText = "P.UNIT",
                DataPropertyName = "PrecioUnitario",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Subtotal",
                HeaderText = "SUBTOTAL",
                DataPropertyName = "Subtotal",
                Width = 120,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "C2", Alignment = DataGridViewContentAlignment.MiddleRight, Font = new Font("Arial", 10, FontStyle.Bold) }
            });

            // Cargar detalles
            var detalles = ReporteService.ObtenerDetalleVenta(venta.Id);
            dgvDetalle.DataSource = detalles;
            panel.Controls.Add(dgvDetalle);

            // Total
            Label lblTotal = new Label();
            lblTotal.Text = $"TOTAL: {venta.Total:C2}";
            lblTotal.Font = new Font("Arial", 14, FontStyle.Bold);
            lblTotal.ForeColor = Color.FromArgb(40, 167, 69);
            lblTotal.Location = new Point(440, 410);
            lblTotal.AutoSize = true;
            panel.Controls.Add(lblTotal);

            formDetalle.Controls.Add(panel);
            formDetalle.ShowDialog();
        }

        private void AnularVenta(VentaDetallada venta)
        {
            // Modal para ingresar motivo
            Form formMotivo = new Form();
            formMotivo.Text = "Anular Venta";
            formMotivo.Size = new Size(450, 250);
            formMotivo.StartPosition = FormStartPosition.CenterParent;
            formMotivo.FormBorderStyle = FormBorderStyle.FixedDialog;
            formMotivo.MaximizeBox = false;
            formMotivo.MinimizeBox = false;

            Label lblPregunta = new Label();
            lblPregunta.Text = $"¿Está seguro de anular la venta?\nTicket: #{venta.NumeroTicket:D6}";
            lblPregunta.Font = new Font("Arial", 11);
            lblPregunta.Location = new Point(30, 20);
            lblPregunta.AutoSize = true;
            formMotivo.Controls.Add(lblPregunta);

            Label lblMotivo = new Label();
            lblMotivo.Text = "Motivo de anulación:";
            lblMotivo.Font = new Font("Arial", 10);
            lblMotivo.Location = new Point(30, 70);
            lblMotivo.AutoSize = true;
            formMotivo.Controls.Add(lblMotivo);

            TextBox txtMotivo = new TextBox();
            txtMotivo.Location = new Point(30, 95);
            txtMotivo.Size = new Size(380, 60);
            txtMotivo.Multiline = true;
            txtMotivo.Font = new Font("Arial", 10);
            formMotivo.Controls.Add(txtMotivo);

            Button btnConfirmar = new Button();
            btnConfirmar.Text = "✓ Confirmar Anulación";
            btnConfirmar.BackColor = Color.FromArgb(220, 53, 69);
            btnConfirmar.ForeColor = Color.White;
            btnConfirmar.FlatStyle = FlatStyle.Flat;
            btnConfirmar.Font = new Font("Arial", 10, FontStyle.Bold);
            btnConfirmar.Location = new Point(220, 170);
            btnConfirmar.Size = new Size(180, 35);
            btnConfirmar.Click += (s, e) =>
            {
                string motivo = txtMotivo.Text.Trim();
                if (string.IsNullOrEmpty(motivo))
                {
                    MessageBox.Show("Debe ingresar un motivo", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool exito = ReporteService.AnularVenta(venta.Id, motivo, usuarioActual.Id);
                if (exito)
                {
                    MessageBox.Show("Venta anulada correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    formMotivo.DialogResult = DialogResult.OK;
                    formMotivo.Close();
                    CargarDatos(); // Recargar datos
                }
            };
            formMotivo.Controls.Add(btnConfirmar);

            Button btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.BackColor = Color.Gray;
            btnCancelar.ForeColor = Color.White;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Font = new Font("Arial", 10);
            btnCancelar.Location = new Point(30, 170);
            btnCancelar.Size = new Size(180, 35);
            btnCancelar.Click += (s, e) => formMotivo.Close();
            formMotivo.Controls.Add(btnCancelar);

            formMotivo.ShowDialog();
        }

        private void CboPeriodo_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cboPeriodo.SelectedIndex)
            {
                case 0: // Hoy
                    fechaInicio = DateTime.Today;
                    fechaFin = DateTime.Today;
                    dtpInicio.Enabled = false;
                    dtpFin.Enabled = false;
                    break;
                case 1: // Esta semana
                    fechaInicio = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    fechaFin = DateTime.Today;
                    dtpInicio.Enabled = false;
                    dtpFin.Enabled = false;
                    break;
                case 2: // Este mes
                    fechaInicio = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    fechaFin = DateTime.Today;
                    dtpInicio.Enabled = false;
                    dtpFin.Enabled = false;
                    break;
                case 3: // Personalizado
                    dtpInicio.Enabled = true;
                    dtpFin.Enabled = true;
                    return;
            }

            dtpInicio.Value = fechaInicio;
            dtpFin.Value = fechaFin;
            CargarDatos();
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            if (cboPeriodo.SelectedIndex == 3)
            {
                fechaInicio = dtpInicio.Value.Date;
                fechaFin = dtpFin.Value.Date;
            }
            paginaActual = 1;
            CargarDatos();
        }
        private void BtnFiltrar_Click(object sender, EventArgs e)
        {
            // Convertir el ComboBox a valor de filtro
            filtroEstadoActual = cboFiltroEstado.SelectedIndex switch
            {
                0 => "todas",
                1 => "completada",
                2 => "anulada",
                _ => "todas"
            };

            busquedaProductoActual = txtBusquedaProducto.Text.Trim();
            paginaActual = 1;
            CargarHistorialVentas();
        }

        private void BtnLimpiarFiltros_Click(object sender, EventArgs e)
        {
            txtBusquedaProducto.Clear();
            cboFiltroEstado.SelectedIndex = 0;
            filtroEstadoActual = "todas";
            busquedaProductoActual = "";
            paginaActual = 1;
            CargarHistorialVentas();
        }
        private void BtnPaginaAnterior_Click(object sender, EventArgs e)
        {
            if (paginaActual > 1)
            {
                paginaActual--;
                CargarHistorialVentas();
            }
        }

        private void BtnPaginaSiguiente_Click(object sender, EventArgs e)
        {
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                CargarHistorialVentas();
            }
        }

        private void CargarDatos()
        {
            try
            {
                int? usuarioIdFiltro = null;
                //int? usuarioIdFiltro = usuarioActual.EsAdmin() ? null : (int?)usuarioActual.Id; ES PARA QUE SOLO VEA SUS VENTAS

                // Métricas
                var metricas = ReporteService.ObtenerMetricas(fechaInicio, fechaFin, usuarioIdFiltro);
                lblTotalVentas.Text = $"S/ {metricas.TotalVentas:N2}";
                lblNumTransacciones.Text = metricas.NumeroTransacciones.ToString();
                lblTicketPromedio.Text = $"S/ {metricas.TicketPromedio:N2}";
                lblProductosVendidos.Text = metricas.ProductosVendidos.ToString();

                // Tab Resumen
                if (usuarioActual.EsAdmin() && dgvEmpleados != null)
                {
                    var ventasPorEmpleado = ReporteService.ObtenerVentasPorEmpleado(fechaInicio, fechaFin);
                    dgvEmpleados.DataSource = ventasPorEmpleado;
                }

                var productos = ReporteService.ObtenerProductosMasVendidos(fechaInicio, fechaFin, 10, usuarioIdFiltro);
                dgvProductos.DataSource = productos;

                // Tab Historial
                CargarHistorialVentas();

                // Gráficas
                CargarGraficas(usuarioIdFiltro);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarHistorialVentas()
        {
            try
            {
                int? usuarioIdFiltro = null;

                // Obtener total de registros con filtros
                totalRegistros = ReporteService.ContarVentasFiltradas(
                    fechaInicio, fechaFin, usuarioIdFiltro, filtroEstadoActual, busquedaProductoActual);

                // Cargar página actual con filtros
                int offset = (paginaActual - 1) * registrosPorPagina;
                var ventas = ReporteService.FiltrarVentas(
                    fechaInicio, fechaFin, usuarioIdFiltro, filtroEstadoActual,
                    busquedaProductoActual, registrosPorPagina, offset);

                // Limpiar grid
                dgvHistorialVentas.Rows.Clear();

                // Agregar filas manualmente
                int numeroFila = (paginaActual - 1) * registrosPorPagina + 1;
                foreach (var venta in ventas)
                {
                    // Agregar fila - MISMA ESTRUCTURA PARA TODOS
                    int rowIndex = dgvHistorialVentas.Rows.Add(
                        venta.Id,                                      // Id (oculto)
                        numeroFila,                                    // #
                        $"#{venta.NumeroTicket:D6}",                  // TICKET
                        venta.FechaHora.ToString("dd/MM/yyyy HH:mm"), // FECHA/HORA
                        venta.ProductosResumen,                        // PRODUCTOS
                        venta.CantidadItems,                          // ITEMS
                        venta.Total,                                   // TOTAL
                        venta.NombreUsuario,                          // CAJERO
                        venta.Estado.ToUpper(),                       // ESTADO
                        "⋮"                                           // ACCIONES
                    );

                    // Aplicar colores al estado
                    var estadoCell = dgvHistorialVentas.Rows[rowIndex].Cells["Estado"];
                    if (venta.Estado == "completada")
                    {
                        estadoCell.Style.BackColor = Color.FromArgb(220, 255, 220);
                        estadoCell.Style.ForeColor = Color.FromArgb(0, 120, 0);
                        estadoCell.Value = "COMPLETADA";
                    }
                    else if (venta.Estado == "anulada")
                    {
                        estadoCell.Style.BackColor = Color.FromArgb(255, 220, 220);
                        estadoCell.Style.ForeColor = Color.FromArgb(180, 0, 0);
                        estadoCell.Value = "ANULADA";
                    }

                    // Guardar el objeto completo en la fila para acceso posterior
                    dgvHistorialVentas.Rows[rowIndex].Tag = venta;

                    numeroFila++;
                }

                // Actualizar paginación
                int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)totalRegistros / registrosPorPagina));
                lblPaginacion.Text = $"Página {paginaActual} de {totalPaginas} ({totalRegistros} registros)";
                btnPaginaAnterior.Enabled = paginaActual > 1;
                btnPaginaSiguiente.Enabled = paginaActual < totalPaginas;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar historial: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarGraficas(int? usuarioIdFiltro)
        {
            try
            {
                // Limpiar gráficas
                plotVentasPorDia.Plot.Clear();
                plotTopProductos.Plot.Clear();

                // ========== GRÁFICA 1: Ventas por día (LÍNEA) ==========
                List<DateTime> fechas = new List<DateTime>();
                List<double> totales = new List<double>();

                using (var connection = DatabaseManager.GetConnection())
                {
                    connection.Open();

                    string query = $@"
                    SELECT 
                        DATE(fecha_hora) as fecha,
                        SUM(total) as total_dia
                    FROM ventas
                    WHERE DATE(fecha_hora) BETWEEN DATE(@fechaInicio) AND DATE(@fechaFin)
                    AND estado = 'completada'
                    {(usuarioIdFiltro.HasValue ? "AND usuario_id = @usuarioId" : "")}
                    GROUP BY DATE(fecha_hora)
                    ORDER BY fecha";

                    using (var cmd = new System.Data.SQLite.SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@fechaFin", fechaFin.ToString("yyyy-MM-dd"));
                        if (usuarioIdFiltro.HasValue)
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioIdFiltro.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                fechas.Add(Convert.ToDateTime(reader["fecha"]));
                                totales.Add(Convert.ToDouble(reader["total_dia"]));
                            }
                        }
                    }
                }

                if (fechas.Count > 0)
                {
                    double[] xs = fechas.Select(f => f.ToOADate()).ToArray();
                    double[] ys = totales.ToArray();

                    var scatter = plotVentasPorDia.Plot.Add.Scatter(xs, ys);
                    scatter.LineWidth = 3;
                    scatter.Color = ScottPlot.Color.FromHex("#007BFF");
                    scatter.MarkerSize = 10;
                    scatter.MarkerShape = ScottPlot.MarkerShape.FilledCircle;

                    plotVentasPorDia.Plot.Axes.DateTimeTicksBottom();
                    plotVentasPorDia.Plot.YLabel("Ventas (S/)");
                    plotVentasPorDia.Plot.XLabel("Fecha");
                    // Título dinámico según período
                    string tituloPeriodo = "";
                    TimeSpan diferencia = fechaFin - fechaInicio;
                    if (diferencia.Days == 0)
                        tituloPeriodo = $"Ventas del {fechaInicio:dd/MM/yyyy}";
                    else if (diferencia.Days <= 7)
                        tituloPeriodo = $"Ventas últimos {diferencia.Days + 1} días";
                    else if (diferencia.Days <= 31)
                        tituloPeriodo = "Ventas del mes";
                    else
                        tituloPeriodo = $"Ventas del {fechaInicio:dd/MM} al {fechaFin:dd/MM}";

                    plotVentasPorDia.Plot.Title(tituloPeriodo);
                    plotVentasPorDia.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
                    plotVentasPorDia.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#F8F9FA");
                }
                else
                {
                    var text = plotVentasPorDia.Plot.Add.Text("Sin datos para mostrar", 0.5, 0.5);
                    text.LabelFontSize = 14;
                    text.LabelFontColor = ScottPlot.Color.FromHex("#999999");
                }

                plotVentasPorDia.Refresh();

                // ========== GRÁFICA 2: Top productos (GRÁFICA CIRCULAR/PIE) ==========
                var topProductos = ReporteService.ObtenerProductosMasVendidos(fechaInicio, fechaFin, 5, usuarioIdFiltro);

                if (topProductos.Count > 0)
                {
                    double[] valores = topProductos.Select(p => (double)p.CantidadVendida).ToArray();
                    string[] nombres = topProductos.Select(p =>
                        p.NombreProducto.Length > 25 ? p.NombreProducto.Substring(0, 22) + "..." : p.NombreProducto
                    ).ToArray();

                    var pie = plotTopProductos.Plot.Add.Pie(valores);

                    // Colores atractivos
                    ScottPlot.Color[] colores = new ScottPlot.Color[]
                    {
                        ScottPlot.Color.FromHex("#FF6384"),
                        ScottPlot.Color.FromHex("#36A2EB"),
                        ScottPlot.Color.FromHex("#FFCE56"),
                        ScottPlot.Color.FromHex("#4BC0C0"),
                        ScottPlot.Color.FromHex("#9966FF")
                    };

                    for (int i = 0; i < Math.Min(valores.Length, colores.Length); i++)
                    {
                        pie.Slices[i].FillColor = colores[i];
                        pie.Slices[i].Label = $"{nombres[i]}\n({valores[i]})";
                    }

                    plotTopProductos.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#FFFFFF");
                    plotTopProductos.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#F8F9FA");
                    plotTopProductos.Plot.Axes.Frameless();
                    plotTopProductos.Plot.HideGrid();

                    // Agregar leyenda
                    var legend = plotTopProductos.Plot.Legend;
                    legend.IsVisible = true;
                    legend.Location = ScottPlot.Alignment.UpperRight;
                }
                else
                {
                    var text = plotTopProductos.Plot.Add.Text("Sin datos para mostrar", 0.5, 0.5);
                    text.LabelFontSize = 14;
                    text.LabelFontColor = ScottPlot.Color.FromHex("#999999");
                }

                plotTopProductos.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar gráficas: {ex.Message}\n\n{ex.StackTrace}",
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void BtnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener TODAS las ventas filtradas (sin paginación)
                int? usuarioIdFiltro = null;
                var todasLasVentas = ReporteService.ObtenerVentasParaExportacion(
                    fechaInicio, fechaFin, usuarioIdFiltro);

                if (todasLasVentas.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar", "Información",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string nombreArchivo = $"Historial_Ventas_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}.xlsx";
                ExportarExcelService.ExportarHistorialVentas(todasLasVentas, nombreArchivo, usuarioActual.EsAdmin());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportarPdf_Click(object sender, EventArgs e)
        {
            try
            {
                int? usuarioIdFiltro = usuarioActual.EsAdmin() ? null : (int?)usuarioActual.Id;

                var metricas = ReporteService.ObtenerMetricas(fechaInicio, fechaFin, usuarioIdFiltro);
                var ventasEmpleados = usuarioActual.EsAdmin() ?
                    ReporteService.ObtenerVentasPorEmpleado(fechaInicio, fechaFin) :
                    new List<ReporteService.VentaPorEmpleado>();
                var topProductos = ReporteService.ObtenerProductosMasVendidos(fechaInicio, fechaFin, 10, usuarioIdFiltro);

                ExportarPdfService.ExportarReporte(metricas, ventasEmpleados, topProductos,
                                                  fechaInicio, fechaFin, usuarioActual.EsAdmin(),
                                                  plotVentasPorDia, plotTopProductos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}