using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data
{
    public class UsuarioRepository
    {
        private readonly IConfiguration _config;

        public UsuarioRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=users.db");

        // GET BY ID
        public async Task<Usuario?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Usuario>("""
            SELECT id, nombre, apellido, email,
                   password_hash AS PasswordHash,
                   fecha_registro AS FechaRegistro,
                   activo,
                   intentos_fallidos AS IntentosFallidos
            FROM users
            WHERE id = @id
        """, new { id });
        }

        // GET BY EMAIL
        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Usuario>("""
            SELECT id, nombre, apellido, email,
                   password_hash AS PasswordHash,
                   fecha_registro AS FechaRegistro,
                   activo,
                   intentos_fallidos AS IntentosFallidos
            FROM users
            WHERE email = @email
        """, new { email });
        }

        // CREATE
        public async Task CreateAsync(Usuario usuario)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
            INSERT INTO users (id, nombre, apellido, email, password_hash, fecha_registro, activo, intentos_fallidos)
            VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, @FechaRegistro, @Activo, @IntentosFallidos)
        """, usuario);
        }

        // UPDATE
        public async Task UpdateAsync(Usuario usuario)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("""
            UPDATE users
            SET activo            = @Activo,
                intentos_fallidos = @IntentosFallidos
            WHERE id = @Id
        """, usuario);
        }
    }
}
