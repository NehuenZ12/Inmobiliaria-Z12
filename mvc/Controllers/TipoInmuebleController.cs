using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Models;

namespace mvc.Controllers
{
    [Authorize]
    public class TipoInmuebleController : Controller
    {
        private readonly AppDbContext _context;

        public TipoInmuebleController(AppDbContext context)
        {
            _context = context;
        }

        // LISTAR
        public async Task<IActionResult> Index(
            string? buscar,
            int pagina = 1)
        {
            int registrosPorPagina = 5;

            var consulta = _context.TiposInmueble.AsQueryable();

            // Buscar por nombre
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                consulta = consulta.Where(t =>
                    t.Nombre.Contains(buscar));
            }

            // Cantidad total de registros
            int totalRegistros = await consulta.CountAsync();

            // Cantidad total de paginas
            int totalPaginas = (int)Math.Ceiling(
                totalRegistros / (double)registrosPorPagina
            );

            // Evitar paginas invalidas
            if (pagina < 1)
            {
                pagina = 1;
            }

            if (totalPaginas > 0 && pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }

            // Obtener solamente los registros de la pagina actual
            var tipos = await consulta
                .OrderBy(t => t.Nombre)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            // Datos para la vista
            ViewBag.Buscar = buscar;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(tipos);
        }

        // CREAR
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TipoInmueble tipoInmueble)
        {
            if (ModelState.IsValid)
            {
                _context.TiposInmueble.Add(tipoInmueble);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(tipoInmueble);
        }

        // EDITAR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipo = await _context.TiposInmueble.FindAsync(id);

            if (tipo == null)
            {
                return NotFound();
            }

            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TipoInmueble tipoInmueble)
        {
            if (id != tipoInmueble.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // traemos el original y copiamos solo los campos del form
                var actual = await _context.TiposInmueble.FindAsync(id);

                if (actual == null)
                {
                    return NotFound();
                }

                actual.Nombre = tipoInmueble.Nombre;
                actual.Descripcion = tipoInmueble.Descripcion;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(tipoInmueble);
        }

        // ELIMINAR
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tipo = await _context.TiposInmueble.FindAsync(id);

            if (tipo == null)
            {
                return NotFound();
            }

            try
            {
                _context.TiposInmueble.Remove(tipo);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Tipo de inmueble eliminado correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se puede eliminar el tipo porque tiene inmuebles asociados.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}