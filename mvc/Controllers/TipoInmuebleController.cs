using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Models;

namespace mvc.Controllers
{
    public class TipoInmuebleController : Controller
    {
        private readonly AppDbContext _context;

        public TipoInmuebleController(AppDbContext context)
        {
            _context = context;
        }

        // LISTAR
        public async Task<IActionResult> Index()
        {
            var tipos = await _context.TiposInmueble.ToListAsync();

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
                _context.TiposInmueble.Update(tipoInmueble);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(tipoInmueble);
        }

        // ELIMINAR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tipo = await _context.TiposInmueble.FindAsync(id);

            if (tipo == null)
            {
                return NotFound();
            }

            _context.TiposInmueble.Remove(tipo);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}