using MinimarketPOS.Models;
using MinimarketPOS.Services;

namespace MinimarketPOS.Forms
{
    public class PanelUsuarios : Panel
    {
        private Usuario usuarioActual;
        private DataGridView gridUsuarios;
        private Button btnNuevo;
        private Button btnRefrescar;
        private Button btnEditar;
        private Button btnCambiarPassword;
        private Button btnActivarDesactivar;

        public PanelUsuarios(Usuario usuario)
        {
            this.usuarioActual = usuario;
            this.BackColor = Color.FromArgb(240, 240, 240);
            ConfigurarPanel();
            CargarUsuarios();
        }

        private void ConfigurarPanel()
        {
            this.AutoScroll = true;

            int posY = 30;
            int margenIzq = 80;

            // Título
            Label lblTitulo = new Label();
            lblTitulo.Text = "👥 GESTIÓN DE USUARIOS";
            lblTitulo.Font = new Font("Arial", 20, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(52, 73, 94);
            lblTitulo.Location = new Point(margenIzq, posY);
            lblTitulo.AutoSize = true;
            this.Controls.Add(lblTitulo);
            posY += 50;

            // Subtítulo
            Label lblSubtitulo = new Label();
            lblSubtitulo.Text = "Administra los usuarios que tienen acceso al sistema";
            lblSubtitulo.Font = new Font("Arial", 10);
            lblSubtitulo.ForeColor = Color.Gray;
            lblSubtitulo.Location = new Point(margenIzq, posY);
            lblSubtitulo.AutoSize = true;
            this.Controls.Add(lblSubtitulo);
            posY += 40;

            // Botones superiores
            btnNuevo = new Button();
            btnNuevo.Text = "➕ Nuevo Usuario";
            btnNuevo.Location = new Point(margenIzq, posY);
            btnNuevo.Size = new Size(170, 45);
            btnNuevo.Font = new Font("Arial", 11, FontStyle.Bold);
            btnNuevo.BackColor = Color.FromArgb(76, 175, 80);
            btnNuevo.ForeColor = Color.White;
            btnNuevo.FlatStyle = FlatStyle.Flat;
            btnNuevo.FlatAppearance.BorderSize = 0;
            btnNuevo.Cursor = Cursors.Hand;
            btnNuevo.Click += BtnNuevo_Click;
            this.Controls.Add(btnNuevo);

            btnRefrescar = new Button();
            btnRefrescar.Text = "🔄 Actualizar";
            btnRefrescar.Location = new Point(margenIzq + 180, posY);
            btnRefrescar.Size = new Size(140, 45);
            btnRefrescar.Font = new Font("Arial", 11);
            btnRefrescar.BackColor = Color.FromArgb(33, 150, 243);
            btnRefrescar.ForeColor = Color.White;
            btnRefrescar.FlatStyle = FlatStyle.Flat;
            btnRefrescar.FlatAppearance.BorderSize = 0;
            btnRefrescar.Cursor = Cursors.Hand;
            btnRefrescar.Click += (s, e) => CargarUsuarios();
            this.Controls.Add(btnRefrescar);
            posY += 65;

            // DataGridView
            gridUsuarios = new DataGridView();
            gridUsuarios.Location = new Point(margenIzq, posY);
            gridUsuarios.Size = new Size(1150, 480);
            gridUsuarios.BackgroundColor = Color.White;
            gridUsuarios.BorderStyle = BorderStyle.FixedSingle;
            gridUsuarios.AllowUserToAddRows = false;
            gridUsuarios.AllowUserToDeleteRows = false;
            gridUsuarios.ReadOnly = true;
            gridUsuarios.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridUsuarios.MultiSelect = false;
            gridUsuarios.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridUsuarios.RowHeadersVisible = false;
            gridUsuarios.Font = new Font("Arial", 11);
            gridUsuarios.RowTemplate.Height = 35;

            // Configurar columnas
            gridUsuarios.Columns.Add("id", "ID");
            gridUsuarios.Columns["id"].FillWeight = 8;

            gridUsuarios.Columns.Add("username", "Usuario");
            gridUsuarios.Columns["username"].FillWeight = 20;

            gridUsuarios.Columns.Add("nombre", "Nombre Completo");
            gridUsuarios.Columns["nombre"].FillWeight = 35;

            gridUsuarios.Columns.Add("rol", "Rol");
            gridUsuarios.Columns["rol"].FillWeight = 15;

            gridUsuarios.Columns.Add("activo", "Estado");
            gridUsuarios.Columns["activo"].FillWeight = 12;

            gridUsuarios.Columns.Add("fecha_creacion", "Fecha Creación");
            gridUsuarios.Columns["fecha_creacion"].FillWeight = 20;

            // Centrar contenido
            gridUsuarios.Columns["id"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridUsuarios.Columns["rol"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridUsuarios.Columns["activo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.Controls.Add(gridUsuarios);
            posY += 500;

            // Botones de acción
            btnEditar = new Button();
            btnEditar.Text = "✏️ Editar";
            btnEditar.Location = new Point(margenIzq, posY);
            btnEditar.Size = new Size(130, 45);
            btnEditar.Font = new Font("Arial", 10);
            btnEditar.BackColor = Color.FromArgb(255, 152, 0);
            btnEditar.ForeColor = Color.White;
            btnEditar.FlatStyle = FlatStyle.Flat;
            btnEditar.FlatAppearance.BorderSize = 0;
            btnEditar.Cursor = Cursors.Hand;
            btnEditar.TextAlign = ContentAlignment.MiddleCenter;
            btnEditar.Click += BtnEditar_Click;
            this.Controls.Add(btnEditar);

            btnCambiarPassword = new Button();
            btnCambiarPassword.Text = "🔒 Cambiar Contraseña";
            btnCambiarPassword.Location = new Point(margenIzq + 140, posY);
            btnCambiarPassword.Size = new Size(210, 45);
            btnCambiarPassword.Font = new Font("Arial", 10);
            btnCambiarPassword.BackColor = Color.FromArgb(156, 39, 176);
            btnCambiarPassword.ForeColor = Color.White;
            btnCambiarPassword.FlatStyle = FlatStyle.Flat;
            btnCambiarPassword.FlatAppearance.BorderSize = 0;
            btnCambiarPassword.Cursor = Cursors.Hand;
            btnCambiarPassword.TextAlign = ContentAlignment.MiddleCenter;
            btnCambiarPassword.Click += BtnCambiarPassword_Click;
            this.Controls.Add(btnCambiarPassword);

            btnActivarDesactivar = new Button();
            btnActivarDesactivar.Text = "🔴 Desactivar";
            btnActivarDesactivar.Location = new Point(margenIzq + 360, posY);
            btnActivarDesactivar.Size = new Size(150, 45);
            btnActivarDesactivar.Font = new Font("Arial", 10);
            btnActivarDesactivar.BackColor = Color.FromArgb(244, 67, 54);
            btnActivarDesactivar.ForeColor = Color.White;
            btnActivarDesactivar.FlatStyle = FlatStyle.Flat;
            btnActivarDesactivar.FlatAppearance.BorderSize = 0;
            btnActivarDesactivar.Cursor = Cursors.Hand;
            btnActivarDesactivar.TextAlign = ContentAlignment.MiddleCenter;
            btnActivarDesactivar.Click += BtnActivarDesactivar_Click;
            this.Controls.Add(btnActivarDesactivar);

            Button btnEliminar = new Button();
            btnEliminar.Text = "🗑️ Eliminar";
            btnEliminar.Location = new Point(margenIzq + 520, posY);
            btnEliminar.Size = new Size(130, 45);
            btnEliminar.Font = new Font("Arial", 10);
            btnEliminar.BackColor = Color.FromArgb(183, 28, 28);
            btnEliminar.ForeColor = Color.White;
            btnEliminar.FlatStyle = FlatStyle.Flat;
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Cursor = Cursors.Hand;
            btnEliminar.TextAlign = ContentAlignment.MiddleCenter;
            btnEliminar.Click += BtnEliminar_Click;
            this.Controls.Add(btnEliminar);

            // Agregar evento SelectionChanged
            gridUsuarios.SelectionChanged += GridUsuarios_SelectionChanged;
        }

        private void CargarUsuarios()
        {
            try
            {
                gridUsuarios.Rows.Clear();
                List<Usuario> usuarios = UsuarioService.ListarTodos();

                foreach (var user in usuarios)
                {
                    string estado = user.Activo ? "✅ Activo" : "❌ Inactivo";
                    string rol = user.Rol == "admin" ? "👑 Admin" : "👤 Empleado";

                    gridUsuarios.Rows.Add(
                        user.Id,
                        user.Username,
                        user.Nombre,
                        rol,
                        estado,
                        user.FechaCreacion.ToString("dd/MM/yyyy HH:mm")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnNuevo_Click(object sender, EventArgs e)
        {
            MostrarFormularioUsuario(null);
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (gridUsuarios.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un usuario", "Información",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int usuarioId = Convert.ToInt32(gridUsuarios.SelectedRows[0].Cells["id"].Value);

            // No permitir editar al propio usuario admin desde aquí
            if (usuarioId == usuarioActual.Id)
            {
                MessageBox.Show("No puede editarse a sí mismo desde aquí. Use Configuración para cambiar su contraseña.",
                              "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Buscar usuario
            List<Usuario> usuarios = UsuarioService.ListarTodos();
            Usuario? usuario = usuarios.FirstOrDefault(u => u.Id == usuarioId);

            if (usuario != null)
            {
                MostrarFormularioUsuario(usuario);
            }
        }

        private void BtnCambiarPassword_Click(object sender, EventArgs e)
        {
            if (gridUsuarios.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un usuario", "Información",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int usuarioId = Convert.ToInt32(gridUsuarios.SelectedRows[0].Cells["id"].Value);
            string nombreUsuario = gridUsuarios.SelectedRows[0].Cells["nombre"].Value.ToString();

            // No permitir cambiar contraseña propia desde aquí
            if (usuarioId == usuarioActual.Id)
            {
                MessageBox.Show("No puede cambiar su propia contraseña desde aquí. Use Configuración.",
                              "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Formulario para nueva contraseña
            Form dialog = new Form();
            dialog.Text = "Restablecer Contraseña";
            dialog.Size = new Size(450, 250);
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;
            dialog.BackColor = Color.White;

            Label lblInfo = new Label();
            lblInfo.Text = $"Restablecer contraseña para: {nombreUsuario}";
            lblInfo.Location = new Point(30, 30);
            lblInfo.Size = new Size(380, 20);
            lblInfo.Font = new Font("Arial", 10, FontStyle.Bold);
            dialog.Controls.Add(lblInfo);

            Label lblNueva = new Label();
            lblNueva.Text = "Nueva Contraseña:";
            lblNueva.Location = new Point(30, 70);
            lblNueva.Size = new Size(150, 20);
            dialog.Controls.Add(lblNueva);

            TextBox txtNueva = new TextBox();
            txtNueva.Location = new Point(30, 95);
            txtNueva.Size = new Size(380, 25);
            txtNueva.Font = new Font("Arial", 11);
            txtNueva.UseSystemPasswordChar = true;
            dialog.Controls.Add(txtNueva);

            Button btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(210, 150);
            btnCancelar.Size = new Size(100, 38);
            btnCancelar.BackColor = Color.Gray;
            btnCancelar.ForeColor = Color.White;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 0; 
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.Click += (s, ev) => dialog.Close();
            dialog.Controls.Add(btnCancelar);

            Button btnConfirmar = new Button();
            btnConfirmar.Text = "Cambiar";
            btnConfirmar.Location = new Point(320, 150);
            btnConfirmar.Size = new Size(100, 38);
            btnConfirmar.BackColor = Color.FromArgb(33, 150, 243);
            btnConfirmar.ForeColor = Color.White;
            btnConfirmar.FlatStyle = FlatStyle.Flat;
            btnConfirmar.FlatAppearance.BorderSize = 0;
            btnConfirmar.Cursor = Cursors.Hand;
            btnConfirmar.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtNueva.Text))
                {
                    MessageBox.Show("Ingrese la nueva contraseña", "Validación",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtNueva.Text.Length < 6)
                {
                    MessageBox.Show("La contraseña debe tener al menos 6 caracteres",
                                  "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool resultado = UsuarioService.RestablecerPassword(usuarioId, txtNueva.Text, usuarioActual.Id);

                if (resultado)
                {
                    MessageBox.Show("✅ Contraseña restablecida correctamente",
                                  "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dialog.Close();
                }
            };
            dialog.Controls.Add(btnConfirmar);

            dialog.ShowDialog();
        }

        private void BtnActivarDesactivar_Click(object sender, EventArgs e)
        {
            if (gridUsuarios.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un usuario", "Información",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int usuarioId = Convert.ToInt32(gridUsuarios.SelectedRows[0].Cells["id"].Value);
            string nombreUsuario = gridUsuarios.SelectedRows[0].Cells["nombre"].Value.ToString();
            bool activoActual = gridUsuarios.SelectedRows[0].Cells["activo"].Value.ToString().Contains("Activo");

            // No permitir desactivar al propio usuario
            if (usuarioId == usuarioActual.Id)
            {
                MessageBox.Show("No puede desactivarse a sí mismo",
                              "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string accion = activoActual ? "desactivar" : "activar";
            DialogResult result = MessageBox.Show(
                $"¿Está seguro que desea {accion} al usuario {nombreUsuario}?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                bool exito = UsuarioService.CambiarEstado(usuarioId, !activoActual, usuarioActual.Id);

                if (exito)
                {
                    MessageBox.Show($"✅ Usuario {accion} correctamente",
                                  "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarUsuarios();
                }
            }
        }

        private void MostrarFormularioUsuario(Usuario? usuario)
        {
            bool esEdicion = usuario != null;

            Form dialog = new Form();
            dialog.Text = esEdicion ? "Editar Usuario" : "Nuevo Usuario";
            dialog.Size = new Size(500, 470);
            dialog.StartPosition = FormStartPosition.CenterScreen;
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.MaximizeBox = false;
            dialog.MinimizeBox = false;

            int posY = 30;

            Label lblTitulo = new Label();
            lblTitulo.Text = esEdicion ? "✏️ Editar Usuario" : "➕ Nuevo Usuario";
            lblTitulo.Font = new Font("Arial", 14, FontStyle.Bold);
            lblTitulo.Location = new Point(30, posY);
            lblTitulo.AutoSize = true;
            dialog.Controls.Add(lblTitulo);
            posY += 50;

            // Username
            Label lblUsername = new Label();
            lblUsername.Text = "Nombre de Usuario:";
            lblUsername.Location = new Point(30, posY);
            lblUsername.Size = new Size(150, 20);
            dialog.Controls.Add(lblUsername);
            posY += 25;

            TextBox txtUsername = new TextBox();
            txtUsername.Location = new Point(30, posY);
            txtUsername.Size = new Size(420, 25);
            txtUsername.Font = new Font("Arial", 11);
            txtUsername.ReadOnly = esEdicion; // No editable si es edición
            if (esEdicion) txtUsername.Text = usuario.Username;
            dialog.Controls.Add(txtUsername);
            posY += 40;

            // Nombre completo
            Label lblNombre = new Label();
            lblNombre.Text = "Nombre Completo:";
            lblNombre.Location = new Point(30, posY);
            lblNombre.Size = new Size(150, 20);
            dialog.Controls.Add(lblNombre);
            posY += 25;

            TextBox txtNombre = new TextBox();
            txtNombre.Location = new Point(30, posY);
            txtNombre.Size = new Size(420, 25);
            txtNombre.Font = new Font("Arial", 11);
            if (esEdicion) txtNombre.Text = usuario.Nombre;
            dialog.Controls.Add(txtNombre);
            posY += 40;

            // Contraseña (solo para nuevo)
            TextBox txtPassword = null;
            if (!esEdicion)
            {
                Label lblPassword = new Label();
                lblPassword.Text = "Contraseña:";
                lblPassword.Location = new Point(30, posY);
                lblPassword.Size = new Size(150, 20);
                dialog.Controls.Add(lblPassword);
                posY += 25;

                txtPassword = new TextBox();
                txtPassword.Location = new Point(30, posY);
                txtPassword.Size = new Size(420, 25);
                txtPassword.Font = new Font("Arial", 11);
                txtPassword.UseSystemPasswordChar = true;
                dialog.Controls.Add(txtPassword);
                posY += 40;
            }

            // Rol
            Label lblRol = new Label();
            lblRol.Text = "Rol:";
            lblRol.Location = new Point(30, posY);
            lblRol.Size = new Size(150, 20);
            dialog.Controls.Add(lblRol);
            posY += 25;

            ComboBox cboRol = new ComboBox();
            cboRol.Location = new Point(30, posY);
            cboRol.Size = new Size(200, 25);
            cboRol.Font = new Font("Arial", 11);
            cboRol.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRol.Items.Add("admin");
            cboRol.Items.Add("empleado");
            cboRol.SelectedIndex = esEdicion && usuario.Rol == "admin" ? 0 : 1;
            dialog.Controls.Add(cboRol);
            posY += 50;

            // Botones
            Button btnCancelar = new Button();
            btnCancelar.Text = "Cancelar";
            btnCancelar.Location = new Point(240, posY);
            btnCancelar.Size = new Size(100, 38);
            btnCancelar.BackColor = Color.Gray;
            btnCancelar.ForeColor = Color.White;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.Click += (s, ev) => dialog.Close();
            dialog.Controls.Add(btnCancelar);

            Button btnGuardar = new Button();
            btnGuardar.Text = esEdicion ? "Actualizar" : "Crear";
            btnGuardar.Location = new Point(350, posY);
            btnGuardar.Size = new Size(100, 38);
            btnGuardar.BackColor = Color.FromArgb(76, 175, 80);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.Cursor = Cursors.Hand;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += (s, ev) =>
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("Complete todos los campos obligatorios",
                                  "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!esEdicion && string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Ingrese una contraseña",
                                  "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!esEdicion && txtPassword.Text.Length < 6)
                {
                    MessageBox.Show("La contraseña debe tener al menos 6 caracteres",
                                  "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool exito;
                if (esEdicion)
                {
                    // Por ahora no hay método para editar usuario completo
                    // Solo se puede cambiar contraseña y estado
                    MessageBox.Show("La edición de usuarios aún no está implementada completamente.\n" +
                                  "Use 'Cambiar Contraseña' o 'Activar/Desactivar'.",
                                  "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    exito = false;
                }
                else
                {
                    exito = UsuarioService.Crear(
                        txtUsername.Text.Trim(),
                        txtPassword.Text,
                        txtNombre.Text.Trim(),
                        cboRol.SelectedItem.ToString(),
                        usuarioActual.Id
                    );
                }

                if (exito)
                {
                    MessageBox.Show("✅ Usuario creado correctamente",
                                  "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dialog.Close();
                    CargarUsuarios();
                }
            };
            dialog.Controls.Add(btnGuardar);

            dialog.ShowDialog();
        }

        private void GridUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            if (gridUsuarios.SelectedRows.Count > 0)
            {
                string estadoActual = gridUsuarios.SelectedRows[0].Cells["activo"].Value.ToString();
                bool estaActivo = estadoActual.Contains("Activo");

                if (estaActivo)
                {
                    btnActivarDesactivar.Text = "🔴 Desactivar";
                    btnActivarDesactivar.BackColor = Color.FromArgb(244, 67, 54);
                }
                else
                {
                    btnActivarDesactivar.Text = "✅ Activar";
                    btnActivarDesactivar.BackColor = Color.FromArgb(76, 175, 80);
                }
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (gridUsuarios.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un usuario", "Información",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int usuarioId = Convert.ToInt32(gridUsuarios.SelectedRows[0].Cells["id"].Value);
            string nombreUsuario = gridUsuarios.SelectedRows[0].Cells["nombre"].Value.ToString();
            string rolUsuario = gridUsuarios.SelectedRows[0].Cells["rol"].Value.ToString();

            if (usuarioId == usuarioActual.Id)
            {
                MessageBox.Show("No puede eliminarse a sí mismo",
                              "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mensajeExtra = rolUsuario.Contains("Admin")
                ? "\n\n⚠️ Este es un usuario ADMINISTRADOR."
                : "";

            DialogResult result = MessageBox.Show(
                $"¿Está seguro que desea ELIMINAR al usuario?\n\n" +
                $"👤 {nombreUsuario}\n" +
                $"🔑 Rol: {rolUsuario}" +
                mensajeExtra +
                $"\n\n⚠️ Esta acción no se puede deshacer completamente.",
                "⚠️ CONFIRMAR ELIMINACIÓN",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                bool exito = UsuarioService.Eliminar(usuarioId, usuarioActual.Id);

                if (exito)
                {
                    MessageBox.Show("✅ Usuario eliminado correctamente",
                                  "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarUsuarios();
                }
            }
        }
    }
}