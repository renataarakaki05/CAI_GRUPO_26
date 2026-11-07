using System.Security.Cryptography;
using System.Text;
using Users.API.Data;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services
{
    // Lógica de negocio de usuarios
    // Si algo sale mal tira una excepción y los handlers de ExceptionHandlers arman la respuesta
    public class UsuarioService
    {
        private readonly UsuarioRepository _repo;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(UsuarioRepository repo, ILogger<UsuarioService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // REGISTRAR
        public async Task<UsuarioDto> RegistrarAsync(RegistrarUsuarioDto request)
        {
            Validar(request);

            // El email se guarda en minúscula 
            string email = request.Email.Trim().ToLower();

            // Si ya hay un usuario con ese email tira error USR-001
            var existente = await _repo.GetByEmailAsync(email);
            if (existente != null)
            {
                throw new BusinessRuleException(ErrorCodes.USR_001,
                    $"El email '{email}' ya está registrado.");
            }

            var usuario = new Usuario();
            usuario.Id = Guid.NewGuid();
            usuario.Nombre = request.Nombre.Trim();
            usuario.Apellido = request.Apellido.Trim();
            usuario.Email = email;
            usuario.PasswordHash = HashPassword(request.Password);
            usuario.FechaRegistro = DateTime.UtcNow;
            usuario.Activo = true;
            usuario.IntentosFallidos = 0;

            await _repo.CreateAsync(usuario);
            _logger.LogInformation("Usuario {UsuarioId} registrado", usuario.Id);

            // Devolvemos los datos sin la contraseña
            return new UsuarioDto(usuario.Id, usuario.Nombre, usuario.Apellido,
                usuario.Email, usuario.FechaRegistro, usuario.Activo);
        }

        // Nunca guardamos la contraseña tal cual y sino su hash SHA256
        private static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        // Revisa los datos del registro. Si hay errores tira USR-002 con todos juntos
        private static void Validar(RegistrarUsuarioDto request)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Nombre))
                errores.Add("El nombre es requerido");

            if (string.IsNullOrWhiteSpace(request.Apellido))
                errores.Add("El apellido es requerido");

            if (string.IsNullOrWhiteSpace(request.Email))
                errores.Add("El email es requerido");
            else if (!request.Email.Contains('@') || !request.Email.Contains('.'))
                errores.Add("El email no tiene un formato válido");

            if (string.IsNullOrWhiteSpace(request.Password))
                errores.Add("La contraseña es requerida");
            else if (request.Password.Length < 6)
                errores.Add("La contraseña debe tener al menos 6 caracteres");

            if (errores.Count > 0)
            {
                throw new ValidationException(ErrorCodes.USR_002,
                    $"Los datos del usuario son inválidos: {string.Join("; ", errores)}.");
            }
        }
    }
}
