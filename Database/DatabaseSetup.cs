using System.Data.SQLite;

namespace MinimarketPOS.Database
{
    public class DatabaseSetup
    {
        public static void CrearTablas()
        {
            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();

                // 1. TABLA usuarios
                string sqlUsuarios = @"
                    CREATE TABLE IF NOT EXISTS usuarios (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT UNIQUE NOT NULL,
                        password_hash TEXT NOT NULL,
                        nombre TEXT NOT NULL,
                        rol TEXT NOT NULL CHECK(rol IN ('admin', 'empleado')),
                        activo BOOLEAN DEFAULT 1,
                        fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_usuarios_username ON usuarios(username);
                    CREATE INDEX IF NOT EXISTS idx_usuarios_rol ON usuarios(rol);
                ";

                // 2. TABLA productos
                string sqlProductos = @"
                    CREATE TABLE IF NOT EXISTS productos (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        codigo_barras TEXT UNIQUE NOT NULL,
                        nombre TEXT NOT NULL,
                        precio DECIMAL(10,2) NOT NULL,
                        stock INTEGER DEFAULT 0,
                        stock_minimo INTEGER DEFAULT 10,
                        fecha_vencimiento DATE,
                        categoria TEXT,
                        activo BOOLEAN DEFAULT 1,
                        fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_productos_codigo ON productos(codigo_barras);
                    CREATE INDEX IF NOT EXISTS idx_productos_nombre ON productos(nombre);
                    CREATE INDEX IF NOT EXISTS idx_productos_categoria ON productos(categoria);
                    CREATE INDEX IF NOT EXISTS idx_productos_vencimiento ON productos(fecha_vencimiento);
                ";

                // 3. TABLA ventas
                string sqlVentas = @"
                    CREATE TABLE IF NOT EXISTS ventas (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        numero_ticket INTEGER UNIQUE NOT NULL,
                        fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP,
                        usuario_id INTEGER NOT NULL,
                        subtotal DECIMAL(10,2) NOT NULL,
                        total DECIMAL(10,2) NOT NULL,
                        efectivo_recibido DECIMAL(10,2),
                        vuelto DECIMAL(10,2),
                        estado TEXT DEFAULT 'completada' CHECK(estado IN ('completada', 'anulada')),
                        motivo_anulacion TEXT,
                        FOREIGN KEY(usuario_id) REFERENCES usuarios(id)
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_ventas_fecha ON ventas(fecha_hora);
                    CREATE INDEX IF NOT EXISTS idx_ventas_usuario ON ventas(usuario_id);
                    CREATE INDEX IF NOT EXISTS idx_ventas_numero ON ventas(numero_ticket);
                    CREATE INDEX IF NOT EXISTS idx_ventas_estado ON ventas(estado);
                ";

                // 4. TABLA detalle_ventas
                string sqlDetalleVentas = @"
                    CREATE TABLE IF NOT EXISTS detalle_ventas (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        venta_id INTEGER NOT NULL,
                        producto_id INTEGER NOT NULL,
                        cantidad INTEGER NOT NULL,
                        precio_unitario DECIMAL(10,2) NOT NULL,
                        subtotal DECIMAL(10,2) NOT NULL,
                        FOREIGN KEY(venta_id) REFERENCES ventas(id),
                        FOREIGN KEY(producto_id) REFERENCES productos(id)
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_detalle_venta ON detalle_ventas(venta_id);
                    CREATE INDEX IF NOT EXISTS idx_detalle_producto ON detalle_ventas(producto_id);
                ";

                // 5. TABLA auditoria
                string sqlAuditoria = @"
                    CREATE TABLE IF NOT EXISTS auditoria (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP,
                        usuario_id INTEGER NOT NULL,
                        accion TEXT NOT NULL,
                        tabla TEXT,
                        registro_id INTEGER,
                        detalle TEXT,
                        FOREIGN KEY(usuario_id) REFERENCES usuarios(id)
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_auditoria_fecha ON auditoria(fecha_hora);
                    CREATE INDEX IF NOT EXISTS idx_auditoria_usuario ON auditoria(usuario_id);
                    CREATE INDEX IF NOT EXISTS idx_auditoria_accion ON auditoria(accion);
                ";

                // 6. TABLA configuracion
                string sqlConfiguracion = @"
                    CREATE TABLE IF NOT EXISTS configuracion (
                        clave TEXT PRIMARY KEY,
                        valor TEXT NOT NULL
                    );
                ";

                // Ejecutar todas las sentencias
                ExecuteNonQuery(connection, sqlUsuarios);
                ExecuteNonQuery(connection, sqlProductos);
                ExecuteNonQuery(connection, sqlVentas);
                ExecuteNonQuery(connection, sqlDetalleVentas);
                ExecuteNonQuery(connection, sqlAuditoria);
                ExecuteNonQuery(connection, sqlConfiguracion);

                Console.WriteLine("✅ Tablas creadas correctamente");
            }
        }

        public static void InsertarDatosIniciales()
        {
            using (var connection = DatabaseManager.GetConnection())
            {
                connection.Open();

                // Verificar si ya hay datos
                string checkQuery = "SELECT COUNT(*) FROM configuracion";
                using (var cmd = new SQLiteCommand(checkQuery, connection))
                {
                    long count = (long)cmd.ExecuteScalar();
                    if (count > 0) return; // Ya hay datos, no insertar
                }

                // Insertar configuración inicial
                string sqlConfig = @"
                    INSERT INTO configuracion (clave, valor) VALUES
                    ('nombre_negocio', 'MINIMARKET ROSITA'),
                    ('direccion', 'Jr. Los Olivos 123'),
                    ('ciudad', 'Ayacucho - Perú'),
                    ('ruc', '10XXXXXXXX'),
                    ('ultimo_ticket', '0'),
                    ('mensaje_ticket', '¡GRACIAS POR SU COMPRA!');
                ";

                // Insertar usuario admin inicial
                // Contraseña: admin123
                string passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                string sqlAdmin = $@"
                    INSERT INTO usuarios (username, password_hash, nombre, rol) 
                    VALUES ('admin', '{passwordHash}', 'Administrador', 'admin');
                ";

                ExecuteNonQuery(connection, sqlConfig);
                ExecuteNonQuery(connection, sqlAdmin);

                Console.WriteLine("✅ Datos iniciales insertados");
                Console.WriteLine("👤 Usuario: admin | Contraseña: admin123");
            }
        }

        private static void ExecuteNonQuery(SQLiteConnection connection, string sql)
        {
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}