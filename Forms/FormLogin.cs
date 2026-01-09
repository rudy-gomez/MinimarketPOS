using MinimarketPOS.Models;
using MinimarketPOS.Services;
using System.IO;

namespace MinimarketPOS.Forms
{
    public partial class FormLogin : Form
    {
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Button btnIngresar;
        private Label lblTitulo;
        private Label lblUsuario;
        private Label lblPassword;

        public FormLogin()
        {
            ConfigurarFormulario();
        }

        private void ConfigurarFormulario()
        {
            // Configuración de la ventana
            this.Text = "Login - Minimarket POS";
            string iconPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "assets",
                "Minimarket.ico"
            );

            this.Icon = new Icon(iconPath);
            this.Size = new Size(450, 430);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Panel contenedor
            Panel panelCentral = new Panel();
            panelCentral.Size = new Size(380, 310);
            panelCentral.Location = new Point(35, 35);
            panelCentral.BackColor = Color.White;
            panelCentral.BorderStyle = BorderStyle.FixedSingle;

            // Título
            lblTitulo = new Label();
            lblTitulo.Text = "MINIMARKET POS";
            lblTitulo.Font = new Font("Arial", 18, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(33, 150, 243);
            lblTitulo.Location = new Point(80, 30);
            lblTitulo.Size = new Size(220, 30);
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

            Label lblSubtitulo = new Label();
            lblSubtitulo.Text = "Sistema de Punto de Venta";
            lblSubtitulo.Font = new Font("Arial", 9);
            lblSubtitulo.ForeColor = Color.Gray;
            lblSubtitulo.Location = new Point(80, 60);
            lblSubtitulo.Size = new Size(220, 20);
            lblSubtitulo.TextAlign = ContentAlignment.MiddleCenter;

            // Usuario
            lblUsuario = new Label();
            lblUsuario.Text = "Usuario:";
            lblUsuario.Font = new Font("Arial", 10);
            lblUsuario.Location = new Point(50, 110);
            lblUsuario.Size = new Size(80, 20);

            txtUsuario = new TextBox();
            txtUsuario.Location = new Point(50, 135);
            txtUsuario.Size = new Size(280, 30);
            txtUsuario.Font = new Font("Arial", 11);

            // Contraseña
            lblPassword = new Label();
            lblPassword.Text = "Contraseña:";
            lblPassword.Font = new Font("Arial", 10);
            lblPassword.Location = new Point(50, 175);
            lblPassword.Size = new Size(100, 20);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(50, 200);
            txtPassword.Size = new Size(280, 30);
            txtPassword.Font = new Font("Arial", 11);
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.KeyPress += TxtPassword_KeyPress;

            // Botón Ingresar
            btnIngresar = new Button();
            btnIngresar.Text = "INGRESAR";
            btnIngresar.Location = new Point(115, 245);
            btnIngresar.Size = new Size(150, 40);
            btnIngresar.Font = new Font("Arial", 11, FontStyle.Bold);
            btnIngresar.BackColor = Color.FromArgb(33, 150, 243);
            btnIngresar.ForeColor = Color.White;
            btnIngresar.FlatStyle = FlatStyle.Flat;
            btnIngresar.FlatAppearance.BorderSize = 0;
            btnIngresar.Cursor = Cursors.Hand;
            btnIngresar.Click += BtnIngresar_Click;

            // Hover effect
            btnIngresar.MouseEnter += (s, e) => btnIngresar.BackColor = Color.FromArgb(25, 118, 210);
            btnIngresar.MouseLeave += (s, e) => btnIngresar.BackColor = Color.FromArgb(33, 150, 243);

            // Agregar controles al panel
            panelCentral.Controls.Add(lblTitulo);
            panelCentral.Controls.Add(lblSubtitulo);
            panelCentral.Controls.Add(lblUsuario);
            panelCentral.Controls.Add(txtUsuario);
            panelCentral.Controls.Add(lblPassword);
            panelCentral.Controls.Add(txtPassword);
            panelCentral.Controls.Add(btnIngresar);

            // Agregar panel al formulario
            this.Controls.Add(panelCentral);

            // Focus inicial
            txtUsuario.Focus();
        }

        private void TxtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Presionar Enter = Click en Ingresar
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                BtnIngresar_Click(sender, e);
            }
        }

        private void BtnIngresar_Click(object sender, EventArgs e)
        {
            string username = txtUsuario.Text.Trim();
            string password = txtPassword.Text;

            // Validaciones
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Por favor ingrese su usuario",
                              "Validación",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                txtUsuario.Focus();
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Por favor ingrese su contraseña",
                              "Validación",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // 🔄 LOADING
            btnIngresar.Text = "Cargando...";
            btnIngresar.Enabled = false;
            btnIngresar.Refresh();

            // Validar credenciales
            Usuario? usuario = AuthService.Login(username, password);

            if (usuario != null)
            {
                this.Hide();

                FormPrincipal formPrincipal = new FormPrincipal(usuario);

                // 🔑 cuando el principal se cierre, volver al login
                formPrincipal.FormClosed += (s, args) =>
                {
                    txtUsuario.Clear();
                    txtPassword.Clear();
                    txtUsuario.Focus();
                    btnIngresar.Enabled = true;
                    btnIngresar.Text = "INGRESAR";
                    this.Show();
                };

                formPrincipal.Show();
            }
            else
            {
                // ❌ Login fallido - RESTAURAR BOTÓN
                MessageBox.Show("Usuario o contraseña incorrectos",
                              "Error de autenticación",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                txtPassword.Clear();
                txtPassword.Focus();
                btnIngresar.Enabled = true;
                btnIngresar.Text = "INGRESAR";
            }
        }
    }
}