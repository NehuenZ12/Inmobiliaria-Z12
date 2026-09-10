using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using mvc.Models;

namespace mvc.Controllers
{
    public class InmuebleController : Controller
    {
        private readonly AppDbContext _context;

        public InmuebleController(AppDbContext context)
        {
            _context = context;
        }

        // LISTAR INMUEBLES

        public async Task<IActionResult> Index()
        {
            var inmuebles = await _context.Inmuebles
                .Include(i => i.Propietario)
                .Include(i => i.TipoInmueble)
                .ToListAsync();

            return View(inmuebles);
        }

        // CREAR INMUEBLE

        public async Task<IActionResult> Create()
        {
            await CargarListas();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                _context.Inmuebles.Add(inmueble);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await CargarListas();

            return View(inmueble);
        }


        // EDITAR INMUEBLE

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inmueble = await _context.Inmuebles.FindAsync(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            await CargarListas();

            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Inmuebles.Update(inmueble);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            await CargarListas();

            return View(inmueble);
        }


        // ELIMINAR INMUEBLE (solo Usuarios Administradores)
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            _context.Inmuebles.Remove(inmueble);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private async Task CargarListas()
        {
            var propietarios = await _context.Propietarios
                .OrderBy(p => p.Apellido)
                .ThenBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Propietarios = new SelectList(
                propietarios,
                "Id",
                "Apellido"
            );

            var tipos = await _context.TiposInmueble
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            ViewBag.Tipos = new SelectList(
                tipos,
                "Id",
                "Nombre"
            );
        }
    }
}
