using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using mvc.Models;

namespace mvc.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;
        private readonly IWebHostEnvironment _env;

        public UsuariosController(
            AppDbContext context,
            IPasswordHasher<Usuario> hasher,
            IWebHostEnvironment env)
        {
            _context = context;
            _hasher = hasher;
            _env = env;
        }

        // =====================================================
        // LISTADO (solo administradores)
        // =====================================================
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();

            return View(usuarios);
        }

        // =====================================================
        // LOGIN
        // =====================================================
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel login, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(login);
            }

            // Buscamos el usuario por email y validamos la clave
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            var esValido = usuario != null &&
                _hasher.VerifyHashedPassword(usuario, usuario.Clave, login.Clave) != PasswordVerificationResult.Failed;

            if (!esValido)
            {
                ModelState.AddModelError("", "Email o clave incorrectos");
                return View(login);
            }

            // Claims de identidad
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario!.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Creamos la cookie de sesión
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Propietario");
        }

        // =====================================================
        // LOGOUT
        // =====================================================
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // =====================================================
        // PERFIL (edición propia o por admin)
        // =====================================================
        public async Task<IActionResult> Perfil(int? id)
        {
            int idSolicitado = id ?? ObtenerIdUsuarioActual();

            // Solo el propio perfil o un administrador pueden acceder
            if (!PuedeAccederAPerfil(idSolicitado))
            {
                return RedirectToAction("Restringido", "Home");
            }

            var usuario = await _context.Usuarios.FindAsync(idSolicitado);

            if (usuario == null)
            {
                return NotFound();
            }

            // No mandamos la clave a la vista
            usuario.Clave = "";

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(int id, Usuario usuario, IFormFile? avatar)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            // Solo el propio perfil o un administrador pueden editar
            if (!PuedeAccederAPerfil(id))
            {
                return RedirectToAction("Restringido", "Home");
            }

            // Si la clave quedó vacía, no la validamos ni la cambiamos
            if (string.IsNullOrWhiteSpace(usuario.Clave))
            {
                ModelState.Remove(nameof(usuario.Clave));
            }
            else if (usuario.Clave.Length < 6)
            {
                ModelState.AddModelError("Clave", "La clave debe tener al menos 6 caracteres");
            }

            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            // Traemos el usuario original para actualizar solo los campos permitidos
            var actual = await _context.Usuarios.FindAsync(id);
            if (actual == null)
            {
                return NotFound();
            }

            actual.Nombre = usuario.Nombre;
            actual.Apellido = usuario.Apellido;
            actual.Email = usuario.Email;
            actual.Avatar = await GuardarAvatarAsync(avatar, actual.Avatar);

            // Hasheamos la clave si el usuario la cambió
            if (!string.IsNullOrWhiteSpace(usuario.Clave))
            {
                actual.Clave = _hasher.HashPassword(usuario, usuario.Clave);
            }

            // Solo un admin puede cambiar el rol desde acá
            if (EsAdmin())
            {
                actual.Rol = usuario.Rol;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Perfil", new { id = actual.Id });
        }

        // =====================================================
        // CREAR USUARIO
        // Acceso anónimo solo si no hay usuarios (primer administrador)
        // =====================================================
        [AllowAnonymous]
        public async Task<IActionResult> Crear()
        {
            if (!await PuedeAccederACrear())
            {
                return RedirectToAction("Restringido", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario usuario, IFormFile? avatar)
        {
            if (!await PuedeAccederACrear())
            {
                return RedirectToAction("Restringido", "Home");
            }

            var esPrimerUsuario = !await _context.Usuarios.AnyAsync();

            // Si es el primer usuario, forzamos el rol Administrador
            if (esPrimerUsuario)
            {
                usuario.Rol = "Administrador";
            }

            // Si no es admin, el rol por defecto es Empleado
            if (!EsAdmin() && usuario.Rol != "Administrador" && usuario.Rol != "Empleado")
            {
                usuario.Rol = "Empleado";
            }

            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            // Hasheamos la clave antes de guardarla
            usuario.Clave = _hasher.HashPassword(usuario, usuario.Clave);

            // Guardamos el avatar si subieron uno
            usuario.Avatar = await GuardarAvatarAsync(avatar, null);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // =====================================================
        // ELIMINAR USUARIO (solo administradores)
        // =====================================================
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            // No permitimos que un admin se borre a sí mismo
            if (id == ObtenerIdUsuarioActual())
            {
                return RedirectToAction("Index");
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // AUXILIARES
        // =====================================================

        // Devuelve el ID del usuario logueado
        private int ObtenerIdUsuarioActual()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out int id) ? id : 0;
        }

        // Indica si el usuario actual es Administrador
        private bool EsAdmin()
        {
            return User.IsInRole("Administrador");
        }

        // Indica si el usuario actual puede acceder a un perfil determinado
        private bool PuedeAccederAPerfil(int id)
        {
            return EsAdmin() || ObtenerIdUsuarioActual() == id;
        }

        // Verifica si se permite acceder a Crear (primer usuario o admin logueado)
        private async Task<bool> PuedeAccederACrear()
        {
            var hayUsuarios = await _context.Usuarios.AnyAsync();
            return !hayUsuarios || EsAdmin();
        }

        // Guarda el archivo de avatar en wwwroot/avatars
        private async Task<string?> GuardarAvatarAsync(IFormFile? avatar, string? avatarActual)
        {
            if (avatar == null || avatar.Length == 0)
            {
                return avatarActual;
            }

            // Validamos tipo de archivo (solo imágenes)
            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (!permitidas.Contains(extension))
            {
                return avatarActual;
            }

            // Límite de 2 MB
            if (avatar.Length > 2 * 1024 * 1024)
            {
                return avatarActual;
            }

            // Creamos la carpeta si no existe
            var carpeta = Path.Combine(_env.WebRootPath, "avatars");
            Directory.CreateDirectory(carpeta);

            // Nombre único para evitar colisiones
            var nombre = $"{Guid.NewGuid()}{extension}";
            var ruta = Path.Combine(carpeta, nombre);

            using var stream = new FileStream(ruta, FileMode.Create);
            await avatar.CopyToAsync(stream);

            return $"/avatars/{nombre}";
        }
    }
}
