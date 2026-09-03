using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Models;

namespace mvc.Controllers
{
  public class InquilinoController : Controller
  {
    private readonly AppDbContext _context;

    // Constructor: recibe la conexión a la base de datos
    public InquilinoController(AppDbContext context)
    {
      _context = context;
    }

    // LISTAR INQUILINOS

    public async Task<IActionResult> Index()
    {
      var inquilinos = await _context.Inquilinos.ToListAsync();

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
        _context.Update(inquilino);

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

      _context.Inquilinos.Remove(inquilino);

      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }
  }
}