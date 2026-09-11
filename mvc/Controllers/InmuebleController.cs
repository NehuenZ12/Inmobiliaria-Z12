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
      await CargarPropietarios();
      await CargarTipos();

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

      await CargarPropietarios();
      await CargarTipos();

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

      await CargarPropietarios();
      await CargarTipos();

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
        // traemos el original y copiamos solo los campos del form
        var actual = await _context.Inmuebles.FindAsync(id);

        if (actual == null)
        {
          return NotFound();
        }

        actual.Direccion = inmueble.Direccion;
        actual.Cupo = inmueble.Cupo;
        actual.Latitud = inmueble.Latitud;
        actual.Longitud = inmueble.Longitud;
        actual.PrecioPorDia = inmueble.PrecioPorDia;
        actual.PorcentajeReserva = inmueble.PorcentajeReserva;
        actual.Disponible = inmueble.Disponible;
        actual.PropietarioId = inmueble.PropietarioId;
        actual.TipoId = inmueble.TipoId;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
      }

      await CargarPropietarios();
      await CargarTipos();

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

      try
      {
        _context.Inmuebles.Remove(inmueble);
        await _context.SaveChangesAsync();
        TempData["Ok"] = "Inmueble eliminado correctamente.";
      }
      catch (DbUpdateException)
      {
        TempData["Error"] = "No se puede eliminar el inmueble porque tiene reservas o imagenes asociadas.";
      }

      return RedirectToAction(nameof(Index));
    }


    // CARGAR PROPIETARIOS

    private async Task CargarPropietarios()
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
    }

    // CARGAR TIPOS

    private async Task CargarTipos()
    {
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