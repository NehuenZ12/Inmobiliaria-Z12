using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using mvc.Models;

namespace mvc.Controllers
{
  public class ReservaController : Controller
  {
    private readonly AppDbContext _context;

    public ReservaController(AppDbContext context)
    {
      _context = context;
    }

    // LISTAR RESERVAS

    public async Task<IActionResult> Index()
    {
      var reservas = await _context.Reservas.ToListAsync();

      // Completamos los campos auxiliares para mostrar en la tabla
      foreach (var reserva in reservas)
      {
        var inquilino = await _context.Inquilinos.FindAsync(reserva.InquilinoId);
        var inmueble = await _context.Inmuebles.FindAsync(reserva.InmuebleId);

        reserva.NombreInquilino = inquilino != null ? $"{inquilino.Nombre} {inquilino.Apellido}" : "-";
        reserva.DireccionInmueble = inmueble != null ? inmueble.Direccion : "-";
      }

      return View(reservas);
    }

    // CREAR RESERVA

    public async Task<IActionResult> Create()
    {
      await CargarListas();

      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reserva reserva)
    {
      // Validacion de fechas: la fecha hasta tiene que ser posterior a la fecha desde
      if (reserva.FechaHasta <= reserva.FechaDesde)
      {
        ModelState.AddModelError("FechaHasta", "La fecha hasta debe ser posterior a la fecha desde");
      }

      // Verificar que el inmueble no este ocupado en esas fechas
      if (ModelState.IsValid && await InmuebleOcupado(reserva.InmuebleId, reserva.FechaDesde, reserva.FechaHasta, null))
      {
        ModelState.AddModelError("InmuebleId", "El inmueble ya esta reservado en esas fechas");
      }

      if (ModelState.IsValid)
      {
        // TODO: reemplazar por el usuario real cuando el equipo defina Usuario
        reserva.UsuarioCreadorId = 1;

        _context.Reservas.Add(reserva);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
      }

      await CargarListas();

      return View(reserva);
    }


    // VERIFICAR SI UN INMUEBLE ESTA OCUPADO EN UN RANGO DE FECHAS
    // reservaIdAExcluir se usa al editar, para no comparar la reserva contra si misma
    private async Task<bool> InmuebleOcupado(int inmuebleId, DateTime fechaDesde, DateTime fechaHasta, int? reservaIdAExcluir)
    {
      var query = _context.Reservas.Where(r =>
          r.InmuebleId == inmuebleId &&
          r.FechaDesde < fechaHasta &&
          r.FechaHasta > fechaDesde);

      if (reservaIdAExcluir.HasValue)
      {
        query = query.Where(r => r.Id != reservaIdAExcluir.Value);
      }

      return await query.AnyAsync();
    }


    // CARGAR INQUILINOS E INMUEBLES PARA LOS DESPLEGABLES

    private async Task CargarListas()
    {
      var inquilinos = await _context.Inquilinos
          .OrderBy(i => i.Apellido)
          .ThenBy(i => i.Nombre)
          .ToListAsync();

      ViewBag.Inquilinos = new SelectList(inquilinos, "Id", "Apellido");

      var inmuebles = await _context.Inmuebles
          .OrderBy(i => i.Direccion)
          .ToListAsync();

      ViewBag.Inmuebles = new SelectList(inmuebles, "Id", "Direccion");

      ViewBag.Usuarios = new List<Usuario>
      {
        new Usuario { Id = 1, Nombre = "Provisorio", Apellido = "Provisorio" }
      };
    }
  }
}