using MinimarketPOS.Models;
using MinimarketPOS.Services;
using MinimarketPOS.Utils;

namespace MinimarketPOS.Forms
{
    public class PanelVentas : Panel
    {
        private Usuario usuarioActual;
        private List<DetalleVenta> detallesVenta;

        // Controles
        private TextBox txtBusqueda;
        private DataGridView dgvDetalles;
        private Label lblSubtotalValor;
        private Label lblTotalValor;
        private TextBox txtEfectivo;
        private Label lblVueltoValor;
        private Button btnGuardar;
        private Button btnCancelar;

        public PanelVentas(Usuario usuario)
        {
            this.usuarioActual = usuario;
            this.detallesVenta = new List<DetalleVenta>();
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.AutoScroll = true;
            ConfigurarPanel();
        }

        private void ConfigurarPanel()
        {
            int margen = 50;
            int posY = 30;

            // Título
            Label lblTitulo = new Label();
            lblTitulo.Text = "🛒 PUNTO DE VENTA";
            lblTitulo.Font = new Font("Arial", 24, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(52, 73, 94);
            lblTitulo.Location = new Point(margen, posY);
            lblTitulo.AutoSize = true;
            this.Controls.Add(lblTitulo);
            posY += 60;

            // Panel de búsqueda
            Panel pnlBusqueda = new Panel();
            pnlBusqueda.BackColor = Color.White;
            pnlBusqueda.Location = new Point(margen, posY);
            pnlBusqueda.Size = new Size(1400, 110);
            pnlBusqueda.BorderStyle = BorderStyle.FixedSingle;

            Label lblIcono = new Label();
            lblIcono.Text = "🔍";
            lblIcono.Font = new Font("Segoe UI", 25);
            lblIcono.Location = new Point(30, 28);
            lblIcono.Size = new Size(70, 50);
            pnlBusqueda.Controls.Add(lblIcono);

            txtBusqueda = new TextBox();
            txtBusqueda.Font = new Font("Arial", 16);
            txtBusqueda.Location = new Point(100, 38);
            txtBusqueda.Size = new Size(1000, 40);
            txtBusqueda.PlaceholderText = "Escanee código de barras o escriba nombre del producto...";
            txtBusqueda.KeyPress += TxtBusqueda_KeyPress;
            pnlBusqueda.Controls.Add(txtBusqueda);

            Button btnBuscar = new Button();
            btnBuscar.Text = "BUSCAR";
            btnBuscar.Font = new Font("Arial", 14, FontStyle.Bold);
            btnBuscar.BackColor = Color.FromArgb(0, 123, 255);
            btnBuscar.ForeColor = Color.White;
            btnBuscar.FlatStyle = FlatStyle.Flat;
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Location = new Point(1110, 30);
            btnBuscar.Size = new Size(250, 50);
            btnBuscar.Cursor = Cursors.Hand;
            btnBuscar.Click += BtnBuscar_Click;
            pnlBusqueda.Controls.Add(btnBuscar);

            this.Controls.Add(pnlBusqueda);
            posY += 120;

            // DataGridView
            dgvDetalles = new DataGridView();
            dgvDetalles.Location = new Point(margen, posY);
            dgvDetalles.Size = new Size(1400, 420);
            dgvDetalles.BackgroundColor = Color.White;
            dgvDetalles.BorderStyle = BorderStyle.FixedSingle;
            dgvDetalles.AllowUserToAddRows = false;
            dgvDetalles.AllowUserToDeleteRows = false;
            dgvDetalles.ReadOnly = true;
            dgvDetalles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDetalles.AutoGenerateColumns = false; // CRÍTICO
            dgvDetalles.RowHeadersVisible = false;
            dgvDetalles.ColumnHeadersHeight = 50;
            dgvDetalles.RowTemplate.Height = 50;
            dgvDetalles.Font = new Font("Arial", 11);
            dgvDetalles.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Bold);
            dgvDetalles.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ConfigurarDataGridView();
            this.Controls.Add(dgvDetalles);
            posY += 370;

            // Panel de totales
            Panel pnlTotales = new Panel();
            pnlTotales.BackColor = Color.White;
            pnlTotales.Location = new Point(margen, posY);
            pnlTotales.Size = new Size(1400, 160);
            pnlTotales.BorderStyle = BorderStyle.FixedSingle;

            // Efectivo
            Label lblEfectivo = new Label();
            lblEfectivo.Text = "Efectivo Recibido:";
            lblEfectivo.Font = new Font("Arial", 12);
            lblEfectivo.Location = new Point(30, 20);
            lblEfectivo.AutoSize = true;
            pnlTotales.Controls.Add(lblEfectivo);

            txtEfectivo = new TextBox();
            txtEfectivo.Font = new Font("Arial", 16, FontStyle.Bold);
            txtEfectivo.Location = new Point(30, 50);
            txtEfectivo.Size = new Size(180, 40);
            txtEfectivo.Text = "0.00";
            txtEfectivo.TextAlign = HorizontalAlignment.Center;
            txtEfectivo.TextChanged += TxtEfectivo_TextChanged;
            txtEfectivo.Click += TxtEfectivo_Click; // NUEVO
            txtEfectivo.Enter += TxtEfectivo_Enter; // NUEVO
            pnlTotales.Controls.Add(txtEfectivo);

            // Vuelto
            Label lblVuelto = new Label();
            lblVuelto.Text = "Vuelto:";
            lblVuelto.Font = new Font("Arial", 12);
            lblVuelto.Location = new Point(220, 20);
            lblVuelto.AutoSize = true;
            pnlTotales.Controls.Add(lblVuelto);

            lblVueltoValor = new Label();
            lblVueltoValor.Text = "S/ 0.00";
            lblVueltoValor.Font = new Font("Arial", 14, FontStyle.Bold);
            lblVueltoValor.ForeColor = Color.FromArgb(0, 123, 255);
            lblVueltoValor.Location = new Point(220, 50);
            lblVueltoValor.Size = new Size(150, 35);
            pnlTotales.Controls.Add(lblVueltoValor);

            // Subtotal
            Label lblSubtotal = new Label();
            lblSubtotal.Text = "Subtotal:";
            lblSubtotal.Font = new Font("Arial", 12);
            lblSubtotal.Location = new Point(800, 20);
            lblSubtotal.AutoSize = true;
            pnlTotales.Controls.Add(lblSubtotal);

            lblSubtotalValor = new Label();
            lblSubtotalValor.Text = "S/ 0.00";
            lblSubtotalValor.Font = new Font("Arial", 14);
            lblSubtotalValor.Location = new Point(920, 20);
            lblSubtotalValor.Size = new Size(200, 30);
            lblSubtotalValor.TextAlign = ContentAlignment.MiddleRight;
            pnlTotales.Controls.Add(lblSubtotalValor);

            // Total
            Label lblTotal = new Label();
            lblTotal.Text = "TOTAL:";
            lblTotal.Font = new Font("Arial", 16, FontStyle.Bold);
            lblTotal.Location = new Point(800, 55);
            lblTotal.AutoSize = true;
            pnlTotales.Controls.Add(lblTotal);

            lblTotalValor = new Label();
            lblTotalValor.Text = "S/ 0.00";
            lblTotalValor.Font = new Font("Arial", 16, FontStyle.Bold);
            lblTotalValor.ForeColor = Color.FromArgb(40, 167, 69);
            lblTotalValor.Location = new Point(920, 55);
            lblTotalValor.Size = new Size(200, 35);
            lblTotalValor.TextAlign = ContentAlignment.MiddleRight;
            pnlTotales.Controls.Add(lblTotalValor);

            // Botones
            btnCancelar = new Button();
            btnCancelar.Text = "✖ Cancelar";
            btnCancelar.Font = new Font("Arial", 12, FontStyle.Bold);
            btnCancelar.BackColor = Color.Gray;
            btnCancelar.ForeColor = Color.White;
            btnCancelar.FlatStyle = FlatStyle.Flat;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Location = new Point(30, 95);
            btnCancelar.Size = new Size(200, 50);
            btnCancelar.Cursor = Cursors.Hand;
            btnCancelar.Click += BtnCancelar_Click;
            pnlTotales.Controls.Add(btnCancelar);

            btnGuardar = new Button();
            btnGuardar.Text = "💰 COBRAR";
            btnGuardar.Font = new Font("Arial", 16, FontStyle.Bold);
            btnGuardar.BackColor = Color.FromArgb(0, 123, 255);
            btnGuardar.ForeColor = Color.White;
            btnGuardar.FlatStyle = FlatStyle.Flat;
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Location = new Point(900, 95);
            btnGuardar.Size = new Size(280, 50);
            btnGuardar.Cursor = Cursors.Hand;
            btnGuardar.Click += BtnGuardar_Click;
            pnlTotales.Controls.Add(btnGuardar);

            this.Controls.Add(pnlTotales);
        }

        private void ConfigurarDataGridView()
        {
            dgvDetalles.Columns.Clear();

            // #
            DataGridViewTextBoxColumn colNumero = new DataGridViewTextBoxColumn();
            colNumero.Name = "Numero";
            colNumero.HeaderText = "#";
            colNumero.Width = 50;
            colNumero.ReadOnly = true;
            colNumero.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDetalles.Columns.Add(colNumero);

            // Botón +
            DataGridViewButtonColumn btnMas = new DataGridViewButtonColumn();
            btnMas.Name = "Mas";
            btnMas.HeaderText = "";
            btnMas.Text = "+";
            btnMas.UseColumnTextForButtonValue = true;
            btnMas.Width = 40;
            btnMas.FlatStyle = FlatStyle.Flat;
            dgvDetalles.Columns.Add(btnMas);

            // Cantidad
            DataGridViewTextBoxColumn colCantidad = new DataGridViewTextBoxColumn();
            colCantidad.Name = "Cantidad";
            colCantidad.HeaderText = "CANT";
            colCantidad.Width = 70;
            colCantidad.ReadOnly = true;
            colCantidad.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colCantidad.DefaultCellStyle.Font = new Font("Arial", 12, FontStyle.Bold);
            colCantidad.DefaultCellStyle.BackColor = Color.FromArgb(230, 240, 255);
            dgvDetalles.Columns.Add(colCantidad);

            // Botón -
            DataGridViewButtonColumn btnMenos = new DataGridViewButtonColumn();
            btnMenos.Name = "Menos";
            btnMenos.HeaderText = "";
            btnMenos.Text = "-";
            btnMenos.UseColumnTextForButtonValue = true;
            btnMenos.Width = 40;
            btnMenos.FlatStyle = FlatStyle.Flat;
            dgvDetalles.Columns.Add(btnMenos);

            // Producto
            DataGridViewTextBoxColumn colProducto = new DataGridViewTextBoxColumn();
            colProducto.Name = "Producto";
            colProducto.HeaderText = "PRODUCTO";
            colProducto.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colProducto.ReadOnly = true;
            dgvDetalles.Columns.Add(colProducto);

            // Precio
            DataGridViewTextBoxColumn colPrecio = new DataGridViewTextBoxColumn();
            colPrecio.Name = "Precio";
            colPrecio.HeaderText = "PRECIO";
            colPrecio.Width = 100;
            colPrecio.ReadOnly = true;
            colPrecio.DefaultCellStyle.Format = "C2";
            colPrecio.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvDetalles.Columns.Add(colPrecio);

            // Total
            DataGridViewTextBoxColumn colTotal = new DataGridViewTextBoxColumn();
            colTotal.Name = "Total";
            colTotal.HeaderText = "TOTAL";
            colTotal.Width = 120;
            colTotal.ReadOnly = true;
            colTotal.DefaultCellStyle.Format = "C2";
            colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTotal.DefaultCellStyle.Font = new Font("Arial", 11, FontStyle.Bold);
            dgvDetalles.Columns.Add(colTotal);

            // Botón eliminar
            DataGridViewButtonColumn btnEliminar = new DataGridViewButtonColumn();
            btnEliminar.Name = "Eliminar";
            btnEliminar.HeaderText = "";
            btnEliminar.Text = "✖";
            btnEliminar.UseColumnTextForButtonValue = true;
            btnEliminar.Width = 50;
            btnEliminar.FlatStyle = FlatStyle.Flat;
            dgvDetalles.Columns.Add(btnEliminar);

            dgvDetalles.CellContentClick += DgvDetalles_CellContentClick;
            dgvDetalles.CellPainting += DgvDetalles_CellPainting;
        }

        // Eventos
        private void TxtBusqueda_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                BtnBuscar_Click(sender, e);
            }
        }

        private void TxtEfectivo_Click(object sender, EventArgs e)
        {
            txtEfectivo.SelectAll();
        }

        private void TxtEfectivo_Enter(object sender, EventArgs e)
        {
            txtEfectivo.SelectAll();
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            string busqueda = txtBusqueda.Text.Trim();
            if (string.IsNullOrEmpty(busqueda))
            {
                MessageBox.Show("Ingrese un código o nombre", "Validación",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Producto? producto = VentaService.BuscarProducto(busqueda);

            if (producto != null)
            {
                var existente = detallesVenta.FirstOrDefault(d => d.ProductoId == producto.Id);

                if (existente != null)
                {
                    existente.Cantidad++;
                    existente.CalcularSubtotal();
                }
                else
                {
                    detallesVenta.Add(new DetalleVenta(producto, 1));
                }

                ActualizarGrid();
                ActualizarTotales();
                txtBusqueda.Clear();
                txtBusqueda.Focus();
            }
            else
            {
                MessageBox.Show("Producto no encontrado", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvDetalles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var detalle = detallesVenta[e.RowIndex];
            string colName = dgvDetalles.Columns[e.ColumnIndex].Name;

            if (colName == "Mas")
            {
                detalle.Cantidad++;
                detalle.CalcularSubtotal();
                ActualizarGrid();
                ActualizarTotales();
            }
            else if (colName == "Menos")
            {
                if (detalle.Cantidad > 1)
                {
                    detalle.Cantidad--;
                    detalle.CalcularSubtotal();
                    ActualizarGrid();
                    ActualizarTotales();
                }
            }
            else if (colName == "Eliminar")
            {
                // Eliminar directo, sin confirmación
                detallesVenta.RemoveAt(e.RowIndex);
                ActualizarGrid();
                ActualizarTotales();
            }
        }

        private void DgvDetalles_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dgvDetalles.Columns[e.ColumnIndex].Name;

            if (colName == "Mas" || colName == "Menos")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 123, 255)), e.CellBounds);

                TextRenderer.DrawText(e.Graphics, colName == "Mas" ? "+" : "-",
                    new Font("Arial", 16, FontStyle.Bold),
                    e.CellBounds,
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
            else if (colName == "Eliminar")
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 53, 69)), e.CellBounds);

                TextRenderer.DrawText(e.Graphics, "✖",
                    new Font("Arial", 14, FontStyle.Bold),
                    e.CellBounds,
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
        }

        private void TxtEfectivo_TextChanged(object sender, EventArgs e)
        {
            CalcularVuelto();
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (detallesVenta.Count == 0)
            {
                MessageBox.Show("Agregue productos", "Validación",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtEfectivo.Text, out decimal efectivo) || efectivo <= 0)
            {
                MessageBox.Show("Ingrese efectivo válido", "Validación",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEfectivo.Focus();
                return;
            }

            decimal total = detallesVenta.Sum(d => d.Subtotal);

            if (efectivo < total)
            {
                MessageBox.Show($"Efectivo insuficiente\nFalta: S/ {(total - efectivo):F2}",
                              "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEfectivo.Focus();
                return;
            }

            decimal vuelto = efectivo - total;

            DialogResult confirm = MessageBox.Show(
                $"Total: S/ {total:F2}\nEfectivo: S/ {efectivo:F2}\nVuelto: S/ {vuelto:F2}\n\n¿Confirmar venta?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                int numeroTicket;
                bool exito = VentaService.RegistrarVenta(detallesVenta, efectivo, usuarioActual.Id, out numeroTicket);

                if (exito)
                {
                    // Vista previa del ticket
                    PrinterService.ImprimirDirecto(numeroTicket, detallesVenta, total, efectivo, vuelto, usuarioActual.Nombre);

                    MessageBox.Show($"✅ Venta registrada\nTicket #{numeroTicket:D6}",
                                  "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LimpiarFormulario();
                }
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (detallesVenta.Count > 0)
            {
                DialogResult result = MessageBox.Show("¿Cancelar venta actual?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                    LimpiarFormulario();
            }
        }

        private void ActualizarGrid()
        {
            dgvDetalles.Rows.Clear();

            for (int i = 0; i < detallesVenta.Count; i++)
            {
                var detalle = detallesVenta[i];
                dgvDetalles.Rows.Add(
                    i + 1,                      // #
                    "+",                        // Botón +
                    detalle.Cantidad,           // CANT
                    "-",                        // Botón -
                    detalle.NombreProducto,     // PRODUCTO
                    detalle.PrecioUnitario,     // PRECIO
                    detalle.Subtotal,           // TOTAL
                    "✖"                         // Botón Eliminar
                );
            }
        }

        private void ActualizarTotales()
        {
            decimal total = detallesVenta.Sum(d => d.Subtotal);
            lblSubtotalValor.Text = $"S/ {total:F2}";
            lblTotalValor.Text = $"S/ {total:F2}";
            CalcularVuelto();
        }

        private void CalcularVuelto()
        {
            decimal total = detallesVenta.Sum(d => d.Subtotal);
            if (decimal.TryParse(txtEfectivo.Text, out decimal efectivo))
            {
                decimal vuelto = efectivo - total;
                lblVueltoValor.Text = $"S/ {vuelto:F2}";
                lblVueltoValor.ForeColor = vuelto >= 0 ? Color.FromArgb(40, 167, 69) : Color.Red;
            }
            else
            {
                lblVueltoValor.Text = "S/ 0.00";
            }
        }

        private void LimpiarFormulario()
        {
            txtBusqueda.Clear();
            txtEfectivo.Text = "0.00";
            detallesVenta.Clear();
            dgvDetalles.Rows.Clear();
            lblSubtotalValor.Text = "S/ 0.00";
            lblTotalValor.Text = "S/ 0.00";
            lblVueltoValor.Text = "S/ 0.00";
            txtBusqueda.Focus();
        }
    }
}