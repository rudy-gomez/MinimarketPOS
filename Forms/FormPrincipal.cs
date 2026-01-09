using MinimarketPOS.Models;
using MinimarketPOS.Services;
using System.IO;

namespace MinimarketPOS.Forms
{
    public partial class FormPrincipal : Form
    {
        private Usuario usuarioActual;
        private Panel panelMenu;
        private Panel panelContenido;
        private Label lblUsuarioInfo;
        private Button botonActivo;
        private Button btnCerrarSesion; // ← Guardamos referencia

        private PanelVentas? panelVentasActual;
        private PanelUsuarios? panelUsuariosActual;
        private PanelConfiguracion? panelConfigActual;
        private PanelReportes? panelReportesActual;
        private PanelInventario? panelInventarioActual;

        public FormPrincipal(Usuario usuario)
        {
            this.usuarioActual = usuario;
            ConfigurarFormulario();
            CrearMenu();
            MostrarBienvenida();
        }

        private void ConfigurarFormulario()
        {
            this.Text = $"Minimarket POS - {usuarioActual.Nombre} ({usuarioActual.Rol})";
            string iconPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "assets",
                "Minimarket.ico"
            );

            this.Icon = new Icon(iconPath);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.Manual;

            // ← Ajustar al área de trabajo (SIN tapar barra de tareas)
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(workingArea.X, workingArea.Y);
            this.Size = new Size(workingArea.Width, workingArea.Height);

            // Panel lateral (menú)
            panelMenu = new Panel();
            panelMenu.Dock = DockStyle.Left;
            panelMenu.Width = 280;
            panelMenu.BackColor = Color.FromArgb(45, 45, 48);

            // Panel contenido
            panelContenido = new Panel();
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.BackColor = Color.FromArgb(240, 240, 240);

            this.Controls.Add(panelContenido);
            this.Controls.Add(panelMenu);

            this.Load += FormPrincipal_Load;
            this.Resize += FormPrincipal_Resize;
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            PosicionarBotonCerrarSesion();
        }

        private void FormPrincipal_Resize(object sender, EventArgs e)
        {
            PosicionarBotonCerrarSesion();
        }

        private void PosicionarBotonCerrarSesion()
        {
            if (btnCerrarSesion != null)
            {
                btnCerrarSesion.Location = new Point(15, panelMenu.Height - 70);
            }
        }

        private void CrearMenu()
        {
            int posY = 20;
            int anchoBoton = 250; // ← Botones más anchos
            int margenIzq = 15;

            // Logo/Título
            Label lblLogo = new Label();
            lblLogo.Text = "MINIMARKET\nPOS";
            lblLogo.Font = new Font("Arial", 16, FontStyle.Bold); // ← Más grande
            lblLogo.ForeColor = Color.White;
            lblLogo.Location = new Point(margenIzq, posY);
            lblLogo.Size = new Size(anchoBoton, 70);
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            panelMenu.Controls.Add(lblLogo);
            posY += 80;

            // Información del usuario
            lblUsuarioInfo = new Label();
            lblUsuarioInfo.Text = $"👤 {usuarioActual.Nombre}\n{usuarioActual.Rol.ToUpper()}";
            lblUsuarioInfo.Font = new Font("Arial", 10); // ← Más grande
            lblUsuarioInfo.ForeColor = Color.LightGray;
            lblUsuarioInfo.Location = new Point(margenIzq, posY);
            lblUsuarioInfo.Size = new Size(anchoBoton, 45);
            lblUsuarioInfo.TextAlign = ContentAlignment.MiddleLeft;
            panelMenu.Controls.Add(lblUsuarioInfo);
            posY += 60;

            // Separador
            Panel separador = new Panel();
            separador.Height = 2;
            separador.Width = anchoBoton;
            separador.BackColor = Color.FromArgb(80, 80, 84);
            separador.Location = new Point(margenIzq, posY);
            panelMenu.Controls.Add(separador);
            posY += 25;

            // BOTÓN: Punto de Venta (TODOS)
            Button btnVentas = CrearBotonMenu("🛒 Punto de Venta", posY);
            btnVentas.Click += (s, e) =>
            {
                MarcarBotonActivo(btnVentas);
                MostrarPanelVentas();
            };
            panelMenu.Controls.Add(btnVentas);
            posY += 55;

            // BOTÓN: Inventario (TODOS)
            Button btnInventario = CrearBotonMenu("📦 Inventario", posY);
            btnInventario.Click += (s, e) =>
            {
                MarcarBotonActivo(btnInventario);
                MostrarPanelInventario();
            };
            panelMenu.Controls.Add(btnInventario);
            posY += 55;

            Button btnReportes = CrearBotonMenu("📊 Reportes", posY);
            btnReportes.Click += (s, e) =>
            {
                MarcarBotonActivo(btnReportes);
                MostrarPanelReportes();
            };
            panelMenu.Controls.Add(btnReportes);
            posY += 55;

            // ========== SOLO ADMIN ==========
            if (usuarioActual.EsAdmin())
            {
                posY += 15;
                Label lblAdmin = new Label();
                lblAdmin.Text = "ADMINISTRACIÓN";
                lblAdmin.Font = new Font("Arial", 9); // ← Más grande
                lblAdmin.ForeColor = Color.Gray;
                lblAdmin.Location = new Point(margenIzq, posY);
                lblAdmin.Size = new Size(anchoBoton, 18);
                panelMenu.Controls.Add(lblAdmin);
                posY += 30;

                Button btnUsuarios = CrearBotonMenu("👥 Usuarios", posY);
                btnUsuarios.Click += (s, e) =>
                {
                    MarcarBotonActivo(btnUsuarios);
                    MostrarPanelUsuarios();
                };
                panelMenu.Controls.Add(btnUsuarios);
                posY += 55;

                Button btnConfiguracion = CrearBotonMenu("⚙️ Configuración", posY);
                btnConfiguracion.Click += (s, e) =>
                {
                    MarcarBotonActivo(btnConfiguracion);
                    MostrarPanelConfiguracion();
                };
                panelMenu.Controls.Add(btnConfiguracion);
            }

            // Botón Cerrar Sesión - NO SE POSICIONA AQUÍ
            btnCerrarSesion = CrearBotonMenu("🚪 Cerrar Sesión", 0); // ← posY temporal
            btnCerrarSesion.BackColor = Color.FromArgb(183, 28, 28);
            btnCerrarSesion.Click += BtnCerrarSesion_Click;
            panelMenu.Controls.Add(btnCerrarSesion);
            // Se posicionará en FormPrincipal_Load cuando ya tengamos la altura real
        }

        private Button CrearBotonMenu(string texto, int posY)
        {
            Button btn = new Button();
            btn.Text = texto;
            btn.Size = new Size(250, 45); // ← Más grande (era 190x40)
            btn.Location = new Point(15, posY);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.FromArgb(62, 62, 66);
            btn.ForeColor = Color.White;
            btn.Font = new Font("Arial", 11); // ← Más grande (era 10)
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 0, 0, 0); // ← Más padding
            btn.Cursor = Cursors.Hand;
            btn.Tag = "menu";

            // Hover effect
            btn.MouseEnter += (s, e) =>
            {
                if (btn != botonActivo && !btn.Text.Contains("Cerrar"))
                {
                    btn.BackColor = Color.FromArgb(80, 80, 84);
                }
            };

            btn.MouseLeave += (s, e) =>
            {
                if (btn.Text.Contains("Cerrar"))
                {
                    btn.BackColor = Color.FromArgb(183, 28, 28);
                }
                else if (btn != botonActivo)
                {
                    btn.BackColor = Color.FromArgb(62, 62, 66);
                }
            };

            return btn;
        }

        private void MarcarBotonActivo(Button botonSeleccionado)
        {
            // Restaurar el botón anterior a su estado normal
            if (botonActivo != null && botonActivo != botonSeleccionado)
            {
                botonActivo.BackColor = Color.FromArgb(62, 62, 66);
                botonActivo.Font = new Font("Arial", 11, FontStyle.Regular);
            }

            // Marcar el nuevo botón como activo
            botonActivo = botonSeleccionado;
            botonActivo.BackColor = Color.FromArgb(90, 90, 94);
            botonActivo.Font = new Font("Arial", 11, FontStyle.Bold);
        }

        private void MostrarBienvenida()
        {
            Button? btnVentas = panelMenu.Controls.OfType<Button>()
               .FirstOrDefault(b => b.Text.Contains("Punto de Venta"));

            if (btnVentas != null)
            {
                MarcarBotonActivo(btnVentas);
                MostrarPanelVentas();
            }
        }

        private void CargarPanel(Control panel)
        {
            panelContenido.Controls.Clear();
            panel.Dock = DockStyle.Fill;
            panelContenido.Controls.Add(panel);
        }

        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                AuditoriaService.RegistrarAccion(
                    usuarioActual.Id,
                    "logout",
                    "usuarios",
                    usuarioActual.Id,
                    "Cierre de sesión"
                );

                panelVentasActual = null;
                panelUsuariosActual = null;
                panelConfigActual = null;
                panelReportesActual = null;
                panelInventarioActual = null;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void MostrarPanelVentas()
        {
            if (panelVentasActual == null)
            {
                panelVentasActual = new PanelVentas(usuarioActual);
            }
            CargarPanel(panelVentasActual);
        }

        private void MostrarPanelInventario()
        {
            if (panelInventarioActual == null)
            {
                panelInventarioActual = new PanelInventario(usuarioActual);
            }
            CargarPanel(panelInventarioActual);
        }

        private void MostrarPanelUsuarios()
        {
            panelUsuariosActual = new PanelUsuarios(usuarioActual);
            CargarPanel(panelUsuariosActual);
        }

        private void MostrarPanelConfiguracion()
        {
            panelConfigActual = new PanelConfiguracion(usuarioActual);
            CargarPanel(panelConfigActual);
        }

        private void InitializeComponent()
        {

        }

        private void MostrarPanelReportes()
        {
            panelReportesActual = new PanelReportes(usuarioActual);
            CargarPanel(panelReportesActual);
        }
    }
}