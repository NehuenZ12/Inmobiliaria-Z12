using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using mvc.Models;

namespace mvc.Controllers
{
  [Authorize]

  public class InquilinoController : Controller
  {
    private readonly AppDbContext _context;

    // Constructor: recibe la conexión a la base de datos
    public InquilinoController(AppDbContext context)
    {
      _context = context;
    }

    // LISTAR INQUILINOS

    private const int TamanioPagina = 3;

    public async Task<IActionResult> Index(int pagina = 1)
    {
      if (pagina < 1) pagina = 1;

      var query = _context.Inquilinos
          .OrderBy(i => i.Apellido)
          .ThenBy(i => i.Nombre);

      var totalInquilinos = await query.CountAsync();
      var totalPaginas = (int)Math.Ceiling(totalInquilinos / (double)TamanioPagina);

      var inquilinos = await query
          .Skip((pagina - 1) * TamanioPagina)
          .Take(TamanioPagina)
          .ToListAsync();

      ViewBag.PaginaActual = pagina;
      ViewBag.TotalPaginas = totalPaginas;

      return View(inquilinos);
    }

    // CREAR INQUILINO
    // Muestra el formulario
    public IActionResult Create()
    {
      return View();
    }

    // Recibe los datos del formulario y los guarda
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Inquilino inquilino)
    {
      if (ModelState.IsValid)
      {
        _context.Inquilinos.Add(inquilino);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
      }

      return View(inquilino);
    }

    // EDITAR INQUILINO

    // Muestra el formulario con los datos actuales
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var inquilino = await _context.Inquilinos.FindAsync(id);

      if (inquilino == null)
      {
        return NotFound();
      }


      return View(inquilino);
    }

    // Recibe los datos modificados y los guarda
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Inquilino inquilino)
    {
      if (id != inquilino.Id)
      {
        return NotFound();
      }

      if (ModelState.IsValid)
      {
        // traemos el original para no pisar fecha_alta, que la maneja la base
        var actual = await _context.Inquilinos.FindAsync(id);

        if (actual == null)
        {
          return NotFound();
        }

        actual.Nombre = inquilino.Nombre;
        actual.Apellido = inquilino.Apellido;
        actual.Dni = inquilino.Dni;
        actual.Telefono = inquilino.Telefono;
        actual.Email = inquilino.Email;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
      }

      return View(inquilino);
    }

    // ELIMINAR INQUILINO

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
      var inquilino = await _context.Inquilinos.FindAsync(id);

      if (inquilino == null)
      {
        return NotFound();
      }

      try
      {
        _context.Inquilinos.Remove(inquilino);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Inquilino eliminado correctamente.";
      }
      catch (DbUpdateException)
      {
        TempData["Error"] = "No se puede eliminar el inquilino porque tiene reservas asociadas.";
      }

      return RedirectToAction(nameof(Index));
    }
  }
}