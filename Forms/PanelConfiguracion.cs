using MinimarketPOS.Models;
using MinimarketPOS.Services;

namespace MinimarketPOS.Forms
{
    public class PanelConfiguracion : Panel
    {
        private Usuario usuarioActual;

        // Controles
        private TextBox txtNombreNegocio;
        private TextBox txtDireccion;
        private TextBox txtCiudad;
        private TextBox txtRuc;
        private TextBox txtMensajeTicket;
        private Button btnGuardar;
        private Button btnCambiarPassword;

        public PanelConfiguracion(Usuario usuario)
        {
            this.usuarioActual = usuario;
            this.BackColor = Color.FromArgb(240, 240, 240);
            ConfigurarPanel();
            CargarConfiguracion();
        }

        private void ConfigurarPanel()
        {
            // Panel con scroll
            this.AutoScroll = true;

            int posY = 30;
            int margenIzq = 80;

            // Título
            Label lblTitulo = new Label();
            lblTitulo.Text = "⚙️ CONFIGURACIÓN DEL SISTEMA";
            lblTitulo.Font = new Font("Arial", 20, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(52, 73, 94);
            lblTitulo.Location = new Point(margenIzq, posY);
            lblTitulo.AutoSize = true;
            this.Controls.Add(lblTitulo);
            posY += 50;

            // Subtítulo
            Label lblSubtitulo = new Label();
            lblSubtitulo.Text = "Datos del negocio que aparecerán en los tickets de venta";
            lblSubtitulo.Font = new Font("Arial", 10);
            lblSubtitulo.ForeColor = Color.Gray;
            lblSubtitulo.Location = new Point(margenIzq, posY);
            lblSubtitulo.AutoSize = true;
            this.Controls.Add(lblSubtitulo);
            posY += 40;

            // Panel contenedor blanco
            Panel panelForm = new Panel();
            panelForm.BackColor = Color.White;
            panelForm.Location = new Point(margenIzq, posY);
            panelForm.Size = new Size(1000, 520);
            panelForm.BorderStyle = BorderStyle.FixedSingle;

            int posYInterno = 30;
            int margenInterno = 40;

            // Nombre del negocio
            Label lblNombre = new Label();
            lblNombre.Text = "Nombre del Negocio:";
            lblNombre.Font = new Font("Arial", 10, FontStyle.Bold);
            lblNombre.Location = new Point(margenInterno, posYInterno);
            lblNombre.Size = new Size(200, 20);
            panelForm.Controls.Add(lblNombre);
            posYInterno += 25;

            txtNombreNegocio = new TextBox();
            txtNombreNegocio.Location = new Point(margenInterno, posYInterno);
            txtNombreNegocio.Size = new Size(920, 30);
            txtNombreNegocio.Font = new Font("Arial", 12);
            panelForm.Controls.Add(txtNombreNegocio);
            posYInterno += 50;

            // Dirección
            Label lblDireccion = new Label();
            lblDireccion.Text = "Dirección:";
            lblDireccion.Font = new Font("Arial", 10, FontStyle.Bold);
            lblDireccion.Location = new Point(margenInterno, posYInterno);
            lblDireccion.Size = new Size(200, 20);
            panelForm.Controls.Add(lblDireccion);
            posYInterno += 25;

            txtDireccion = new TextBox();
            txtDireccion.Location = new Point(margenInterno, posYInterno);
            txtDireccion.Size = new Size(640, 25);
            txtDireccion.Font = new Font("Arial", 11);
            panelForm.Controls.Add(txtDireccion);
            posYInterno += 45;

            // Ciudad
            Label lblCiudad = new Label();
            lblCiudad.Text = "Ciudad:";
            lblCiudad.Font = new Font("Arial", 10, FontStyle.Bold);
            lblCiudad.Location = new Point(margenInterno, posYInterno);
            lblCiudad.Size = new Size(200, 20);
            panelForm.Controls.Add(lblCiudad);
            posYInterno += 25;

            txtCiudad = new TextBox();
            txtCiudad.Location = new Point(margenInterno, posYInterno);
            txtCiudad.Size = new Size(930, 30);
            txtCiudad.Font = new Font("Arial", 11);
            panelForm.Controls.Add(txtCiudad);
            posYInterno += 45;

            // RUC
            Label lblRuc = new Label();
            lblRuc.Text = "RUC:";
            lblRuc.Font = new Font("Arial", 10, FontStyle.Bold);
            lblRuc.Location = new Point(margenInterno, posYInterno);
            lblRuc.Size = new Size(200, 20);
            panelForm.Controls.Add(lblRuc);
            posYInterno += 25;

            txtRuc = new TextBox();
            txtRuc.Location = new Point(margenInterno, posYInterno);
            txtRuc.Size = new Size(300, 30);
            txtRuc.Font = new Font("Arial", 11);
            txtRuc.MaxLength = 11;
            panelForm.Controls.Add(txtRuc);
            posYInterno += 45;

            // Mensaje ticket
            Label lblMensaje = new Label();
            lblMensaje.Text = "Mensaje del Ticket:";
            lblMensaje.Font = new Font("Arial", 10, FontStyle.Bold);
            lblMensaje.Location = new Point(margenInterno, posYInterno);
            lblMensaje.Size = new Size(200, 20);
            panelForm.Controls.Add(lblMensaje);
            posYInterno += 25;

            txtMensajeTicket = new TextBox();
            txtMensajeTicket.Location = new Point(margenInterno, posYInterno);
            txtMensajeTicket.Size = new Size(920, 30);
            txtMensajeTicket.Font = new Font("Arial", 11);
            panelForm.Controls.Add(txtMensajeTicket);
            posYInterno += 65;

            // Línea separadora
            Panel separador = new Panel();
            separador.Height = 2;
            separador.Width = 920;
            separador.BackColor = Color.FromArgb(220, 220, 220);
            separador.Location = new Point(margenInterno, posYInterno);
            panelForm.Controls.Add(separador);
            posYInterno += 25;

            // Botón Cambiar Contraseña
            btnCambiarPassword = new Button();
            btnCambiarPassword.Text = "🔒 Cambiar Contraseña";
            btnCambiarPassword.Location = new Point(margenInterno, posYInterno);
            btnCambiarPassword.Size = new Size(240, 45);
            btnCambiarPassword.Font = new Font("Arial", 10);
            btnCambiarPassword.BackColor = Color.FromArgb(255, 152, 0);
            btnCambiarPassword.ForeColor = Color.White;
            btnCambiarPassword.FlatStyle = FlatStyle.Flat;
            btnCambiarPassword.FlatAppearance.BorderSize = 0;
            btnCambiarPassword.Cursor = Cursors.Hand;
            btnCambiarPassword.TextAlign = ContentAlignment.MiddleCenter;
            btnCambiarPassword.Click += BtnCambiarPassword_Click;
            panelForm.Controls.Add(btnCambiarPassword);

            // Botón Guardar
            btnGuardar = new Button();
            btnGuardar.Text = "💾 Guardar Cambios";
            btnGuardar.Location = new Point(700, posYInterno);
            btnGuardar.Size = new Size(260, 45);
            btnGuardar.Font = new Font("Arial", 12, FontStyle.Bold);
            btnGuardar.BackColor = Color.FromArgb(76, 175, 80);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Cursor = Cursors.Hand;
            btnGuardar.TextAlign = ContentAlignment.MiddleCenter;
            btnGuardar.Click += BtnGuardar_Click;
            panelForm.Controls.Add(btnGuardar);

            this.Controls.Add(panelForm);
        }

        private void CargarConfiguracion()
        {
            try
            {
                var config = ConfiguracionService.ObtenerTodo();

                txtNombreNegocio.Text = config.ContainsKey("nombre_negocio") ? config["nombre_negocio"] : "";
                txtDireccion.Text = config.ContainsKey("direccion") ? config["direccion"] : "";
                txtCiudad.Text = config.ContainsKey("ciudad") ? config["ciudad"] : "";
                txtRuc.Text = config.ContainsKey("ruc") ? config["ruc"] : "";
                txtMensajeTicket.Text = config.ContainsKey("mensaje_ticket") ? config["mensaje_ticket"] : "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar configuración: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(txtNombreNegocio.Text))
                {
                    MessageBox.Show("El nombre del negocio es obligatorio",
                                  "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNombreNegocio.Focus();
                    return;
                }

                // Guardar configuración
                ConfiguracionService.ActualizarValor("nombre_negocio", txtNombreNegocio.Text.Trim(), usuarioActual.Id);
                ConfiguracionService.ActualizarValor("direccion", txtDireccion.Text.Trim(), usuarioActual.Id);
                ConfiguracionService.ActualizarValor("ciudad", txtCiudad.Text.Trim(), usuarioActual.Id);
                ConfiguracionService.ActualizarValor("ruc", txtRuc.Text.Trim(), usuarioActual.Id);
                ConfiguracionService.ActualizarValor("mensaje_ticket", txtMensajeTicket.Text.Trim(), usuarioActual.Id);

                MessageBox.Show("✅ Configuración guardada correctamente",
                              "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCambiarPassword_Click(object sender, EventArgs e)
        {
            // Formulario modal para cambiar contraseña
            Form dialogPassword = new Form();
            dialogPassword.Text = "Cambiar Contraseña";
            dialogPassword.Size = new Size(450, 320);
            dialogPassword.StartPosition = FormStartPosition.CenterScreen;
            dialogPassword.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialogPassword.MaximizeBox = false;
            dialogPassword.MinimizeBox = false;
            dialogPassword.BackColor = Color.White;

            int posY = 30;

            Label lblTitulo = new Label();
            lblTitulo.Text = "🔒 Cambiar Contraseña";
            lblTitulo.Font = new Font("Arial", 14, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(33, 150, 243);
            lblTitulo.Location = new Point(30, posY);
            lblTitulo.AutoSize = true;
            dialogPassword.Controls.Add(lblTitulo);
            posY += 50;

            Label lblActual = new Label();
            lblActual.Text = "Contraseña Actual:";
            lblActual.Font = new Font("Arial", 10);
            lblActual.Location = new Point(30, posY);
            lblActual.Size = new Size(150, 20);
            dialogPassword.Controls.Add(lblActual);
            posY += 25;

            TextBox txtActual = new TextBox();
            txtActual.Location = new Point(30, posY);
            txtActual.Size = new Size(380, 25);
            txtActual.Font = new Font("Arial", 11);
            txtActual.UseSystemPasswordChar = true;
            dialogPassword.Controls.Add(txtActual);
            posY += 40;

            Label lblNueva = new Label();
            lblNueva.Text = "Contraseña Nueva:";
            lblNueva.Font = new Font("Arial", 10);
            lblNueva.Location = new Point(30, posY);
            lblNueva.Size = new Size(150, 20);
            dialogPassword.Controls.Add(lblNueva);
            posY += 25;

            TextBox txtNueva = new TextBox();
            txtNueva.Location = new Point(30, posY);
            txtNueva.Size = new Size(380, 25);
            txtNueva.Font = new Font("Arial", 11);
            txtNueva.UseSystemPasswordChar = true;
            dialogPassword.Controls.Add(txtNueva);
            posY += 55;

            Button btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(210, posY);
            btnCancelar.Size = new Size(100, 38);
            btnCancelar.Font = new Font("Arial", 10);
            btnCancelar.BackColor = Color.Gray;
            btnCancelar.ForeColor = Color.White;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.Click += (s, ev) => dialogPassword.Close();
            dialogPassword.Controls.Add(btnCancelar);

            Button btnConfirmar = new Button();
            btnConfirmar.Text = "Cambiar";
            btnConfirmar.Location = new Point(320, posY);
            btnConfirmar.Size = new Size(100, 38);
            btnConfirmar.Font = new Font("Arial", 10, FontStyle.Bold);
            btnConfirmar.BackColor = Color.FromArgb(33, 150, 243);
            btnConfirmar.ForeColor = Color.White;
            btnConfirmar.FlatStyle = FlatStyle.Flat;
            btnConfirmar.FlatAppearance.BorderSize = 0;
            btnConfirmar.Cursor = Cursors.Hand;
            btnConfirmar.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtActual.Text) || string.IsNullOrWhiteSpace(txtNueva.Text))
                {
                    MessageBox.Show("Complete todos los campos", "Validación",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtNueva.Text.Length < 6)
                {
                    MessageBox.Show("La contraseña debe tener al menos 6 caracteres",
                                  "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool resultado = AuthService.CambiarPassword(
                    usuarioActual.Id,
                    txtActual.Text,
                    txtNueva.Text);

                if (resultado)
                {
                    MessageBox.Show("✅ Contraseña cambiada correctamente",
                                  "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dialogPassword.Close();
                }
            };
            dialogPassword.Controls.Add(btnConfirmar);

            dialogPassword.ShowDialog();
        }
    }
}