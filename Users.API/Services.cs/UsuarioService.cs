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
        private const int MaxIntentosFallidos = 3; // A los 3 intentos fallidos seguidos se bloquea la cuenta
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


        // LOGIN
        public async Task<LoginResponseDto> LoginAsync(LoginDto request)
        {
            // Si falta el email o la contraseña tira error USR-002
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ValidationException(ErrorCodes.USR_002,
                    "Los datos del usuario son inválidos: el email y la contraseña son requeridos.");
            }

            string email = request.Email.Trim().ToLower();
            var usuario = await _repo.GetByEmailAsync(email);

            if (usuario == null) // Si el email no existe va lo mismo que con contraseña incorrecta
            {
                throw new BusinessRuleException(ErrorCodes.USR_003, "Credenciales incorrectas.");
            }

            if (!usuario.Activo)
            {
                LanzarErrorDeBloqueo(usuario); // Si ya está bloqueado no lo dejo entrar, aunque la contraseña sea correcta
            }

            if (usuario.PasswordHash != HashPassword(request.Password))  // Si la contraseña es incorrecta, sumamos un intento fallido
            {
                usuario.IntentosFallidos = usuario.IntentosFallidos + 1;

                if (usuario.IntentosFallidos >= MaxIntentosFallidos) // Al tercer intento se bloquea la cuenta
                {
                    usuario.Activo = false;
                    await _repo.UpdateAsync(usuario);
                    _logger.LogWarning("Usuario {UsuarioId} bloqueado por {Intentos} intentos fallidos",
                        usuario.Id, usuario.IntentosFallidos);
                    LanzarErrorDeBloqueo(usuario);
                }

                await _repo.UpdateAsync(usuario);
                throw new BusinessRuleException(ErrorCodes.USR_003, "Credenciales incorrectas.");
            }

            usuario.IntentosFallidos = 0; // Cuando el loggin es correcto los intentos fallidos vuelven a 0
            await _repo.UpdateAsync(usuario);
            _logger.LogInformation("Usuario {UsuarioId} inició sesión", usuario.Id);

            return new LoginResponseDto(usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email);
        }

        // Elige qué error de bloqueo mostrar:
        // con 3 o más intentos fallidos se bloqueó solo: USR-004
        // con menos, lo bloquearon a mano en la base por fraude: USR-005
        private static void LanzarErrorDeBloqueo(Usuario usuario)
        {
            if (usuario.IntentosFallidos >= MaxIntentosFallidos)
            {
                throw new BusinessRuleException(ErrorCodes.USR_004,
                    "Usuario bloqueado por demasiados intentos fallidos.");
            }

            throw new BusinessRuleException(ErrorCodes.USR_005,
                "Usuario bloqueado por detección de fraude.");
        }

        private static string HashPassword(string password) // Nunca guardamos la contraseña tal cual y sino su hash SHA256
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        private static void Validar(RegistrarUsuarioDto request)  // Revisa los datos del registro. Si hay errores tira USR-002 con todos juntos
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
