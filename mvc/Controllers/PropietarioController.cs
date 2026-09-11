using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Models;

namespace mvc.Controllers
{
    public class PropietarioController : Controller
    {
        private readonly AppDbContext _context;

        public PropietarioController(AppDbContext context)
        {
            _context = context;
        }

        // LISTAR PROPIETARIOS
        public async Task<IActionResult> Index()
        {
            var propietarios = await _context.Propietarios.ToListAsync();

            return View(propietarios);
        }

        // CREAR - muestra el formulario
        public IActionResult Create()
        {
            return View();
        }

        // CREAR - guarda el propietario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Propietario propietario)
        {
            // Comprobar si el email ya está registrado
            if (!string.IsNullOrWhiteSpace(propietario.Email))
            {
                bool emailExiste = await _context.Propietarios
                    .AnyAsync(p => p.Email == propietario.Email);

                if (emailExiste)
                {
                    ModelState.AddModelError(
                        "Email",
                        "Ya existe un propietario con ese email."
                    );
                }
            }

            if (ModelState.IsValid)
            {
                _context.Propietarios.Add(propietario);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(propietario);
        }

        // EDITAR - muestra el formulario
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var propietario = await _context.Propietarios.FindAsync(id);

            if (propietario == null)
            {
                return NotFound();
            }

            return View(propietario);
        }

        // EDITAR - guarda los cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Propietario propietario)
        {
            if (id != propietario.Id)
            {
                return NotFound();
            }

            // Comprobar si el email ya pertenece a otro propietario
            if (!string.IsNullOrWhiteSpace(propietario.Email))
            {
                bool emailExiste = await _context.Propietarios
                    .AnyAsync(p =>
                        p.Email == propietario.Email &&
                        p.Id != propietario.Id
                    );

                if (emailExiste)
                {
                    ModelState.AddModelError(
                        "Email",
                        "Ya existe otro propietario con ese email."
                    );
                }
            }

            if (ModelState.IsValid)
            {
                // traemos el original y copiamos solo los campos del form
                var actual = await _context.Propietarios.FindAsync(id);

                if (actual == null)
                {
                    return NotFound();
                }

                actual.Nombre = propietario.Nombre;
                actual.Apellido = propietario.Apellido;
                actual.Dni = propietario.Dni;
                actual.Telefono = propietario.Telefono;
                actual.Email = propietario.Email;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(propietario);
        }

        // ELIMINAR PROPIETARIO (solo Usuarios Administradores)
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var propietario = await _context.Propietarios.FindAsync(id);

            if (propietario == null)
            {
                return NotFound();
            }

            try
            {
                _context.Propietarios.Remove(propietario);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Propietario eliminado correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se puede eliminar el propietario porque tiene inmuebles asociados.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}